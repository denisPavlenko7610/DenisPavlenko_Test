using UnityEngine;
using UnityTemplates.Attributes;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class ObstacleView : MonoBehaviour
	{
		private const float DefaultAnimationDuration = 0.2f;
		private static readonly Color DefaultInfectedColor = new(1f, 0.35f, 0.08f);

		[SerializeField, Assign(AssignMode.Children)] private Renderer _renderer;
		[SerializeField] private ObstacleVisualConfig _config;

		private static readonly int TintPropertyId = Shader.PropertyToID("_BaseColor");

		private MaterialPropertyBlock _propertyBlock;
		private Color _infectedColor;
		private float _animationDuration;
		private Color _tint = Color.white;
		private bool _exploding;

		public float PositionZ => transform.position.z;
		public float CenterX => transform.position.x;
		public float HalfWidth => transform.localScale.x * 0.5f;
		public float HalfDepth => transform.localScale.z * 0.5f;

		private void Awake()
		{
			_propertyBlock = new MaterialPropertyBlock();
			_infectedColor = _config != null ? _config.InfectedColor : DefaultInfectedColor;
			_animationDuration = _config != null ? _config.AnimationDuration : DefaultAnimationDuration;
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
			Tween.To(this, () => _tint, ApplyTint, _infectedColor, _animationDuration)
				.SetEase(EaseType.OutQuad);

			transform.ScaleTo(Vector3.zero, _animationDuration)
				.SetEase(EaseType.InBack)
				.OnComplete(() => gameObject.SetActive(false));
		}

		private void ApplyTint(Color tint)
		{
			_tint = tint;
			_propertyBlock.SetColor(TintPropertyId, tint);
			_renderer.SetPropertyBlock(_propertyBlock);
		}
	}
}
