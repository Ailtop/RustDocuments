using System.Collections;
using Characters.Abilities;
using Characters.Actions;
using FX;
using UnityEditor;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Preparation : Keyword
	{
		protected class CoolDown : IAbility, IAbilityInstance
		{
			private Character _owner;

			private Sprite _icon;

			private EffectInfo _effectOnDetach;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon => _icon;

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => true;

			public bool iconFillFlipped => true;

			public int iconStacks => 0;

			public bool expired { get; set; }

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public CoolDown(Character owner, Sprite icon, EffectInfo effectOnDetach)
			{
				_owner = owner;
				_icon = icon;
				_effectOnDetach = effectOnDetach;
			}

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public void Attach()
			{
				remainTime = duration;
			}

			public void Detach()
			{
			}

			public void Initialize()
			{
			}

			public void Refresh()
			{
				remainTime = duration;
			}

			public void UpdateTime(float deltaTime)
			{
				if (!(remainTime <= 0f))
				{
					remainTime -= deltaTime;
					if (remainTime <= 0f)
					{
						_effectOnDetach?.Spawn(_owner.transform.position, _owner);
					}
				}
			}
		}

		protected class Ability : IAbility, IAbilityInstance
		{
			public Sprite buffIcon;

			private Stat.Values _stat;

			private ValueByLevel _attackSpeedByLevel;

			private EffectInfo _attachEffect;

			private Character _owner;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon => buffIcon;

			public float iconFillAmount => 1f - remainTime / duration;

			public bool iconFillInversed => false;

			public bool iconFillFlipped => false;

			public int iconStacks => 0;

			public bool expired { get; set; }

			public float duration { get; set; }

			public int iconPriority => 0;

			public bool removeOnSwapWeapon => false;

			public IAbilityInstance CreateInstance(Character owner)
			{
				return this;
			}

			public Ability(Character owner, ValueByLevel criticalChanceByLevel, EffectInfo attachEffect, Stat.Values values)
			{
				_owner = owner;
				_attackSpeedByLevel = criticalChanceByLevel;
				_attachEffect = attachEffect;
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
				Stat.Value[] values = _stat.values;
				for (int i = 0; i < values.Length; i++)
				{
					values[i].value = _attackSpeedByLevel.GetValue() * 0.01f;
				}
				_owner.stat.AttachValues(_stat);
				_attachEffect?.Spawn(_owner.transform.position, _owner);
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
		private float _coolDownTime = 10f;

		[SerializeField]
		private float _buffDuration = 15f;

		[SerializeField]
		private ActionTypeBoolArray _actionType;

		[SerializeField]
		[Subcomponent(typeof(ValueByLevel))]
		private ValueByLevel _attackSpeedByLevel;

		[SerializeField]
		private Sprite _icon;

		[SerializeField]
		private Sprite _coolDownIcon;

		[Header("이펙트")]
		[SerializeField]
		private EffectInfo _prepareEffect;

		[SerializeField]
		private EffectInfo _attachEffect;

		[Header("스텟 ")]
		[SerializeField]
		private Stat.Values _stat;

		private CoolDown _coolDown;

		private Ability _ability;

		public override Key key => Key.Preparation;

		protected override IList valuesByLevel => _attackSpeedByLevel.values;

		protected override void Initialize()
		{
			_ability = new Ability(base.character, _attackSpeedByLevel, _attachEffect, _stat)
			{
				buffIcon = _icon,
				duration = _buffDuration
			};
			_ability.Initialize();
			_coolDown = new CoolDown(base.character, _coolDownIcon, _prepareEffect)
			{
				duration = _coolDownTime
			};
			_coolDown.Initialize();
		}

		protected override void UpdateBonus()
		{
			_attackSpeedByLevel.level = base.level;
		}

		protected override void OnAttach()
		{
			base.character.ability.Add(_coolDown);
			base.character.onStartAction += OnStartAction;
		}

		protected override void OnDetach()
		{
			base.character.onStartAction -= OnStartAction;
			base.character.ability.Remove(_ability);
			base.character.ability.Remove(_coolDown);
		}

		private void OnStartAction(Action action)
		{
			if (!(_coolDown.remainTime > 0f) && _actionType[action.type])
			{
				base.character.ability.Add(_ability);
				_coolDown.Refresh();
			}
		}
	}
}
