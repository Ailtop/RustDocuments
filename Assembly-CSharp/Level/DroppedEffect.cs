using UnityEngine;

namespace Level
{
	public class DroppedEffect : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] _spawned;

		public void Spawn()
		{
			for (int i = 0; i < _spawned.Length; i++)
			{
				_spawned[i].SetActive(true);
			}
		}

		public void Despawn()
		{
			for (int i = 0; i < _spawned.Length; i++)
			{
				_spawned[i].SetActive(false);
			}
		}
	}
}
