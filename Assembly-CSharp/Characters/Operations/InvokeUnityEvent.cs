using UnityEngine;
using UnityEngine.Events;

namespace Characters.Operations
{
	public sealed class InvokeUnityEvent : Operation
	{
		[SerializeField]
		private UnityEvent _unityEvent;

		public override void Run()
		{
			_unityEvent.Invoke();
		}
	}
}
