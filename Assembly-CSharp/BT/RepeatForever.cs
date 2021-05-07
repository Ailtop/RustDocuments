namespace BT
{
	public class RepeatForever : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			if (_subTree.Tick(context) != NodeState.Running)
			{
				_subTree.node.ResetState();
			}
			return NodeState.Running;
		}
	}
}
