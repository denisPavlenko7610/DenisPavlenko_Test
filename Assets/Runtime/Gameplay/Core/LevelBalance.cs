using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class LevelBalance
	{
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

			// every obstacle needs a shot of radius obstacleRadius / blastMultiplier,
			// and such a shot costs shot^3 of ball volume.
			float totalShotVolume = TotalShotVolume(layout, out float largestShotRadius);
			largestShotRadius = Math.Max(largestShotRadius, _config.MinShotRadius);

			float startVolume = Math.Max(
				VolumeWithReserve(totalShotVolume),
				MinimumStartVolumeForLargestShot(largestShotRadius)
			);

			return VolumeMath.CubeRoot(startVolume);
		}

		private float TotalShotVolume(LevelLayout layout, out float largestShotRadius)
		{
			float total = 0f;
			largestShotRadius = 0f;

			foreach (Obstacle obstacle in layout.Obstacles)
			{
				float shotRadius = RequiredShotRadius(obstacle);
				total += VolumeMath.Cube(shotRadius);
				largestShotRadius = Math.Max(largestShotRadius, shotRadius);
			}

			return total;
		}

		private float RequiredShotRadius(Obstacle obstacle) =>
			obstacle.BoundingRadius / _config.BlastRadiusMultiplier;

		// Level cost plus 20% reserve.
		private float VolumeWithReserve(float totalShotVolume) =>
			totalShotVolume * _config.SafetyMargin;

		// Start volume that still lets the ball charge the biggest shot without hitting the critical size.
		private float MinimumStartVolumeForLargestShot(float largestShotRadius) =>
			VolumeMath.Cube(largestShotRadius) / (1f - _config.CriticalVolumeFraction);
	}
}
