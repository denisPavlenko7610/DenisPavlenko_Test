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

		public GameSession(GameConfig config, LevelLayout layout)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_layout = layout ?? throw new ArgumentNullException(nameof(layout));
			_ball = new PlayerBall(config);
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
				? VolumeMath.Cube(_shotRadius / _config.InitialBallRadius)
				: 0f;

		public event Action<float> ShotFired;
		public event Action<IReadOnlyList<int>> ObstaclesDestroyed;
		public event Action Won;
		public event Action Lost;

		public bool BeginCharge()
		{
			if (Phase != GamePhase.Idle)
			{
				return false;
			}

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
			ShotFired?.Invoke(_shotRadius);
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
			_shotZ += _config.ShotSpeed * deltaTime;

			float? nextBlockingZ = _layout.FindNextBlockingZ(_playerZ, _ball.Radius);

			if (nextBlockingZ == null || _shotZ >= nextBlockingZ.Value)
			{
				ResolveBlast(nextBlockingZ ?? _layout.TargetZ);
			}
		}

		private void ResolveBlast(float blastZ)
		{
			float blastRadius = _shotRadius * _config.BlastRadiusMultiplier;
			List<int> destroyed = DestroyObstaclesInRadius(blastZ, blastRadius);

			_shotActive = false;
			Phase = GamePhase.Idle;
			ObstaclesDestroyed?.Invoke(destroyed);

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
			foreach (Obstacle obstacle in _layout.ObstaclesInRadius(blastZ, 0f, blastRadius))
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
			float? nextBlocking = _layout.FindNextBlockingZ(_playerZ, _ball.Radius);
			_stopZ = nextBlocking == null
				? _layout.TargetZ
				: nextBlocking.Value - _config.ObstacleApproachDistance;
		}

		private void TickAdvance(float deltaTime)
		{
			_playerZ = Math.Min(_playerZ + _config.PlayerAdvanceSpeed * deltaTime, _stopZ);

			if (_playerZ >= _layout.TargetZ)
			{
				Win();
			}
			else if (Math.Abs(_playerZ - _stopZ) < float.Epsilon)
			{
				Phase = GamePhase.Idle;
				CheckStuck();
			}
		}

		private void CheckStuck()
		{
			if (_layout.FindNextBlockingZ(_playerZ, _ball.Radius) == null)
			{
				return;
			}

			if (_ball.IsCriticallySmall || _ball.MaxShotRadius < _config.MinShotRadius)
			{
				Fail();
			}
		}

		private void Win()
		{
			Phase = GamePhase.Won;
			Won?.Invoke();
		}

		private void Fail()
		{
			Phase = GamePhase.Lost;
			Lost?.Invoke();
		}
	}
}
