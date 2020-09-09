namespace Rust.Ai
{
	public class IsTargetAlive : BaseScorer
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
			if (c.Human.AttackTarget != null && !c.Human.AttackTarget.IsDestroyed && ((c.EnemyPlayer != null && !c.EnemyPlayer.IsDead()) || (c.EnemyNpc != null && !c.EnemyNpc.IsDead())))
			{
				return (c.Human.AttackTarget.ServerPosition - c.Human.ServerPosition).sqrMagnitude < c.Human.Stats.DeaggroRange * c.Human.Stats.DeaggroRange;
			}
			return false;
		}
	}
}
