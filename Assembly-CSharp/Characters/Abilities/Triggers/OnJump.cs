using System;
using Characters.Movements;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnJump : Trigger
	{
		[SerializeField]
		private JumpTypeBoolArray _types;

		private Character _character;

		public OnJump()
		{
		}

		public OnJump(JumpTypeBoolArray types)
		{
			_types = types;
		}

		public override void Attach(Character character)
		{
			_character = character;
			_character.movement.onJump += OnCharacterJump;
		}

		private void OnCharacterJump(Movement.JumpType jumpType, float jumpHeight)
		{
			if (_types[jumpType])
			{
				Invoke();
			}
		}

		public override void Detach()
		{
			_character.movement.onJump -= OnCharacterJump;
		}
	}
}
