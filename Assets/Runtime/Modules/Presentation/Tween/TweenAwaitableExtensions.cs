using System;
using System.Threading;
using UnityEngine;

namespace UnityTemplates.Tween
{
	public static class TweenAwaitableExtensions
	{
		public static async Awaitable WaitForCompletionAsync(
			this TweenCore tween,
			CancellationToken cancellationToken = default
		)
		{
			RequireStandaloneTween(tween);

			while (!tween.IsComplete && !tween.IsKilled)
			{
				await Awaitable.NextFrameAsync(cancellationToken);
			}
		}

		public static async Awaitable WaitForLoopsAsync(
			this TweenCore tween,
			int loops,
			CancellationToken cancellationToken = default
		)
		{
			RequireStandaloneTween(tween);

			if (loops < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(loops));
			}

			while (!tween.IsComplete && !tween.IsKilled && tween.LoopsDone < loops)
			{
				await Awaitable.NextFrameAsync(cancellationToken);
			}
		}

		private static void RequireStandaloneTween(TweenCore tween)
		{
			if (tween == null)
			{
				throw new ArgumentNullException(nameof(tween));
			}

			if (tween.IsSequenceOwned)
			{
				throw new InvalidOperationException("Await the owning sequence instead of its child tween.");
			}
		}
	}
}