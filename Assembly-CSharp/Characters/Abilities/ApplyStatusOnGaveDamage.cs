using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ApplyStatusOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<ApplyStatusOnGaveDamage>
		{
			internal Instance(Character owner, ApplyStatusOnGaveDamage ability)
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

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(target.character == null) && !(target.character == owner) && (!ability._onCritical || tookDamage.critical) && ability._attackTypes[tookDamage.motionType] && ability._types[tookDamage.attackType] && MMMaths.PercentChance(ability._chance))
				{
					owner.GiveStatus(target.character, ability._status);
				}
			}
		}

		[Serializable]
		private class AttackTypeBoolArray : EnumArray<Damage.MotionType, bool>
		{
		}

		[Serializable]
		private class DamageTypeBoolArray : EnumArray<Damage.AttackType, bool>
		{
		}

		[SerializeField]
		private CharacterStatus.ApplyInfo _status;

		[SerializeField]
		[Range(1f, 100f)]
		private int _chance = 100;

		[SerializeField]
		private bool _onCritical;

		[SerializeField]
		private AttackTypeBoolArray _attackTypes;

		[SerializeField]
		private DamageTypeBoolArray _types;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
