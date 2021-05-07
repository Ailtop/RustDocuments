using Characters;
using UnityEngine;

namespace Runnables.Triggers
{
	public class ByPosition : Trigger
	{
		private enum Direction
		{
			Left,
			Right
		}

		[SerializeField]
		private Target _target;

		[SerializeField]
		private Direction _direction;

		[SerializeField]
		private Transform _base;

		protected override bool Check()
		{
			Character character = _target.character;
			if (character == null)
			{
				return false;
			}
			if (_direction == Direction.Left && character.transform.position.x < _base.position.x)
			{
				return true;
			}
			if (_direction == Direction.Right && character.transform.position.x > _base.position.x)
			{
				return true;
			}
			return false;
		}
	}
}
