using DenisPavlenko.Game.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace DenisPavlenko.Game
{
	public sealed class PlayerBallView : MonoBehaviour
	{
		[FormerlySerializedAs("_hopHeight"), SerializeField, Min(0f)]
			private float _advanceHopHeight = 0.6f;
		[FormerlySerializedAs("_hopFrequency"), SerializeField, Min(0f)]
			private float _advanceHopCyclesPerSecond = 14f;
		private float _radius;

		public void Set(float radius, float z, bool isMoving)
		{
			_radius = radius;
			float hop = isMoving
				? Mathf.Abs(Mathf.Sin(Time.time * _advanceHopCyclesPerSecond)) * _advanceHopHeight
				: 0f;
			ApplyScale();
			transform.position = new Vector3(0f, radius + hop, z);
		}

		private void ApplyScale()
		{
			transform.localScale = Vector3.one * VolumeMath.Diameter(_radius);
		}
	}
}
