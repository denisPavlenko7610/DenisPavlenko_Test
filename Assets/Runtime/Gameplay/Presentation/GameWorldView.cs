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
		[SerializeField] private Transform _obstaclesRoot;
		[SerializeField] private TrackView _track;
		[SerializeField] private DoorView _door;
		[SerializeField] private GameHud _hud;
		[SerializeField] private Camera _camera;
		[SerializeField] private Transform _cameraStart;
		[SerializeField] private Transform _cameraLookAt;

		private GameConfig _config;
		private PlayerBallView _player;
		private ExplosionView _explosion;

		public float TargetZ => _door.PositionZ;

		public event Action RestartRequested
		{
			add => _hud.RestartRequested += value;
			remove => _hud.RestartRequested -= value;
		}

		[Inject]
		private void Construct(GameConfig config, PlayerBallView player, ExplosionView explosion)
		{
			_config = config;
			_player = player;
			_explosion = explosion;
		}

		public void CollectObstacles(List<ObstacleView> results)
		{
			results.AddRange(_obstaclesRoot.GetComponentsInChildren<ObstacleView>(true));
		}

		public void Present(GameSession session)
		{
			float radius = session.Ball.Radius;
			_player.Set(radius, session.PlayerZ, session.Phase == GamePhase.Advancing);
			_track.SetWidth(_config.TrackWidthPerBallRadius * radius);
			_hud.SetVolumes(session.PlayerVolumeFraction, session.ShotVolumeFraction);
			_door.OpenWhenNear(session.PlayerZ);

			Vector3 offset = Vector3.forward * session.PlayerZ;
			Vector3 position = _cameraStart.position + offset;
			Vector3 lookPoint = _cameraLookAt.position + offset;
			_camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookPoint - position));
		}

		public void PulsePlayer() => _player.Pulse();

		public void PlayImpact(float blastRadius, float shotZ, IReadOnlyList<ObstacleView> obstacles,
			IReadOnlyList<int> destroyedIds)
		{
			_explosion.Play(blastRadius, shotZ);
			for (int index = 0; index < destroyedIds.Count; index++)
			{
				obstacles[destroyedIds[index]].Explode(index * _config.ObstacleExplosionDelay);
			}
		}

		public void ShowResult(bool isWin) => _hud.ShowResult(isWin);
	}
}
