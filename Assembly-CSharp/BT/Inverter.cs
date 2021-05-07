namespace BT
{
	public class Inverter : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			NodeState nodeState = _subTree.Tick(context);
			switch (nodeState)
			{
			case NodeState.Success:
				return NodeState.Fail;
			default:
				return nodeState;
			case NodeState.Fail:
				return NodeState.Success;
			}
		}
	}
}
