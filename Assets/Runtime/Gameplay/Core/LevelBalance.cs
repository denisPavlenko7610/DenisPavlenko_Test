using System;
using System.Collections.Generic;

namespace DenisPavlenko.Game.Core
{
	public sealed class LevelBalance
	{
		private const int MaxBalanceAttempts = 64;
		private const float VolumeGrowth = 1.1f;

		private readonly GameConfig _config;

		public LevelBalance(GameConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
		}

		public float ComputeStartRadius(LevelLayout layout)
		{
			if (layout == null)
			{
				throw new ArgumentNullException(nameof(layout));
			}

			float startVolume = EstimateRequiredVolume(layout);

			for (int attempt = 0; attempt < MaxBalanceAttempts; attempt++)
			{
				if (CanClearRoute(layout, startVolume))
				{
					return VolumeMath.CubeRoot(startVolume);
				}

				startVolume *= VolumeGrowth;
			}

			throw new InvalidOperationException("Could not balance the current level layout.");
		}

		private float EstimateRequiredVolume(LevelLayout layout)
		{
			float volume = VolumeMath.Cube(_config.MinShotRadius);
			foreach (Obstacle obstacle in layout.Obstacles)
			{
				float shotRadius = Math.Max(
					_config.MinShotRadius,
					obstacle.FootprintRadius / _config.BlastRadiusMultiplier
				);
				volume += VolumeMath.Cube(shotRadius);
			}

			return volume / (1f - _config.CriticalVolumeFraction);
		}

		private bool CanClearRoute(LevelLayout source, float startVolume)
		{
			LevelLayout layout = new LevelLayout(source.Obstacles, source.TargetZ);
			float remainingVolume = startVolume;
			float reserveVolume = startVolume * _config.CriticalVolumeFraction;

			for (int shot = 0; shot < layout.Obstacles.Count; shot++)
			{
				float playerRadius = VolumeMath.CubeRoot(remainingVolume);
				if (layout.FindNextBlockingObstacle(
						0f,
						playerRadius + _config.ObstacleClearance
					) == null)
				{
					return true;
				}

				float shotRadius = SmallestShotRadius(layout.Obstacles);
				float shotVolume = VolumeMath.Cube(shotRadius);
				if (remainingVolume - shotVolume < reserveVolume)
				{
					return false;
				}

				Obstacle hit = layout.FindNextBlockingObstacle(0f, shotRadius);
				if (hit == null)
				{
					return false;
				}

				remainingVolume -= shotVolume;
				layout.RemoveObstacle(hit.Id);
				foreach (Obstacle obstacle in layout.ObstaclesInRadius(
					hit.PositionZ,
					shotRadius * _config.BlastRadiusMultiplier
				))
				{
					layout.RemoveObstacle(obstacle.Id);
				}
			}

			return false;
		}

		private float SmallestShotRadius(IEnumerable<Obstacle> obstacles)
		{
			float closestDistance = float.MaxValue;
			foreach (Obstacle obstacle in obstacles)
			{
				closestDistance = Math.Min(closestDistance, obstacle.DistanceToCenterLine);
			}

			return Math.Max(_config.MinShotRadius, closestDistance);
		}
	}
}
