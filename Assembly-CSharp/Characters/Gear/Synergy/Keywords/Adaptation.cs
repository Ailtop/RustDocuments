using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Abilities;
using Characters.Operations;
using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public class Adaptation : Keyword
	{
		protected class StatBonus : IAbility, IAbilityInstance
		{
			[NonSerialized]
			public double currentStatBonus;

			[NonSerialized]
			public Stat.Values stat = new Stat.Values(new Stat.Value(0, 0, 0.0));

			private Character _owner;

			Character IAbilityInstance.owner => _owner;

			public IAbility ability => this;

			public float remainTime { get; set; }

			public bool attached => true;

			public Sprite icon { get; set; }

			public float iconFillAmount => 1f - remainTime / duration;

			public int iconStacks => (int)(currentStatBonus * 100.0);

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

			public StatBonus(Character owner)
			{
				_owner = owner;
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
				remainTime = duration;
				expired = false;
				_owner.stat.AttachValues(stat);
			}

			void IAbilityInstance.Detach()
			{
				_owner.stat.DetachValues(stat);
			}

			public void UpdateStat()
			{
				stat.values[0].value = currentStatBonus;
				_owner.stat.SetNeedUpdate();
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
		private Sprite _physicalIcon;

		[SerializeField]
		private Sprite _magicIcon;

		[SerializeField]
		private float _statBonusDuration = 20f;

		[SerializeField]
		private float _refreshInterval = 30f;

		[SerializeField]
		private double[] _statBonusByLevel = new double[4] { 10.0, 25.0, 60.0, 100.0 };

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _onCompleteCoolDown;

		private Stat.Category _currentStatCategory;

		private Stat.Kind _currentStatKind;

		private Sprite _currentIcon;

		private StatBonus _statBonus;

		private bool _completeCoolDown = true;

		public override Key key => Key.Adaptation;

		protected override IList valuesByLevel => _statBonusByLevel;

		private double[] statBonusByLevel => _statBonusByLevel;

		protected override void Initialize()
		{
			_statBonus = new StatBonus(base.character)
			{
				duration = _statBonusDuration
			};
			_statBonus.Initialize();
			_onCompleteCoolDown.Initialize();
		}

		protected override void OnAttach()
		{
			_completeCoolDown = true;
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(obj.onGaveDamage, new GaveDamageDelegate(ApplyStatBonus));
		}

		protected override void OnDetach()
		{
			Character obj = base.character;
			obj.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(obj.onGaveDamage, new GaveDamageDelegate(ApplyStatBonus));
			base.character.ability.Remove(_statBonus);
			StopCoroutine("CRefresh");
		}

		private IEnumerator CRefresh()
		{
			_completeCoolDown = false;
			yield return base.character.chronometer.master.WaitForSeconds(_refreshInterval);
			_completeCoolDown = true;
			_onCompleteCoolDown.Run(base.character);
		}

		private void ApplyStatBonus(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage gaveDamage, double damageDealt)
		{
			if (_completeCoolDown)
			{
				if (gaveDamage.attribute == Damage.Attribute.Magic)
				{
					_currentIcon = _magicIcon;
					_currentStatCategory = Stat.Category.PercentPoint;
					_currentStatKind = Stat.Kind.MagicAttackDamage;
					base.character.ability.Add(_statBonus);
					UpdateStat();
					StartCoroutine(CRefresh());
				}
				else if (gaveDamage.attribute == Damage.Attribute.Physical)
				{
					_currentIcon = _physicalIcon;
					_currentStatCategory = Stat.Category.PercentPoint;
					_currentStatKind = Stat.Kind.PhysicalAttackDamage;
					base.character.ability.Add(_statBonus);
					UpdateStat();
					StartCoroutine(CRefresh());
				}
			}
		}

		public void UpdateStat()
		{
			_statBonus.currentStatBonus = statBonusByLevel[base.level];
			if (_currentStatCategory.index == Stat.Category.Percent.index)
			{
				_statBonus.currentStatBonus = _statBonus.currentStatBonus * 0.01 + 1.0;
			}
			else if (_currentStatCategory.index == Stat.Category.PercentPoint.index)
			{
				_statBonus.currentStatBonus *= 0.01;
			}
			_statBonus.icon = _currentIcon;
			_statBonus.stat.values[0].categoryIndex = _currentStatCategory.index;
			_statBonus.stat.values[0].kindIndex = _currentStatKind.index;
			_statBonus.UpdateStat();
		}

		protected override void UpdateBonus()
		{
		}
	}
}
