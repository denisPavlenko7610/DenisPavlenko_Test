using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class PlayerBall
	{
		private readonly GameConfig _config;
		private readonly float _initialRadius;
		private readonly float _initialVolume;
		private readonly float _criticalVolume;
		private float _remainingMass;
		private float _radius;
		private float _chargeElapsed;

		public PlayerBall(GameConfig config, float initialRadius)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));

			if (initialRadius <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(initialRadius));
			}

			_initialRadius = initialRadius;
			_radius = initialRadius;
			_initialVolume = VolumeMath.Cube(_radius);
			_criticalVolume = _initialVolume * _config.CriticalVolumeFraction;
			_remainingMass = _initialVolume;
		}

		public float Radius => _radius;
		public float InitialRadius => _initialRadius;
		public float VolumeFraction => VolumeMath.Cube(_radius) / _initialVolume;
		public float ChargedShotVolumeFraction => VolumeMath.Cube(ChargedShotRadius) / _initialVolume;
		public bool IsCriticallySmall => VolumeMath.Cube(_radius) <= _criticalVolume;
		public float ChargedShotRadius { get; private set; }

		public float MaxShotRadius => VolumeMath.CubeRoot(Math.Max(0f, _remainingMass - _criticalVolume));

		public void BeginCharge()
		{
			_chargeElapsed = 0f;
			ChargedShotRadius = 0f;
			_radius = VolumeMath.CubeRoot(_remainingMass);
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

			ChargedShotRadius = MaxShotRadius * chargeProgress;
			float chargedVolume = VolumeMath.Cube(ChargedShotRadius);
			_radius = VolumeMath.CubeRoot(Math.Max(_criticalVolume, _remainingMass - chargedVolume));
			overcharged = chargeProgress >= 1f;
		}

		public float ConsumeShot()
		{
			float fired = ChargedShotRadius;
			_remainingMass = VolumeMath.Cube(_radius);
			ChargedShotRadius = 0f;
			_chargeElapsed = 0f;
			return fired;
		}

		public void CancelCharge()
		{
			ChargedShotRadius = 0f;
			_chargeElapsed = 0f;
			_radius = VolumeMath.CubeRoot(_remainingMass);
		}
	}
}
