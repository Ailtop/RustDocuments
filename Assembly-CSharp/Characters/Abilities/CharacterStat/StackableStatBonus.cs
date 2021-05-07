using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StackableStatBonus : Ability
	{
		public class Instance : AbilityInstance<StackableStatBonus>
		{
			private int _stacks;

			private Stat.Values _stat;

			public override int iconStacks => (int)((float)_stacks * ability._iconStacksPerStack);

			public Instance(Character owner, StackableStatBonus ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerStack.Clone();
				_stacks = 1;
				owner.stat.AttachValues(_stat);
				UpdateStack();
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
			}

			public override void Refresh()
			{
				if (ability._refreshRemainTime)
				{
					base.Refresh();
				}
				if (_stacks < ability._maxStack)
				{
					_stacks++;
				}
				UpdateStack();
			}

			private void UpdateStack()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerStack.values[i].GetStackedValue(_stacks);
				}
				owner.stat.SetNeedUpdate();
			}
		}

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
