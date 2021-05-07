using UnityEngine;

namespace Characters.Operations
{
	public class SwapWeapon : CharacterOperation
	{
		[SerializeField]
		private bool _force = true;

		public override void Run(Character owner)
		{
			owner.playerComponents.inventory.weapon.NextWeapon(_force);
		}
	}
}
