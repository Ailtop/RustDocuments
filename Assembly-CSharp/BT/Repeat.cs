using UnityEngine;

namespace BT
{
	public class Repeat : Decorator
	{
		[Header("성공이든 실패든 연속 실행 후 성공 반환")]
		[SerializeField]
		private int _count;

		private int _currentCount;

		protected override void OnInitialize()
		{
			_currentCount = 0;
			base.OnInitialize();
		}

		protected override NodeState UpdateDeltatime(Context context)
		{
			NodeState nodeState = _subTree.Tick(context);
			if (nodeState == NodeState.Running)
			{
				return nodeState;
			}
			_currentCount++;
			if (_currentCount < _count)
			{
				return NodeState.Running;
			}
			return NodeState.Success;
		}

		protected override void DoReset(NodeState state)
		{
			_currentCount = 0;
			base.DoReset(state);
		}
	}
}
