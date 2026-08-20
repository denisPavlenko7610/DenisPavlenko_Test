using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTemplates.Tween
{
	public sealed class TweenSequence : TweenCore
	{
		private sealed class Item
		{
			public TweenCore Tween;

			public float Start;

			public float End;
		}

		private readonly List<Item> _items = new List<Item>();

		private float _duration;

		private float _lastAppendStart;

		public override float Duration => _duration;

		internal TweenSequence() { }

		public TweenSequence Append(TweenCore tween)
		{
			EnsureCanEdit();

			if (tween == null)
			{
				throw new ArgumentNullException(nameof(tween));
			}

			float start = _duration;

			AddTween(tween, start);

			_lastAppendStart = start;

			return this;
		}

		public TweenSequence Join(TweenCore tween)
		{
			EnsureCanEdit();

			if (tween == null)
			{
				throw new ArgumentNullException(nameof(tween));
			}

			AddTween(tween, _lastAppendStart);

			return this;
		}

		public TweenSequence Insert(float time, TweenCore tween)
		{
			EnsureCanEdit();

			if (float.IsNaN(time) || float.IsInfinity(time) || time < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(time));
			}

			if (tween == null)
			{
				throw new ArgumentNullException(nameof(tween));
			}

			AddTween(tween, time);

			return this;
		}

		public TweenSequence AppendInterval(float duration)
		{
			EnsureCanEdit();

			if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(duration));
			}

			float start = _duration;

			_duration += duration;

			_lastAppendStart = start;

			return this;
		}

		protected override void PrepareTween()
		{
			// Children prepare lazily when their timeline position
			// is reached.
		}

		protected override void EvaluateTween(float normalizedPosition)
		{
			float time = _duration <= 0f
				? 0f
				: normalizedPosition * _duration;

			for (int index = 0; index < _items.Count; index++)
			{
				Item item = _items[index];

				double localTime = time - item.Start;

				item.Tween.EvaluateFromSequence(localTime);
			}
		}

		private void AddTween(TweenCore tween, float start)
		{
			if (ReferenceEquals(tween, this))
			{
				throw new InvalidOperationException("A sequence cannot contain itself.");
			}

			double childDuration = tween.SequenceDuration;

			if (double.IsNaN(childDuration) || double.IsInfinity(childDuration) || childDuration < 0d)
			{
				throw new InvalidOperationException("The child tween has an invalid sequence duration.");
			}

			double endDouble = start + childDuration;

			if (endDouble > float.MaxValue)
			{
				throw new InvalidOperationException("The sequence duration overflowed.");
			}

			tween.AttachToSequence();

			float end = (float)endDouble;

			_items.Add(new Item { Tween = tween, Start = start, End = end });

			_duration = Mathf.Max(_duration, end);
		}

		private void EnsureCanEdit()
		{
			EnsureCanConfigure();

			if (HasStarted)
			{
				throw new InvalidOperationException("A sequence cannot be modified after playback has started.");
			}
		}
	}
}