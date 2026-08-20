using System;
using UnityEngine;
using UnityEngine.UI;

namespace DenisPavlenko.Game.UI
{
	public sealed class GameHud : MonoBehaviour
	{
		private const string PlayerLabel = "PLAYER  {0}";
		private const string ShotLabel = "SHOT  {0}";
		private const int WholePercent = 100;

		public event Action RestartRequested;

		[SerializeField] private Text _playerVolumeText;
		[SerializeField] private Text _shotVolumeText;
		[SerializeField] private ResultPanelView _winPanel;
		[SerializeField] private ResultPanelView _losePanel;

		private void Awake()
		{
			_winPanel.RestartRequested += OnRestartClicked;
			_losePanel.RestartRequested += OnRestartClicked;
		}

		private void OnDestroy()
		{
			if (_winPanel == null)
			{
				return;
			}

			_winPanel.RestartRequested -= OnRestartClicked;
			_losePanel.RestartRequested -= OnRestartClicked;
		}

		public void SetVolumes(float playerFraction, float shotFraction)
		{
			_playerVolumeText.text = string.Format(PlayerLabel, ToWholePercent(playerFraction));
			_shotVolumeText.text = string.Format(ShotLabel, ToWholePercent(shotFraction));
		}

		public void ShowResult(bool isWin) => (isWin ? _winPanel : _losePanel).Show();

		private static int ToWholePercent(float fraction) =>
			Mathf.Clamp(Mathf.RoundToInt(fraction * WholePercent), 0, WholePercent);

		private void OnRestartClicked() => RestartRequested?.Invoke();
	}
}