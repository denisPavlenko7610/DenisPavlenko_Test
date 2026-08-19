using System.Collections.Generic;
using UnityEngine;
using VContainer;
using DenisPavlenko.Game.Core;
using DenisPavlenko.Game.UI;
using UnityTemplates.Attributes;
using UnityTemplates.Foundation;
using UnityTemplates.Haptics;
using UnityTemplates.SceneFlow;

namespace DenisPavlenko.Game
{
	public sealed class GameFlowController : MonoBehaviour
	{
		private const float MinChargedShotVisualRadius = 0.08f;
		private const float ZoneTolerance = 0.01f;
		private const float RequiredMargin = 0.2f;
		private const string LevelValidationLog = "Level needs {0:P0} of mass budget, margin {1:P0}";
		private const string InsufficientMarginWarning = "Level margin {0:P0} is below required {1:P0}";

		[Inject] private ISceneFlow _sceneFlow;
		[Inject] private GameConfig _config;

		[Assign(AssignMode.Children)] [SerializeField] private PlayerBallView _playerBall;
		[Assign(AssignMode.Children)] [SerializeField] private ShotView _shot;
		[Assign(AssignMode.Children)] [SerializeField] private TrackView _track;
		[Assign(AssignMode.Children)] [SerializeField] private DoorView _door;
		[Assign(AssignMode.Children)] [SerializeField] private TargetView _target;
		[Assign(AssignMode.Children)] [SerializeField] private GameHud _hud;
		[Assign(AssignMode.Children)] [SerializeField] private Camera _camera;

		private readonly ShootInput _input = new();
		private readonly List<ObstacleView> _obstacleViews = new();
		private GameSession _session;
		private bool _finished;
		private bool _doorHapticPlayed;

		private void Start()
		{
			List<Obstacle> obstacles = new();
			int id = 0;
			foreach (ObstacleView view in GetComponentsInChildren<ObstacleView>(true))
			{
				_obstacleViews.Add(view);
				obstacles.Add(new Obstacle(id, view.PositionZ, view.CenterX, view.HalfWidth));
				id++;
			}

			_session = new GameSession(_config, new LevelLayout(obstacles, _target.transform.position.z));
			_session.ShotFired += radius => _shot.Show(radius, _session.PlayerZ);
			_session.ObstaclesDestroyed += OnObstaclesDestroyed;
			_session.Won += () => Finish(true);
			_session.Lost += () => Finish(false);
			_hud.RestartRequested += OnRestartRequested;
			UpdateBall();
			UpdateWorld();
			ValidateMargin();
		}

		private void Update()
		{
			if (_finished)
			{
				return;
			}

			_input.Poll();
			float deltaTime = Time.deltaTime;

			switch (_session.Phase)
			{
				case GamePhase.Idle:
					if (_input.PressedThisFrame && _session.BeginCharge())
					{
						Haptics.Play(HapticPreset.Selection);
					}
					break;

				case GamePhase.Charging:
					if (_input.IsPressed)
					{
						_session.TickCharge(deltaTime);
						_shot.Show(Mathf.Max(_session.Ball.ChargedShotRadius, MinChargedShotVisualRadius), _session.PlayerZ);
					}
					else
					{
						if (_session.ReleaseShot())
						{
							Haptics.Play(HapticPreset.LightImpact);
						}
						else
						{
							_shot.Hide();
						}
					}
					break;

				default:
					_session.Tick(deltaTime);
					break;
			}

			if (_session.ShotActive)
			{
				_shot.Set(_session.ShotRadius, _session.ShotZ);
			}
			else if (_session.Phase != GamePhase.Charging)
			{
				_shot.Hide();
			}

			UpdateBall();
			UpdateWorld();
		}

		private void UpdateBall()
		{
			float hop = _session.Phase == GamePhase.Advancing
				? Mathf.Abs(Mathf.Sin(Time.time * _config.BallHopFrequency)) * _config.BallHopHeight
				: 0f;
			_playerBall.Set(_session.Ball.Radius, _session.PlayerZ, hop);
			_hud.SetBallFraction(_session.Ball.Radius / _config.InitialBallRadius);
		}

