using System;
using DenisPavlenko.Game.Core;

namespace DenisPavlenko.Game
{
	public sealed class ShotPresenter
	{
		private readonly ShotPool _pool;
		private ShotView _shot;

		public ShotPresenter(ShotPool pool)
		{
			_pool = pool ?? throw new ArgumentNullException(nameof(pool));
		}

		public void Update(GameSession session)
		{
			if (session.Phase == GamePhase.Charging)
			{
				GetShot().Show(session.Ball.ChargedShotRadius, session.PlayerZ);
			}
			else if (session.ShotActive)
			{
				GetShot().Set(session.ShotRadius, session.ShotZ);
			}
			else
			{
				Release();
			}
		}

		public void ShowFired(float radius, float playerZ) => GetShot().Show(radius, playerZ);

		public void Release()
		{
			if (_shot == null)
			{
				return;
			}

			_pool.Release(_shot);
			_shot = null;
		}

		private ShotView GetShot() => _shot ??= _pool.Get();
	}
}