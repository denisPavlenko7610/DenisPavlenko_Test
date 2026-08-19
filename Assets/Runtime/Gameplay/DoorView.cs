using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class DoorView : MonoBehaviour
	{
		[SerializeField] private Transform _leftPanel;
		[SerializeField] private Transform _rightPanel;
		[SerializeField] private float _openDelta = 2f;

		private Vector3 _leftStart;
		private Vector3 _rightStart;

		private void Awake()
		{
			_leftStart = _leftPanel.localPosition;
			_rightStart = _rightPanel.localPosition;
		}

		public void SetProgress(float t)
		{
			float delta = Mathf.Clamp01(t) * _openDelta;
			_leftPanel.localPosition = _leftStart + Vector3.left * delta;
			_rightPanel.localPosition = _rightStart + Vector3.right * delta;
		}
	}
}