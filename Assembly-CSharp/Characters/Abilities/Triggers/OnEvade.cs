using System;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnEvade : Trigger
	{
		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			_character.onEvade += OnCharacterEvade;
		}

		public override void Detach()
		{
			_character.onEvade -= OnCharacterEvade;
		}

		private void OnCharacterEvade(ref Damage damage)
		{
			Invoke();
		}
	}
}
