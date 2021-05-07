using Characters.AI;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations
{
	public class ShiftObject : CharacterOperation
	{
		[SerializeField]
		private AIController _controller;

		[SerializeField]
		private Transform _object;

		[SerializeField]
		private float _offsetY;

		[SerializeField]
		private float _offsetX;

		[SerializeField]
		private bool _lastStandingPlatform = true;

		[SerializeField]
		private bool _fromPlatform;

		[SerializeField]
		private bool _underTheCeiling;

		private static NonAllocCaster caster;

		static ShiftObject()
		{
			caster = new NonAllocCaster(1);
		}

		public override void Run(Character owner)
		{
			Character target = _controller.target;
			if (!(target == null))
			{
				Collider2D platform = GetPlatform();
				Bounds bounds = ((platform != null) ? platform.bounds : target.movement.controller.collisionState.lastStandingCollider.bounds);
				float x = target.transform.position.x + _offsetX;
				float y;
				if (_underTheCeiling)
				{
					y = (_fromPlatform ? GetClosestCeiling(_offsetY, new Vector2(target.transform.position.x, bounds.max.y)) : GetClosestCeiling(_offsetY, target.transform.position));
				}
				else
				{
					y = (_fromPlatform ? bounds.max.y : target.transform.position.y);
					y += _offsetY;
				}
				_object.position = new Vector2(x, y);
			}
		}

		private float GetClosestCeiling(float distance, Vector3 from)
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(from, Vector2.up, distance, Layers.groundMask);
			if ((bool)raycastHit2D)
			{
				return raycastHit2D.point.y;
			}
			return from.y + distance;
		}

		private Collider2D GetPlatform()
		{
			if (_lastStandingPlatform)
			{
				return null;
			}
			caster.contactFilter.SetLayerMask(Layers.groundMask);
			NonAllocCaster nonAllocCaster = caster.BoxCast(_controller.target.transform.position, _controller.target.collider.bounds.size, 0f, Vector2.down, 100f);
			if (nonAllocCaster.results.Count == 0)
			{
				return null;
			}
			return nonAllocCaster.results[0].collider;
		}
	}
}
