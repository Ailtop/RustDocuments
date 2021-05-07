using UnityEngine;

namespace Characters
{
	public class SkulHeadToTeleport : MonoBehaviour
	{
		[SerializeField]
		private PoolObject _poolObject;

		public static SkulHeadToTeleport instance { get; private set; }

		private void OnEnable()
		{
			instance = this;
		}

		public void Despawn()
		{
			_poolObject.Despawn();
		}
	}
}
