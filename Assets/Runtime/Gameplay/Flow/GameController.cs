using System;
using System.Collections.Generic;
using DenisPavlenko.Game.Core;
using UnityEngine;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;
using VContainer;

namespace DenisPavlenko.Game
{
	public sealed partial class GameController : MonoBehaviour
	{
		[SerializeField] private ShotPool _shotPool;

		private readonly List<ObstacleView> _obstacleViews = new();

		private ISceneFlow _sceneFlow;
		private GameConfig _config;
		private GameWorldView _world;
		private GameSession _session;
		private GameplayDriver _driver;

		[Inject]
		private void Construct(ISceneFlow sceneFlow, GameConfig config, GameWorldView world)
		{
			_sceneFlow = sceneFlow;
			_config = config;
			_world = world;
		}

		public void Initialize()
		{
			ShotPresenter shotPresenter = new(_shotPool);

			List<Obstacle> obstacles = new();
			_world.CollectObstacles(_obstacleViews);
			for (int id = 0; id < _obstacleViews.Count; id++)
			{
				ObstacleView view = _obstacleViews[id];
				obstacles.Add(new Obstacle(id, view.PositionZ, view.CenterX, view.HalfWidth, view.HalfDepth));
			}

			_session = new GameSession(_config, new LevelLayout(obstacles, _world.TargetZ));
			_session.ShotFired += OnShotFired;
			_session.ObstaclesDestroyed += OnObstaclesDestroyed;
			_session.Won += OnWon;
			_session.Lost += OnLost;
			_world.RestartRequested += Restart;

			_driver = new GameplayDriver(_session, shotPresenter);

			UpdatePresentation();
		}

		private void Update()
		{
			if (_session == null || _session.Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			_driver.Tick(Time.deltaTime);
			UpdatePresentation();
		}

		private void UpdatePresentation() => _world.Present(_session);

		private void OnShotFired(float radius)
		{
			_driver.ShowFired(radius, _session.PlayerZ);
			_world.PulsePlayer();
		}

		private void OnObstaclesDestroyed(IReadOnlyList<int> ids)
		{
			_driver.HideShot();
			float blastRadius = _session.ShotRadius * _config.BlastRadiusMultiplier;
			_world.PlayImpact(blastRadius, _session.ShotZ, _obstacleViews, ids);
		}

		private void OnWon() => Finish(true);

		private void OnLost() => Finish(false);

		private void Finish(bool isWin)
		{
			_driver.HideShot();
			_world.ShowResult(isWin);
		}

		private void Restart()
		{
			if (!_sceneFlow.IsBusy)
			{
				_ = ReloadSceneAsync();
			}
		}

		private async Awaitable ReloadSceneAsync()
		{
			try
			{
				await _sceneFlow.ReloadActiveSceneAsync();
			}
			catch (Exception exception)
			{
				UnityLogger.LogException(exception, this);
			}
		}

		private void OnDestroy()
		{
			if (_session == null)
			{
				return;
			}

			_session.ShotFired -= OnShotFired;
			_session.ObstaclesDestroyed -= OnObstaclesDestroyed;
			_session.Won -= OnWon;
			_session.Lost -= OnLost;
			_world.RestartRequested -= Restart;
		}
	}
}
