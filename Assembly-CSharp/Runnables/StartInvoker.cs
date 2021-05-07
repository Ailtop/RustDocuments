using Runnables.Triggers;
using UnityEngine;

namespace Runnables
{
	public class StartInvoker : MonoBehaviour
	{
		[SerializeField]
		[Trigger.Subcomponent]
		private Trigger _trigger;

		[SerializeField]
		private Runnable _execute;

		private void Start()
		{
			if (_trigger.isSatisfied())
			{
				_execute.Run();
			}
		}
	}
}
