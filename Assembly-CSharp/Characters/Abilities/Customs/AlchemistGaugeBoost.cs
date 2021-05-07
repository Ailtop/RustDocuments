using System;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class AlchemistGaugeBoost : Ability
	{
		public class Instance : AbilityInstance<AlchemistGaugeBoost>
		{
			public Instance(Character owner, AlchemistGaugeBoost ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
			}

			protected override void OnDetach()
			{
			}
		}

		private Instance _instance;

		[SerializeField]
		private int _mutiplier;

		public bool attached => _instance?.attached ?? false;

		public int multiplier => _mutiplier;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return _instance = new Instance(owner, this);
		}
	}
}
