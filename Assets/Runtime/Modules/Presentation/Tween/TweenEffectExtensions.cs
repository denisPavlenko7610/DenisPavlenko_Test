using System;
using UnityEngine;

namespace UnityTemplates.Tween
{
	public static class TweenEffectExtensions
	{

		// ==================================================
		// Jump implementation
		// ==================================================

		private sealed class JumpTween : TweenCore
		{
			private readonly Transform _transform;

			private readonly Vector3 _target;

			private readonly float _height;

			private readonly bool _local;

			private Vector3 _start;

			public override float Duration { get; }

			public JumpTween(Transform transform, Vector3 target, float duration, float height, bool local) : base(
				transform
			)
			{
				ValidateDuration(duration);

				if (!IsFinite(height))
				{
					throw new ArgumentOutOfRangeException(nameof(height));
				}

				_transform = transform;
				_target = target;
				Duration = duration;
				_height = height;
				_local = local;
			}

			protected override void PrepareTween()
			{
				_start = _local
					? _transform.localPosition
					: _transform.position;
			}

			protected override void EvaluateTween(float normalizedPosition)
			{
				float t = Mathf.Clamp01(normalizedPosition);

				Vector3 value = Vector3.LerpUnclamped(_start, _target, t);

				value.y += Mathf.Sin(t * Mathf.PI) * _height;

				if (_local)
				{
					_transform.localPosition = value;
				}
				else
				{
					_transform.position = value;
				}
			}
		}

		// ==================================================
		// Punch implementation
		// ==================================================

		private sealed class PunchVector3Tween : TweenCore
		{
			private readonly Func<Vector3> _getter;

			private readonly Action<Vector3> _setter;

			private readonly Vector3 _punch;

			private readonly int _vibrato;

			private readonly float _elasticity;

			private Vector3 _start;

			public override float Duration { get; }

			public PunchVector3Tween(
				object target,
				Func<Vector3> getter,
				Action<Vector3> setter,
				Vector3 punch,
				float duration,
				int vibrato,
				float elasticity
			) : base(target)
			{
				ValidateDuration(duration);
				ValidateVibrato(vibrato);
				ValidateElasticity(elasticity);

				_getter = getter ?? throw new ArgumentNullException(nameof(getter));
				_setter = setter ?? throw new ArgumentNullException(nameof(setter));

				_punch = punch;
				Duration = duration;
				_vibrato = vibrato;
				_elasticity = elasticity;
			}

			protected override void PrepareTween()
			{
				_start = _getter();
			}

			protected override void EvaluateTween(float normalizedPosition)
			{
				float value = EvaluatePunch(normalizedPosition, _vibrato, _elasticity);

				_setter(_start + _punch * value);
			}
		}

		private sealed class PunchRotationTween : TweenCore
		{
			private readonly Transform _transform;

			private readonly Vector3 _punch;

			private readonly int _vibrato;

			private readonly float _elasticity;

			private Quaternion _start;

			public override float Duration { get; }

			public PunchRotationTween(Transform transform, Vector3 punch, float duration, int vibrato, float elasticity)
				: base(transform)
			{
				ValidateDuration(duration);
				ValidateVibrato(vibrato);
				ValidateElasticity(elasticity);

				_transform = transform;
				_punch = punch;
				Duration = duration;
				_vibrato = vibrato;
				_elasticity = elasticity;
			}

			protected override void PrepareTween()
			{
				_start = _transform.localRotation;
			}

			protected override void EvaluateTween(float normalizedPosition)
			{
				float amount = EvaluatePunch(normalizedPosition, _vibrato, _elasticity);

				Quaternion offset = Quaternion.Euler(_punch * amount);

				_transform.localRotation = _start * offset;
			}
		}

		// ==================================================
		// Shake implementation
		// ==================================================

		private sealed class ShakeVector3Tween : TweenCore
		{
			private readonly Func<Vector3> _getter;

