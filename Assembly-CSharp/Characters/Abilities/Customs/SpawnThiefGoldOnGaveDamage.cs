using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Data;
using Level;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class SpawnThiefGoldOnGaveDamage : Ability
	{
		public class Instance : AbilityInstance<SpawnThiefGoldOnGaveDamage>
		{
			public Instance(Character owner, SpawnThiefGoldOnGaveDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnCharacterGaveDamage));
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnCharacterGaveDamage));
			}

			private void OnCharacterGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				Damage damage = gaveDamage;
				if (!(damage.amount < ability._minDamage) && !(target.character == null) && ability._characterTypeFilter[target.character.type] && ability._motionTypeFilter[gaveDamage.motionType] && ability._attackTypeFilter[gaveDamage.attackType])
				{
					SpawnGold(gaveDamage.hitPoint);
				}
			}

			private void SpawnGold(Vector3 position)
			{
				CurrencyParticle component = ability._thiefGold.Spawn(position).GetComponent<CurrencyParticle>();
				component.currencyType = GameData.Currency.Type.Gold;
				component.currencyAmount = ability._goldAmount;
			}
		}

		[SerializeField]
		private PoolObject _thiefGold;

		[SerializeField]
		private int _goldAmount;

		[Header("Filter")]
		[SerializeField]
		private double _minDamage = 1.0;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter = new CharacterTypeBoolArray(true, true, true, true, true, false, false, false);

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
