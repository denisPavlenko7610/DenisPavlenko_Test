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
		public float MinZ => PositionZ - HalfDepth;
		public float MaxZ => PositionZ + HalfDepth;
		public float BoundingRadius => (float)Math.Sqrt(HalfWidth * HalfWidth + HalfDepth * HalfDepth);

		public bool TryGetContactZ(float movingSphereRadius, out float contactZ)
		{
			float distanceToCenterLine = Math.Max(Math.Max(MinX, 0f), -MaxX);

			if (distanceToCenterLine > movingSphereRadius)
			{
				contactZ = 0f;
				return false;
			}

			float longitudinalReach = (float)Math.Sqrt(
				movingSphereRadius * movingSphereRadius - distanceToCenterLine * distanceToCenterLine
			);
			contactZ = MinZ - longitudinalReach;
			return true;
		}
	}
}
