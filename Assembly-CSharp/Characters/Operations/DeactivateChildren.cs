using UnityEngine;

namespace Characters.Operations
{
	public class DeactivateChildren : CharacterOperation
	{
		[SerializeField]
		private ParentPool _parentPool;

		public override void Run(Character owner)
		{
			foreach (Transform item in _parentPool.currentEffectParent)
			{
				item.gameObject.SetActive(false);
			}
		}
	}
}
