using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AttachAbilityToTargetOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<AttachAbilityToTargetOnGaveDamage>
		{
			public Instance(Character owner, AttachAbilityToTargetOnGaveDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
			}

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (!(target.character == null) && !target.character.health.dead && (string.IsNullOrWhiteSpace(ability._attackKey) || gaveDamage.key.Equals(ability._attackKey, StringComparison.OrdinalIgnoreCase)) && ability._motionTypeFilter[gaveDamage.motionType] && ability._attackTypeFilter[gaveDamage.attackType])
				{
					target.character.ability.Add(ability._abilityComponent.ability);
				}
			}
		}

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		[SerializeField]
		private string _attackKey;

		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		public override void Initialize()
		{
			base.Initialize();
			_abilityComponent.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
