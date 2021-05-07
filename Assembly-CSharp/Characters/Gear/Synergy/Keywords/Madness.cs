using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using FX;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Madness : Keyword
	{
		private class Ability : IAbility, IAbilityInstance
		{
			private readonly EffectInfo _loopEffect;

			private readonly double _attackSpeedPerStack;

			public int maxStack;

			private Character _owner;

			private int _stacks;

			private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.AttackSpeed, 0.0));

			private Sprite _icon;

			private ReusableChronoSpriteEffect _loopEffectInstance;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon
			{
				get
				{
					if (_stacks <= 0)
					{
						return null;
					}
					return _icon;
				}
			}

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => (int)((double)_stacks * _attackSpeedPerStack * 100.0);

			public bool expired => false;

			public float duration => 5f;

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, Sprite icon, EffectInfo loopEffect, double attackSpeedPerStack)
			{
				_owner = owner;
				_icon = icon;
				_loopEffect = loopEffect;
				_attackSpeedPerStack = attackSpeedPerStack;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
				if (_stacks <= 0)
				{
					return;
				}
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					if (_loopEffectInstance != null)
					{
						_loopEffectInstance.reusable.Despawn();
						_loopEffectInstance = null;
					}
					_stacks = 0;
					UpdateStat();
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				remainTime = duration;
				Character owner = _owner;
				owner.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(owner.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
				_owner.stat.AttachValues(_stat);
			}

			void IAbilityInstance.Detach()
			{
				Character owner = _owner;
				owner.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(owner.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
				_owner.stat.DetachValues(_stat);
				if (_loopEffectInstance != null)
				{
					_loopEffectInstance.reusable.Despawn();
					_loopEffectInstance = null;
				}
			}

			private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(target.character == null) && tookDamage.attackType != 0 && tookDamage.motionType == Damage.MotionType.Basic && !(damageDealt <= 1.4012984643248171E-45))
				{
					if (_loopEffectInstance == null)
					{
						_loopEffectInstance = _loopEffect.Spawn(_owner.transform.position, _owner);
					}
					_stacks++;
					if (_stacks > maxStack)
					{
						_stacks = maxStack;
					}
					remainTime = duration;
					UpdateStat();
				}
			}

			private void UpdateStat()
			{
				_stat.values[0].value = (double)_stacks * _attackSpeedPerStack;
				_owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private int _attackSpeedPerStack;

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private int[] _maxStacksByLevel;

		[SerializeField]
		private EffectInfo _loopEffect = new EffectInfo
		{
			subordinated = true
		};

		private int[] _valuesByLevel;

		private Ability _ability;

		public override Key key => Key.Madness;

		protected override IList valuesByLevel
		{
			get
			{
				if (_valuesByLevel == null)
				{
					_valuesByLevel = (int[])_maxStacksByLevel.Clone();
					for (int i = 0; i < _valuesByLevel.Length; i++)
					{
						_valuesByLevel[i] *= _attackSpeedPerStack;
					}
				}
				return _valuesByLevel;
			}
		}

		protected override void Initialize()
		{
			_ability = new Ability(base.character, _icon, _loopEffect, (double)_attackSpeedPerStack * 0.01);
			_ability.Initialize();
		}

		protected override void UpdateBonus()
		{
			_ability.maxStack = _maxStacksByLevel[base.level];
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_ability);
		}

		protected override void OnDetach()
		{
			base.character.ability.Remove(_ability);
		}
	}
}
