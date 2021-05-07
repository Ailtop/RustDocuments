using Characters.Operations;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	public class BoneOfManaComponent : AbilityComponent<BoneOfMana>
	{
		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation0;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation1;

		[SerializeField]
		[CharacterOperation.Subcomponent]
		private CharacterOperation.Subcomponents _operation2;

		public override void Initialize()
		{
			_ability.operationsByCount = new CharacterOperation[3][] { _operation0.components, _operation1.components, _operation2.components };
			base.Initialize();
		}
	}
}
