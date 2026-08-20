using UnityEngine;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class ObstacleView : MonoBehaviour
	{
		[SerializeField] private Color _infectedColor = new(1f, 0.35f, 0.08f);
		[SerializeField, Min(0.01f)] private float _animationDuration = 0.2f;

		private Material[] _materials;
		private bool _exploding;

		public float PositionZ => transform.position.z;
		public float CenterX => transform.position.x;
		public float HalfWidth => transform.localScale.x * 0.5f;
		public float HalfDepth => transform.localScale.z * 0.5f;

		private void Awake()
		{
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			_materials = new Material[renderers.Length];
			for (int index = 0; index < renderers.Length; index++)
			{
				_materials[index] = renderers[index].material;
			}
		}

		public void Explode(float delay)
		{
			if (_exploding)
			{
				return;
			}

			_exploding = true;
			Tween.DelayedCall(delay, PlayExplosion);
		}

		private void PlayExplosion()
		{
			foreach (Material material in _materials)
			{
				Tween.To(material, () => material.color, value => material.color = value,
					_infectedColor, _animationDuration).SetEase(EaseType.OutQuad);
			}

			transform.ScaleTo(Vector3.zero, _animationDuration)
				.SetEase(EaseType.InBack)
				.OnComplete(() => gameObject.SetActive(false));
		}
	}
}
