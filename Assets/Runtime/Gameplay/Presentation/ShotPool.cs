using UnityEngine;
using UnityEngine.Pool;

namespace DenisPavlenko.Game
{
	public sealed class ShotPool : MonoBehaviour
	{
		private const int PoolCapacity = 1;

		[SerializeField] private ShotView _prefab;

		private ObjectPool<ShotView> _pool;

		private void Awake()
		{
			_pool = new ObjectPool<ShotView>(Create, null, OnRelease, DestroyPooled,
				collectionCheck: false, defaultCapacity: PoolCapacity, maxSize: PoolCapacity);
		}

		private void OnDestroy() => _pool?.Clear();

		public ShotView Get() => _pool.Get();

		public void Release(ShotView shot) => _pool.Release(shot);

		private ShotView Create()
		{
			ShotView shot = Instantiate(_prefab, transform);
			shot.Hide();
			return shot;
		}

		private static void OnRelease(ShotView shot) => shot.Hide();

		private static void DestroyPooled(ShotView shot) => Destroy(shot.gameObject);
	}
}
