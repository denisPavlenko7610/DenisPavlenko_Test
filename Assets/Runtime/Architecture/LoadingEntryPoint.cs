using System;
using UnityEngine;
using VContainer;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class LoadingEntryPoint : MonoBehaviour
	{
		private ISceneFlow _sceneFlow;

		[Inject]
		private void Construct(ISceneFlow sceneFlow)
		{
			_sceneFlow = sceneFlow;
		}

		private async void Start()
		{
			try
			{
				await _sceneFlow.ChangeSceneAsync(SceneIds.Game);
			}
			catch (Exception exception)
			{
				UnityLogger.LogException(exception, this);
			}
		}
	}
}
