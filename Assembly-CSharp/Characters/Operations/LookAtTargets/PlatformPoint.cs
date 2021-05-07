using UnityEngine;

namespace Characters.Operations.LookAtTargets
{
	public class PlatformPoint : Target
	{
		private enum Point
		{
			Left,
			Center,
			Right
		}

		[SerializeField]
		private Point _point;

		public override Character.LookingDirection GetDirectionFrom(Character character)
		{
			Collider2D lastStandingCollider = character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider == null)
			{
				return character.lookingDirection;
			}
			float x = character.transform.position.x;
			switch (_point)
			{
			case Point.Left:
			{
				float x4 = lastStandingCollider.bounds.min.x;
				if (x >= x4)
				{
					return Character.LookingDirection.Left;
				}
				return Character.LookingDirection.Right;
			}
			case Point.Center:
			{
				float x3 = lastStandingCollider.bounds.center.x;
				if (x >= x3)
				{
					return Character.LookingDirection.Left;
				}
				return Character.LookingDirection.Right;
			}
			case Point.Right:
			{
				float x2 = lastStandingCollider.bounds.max.x;
				if (x >= x2)
				{
					return Character.LookingDirection.Left;
				}
				return Character.LookingDirection.Right;
			}
			default:
				return character.lookingDirection;
			}
		}
	}
}
