using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class PlayerBall
	{
		private readonly GameConfig _config;
		private readonly float _initialVolume;
		private readonly float _criticalVolume;
		private float _remainingMass;
		private float _radius;

		public PlayerBall(GameConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_radius = config.InitialBallRadius;
			_initialVolume = VolumeMath.Cube(_radius);
			_criticalVolume = VolumeMath.Cube(config.CriticalRadius);
			_remainingMass = _initialVolume;
		}

		public float Radius => _radius;
		public float VolumeFraction => VolumeMath.Cube(_radius) / _initialVolume;
		public float ChargedShotVolumeFraction => VolumeMath.Cube(ChargedShotRadius) / _initialVolume;
		public bool IsCriticallySmall => _radius <= _config.CriticalRadius + float.Epsilon;
		public float ChargedShotRadius { get; private set; }

		public float MaxShotRadius => VolumeMath.CubeRoot(Math.Max(0f, _remainingMass - _criticalVolume));

		public void Charge(float deltaTime, out bool overcharged)
		{
			if (deltaTime <= 0f)
			{
				overcharged = false;
				return;
			}

			float nextShot = ChargedShotRadius + _config.ShotGrowRate * deltaTime;
			float nextBall = VolumeMath.CubeRoot(Math.Max(0f, _remainingMass - VolumeMath.Cube(nextShot)));

			if (nextBall < _config.CriticalRadius)
			{
				overcharged = true;
				return;
			}

			overcharged = false;
			ChargedShotRadius = nextShot;
			_radius = nextBall;
		}

		public float ConsumeShot()
		{
			float fired = ChargedShotRadius;
			_remainingMass = VolumeMath.Cube(_radius);
			ChargedShotRadius = 0f;
			return fired;
		}

		public void CancelCharge()
		{
			ChargedShotRadius = 0f;
			_radius = VolumeMath.CubeRoot(_remainingMass);
		}
	}
}