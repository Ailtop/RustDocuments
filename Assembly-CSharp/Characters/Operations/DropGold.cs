using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class DropGold : CharacterOperation
	{
		[SerializeField]
		private Transform _dropPosition;

		[SerializeField]
		private int _amount;

		[SerializeField]
		private int _count;

		public override void Run(Character owner)
		{
			Singleton<Service>.Instance.levelManager.DropGold(_amount, _count, _dropPosition.position);
		}
	}
}
