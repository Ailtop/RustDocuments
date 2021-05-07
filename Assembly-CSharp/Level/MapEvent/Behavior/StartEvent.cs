using UnityEngine;
using UnityEngine.Events;

namespace Level.MapEvent.Behavior
{
	public class StartEvent : Behavior
	{
		[SerializeField]
		private UnityEvent _event;

		public override void Run()
		{
			_event.Invoke();
		}
	}
}
