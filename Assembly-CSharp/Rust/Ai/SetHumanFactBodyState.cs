using Apex.Serialization;

namespace Rust.Ai
{
	public class SetHumanFactBodyState : BaseAction
	{
		[ApexSerialization(defaultValue = NPCPlayerApex.BodyState.StandingTall)]
		public NPCPlayerApex.BodyState value;

		public override void DoExecute(BaseContext c)
		{
			c.SetFact(NPCPlayerApex.Facts.BodyState, (byte)value, true, false);
		}
	}
}
