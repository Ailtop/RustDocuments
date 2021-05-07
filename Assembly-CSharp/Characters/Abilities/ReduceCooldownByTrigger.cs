using System;
using Characters.Actions;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ReduceCooldownByTrigger : Ability
	{
		public class Instance : AbilityInstance<ReduceCooldownByTrigger>
		{
			private float _remainCooldownTime;

			public override float iconFillAmount
			{
				get
				{
					if (!(ability.trigger.cooldownTime > 0f))
					{
						return base.iconFillAmount;
					}
					return ability.trigger.remainCooldownTime / ability.trigger.cooldownTime;
				}
			}

			public Instance(Character owner, ReduceCooldownByTrigger ability)
				: base(owner, ability)
			{
			}

			protected override void OnAttach()
			{
				ability.trigger.Attach(owner);
				ability.trigger.onTriggered += OnTriggered;
			}

			protected override void OnDetach()
			{
				ability.trigger.Detach();
				ability.trigger.onTriggered -= OnTriggered;
			}

			public override void UpdateTime(float deltaTime)
			{
				base.UpdateTime(deltaTime);
				ability.trigger.UpdateTime(deltaTime);
			}

			private void OnTriggered()
			{
				foreach (Characters.Actions.Action action in owner.actions)
				{
					if (action.cooldown.time != null)
					{
						bool num = ability._skill && action.type == Characters.Actions.Action.Type.Skill;
						bool flag = ability._dash && action.type == Characters.Actions.Action.Type.Dash;
						if (num || flag)
						{
							action.cooldown.time.remainTime -= ability._timeToReduce;
						}
					}
				}
			}
		}

		public ITrigger trigger;

		[SerializeField]
		private float _timeToReduce;

		[SerializeField]
		private bool _skill = true;

		[SerializeField]
		private bool _dash;

		public ReduceCooldownByTrigger()
		{
		}

		public ReduceCooldownByTrigger(ITrigger trigger)
		{
			this.trigger = trigger;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
