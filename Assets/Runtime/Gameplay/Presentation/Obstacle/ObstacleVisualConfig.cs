using UnityEngine;

namespace DenisPavlenko.Game
{
	[CreateAssetMenu(menuName = "Denis Pavlenko/Obstacle Visual Config", fileName = "ObstacleVisualConfig")]
	public sealed class ObstacleVisualConfig : ScriptableObject
	{
		[SerializeField] private Color _infectedColor = new Color(1f, 0.35f, 0.08f);
		[SerializeField, Min(0.01f)] private float _animationDuration = 0.2f;

		public Color InfectedColor => _infectedColor;
		public float AnimationDuration => _animationDuration;
	}
}