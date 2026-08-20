using DenisPavlenko.Game.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class PlayerBallView : MonoBehaviour
	{
		[FormerlySerializedAs("_hopHeight"), SerializeField, Min(0f)]
			private float _advanceHopHeight = 0.6f;
		[FormerlySerializedAs("_hopFrequency"), SerializeField, Min(0f)]
			private float _advanceHopCyclesPerSecond = 14f;
		[SerializeField, Min(1f)] private float _pulseScale = 1.08f;
		[SerializeField, Min(0.01f)] private float _pulseDuration = 0.18f;

		private float _radius;
		private float _pulseMultiplier = 1f;

		public void Set(float radius, float z, bool isMoving)
		{
			_radius = radius;
			float hop = isMoving
				? Mathf.Abs(Mathf.Sin(Time.time * _advanceHopCyclesPerSecond)) * _advanceHopHeight
				: 0f;
			ApplyScale();
			transform.position = new Vector3(0f, radius + hop, z);
		}

		public void Pulse()
		{
			Tween.Kill(this);
			float pulseScale = Mathf.Max(1f, _pulseScale);
			Tween.To(this, () => _pulseMultiplier, SetPulseMultiplier, pulseScale, _pulseDuration * 0.5f)
				.SetEase(EaseType.OutQuad)
				.OnComplete(() => Tween.To(
						this,
						() => _pulseMultiplier,
						SetPulseMultiplier,
						1f,
						_pulseDuration * 0.5f
					)
				);
		}

		private void SetPulseMultiplier(float value)
		{
			_pulseMultiplier = value;
			ApplyScale();
		}

		private void ApplyScale()
		{
			transform.localScale = Vector3.one * (VolumeMath.Diameter(_radius) * _pulseMultiplier);
		}
	}
}
