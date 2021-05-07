using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class SpawnGold : Operation
	{
		[SerializeField]
		private Transform _point;

		[SerializeField]
		private int _gold;

		[SerializeField]
		private int _count;

		public override void Run()
		{
			Vector3 position = ((_point == null) ? base.transform.position : _point.position);
			Singleton<Service>.Instance.levelManager.DropGold(_gold, _count, position);
		}
	}
}
