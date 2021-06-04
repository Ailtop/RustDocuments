namespace Rust.Ai
{
	public class HasHideout : BaseScorer
	{
		public override float GetScore(BaseContext context)
		{
			NPCHumanContext nPCHumanContext = context as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				if (nPCHumanContext.EnemyHideoutGuess == null)
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}
	}
}