		private void UpdateWorld()
		{
			_track.Set(_config.TrackWidthFactor * _session.Ball.Radius, _session.PlayerZ);

			float targetZ = _target.transform.position.z;
			float doorProgress = 1f - Mathf.Abs(targetZ - _session.PlayerZ) / _config.DoorOpenDistance;
			_door.SetProgress(doorProgress);

			if (doorProgress > 0f && !_doorHapticPlayed)
			{
				_doorHapticPlayed = true;
				Haptics.Play(HapticPreset.Selection);
			}

			Vector3 cameraPosition = new(-_config.CameraSideOffset, _config.CameraHeight, _session.PlayerZ - _config.CameraBackOffset);
			_camera.transform.position = cameraPosition;
			_camera.transform.rotation = Quaternion.LookRotation(
				new Vector3(0f, _config.CameraLookHeight, _session.PlayerZ + _config.CameraLookAhead) - cameraPosition);
		}

		private void OnObstaclesDestroyed(IReadOnlyList<int> ids)
		{
			foreach (int id in ids)
			{
				if (id < _obstacleViews.Count && _obstacleViews[id] != null)
				{
					Destroy(_obstacleViews[id].gameObject);
				}
			}

			if (ids.Count > 0)
			{
				Haptics.Play(HapticPreset.MediumImpact);
			}
		}

		private void Finish(bool isWin)
		{
			_finished = true;
			_shot.Hide();
			Haptics.Play(isWin ? HapticPreset.Success : HapticPreset.Failure);
			_hud.ShowResult(isWin);
		}

		private async void OnRestartRequested()
		{
			await _sceneFlow.ReloadActiveSceneAsync();
		}

		[Button("Validate Level")]
		private void ValidateLevel()
		{
			if (_target == null || _config == null)
			{
				return;
			}

			float budget = 1f - _config.CriticalBallFraction * _config.CriticalBallFraction * _config.CriticalBallFraction;
			float needed = 0f;
			float zoneZ = float.MaxValue;
			float zoneMin = float.MaxValue;

			foreach (ObstacleView view in GetComponentsInChildren<ObstacleView>(true))
			{
				if (Mathf.Abs(view.PositionZ - zoneZ) > ZoneTolerance)
				{
					AddZoneCost(ref needed, zoneMin);
					zoneZ = view.PositionZ;
					zoneMin = Mathf.Abs(view.CenterX);
				}
				else
				{
					zoneMin = Mathf.Min(zoneMin, Mathf.Abs(view.CenterX));
				}
			}
			AddZoneCost(ref needed, zoneMin);

			float margin = 1f - needed / budget;
			UnityLogger.Log(string.Format(LevelValidationLog, needed / budget, margin));
		}

		private void AddZoneCost(ref float total, float minCenterDistance)
		{
			if (minCenterDistance >= float.MaxValue)
			{
				return;
			}

			float shot = minCenterDistance / _config.BlastRadiusMultiplier;
			total += shot * shot * shot;
		}

		private void ValidateMargin()
		{
			if (_target == null || _config == null)
			{
				return;
			}

			float budget = 1f - _config.CriticalBallFraction * _config.CriticalBallFraction * _config.CriticalBallFraction;
			float needed = 0f;
			float zoneZ = float.MaxValue;
			float zoneMin = float.MaxValue;

			foreach (ObstacleView view in _obstacleViews)
			{
				if (Mathf.Abs(view.PositionZ - zoneZ) > ZoneTolerance)
				{
					AddZoneCost(ref needed, zoneMin);
					zoneZ = view.PositionZ;
					zoneMin = Mathf.Abs(view.CenterX);
				}
				else
				{
					zoneMin = Mathf.Min(zoneMin, Mathf.Abs(view.CenterX));
				}
			}
			AddZoneCost(ref needed, zoneMin);

			float margin = 1f - needed / budget;
			if (margin < RequiredMargin)
			{
				Debug.LogWarning(string.Format(InsufficientMarginWarning, margin, RequiredMargin));
			}
		}
	}
}