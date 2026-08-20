using System;

namespace DenisPavlenko.Game.Core
{
	public static class VolumeMath
	{
		private const double CubeRootExponent = 1.0 / 3.0;

		public static float Cube(float value)
		{
			return value * value * value;
		}

		public static float CubeRoot(float value)
		{
			return (float)Math.Pow(value, CubeRootExponent);
		}

		public static float Diameter(float radius)
		{
			return radius * 2f;
		}
	}
}
