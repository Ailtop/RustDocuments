using System;

namespace Characters.Abilities
{
	[Serializable]
	public class DummyAbility : Ability
	{
		public class Instance : AbilityInstance<DummyAbility>
		{
			public Instance(Character owner, DummyAbility ability)
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

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
