using System;
using UnityEngine;
using UnityTemplates.Foundation;

namespace UnityTemplates.Tween
{
	public abstract class TweenCore
	{
		private bool _prepared;
		private bool _started;
		private bool _sequenceOwned;

		private Action _onStart;
		private Action _onUpdate;
		private Action _onComplete;
		private Action _onKill;

		private const int InfiniteLoops = -1;

		private const double TimeEpsilon = 0.0000001d;

		public abstract float Duration { get; }

		public float Delay { get; private set; }

		public float Speed { get; private set; } = 1f;

		public int Loops { get; private set; } = 1;

		public int LoopsDone { get; private set; }

		public TweenLoopType LoopType { get; private set; } = TweenLoopType.Restart;

		public TweenUpdateType UpdateType { get; private set; } = TweenUpdateType.Normal;

		public bool IsUnscaledTime { get; private set; }

		public TweenState State { get; private set; } = TweenState.Idle;

		public bool IsPlaying => State == TweenState.Playing;

		public bool IsPaused => State == TweenState.Paused;

		public bool IsComplete => State == TweenState.Completed;

		public bool IsKilled => State == TweenState.Killed;

		public bool IsActive => State is TweenState.Playing or TweenState.Paused;

		public object Target { get; private set; }

		public object Id { get; private set; }

		public double Elapsed { get; private set; }

		public double TotalDuration
		{
			get
			{
				if (Loops < 0)
				{
					return double.PositiveInfinity;
				}

				return Delay + (double)Duration * Loops;
			}
		}

		public float NormalizedPosition => ResolveNormalized(Elapsed);

		internal bool IsSequenceOwned => _sequenceOwned;

		internal bool HasStarted => _started;

		internal double SequenceDuration => Delay + Duration;

		protected TweenCore(object target = null)
		{
			Target = target;
		}

		public TweenCore SetDelay(float delay)
		{
			EnsureCanConfigure();

			if (!IsFinite(delay) || delay < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(delay));
			}

			Delay = delay;

			return this;
		}

		public TweenCore SetSpeed(float speed)
		{
			if (!IsFinite(speed) || speed < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(speed));
			}

			EnsureNotKilled();

			Speed = speed;

