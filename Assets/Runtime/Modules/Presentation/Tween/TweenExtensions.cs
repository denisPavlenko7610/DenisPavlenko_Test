using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTemplates.Tween
{
	public static class TweenExtensions
	{
		// --------------------------------------------------
		// Transform
		// --------------------------------------------------

		public static PropertyTween<Vector3> MoveTo(this Transform transform, Vector3 target, float duration)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.position,
				value => transform.position = value,
				target,
				duration
			);
		}

		public static PropertyTween<Vector3> LocalMoveTo(this Transform transform, Vector3 target, float duration)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.localPosition,
				value =>
					transform.localPosition =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<Vector3> ScaleTo(this Transform transform, Vector3 target, float duration)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.localScale,
				value =>
					transform.localScale =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<Quaternion> RotateTo(this Transform transform, Quaternion target, float duration)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.rotation,
				value =>
					transform.rotation =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<Quaternion> RotateTo(
			this Transform transform,
			Vector3 targetEulerAngles,
			float duration
		)
		{
			return transform.RotateTo(
				Quaternion.Euler(targetEulerAngles),
				duration
			);
		}

		public static PropertyTween<Quaternion> LocalRotateTo(
			this Transform transform,
			Quaternion target,
			float duration
		)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.localRotation,
				value =>
					transform.localRotation =
						value,
				target,
				duration
			);
		}

		// --------------------------------------------------
		// RectTransform
		// --------------------------------------------------

		public static PropertyTween<Vector2> AnchoredPositionTo(
			this RectTransform transform,
			Vector2 target,
			float duration
		)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.anchoredPosition,
				value =>
					transform.anchoredPosition =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<Vector2> SizeDeltaTo(this RectTransform transform, Vector2 target, float duration)
		{
			Require(transform);

			return Tween.To(
				transform,
				() => transform.sizeDelta,
				value =>
					transform.sizeDelta =
						value,
				target,
				duration
			);
		}

		// --------------------------------------------------
		// CanvasGroup
		// --------------------------------------------------

		public static PropertyTween<float> FadeTo(this CanvasGroup canvasGroup, float target, float duration)
		{
			Require(canvasGroup);

			return Tween.To(
				canvasGroup,
				() => canvasGroup.alpha,
				value =>
					canvasGroup.alpha =
						value,
				target,
				duration
			);
		}

		// --------------------------------------------------
		// Graphic
		// --------------------------------------------------

		public static PropertyTween<Color> ColorTo(this Graphic graphic, Color target, float duration)
		{
			Require(graphic);

			return Tween.To(
				graphic,
				() => graphic.color,
				value =>
					graphic.color =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<float> FadeTo(this Graphic graphic, float target, float duration)
		{
			Require(graphic);

			return Tween.To(
				graphic,
				() => graphic.color.a,
				value =>
				{
					Color color =
						graphic.color;

					color.a =
						value;

					graphic.color =
						color;
				},
				target,
				duration
			);
		}

		// --------------------------------------------------
		// Image
		// --------------------------------------------------

		public static PropertyTween<float> FillAmountTo(this Image image, float target, float duration)
		{
			Require(image);

			return Tween.To(
				image,
				() => image.fillAmount,
				value =>
					image.fillAmount =
						value,
				target,
				duration
			);
		}

		// --------------------------------------------------
		// SpriteRenderer
		// --------------------------------------------------

		public static PropertyTween<Color> ColorTo(this SpriteRenderer renderer, Color target, float duration)
		{
			Require(renderer);

			return Tween.To(
				renderer,
				() => renderer.color,
				value =>
					renderer.color =
						value,
				target,
				duration
			);
		}

		public static PropertyTween<float> FadeTo(this SpriteRenderer renderer, float target, float duration)
		{
			Require(renderer);

			return Tween.To(
				renderer,
				() => renderer.color.a,
				value =>
				{
					Color color =
						renderer.color;

					color.a =
						value;

					renderer.color =
						color;
				},
				target,
				duration
			);
		}

		// --------------------------------------------------
		// Camera
		// --------------------------------------------------

		public static PropertyTween<Color> BackgroundColorTo(this Camera camera, Color target, float duration)
		{
			Require(camera);

			return Tween.To(
				camera,
				() => camera.backgroundColor,
				value =>
					camera.backgroundColor =
						value,
				target,
				duration
			);
		}

		private static void Require(UnityEngine.Object target)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}
		}
	}
}
