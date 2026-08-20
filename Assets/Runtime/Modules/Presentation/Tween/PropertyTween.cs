using System;

namespace UnityTemplates.Tween
{
	public sealed class PropertyTween<T> : TweenCore
	{
		private readonly Func<T> _getter;

		private readonly Action<T> _setter;

		private readonly T _targetValue;

		private readonly float _duration;

		private readonly Func<T, T, float, T> _interpolate;

		private readonly Func<T, T, T> _add;

		private EaseType _ease = EaseType.OutQuad;

		private bool _relative;

		private bool _hasFrom;

		private T _from;

		private T _start;

		private T _end;

		internal PropertyTween(
			object target,
			Func<T> getter,
			Action<T> setter,
			T targetValue,
			float duration,
			Func<T, T, float, T> interpolate,
			Func<T, T, T> add = null
		) : base(target)
		{
			if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(duration));
			}

			_getter = getter ?? throw new ArgumentNullException(nameof(getter));
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));

			_targetValue = targetValue;
			_duration = duration;

			_interpolate = interpolate ?? throw new ArgumentNullException(nameof(interpolate));
			_add = add;
		}

		public override float Duration => _duration;

		public PropertyTween<T> SetEase(EaseType ease)
		{
			EnsureCanConfigure();

			if (!Enum.IsDefined(typeof(EaseType), ease))
			{
				throw new ArgumentOutOfRangeException(nameof(ease));
			}

			_ease = ease;

			return this;
		}

		public PropertyTween<T> SetRelative(bool relative = true)
		{
			EnsureCanConfigure();

			if (relative && _add == null)
			{
				throw new InvalidOperationException($"Relative tweening is not supported for '{typeof(T).Name}'.");
			}

			_relative = relative;

			return this;
		}

		public PropertyTween<T> From(T value)
		{
			EnsureCanConfigure();

			_hasFrom = true;

			_from = value;

			return this;
		}

		protected override void PrepareTween()
		{
			T current = _getter();

			_start = _hasFrom
				? _from
				: current;

			if (_hasFrom)
			{
				_setter(_start);
			}

			_end = _relative
				? _add(_start, _targetValue)
				: _targetValue;
		}

		protected override void EvaluateTween(float normalizedPosition)
		{
			float eased = EaseUtility.Evaluate(_ease, normalizedPosition);

			_setter(_interpolate(_start, _end, eased));
		}
	}
}
