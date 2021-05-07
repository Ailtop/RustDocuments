using Apex.Serialization;
using UnityEngine.AI;

namespace Rust.Ai
{
	public class HasPathStatus : BaseScorer
	{
		[ApexSerialization]
		private NavMeshPathStatus Status;

		public override float GetScore(BaseContext c)
		{
			if (c.AIAgent.IsNavRunning())
			{
				if (c.AIAgent.GetNavAgent.pathStatus != Status)
				{
					return 0f;
				}
				return 1f;
			}
			return 0f;
		}
	}
}
