using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Magician
{
	public class KeepDistance : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestinationWithFly))]
		private MoveToDestinationWithFly _moveToDestinationWithFly;

		[SerializeField]
		[MinMaxSlider(0f, 30f)]
		private Vector2Int _distance;

		[SerializeField]
		private float _minDistanceWithSide;

		[SerializeField]
		private Action _backMotion;

		[SerializeField]
		private Action _frontMotion;

		private Action _motion;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character target = controller.target;
			int num = Random.Range(_distance.x, _distance.y);
			float moveDirection = GetMoveDirection(character.transform.position);
			SetMotion(character.transform.position, moveDirection, target.transform.position, character);
			SetDestination(controller, character.transform.position, moveDirection, num);
			_motion.TryStart();
			character.ForceToLookAt(character.transform.position.x + moveDirection);
			yield return _moveToDestinationWithFly.CRun(controller);
			if (!controller.stuned && !controller.dead)
			{
				character.CancelAction();
			}
			base.result = Result.Done;
		}

		private float GetMoveDirection(Vector3 origin)
		{
			Vector2 direction = (MMMaths.RandomBool() ? Vector2.right : Vector2.left);
			if ((bool)Physics2D.Raycast(origin, direction, _minDistanceWithSide, Layers.groundMask))
			{
				direction *= -1f;
			}
			return direction.x;
		}

		private void SetMotion(Vector2 origin, float direction, Vector2 target, Character character)
		{
			if (target.x - origin.x > 0f && direction > 0f)
			{
				_motion = _frontMotion;
			}
			else if (target.x - origin.x > 0f && direction < 0f)
			{
				_motion = _backMotion;
			}
			else if (target.x - origin.x < 0f && direction > 0f)
			{
				_motion = _backMotion;
			}
			else if (target.x - origin.x < 0f && direction < 0f)
			{
				_motion = _frontMotion;
			}
			else
			{
				_motion = _frontMotion;
			}
		}

		private void SetDestination(AIController controller, Vector2 origin, float direction, float distance)
		{
			Character character = controller.character;
			RaycastHit2D point;
			if (character == null)
			{
				controller.destination = new Vector2(origin.x, origin.y);
			}
			else if (character.movement.TryBelowRayCast(character.movement.controller.terrainMask, out point, 20f))
			{
				Collider2D collider = point.collider;
				if (direction > 0f)
				{
					if (origin.x + direction * distance > collider.bounds.max.x - 1f)
					{
						controller.destination = new Vector2(collider.bounds.max.x - 1f, origin.y);
					}
					else
					{
						controller.destination = new Vector2(origin.x + direction * distance, origin.y);
					}
				}
				else if (origin.x + direction * distance < collider.bounds.min.x + 1f)
				{
					controller.destination = new Vector2(collider.bounds.min.x + 1f, origin.y);
				}
				else
				{
					controller.destination = new Vector2(origin.x + direction * distance, origin.y);
				}
			}
			else
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, new Vector2(direction, 0f), distance, Layers.terrainMask);
				float num = ((distance > 0f) ? (-0.5f) : 0.5f);
				controller.destination = new Vector2(origin.x + direction * distance + num, origin.y);
				if ((bool)raycastHit2D)
				{
					float x = controller.target.movement.controller.collisionState.lastStandingCollider.bounds.center.x;
					int num2 = ((!(raycastHit2D.point.x > x)) ? 1 : (-1));
					controller.destination = new Vector2(raycastHit2D.point.x + (float)num2, origin.y);
				}
			}
		}
	}
}
