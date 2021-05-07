using UnityEngine;
using UnityEngine.Events;

namespace Runnables
{
	public class InvokeUnityEvent : Runnable
	{
		[SerializeField]
		private UnityEvent _unityEvent;

		public override void Run()
		{
			_unityEvent?.Invoke();
		}
	}
}
