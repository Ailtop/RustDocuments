using Apex.Serialization;

namespace Rust.Ai
{
	public class HasHumanFactBoolean : BaseScorer
	{
		[ApexSerialization]
		public NPCPlayerApex.Facts fact;

		[ApexSerialization(defaultValue = false)]
		public bool value;

		public override float GetScore(BaseContext c)
		{
			byte b = (byte)(value ? 1 : 0);
			if (c.GetFact(fact) != b)
			{
				return 0f;
			}
			return 1f;
		}
	}
}
