using System;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnSwap : Trigger
	{
		[SerializeField]
		private WeaponTypeBoolArray _types;

		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			_character.playerComponents.inventory.weapon.onSwap += Check;
		}

		public override void Detach()
		{
			_character.playerComponents.inventory.weapon.onSwap -= Check;
		}

		private void Check()
		{
			if (_types[_character.playerComponents.inventory.weapon.polymorphOrCurrent.category])
			{
				Invoke();
			}
		}
	}
}
