using Apex.AI;

namespace Rust.Ai
{
	public abstract class BaseAction : ActionBase
	{
		private string DebugName;

		public BaseAction()
		{
			DebugName = GetType().Name;
		}

		public override void Execute(IAIContext context)
		{
			BaseContext baseContext = context as BaseContext;
			if (baseContext != null)
			{
				DoExecute(baseContext);
			}
		}

		public abstract void DoExecute(BaseContext context);
	}
}
