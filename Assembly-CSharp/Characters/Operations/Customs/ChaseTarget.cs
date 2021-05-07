using System.Collections;
using Characters.AI;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class ChaseTarget : CharacterOperation
	{
		[SerializeField]
		private AIController _ai;

		[SerializeField]
		private float _duration;

		[SerializeField]
		private bool _lookTarget;

		private Coroutine _cExpire;

		private const float epsilon = 1f;

		public override void Run(Character owner)
		{
			_cExpire = StartCoroutine(CRun(owner, _ai.target));
		}

		private IEnumerator CRun(Character owner, Character target)
		{
			float elpased = 0f;
			while (elpased <= _duration)
			{
				float num = owner.transform.position.x - target.transform.position.x;
				if (Mathf.Abs(num) > 1f)
				{
					owner.movement.Move((num > 0f) ? Vector2.left : Vector2.right);
				}
				if (_lookTarget)
				{
					owner.DesireToLookAt(target.transform.position.x);
				}
				yield return null;
			}
		}

		public override void Stop()
		{
			base.Stop();
			if (_cExpire != null)
			{
				StopCoroutine(_cExpire);
			}
		}
	}
}
