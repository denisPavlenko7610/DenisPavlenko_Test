using System;
using System.Collections.Generic;
using DenisPavlenko.Game.Core;
using DenisPavlenko.Game.UI;
using UnityEngine;
using VContainer;

namespace DenisPavlenko.Game
{
	public sealed class GameWorldView : MonoBehaviour
	{
		[SerializeField] private Transform _blockingObstaclesRoot;
		[SerializeField] private TrackView _track;
		[SerializeField] private DoorView _door;
		[SerializeField] private GameHud _hud;
		[SerializeField] private Camera _camera;
		[SerializeField] private Transform _cameraStart;
		[SerializeField] private Transform _cameraLookAt;
		[SerializeField] private ShotView _shotPrefab;

		private GameConfig _config;
		private PlayerBallView _player;
		private ExplosionView _explosion;
		private GameCameraRig _cameraRig;
		private readonly List<ShotView> _activeShotViews = new List<ShotView>();
		private ShotView _chargingShot;

		public float TargetZ => _door.PositionZ;

		public event Action RestartRequested
		{
			add => _hud.RestartRequested += value;
			remove => _hud.RestartRequested -= value;
		}

		public void CollectObstacles(List<ObstacleView> results)
		{
			results.AddRange(_blockingObstaclesRoot.GetComponentsInChildren<ObstacleView>(true));
		}

		public void Present(GameSession session)
		{
			PresentPlayer(session);
			PresentTrack(session);
			PresentShot(session);
			PresentHud(session);
			PresentDoor(session);
			PresentCamera(session);
		}

		public void HideShot()
		{
			if (_chargingShot != null)
			{
				_chargingShot.Hide();
			}

			for (int index = 0; index < _activeShotViews.Count; index++)
			{
				_activeShotViews[index].Hide();
			}
		}

		public void PlayImpact(
			float blastRadius,
			float shotZ,
			IReadOnlyList<ObstacleView> obstacles,
			IReadOnlyList<int> destroyedIds,
			Action onObstaclesHidden
		)
		{
			_explosion.Play(blastRadius, shotZ);
			int remainingAnimations = destroyedIds.Count;
			for (int index = 0; index < destroyedIds.Count; index++)
			{
				obstacles[destroyedIds[index]].Explode(
					index * _config.ObstacleExplosionDelay,
					OnObstacleHidden
				);
			}

			void OnObstacleHidden()
			{
				remainingAnimations--;
				if (remainingAnimations == 0)
				{
					onObstaclesHidden?.Invoke();
				}
			}
		}

		public void ShowResult(bool isWin)
		{
			_hud.ShowResult(isWin);
		}

		[Inject]
		private void Construct(GameConfig config, PlayerBallView player, ExplosionView explosion)
		{
			_config = config;
			_player = player;
			_explosion = explosion;
			_cameraRig = new GameCameraRig(_camera, _cameraStart, _cameraLookAt);
		}

		private void PresentPlayer(GameSession session)
		{
			_player.Set(
				session.Ball.Radius,
				session.PlayerZ,
				session.Phase == GamePhase.Advancing
			);
		}

		private void PresentTrack(GameSession session)
		{
			_track.SetWidth(_config.TrackWidthPerBallRadius * session.Ball.Radius);
		}

		private void PresentShot(GameSession session)
		{
			PresentActiveShots(session.ActiveShots);

			if (session.Phase == GamePhase.Charging)
			{
				GetChargingShot().Show(session.Ball.ChargedShotRadius, session.PlayerZ);
			}
			else if (_chargingShot != null)
			{
				_chargingShot.Hide();
			}
		}

		private void PresentActiveShots(IReadOnlyList<ActiveShot> shots)
		{
			for (int index = 0; index < shots.Count; index++)
			{
				ActiveShot shot = shots[index];
				GetActiveShotView(index).Show(shot.Radius, shot.Z);
			}

			for (int index = shots.Count; index < _activeShotViews.Count; index++)
			{
				_activeShotViews[index].Hide();
			}
		}

		private void PresentHud(GameSession session)
		{
			_hud.SetVolumes(
				session.PlayerVolumeFraction,
				session.ShotVolumeFraction
			);
		}

		private void PresentDoor(GameSession session)
		{
			_door.OpenWhenNear(session.PlayerZ);
		}

		private void PresentCamera(GameSession session)
		{
			_cameraRig.Present(session.PlayerZ);
		}

		private ShotView GetChargingShot()
		{
			return _chargingShot ??= Instantiate(_shotPrefab, transform);
		}

		private ShotView GetActiveShotView(int index)
		{
			while (_activeShotViews.Count <= index)
			{
				_activeShotViews.Add(Instantiate(_shotPrefab, transform));
			}

			return _activeShotViews[index];
		}
	}
}
