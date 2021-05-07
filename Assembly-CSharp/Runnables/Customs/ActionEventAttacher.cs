using Characters.Actions;
using UnityEngine;

namespace Runnables.Customs
{
	public class ActionEventAttacher : MonoBehaviour
	{
		private enum Type
		{
			OnStart,
			OnEnd
		}

		[SerializeField]
		private Action _action;

		[SerializeField]
		private Type _type;

		[SerializeField]
		private Runnable _execute;

		[SerializeField]
		private bool _once;

		private bool _executed;

		private void OnEnable()
		{
			switch (_type)
			{
			case Type.OnStart:
				_action.onStart += Run;
				break;
			case Type.OnEnd:
				_action.onEnd += Run;
				break;
			}
		}

		private void OnDisable()
		{
			if (!(_action == null))
			{
				switch (_type)
				{
				case Type.OnStart:
					_action.onStart -= Run;
					break;
				case Type.OnEnd:
					_action.onEnd -= Run;
					break;
				}
			}
		}

		private void Run()
		{
			if (!_once || !_executed)
			{
				_execute.Run();
				_executed = true;
			}
		}
	}
}
