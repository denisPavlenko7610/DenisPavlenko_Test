using System;
using System.Collections.Generic;

namespace DenisPavlenko.Game.Core
{
	public enum GamePhase
	{
		Idle,
		Charging,
		ShotInFlight,
		Advancing,
		Won,
		Lost
	}

	public sealed class GameSession
	{
		private readonly GameConfig _config;
		private readonly PlayerBall _ball;
		private readonly LevelLayout _layout;

		private float _playerZ;
		private bool _shotActive;
		private float _shotRadius;
		private float _shotZ;
		private float _stopZ;

		public GameSession(GameConfig config, LevelLayout layout, float initialBallRadius)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_layout = layout ?? throw new ArgumentNullException(nameof(layout));
			_ball = new PlayerBall(config, initialBallRadius);
		}

		public GamePhase Phase { get; private set; } = GamePhase.Idle;

		public PlayerBall Ball => _ball;

		public float PlayerZ => _playerZ;

		public bool ShotActive => _shotActive;

		public float ShotRadius => _shotRadius;

		public float ShotZ => _shotZ;

		public float PlayerVolumeFraction => _ball.VolumeFraction;

		public float ShotVolumeFraction => Phase == GamePhase.Charging
			? _ball.ChargedShotVolumeFraction
			: _shotActive
				? VolumeMath.Cube(_shotRadius / _ball.InitialRadius)
				: 0f;

		public event Action ShotFired;
		public event Action<float, float, IReadOnlyList<int>> ObstaclesDestroyed;
		public event Action Won;
		public event Action Lost;

		public bool BeginCharge()
		{
			if (Phase != GamePhase.Idle)
			{
				return false;
			}

			_ball.BeginCharge();
			Phase = GamePhase.Charging;
			return true;
		}

		public bool TickCharge(float deltaTime)
		{
			if (Phase != GamePhase.Charging)
			{
				return false;
			}

			_ball.Charge(deltaTime, out bool overcharged);
			if (overcharged)
			{
				Fail();
				return false;
			}

			return true;
		}

		public bool ReleaseShot()
		{
			if (Phase != GamePhase.Charging)
			{
				return false;
			}

			if (_ball.ChargedShotRadius < _config.MinShotRadius)
			{
				_ball.CancelCharge();
				Phase = GamePhase.Idle;
				return false;
			}

			_shotRadius = _ball.ConsumeShot();
			_shotActive = true;
			_shotZ = _playerZ;
			Phase = GamePhase.ShotInFlight;
			ShotFired?.Invoke();
			return true;
		}

		public void Tick(float deltaTime)
		{
			switch (Phase)
			{
				case GamePhase.ShotInFlight:
					TickShot(deltaTime);
					break;

				case GamePhase.Advancing:
					TickAdvance(deltaTime);
					break;

				case GamePhase.Idle:
					CheckStuck();
					break;
			}
		}

		private void TickShot(float deltaTime)
		{
			Obstacle nextObstacle = _layout.FindNextBlockingObstacle(_shotZ, _shotRadius);
			float nextShotZ = _shotZ + _config.ShotSpeed * deltaTime;

			if (nextObstacle != null &&
				nextObstacle.TryGetContactZ(_shotRadius, out float contactZ) &&
				nextShotZ >= contactZ)
			{
				_shotZ = contactZ;
				ResolveBlast(nextObstacle.PositionZ);
				return;
			}

			_shotZ = nextShotZ;
			if (_shotZ >= _layout.TargetZ)
			{
				_shotZ = _layout.TargetZ;
				ResolveBlast(_layout.TargetZ);
			}
		}

		private void ResolveBlast(float blastZ)
		{
			float blastRadius = _shotRadius * _config.BlastRadiusMultiplier;
			List<int> destroyed = DestroyObstaclesInRadius(blastZ, blastRadius);

			_shotActive = false;
			Phase = GamePhase.Idle;
			ObstaclesDestroyed?.Invoke(blastZ, blastRadius, destroyed);

			if (_ball.IsCriticallySmall && destroyed.Count == 0)
			{
				Fail();
				return;
			}

			StartAdvancing();
		}

		private List<int> DestroyObstaclesInRadius(float blastZ, float blastRadius)
		{
			List<int> destroyed = new();
			foreach (Obstacle obstacle in _layout.ObstaclesInRadius(blastZ, blastRadius))
			{
				if (_layout.RemoveObstacle(obstacle.Id))
				{
					destroyed.Add(obstacle.Id);
				}
			}

			return destroyed;
		}

		private void StartAdvancing()
		{
			Phase = GamePhase.Advancing;
			Obstacle nextBlocking = _layout.FindNextBlockingObstacle(
				_playerZ,
				_ball.Radius + _config.ObstacleClearance
			);

			if (nextBlocking == null)
			{
				_stopZ = _layout.TargetZ;
			}
			else
			{
				_stopZ = Math.Max(
					_playerZ,
					GetContactZ(nextBlocking, _ball.Radius + _config.ObstacleClearance) -
					_config.ObstacleApproachDistance
				);
			}
		}

		private void TickAdvance(float deltaTime)
		{
			_playerZ = Math.Min(_playerZ + _config.PlayerAdvanceSpeed * deltaTime, _stopZ);

			if (_playerZ >= _layout.TargetZ)
			{
				Win();
			}
			else if (_playerZ >= _stopZ)
			{
				Phase = GamePhase.Idle;
				CheckStuck();
			}
		}

		private void CheckStuck()
		{
			if (_layout.FindNextBlockingObstacle(
				_playerZ,
				_ball.Radius + _config.ObstacleClearance
			) == null)
			{
				return;
			}

			if (_ball.IsCriticallySmall || _ball.MaxShotRadius < _config.MinShotRadius)
			{
				Fail();
			}
		}

		private static float GetContactZ(Obstacle obstacle, float movingSphereRadius)
		{
			return obstacle.TryGetContactZ(movingSphereRadius, out float contactZ)
				? contactZ
				: obstacle.MinZ;
		}

		private void Win()
		{
			Phase = GamePhase.Won;
			Won?.Invoke();
		}

		private void Fail()
		{
			if (Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			_shotActive = false;
			Phase = GamePhase.Lost;
			Lost?.Invoke();
		}
	}
}
