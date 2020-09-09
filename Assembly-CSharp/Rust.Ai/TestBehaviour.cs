using Apex.Serialization;

namespace Rust.Ai
{
	public sealed class TestBehaviour : BaseScorer
	{
		[ApexSerialization]
		public BaseNpc.Behaviour Behaviour;

		public override float GetScore(BaseContext c)
		{
			return (c.AIAgent.CurrentBehaviour == Behaviour) ? 1 : 0;
		}
	}
}
