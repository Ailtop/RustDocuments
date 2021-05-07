using System.Collections.Generic;
using Characters.Movements;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class PrisonerPhaser : CharacterOperation
	{
		[SerializeField]
		private Collider2D _collider2D;

		[SerializeField]
		private float _distance = 1f;

		private static readonly NonAllocOverlapper _enemyOverlapper;

		static PrisonerPhaser()
		{
			_enemyOverlapper = new NonAllocOverlapper(31);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		public override void Run(Character owner)
		{
			Target target = FindClosestPlayerBody(owner, _collider2D);
			if (target == null)
			{
				return;
			}
			if (target.character != null && target.character.movement.config.type == Characters.Movements.Movement.Config.Type.Walking)
			{
				Vector2 destination;
				Vector2 direction;
				if (target.character.lookingDirection == Character.LookingDirection.Right)
				{
					destination = new Vector2(target.transform.position.x - _distance, target.transform.position.y);
					direction = Vector2.left;
				}
				else
				{
					destination = new Vector2(target.transform.position.x + _distance, target.transform.position.y);
					direction = Vector2.right;
				}
				if (owner.movement.controller.Teleport(destination, direction, _distance))
				{
					owner.ForceToLookAt(target.character.lookingDirection);
					owner.movement.verticalVelocity = 0f;
				}
				else if (owner.movement.controller.Teleport(target.transform.position, direction, _distance))
				{
					owner.movement.verticalVelocity = 0f;
				}
			}
			else
			{
				Vector2 destination;
				Vector2 direction;
				if (MMMaths.RandomBool())
				{
					destination = new Vector2(target.transform.position.x - _distance, target.transform.position.y);
					direction = Vector2.left;
				}
				else
				{
					destination = new Vector2(target.transform.position.x + _distance, target.transform.position.y);
					direction = Vector2.right;
				}
				if (owner.movement.controller.Teleport(destination, direction, _distance))
				{
					owner.movement.verticalVelocity = 0f;
				}
			}
		}

		private Target FindClosestPlayerBody(Character character, Collider2D collider)
		{
			List<Target> components = _enemyOverlapper.OverlapCollider(collider).GetComponents<Target>();
			if (components.Count == 0)
			{
				return null;
			}
			if (components.Count == 1)
			{
				return components[0];
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < components.Count; i++)
			{
				if (!(components[i].character == null) && components[i].character.movement.isGrounded)
				{
					float distance = Physics2D.Distance(components[i].character.collider, character.collider).distance;
					if (num > distance)
					{
						index = i;
						num = distance;
					}
				}
			}
			return components[index];
		}
	}
}
