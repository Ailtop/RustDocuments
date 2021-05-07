using Characters.Gear.Weapons;
using UnityEngine;

namespace Characters.Abilities
{
	public class HeadCategoryAttacher : AbilityAttacher
	{
		[SerializeField]
		private Weapon.Category _category1;

		[SerializeField]
		private Weapon.Category _category2;

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
			base.owner.playerComponents.inventory.weapon.onChanged += Check;
			Check(null, null);
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.playerComponents.inventory.weapon.onChanged -= Check;
				base.owner.ability.Remove(_abilityComponent.ability);
			}
		}

		private void Check(Weapon old, Weapon @new)
		{
			_003C_003Ec__DisplayClass7_0 _003C_003Ec__DisplayClass7_ = default(_003C_003Ec__DisplayClass7_0);
			_003C_003Ec__DisplayClass7_.categoryCounts = new EnumArray<Weapon.Category, int>();
			_003C_003Ec__DisplayClass7_.categoryCounts[_category1]++;
			_003C_003Ec__DisplayClass7_.categoryCounts[_category2]++;
			Weapon[] weapons = base.owner.playerComponents.inventory.weapon.weapons;
			foreach (Weapon weapon in weapons)
			{
				if (!(weapon == null))
				{
					_003C_003Ec__DisplayClass7_.categoryCounts[weapon.category]--;
				}
			}
			if (_003CCheck_003Eg__CanAttach_007C7_0(ref _003C_003Ec__DisplayClass7_))
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
