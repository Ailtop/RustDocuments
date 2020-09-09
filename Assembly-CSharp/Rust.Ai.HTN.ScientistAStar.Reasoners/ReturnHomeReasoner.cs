using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class ReturnHomeReasoner : INpcReasoner
	{
		public float TickFrequency
		{
			get;
			set;
		}

		public float LastTickTime
		{
			get;
			set;
		}

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistAStarContext scientistAStarContext = npc.AiDomain.NpcContext as ScientistAStarContext;
			if (scientistAStarContext == null)
			{
				return;
			}
			if (!scientistAStarContext.IsFact(Facts.IsReturningHome))
			{
				if (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null || time - scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.Time > scientistAStarContext.Body.AiDefinition.Memory.NoSeeReturnToSpawnTime)
				{
					scientistAStarContext.SetFact(Facts.IsReturningHome, true);
				}
			}
			else if (scientistAStarContext.IsFact(Facts.CanSeeEnemy) || time - scientistAStarContext.Body.lastAttackedTime < 2f || scientistAStarContext.IsFact(Facts.AtLocationHome))
			{
				scientistAStarContext.SetFact(Facts.IsReturningHome, false);
			}
		}
	}
}
