using UnityEngine;
using UnityEngine.InputSystem;

namespace DenisPavlenko.Game
{
	public sealed class ShootInput
	{
		public bool IsPressed { get; private set; }

		public bool PressedThisFrame { get; private set; }

		public void Poll()
		{
			PressedThisFrame = false;

			if (Pointer.current == null)
			{
				IsPressed = false;
				return;
			}

			bool down = Pointer.current.press.isPressed;

			if (down && !IsPressed)
			{
				PressedThisFrame = true;
			}
			IsPressed = down;
		}
	}
}
