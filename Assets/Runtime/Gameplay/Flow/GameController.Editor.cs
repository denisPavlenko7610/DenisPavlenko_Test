using DenisPavlenko.Game.Core;
using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed partial class GameController
	{
#if UNITY_EDITOR
		[SerializeField] private GameConfig _gizmoConfig;
		[SerializeField] private Transform _gizmoOrigin;
		[SerializeField, Min(0.01f)] private float _gizmoShotRadius = 0.5f;

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