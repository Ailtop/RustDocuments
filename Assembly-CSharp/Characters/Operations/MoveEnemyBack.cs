using System.Collections.Generic;
using Characters.Movements;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;

namespace Characters.Operations
{
	public class MoveEnemyBack : CharacterOperation
	{
		[SerializeField]
		private bool _optimizeCollider = true;

		[SerializeField]
		private Collider2D _collider2D;

		[SerializeField]
		private float _distance = 1f;

		private static readonly NonAllocOverlapper _enemyOverlapper;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _onSuccess;

		static MoveEnemyBack()
		{
			_enemyOverlapper = new NonAllocOverlapper(31);
			_enemyOverlapper.contactFilter.SetLayerMask(1024);
		}

		public override void Run(Character owner)
		{
			Character character = FindClosestPlayerBody(owner, _collider2D);
			if (character == null)
			{
				return;
			}
			if (character.movement.config.type == Characters.Movements.Movement.Config.Type.Walking)
			{
				Vector2 destination;
				Vector2 direction;
				if (character.lookingDirection == Character.LookingDirection.Right)
				{
					destination = new Vector2(character.transform.position.x - _distance, character.transform.position.y);
					direction = Vector2.left;
				}
				else
				{
					destination = new Vector2(character.transform.position.x + _distance, character.transform.position.y);
					direction = Vector2.right;
				}
				if (owner.movement.controller.Teleport(destination, direction, _distance))
				{
					owner.ForceToLookAt(character.transform.position.x);
					owner.movement.verticalVelocity = 0f;
					StartCoroutine(_onSuccess.CRun(owner));
				}
				else if (owner.movement.controller.Teleport(character.transform.position, direction, _distance))
				{
					owner.movement.verticalVelocity = 0f;
					StartCoroutine(_onSuccess.CRun(owner));
				}
			}
			else
			{
				Vector2 destination = character.transform.position;
				if (owner.movement.controller.Teleport(destination, _distance))
				{
					owner.movement.verticalVelocity = 0f;
					StartCoroutine(_onSuccess.CRun(owner));
				}
			}
		}

		private void OnDisable()
		{
			_onSuccess?.StopAll();
		}

		private Character FindClosestPlayerBody(Character character, Collider2D collider)
		{
			collider.enabled = true;
			List<Target> components = _enemyOverlapper.OverlapCollider(collider).GetComponents<Target>();
			if (components.Count == 0)
			{
				if (_optimizeCollider)
				{
					collider.enabled = false;
				}
				return null;
			}
			if (components.Count == 1)
			{
				if (_optimizeCollider)
				{
					collider.enabled = false;
				}
				return components[0].character;
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < components.Count; i++)
			{
				if (!(components[i].character == null) && (components[i].character.type == Character.Type.TrashMob || components[i].character.type == Character.Type.Adventurer || components[i].character.type == Character.Type.Boss) && components[i].character.movement.isGrounded)
				{
					float distance = Physics2D.Distance(components[i].character.collider, character.collider).distance;
					if (num > distance)
					{
						index = i;
						num = distance;
					}
				}
			}
			if (_optimizeCollider)
			{
				collider.enabled = false;
			}
			return components[index].character;
		}
	}
}
