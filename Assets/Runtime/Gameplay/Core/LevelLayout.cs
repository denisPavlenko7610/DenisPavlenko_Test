using System;
using System.Collections.Generic;

namespace DenisPavlenko.Game.Core
{
	public sealed class LevelLayout
	{
		private readonly Dictionary<int, Obstacle> _byId = new();

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

		public float TargetZ { get; }
		public IReadOnlyCollection<Obstacle> Obstacles => _byId.Values;

		public IReadOnlyList<Obstacle> ObstaclesInRadius(float z, float x, float radius)
		{
			List<Obstacle> hit = new();

			foreach (Obstacle obstacle in _byId.Values)
			{
				float closestX = Math.Clamp(x, obstacle.MinX, obstacle.MaxX);
				float closestZ = Math.Clamp(z, obstacle.PositionZ - 0.5f, obstacle.PositionZ + 0.5f);
				float dx = x - closestX;
				float dz = z - closestZ;

				if (dx * dx + dz * dz <= radius * radius)
				{
					hit.Add(obstacle);
				}
			}

			return hit;
		}

		public bool RemoveObstacle(int id) => _byId.Remove(id);
	}
}