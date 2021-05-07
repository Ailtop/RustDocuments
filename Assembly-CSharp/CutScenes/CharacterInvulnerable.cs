using Runnables;
using UnityEngine;

namespace CutScenes
{
	public class CharacterInvulnerable : State
	{
		[SerializeField]
		private Target _target;

		public override void Attach()
		{
			_target.character.invulnerable.Attach(State.key);
		}

		public override void Detach()
		{
			if (_target != null && _target.character != null)
			{
				_target.character.invulnerable.Detach(State.key);
			}
		}

		private void OnDisable()
		{
			Detach();
		}

		private void OnDestroy()
		{
			if (_target != null && !(_target.character == null))
			{
				_target.character.invulnerable.Detach(State.key);
			}
		}
	}
}
