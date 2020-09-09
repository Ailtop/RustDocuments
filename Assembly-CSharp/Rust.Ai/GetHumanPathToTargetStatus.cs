using Apex.Serialization;
using UnityEngine.AI;

namespace Rust.Ai
{
	public class GetHumanPathToTargetStatus : BaseScorer
	{
		[ApexSerialization]
		public NavMeshPathStatus Status;

		public override float GetScore(BaseContext c)
		{
			return Evaluate(c as NPCHumanContext, Status) ? 1 : 0;
		}

		public static bool Evaluate(NPCHumanContext c, NavMeshPathStatus s)
		{
			byte fact = c.GetFact(NPCPlayerApex.Facts.PathToTargetStatus);
			return c.Human.ToPathStatus(fact) == s;
		}
	}
}
