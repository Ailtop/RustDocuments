using System;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class AdditionalHitToStatusTaker : Ability
	{
		public class Instance : AbilityInstance<AdditionalHitToStatusTaker>
		{
			public Instance(Character owner, AdditionalHitToStatusTaker ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Combine(character.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveStatus = (Character.OnGaveStatusDelegate)Delegate.Remove(character.onGaveStatus, new Character.OnGaveStatusDelegate(OnGaveStatus));
			}

			private void OnGaveStatus(Character target, CharacterStatus.ApplyInfo applyInfo, bool result)
			{
				if (result && ability._statuses[applyInfo.kind])
				{
					Damage damage = owner.stat.GetDamage(ability._additionalDamageAmount, MMMaths.RandomPointWithinBounds(target.collider.bounds), ability._additionalHit);
					owner.Attack(target, ref damage);
				}
			}
		}

		[SerializeField]
		private CharacterStatusKindBoolArray _statuses;

		[SerializeField]
		private float _additionalDamageAmount;

		[SerializeField]
		private HitInfo _additionalHit = new HitInfo(Damage.AttackType.Additional);

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
