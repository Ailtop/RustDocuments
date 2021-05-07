using UnityEngine;

namespace BT
{
	public class TimeLimit : Decorator
	{
		[SerializeField]
		private CustomFloat _durationRange;

		private float _elapsed;

		private float _duration;

		protected override void OnInitialize()
		{
			_duration = _durationRange.value;
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			float deltaTime = context.deltaTime;
			_elapsed += deltaTime;
			if (_elapsed >= _duration)
			{
				return NodeState.Fail;
			}
			return _subTree.Tick(context);
		}

		protected override void DoReset(NodeState state)
		{
			_elapsed = 0f;
			base.DoReset(state);
		}
	}
}
