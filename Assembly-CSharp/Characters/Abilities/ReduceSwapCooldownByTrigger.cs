using System;
using UnityEngine;

namespace Characters.Abilities
{
	[Serializable]
	public class ReduceSwapCooldownByTrigger : Ability
	{
		public class Instance : AbilityInstance<ReduceSwapCooldownByTrigger>
		{
			private float _remainCooldownTime;

			public Instance(Character owner, ReduceSwapCooldownByTrigger ability)
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

			private void OnTriggered()
			{
				owner.playerComponents.inventory.weapon.ReduceSwapCooldown(ability._timeToReduce);
			}
		}

		public ITrigger trigger;

		[SerializeField]
		private float _timeToReduce;

		public ReduceSwapCooldownByTrigger()
		{
		}

		public ReduceSwapCooldownByTrigger(ITrigger trigger)
		{
			this.trigger = trigger;
		}

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
