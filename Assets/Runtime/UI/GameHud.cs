using System;
using UnityEngine;
using UnityEngine.UI;

namespace DenisPavlenko.Game.UI
{
	public sealed class GameHud : MonoBehaviour
	{
		private const string BallLabel = "Ball: {0}%";
		private const string VictoryText = "Victory!";
		private const string DefeatText = "Defeat";
		private static readonly Color WinColor = new(0.2f, 0.8f, 0.3f, 1f);
		private static readonly Color LoseColor = new(0.9f, 0.25f, 0.25f, 1f);

		public event Action RestartRequested;

		[SerializeField] private Text _ballText;
		[SerializeField] private GameObject _resultPanel;
		[SerializeField] private Text _resultText;
		[SerializeField] private Button _restartButton;

		private void Awake()
		{
			_restartButton.onClick.AddListener(() => RestartRequested?.Invoke());
		}

		public void SetBallFraction(float fraction)
		{
			_ballText.text = string.Format(BallLabel, Mathf.RoundToInt(fraction * 100f));
		}

		public void ShowResult(bool isWin)
		{
			_resultText.text = isWin ? VictoryText : DefeatText;
			_resultText.color = isWin ? WinColor : LoseColor;
			_resultPanel.SetActive(true);
		}
	}
}