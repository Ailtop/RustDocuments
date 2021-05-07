using UnityEngine;

namespace BT
{
	public sealed class Cooldown : Decorator
	{
		[SerializeField]
		private CustomFloat _durationRange;

		private float _duration;

		private float _startTimeStamp;

		private bool _onCoolDown;

		protected override void OnInitialize()
		{
			_duration = _durationRange.value;
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			if (!_onCoolDown)
			{
				return RegularBehaviour(context);
			}
			return CoolDownBehaviour(context);
		}

		private NodeState RegularBehaviour(Context context)
		{
			NodeState num = _subTree.Tick(context);
			if (num == NodeState.Success)
			{
				EnterCoolDown();
			}
			return num;
		}

		private NodeState CoolDownBehaviour(Context context)
		{
			if (Time.time - _startTimeStamp >= _duration)
			{
				ExitCoolDown();
				return RegularBehaviour(context);
			}
			return NodeState.Fail;
		}

		private void EnterCoolDown()
		{
			_startTimeStamp = Time.time;
			_onCoolDown = true;
		}

		private void ExitCoolDown()
		{
			_onCoolDown = false;
		}

		protected override void OnTerminate(NodeState state)
		{
			base.OnTerminate(state);
		}
	}
}
