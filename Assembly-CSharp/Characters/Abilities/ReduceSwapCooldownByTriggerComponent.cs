using Characters.Abilities.Triggers;
using UnityEngine;

namespace Characters.Abilities
{
	public class ReduceSwapCooldownByTriggerComponent : AbilityComponent<ReduceSwapCooldownByTrigger>
	{
		[SerializeField]
		[TriggerComponent.Subcomponent]
		private TriggerComponent _triggerComponent;

		public override void Initialize()
		{
			_ability.trigger = _triggerComponent;
			base.Initialize();
		}
	}
}
