using System;
using System.Collections.Generic;
using UnityEngine;
using UnityTemplates.Foundation;

namespace UnityTemplates.Tween
{
	internal sealed class TweenManager
	{
		private static TweenManager _instance;

		internal static TweenManager Instance
		{
			get
			{
				_instance ??= new TweenManager();

				return _instance;
			}
		}

		private readonly List<TweenCore> _active = new List<TweenCore>();

		private TweenRunner _runner;

		private TweenManager()
		{
			EnsureRunner();
		}

		internal void Register(TweenCore tween)
		{
			if (tween == null || tween.IsSequenceOwned || tween.IsComplete || tween.IsKilled)
			{
				return;
			}

			EnsureRunner();

			if (!_active.Contains(tween))
			{
				_active.Add(tween);
			}
		}

		internal void Update(TweenUpdateType updateType, float scaledDeltaTime, float unscaledDeltaTime)
		{
			for (int index = _active.Count - 1; index >= 0; index--)
			{
				TweenCore tween = _active[index];

				if (tween == null || tween.IsSequenceOwned || tween.IsComplete || tween.IsKilled)
				{
					_active.RemoveAt(index);

					continue;
				}

				if (tween.UpdateType != updateType)
				{
					continue;
				}

				float deltaTime = tween.IsUnscaledTime
					? unscaledDeltaTime
					: scaledDeltaTime;

				try
				{
					if (!tween.Tick(deltaTime))
					{
						_active.RemoveAt(index);
					}
				}
				catch (Exception exception)
				{
					UnityLogger.LogException(exception);

					tween.KillFromManager();

					_active.RemoveAt(index);
				}
			}
		}

		internal int KillByTarget(object target, bool complete)
		{
			return target == null
				? 0
				: KillMatching(complete, tween => ReferenceEquals(tween.Target, target));
		}

		internal int KillById(object id, bool complete)
		{
			return id == null
				? 0
				: KillMatching(complete, tween => Equals(tween.Id, id));
		}

		internal int KillAll(bool complete)
		{
			return KillMatching(complete, tween => true);
		}

		internal int PauseAll()
		{
			int count = 0;

			for (int index = 0; index < _active.Count; index++)
			{
				TweenCore tween =
					_active[index];

				if (tween == null || tween.IsSequenceOwned || !tween.IsPlaying)
				{
					continue;
				}

				tween.Pause();

				count++;
			}

			return count;
		}

		internal int ResumeAll()
		{
			int count = 0;

			for (int index = 0; index < _active.Count; index++)
			{
				TweenCore tween = _active[index];

				if (tween == null || tween.IsSequenceOwned || !tween.IsPaused)
				{
					continue;
				}

				tween.Resume();

				count++;
			}

			return count;
		}

		internal bool HasTarget(object target)
		{
			if (target == null)
			{
				return false;
			}

			for (int index = 0; index < _active.Count; index++)
			{
				TweenCore tween = _active[index];

				if (tween != null && tween.IsActive && !tween.IsSequenceOwned && ReferenceEquals(tween.Target, target))
				{
					return true;
				}
			}

			return false;
		}

		internal bool HasId(object id)
		{
			if (id == null)
			{
				return false;
			}

			for (int index = 0; index < _active.Count; index++)
			{
				TweenCore tween = _active[index];

				if (tween != null && tween.IsActive && !tween.IsSequenceOwned && Equals(tween.Id, id))
				{
					return true;
				}
			}

			return false;
		}

		internal int ActiveCount()
		{
			int count = 0;

			for (int index = 0; index < _active.Count; index++)
			{
				TweenCore tween = _active[index];

				if (tween != null && tween.IsActive && !tween.IsSequenceOwned)
				{
					count++;
				}
			}

			return count;
		}

		private int KillMatching(bool complete, Func<TweenCore, bool> match)
		{
			int count = 0;

			for (int index = _active.Count - 1; index >= 0; index--)
			{
				TweenCore tween = _active[index];

				if (tween == null || tween.IsKilled || tween.IsSequenceOwned || !match(tween))
				{
					continue;
				}

				tween.Kill(complete);

				count++;
			}

			return count;
		}

		private void EnsureRunner()
		{
			if (_runner != null)
			{
				return;
			}

			if (!Application.isPlaying)
			{
				throw new InvalidOperationException("Tweens can only run while the application is playing.");
			}

			TweenRunner existing = UnityEngine.Object.FindAnyObjectByType<TweenRunner>();

			if (existing != null)
			{
				_runner = existing;

				_runner.Initialize(this);

				return;
			}

			GameObject gameObject = new GameObject("[UnityTemplates.Tween]");

			UnityEngine.Object.DontDestroyOnLoad(gameObject);

			_runner = gameObject.AddComponent<TweenRunner>();

			_runner.Initialize(this);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void ResetStatics()
		{
			_instance = null;
		}
	}

	[DisallowMultipleComponent]
	internal sealed class TweenRunner : MonoBehaviour
	{
		private TweenManager _manager;

		private void Update()
		{
			_manager?.Update(TweenUpdateType.Normal, Time.deltaTime, Time.unscaledDeltaTime);
		}

		private void FixedUpdate()
		{
			_manager?.Update(TweenUpdateType.Fixed, Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
		}

		private void LateUpdate()
		{
			_manager?.Update(TweenUpdateType.Late, Time.deltaTime, Time.unscaledDeltaTime);
		}

		private void OnDestroy()
		{
			_manager = null;
		}

		internal void Initialize(TweenManager manager)
		{
			_manager = manager;
		}
	}
}