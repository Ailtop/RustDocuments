using UnityEngine;

namespace Characters.Operations.Movement
{
	public class ChangeGravity : CharacterOperation
	{
		[SerializeField]
		private float _gravirty;

		private Character character;

		private float _originalGravity;

		public override void Run(Character owner)
		{
			character = owner;
			_originalGravity = owner.movement.config.gravity;
			owner.movement.config.gravity = _gravirty;
		}

		public override void Stop()
		{
			if (character != null)
			{
				character.movement.config.gravity = _originalGravity;
			}
		}
	}
}
