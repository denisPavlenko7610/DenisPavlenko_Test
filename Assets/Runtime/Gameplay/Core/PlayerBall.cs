using System;

namespace DenisPavlenko.Game.Core
{
	public sealed class PlayerBall
	{
		private readonly GameConfig _config;
		private readonly float _initialVolume;
		private float _remainingMass;
		private float _radius;

		public PlayerBall(GameConfig config)
		{
			_config = config ?? throw new ArgumentNullException(nameof(config));
			_radius = config.InitialBallRadius;
			_initialVolume = Cube(_radius);
			_remainingMass = _initialVolume;
		}

		public float Radius => _radius;
		public float VolumeFraction => Cube(_radius) / _initialVolume;
		public float ChargedShotVolumeFraction => Cube(ChargedShotRadius) / _initialVolume;
		public bool IsCriticallySmall => _radius <= _config.CriticalRadius + float.Epsilon;
		public float ChargedShotRadius { get; private set; }

		public float MaxShotRadius => (float)Math.Pow(Math.Max(0f, _remainingMass - _config.CriticalRadius * _config.CriticalRadius * _config.CriticalRadius), 1.0 / 3.0);

		public void Charge(float deltaTime, out bool overcharged)
		{
			if (deltaTime <= 0f)
			{
				overcharged = false;
				return;
			}

			float nextShot = ChargedShotRadius + _config.ShotGrowRate * deltaTime;
			float nextBall = (float)Math.Pow(Math.Max(0f, _remainingMass - nextShot * nextShot * nextShot), 1.0 / 3.0);

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
			_remainingMass = _radius * _radius * _radius;
			ChargedShotRadius = 0f;
			return fired;
		}

		public void CancelCharge()
		{
			ChargedShotRadius = 0f;
			_radius = (float)Math.Pow(_remainingMass, 1.0 / 3.0);
		}

		private static float Cube(float value) => value * value * value;
	}
}
