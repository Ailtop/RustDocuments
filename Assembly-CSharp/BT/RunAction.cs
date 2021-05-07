using Characters.Actions;
using UnityEngine;

namespace BT
{
	public sealed class RunAction : Node
	{
		[SerializeField]
		private Action _action;

		private bool _running;

		protected override NodeState UpdateDeltatime(Context context)
		{
			if (_action.running)
			{
				return NodeState.Running;
			}
			if (_running)
			{
				return NodeState.Success;
			}
			if (!_action.TryStart())
			{
				return NodeState.Fail;
			}
			_running = true;
			return NodeState.Running;
		}

		protected override void OnTerminate(NodeState state)
		{
			_running = false;
			base.OnTerminate(state);
		}

		protected override void DoReset(NodeState state)
		{
			_running = false;
			base.DoReset(state);
		}
	}
}
