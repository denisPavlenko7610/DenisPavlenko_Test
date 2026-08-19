using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class PlayerBallView : MonoBehaviour
	{
		public void Set(float radius, float z, float hop)
		{
			transform.localScale = Vector3.one * radius * 2f;
			transform.position = new Vector3(0f, radius + hop, z);
		}
	}
}