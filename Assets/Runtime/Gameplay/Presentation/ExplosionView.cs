using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class ExplosionView : MonoBehaviour
	{
		[SerializeField] private ParticleSystem _particles;
		[SerializeField, Min(0f)] private float _height = 0.25f;

		public void Play(float radius, float z)
		{
			transform.position = new Vector3(0f, _height, z);
			transform.localScale = Vector3.one * radius;

			_particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
			_particles.Play(true);
		}
	}
}
