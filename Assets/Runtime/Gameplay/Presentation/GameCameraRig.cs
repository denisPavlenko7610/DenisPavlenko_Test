using System;
using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class GameCameraRig
	{
		private readonly Camera _camera;
		private readonly Transform _start;
		private readonly Transform _lookAt;

		public GameCameraRig(Camera camera, Transform start, Transform lookAt)
		{
			_camera = camera ?? throw new ArgumentNullException(nameof(camera));
			_start = start ?? throw new ArgumentNullException(nameof(start));
			_lookAt = lookAt ?? throw new ArgumentNullException(nameof(lookAt));
		}

		public void Present(float playerZ)
		{
			Vector3 offset = Vector3.forward * playerZ;
			Vector3 position = _start.position + offset;
			Vector3 lookPoint = _lookAt.position + offset;
			_camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookPoint - position));
		}
	}
}