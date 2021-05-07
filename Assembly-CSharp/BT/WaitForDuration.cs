using UnityEngine;

namespace BT
{
	public class WaitForDuration : Node
	{
		[SerializeField]
		private CustomFloat _durationRange;

		private float _duration;

		private float _elapsed;

		protected override void OnInitialize()
		{
			_duration = _durationRange.value;
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			_elapsed += context.deltaTime;
			if (_elapsed >= _duration)
			{
				return NodeState.Success;
			}
			return NodeState.Running;
		}

		protected override void OnTerminate(NodeState state)
		{
			_elapsed = 0f;
			base.OnTerminate(state);
		}

		protected override void DoReset(NodeState state)
		{
			_elapsed = 0f;
			base.DoReset(state);
		}
	}
}
