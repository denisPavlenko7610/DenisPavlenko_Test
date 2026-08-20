using System;
using DenisPavlenko.Game.Core;

namespace DenisPavlenko.Game
{
	public sealed class GameplayDriver
	{
		private readonly ShootInput _input = new();
		private readonly ShotPresenter _shotPresenter;
		private readonly GameSession _session;

		public GameplayDriver(GameSession session, ShotPresenter shotPresenter)
		{
			_session = session ?? throw new ArgumentNullException(nameof(session));
			_shotPresenter = shotPresenter ?? throw new ArgumentNullException(nameof(shotPresenter));
		}

		public void Tick(float deltaTime)
		{
			_input.Poll();
			UpdateGameplay(deltaTime);
			_shotPresenter.Update(_session);
		}

		public void ShowFired(float radius, float playerZ) => _shotPresenter.ShowFired(radius, playerZ);

		public void HideShot() => _shotPresenter.Release();

		private void UpdateGameplay(float deltaTime)
		{
			switch (_session.Phase)
			{
				case GamePhase.Idle when _input.PressedThisFrame:
					_session.BeginCharge();
					break;

				case GamePhase.Charging when _input.IsPressed:
					_session.TickCharge(deltaTime);
					break;

				case GamePhase.Charging:
					if (!_session.ReleaseShot())
					{
						_shotPresenter.Release();
					}
					break;

				default:
					_session.Tick(deltaTime);
					break;
			}
		}
	}
}