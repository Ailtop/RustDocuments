using System;
using Characters.Gear.Weapons.Gauges;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class NonConsumption : Ability
	{
		public class Instance : AbilityInstance<NonConsumption>
		{
			public Instance(Character owner, NonConsumption ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				ability._magazine.nonConsumptionState = true;
			}

			protected override void OnDetach()
			{
				ability._magazine.nonConsumptionState = false;
			}
		}

		[SerializeField]
		private Magazine _magazine;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
