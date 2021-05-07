using System;
using System.Runtime.CompilerServices;
using Characters.Projectiles.Movements.SubMovements;
using UnityEngine;

namespace Characters.Projectiles.Movements
{
	public class Leap : Movement
	{
		[SerializeField]
		private TargetFinder _finder;

		[SerializeField]
		private SubMovement _subMovement;

		[SerializeField]
		private float _duration = 1f;

		private Vector2 _directionVector;

		private float _distance;

		public override void Initialize(Projectile projectile, float direction)
		{
			if (_finder.range != null)
			{
				_finder.Initialize(projectile);
				Target target = _finder.Find();
				if (target != null)
				{
					Vector2 vector = projectile.transform.position;
					Bounds bounds = target.character.movement.controller.collisionState.lastStandingCollider.bounds;
					RaycastHit2D raycastHit2D = Physics2D.Raycast(target.character.transform.position, Vector2.down, float.PositiveInfinity, Layers.groundMask);
					Vector2 vector2 = ((!raycastHit2D) ? new Vector2(target.transform.position.x, bounds.center.y) : raycastHit2D.point);
					_directionVector = vector2 - vector;
					direction = Mathf.Atan2(_directionVector.y, _directionVector.x) * 57.29578f;
					_distance = Vector2.Distance(vector, vector2);
				}
			}
			base.Initialize(projectile, direction);
			_subMovement.Move(projectile);
		}

		[return: TupleElementNames(new string[] { "direction", "speed" })]
		public override ValueTuple<Vector2, float> GetSpeed(float time, float deltaTime)
		{
			float item = ((time > _duration) ? 0f : _distance);
			return new ValueTuple<Vector2, float>(_directionVector.normalized, item);
		}
	}
}
