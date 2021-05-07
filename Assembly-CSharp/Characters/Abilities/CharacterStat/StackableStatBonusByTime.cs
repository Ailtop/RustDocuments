using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StackableStatBonusByTime : Ability
	{
		public class Instance : AbilityInstance<StackableStatBonusByTime>
		{
			private float _remainUpdateTime;

			private int _stacks;

			private Stat.Values _stat;

			public override float iconFillAmount
			{
				get
				{
					if (_stacks >= ability._maxStack)
					{
						return base.iconFillAmount;
					}
					if (_remainUpdateTime > 0f)
					{
						return _remainUpdateTime / ability._updateInterval;
					}
					return base.iconFillAmount;
				}
			}

			public override int iconStacks => (int)((float)_stacks * ability._iconStacksPerStack);

			public Instance(Character owner, StackableStatBonusByTime ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerStack.Clone();
				_stacks = ability._startStack;
				owner.stat.AttachValues(_stat);
				UpdateStack();
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
			}

			private void UpdateStack()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerStack.values[i].GetStackedValue(_stacks);
				}
				owner.stat.SetNeedUpdate();
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				_remainUpdateTime -= deltaTime;
				if (_stacks < ability._maxStack && _remainUpdateTime < 0f)
				{
					_remainUpdateTime += ability._updateInterval;
					if (_stacks < ability._maxStack)
					{
						_stacks++;
						UpdateStack();
					}
				}
			}
		}

		[SerializeField]
		private float _updateInterval;

		[SerializeField]
		private int _startStack;

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		[Tooltip("스택이 쌓일 때마다 남은 시간을 초기화할지")]
		private bool _refreshRemainTime = true;

		[SerializeField]
		[Tooltip("실제 스택 1개당 아이콘 상에 표시할 스택")]
		private float _iconStacksPerStack = 1f;

		[SerializeField]
		private Stat.Values _statPerStack;

		public override void Initialize()
		{
			base.Initialize();
			if (_maxStack == 0)
			{
				_maxStack = int.MaxValue;
			}
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
