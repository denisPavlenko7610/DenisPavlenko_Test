using System;
using UnityEngine;

namespace UnityTemplates.Tween
{
	public static class EaseUtility
	{
		private const float BackC1 = 1.70158f;
		private const float BackC2 = BackC1 * 1.525f;

		public static float Evaluate(EaseType ease, float t)
		{
			t = Mathf.Clamp01(t);

			return ease switch
			{
				EaseType.Linear => t,
				EaseType.InQuad => t * t,
				EaseType.OutQuad => 1f - (1f - t) * (1f - t),
				EaseType.InOutQuad => t < 0.5f
					? 2f * t * t
					: 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
				EaseType.InCubic => t * t * t,
				EaseType.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
				EaseType.InOutCubic => t < 0.5f
					? 4f * t * t * t
					: 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,
				EaseType.InQuart => t * t * t * t,
				EaseType.OutQuart => 1f - Mathf.Pow(1f - t, 4f),
				EaseType.InOutQuart => t < 0.5f
					? 8f * t * t * t * t
					: 1f - Mathf.Pow(-2f * t + 2f, 4f) / 2f,
				EaseType.InQuint => t * t * t * t * t,
				EaseType.OutQuint => 1f - Mathf.Pow(1f - t, 5f),
				EaseType.InOutQuint => t < 0.5f
					? 16f * t * t * t * t * t
					: 1f - Mathf.Pow(-2f * t + 2f, 5f) / 2f,
				EaseType.InSine => 1f - Mathf.Cos(t * Mathf.PI / 2f),
				EaseType.OutSine => Mathf.Sin(t * Mathf.PI / 2f),
				EaseType.InOutSine => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f,
				EaseType.InExpo => t <= 0f
					? 0f
					: Mathf.Pow(2f, 10f * t - 10f),
				EaseType.OutExpo => t >= 1f
					? 1f
					: 1f - Mathf.Pow(2f, -10f * t),
				EaseType.InOutExpo => EvaluateInOutExpo(t),
				EaseType.InCirc => 1f - Mathf.Sqrt(1f - t * t),
				EaseType.OutCirc => Mathf.Sqrt(1f - Mathf.Pow(t - 1f, 2f)),
				EaseType.InOutCirc => EvaluateInOutCirc(t),
				EaseType.InBack => EvaluateInBack(t),
				EaseType.OutBack => EvaluateOutBack(t),
				EaseType.InOutBack => EvaluateInOutBack(t),
				EaseType.InBounce => 1f - EvaluateOutBounce(1f - t),
				EaseType.OutBounce => EvaluateOutBounce(t),
				EaseType.InOutBounce => t < 0.5f
					? (1f - EvaluateOutBounce(1f - 2f * t)) / 2f
					: (1f + EvaluateOutBounce(2f * t - 1f)) / 2f,
				EaseType.InElastic => EvaluateInElastic(t),
				EaseType.OutElastic => EvaluateOutElastic(t),
				EaseType.InOutElastic => EvaluateInOutElastic(t),
				_ => throw new ArgumentOutOfRangeException(nameof(ease), ease, null)
			};
		}

		private static float EvaluateInOutExpo(float t)
		{
			return t switch
			{
				<= 0f => 0f,
				>= 1f => 1f,
				_ => t < 0.5f ? Mathf.Pow(2f, 20f * t - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * t + 10f)) / 2f
			};
		}

		private static float EvaluateInOutCirc(float t)
		{
			return t < 0.5f
				? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * t, 2f))) / 2f
				: (Mathf.Sqrt(1f - Mathf.Pow(-2f * t + 2f, 2f)) + 1f) / 2f;
		}

		private static float EvaluateInBack(float t)
		{
			float c3 = BackC1 + 1f;

			return c3 * t * t * t - BackC1 * t * t;
		}

		private static float EvaluateOutBack(float t)
		{
			float c3 = BackC1 + 1f;

			return 1f + c3 * Mathf.Pow(t - 1f, 3f) + BackC1 * Mathf.Pow(t - 1f, 2f);
		}

		private static float EvaluateInOutBack(float t)
		{
			return t < 0.5f
				? Mathf.Pow(2f * t, 2f) * ((BackC2 + 1f) * 2f * t - BackC2) / 2f
				: (Mathf.Pow(2f * t - 2f, 2f) * ((BackC2 + 1f) * (t * 2f - 2f) + BackC2) + 2f) / 2f;
		}

		private static float EvaluateOutBounce(float t)
		{
			const float n1 = 7.5625f;
			const float d1 = 2.75f;

			if (t < 1f / d1)
			{
				return n1 * t * t;
			}

			if (t < 2f / d1)
			{
				t -= 1.5f / d1;

				return n1 * t * t + 0.75f;
			}

			if (t < 2.5f / d1)
			{
				t -= 2.25f / d1;

				return n1 * t * t + 0.9375f;
			}

			t -= 2.625f / d1;

			return n1 * t * t + 0.984375f;
		}

		private static float EvaluateInElastic(float t)
		{
			if (t is <= 0f or >= 1f)
			{
				return t;
			}

			const float c4 = 2f * Mathf.PI / 3f;

			return -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * c4);
		}

		private static float EvaluateOutElastic(float t)
		{
			if (t is <= 0f or >= 1f)
			{
				return t;
			}

			const float c4 = 2f * Mathf.PI / 3f;

			return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
		}

		private static float EvaluateInOutElastic(float t)
		{
			if (t is <= 0f or >= 1f)
			{
				return t;
			}

			const float c5 = 2f * Mathf.PI / 4.5f;

			return t < 0.5f
				? -(Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f
				: (Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * c5)) / 2f + 1f;
		}
	}
}
