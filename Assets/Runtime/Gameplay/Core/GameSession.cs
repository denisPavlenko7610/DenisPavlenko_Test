using System;
using System.Collections.Generic;

namespace DenisPavlenko.Game.Core
{
	public enum GamePhase
	{
		Idle,
		Charging,
		Advancing,
		Won,
		Lost
	}

	public readonly struct ActiveShot
	{

		public float Radius { get; }
		public float Z { get; }

		public ActiveShot(float radius, float z)
		{
			Radius = radius;
			Z = z;
		}
	}

	public sealed class GameSession
	{
		private readonly GameConfig _config;
		private readonly LevelLayout _layout;

		private readonly List<ActiveShot> _activeShots = new List<ActiveShot>();
		private float _stopZ;
		private bool _advancePending;

		public GamePhase Phase { get; private set; } = GamePhase.Idle;

		public PlayerBall Ball { get; }

		public float PlayerZ { get; private set; }

		public IReadOnlyList<ActiveShot> ActiveShots => _activeShots;

		public float PlayerVolumeFraction => Ball.VolumeFraction;

		public float ShotVolumeFraction => (Phase == GamePhase.Charging ? Ball.ChargedShotVolumeFraction : 0f) +
			GetActiveShotsVolumeFraction();

		public GameSession(GameConfig config, LevelLayout layout, float initialBallRadius)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_layout = layout ?? throw new ArgumentNullException(nameof(layout));
			Ball = new PlayerBall(config, initialBallRadius);
		}

		public event Action<float, float, IReadOnlyList<int>> ObstaclesDestroyed;
		public event Action Won;
		public event Action Lost;

		public bool BeginCharge()
		{
			if (Phase != GamePhase.Idle)
			{
				return false;
			}

			Ball.BeginCharge();
			Phase = GamePhase.Charging;
			return true;
		}

		public bool TickCharge(float deltaTime)
		{
			if (Phase != GamePhase.Charging)
			{
				return false;
			}

			Ball.Charge(deltaTime, out bool overcharged);
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

			if (Ball.ChargedShotRadius < _config.MinShotRadius)
			{
				Ball.CancelCharge();
				Phase = GamePhase.Idle;
				return false;
			}

			_activeShots.Add(new ActiveShot(Ball.ConsumeShot(), PlayerZ));
			Phase = GamePhase.Idle;
			return true;
		}

		public void CompleteObstacleDestruction(IReadOnlyList<int> obstacleIds)
		{
			if (obstacleIds == null)
			{
				throw new ArgumentNullException(nameof(obstacleIds));
			}

			bool removedAny = false;
			for (int index = 0; index < obstacleIds.Count; index++)
			{
				removedAny |= _layout.RemoveObstacle(obstacleIds[index]);
			}

			if (removedAny && Phase is not (GamePhase.Won or GamePhase.Lost))
			{
				RequestAdvance();
			}
		}

		public void Tick(float deltaTime)
		{
			if (Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			TickShots(deltaTime);
			if (Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			if (_advancePending && Phase == GamePhase.Idle)
			{
				StartAdvancing();
			}

			switch (Phase)
			{
				case GamePhase.Advancing:
					TickAdvance(deltaTime);
					break;

				case GamePhase.Idle:
					CheckProgress();
					break;
			}
		}

		private void TickShots(float deltaTime)
		{
			for (int index = _activeShots.Count - 1; index >= 0; index--)
			{
				ActiveShot shot = _activeShots[index];
				Obstacle nextObstacle = _layout.FindNextBlockingObstacle(shot.Z, shot.Radius);
				float nextShotZ = shot.Z + _config.ShotSpeed * deltaTime;

				if (nextObstacle != null &&
					nextObstacle.TryGetContactZ(shot.Radius, out float contactZ) &&
					nextShotZ >= contactZ)
				{
					_activeShots.RemoveAt(index);
					ResolveBlast(nextObstacle.PositionZ, shot.Radius, nextObstacle.Id);
					continue;
				}

				if (nextShotZ >= _layout.TargetZ)
				{
					_activeShots.RemoveAt(index);
					ResolveBlast(_layout.TargetZ, shot.Radius);
					continue;
				}

				_activeShots[index] = new ActiveShot(shot.Radius, nextShotZ);
			}
		}

		private void ResolveBlast(float blastZ, float shotRadius, int? directHitId = null)
		{
			float blastRadius = shotRadius * _config.BlastRadiusMultiplier;
			List<int> destroyed = DestroyObstaclesInRadius(blastZ, blastRadius, directHitId);

			ObstaclesDestroyed?.Invoke(blastZ, blastRadius, destroyed);
		}

		private float GetActiveShotsVolumeFraction()
		{
			float volume = 0f;
			for (int index = 0; index < _activeShots.Count; index++)
			{
				volume += VolumeMath.Cube(_activeShots[index].Radius);
			}

			return volume / VolumeMath.Cube(Ball.InitialRadius);
		}

		private void RequestAdvance()
		{
			_advancePending = true;
			if (Phase == GamePhase.Idle)
			{
				StartAdvancing();
			}
		}

		private List<int> DestroyObstaclesInRadius(float blastZ, float blastRadius, int? directHitId)
		{
			List<int> destroyed = new List<int>();
			if (directHitId.HasValue && _layout.MarkObstacleForRemoval(directHitId.Value))
			{
				destroyed.Add(directHitId.Value);
			}

			foreach (Obstacle obstacle in _layout.ObstaclesInRadius(blastZ, blastRadius))
			{
				if (_layout.MarkObstacleForRemoval(obstacle.Id))
				{
					destroyed.Add(obstacle.Id);
				}
			}

			return destroyed;
		}

		private void StartAdvancing()
		{
			_advancePending = false;
			Phase = GamePhase.Advancing;
			Obstacle nextBlocking = _layout.FindNextBlockingObstacle(
				PlayerZ,
				Ball.Radius + _config.ObstacleClearance
			);

			if (nextBlocking == null)
			{
				_stopZ = _layout.TargetZ;
			}
			else
			{
				_stopZ = Math.Max(
					PlayerZ,
					GetContactZ(nextBlocking, Ball.Radius + _config.ObstacleClearance) -
					_config.ObstacleApproachDistance
				);
			}
		}

		private void TickAdvance(float deltaTime)
		{
			PlayerZ = Math.Min(PlayerZ + _config.PlayerAdvanceSpeed * deltaTime, _stopZ);

			if (PlayerZ >= _layout.TargetZ)
			{
				Win();
			}
			else if (PlayerZ >= _stopZ)
			{
				Phase = GamePhase.Idle;
				CheckProgress();
			}
		}

		private void CheckProgress()
		{
			Obstacle nextBlocking = _layout.FindNextBlockingObstacle(
				PlayerZ,
				Ball.Radius + _config.ObstacleClearance
			);

			if (nextBlocking == null)
			{
				if (PlayerZ < _layout.TargetZ)
				{
					StartAdvancing();
				}

				return;
			}

			if (_activeShots.Count > 0)
			{
				return;
			}

			if (nextBlocking.IsPendingRemoval)
			{
				return;
			}

			if (Ball.IsCriticallySmall || Ball.MaxShotRadius < _config.MinShotRadius)
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
			if (Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			_activeShots.Clear();
			Phase = GamePhase.Lost;
			Lost?.Invoke();
		}

		private static float GetContactZ(Obstacle obstacle, float movingSphereRadius)
		{
			return obstacle.TryGetContactZ(movingSphereRadius, out float contactZ)
				? contactZ
				: obstacle.MinZ;
		}
	}
}
