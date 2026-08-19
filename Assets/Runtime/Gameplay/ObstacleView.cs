using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class ObstacleView : MonoBehaviour
	{
		public float PositionZ => transform.position.z;

		public float CenterX => transform.position.x;

		public float HalfWidth => transform.localScale.x * 0.5f;
	}
}