using System;
using System.Collections.Generic;
using DenisPavlenko.Game.Core;
using UnityEngine;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;
using VContainer;

namespace DenisPavlenko.Game
{
	public sealed class GameController : MonoBehaviour
	{
		private readonly List<ObstacleView> _obstacleViews = new List<ObstacleView>();
		private readonly ShootInput _input = new ShootInput();

		private ISceneFlow _sceneFlow;
		private GameConfig _config;
		private GameWorldView _world;
		private GameSession _session;

		private void Update()
		{
			if (_session == null || _session.Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			_input.Poll();
			UpdateGameplay(Time.deltaTime);
			UpdatePresentation();
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

		public void Initialize()
		{
			List<Obstacle> obstacles = new List<Obstacle>();
			_world.CollectObstacles(_obstacleViews);
			for (int id = 0; id < _obstacleViews.Count; id++)
			{
				ObstacleView view = _obstacleViews[id];
				obstacles.Add(new Obstacle(id, view.PositionZ, view.CenterX, view.HalfWidth, view.HalfDepth));
			}

			LevelLayout layout = new LevelLayout(obstacles, _world.TargetZ);
			float startRadius = new LevelBalance(_config).ComputeStartRadius(layout);

			_session = new GameSession(_config, layout, startRadius);
			_session.ShotFired += OnShotFired;
			_session.ObstaclesDestroyed += OnObstaclesDestroyed;
			_session.Won += OnWon;
			_session.Lost += OnLost;
			_world.RestartRequested += Restart;

			UpdatePresentation();
		}

		[Inject]
		private void Construct(ISceneFlow sceneFlow, GameConfig config, GameWorldView world)
		{
			_sceneFlow = sceneFlow;
			_config = config;
			_world = world;
		}

		private void UpdateGameplay(float deltaTime)
		{
			_session.Tick(deltaTime);

			switch (_session.Phase)
			{
				case GamePhase.Idle when _input.PressedThisFrame:
					_session.BeginCharge();
					break;

				case GamePhase.Charging when _input.IsPressed:
					_session.TickCharge(deltaTime);
					break;

				case GamePhase.Charging:
					_session.ReleaseShot();
					break;
			}
		}

		private void UpdatePresentation()
		{
			_world.Present(_session);
		}

		private void OnShotFired()
		{
			_world.PulsePlayer();
		}

		private void OnObstaclesDestroyed(float blastZ, float blastRadius, IReadOnlyList<int> ids)
		{
			_world.PlayImpact(
				blastRadius,
				blastZ,
				_obstacleViews,
				ids,
				ids.Count > 0 ? () => _session.CompleteObstacleDestruction(ids) : null
			);
		}

		private void OnWon()
		{
			Finish(true);
		}

		private void OnLost()
		{
			Finish(false);
		}

		private void Finish(bool isWin)
		{
			_world.HideShot();
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
	}
}
