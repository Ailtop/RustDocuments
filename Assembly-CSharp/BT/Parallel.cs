using UnityEngine;

namespace BT
{
	public class Parallel : Composite
	{
		[SerializeField]
		private int _successCount;

		[SerializeField]
		private int _failCount;

		protected override NodeState UpdateDeltatime(Context context)
		{
			return NodeState.Running;
		}

		protected override void DoReset(NodeState state)
		{
			base.DoReset(state);
		}
	}
}
