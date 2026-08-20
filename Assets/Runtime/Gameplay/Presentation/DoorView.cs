using UnityEngine;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game
{
	public sealed class DoorView : MonoBehaviour
	{
		[SerializeField] private Transform _leftPanel;
		[SerializeField] private Transform _rightPanel;
		[SerializeField, Min(0f)] private float _activationDistance = 5f;
		[SerializeField, Min(0f)] private float _panelTravelDistance = 2.2f;
		[SerializeField, Min(0.01f)] private float _openDuration = 0.5f;

		private Vector3 _leftStart;
		private Vector3 _rightStart;
		private bool _opened;

		public float PositionZ => transform.position.z;

		private void Awake()
		{
			_leftStart = _leftPanel.localPosition;
			_rightStart = _rightPanel.localPosition;
		}

		public void OpenWhenNear(float playerZ)
		{
			if (_opened || PositionZ - playerZ > _activationDistance)
			{
				return;
			}

			_opened = true;
			_leftPanel.LocalMoveTo(_leftStart + Vector3.left * _panelTravelDistance, _openDuration).SetEase(EaseType.OutBack);
			_rightPanel.LocalMoveTo(_rightStart + Vector3.right * _panelTravelDistance, _openDuration).SetEase(EaseType.OutBack);
		}
	}
}