			private readonly Action<Vector3> _setter;

			private readonly Vector3 _strength;

			private readonly Vector3[] _samples;

			private Vector3 _start;

			public override float Duration { get; }

			public ShakeVector3Tween(
				object target,
				Func<Vector3> getter,
				Action<Vector3> setter,
				Vector3 strength,
				float duration,
				int vibrato,
				float randomness,
				int seed
			) : base(target)
			{
				ValidateDuration(duration);
				ValidateVibrato(vibrato);
				ValidateRandomness(randomness);

				_getter = getter ?? throw new ArgumentNullException(nameof(getter));
				_setter = setter ?? throw new ArgumentNullException(nameof(setter));

				_strength = strength;
				Duration = duration;

				_samples = BuildSamples(vibrato, randomness, seed);
			}

			protected override void PrepareTween()
			{
				_start = _getter();
			}

			protected override void EvaluateTween(float normalizedPosition)
			{
				float t = Mathf.Clamp01(normalizedPosition);

				if (t >= 1f)
				{
					_setter(_start);
					return;
				}

				Vector3 direction = EvaluateSamples(_samples, t);

				float decay = 1f - t;

				Vector3 offset = Vector3.Scale(direction, _strength) * decay;
				_setter(_start + offset);
			}
		}

		private sealed class ShakeRotationTween : TweenCore
		{
			private readonly Transform _transform;

			private readonly Vector3 _strength;

			private readonly Vector3[] _samples;

			private Quaternion _start;

			public override float Duration { get; }

			public ShakeRotationTween(
				Transform transform,
				Vector3 strength,
				float duration,
				int vibrato,
				float randomness,
				int seed
			) : base(transform)
			{
				ValidateDuration(duration);
				ValidateVibrato(vibrato);
				ValidateRandomness(randomness);

				_transform = transform;
				_strength = strength;
				Duration = duration;

				_samples = BuildSamples(vibrato, randomness, seed);
			}

			protected override void PrepareTween()
			{
				_start = _transform.localRotation;
			}

			protected override void EvaluateTween(float normalizedPosition)
			{
				float t = Mathf.Clamp01(normalizedPosition);

				if (t >= 1f)
				{
					_transform.localRotation = _start;

					return;
				}

				Vector3 direction = EvaluateSamples(_samples, t);

				float decay = 1f - t;

				Vector3 euler = Vector3.Scale(direction, _strength) * decay;

				_transform.localRotation = _start * Quaternion.Euler(euler);
			}
		}

		private const uint LcgMultiplier = 1664525u;

		private const uint LcgIncrement = 1013904223u;

		private const uint UpperBitsMask = 0x00FFFFFFu;

		private const int UpperBitsShift = 8;

		private const float UpperBitsScale = 0xFFFFFF; // 2^24 - 1

		private const uint GoldenRatioSeed = 0x9E3779B9u;

		private const int AxisDirectionCount = 6;

		private const float SqrMagnitudeEpsilon = 0.000001f;

		private const float DirectionFlipDot = 0.85f;

		// ==================================================
		// Jump
		// ==================================================

		public static TweenCore JumpTo(this Transform transform, Vector3 target, float duration, float height)
		{
			Require(transform);

			TweenCore tween = new JumpTween(transform, target, duration, height, false);

			tween.Play();

			return tween;
		}

