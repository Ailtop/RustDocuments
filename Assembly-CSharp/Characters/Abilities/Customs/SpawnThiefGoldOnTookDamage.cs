using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Data;
using Level;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class SpawnThiefGoldOnTookDamage : Ability
	{
		public class Instance : AbilityInstance<SpawnThiefGoldOnTookDamage>
		{
			public Instance(Character owner, SpawnThiefGoldOnTookDamage ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				if (ability._characterTypeFilter[owner.type])
				{
					owner.health.onTookDamage += OnCharacterTookDamage;
				}
			}

			protected override void OnDetach()
			{
				if (ability._characterTypeFilter[owner.type])
				{
					owner.health.onTookDamage -= OnCharacterTookDamage;
				}
			}

			private void OnCharacterTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				Damage damage = tookDamage;
				if (!(damage.amount < ability._minDamage) && ability._motionTypeFilter[tookDamage.motionType] && ability._attackTypeFilter[tookDamage.attackType])
				{
					SpawnGold(tookDamage.hitPoint);
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
