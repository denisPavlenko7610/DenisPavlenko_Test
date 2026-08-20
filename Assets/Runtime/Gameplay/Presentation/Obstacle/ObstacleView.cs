using System;
using UnityEngine;
using UnityTemplates.Attributes;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class ObstacleView : MonoBehaviour
	{
		private static readonly int TintPropertyId = Shader.PropertyToID("_BaseColor");

		[SerializeField, Assign(AssignMode.Children)] private Renderer _renderer;
		[SerializeField] private ObstacleVisualConfig _config;

		private MaterialPropertyBlock _propertyBlock;
		private Color _infectedColor;
		private float _animationDuration;
		private Color _tint = Color.white;
		private bool _exploding;

		public float PositionZ => _renderer.bounds.center.z;
		public float CenterX => _renderer.bounds.center.x;
		public float HalfWidth => _renderer.bounds.extents.x;
		public float HalfDepth => _renderer.bounds.extents.z;

		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
			_infectedColor = _config.InfectedColor;
			_animationDuration = _config.AnimationDuration;
		}

		public void Explode(float delay, Action onComplete)
		{
			if (_exploding)
			{
				return;
			}

			_exploding = true;
			Tween.DelayedCall(delay, () => PlayExplosion(onComplete));
		}

		private void PlayExplosion(Action onComplete)
		{
			Tween.To(this, () => _tint, ApplyTint, _infectedColor, _animationDuration)
				.SetEase(EaseType.OutQuad);

			transform.ScaleTo(Vector3.zero, _animationDuration)
				.SetEase(EaseType.InBack)
				.OnComplete(() => CompleteExplosion(onComplete));
		}

		private void CompleteExplosion(Action onComplete)
		{
			gameObject.SetActive(false);
			onComplete?.Invoke();
		}

		private void ApplyTint(Color tint)
		{
			_tint = tint;
			_propertyBlock.SetColor(TintPropertyId, tint);
			_renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}
