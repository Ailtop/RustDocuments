using System;
using Characters.Gear.Weapons.Gauges;
using Characters.Operations.Summon;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class RockstarPassive : Ability, IAbilityInstance
	{
		[SerializeField]
		private ValueGauge _gauge;

		[SerializeField]
		[Tooltip("실제 스택 1개당 아이콘 상에 표시할 스택")]
		private float _iconStacksPerStack = 1f;

		[SerializeField]
		private Stat.Values _statPerStack;

		private Stat.Values _stat;

		[SerializeField]
		private float _buffDuration;

		private float _buffRemaintime;

		[SerializeField]
		private float _summonCooldown;

		private float _summonRemainCooldown;

		[SerializeField]
		[Tooltip("게이지가 꽉찼을 때 실행할 SummonOperationRunner")]
		private SummonOperationRunner _summonOperationRunner;

		public Action onSummon;

		public Character owner { get; set; }

		public IAbility ability => this;

		public float remainTime { get; set; }

		public bool attached => true;

		public Sprite icon => _defaultIcon;

		public float iconFillAmount => _summonRemainCooldown / _summonCooldown;

		public int iconStacks => (int)(_gauge.currentValue * _iconStacksPerStack);

		public bool expired => false;

		public override void Initialize()
		{
			base.Initialize();
			_stat = _statPerStack.Clone();
			_summonOperationRunner.Initialize();
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			this.owner = owner;
			return this;
		}

		public void UpdateTime(float deltaTime)
		{
			_buffRemaintime -= deltaTime;
			if (_buffRemaintime < 0f)
			{
				_buffRemaintime = _buffDuration;
				_gauge.Clear();
				UpdateStack();
			}
			_summonRemainCooldown -= deltaTime;
		}

		public void Refresh()
		{
		}

		public void Attach()
		{
			_buffRemaintime = _buffDuration;
			owner.stat.AttachValues(_stat);
			UpdateStack();
		}

		public void Detach()
		{
			owner.stat.DetachValues(_stat);
		}

		public void AddStack(int amount)
		{
			_buffRemaintime = _buffDuration;
			_gauge.Add(amount);
			UpdateStack();
			if (!(_gauge.currentValue < _gauge.maxValue) && !(_summonRemainCooldown > 0f))
			{
				_summonRemainCooldown = _summonCooldown;
				_summonOperationRunner.Run(owner);
				onSummon?.Invoke();
			}
		}

		private void UpdateStack()
		{
			for (int i = 0; i < _stat.values.Length; i++)
			{
				_stat.values[i].value = _statPerStack.values[i].GetStackedValue((int)_gauge.currentValue);
			}
			owner.stat.SetNeedUpdate();
		}
	}
}
