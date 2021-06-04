using Apex.Serialization;

namespace Rust.Ai
{
	public sealed class AllowBreaking : BaseAction
	{
		[ApexSerialization]
		public bool Allow;

		public override void DoExecute(BaseContext c)
		{
			c.AIAgent.AutoBraking = Allow;
		}
	}
}
