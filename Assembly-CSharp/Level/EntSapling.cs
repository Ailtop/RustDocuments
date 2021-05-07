using UnityEngine;

namespace Level
{
	public class EntSapling : MonoBehaviour
	{
		[SerializeField]
		private PoolObject _pool;

		[SerializeField]
		private GameObject _introInvoker;

		public void Preload(int count)
		{
			_pool.Preload(count);
		}

		public void RunIntro(bool activate)
		{
			_introInvoker.SetActive(activate);
		}

		public PoolObject Spawn(Vector3 position, bool intro)
		{
			PoolObject poolObject = _pool.Spawn(position, Quaternion.identity);
			poolObject.GetComponent<EntSapling>().RunIntro(intro);
			return poolObject;
		}

		public void Despawn()
		{
			_introInvoker.SetActive(false);
			_pool.Despawn();
		}
	}
}
