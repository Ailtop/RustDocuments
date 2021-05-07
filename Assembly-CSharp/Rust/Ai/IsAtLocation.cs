using Apex.Serialization;

namespace Rust.Ai
{
	public class IsAtLocation : BaseScorer
	{
		[ApexSerialization]
		public AiLocationSpawner.SquadSpawnerLocation Location;

		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				if (!Test(nPCHumanContext, Location))
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}

		public static bool Test(NPCHumanContext c, AiLocationSpawner.SquadSpawnerLocation location)
		{
			if (c.AiLocationManager != null)
			{
				return c.AiLocationManager.LocationType == location;
			}
			return false;
		}
	}
}
