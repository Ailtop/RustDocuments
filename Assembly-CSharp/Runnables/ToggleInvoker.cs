using System.Collections;
using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	public class ToggleInvoker : MonoBehaviour
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

		private IEnumerator CRun()
		{
			while (true)
			{
				if (!_trigger.isSatisfied())
				{
					yield return null;
					continue;
				}
				_execute.Run();
				while (_trigger.isSatisfied())
				{
					yield return null;
				}
			}
		}
	}
}
