using UnityEngine;

namespace Characters.Operations.Decorator
{
	public class ByLookingDirection : CharacterOperation
	{
		[SerializeField]
		[Subcomponent]
		private Subcomponents _left;

		[SerializeField]
		[Subcomponent]
		private Subcomponents _right;

		public override void Initialize()
		{
			if (_left != null)
			{
				_left.Initialize();
			}
			if (_right != null)
			{
				_right.Initialize();
			}
		}

		public override void Run(Character owner)
		{
			if (owner.lookingDirection == Character.LookingDirection.Left)
			{
				_left.Stop();
				_left.Run(owner);
			}
			else
			{
				_right.Stop();
				_right.Run(owner);
			}
		}

		public override void Stop()
		{
			if (_left != null)
			{
				_left.Stop();
			}
			if (_right != null)
			{
				_right.Stop();
			}
		}
	}
}
