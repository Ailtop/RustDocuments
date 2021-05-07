using Characters.Abilities;
using UnityEngine;

namespace Characters.Operations
{
	public class AttachAbility : CharacterOperation
	{
		[SerializeField]
		[AbilityComponent.Subcomponent]
		private AbilityComponent _abilityComponent;

		private Character _target;

		public override void Initialize()
		{
			_abilityComponent.Initialize();
		}

		public override void Run(Character target)
		{
			_target = target;
			target.ability.Add(_abilityComponent.ability);
		}

		public override void Stop()
		{
			_target?.ability.Remove(_abilityComponent.ability);
		}

		public override string ToString()
		{
			return this.GetAutoName();
		}
	}
}
