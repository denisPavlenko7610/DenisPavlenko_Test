using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class BootstrapEntryPoint : IStartable
	{
		private ISceneFlow _sceneFlow;

		[Inject]
		private void Construct(ISceneFlow sceneFlow)
		{
			_sceneFlow = sceneFlow;
		}

		public void Start()
		{
			_ = LoadGameAsync();
		}

		private async Awaitable LoadGameAsync()
		{
			try
			{
				await _sceneFlow.ChangeSceneAsync(SceneIds.Game);
			}
			catch (Exception exception)
			{
				UnityLogger.LogException(exception);
			}
		}
	}
}
