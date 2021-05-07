using System;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnGrounded : Trigger
	{
		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			_character.movement.onGrounded += base.Invoke;
		}

		public override void Detach()
		{
			_character.movement.onGrounded -= base.Invoke;
		}
	}
}
