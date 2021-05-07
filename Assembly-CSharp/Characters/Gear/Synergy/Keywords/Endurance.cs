using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Endurance : Keyword
	{
		protected class Ability : IAbility, IAbilityInstance
		{
			public Sprite buffIcon;

			private readonly double _damageReductionPerStack;

			private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.TakingDamage, 1.0));

			private Character _owner;

			private int _stacks;

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
					return buffIcon;
				}
			}

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => Math.Min(100, (int)((double)_stacks * _damageReductionPerStack * 100.0));

			public bool expired => false;

			public float duration { get; set; } = 3f;


			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, float damageReductionPerStack)
			{
				_owner = owner;
				_damageReductionPerStack = damageReductionPerStack;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					_stacks = 0;
					UpdateStat();
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				_owner.stat.AttachValues(_stat);
				_owner.health.onTookDamage += OnTookDamage;
			}

			void IAbilityInstance.Detach()
			{
				_owner.stat.DetachValues(_stat);
				_owner.health.onTookDamage -= OnTookDamage;
			}

			private void OnTookDamage([In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
			{
				if (!(damageDealt < 1.0))
				{
					remainTime = duration;
					_stacks++;
					UpdateStat();
				}
			}

			public void UpdateStat()
			{
				_stat.values[0].value = Math.Max(0.0, 1.0 - _damageReductionPerStack * (double)_stacks);
				_owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private float _damageReductionPerStack = 0.2f;

		[SerializeField]
		private float[] _durationByLevel = new float[6] { 0f, 1f, 2f, 3f, 4f, 5f };

		[SerializeField]
		private Sprite _icon;

		private Ability _ability;

		public override Key key => Key.Endurance;

		protected override IList valuesByLevel => _durationByLevel;

		protected override void Initialize()
		{
			_ability = new Ability(base.character, _damageReductionPerStack);
			_ability.buffIcon = _icon;
			_ability.Initialize();
		}

		protected override void UpdateBonus()
		{
			_ability.duration = _durationByLevel[base.level];
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
