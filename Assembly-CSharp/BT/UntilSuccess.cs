namespace BT
{
	public class UntilSuccess : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			if (_subTree.Tick(context) != NodeState.Success)
			{
				return NodeState.Running;
			}
			return NodeState.Success;
		}
	}
}
