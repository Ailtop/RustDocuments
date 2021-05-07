using System;
using Characters.Gear.Items;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class ElderEntsGratitude : Ability
	{
		public class Instance : AbilityInstance<ElderEntsGratitude>
		{
			private Characters.Shield.Instance _shieldInstance;

			public override int iconStacks => (int)_shieldInstance.amount;

			public Instance(Character owner, ElderEntsGratitude ability)
				: base(owner, ability)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				_shieldInstance.amount = ability.component.shieldAmount;
			}

			private void OnShieldBroke()
			{
				RemoveItem();
				owner.ability.Remove(this);
			}

			protected override void OnAttach()
			{
				_shieldInstance = owner.health.shield.Add(ability, (float)ability.component.shieldAmount, OnShieldBroke);
				owner.stat.AttachValues(ability._stat);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(ability._stat);
				if (_shieldInstance != null && _shieldInstance.amount > 0.0)
				{
					ability.component.shieldAmount = _shieldInstance.amount;
				}
				if (owner.health.shield.Remove(ability))
				{
					_shieldInstance = null;
				}
			}

			private void RemoveItem()
			{
				ability._operationsOnChange.Run(owner);
				ability._elderEntsGratitudeItem.RemoveOnInventory();
			}
		}

		[SerializeField]
		private Item _elderEntsGratitudeItem;

		[Space]
		[SerializeField]
		private float _amount;

		[SerializeField]
		private Stat.Values _stat;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operationsOnChange;

		public ElderEntsGratitudeComponent component { get; set; }

		public float amount => _amount;

		public override void Initialize()
		{
			base.Initialize();
			_operationsOnChange.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
