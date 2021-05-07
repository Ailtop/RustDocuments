using System;
using Characters.Controllers;
using UnityEngine;

namespace Characters.Actions.Constraints
{
	public class DirectionConstraint : Constraint
	{
		[Flags]
		public enum Direction
		{
			Up = 0x1,
			Down = 0x2,
			Left = 0x4,
			Right = 0x8
		}

		public const float threshold = 0.66f;

		[SerializeField]
		[EnumFlag]
		protected Direction _direcion;

		public override bool Pass()
		{
			return Pass(_action.owner, _direcion);
		}

		public static bool Pass(Character character, Direction direction)
		{
			PlayerInput component = character.GetComponent<PlayerInput>();
			if (component == null)
			{
				return false;
			}
			Direction direction2 = (Direction)0;
			if (component.direction.y > 0.66f)
			{
				direction2 |= Direction.Up;
			}
			if (component.direction.y < -0.66f)
			{
				direction2 |= Direction.Down;
			}
			if (component.direction.x < -0.66f)
			{
				direction2 |= Direction.Left;
			}
			if (component.direction.x > 0.66f)
			{
				direction2 |= Direction.Right;
			}
			return (direction & direction2) == direction;
		}
	}
}