		public static TweenCore LocalJumpTo(this Transform transform, Vector3 target, float duration, float height)
		{
			Require(transform);

			TweenCore tween = new JumpTween(transform, target, duration, height, true);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Punch Position
		// ==================================================

		public static TweenCore PunchPosition(
			this Transform transform,
			Vector3 punch,
			float duration,
			int vibrato = 8,
			float elasticity = 1f
		)
		{
			Require(transform);

			TweenCore tween =
				new PunchVector3Tween(
					transform,
					() => transform.position,
					value => transform.position = value,
					punch,
					duration,
					vibrato,
					elasticity
				);

			tween.Play();

			return tween;
		}

		public static TweenCore PunchLocalPosition(
			this Transform transform,
			Vector3 punch,
			float duration,
			int vibrato = 8,
			float elasticity = 1f
		)
		{
			Require(transform);

			TweenCore tween =
				new PunchVector3Tween(
					transform,
					() => transform.localPosition,
					value => transform.localPosition = value,
					punch,
					duration,
					vibrato,
					elasticity
				);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Punch Scale
		// ==================================================

		public static TweenCore PunchScale(
			this Transform transform,
			Vector3 punch,
			float duration,
			int vibrato = 8,
			float elasticity = 1f
		)
		{
			Require(transform);

			TweenCore tween =
				new PunchVector3Tween(
					transform,
					() => transform.localScale,
					value => transform.localScale = value,
					punch,
					duration,
					vibrato,
					elasticity
				);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Punch Rotation
		// ==================================================

		public static TweenCore PunchRotation(
			this Transform transform,
			Vector3 punchEuler,
			float duration,
			int vibrato = 8,
			float elasticity = 1f
		)
		{
			Require(transform);

			TweenCore tween =
				new PunchRotationTween(
					transform,
					punchEuler,
					duration,
					vibrato,
					elasticity
				);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Shake Position
		// ==================================================

		public static TweenCore ShakePosition(
			this Transform transform,
			Vector3 strength,
			float duration,
			int vibrato = 10,
			float randomness = 1f,
			int seed = 0
		)
		{
			Require(transform);

			TweenCore tween =
				new ShakeVector3Tween(
					transform,
					() => transform.position,
					value => transform.position = value,
					strength,
					duration,
					vibrato,
					randomness,
					seed
				);

			tween.Play();

			return tween;
		}

		public static TweenCore ShakePosition(
			this Transform transform,
			float strength,
			float duration,
			int vibrato = 10,
			float randomness = 1f,
			int seed = 0
		)
		{
			return transform.ShakePosition(
				Vector3.one * strength,
				duration,
				vibrato,
				randomness,
				seed
			);
		}

		public static TweenCore ShakeLocalPosition(
			this Transform transform,
			Vector3 strength,
			float duration,
			int vibrato = 10,
			float randomness = 1f,
			int seed = 0
		)
		{
			Require(transform);

			TweenCore tween =
				new ShakeVector3Tween(
					transform,
					() => transform.localPosition,
					value => transform.localPosition = value,
					strength,
					duration,
					vibrato,
					randomness,
					seed
				);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Shake Scale
		// ==================================================

		public static TweenCore ShakeScale(
			this Transform transform,
			Vector3 strength,
			float duration,
			int vibrato = 10,
			float randomness = 1f,
			int seed = 0
		)
		{
			Require(transform);

			TweenCore tween =
				new ShakeVector3Tween(
					transform,
					() => transform.localScale,
					value => transform.localScale = value,
					strength,
					duration,
					vibrato,
					randomness,
					seed
				);

			tween.Play();

			return tween;
		}

		// ==================================================
		// Shake Rotation
		// ==================================================

		public static TweenCore ShakeRotation(
			this Transform transform,
			Vector3 strengthEuler,
			float duration,
			int vibrato = 10,
			float randomness = 1f,
			int seed = 0
		)
		{
			Require(transform);

			TweenCore tween =
				new ShakeRotationTween(
					transform,
					strengthEuler,
					duration,
					vibrato,
					randomness,
					seed
				);

			tween.Play();

			return tween;
		}

		private static void Require(UnityEngine.Object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}
		}

		private static void ValidateDuration(float duration)
		{
			if (!IsFinite(duration) || duration < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(duration));
			}
		}

		private static void ValidateVibrato(int vibrato)
		{
			if (vibrato < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(vibrato), vibrato, "Vibrato must be positive.");
			}
		}

		private static void ValidateElasticity(float elasticity)
		{
			if (!IsFinite(elasticity) || elasticity < 0f || elasticity > 1f)
			{
				throw new ArgumentOutOfRangeException(
					nameof(elasticity),
					elasticity,
					"Elasticity must be between zero and one."
				);
			}
		}

		private static void ValidateRandomness(float randomness)
		{
			if (!IsFinite(randomness) || randomness < 0f || randomness > 1f)
			{
				throw new ArgumentOutOfRangeException(
					nameof(randomness),
					randomness,
					"Randomness must be between zero and one."
				);
			}
		}

		private static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		private static float EvaluatePunch(float normalizedPosition, int vibrato, float elasticity)
		{
			float t = Mathf.Clamp01(normalizedPosition);

			if (t is <= 0f or >= 1f)
			{
				return 0f;
			}

			float oscillation = Mathf.Sin(t * vibrato * Mathf.PI * 2f);

			float decay = 1f - t;

			// elasticity = 1 -> normal long punch
			// elasticity = 0 -> dies considerably faster.
			float damping = Mathf.Lerp(decay * decay * decay, decay, elasticity);

			return oscillation * damping;
		}

		// ==================================================
		// Deterministic shake samples
		// ==================================================

		private static Vector3[] BuildSamples(int vibrato, float randomness, int seed)
		{
			// +1 gives us a final zero sample so that interpolation
			// always converges smoothly back to the original value.
			Vector3[] result = new Vector3[vibrato + 1];

			uint state = seed == 0
				? GoldenRatioSeed
				: unchecked((uint)seed);

			Vector3 previous =
				Vector3.zero;

			for (int index = 0; index < vibrato; index++)
			{
				Vector3 random = NextUnitVector(ref state);

				Vector3 structured = StructuredDirection(index);

				Vector3 direction = Vector3.Lerp(structured, random, randomness);

				if (direction.sqrMagnitude > SqrMagnitudeEpsilon)
				{
					direction.Normalize();
				}

				// Avoid long stretches in virtually the same direction.
				if (index > 0 && Vector3.Dot(previous, direction) > DirectionFlipDot)
				{
					direction = -direction;
				}

				result[index] = direction;

				previous =
					direction;
			}

			result[^1] = Vector3.zero;

			return result;
		}

		private static Vector3 EvaluateSamples(Vector3[] samples, float normalizedPosition)
		{
			if (samples.Length <= 1)
			{
				return Vector3.zero;
			}

			float position = normalizedPosition * (samples.Length - 1);

			int from = Mathf.Min(Mathf.FloorToInt(position), samples.Length - 1);
			int to = Mathf.Min(from + 1, samples.Length - 1);

			float local = position - from;

			// Smooth interpolation avoids hard frame-to-frame jumps.
			float smooth = Mathf.SmoothStep(0f, 1f, local);

			return Vector3.LerpUnclamped(samples[from], samples[to], smooth);
		}

		private static Vector3 StructuredDirection(int index)
		{
			// A predictable base pattern prevents the shake from
			// accidentally drifting mostly along one direction.
			return (index % AxisDirectionCount) switch
			{
				0 => Vector3.right,
				1 => Vector3.up,
				2 => Vector3.forward,
				3 => Vector3.left,
				4 => Vector3.down,
				_ => Vector3.back
			};
		}

		private static Vector3 NextUnitVector(ref uint state)
		{
			float x = NextSigned(ref state);
			float y = NextSigned(ref state);
			float z = NextSigned(ref state);

			Vector3 value = new Vector3(x, y, z);

			return value.sqrMagnitude < SqrMagnitudeEpsilon ? Vector3.right : value.normalized;
		}

		private static float NextSigned(ref uint state)
		{
			state = state * LcgMultiplier + LcgIncrement;

			uint value = state >> UpperBitsShift & UpperBitsMask;

			float normalized = value / UpperBitsScale;

			return normalized * 2f - 1f;
		}
	}
}