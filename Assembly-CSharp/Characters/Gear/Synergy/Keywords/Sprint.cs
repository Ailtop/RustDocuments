using System;
using System.Collections;
using Characters.Abilities;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Sprint : Keyword
	{
		protected class Ability : IAbility, IAbilityInstance
		{
			private const double _speedPerStack = 0.01;

			public Sprite buffIcon;

			public int maxStack;

			private Stat.Values _stat = new Stat.Values(new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.MovementSpeed, 1.0), new Stat.Value(Stat.Category.PercentPoint, Stat.Kind.AttackSpeed, 1.0));

			private Character _owner;

			private float _stacks;

			private float _moved;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon
			{
				get
				{
					if (!(_stacks > 0f))
					{
						return null;
					}
					return buffIcon;
				}
			}

			public float iconFillAmount => 0f;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => (int)((double)_stacks * 0.01 * 100.0);

			public bool expired => false;

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner)
			{
				_owner = owner;
			}

			public void Initialize()
			{
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
				if (remainTime < 0f)
				{
					remainTime += 0.2f;
					UpdateStat();
				}
			}

			public void Refresh()
			{
			}

			void IAbilityInstance.Attach()
			{
				_owner.movement.onMoved += OnMoved;
				_owner.stat.AttachValues(_stat);
			}

			void IAbilityInstance.Detach()
			{
				_owner.movement.onMoved -= OnMoved;
				_owner.stat.DetachValues(_stat);
			}

			private void OnMoved(Vector2 amount)
			{
				_moved += Mathf.Abs(amount.x);
			}

			public void UpdateStat()
			{
				_stacks -= 4f;
				_stacks += _moved * 5f;
				_moved = 0f;
				_stacks = Mathf.Clamp(_stacks, 0f, maxStack);
				_stat.values[0].value = Math.Max(0.0, 0.01 * (double)_stacks);
				_stat.values[1].value = Math.Max(0.0, 0.01 * (double)_stacks);
				_owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private float _duration = 3f;

		[SerializeField]
		private int[] _maxSpeedByLevel;

		private Ability _ability;

		public override Key key => Key.Sprint;

		protected override IList valuesByLevel => _maxSpeedByLevel;

		protected override void Initialize()
		{
			_ability = new Ability(base.character);
			_ability.duration = _duration;
			_ability.buffIcon = _icon;
			_ability.Initialize();
		}

		protected override void UpdateBonus()
		{
			_ability.maxStack = _maxSpeedByLevel[base.level];
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
