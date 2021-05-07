using System.Collections;
using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class Leap : CharacterOperation
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private Transform _target;

		[SerializeField]
		private float _duration;

		private void Awake()
		{
			if (_target != null)
			{
				_target.transform.parent = null;
			}
		}

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
			float destination = ((_target == null) ? _aiController.target.transform.position.x : _target.transform.position.x);
			float source = owner.transform.position.x;
			float elapsed = 0f;
			while (true)
			{
				yield return null;
				float num = Mathf.Lerp(source, destination, elapsed / _duration);
				owner.movement.force.x = num - owner.transform.position.x;
				if (!owner.stunedOrFreezed)
				{
					if (elapsed > _duration)
					{
						break;
					}
					elapsed += owner.chronometer.master.deltaTime;
				}
			}
		}
	}
}
