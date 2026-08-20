using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class PlayerBall
	{
		private readonly GameConfig _config;
		private readonly float _initialVolume;
		private readonly float _criticalVolume;
		private float _remainingMass;
		private float _chargeElapsed;
		private float _chargeStartRadius;

		public float Radius { get; private set; }
		public float InitialRadius { get; }
		public float VolumeFraction => VolumeMath.Cube(Radius) / _initialVolume;
		public float ChargedShotVolumeFraction => VolumeMath.Cube(ChargedShotRadius) / _initialVolume;
		public bool IsCriticallySmall => VolumeMath.Cube(Radius) <= _criticalVolume;
		public float ChargedShotRadius { get; private set; }

		public float MaxShotRadius => VolumeMath.CubeRoot(Math.Max(0f, _remainingMass - _criticalVolume));

		public PlayerBall(GameConfig config, float initialRadius)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));

			if (initialRadius <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(initialRadius));
			}

			InitialRadius = initialRadius;
			Radius = initialRadius;
			_initialVolume = VolumeMath.Cube(Radius);
			_criticalVolume = _initialVolume * _config.CriticalVolumeFraction;
			_remainingMass = _initialVolume;
		}

		public void BeginCharge()
		{
			_chargeElapsed = 0f;
			float chargedVolume = GetMinimumShotVolume();
			ChargedShotRadius = VolumeMath.CubeRoot(chargedVolume);
			Radius = VolumeMath.CubeRoot(_remainingMass - chargedVolume);
			_chargeStartRadius = Radius;
		}

		public void Charge(float deltaTime, out bool overcharged)
		{
			if (deltaTime <= 0f)
			{
				overcharged = false;
				return;
			}

			_chargeElapsed += deltaTime;
			float chargeProgress = Math.Min(1f, _chargeElapsed / _config.MaxChargeDuration);

			Radius = _chargeStartRadius +
				(VolumeMath.CubeRoot(_criticalVolume) - _chargeStartRadius) * chargeProgress;
			float chargedVolume = _remainingMass - VolumeMath.Cube(Radius);
			ChargedShotRadius = VolumeMath.CubeRoot(Math.Max(0f, chargedVolume));
			overcharged = chargeProgress >= 1f;
		}

		public float ConsumeShot()
		{
			float fired = ChargedShotRadius;
			_remainingMass = VolumeMath.Cube(Radius);
			ChargedShotRadius = 0f;
			_chargeElapsed = 0f;
			return fired;
		}

		public void CancelCharge()
		{
			ChargedShotRadius = 0f;
			_chargeElapsed = 0f;
			Radius = VolumeMath.CubeRoot(_remainingMass);
		}

		private float GetMinimumShotVolume()
		{
			return Math.Min(
				VolumeMath.Cube(_config.MinShotRadius),
				Math.Max(0f, _remainingMass - _criticalVolume)
			);
		}
	}
}
