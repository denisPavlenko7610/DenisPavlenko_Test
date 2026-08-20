using DenisPavlenko.Game.Core;
using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class ShotView : MonoBehaviour
	{
		[SerializeField, Min(0.01f)] private float _minimumVisibleRadius = 0.08f;

		public void Show(float radius, float z)
		{
			gameObject.SetActive(true);
			Set(Mathf.Max(radius, _minimumVisibleRadius), z);
		}

		public void Set(float radius, float z)
		{
			transform.localScale = Vector3.one * VolumeMath.Diameter(radius);
			transform.position = new Vector3(0f, radius, z);
		}

		public void Hide() => gameObject.SetActive(false);
	}
}
