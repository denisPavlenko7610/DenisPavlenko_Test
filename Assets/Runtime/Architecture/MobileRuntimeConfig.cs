using UnityEngine;

namespace DenisPavlenko.Project.Architecture
{
	public sealed class MobileRuntimeConfig : MonoBehaviour
	{
		private const int TargetFrameRate = 60;

		private void Start()
		{
			Screen.sleepTimeout = SleepTimeout.NeverSleep;
			Application.targetFrameRate = TargetFrameRate;
		}
	}
}