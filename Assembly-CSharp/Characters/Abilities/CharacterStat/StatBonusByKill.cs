using System;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusByKill : Ability
	{
		public class Instance : AbilityInstance<StatBonusByKill>
		{
			private int _stacks;

			private Stat.Values _stat;

			private int stacks
			{
				get
				{
					if (!ability._permanenet)
					{
						return _stacks;
					}
					return ability._stacks;
				}
				set
				{
					if (ability._permanenet)
					{
						ability._stacks = value;
					}
					else
					{
						_stacks = value;
					}
				}
			}

			public override int iconStacks => (int)((float)ability._stacks * ability._iconStacksPerStack);

			public Instance(Character owner, StatBonusByKill ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerStack.Clone();
				Character character = owner;
				character.onKilled = (Character.OnKilledDelegate)Delegate.Combine(character.onKilled, new Character.OnKilledDelegate(OnCharacterKilled));
				owner.stat.AttachValues(_stat);
				UpdateStack();
			}

			protected override void OnDetach()
			{
				Character character = owner;
				character.onKilled = (Character.OnKilledDelegate)Delegate.Remove(character.onKilled, new Character.OnKilledDelegate(OnCharacterKilled));
				owner.stat.DetachValues(_stat);
			}

			public override void Refresh()
			{
				if (ability._refreshRemainTime)
				{
					base.Refresh();
				}
				AddStack();
			}

			private void AddStack()
			{
				if (stacks < ability._maxStack)
				{
					stacks++;
					UpdateStack();
				}
			}

			private void UpdateStack()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerStack.values[i].GetStackedValue(stacks);
				}
				owner.stat.SetNeedUpdate();
			}

			private void OnCharacterKilled(ITarget target, ref Damage damage)
			{
				if (ability._characterTypeFilter[target.character.type] && (!ability._characterTypeFilter[Character.Type.Boss] || (target.character.key != Key.FirstHero1 && target.character.key != Key.FirstHero2 && target.character.key != 0)) && ability._motionTypeFilter[damage.motionType] && ability._attackTypeFilter[damage.attackType] && (string.IsNullOrWhiteSpace(ability._attackKey) || damage.key.Equals(ability._attackKey, StringComparison.OrdinalIgnoreCase)))
				{
					AddStack();
				}
			}
		}

		[SerializeField]
		[Tooltip("비어있지 않을 경우, 해당 키를 가진 공격에만 발동됨")]
		private string _attackKey;

		[SerializeField]
		private MotionTypeBoolArray _motionTypeFilter;

		[SerializeField]
		private AttackTypeBoolArray _attackTypeFilter;

		[SerializeField]
		private CharacterTypeBoolArray _characterTypeFilter = new CharacterTypeBoolArray(true, true, true, true, true, false, false, false);

		[Space]
		[SerializeField]
		private bool _permanenet;

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

		private int _stacks;

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
