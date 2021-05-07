using System;
using Characters.Actions;
using UnityEngine;

namespace Characters.Abilities.CharacterStat
{
	[Serializable]
	public class StatBonusBySkillsInCooldown : Ability
	{
		public class Instance : AbilityInstance<StatBonusBySkillsInCooldown>
		{
			private int _stacks;

			private Stat.Values _stat;

			public override Sprite icon
			{
				get
				{
					if (_stacks > 0)
					{
						return base.icon;
					}
					return null;
				}
			}

			public override int iconStacks => _stacks;

			public Instance(Character owner, StatBonusBySkillsInCooldown ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				_stat = ability._statPerStack.Clone();
				owner.stat.AttachValues(_stat);
			}

			protected override void OnDetach()
			{
				owner.stat.DetachValues(_stat);
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				int num = 0;
				foreach (Characters.Actions.Action action in owner.actions)
				{
					if (action.cooldown.time != null && action.type == Characters.Actions.Action.Type.Skill && !action.cooldown.canUse)
					{
						num++;
					}
				}
				if (num >= ability._maxStack)
				{
					num = ability._maxStack;
				}
				_stacks = num;
				UpdateStat();
			}

			private void UpdateStat()
			{
				for (int i = 0; i < _stat.values.Length; i++)
				{
					_stat.values[i].value = ability._statPerStack.values[i].GetStackedValue(_stacks);
				}
				owner.stat.SetNeedUpdate();
			}
		}

		[SerializeField]
		private Stat.Values _statPerStack;

		[SerializeField]
		private int _maxStack;

		[SerializeField]
		private int _skillInCooldownPerStack;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
