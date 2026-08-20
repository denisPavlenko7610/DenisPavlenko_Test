using DenisPavlenko.Game.Core;
using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class TrackView : MonoBehaviour
	{
		[SerializeField] private Transform _road;

		public void SetWidth(float halfWidth)
		{
			Vector3 roadScale = _road.localScale;
			roadScale.x = VolumeMath.Diameter(halfWidth);
			_road.localScale = roadScale;
		}
	}
}
