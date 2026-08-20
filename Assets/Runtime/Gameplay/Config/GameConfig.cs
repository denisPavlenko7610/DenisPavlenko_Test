using UnityEngine;

namespace DenisPavlenko.Game.Core
{
	[CreateAssetMenu(menuName = "Denis Pavlenko/Game Config", fileName = "GameConfig")]
	public sealed class GameConfig : ScriptableObject
	{
		[Header("Ball")]
		[SerializeField, Range(0.05f, 0.5f)] private float _criticalVolumeFraction = 0.2f;
		[SerializeField, Min(0.01f)] private float _minimumShotRadius = 0.15f;
		[SerializeField, Min(0.1f)] private float _maxChargeDuration = 1.25f;

		[Header("Shot")]
		[SerializeField, Min(0.1f)] private float _shotSpeed = 14f;
		[SerializeField, Min(0.1f)] private float _blastRadiusMultiplier = 5f;
		[SerializeField, Min(0f)] private float _obstacleExplosionDelay = 0.04f;

		[Header("Movement")]
		[SerializeField, Min(1f)] private float _trackWidthPerBallRadius = 2.6f;
		[SerializeField, Min(0.1f)] private float _playerAdvanceSpeed = 9f;
		[SerializeField, Min(0f)] private float _obstacleClearance = 0.15f;
		[SerializeField, Min(0f)] private float _obstacleApproachDistance = 1.2f;

		public float CriticalVolumeFraction => _criticalVolumeFraction;
		public float MinShotRadius => _minimumShotRadius;
		public float MaxChargeDuration => _maxChargeDuration;
		public float ShotSpeed => _shotSpeed;
		public float BlastRadiusMultiplier => _blastRadiusMultiplier;
		public float ObstacleExplosionDelay => _obstacleExplosionDelay;
		public float TrackWidthPerBallRadius => _trackWidthPerBallRadius;
		public float PlayerAdvanceSpeed => _playerAdvanceSpeed;
		public float ObstacleClearance => _obstacleClearance;
		public float ObstacleApproachDistance => _obstacleApproachDistance;
	}
}
