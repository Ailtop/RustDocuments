using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public sealed class EntityDangerLevel : WeightedScorerBase<BaseEntity>
	{
		[ApexSerialization]
		public float MinScore;

		public override float GetScore(BaseContext c, BaseEntity target)
		{
			foreach (Memory.SeenInfo item in c.Memory.All)
			{
				if (item.Entity == target)
				{
					return Mathf.Max(item.Danger, MinScore);
				}
			}
			return MinScore;
		}
	}
}
