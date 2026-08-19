using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class ShotView : MonoBehaviour
	{
		public void Show(float radius, float z)
		{
			gameObject.SetActive(true);
			Set(radius, z);
		}

		public void Set(float radius, float z)
		{
			transform.localScale = Vector3.one * radius * 2f;
			transform.position = new Vector3(0f, radius, z);
		}

		public void Hide() => gameObject.SetActive(false);
	}
}