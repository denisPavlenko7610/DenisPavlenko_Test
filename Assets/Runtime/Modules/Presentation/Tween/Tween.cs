using System;
using UnityEngine;

namespace UnityTemplates.Tween
{
	public static class Tween
	{

		private sealed class DelayTween : TweenCore
		{
			public override float Duration => 0f;

			protected override void PrepareTween() { }

			protected override void EvaluateTween(float normalizedPosition) { }
		}

		public static PropertyTween<float> To(
			object target,
			Func<float> getter,
			Action<float> setter,
			float endValue,
			float duration,
			bool autoPlay = true
		)
		{
			ValidateFinite(
				endValue,
				nameof(endValue)
			);

			return CreatePropertyTween(
				target,
				getter,
				setter,
				endValue,
				duration,
				autoPlay,
				Mathf.LerpUnclamped,
				(left, right) =>
					left + right
			);
		}

		public static PropertyTween<Vector2> To(
			object target,
			Func<Vector2> getter,
			Action<Vector2> setter,
			Vector2 endValue,
			float duration,
			bool autoPlay = true
		)
		{
			ValidateFinite(
				endValue,
				nameof(endValue)
			);

			return CreatePropertyTween(
				target,
				getter,
				setter,
				endValue,
				duration,
				autoPlay,
				Vector2.LerpUnclamped,
				(left, right) =>
					left + right
			);
		}

		public static PropertyTween<Vector3> To(
			object target,
			Func<Vector3> getter,
			Action<Vector3> setter,
			Vector3 endValue,
			float duration,
			bool autoPlay = true
		)
		{
			ValidateFinite(
				endValue,
				nameof(endValue)
			);

			return CreatePropertyTween(
				target,
				getter,
				setter,
				endValue,
				duration,
				autoPlay,
				Vector3.LerpUnclamped,
				(left, right) =>
					left + right
			);
		}

		public static PropertyTween<Color> To(
			object target,
			Func<Color> getter,
			Action<Color> setter,
			Color endValue,
			float duration,
			bool autoPlay = true
		)
		{
			ValidateFinite(
				endValue,
				nameof(endValue)
			);

			return CreatePropertyTween(
				target,
				getter,
				setter,
				endValue,
				duration,
				autoPlay,
				Color.LerpUnclamped,
				(left, right) =>
					left + right
			);
		}

		public static PropertyTween<Quaternion> To(
			object target,
			Func<Quaternion> getter,
			Action<Quaternion> setter,
			Quaternion endValue,
			float duration,
			bool autoPlay = true
		)
		{
			ValidateFinite(
				endValue,
				nameof(endValue)
			);

			return CreatePropertyTween(
				target,
				getter,
				setter,
				endValue,
				duration,
				autoPlay,
				Quaternion.SlerpUnclamped,
				(left, right) =>
					left * right
			);
		}

		/// <summary>
		///     Sequences are created paused so their timeline can be
		///     completely built before playback starts.
		/// </summary>
		public static TweenSequence Sequence()
		{
			return new TweenSequence();
		}

		public static TweenCore DelayedCall(
			float delay,
			Action callback
		)
		{
			if (callback == null)
			{
				throw new ArgumentNullException(nameof(callback));
			}

			if (float.IsNaN(delay) || float.IsInfinity(delay) || delay < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			DelayTween tween =
				new DelayTween();

			tween
				.SetDelay(delay)
				.OnComplete(callback)
				.Play();

			return tween;
		}

		public static int Kill(
			object target,
			bool complete = false
		)
		{
			return TweenManager
				.Instance
				.KillByTarget(
					target,
					complete
				);
		}

		public static int KillById(
			object id,
			bool complete = false
		)
		{
			return TweenManager
				.Instance
				.KillById(
					id,
					complete
				);
		}

		public static int KillAll(bool complete = false)
		{
			return TweenManager
				.Instance
				.KillAll(complete);
		}

		public static int PauseAll()
		{
			return TweenManager
				.Instance
				.PauseAll();
		}

		public static int ResumeAll()
		{
			return TweenManager
				.Instance
				.ResumeAll();
		}

		public static bool IsTweening(object target)
		{
			return TweenManager
				.Instance
				.HasTarget(target);
		}

		public static bool IsTweeningId(object id)
		{
			return TweenManager
				.Instance
				.HasId(id);
		}

		public static int ActiveCount()
		{
			return TweenManager
				.Instance
				.ActiveCount();
		}

		private static PropertyTween<T> CreatePropertyTween<T>(
			object target,
			Func<T> getter,
			Action<T> setter,
			T endValue,
			float duration,
			bool autoPlay,
			Func<T, T, float, T> interpolate,
			Func<T, T, T> add
		)
		{
			PropertyTween<T> tween =
				new PropertyTween<T>(
					target,
					getter,
					setter,
					endValue,
					duration,
					interpolate,
					add
				);

			if (autoPlay)
			{
				tween.Play();
			}

			return tween;
		}

		private static void ValidateFinite(
			float value,
			string parameterName
		)
		{
			if (float.IsNaN(value) || float.IsInfinity(value))
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
		}

		private static void ValidateFinite(
			Vector2 value,
			string parameterName
		)
		{
			ValidateFinite(
				value.x,
				parameterName
			);

			ValidateFinite(
				value.y,
				parameterName
			);
		}

		private static void ValidateFinite(
			Vector3 value,
			string parameterName
		)
		{
			ValidateFinite(
				value.x,
				parameterName
			);

			ValidateFinite(
				value.y,
				parameterName
			);

			ValidateFinite(
				value.z,
				parameterName
			);
		}

		private static void ValidateFinite(
			Color value,
			string parameterName
		)
		{
			ValidateFinite(
				value.r,
				parameterName
			);

			ValidateFinite(
				value.g,
				parameterName
			);

			ValidateFinite(
				value.b,
				parameterName
			);

			ValidateFinite(
				value.a,
				parameterName
			);
		}

		private static void ValidateFinite(
			Quaternion value,
			string parameterName
		)
		{
			ValidateFinite(
				value.x,
				parameterName
			);

			ValidateFinite(
				value.y,
				parameterName
			);

			ValidateFinite(
				value.z,
				parameterName
			);

			ValidateFinite(
				value.w,
				parameterName
			);
		}
	}
}