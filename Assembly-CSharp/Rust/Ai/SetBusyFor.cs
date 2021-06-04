using Apex.Serialization;

namespace Rust.Ai
{
	public class SetBusyFor : BaseAction
	{
		[ApexSerialization]
		public float BusyTime;

		public override void DoExecute(BaseContext c)
		{
			c.AIAgent.SetBusyFor(BusyTime);
		}
	}
}
