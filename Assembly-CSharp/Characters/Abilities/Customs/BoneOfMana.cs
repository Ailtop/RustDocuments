using System;
using Characters.Actions;
using Characters.Gear.Weapons;
using Characters.Operations;
using Characters.Player;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class BoneOfMana : Ability
	{
		public class Instance : AbilityInstance<BoneOfMana>
		{
			private WeaponInventory _weaponInventory;

			private int _balanceHeadCount;

			public override int iconStacks => _balanceHeadCount;

			public Instance(Character owner, BoneOfMana ability)
				: base(owner, ability)
			{
				_weaponInventory = owner.playerComponents.inventory.weapon;
				UpdateBalanaceHeadCount();
			}

			protected override void OnAttach()
			{
				owner.onStartAction += OnOwnerStartAction;
				_weaponInventory.onChanged += OnWeaponChanged;
			}

			protected override void OnDetach()
			{
				owner.onStartAction -= OnOwnerStartAction;
				_weaponInventory.onChanged -= OnWeaponChanged;
			}

			private void OnOwnerStartAction(Characters.Actions.Action action)
			{
				if (ability._actionTypeFilter.GetOrDefault(action.type) && !action.cooldown.usedByStreak)
				{
					CharacterOperation[] array = ability.operationsByCount[_balanceHeadCount];
					for (int i = 0; i < array.Length; i++)
					{
						array[i].Run(owner);
					}
				}
			}

			private void OnWeaponChanged(Weapon old, Weapon @new)
			{
				UpdateBalanaceHeadCount();
			}

			private void UpdateBalanaceHeadCount()
			{
				_balanceHeadCount = _weaponInventory.GetCountByCategory(Weapon.Category.Balance);
			}
		}

		[NonSerialized]
		public CharacterOperation[][] operationsByCount;

		[SerializeField]
		private ActionTypeBoolArray _actionTypeFilter;

		public override void Initialize()
		{
			base.Initialize();
			CharacterOperation[][] array = operationsByCount;
			foreach (CharacterOperation[] array2 in array)
			{
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].Initialize();
				}
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
