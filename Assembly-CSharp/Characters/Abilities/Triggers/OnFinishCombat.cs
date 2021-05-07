using System;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnFinishCombat : Trigger
	{
		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			_character.playerComponents.combatDetector.onFinishCombat += base.Invoke;
		}

		public override void Detach()
		{
			_character.playerComponents.combatDetector.onFinishCombat -= base.Invoke;
		}
	}
}
