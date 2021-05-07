using System.Collections;
using Characters.AI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Operations
{
	public class LeapWithFly : CharacterOperation
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private Transform _target;

		[SerializeField]
		private Curve curve;

		[SerializeField]
		[FormerlySerializedAs("_chaseTime")]
		private float _lookingTime;

		public override void Run(Character owner)
		{
			StartCoroutine(CMoveToTarget(owner));
		}

		public override void Stop()
		{
			StopAllCoroutines();
		}

		private IEnumerator CMoveToTarget(Character owner)
		{
			Vector3 destination = ((_target == null) ? _aiController.target.transform.position : _target.transform.position);
			Vector3 source = owner.transform.position;
			for (float elapsed = 0f; elapsed < curve.duration; elapsed += owner.chronometer.master.deltaTime)
			{
				yield return null;
				if (elapsed < _lookingTime)
				{
					destination = ((_target == null) ? _aiController.target.transform.position : _target.transform.position);
					owner.ForceToLookAt(destination.x);
				}
				Vector2 vector = Vector2.Lerp(source, destination, curve.Evaluate(elapsed));
				owner.movement.force = vector - (Vector2)owner.transform.position;
			}
		}
	}
}
