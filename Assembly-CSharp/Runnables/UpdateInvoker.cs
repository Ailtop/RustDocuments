using System.Collections;
using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	public class UpdateInvoker : MonoBehaviour
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		[SerializeField]
		private bool _once;

		private void Start()
		{
			if (_once)
			{
				StartCoroutine(CRun());
			}
			else
			{
				StartCoroutine(CUpdate());
			}
		}

		private IEnumerator CUpdate()
		{
			while (true)
			{
				yield return null;
				if (_trigger.isSatisfied())
				{
					_execute.Run();
				}
			}
		}

		private IEnumerator CRun()
		{
			while (!_trigger.isSatisfied())
			{
				yield return null;
			}
			_execute.Run();
		}
	}
}
