using System;
using UnityEngine;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;
using VContainer;
using VContainer.Unity;

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

		public void Start()
		{
			_ = LoadGameAsync();
		}
	}
}
