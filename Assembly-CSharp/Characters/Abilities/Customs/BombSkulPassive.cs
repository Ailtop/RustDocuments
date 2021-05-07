using System;
using System.Collections.Generic;
using Characters.Abilities.Constraints;
using Characters.Actions;
using Characters.Gear.Weapons.Gauges;
using Characters.Operations;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class BombSkulPassive : Ability, IAbilityInstance
	{
		private const int _maxSmallBombs = 100;

		[Header("Gauge")]
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		private Color _defaultGaugeColor;

		[SerializeField]
		[Tooltip("피해량 증가 스택에 비례해 게이지 색깔이 이 색깔로 변합니다. 100일 때 완전히 변합니다.")]
		private Color _damageStackedGaugeColor;

		[SerializeField]
		private float _gaugeAmountPerSecond;

		[SerializeField]
		[Constraint.Subcomponent]
		private Constraint.Subcomponents _gaugeConstraints;

		[Header("Risky Upgrade")]
		[SerializeField]
		private Characters.Actions.Action _riskyUpgrade;

		[SerializeField]
		private int _upgradablecount;

		[SerializeField]
		private int _damageStacksByUpgrade;

		[SerializeField]
		private int[] _upgradeChances;

		private int _upgradedCount;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _onUpgradeSucceeded;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _onUpgradeFailed;

		[Space]
		[SerializeField]
		private PlaySoundInfo _fuseSound;

		private int _damageStacks;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

		private readonly List<OperationRunner> _smallBombs = new List<OperationRunner>(100);

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		public bool attached => true;

		public Sprite icon
		{
			get
			{
				if (_damageStacks <= 0)
				{
					return null;
				}
				return _defaultIcon;
			}
		}

		public float iconFillAmount => 0f;

		public int iconStacks => _damageStacks;

		public bool expired => false;

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
			_onUpgradeSucceeded.Initialize();
			_onUpgradeFailed.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		public void UpdateTime(float deltaTime)
		{
			if (_gaugeConstraints.Pass())
			{
				AddGauge(_gaugeAmountPerSecond * deltaTime);
			}
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			_damageStacks = 0;
			_upgradedCount = 0;
			_riskyUpgrade.cooldown.stacks = _upgradablecount;
			_gauge.defaultBarColor = _defaultGaugeColor;
			ResetGauge();
			owner.onGiveDamage.Remove(OnGiveDamage);
			Singleton<Service>.Instance.levelManager.onMapLoaded += RemoveAllSmallBombs;
		}

		public void Detach()
		{
			owner.onGiveDamage.Remove(OnGiveDamage);
			Singleton<Service>.Instance.levelManager.onMapLoaded -= RemoveAllSmallBombs;
			RemoveAllSmallBombs();
		}

		public void AddDamageStack(int amount)
		{
			_damageStacks += amount;
			_gauge.defaultBarColor = Color.Lerp(_defaultGaugeColor, _damageStackedGaugeColor, (float)_damageStacks / 100f);
		}

		public void AddGauge(float amount)
		{
			_gauge.Add(amount);
			if (!(_gauge.currentValue < _gauge.maxValue))
			{
				Explode();
			}
		}

		public void ResetGauge()
		{
			_fuseSound.Stop();
			_fuseSound.Play();
			_gauge.Clear();
		}

		public void Explode()
		{
			_gauge.Clear();
			owner.onGiveDamage.Add(0, OnGiveDamage);
			_operations.Run(owner);
			foreach (OperationRunner smallBomb in _smallBombs)
			{
				smallBomb.operationInfos.Run(owner);
			}
			_smallBombs.Clear();
			owner.onGiveDamage.Remove(OnGiveDamage);
			owner.playerComponents.inventory.weapon.NextWeapon(true);
		}

		public void RiskyUpgrade()
		{
			if (!MMMaths.PercentChance(_upgradeChances[Math.Min(_upgradedCount, _upgradeChances.Length - 1)]))
			{
				_onUpgradeFailed.Run(owner);
				Explode();
				return;
			}
			_onUpgradeSucceeded.Run(owner);
			ResetGauge();
			_upgradedCount++;
			AddDamageStack(_damageStacksByUpgrade);
		}

		private bool OnGiveDamage(ITarget target, ref Damage damage)
		{
			damage.multiplier += (double)_damageStacks * 0.01;
			return false;
		}

		private void RemoveAllSmallBombs()
		{
			if (_smallBombs.Count != 0)
			{
				_smallBombs[0].poolObject.DespawnAllSiblings();
				_smallBombs.Clear();
			}
		}

		public void RegisterSmallBomb(OperationRunner smallBomb)
		{
			if (_smallBombs.Count >= 100)
			{
				_smallBombs[0].operationInfos.Run(owner);
				_smallBombs.RemoveAt(0);
			}
			_smallBombs.Add(smallBomb);
		}
	}
}
