using Apex.Serialization;

namespace Rust.Ai
{
	public class IsClosestPlayerWithinDistance : BaseScorer
	{
		[ApexSerialization]
		private float distance = 4f;

		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				if (!Test(nPCHumanContext, distance))
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}

		public static bool Test(NPCHumanContext c, float distance)
		{
			if (c != null && c.ClosestPlayer != null)
			{
				return (c.ClosestPlayer.ServerPosition - c.Position).sqrMagnitude < distance * distance;
			}
			return false;
		}
	}
}
