using System.Collections;
using Characters.AI;
using UnityEngine;

namespace Characters.Operations
{
	public class Leap2 : CharacterOperation
	{
		[SerializeField]
		private AIController _aiController;

		[SerializeField]
		private Transform _target;

		[SerializeField]
		private float _checkDistance = 0.5f;

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
			while (true)
			{
				float num = destination - source;
				Vector2 normalizedDirection = ((num > 0f) ? Vector2.right : Vector2.left);
				owner.movement.Move(normalizedDirection);
				if (num > 0f)
				{
					if (owner.transform.position.x + normalizedDirection.x * _checkDistance > destination)
					{
						break;
					}
				}
				else if (owner.transform.position.x + normalizedDirection.x * _checkDistance < destination)
				{
					break;
				}
				yield return null;
			}
		}
	}
}
