using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByGaveDamage : Ability
	{
		public class Instance : AbilityInstance<StatBonusByGaveDamage>
		{
			private const float _updateInterval = 0.2f;

			private float _remainUpdateTime;

			private Stat.Values _stat;

			private float _gaveDamage;

			private float _gaveDamageBefore;

			private float _remainTime;

			public override Sprite icon
			{
				get
				{
					if (!(_gaveDamage > 0f))
					{
						return null;
					}
					return ability.defaultIcon;
				}
			}

			public override float iconFillAmount => 1f - _remainTime / ability._timeToReset;

			public override int iconStacks => (int)(_stat.values[0].value * 100.0);

			public Instance(Character owner, StatBonusByGaveDamage ability)
				: base(owner, ability)
			{
				_stat = ability._maxStat.Clone();
			}

			protected override void OnAttach()
			{
				Stat.Value[] values = _stat.values;
				for (int i = 0; i < values.Length; i++)
				{
					values[i].value = ability._maxStat.values[i].GetMultipliedValue(0f);
				}
				owner.stat.AttachValues(_stat);
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
				Character character = owner;
				character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnOwnerGaveDamage));
			}

			private void OnOwnerGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
			{
				if (!(target.character == null) && ability._motionTypeFilter[gaveDamage.motionType] && ability._damageTypeFilter[gaveDamage.attackType] && ability._attributeFilter[gaveDamage.attribute])
				{
					_remainTime = ability._timeToReset;
					float gaveDamage2 = _gaveDamage;
					Damage damage = gaveDamage;
					_gaveDamage = gaveDamage2 + (float)damage.amount;
					if (_gaveDamage > ability._damageToMax)
					{
						_gaveDamage = ability._damageToMax;
					}
				}
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainTime -= deltaTime;
				_remainUpdateTime -= deltaTime;
				if (_remainTime < 0f)
				{
					_gaveDamage = 0f;
					UpdateStat();
				}
				else if (_remainUpdateTime < 0f)
				{
					_remainUpdateTime = 0.2f;
					UpdateStat();
				}
			}

			public void UpdateStat()
			{
				if (_gaveDamage != _gaveDamageBefore)
				{
					Stat.Value[] values = _stat.values;
					float multiplier = _gaveDamage / ability._damageToMax;
					for (int i = 0; i < values.Length; i++)
					{
						values[i].value = ability._maxStat.values[i].GetMultipliedValue(multiplier);
					}
					owner.stat.SetNeedUpdate();
					_gaveDamageBefore = _gaveDamage;
				}
			}
		}

		[SerializeField]
		private float _timeToReset;

		[SerializeField]
		private float _damageToMax;

		[Space]
		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _damageTypeFilter;

		[SerializeField]
		private DamageAttributeBoolArray _attributeFilter;

		[Space]
		[SerializeField]
		private Stat.Values _maxStat;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
