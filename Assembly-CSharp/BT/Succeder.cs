namespace BT
{
	public class Succeder : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			NodeState nodeState = _subTree.Tick(context);
			if (nodeState == NodeState.Success || nodeState == NodeState.Fail)
			{
				return NodeState.Success;
			}
			return nodeState;
		}
	}
}
