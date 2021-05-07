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
	public class Duel : Keyword
	{
		protected class StatBonus : IAbility, IAbilityInstance
		{
			[NonSerialized]
			public Stat.Values statPerStack = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, 0.0));

			private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, 1.0));

			private Character _owner;

			private readonly int _maxStack;

			private int _stacks;

			private readonly EffectInfo _effect;

			private ReusableChronoSpriteEffect _effectInstance;

			public Character owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached { get; private set; }

			public Sprite icon { get; set; }

			public float iconFillAmount => 0f;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired => remainTime < 0f;

			public float duration { get; set; } = 5f;


			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public StatBonus(int maxStack, EffectInfo effect)
			{
				_maxStack = maxStack;
				_effect = effect;
			}

			public IAbilityInstance CreateInstance(Character owner)
			{
				_owner = owner;
				return this;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
			}

			public void Refresh()
			{
				remainTime = duration;
				AddStack();
			}

			void IAbilityInstance.Attach()
			{
				attached = true;
				_effectInstance = ((_effect == null) ? null : _effect.Spawn(owner.transform.position, owner));
				_stacks = 1;
				_owner.stat.AttachValues(_stat);
				remainTime = duration;
				UpdateStack();
			}

			void IAbilityInstance.Detach()
			{
				attached = false;
				if (_effectInstance != null)
				{
					_effectInstance.reusable.Despawn();
					_effectInstance = null;
				}
				_stacks = 0;
				_owner.stat.DetachValues(_stat);
			}

			public void AddStack()
			{
				remainTime = duration;
				if (_stacks != _maxStack)
				{
					_stacks++;
					_effectInstance.animator.SetInteger("Stacks", _stacks);
					UpdateStack();
				}
			}

			private void UpdateStack()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = statPerStack.values[i].GetStackedValue(_stacks);
				}
				_owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private int _maxStack = 10;

		[SerializeField]
		private double[] _damageByStackByLevel;

		[SerializeField]
		private TargetLayer _targetLayer;

		[SerializeField]
		[Tooltip("혹시 단계별 이펙트를 바꾸고 싶을 경우 Duel 애니메이터의 Transition과 Parameter를 수정하면 됨")]
		private EffectInfo _effect = new EffectInfo
		{
			subordinated = true
		};

		[SerializeField]
		private SoundInfo _attachSound;

		private StatBonus _currentInstance;

		public override Key key => Key.Duel;

		protected override IList valuesByLevel => _damageByStackByLevel;

		protected override void Initialize()
		{
			_currentInstance = new StatBonus(_maxStack, _effect);
		}

		protected override void UpdateBonus()
		{
			_currentInstance.statPerStack.values[0].value = 1.0 + _damageByStackByLevel[base.level] * 0.01;
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
			if (target.character == null || gaveDamage.attackType == Damage.AttackType.None || gaveDamage.attackType == Damage.AttackType.Additional || !_targetLayer.Evaluate(base.character.gameObject).Contains(target.character.gameObject.layer))
			{
				return;
			}
			if (_currentInstance.attached && _currentInstance.owner != null && _currentInstance.owner.liveAndActive)
			{
				if (_currentInstance.owner == target.character)
				{
					_currentInstance.AddStack();
				}
			}
			else
			{
				target.character.ability.Add(_currentInstance);
				PersistentSingleton<SoundManager>.Instance.PlaySound(_attachSound, target.transform.position);
			}
		}
	}
}
