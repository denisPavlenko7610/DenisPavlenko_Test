using System;
using System.Collections.Generic;
using DenisPavlenko.Game.Core;
using UnityEngine;
using UnityTemplates.Foundation;
using UnityTemplates.SceneFlow;
using VContainer;

namespace DenisPavlenko.Game
{
	public sealed class GameFlowController : MonoBehaviour
	{
		[SerializeField] private ShotPool _shotPool;

#if UNITY_EDITOR
		[SerializeField] private GameConfig _gizmoConfig;
		[SerializeField] private Transform _gizmoOrigin;
		[SerializeField, Min(0.01f)] private float _gizmoShotRadius = 0.5f;
#endif

		private readonly ShootInput _input = new();
		private readonly List<ObstacleView> _obstacleViews = new();

		private ISceneFlow _sceneFlow;
		private GameConfig _config;
		private GameWorldView _world;
		private GameSession _session;
		private ShotView _shot;

		[Inject]
		private void Construct(ISceneFlow sceneFlow, GameConfig config, GameWorldView world)
		{
			_sceneFlow = sceneFlow;
			_config = config;
			_world = world;
		}

		public void Initialize()
		{
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

			UpdatePresentation();
		}

		private void Update()
		{
			if (_session == null)
			{
				return;
			}

			if (_session.Phase is GamePhase.Won or GamePhase.Lost)
			{
				return;
			}

			_input.Poll();
			UpdateGameplay(Time.deltaTime);
			UpdateShot();
			UpdatePresentation();
		}

		private void UpdateGameplay(float deltaTime)
		{
			switch (_session.Phase)
			{
				case GamePhase.Idle when _input.PressedThisFrame:
					_session.BeginCharge();
					break;

				case GamePhase.Charging when _input.IsPressed:
					_session.TickCharge(deltaTime);
					break;

				case GamePhase.Charging:
					if (!_session.ReleaseShot())
					{
						ReleaseShot();
					}
					break;

				default:
					_session.Tick(deltaTime);
					break;
			}
		}

		private void UpdateShot()
		{
			if (_session.Phase == GamePhase.Charging)
			{
				GetShot().Show(_session.Ball.ChargedShotRadius, _session.PlayerZ);
			}
			else if (_session.ShotActive)
			{
				GetShot().Set(_session.ShotRadius, _session.ShotZ);
			}
			else
			{
				ReleaseShot();
			}
		}

		private void UpdatePresentation()
		{
			_world.Present(_session);
		}

		private void OnShotFired(float radius)
		{
			GetShot().Show(radius, _session.PlayerZ);
			_world.PulsePlayer();
		}

		private void OnObstaclesDestroyed(IReadOnlyList<int> ids)
		{
			float blastRadius = _session.ShotRadius * _config.BlastRadiusMultiplier;
			ReleaseShot();
			_world.PlayImpact(blastRadius, _session.ShotZ, _obstacleViews, ids);
		}

		private void OnWon() => Finish(true);

		private void OnLost() => Finish(false);

		private void Finish(bool isWin)
		{
			ReleaseShot();
			_world.ShowResult(isWin);
		}

		private ShotView GetShot() => _shot ??= _shotPool.Get();

		private void ReleaseShot()
		{
			if (_shot == null)
			{
				return;
			}

			_shotPool.Release(_shot);
			_shot = null;
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

#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			GameConfig config = Application.isPlaying ? _config : _gizmoConfig;
			if (config == null || _gizmoOrigin == null)
			{
				return;
			}

			float shotRadius = _gizmoShotRadius;
			float shotZ = _gizmoOrigin.position.z;
			if (_session != null)
			{
				if (_session.Phase == GamePhase.Charging)
				{
					shotRadius = Mathf.Max(_session.Ball.ChargedShotRadius, _gizmoShotRadius);
					shotZ = _session.PlayerZ;
				}
				else if (_session.ShotActive)
				{
					shotRadius = _session.ShotRadius;
					shotZ = _session.ShotZ;
				}
			}

			Vector3 shotCenter = new(_gizmoOrigin.position.x, shotRadius, shotZ);
			Vector3 blastCenter = new(_gizmoOrigin.position.x, 0f, shotZ);
			float blastRadius = shotRadius * config.BlastRadiusMultiplier;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(shotCenter, shotRadius);
			Gizmos.color = new Color(1f, 0.35f, 0.05f);
			Gizmos.DrawWireSphere(blastCenter, blastRadius);
			UnityEditor.Handles.Label(shotCenter + Vector3.up * shotRadius,
				$"Shot: {shotRadius:0.00}  Blast: {blastRadius:0.00}");
		}
#endif
	}
}
