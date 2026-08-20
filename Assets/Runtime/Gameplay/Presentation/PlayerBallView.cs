using UnityEngine;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class PlayerBallView : MonoBehaviour
	{
		[SerializeField, Min(0f)] private float _advanceHopHeight = 0.6f;
		[SerializeField, Min(0f)] private float _advanceHopCyclesPerSecond = 14f;
		[SerializeField, Min(1f)] private float _pulseScale = 1.08f;
		[SerializeField, Min(0.01f)] private float _pulseDuration = 0.18f;

		private float _radius;

		public void Set(float radius, float z, bool isMoving)
		{
			_radius = radius;
			float hop = isMoving
				? Mathf.Abs(Mathf.Sin(Time.time * _advanceHopCyclesPerSecond)) * _advanceHopHeight
				: 0f;
			transform.localScale = Vector3.one * (radius * 2f);
			transform.position = new Vector3(0f, radius + hop, z);
		}

		public void Pulse()
		{
			Vector3 normalScale = Vector3.one * (_radius * 2f);
			transform.ScaleTo(normalScale * _pulseScale, _pulseDuration * 0.5f)
				.SetEase(EaseType.OutQuad)
				.OnComplete(() => transform.ScaleTo(normalScale, _pulseDuration * 0.5f));
		}
	}
}
