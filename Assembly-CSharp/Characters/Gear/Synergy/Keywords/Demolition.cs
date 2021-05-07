using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using FX;
using Singletons;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Demolition : Keyword
	{
		private class CooldownChecker : IAbility
		{
			private class Instance : IAbilityInstance
			{
				public Character owner { get; private set; }

				public IAbility ability { get; private set; }

				public float remainTime { get; set; }

				public bool attached { get; private set; }

				public Sprite icon => null;

				public float iconFillAmount => 0f;

				public bool iconFillInversed => false;

				public bool iconFillFlipped => false;

				public int iconStacks => 0;

				public bool expired => remainTime <= 0f;

				public Instance(IAbility ability, Character owner)
				{
					this.ability = ability;
					this.owner = owner;
				}

				public void Attach()
				{
					attached = true;
				}

				public void Detach()
				{
					attached = false;
				}

				public void Refresh()
				{
				}

				public void UpdateTime(float deltaTime)
				{
					remainTime -= deltaTime;
				}
			}

			public float duration => 0f;

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return new Instance(this, owner);
			}

			public void Initialize()
			{
			}
		}

		[SerializeField]
		private float _cooldownPerCharacter = 10f;

		[SerializeField]
		private double[] _damageMultiplierByLevel;

		[Space]
		[SerializeField]
		[Tooltip("발동 시 대상 위치에 스폰할 사운드")]
		private SoundInfo _sound;

		[SerializeField]
		[Tooltip("발동 시 대상 위치에 스폰할 이펙트")]
		private EffectInfo _effect;

		private CooldownChecker _cooldownChecker;

		public override Key key => Key.Demolition;

		protected override IList valuesByLevel => _damageMultiplierByLevel;

		protected override void Initialize()
		{
			_cooldownChecker = new CooldownChecker();
			_cooldownChecker.Initialize();
		}

		protected override void UpdateBonus()
		{
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
			if (base.level != 0 && gaveDamage.attribute == Damage.Attribute.Physical && !(target.character == null) && !target.character.ability.Contains(_cooldownChecker))
			{
				Vector2 hitPoint = gaveDamage.hitPoint;
				hitPoint.y += 0.5f;
				Damage damage = new Damage(base.character, damageDealt * _damageMultiplierByLevel[base.level] * 0.01, hitPoint, Damage.Attribute.Fixed, Damage.AttackType.Additional, Damage.MotionType.Item, 1.0, 0f, 0.0, 1.0, true);
				base.character.Attack(target.character, ref damage);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_sound, target.transform.position);
				_effect.Spawn(target.transform.position, target.character);
				target.character.ability.Add(_cooldownChecker).remainTime = _cooldownPerCharacter;
			}
		}
	}
}
