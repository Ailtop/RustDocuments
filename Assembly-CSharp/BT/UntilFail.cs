namespace BT
{
	public class UntilFail : Decorator
	{
		protected override NodeState UpdateDeltatime(Context context)
		{
			if (_subTree.Tick(context) != 0)
			{
				return NodeState.Running;
			}
			return NodeState.Success;
		}
	}
}
