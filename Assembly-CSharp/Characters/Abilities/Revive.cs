using System;
using Characters.Operations;
using FX.SpriteEffects;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class Revive : Ability
	{
		public class Instance : AbilityInstance<Revive>
		{
			public Instance(Character owner, Revive ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.health.onDie += ReviveOwner;
			}

			protected override void OnDetach()
			{
				owner.health.onDie -= ReviveOwner;
			}

			private void ReviveOwner()
			{
				owner.health.onDie -= ReviveOwner;
				Chronometer.global.AttachTimeScale(this, 0.2f, 0.5f);
				owner.health.Heal(ability._heal);
				Resource.instance.reassembleParticle.Emit(owner.transform.position, owner.collider.bounds, owner.movement.push);
				owner.CancelAction();
				owner.chronometer.master.AttachTimeScale(this, 0.01f, 0.5f);
				owner.spriteEffectStack.Add(new ColorBlend(int.MaxValue, Color.clear, 0.5f));
				GetInvulnerable getInvulnerable = new GetInvulnerable();
				getInvulnerable.duration = 3f;
				owner.spriteEffectStack.Add(new Invulnerable(0, 0.2f, getInvulnerable.duration));
				owner.ability.Add(getInvulnerable);
				owner.ability.Add(ability._ability.ability);
				ability._operations.Run(owner);
			}
		}

		[SerializeField]
		private int _heal = 30;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _ability;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

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
