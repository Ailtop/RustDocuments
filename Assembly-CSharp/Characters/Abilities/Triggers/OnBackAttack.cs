using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Characters.Movements;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnBackAttack : Trigger
	{
		private Character _character;

		public override void Attach(Character character)
		{
			_character = character;
			Character character2 = _character;
			character2.onGaveDamage = (GaveDamageDelegate)Delegate.Combine(character2.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		public override void Detach()
		{
			Character character = _character;
			character.onGaveDamage = (GaveDamageDelegate)Delegate.Remove(character.onGaveDamage, new GaveDamageDelegate(OnGaveDamage));
		}

		private void OnGaveDamage(ITarget target, [In][IsReadOnly] ref Damage originalDamage, [In][IsReadOnly] ref Damage tookDamage, double damageDealt)
		{
			if (target.character == null)
			{
				return;
			}
			Movement movement = target.character.movement;
			if ((object)movement == null || movement.config.type != 0)
			{
				int num = Math.Sign(_character.transform.position.x - target.transform.position.x);
				if ((num == -1 && target.character.lookingDirection == Character.LookingDirection.Right) || (num == 1 && target.character.lookingDirection == Character.LookingDirection.Left))
				{
					Invoke();
				}
			}
		}
	}
}
