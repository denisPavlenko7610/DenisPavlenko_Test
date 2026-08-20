using System;
using UnityEngine;
using UnityEngine.UI;
using UnityTemplates.Tween;

namespace DenisPavlenko.Game.UI
{
	public sealed class GameHud : MonoBehaviour
	{
		private const string PlayerLabel = "PLAYER  {0}";
		private const string ShotLabel = "SHOT  {0}";

		public event Action RestartRequested;

		[SerializeField] private Text _playerVolumeText;
		[SerializeField] private Text _shotVolumeText;
		[SerializeField] private GameObject _winPanel;
		[SerializeField] private GameObject _losePanel;
		[SerializeField] private Button _winRestartButton;
		[SerializeField] private Button _loseRestartButton;
		[SerializeField, Min(0.01f)] private float _resultAnimationDuration = 0.28f;

		private void Awake()
		{
			_winPanel.SetActive(false);
			_losePanel.SetActive(false);
			_winRestartButton.onClick.AddListener(OnRestartClicked);
			_loseRestartButton.onClick.AddListener(OnRestartClicked);
		}

		private void OnDestroy()
		{
			_winRestartButton.onClick.RemoveListener(OnRestartClicked);
			_loseRestartButton.onClick.RemoveListener(OnRestartClicked);
		}

		public void SetVolumes(float playerFraction, float shotFraction)
		{
			_playerVolumeText.text = string.Format(PlayerLabel, ToWholeVolume(playerFraction));
			_shotVolumeText.text = string.Format(ShotLabel, ToWholeVolume(shotFraction));
		}

		public void ShowResult(bool isWin)
		{
			GameObject panelObject = isWin ? _winPanel : _losePanel;
			panelObject.SetActive(true);
			Transform panel = panelObject.transform;
			panel.localScale = Vector3.zero;
			panel.ScaleTo(Vector3.one, _resultAnimationDuration).SetEase(EaseType.OutBack);
		}

		private static int ToWholeVolume(float fraction) => Mathf.Clamp(Mathf.RoundToInt(fraction * 100f), 0, 100);

		private void OnRestartClicked() => RestartRequested?.Invoke();
	}
}
