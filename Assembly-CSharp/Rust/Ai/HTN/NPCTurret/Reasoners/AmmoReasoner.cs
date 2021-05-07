using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.NPCTurret.Reasoners
{
	public class AmmoReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			NPCTurretContext nPCTurretContext = npc.AiDomain.NpcContext as NPCTurretContext;
			if (nPCTurretContext == null)
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
						nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.FullClip);
					}
					else if (num > 0.6f)
					{
						nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.HighClip);
					}
					else if (num > 0.17f)
					{
						nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.MediumClip);
					}
					else if (num > 0f)
					{
						nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.LowAmmo);
					}
					else
					{
						nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.EmptyClip);
					}
					return;
				}
			}
			nPCTurretContext.SetFact(Facts.AmmoState, AmmoState.DontRequireAmmo);
		}
	}
}
