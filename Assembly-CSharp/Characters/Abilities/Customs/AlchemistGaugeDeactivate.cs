using System;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class AlchemistGaugeDeactivate : Ability
	{
		public class Instance : AbilityInstance<AlchemistGaugeDeactivate>
		{
			public Instance(Character owner, AlchemistGaugeDeactivate ability)
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

		public bool attached => _instance?.attached ?? false;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return _instance = new Instance(owner, this);
		}
	}
}
