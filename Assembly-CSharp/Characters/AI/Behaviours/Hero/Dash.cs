using System;
using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class Dash : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _readyAction;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChainAction))]
		private Characters.Actions.Action _attackAction;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _distanceRange;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		[SerializeField]
		private Curve curve;

		[SerializeField]
		private float _durationMultiplierPerDistance = 1f;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_readyAction.TryStart();
			while (_readyAction.running)
			{
				yield return null;
			}
			SetDestination(controller);
			_attackAction.TryStart();
			yield return CMoveToDestination(controller, controller.character);
			if (!controller.dead)
			{
				controller.character.CancelAction();
			}
			base.result = Result.Success;
		}

		private void SetDestination(AIController controller)
		{
			float num = UnityEngine.Random.Range(_distanceRange.x, _distanceRange.y);
			if (controller.target == null)
			{
				throw new Exception("target is null");
			}
			float y = controller.character.movement.controller.collisionState.lastStandingCollider.bounds.max.y;
			float x = ((!MMMaths.RandomBool()) ? Math.Max(controller.character.movement.controller.collisionState.lastStandingCollider.bounds.min.x, controller.target.transform.position.x - num) : Math.Min(controller.character.movement.controller.collisionState.lastStandingCollider.bounds.max.x, controller.target.transform.position.x + num));
			controller.destination = new Vector2(x, y);
		}

		private IEnumerator CMoveToDestination(AIController controller, Character owner)
		{
			Vector2 destination = controller.destination;
			Vector3 source = owner.transform.position;
			float elapsed = 0f;
			float num = Mathf.Abs(destination.x - source.x);
			float duration = num * owner.stat.GetInterpolatedMovementSpeed() / 60f;
			curve.duration = duration * _durationMultiplierPerDistance;
			for (; elapsed < curve.duration; elapsed += owner.chronometer.master.deltaTime)
			{
				yield return null;
				if (elapsed < duration)
				{
					owner.ForceToLookAt(destination.x);
				}
				float num2 = Mathf.Lerp(source.x, destination.x, curve.Evaluate(elapsed));
				owner.movement.force = new Vector2(num2 - owner.transform.position.x, 0f);
			}
		}
	}
}
