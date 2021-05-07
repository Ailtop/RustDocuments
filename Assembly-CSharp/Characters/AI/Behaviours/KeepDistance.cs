using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class KeepDistance : Behaviour
	{
		private enum Type
		{
			Move,
			MoveToDistanceWithTarget,
			BackStepFromTarget,
			BackStepToWide
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private float _moveCooldownTime;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _distance;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BackStep))]
		private BackStep _backStep;

		private bool _moveCanUse = true;

		private void Start()
		{
			_childs = new List<Behaviour> { _moveToDestination, _backStep };
		}

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			switch (_type)
			{
			case Type.Move:
				yield return MoveToDestination(controller);
				break;
			case Type.BackStepFromTarget:
				yield return BackStepFromTarget(controller);
				break;
			case Type.BackStepToWide:
				yield return BackStepToWide(controller);
				break;
			}
			base.result = Result.Done;
		}

		private IEnumerator MoveToDestination(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			if (!(character.movement.controller.collisionState.lastStandingCollider == null))
			{
				Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
				Vector2 vector = ((target.transform.position.x - character.transform.position.x > 0f) ? Vector2.left : Vector2.right);
				float num = Random.Range(_distance.x, _distance.y);
				float x = ((vector == Vector2.left) ? Mathf.Max(bounds.min.x, character.transform.position.x - num) : Mathf.Min(bounds.max.x, character.transform.position.x + num));
				Vector3 position = character.transform.position;
				if (vector == Vector2.right && bounds.max.x < position.x + num)
				{
					x = Mathf.Max(bounds.min.x, position.x - num);
				}
				else if (vector == Vector2.left && bounds.min.x > position.x - num)
				{
					x = Mathf.Min(bounds.max.x, position.x + num);
				}
				controller.destination = new Vector2(x, 0f);
				_moveCanUse = false;
				StartCoroutine(CCheckMoveCoolDown(controller.character.chronometer.master));
				yield return _moveToDestination.CRun(controller);
			}
		}

		private IEnumerator BackStepFromTarget(AIController controller)
		{
			Character character = controller.character;
			Vector2 vector = ((controller.target.transform.position.x - character.transform.position.x > 0f) ? Vector2.right : Vector2.left);
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			float num = Random.Range(_distance.x, _distance.y);
			if (vector == Vector2.right && bounds.min.x > character.transform.position.x - num)
			{
				character.ForceToLookAt(Character.LookingDirection.Left);
			}
			else if (vector == Vector2.left && bounds.max.x < character.transform.position.x + num)
			{
				character.ForceToLookAt(Character.LookingDirection.Right);
			}
			yield return _backStep.CRun(controller);
		}

		private IEnumerator BackStepToWide(AIController controller)
		{
			Character character = controller.character;
			Character target = controller.target;
			Bounds targetPlatformBounds = target.movement.controller.collisionState.lastStandingCollider.bounds;
			Vector2 move = ((targetPlatformBounds.center.x > character.transform.position.x) ? Vector2.right : Vector2.left);
			character.movement.move = move;
			yield return _backStep.CRun(controller);
			if (targetPlatformBounds.center.x > character.transform.position.x)
			{
				character.lookingDirection = Character.LookingDirection.Left;
			}
			else
			{
				character.lookingDirection = Character.LookingDirection.Right;
			}
		}

		private IEnumerator CCheckMoveCoolDown(Chronometer chronometer)
		{
			yield return chronometer.WaitForSeconds(_moveCooldownTime);
			_moveCanUse = true;
		}

		public bool CanUseBackStep()
		{
			return _backStep.CanUse();
		}

		public bool CanUseBackMove()
		{
			return _moveCanUse;
		}
	}
}
