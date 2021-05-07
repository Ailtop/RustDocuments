using Characters.Gear.Items;
using UnityEngine;

namespace Characters.Abilities
{
	public class OwnItemAbilityAttacher : AbilityAttacher
	{
		[SerializeField]
		private Item _item;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		public override void OnIntialize()
		{
			_abilityComponent.Initialize();
		}

		public override void StartAttach()
		{
			base.owner.playerComponents.inventory.item.onChanged += OnItemInventoryChanged;
			OnItemInventoryChanged();
		}

		public override void StopAttach()
		{
			if (!(base.owner == null))
			{
				base.owner.playerComponents.inventory.item.onChanged -= OnItemInventoryChanged;
				Detach();
			}
		}

		private void OnItemInventoryChanged()
		{
			if (base.owner.playerComponents.inventory.item.Has(_item))
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
			base.owner.ability.Add(_abilityComponent.ability);
		}

		private void Detach()
		{
			base.owner.ability.Remove(_abilityComponent.ability);
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
