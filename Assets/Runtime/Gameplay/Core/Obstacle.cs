using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class Obstacle
	{
		public int Id { get; }
		public float PositionZ { get; }
		public float CenterX { get; }
		public float HalfWidth { get; }
		public float HalfDepth { get; }

		public Obstacle(int id, float positionZ, float centerX, float halfWidth, float halfDepth)
		{
			if (halfWidth <= 0f || halfDepth <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(halfWidth), "Obstacle size must be positive.");
			}

			Id = id;
			PositionZ = positionZ;
			CenterX = centerX;
			HalfWidth = halfWidth;
			HalfDepth = halfDepth;
		}

		public float MinX => CenterX - HalfWidth;
		public float MaxX => CenterX + HalfWidth;
	}
}
