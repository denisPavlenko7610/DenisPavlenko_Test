using System;
using System.Collections.Generic;

namespace DenisPavlenko.Game.Core
{
	public sealed class LevelLayout
	{
		private readonly Dictionary<int, Obstacle> _byId = new Dictionary<int, Obstacle>();

		public float TargetZ { get; }
		public IReadOnlyCollection<Obstacle> Obstacles => _byId.Values;

		public LevelLayout(IEnumerable<Obstacle> obstacles, float targetZ)
		{
			if (targetZ <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(targetZ));
			}

			TargetZ = targetZ;

			if (obstacles != null)
			{
				foreach (Obstacle obstacle in obstacles)
				{
					_byId.Add(obstacle.Id, obstacle);
				}
			}
		}

		public Obstacle FindNextBlockingObstacle(float fromZ, float movingSphereRadius)
		{
			Obstacle nearest = null;
			float nearestContactZ = float.MaxValue;

			foreach (Obstacle obstacle in _byId.Values)
			{
				if (!obstacle.TryGetContactRange(
						movingSphereRadius,
						out float contactZ,
						out float exitZ
					) ||
					exitZ <= fromZ)
				{
					continue;
				}

				contactZ = Math.Max(fromZ, contactZ);

				if (contactZ < nearestContactZ)
				{
					nearest = obstacle;
					nearestContactZ = contactZ;
				}
			}

			return nearest;
		}

		public IReadOnlyList<Obstacle> ObstaclesInRadius(float z, float radius)
		{
			List<Obstacle> hit = new List<Obstacle>();

			foreach (Obstacle obstacle in _byId.Values)
			{
				float dx = obstacle.CenterX;
				float dz = obstacle.PositionZ - z;
				float combinedRadius = radius + obstacle.FootprintRadius;

				if (dx * dx + dz * dz <= combinedRadius * combinedRadius)
				{
					hit.Add(obstacle);
				}
			}

			return hit;
		}

		public bool RemoveObstacle(int id)
		{
			return _byId.Remove(id);
		}

		public bool MarkObstacleForRemoval(int id)
		{
			return _byId.TryGetValue(id, out Obstacle obstacle) && obstacle.MarkForRemoval();
		}
	}
}
