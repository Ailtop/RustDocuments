using System.Collections.Generic;
using UnityEngine;

namespace UI.Pause
{
	public class PauseEventSystem : MonoBehaviour
	{
		[SerializeField]
		[PauseEvent.Subcomponent]
		private PauseEvent _baseEvent;

		private PauseEvent _empty;

		private Stack<PauseEvent> _events;

		private void Awake()
		{
			_events = new Stack<PauseEvent>();
			_empty = base.gameObject.AddComponent<Empty>();
			_events.Push(_baseEvent);
		}

		public void Run()
		{
			if (_events.Count == 0)
			{
				Debug.LogError("Panel이 없습니다.");
			}
			else
			{
				_events.Peek().Invoke();
			}
		}

		public void PushEvent(PauseEvent type)
		{
			_events.Push(type);
		}

		public void PopEvent()
		{
			_events.Pop();
		}

		public void PushEmpty()
		{
			PushEvent(_empty);
		}
	}
}
