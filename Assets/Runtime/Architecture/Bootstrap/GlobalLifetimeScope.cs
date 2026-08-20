using UnityEngine;
using UnityTemplates.SceneFlow;
using VContainer;
using VContainer.Unity;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class GlobalLifetimeScope : LifetimeScope
	{
		[SerializeField] private SceneCatalog _sceneCatalog;

		protected override void Awake()
		{
			DontDestroyOnLoad(gameObject);
			base.Awake();
		}

		protected override void Configure(IContainerBuilder builder)
		{
			if (_sceneCatalog == null)
			{
				throw new System.InvalidOperationException("Scene catalog is required by the global lifetime scope.");
			}

			builder.RegisterInstance(_sceneCatalog);
			builder.Register<ISceneFlow>(
				resolver => new SceneFlow(resolver.Resolve<SceneCatalog>()),
				Lifetime.Singleton
			);
			builder.RegisterEntryPoint<BootstrapEntryPoint>();
		}
	}
}
