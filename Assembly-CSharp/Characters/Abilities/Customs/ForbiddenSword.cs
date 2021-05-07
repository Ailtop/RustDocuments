using System;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class ForbiddenSword : Ability
	{
		public class Instance : AbilityInstance<ForbiddenSword>
		{
			public override int iconStacks => ability.component.currentKillCount;

			public override float iconFillAmount => 0f;

			public Instance(Character owner, ForbiddenSword ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnOwnerKilled));
			}

			private void OnOwnerKilled(ITarget target, ref Damage damage)
			{
				if (!(target.character == null) && ability._characterTypeFilter[target.character.type])
				{
					ability.component.currentKillCount++;
					if (!((float)ability.component.currentKillCount < ability._killCount))
					{
						ability._operations.Run(owner);
					}
				}
			}
		}

		[SerializeField]
		private float _killCount = 666f;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter = new CharacterTypeBoolArray(true, true, true, true, true, false, false, false);

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

		public ForbiddenSwordComponent component { get; set; }

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
