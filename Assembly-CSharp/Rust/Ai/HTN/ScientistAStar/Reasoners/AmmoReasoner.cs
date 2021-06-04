using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class AmmoReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			HTNPlayer hTNPlayer = npc as HTNPlayer;
			if (hTNPlayer == null)
			{
				return;
			}
			AttackEntity attackEntity = hTNPlayer.GetHeldEntity() as AttackEntity;
			if ((bool)attackEntity)
			{
				BaseProjectile baseProjectile = attackEntity as BaseProjectile;
				if (baseProjectile != null)
				{
					float num = (float)baseProjectile.primaryMagazine.contents / (float)baseProjectile.primaryMagazine.capacity;
					if (num > 0.9f)
					{
						scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.FullClip);
					}
					else if (num > 0.6f)
					{
						scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.HighClip);
					}
					else if (num > 0.17f)
					{
						scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.MediumClip);
					}
					else if (num > 0f)
					{
						scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.LowAmmo);
					}
					else
					{
						scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.EmptyClip);
					}
					return;
				}
			}
			scientistAStarContext.SetFact(Facts.AmmoState, AmmoState.DontRequireAmmo);
		}
	}
}
