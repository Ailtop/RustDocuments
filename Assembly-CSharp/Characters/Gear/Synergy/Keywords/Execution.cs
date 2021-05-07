using System;
using System.Collections;
using Characters.Abilities;
using FX;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Execution : Keyword
	{
		[Serializable]
		public class Debuff : Ability
		{
			public class Instance : AbilityInstance<Debuff>
			{
				private ReusableChronoSpriteEffect _executionLoopEffect;

				public Instance(Character owner, Debuff ability)
					: base(owner, ability)
				{
				}

				protected override void OnAttach()
				{
					owner.health.onTakeDamage.Add(int.MinValue, OnTakeDamage);
					owner.health.onChanged += OnHealthChanged;
				}

				protected override void OnDetach()
				{
					owner.health.onTakeDamage.Remove(OnTakeDamage);
					owner.health.onChanged -= OnHealthChanged;
					if (_executionLoopEffect != null)
					{
						_executionLoopEffect.reusable.Despawn();
						_executionLoopEffect = null;
					}
				}

				private bool OnTakeDamage(ref Damage damage)
				{
					if (owner.health.percent > ability.criticalHealthThresould)
					{
						return false;
					}
					damage.criticalChance = 1.0;
					damage.Evaluate(false);
					return false;
				}

				private void OnHealthChanged()
				{
					if (owner.health.percent > ability.criticalHealthThresould)
					{
						if (_executionLoopEffect != null)
						{
							_executionLoopEffect.reusable.Despawn();
							_executionLoopEffect = null;
						}
					}
					else if (_executionLoopEffect == null)
					{
						ability._executionActivatingEffect.Spawn(owner.transform.position, owner);
						_executionLoopEffect = ability._executionLoopEffect.Spawn(owner.transform.position, owner);
					}
				}
			}

			[NonSerialized]
			public double criticalHealthThresould;

			[Header("찐 이펙트")]
			[SerializeField]
			[Tooltip("처형 조건이 충족될 때 지속적으로 표시될 이펙트")]
			private EffectInfo _executionLoopEffect = new EffectInfo
			{
				subordinated = true
			};

			[SerializeField]
			[Tooltip("처형 조건이 충족될 때 한 번 표시될 이펙트")]
			private EffectInfo _executionActivatingEffect;

			public override IAbilityInstance CreateInstance(Character owner)
			{
				return new Instance(owner, this);
			}
		}

		[SerializeField]
		private double[] _criticalHealthThresholdByLevel;

		[SerializeField]
		private Debuff _debuff;

		public override Key key => Key.Execution;

		protected override IList valuesByLevel => _criticalHealthThresholdByLevel;

		protected override void Initialize()
		{
			_debuff.Initialize();
		}

		protected override void UpdateBonus()
		{
			_debuff.criticalHealthThresould = _criticalHealthThresholdByLevel[base.level] * 0.01;
		}

		protected override void OnAttach()
		{
			base.character.onGiveDamage.Add(int.MinValue, GiveDamageDelegate);
		}

		protected override void OnDetach()
		{
			base.character.onGiveDamage.Remove(GiveDamageDelegate);
		}

		private bool GiveDamageDelegate(ITarget target, ref Damage damage)
		{
			if (target.character == null)
			{
				return false;
			}
			if (TargetLayer.IsPlayer(target.character.gameObject.layer))
			{
				return false;
			}
			target.character.ability.Add(_debuff);
			return false;
		}
	}
}
