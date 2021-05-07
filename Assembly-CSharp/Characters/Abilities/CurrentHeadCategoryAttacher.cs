using Characters.Gear.Weapons;
using UnityEngine;

namespace Characters.Abilities
{
	public class CurrentHeadCategoryAttacher : AbilityAttacher
	{
		[SerializeField]
		private Weapon.Category _category;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private bool _attached;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.playerComponents.inventory.weapon.onSwap += Check;
			base.owner.playerComponents.inventory.weapon.onChanged += OnWeaponChange;
			Check();
		}

		private void OnWeaponChange(Weapon old, Weapon @new)
		{
			Check();
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.playerComponents.inventory.weapon.onSwap -= Check;
				base.owner.playerComponents.inventory.weapon.onChanged -= OnWeaponChange;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		private void Check()
		{
			if (base.owner.playerComponents.inventory.weapon.polymorphOrCurrent.category == _category)
			{
				Attach();
			}
			else
			{
				Detach();
			}
		}

		private void Attach()
		{
			if (!_attached)
			{
				_attached = true;
				base.owner.ability.Add(_abilityComponent.ability);
			}
		}

		private void Detach()
		{
			if (_attached)
			{
				_attached = false;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