			return this;
		}

		public TweenCore SetLoops(int loops, TweenLoopType loopType = TweenLoopType.Restart)
		{
			EnsureCanConfigure();

			if (loops != InfiniteLoops && loops < 1)
			{
				throw new ArgumentOutOfRangeException(
					nameof(loops),
					loops,
					"Loops must be positive or -1 for infinite."
				);
			}

			if (!Enum.IsDefined(typeof(TweenLoopType), loopType))
			{
				throw new ArgumentOutOfRangeException(nameof(loopType));
			}

			Loops = loops;

			LoopType = loopType;

			return this;
		}

		public TweenCore SetUpdate(TweenUpdateType updateType, bool unscaledTime = false)
		{
			EnsureCanConfigure();

			if (!Enum.IsDefined(typeof(TweenUpdateType), updateType))
			{
				throw new ArgumentOutOfRangeException(nameof(updateType));
			}

			UpdateType = updateType;

			IsUnscaledTime = unscaledTime;

			return this;
		}

		public TweenCore SetTarget(object target)
		{
			EnsureNotKilled();

			Target = target;

			return this;
		}

		public TweenCore SetId(object id)
		{
			EnsureNotKilled();

			Id = id;

			return this;
		}

		public TweenCore OnStart(Action callback) =>
			AttachCallback(ref _onStart, callback, nameof(callback));

		public TweenCore OnUpdate(Action callback) =>
			AttachCallback(ref _onUpdate, callback, nameof(callback));

		public TweenCore OnComplete(Action callback) =>
			AttachCallback(ref _onComplete, callback, nameof(callback));

		public TweenCore OnKill(Action callback) =>
			AttachCallback(ref _onKill, callback, nameof(callback));

		public TweenCore Play()
		{
			EnsureStandaloneControl();

			if (State == TweenState.Killed)
			{
				throw new InvalidOperationException("A killed tween cannot be played again.");
			}

			if (State == TweenState.Completed)
			{
				return Restart(play: true);
			}

			if (State == TweenState.Playing)
			{
				return this;
			}

			State = TweenState.Playing;

			TweenManager.Instance.Register(this);

			return this;
		}

		public TweenCore Pause()
		{
			EnsureStandaloneControl();

			EnsureNotKilled();

			if (State == TweenState.Playing)
			{
				State = TweenState.Paused;
			}

			return this;
		}

		public TweenCore Resume()
		{
			return Play();
		}

		public TweenCore Kill(bool complete = false)
		{
			EnsureStandaloneControl();

			if (State == TweenState.Killed)
			{
				return this;
			}

			if (complete && State != TweenState.Completed)
			{
				CompleteInternal(withCallbacks: true);
			}

			KillInternal();

			return this;
		}

		public TweenCore Complete(bool withCallbacks = true)
		{
			EnsureStandaloneControl();

			CompleteInternal(withCallbacks);

			return this;
		}

		public TweenCore Rewind(bool play = false)
		{
			EnsureStandaloneControl();

			EnsureNotKilled();

			Elapsed = 0d;

			LoopsDone = 0;

			_started = false;

			if (_prepared)
			{
				EvaluateTween(0f);
			}

			State = play
				? TweenState.Playing
				: TweenState.Paused;

			if (play)
			{
				TweenManager.Instance.Register(this);
			}

			return this;
		}

		public TweenCore Restart(bool play = true)
		{
			return Rewind(play);
		}

		public TweenCore Goto(float time, bool play = false)
		{
			EnsureStandaloneControl();

			EnsureNotKilled();

			if (!IsFinite(time))
			{
				throw new ArgumentOutOfRangeException(nameof(time));
			}

			EnsurePrepared();

			_started = true;

			float clamped = Mathf.Clamp(time, 0f, Duration);

			Elapsed = Delay + clamped;

			LoopsDone =
				CalculateLoopsDone(Elapsed);

			EvaluateTween(
				Duration <= 0f
					? 1f
					: clamped / Duration
			);

			State = play
				? TweenState.Playing
				: TweenState.Paused;

			if (play)
			{
				TweenManager.Instance.Register(this);
			}

			return this;
		}

		public TweenCore GotoNormalized(float normalizedPosition, bool play = false)
		{
			if (!IsFinite(normalizedPosition))
			{
				throw new ArgumentOutOfRangeException(nameof(normalizedPosition));
			}

			return Goto(Duration * Mathf.Clamp01(normalizedPosition), play);
		}

		internal bool Tick(float deltaTime)
		{
			if (_sequenceOwned || State == TweenState.Completed || State == TweenState.Killed)
			{
				return false;
			}

			if (State != TweenState.Playing)
			{
				return true;
			}

			if (IsTargetDestroyed())
			{
				KillInternal();

				return false;
			}

			if (!IsFinite(deltaTime) || deltaTime < 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(deltaTime));
			}

			double delta = deltaTime * (double)Speed;

			if (double.IsNaN(delta) || double.IsInfinity(delta))
			{
				throw new InvalidOperationException("Tween delta time overflowed.");
			}

			if (delta <= 0d)
			{
				return true;
			}

			double totalDuration = TotalDuration;

			double next = Elapsed + delta;

			if (!double.IsInfinity(totalDuration))
			{
				next = Math.Min(next, totalDuration);
			}

			if (!_started && next >= Delay)
			{
				EnsurePrepared();

				_started = true;

				InvokeSafely(_onStart);

				if (State == TweenState.Killed)
				{
					return false;
				}
			}

			Elapsed = next;

			if (_started)
			{
				LoopsDone = CalculateLoopsDone(Elapsed);

				EvaluateTween(ResolveNormalized(Elapsed));

				InvokeSafely(_onUpdate);

				if (State is TweenState.Killed or TweenState.Completed)
				{
					return false;
				}
			}

			if (!double.IsInfinity(totalDuration) && Elapsed >= totalDuration)
			{
				State = TweenState.Completed;

				InvokeSafely(_onComplete);

				return false;
			}

			return true;
		}

		internal void KillFromManager()
		{
			if (State != TweenState.Killed)
			{
				KillInternal();
			}
		}

		internal void AttachToSequence()
		{
			if (_sequenceOwned)
			{
				throw new InvalidOperationException("The tween already belongs to a sequence.");
			}

			if (_started)
			{
				throw new InvalidOperationException("A tween cannot be added to a sequence after it has started.");
			}

			if (State is TweenState.Completed or TweenState.Killed)
			{
				throw new InvalidOperationException("A completed or killed tween cannot be added to a sequence.");
			}

			if (Loops != 1)
			{
				throw new InvalidOperationException("Child tweens inside a sequence must use exactly one loop.");
			}

			if (HasCallbacks())
			{
				throw new InvalidOperationException(
					"Child tween callbacks are not supported inside a sequence. "
					+ "Attach callbacks to the sequence instead."
				);
			}

			_sequenceOwned = true;

			State = TweenState.Paused;
		}

		internal void EvaluateFromSequence(double sequenceTime)
		{
			if (!_sequenceOwned)
			{
				throw new InvalidOperationException("Only sequence-owned tweens can be evaluated by a sequence.");
			}

			if (IsTargetDestroyed())
			{
				throw new MissingReferenceException("A tween target used by a sequence was destroyed.");
			}

			if (Duration <= 0f)
			{
				if (sequenceTime < Delay)
				{
					if (_prepared)
					{
						EvaluateTween(0f);
					}

					return;
				}

				EnsurePrepared();

				_started = true;

				EvaluateTween(1f);

				return;
			}

			if (sequenceTime <= Delay)
			{
				if (_prepared)
				{
					EvaluateTween(0f);
				}

				return;
			}

			EnsurePrepared();

			_started = true;

			double activeTime = sequenceTime - Delay;

			float normalized = Mathf.Clamp01((float)(activeTime / Duration));

			EvaluateTween(normalized);
		}

		protected void EnsureCanConfigure()
		{
			EnsureNotKilled();

			if (_started)
			{
				throw new InvalidOperationException(
					"Tween configuration cannot be changed after playback has started."
				);
			}
		}

		protected abstract void PrepareTween();

		protected abstract void EvaluateTween(float normalizedPosition);

		private void CompleteInternal(bool withCallbacks)
		{
			if (State is TweenState.Killed or TweenState.Completed)
			{
				return;
			}

			EnsurePrepared();

			if (!_started)
			{
				_started = true;

				InvokeSafely(_onStart);

				if (State == TweenState.Killed)
				{
					return;
				}
			}

			if (Loops > 0)
			{
				Elapsed = TotalDuration;

				LoopsDone = Loops;
			}
			else
			{
				Elapsed = Delay + Duration;
			}

			EvaluateTween(ResolveNormalized(Elapsed));

			State = TweenState.Completed;

			if (withCallbacks)
			{
				InvokeSafely(_onComplete);
			}
		}

		private void KillInternal()
		{
			if (State == TweenState.Killed)
			{
				return;
			}

			State = TweenState.Killed;

			InvokeSafely(_onKill);
		}

		private void EnsurePrepared()
		{
			if (_prepared)
			{
				return;
			}

			if (IsTargetDestroyed())
			{
				throw new MissingReferenceException("The tween target was destroyed.");
			}

			PrepareTween();

			_prepared = true;
		}

		private int CalculateLoopsDone(double elapsed)
		{
			if (!_started)
			{
				return 0;
			}

			if (Duration <= 0f)
			{
				return Loops > 0
					? Loops
					: 1;
			}

			double active = Math.Max(0d, elapsed - Delay);

			long completed = (long)Math.Floor(active / Duration);

			if (Loops > 0)
			{
				completed = Math.Min(completed, Loops);
			}

			return completed >= int.MaxValue
				? int.MaxValue
				: (int)completed;
		}

		private float ResolveNormalized(double elapsed)
		{
			if (!_started && !_prepared)
			{
				return 0f;
			}

			if (Duration <= 0f)
			{
				return 1f;
			}

			double active = Math.Max(0d, elapsed - Delay);

			if (active <= 0d)
			{
				return 0f;
			}

			long loopIndex;

			double localTime;

			double configuredDuration = Loops > 0
				? (double)Duration * Loops
				: double.PositiveInfinity;

			if (Loops > 0 && active >= configuredDuration)
			{
				loopIndex = Loops - 1L;

				localTime = Duration;
			}
			else
			{
				loopIndex = (long)Math.Floor(active / Duration);

				localTime = active - loopIndex * Duration;

				if (localTime <= TimeEpsilon && active > 0d)
				{
					loopIndex = Math.Max(0L, loopIndex - 1L);

					localTime = Duration;
				}
			}

			float normalized = Mathf.Clamp01((float)(localTime / Duration));

			if (LoopType == TweenLoopType.Yoyo && (loopIndex & 1L) != 0L)
			{
				normalized = 1f - normalized;
			}

			return normalized;
		}

		private bool HasCallbacks()
		{
			return
				_onStart != null || _onUpdate != null || _onComplete != null || _onKill != null;
		}

		private void EnsureCanAddCallbacks()
		{
			EnsureNotKilled();

			if (_sequenceOwned)
			{
				throw new InvalidOperationException(
					"Callbacks must be attached to the owning sequence, not to a child tween."
				);
			}
		}

		private TweenCore AttachCallback(ref Action field, Action callback, string parameterName)
		{
			EnsureCanAddCallbacks();

			field += callback ?? throw new ArgumentNullException(parameterName);

			return this;
		}

		private void EnsureStandaloneControl()
		{
			if (_sequenceOwned)
			{
				throw new InvalidOperationException("A tween owned by a sequence cannot be controlled directly.");
			}
		}

		private void EnsureNotKilled()
		{
			if (State == TweenState.Killed)
			{
				throw new InvalidOperationException("The tween has been killed.");
			}
		}

		private bool IsTargetDestroyed()
		{
			return Target is UnityEngine.Object unityObject && unityObject == null;
		}

		private static void InvokeSafely(Action callback)
		{
			if (callback == null)
			{
				return;
			}

			try
			{
				callback();
			}
			catch (Exception exception)
			{
				UnityLogger.LogException(exception);
			}
		}

		private static bool IsFinite(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}
	}
}
