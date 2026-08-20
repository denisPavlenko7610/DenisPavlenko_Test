using DenisPavlenko.Game;
using DenisPavlenko.Game.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class GameLifetimeScope : LifetimeScope
	{
		[SerializeField] private GameConfig _config;
		[SerializeField] private PlayerBallView _playerPrefab;
		[SerializeField] private ExplosionView _explosionPrefab;
		[SerializeField] private Transform _playerSpawnPoint;
		[SerializeField] private Transform _runtimeRoot;
		[SerializeField] private GameWorldView _world;
		[SerializeField] private GameController _controller;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterInstance(_config);
			builder.RegisterComponentInNewPrefab(_playerPrefab, Lifetime.Scoped)
				.UnderTransform(_playerSpawnPoint);

			builder.RegisterComponentInNewPrefab(_explosionPrefab, Lifetime.Scoped)
				.UnderTransform(_runtimeRoot);

			builder.RegisterComponent(_world);
			builder.RegisterComponent(_controller);
			builder.RegisterBuildCallback(resolver => resolver.Resolve<GameController>().Initialize());
		}
	}
}
