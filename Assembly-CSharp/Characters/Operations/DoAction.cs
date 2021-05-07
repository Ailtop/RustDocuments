using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.Operations
{
	public class DoAction : Operation
	{
		[SerializeField]
		private Action _action;

		[Tooltip("Constraints, Cooldown 등에 의해 action 실행에 실패할 경우 성공할 때까지 혹은 stop될 때까지 반복합니다. 예를 들어 IdleConstraint와 함께 사용하면 Idle 상태가 되자마자 실행됩니다.")]
		[SerializeField]
		private bool _repeatUntilSuccess;

		public override void Run()
		{
			if (!_action.TryStart() && _repeatUntilSuccess)
			{
				StopAllCoroutines();
				StartCoroutine(CRun());
			}
		}

		private IEnumerator CRun()
		{
			do
			{
				yield return null;
			}
			while (!_action.TryStart());
		}

		public override void Stop()
		{
			base.Stop();
			StopAllCoroutines();
		}
	}
}
