using Services;
using Singletons;
using UnityEngine;

namespace Characters.Operations
{
	public class DropDarkQuartz : CharacterOperation
	{
		[SerializeField]
		private Transform _dropPosition;

		[SerializeField]
		private int _count;

		[SerializeField]
		private CustomFloat _amountRange = new CustomFloat(0f);

		public override void Run(Character owner)
		{
			Singleton<Service>.Instance.levelManager.DropDarkQuartz((int)_amountRange.value, _count, _dropPosition.position);
		}
	}
}
