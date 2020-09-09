using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class AtLastKnownEnemyPlayerLocationReasoner : INpcReasoner
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
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext != null)
			{
				if (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null && (ScientistDomain.NavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(scientistContext) - scientistContext.Body.transform.position).sqrMagnitude < 1f)
				{
					scientistContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 1);
				}
				else
				{
					scientistContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 0);
				}
			}
		}
	}
}
