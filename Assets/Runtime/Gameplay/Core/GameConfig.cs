namespace DenisPavlenko.Game.Core
{
	public sealed class GameConfig
	{
		public static GameConfig CreateDefault() => new();

		public float InitialBallRadius { get; set; } = 1f;
		public float CriticalBallFraction { get; set; } = 0.18f;
		public float ShotGrowRate { get; set; } = 1.2f;
		public float ShotSpeed { get; set; } = 14f;
		public float BlastRadiusMultiplier { get; set; } = 2.6f;
		public float InfectionRadius { get; set; } = 2.2f;
		public float TrackWidthFactor { get; set; } = 2.6f;
		public float AdvanceSpeed { get; set; } = 9f;
		public float DoorOpenDistance { get; set; } = 5f;
		public float StopBeforeObstacleDistance { get; set; } = 1.2f;
		public float BallHopHeight { get; set; } = 0.6f;
		public float BallHopFrequency { get; set; } = 14f;
		public float MinShotRadius { get; set; } = 0.15f;
		public float CameraSideOffset { get; set; } = 12f;
		public float CameraHeight { get; set; } = 9f;
		public float CameraBackOffset { get; set; } = 4f;
		public float CameraLookAhead { get; set; } = 25f;
		public float CameraLookHeight { get; set; } = 1.2f;

		public float CriticalRadius => InitialBallRadius * CriticalBallFraction;
	}
}