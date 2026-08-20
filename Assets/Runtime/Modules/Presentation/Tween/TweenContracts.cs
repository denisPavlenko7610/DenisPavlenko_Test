namespace UnityTemplates.Tween
{
	public enum TweenUpdateType
	{
		Normal = 0,
		Late = 1,
		Fixed = 2
	}

	public enum TweenLoopType
	{
		Restart = 0,
		Yoyo = 1
	}

	public enum TweenState
	{
		Idle = 0,
		Playing = 1,
		Paused = 2,
		Completed = 3,
		Killed = 4
	}

	public enum EaseType
	{
		Linear = 0,

		InQuad = 1,
		OutQuad = 2,
		InOutQuad = 3,

		InCubic = 4,
		OutCubic = 5,
		InOutCubic = 6,

		InQuart = 7,
		OutQuart = 8,
		InOutQuart = 9,

		InQuint = 10,
		OutQuint = 11,
		InOutQuint = 12,

		InSine = 13,
		OutSine = 14,
		InOutSine = 15,

		InExpo = 16,
		OutExpo = 17,
		InOutExpo = 18,

		InCirc = 19,
		OutCirc = 20,
		InOutCirc = 21,

		InBack = 22,
		OutBack = 23,
		InOutBack = 24,

		InBounce = 25,
		OutBounce = 26,
		InOutBounce = 27,

		InElastic = 28,
		OutElastic = 29,
		InOutElastic = 30
	}
}
