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
		public bool IsPendingRemoval { get; private set; }

		public float MinX => CenterX - HalfWidth;
		public float MaxX => CenterX + HalfWidth;
		public float MinZ => PositionZ - HalfDepth;
		public float MaxZ => PositionZ + HalfDepth;
		public float FootprintRadius => Math.Max(HalfWidth, HalfDepth);
		public float DistanceToCenterLine => Math.Max(0f, Math.Abs(CenterX) - FootprintRadius);

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

		public bool TryGetContactZ(float movingSphereRadius, out float contactZ)
		{
			return TryGetContactRange(movingSphereRadius, out contactZ, out _);
		}

		public bool TryGetContactRange(float movingSphereRadius, out float enterZ, out float exitZ)
		{
			float combinedRadius = movingSphereRadius + FootprintRadius;
			float distanceToCenterLine = Math.Abs(CenterX);

			if (distanceToCenterLine > combinedRadius)
			{
				enterZ = 0f;
				exitZ = 0f;
				return false;
			}

			float longitudinalReach = (float)Math.Sqrt(
				combinedRadius * combinedRadius - distanceToCenterLine * distanceToCenterLine
			);
			enterZ = PositionZ - longitudinalReach;
			exitZ = PositionZ + longitudinalReach;
			return true;
		}

		public bool MarkForRemoval()
		{
			if (IsPendingRemoval)
			{
				return false;
			}

			IsPendingRemoval = true;
			return true;
		}
	}
}
