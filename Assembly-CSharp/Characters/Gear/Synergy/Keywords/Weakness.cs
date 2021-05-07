using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using FX;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Weakness : Keyword
	{
		[Serializable]
		public class Debuff : Ability
		{
			public class Instance : AbilityInstance<Debuff>
			{
				private ReusableChronoSpriteEffect _weaknessLoopEffect;

				public Instance(Character owner, Debuff ability)
					: base(owner, ability)
				{
				}

				protected override void OnAttach()
				{
					owner.health.onTakeDamage.Add(int.MinValue, OnTakeDamage);
					_weaknessLoopEffect = ability._weaknessLoopEffect.Spawn(owner.transform.position, owner);
				}

				protected override void OnDetach()
				{
					owner.health.onTakeDamage.Remove(OnTakeDamage);
					if (_weaknessLoopEffect != null)
					{
						_weaknessLoopEffect.reusable.Despawn();
						_weaknessLoopEffect = null;
					}
				}

				private bool OnTakeDamage(ref Damage damage)
				{
					if (!damage.critical)
					{
						return false;
					}
					damage.criticalDamageMultiplier += ability._damageMultiplierByLevel.GetValue() * 0.01f;
					return false;
				}
			}

			[SerializeField]
			private ValueByLevel _damageMultiplierByLevel;

			[Header("이펙트")]
			[SerializeField]
			[Tooltip("취약점 조건이 충족될 때 계속 표시될 이펙트")]
			private EffectInfo _weaknessLoopEffect;

			public override IAbilityInstance CreateInstance(Character owner)
			{
				return new Instance(owner, this);
			}
		}

		[SerializeField]
		private CharacterTypeBoolArray _characterType;

		[SerializeField]
		[Subcomponent(typeof(ValueByLevel))]
		private ValueByLevel _damageMultiplierByLevel;

		[SerializeField]
		private Debuff _debuff;

		public override Key key => Key.Weakness;

		protected override IList valuesByLevel => _damageMultiplierByLevel.values;

		protected override void Initialize()
		{
			_debuff.Initialize();
		}

		protected override void UpdateBonus()
		{
			_damageMultiplierByLevel.level = base.level;
		}

		protected override void OnAttach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(obj.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			if (!(target.character == null) && gaveDamage.critical && !TargetLayer.IsPlayer(target.character.gameObject.layer) && _characterType[target.character.type] && !target.character.ability.Contains(_debuff))
			{
				target.character.ability.Add(_debuff);
			}
		}
	}
}
