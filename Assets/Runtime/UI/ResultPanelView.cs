using System;
using UnityEngine;
using UnityEngine.UI;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game.UI
{
	public sealed class ResultPanelView : MonoBehaviour
	{

		[SerializeField] private Button _restartButton;
		[SerializeField, Min(0.01f)] private float _showDuration = 0.28f;

		private void Awake()
		{
			_restartButton.onClick.AddListener(OnRestartClicked);
		}

		private void OnDestroy()
		{
			_restartButton.onClick.RemoveListener(OnRestartClicked);
		}

		public event Action RestartRequested;

		public void Show()
		{
			gameObject.SetActive(true);
			transform.localScale = Vector3.zero;
			transform.ScaleTo(Vector3.one, _showDuration).SetEase(EaseType.OutBack);
		}

		private void OnRestartClicked()
		{
			RestartRequested?.Invoke();
		}
	}
}
