using Characters.Abilities.Triggers;
using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities
{
	public class OperationByTriggerComponent : AbilityComponent<OperationByTrigger>
	{
		[SerializeField]
		[TriggerComponent.Subcomponent]
		private TriggerComponent _triggerComponent;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operations;

		public override void Initialize()
		{
			_ability.trigger = _triggerComponent;
			_ability.operations = _operations.components;
			base.Initialize();
		}
	}
}
