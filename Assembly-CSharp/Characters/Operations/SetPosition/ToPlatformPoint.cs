using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToPlatformPoint : Policy
	{
		private enum Point
		{
			Left,
			Center,
			Right
		}

		[SerializeField]
		private Character _owner;

		[SerializeField]
		private Point _point;

		public override Vector2 GetPosition()
		{
			Bounds bounds = _owner.movement.controller.collisionState.lastStandingCollider.bounds;
			switch (_point)
			{
			case Point.Left:
				return new Vector2(ClampX(_owner, bounds.min.x, bounds), bounds.max.y);
			case Point.Center:
				return new Vector2(ClampX(_owner, bounds.center.x, bounds), bounds.max.y);
			case Point.Right:
				return new Vector2(ClampX(_owner, bounds.max.x, bounds), bounds.max.y);
			default:
				return _owner.transform.position;
			}
		}

		private float ClampX(Character owner, float x, Bounds platform)
		{
			if (x <= platform.min.x + owner.collider.size.x)
			{
				x = platform.min.x + owner.collider.size.x;
			}
			else if (x >= platform.max.x - owner.collider.size.x)
			{
				x = platform.max.x - owner.collider.size.x;
			}
			return x;
		}
	}
}
