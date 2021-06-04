namespace Rust.Ai
{
	public class LookAtClosestPlayer : BaseAction
	{
		public override void DoExecute(BaseContext context)
		{
			NPCHumanContext nPCHumanContext = context as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				Do(nPCHumanContext);
			}
		}

		public static void Do(NPCHumanContext c)
		{
			c.Human.LookAtEyes = c.ClosestPlayer.eyes;
		}
	}
}
