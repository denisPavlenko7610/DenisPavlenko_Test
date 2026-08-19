using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class Obstacle
	{
		public int Id { get; }
		public float PositionZ { get; }
		public float CenterX { get; }
		public float HalfWidth { get; }

		public Obstacle(int id, float positionZ, float centerX, float halfWidth)
		{
			if (halfWidth <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(halfWidth));
			}

			Id = id;
			PositionZ = positionZ;
			CenterX = centerX;
			HalfWidth = halfWidth;
		}

		public float MinX => CenterX - HalfWidth;
		public float MaxX => CenterX + HalfWidth;
	}
}