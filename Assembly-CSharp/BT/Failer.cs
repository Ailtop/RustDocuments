namespace BT
{
	public class Failer : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			NodeState nodeState = _subTree.Tick(context);
			if (nodeState == NodeState.Success || nodeState == NodeState.Fail)
			{
				return NodeState.Fail;
			}
			return nodeState;
		}
	}
}
