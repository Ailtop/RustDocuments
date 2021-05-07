using System;
using Characters.Actions;
using UnityEngine;

namespace Characters.Abilities.Triggers
{
	[Serializable]
	public class OnChargeAction : Trigger
	{
		[SerializeField]
		private ActionTypeBoolArray _types;

		private Character _character;

		public OnChargeAction()
		{
		}

		public OnChargeAction(ActionTypeBoolArray types)
		{
			_types = types;
		}

		public override void Attach(Character character)
		{
			_character = character;
			Character character2 = _character;
			character2.onStartCharging = (Action<Characters.Actions.Action>)Delegate.Combine(character2.onStartCharging, new Action<Characters.Actions.Action>(OnCharacterCharging));
		}

		public override void Detach()
		{
			Character character = _character;
			character.onStartCharging = (Action<Characters.Actions.Action>)Delegate.Remove(character.onStartCharging, new Action<Characters.Actions.Action>(OnCharacterCharging));
		}

		private void OnCharacterCharging(Characters.Actions.Action action)
		{
			if (_types.GetOrDefault(action.type))
			{
				Invoke();
			}
		}
	}
}
