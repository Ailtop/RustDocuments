namespace BT
{
	public class AutoReset : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			return _subTree.Tick(context);
		}

		protected override void OnTerminate(NodeState state)
		{
			_subTree.node.ResetState();
		}
	}
}
