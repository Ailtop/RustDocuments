using System.Collections;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class MoveToDestination : CharacterOperation
	{
		[SerializeField]
		private Transform _destination;

		[SerializeField]
		private Curve _curve;

		private CoroutineReference _coroutineReference;

		public override void Run(Character owner)
		{
			if (!(owner.movement == null))
			{
				Vector2 vector = _destination.position - owner.transform.position;
				if (_curve.duration > 0f)
				{
					_coroutineReference.Stop();
					_coroutineReference = owner.StartCoroutineWithReference(CMove(owner, _curve, vector));
				}
				else
				{
					owner.movement.force += vector;
				}
			}
		}

		private IEnumerator CMove(Character character, Curve curve, Vector2 distance)
		{
			float t = 0f;
			float amountBefore = 0f;
			for (; t < curve.duration; t += character.chronometer.animation.deltaTime)
			{
				if (character == null)
				{
					break;
				}
				if (!character.liveAndActive)
				{
					break;
				}
				float num = curve.Evaluate(t);
				character.movement.force += distance * (num - amountBefore);
				amountBefore = num;
				yield return null;
			}
		}

		public override void Stop()
		{
			_coroutineReference.Stop();
		}
	}
}
