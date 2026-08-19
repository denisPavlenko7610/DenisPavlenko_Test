using UnityEngine;

namespace DenisPavlenko.Game
{
	public sealed class TrackView : MonoBehaviour
	{
		private const float WallHalfWidth = 0.5f;

		[SerializeField] private GameObject _leftWall;
		[SerializeField] private GameObject _rightWall;

		public void Set(float halfWidth, float z)
		{
			float wallX = halfWidth + WallHalfWidth;
			_leftWall.transform.position = new Vector3(-wallX, _leftWall.transform.position.y, z);
			_rightWall.transform.position = new Vector3(wallX, _rightWall.transform.position.y, z);
		}
	}
}