using System;
using System.Collections;
using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	[Obsolete("삭제될 예정입니다. UpdateInvoker를 사용해주세요.")]
	public class OnceInvoke : MonoBehaviour
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		private void Start()
		{
			StartCoroutine(CRun());
		}

		public IEnumerator CRun()
		{
			while (!_trigger.isSatisfied())
			{
				yield return null;
			}
			_execute.Run();
		}
	}
}
