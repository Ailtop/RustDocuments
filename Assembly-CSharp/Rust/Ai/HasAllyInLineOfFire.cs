using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai
{
	public class HasAllyInLineOfFire : BaseScorer
	{
		public override float GetScore(BaseContext ctx)
		{
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext != null)
			{
				Scientist scientist = nPCHumanContext.Human as Scientist;
				List<Scientist> allies;
				if (scientist != null && scientist.GetAlliesInRange(out allies) > 0)
				{
					foreach (Scientist item in allies)
					{
						Vector3 vector = nPCHumanContext.EnemyPosition - nPCHumanContext.Position;
						Vector3 vector2 = item.Entity.ServerPosition - nPCHumanContext.Position;
						if (vector2.sqrMagnitude < vector.sqrMagnitude && Vector3.Dot(vector.normalized, vector2.normalized) > 0.9f)
						{
							return 1f;
						}
					}
				}
			}
			return 0f;
		}
	}
}
