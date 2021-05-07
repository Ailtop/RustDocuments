using System;
using Characters.Actions;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class IgnoreSkillCooldown : Ability
	{
		public class Instance : AbilityInstance<IgnoreSkillCooldown>
		{
			public Instance(Character owner, IgnoreSkillCooldown ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				owner.onStartAction += OnStartAction;
			}

			protected override void OnDetach()
			{
				owner.onStartAction -= OnStartAction;
			}

			private void OnStartAction(Characters.Actions.Action action)
			{
				if (MMMaths.PercentChance(ability._possibility) && action.type == Characters.Actions.Action.Type.Skill && action.cooldown.time != null && !action.cooldown.usedByStreak)
				{
					action.cooldown.time.remainTime = Mathf.Min(0.2f, action.cooldown.time.cooldownTime);
				}
			}
		}

		[SerializeField]
		[Range(1f, 100f)]
		private int _possibility;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
