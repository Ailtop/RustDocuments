using System.Collections;
using Characters.Abilities;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Blitz : Keyword
	{
		protected class Ability : IAbility, IAbilityInstance
		{
			private Stat.Values _stat;

			private ValueByLevel _criticalChanceByLevel;

			private Character _owner;

			public Sprite buffIcon;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon => buffIcon;

			public float iconFillAmount => 1f - remainTime / duration;

			public int iconStacks => 0;

			public bool expired { get; set; }

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, ValueByLevel criticalChanceByLevel, Stat.Values values)
			{
				_owner = owner;
				_criticalChanceByLevel = criticalChanceByLevel;
				_stat = values;
			}

			public void Initialize()
			{
			}

			public void Refresh()
			{
				remainTime = duration;
			}

			void IAbilityInstance.Attach()
			{
				expired = false;
				remainTime = duration;
				_stat.values[0].value = _criticalChanceByLevel.GetValue() * 0.01f;
				_owner.stat.AttachValues(_stat);
			}

			void IAbilityInstance.Detach()
			{
				_owner.stat.DetachValues(_stat);
			}

			public void UpdateTime(float deltaTime)
			{
				remainTime -= deltaTime;
				if (remainTime <= 0f)
				{
					expired = true;
				}
			}
		}

		[SerializeField]
		private float _duration = 1f;

		[SerializeField]
		private ActionTypeBoolArray _actionType;

		[SerializeField]
		private Stat.Values _stats;

		[SerializeField]
		[Subcomponent(typeof(ValueByLevel))]
		private ValueByLevel _criticalChanceByLevel;

		[SerializeField]
		private Sprite _icon;

		private Ability _ability;

		public override Key key => Key.Blitz;

		protected override IList valuesByLevel => _criticalChanceByLevel.values;

		protected override void Initialize()
		{
			_ability = new Ability(base.character, _criticalChanceByLevel, _stats)
			{
				buffIcon = _icon,
				duration = _duration
			};
			_ability.Initialize();
		}

		protected override void UpdateBonus()
		{
			_criticalChanceByLevel.level = base.level;
		}

		protected override void OnAttach()
		{
			base.character.onStartAction += OnStartAction;
		}

		protected override void OnDetach()
		{
			base.character.onStartAction -= OnStartAction;
			base.character.ability.Remove(_ability);
		}

		private void OnStartAction(Action action)
		{
			if (_actionType[action.type])
			{
				base.character.ability.Add(_ability);
			}
		}
	}
}
