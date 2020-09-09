namespace Rust.Ai
{
	public class NeverMoves : BaseScorer
	{
		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				if (!Test(nPCHumanContext))
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}

		public static bool Test(NPCHumanContext c)
		{
			return c.Human.NeverMove;
		}
	}
}
