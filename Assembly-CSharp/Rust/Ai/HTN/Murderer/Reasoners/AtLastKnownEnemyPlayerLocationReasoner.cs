using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class AtLastKnownEnemyPlayerLocationReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext != null)
			{
				if (murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null && (MurdererDomain.MurdererNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(murdererContext) - murdererContext.Body.transform.position).sqrMagnitude < 1f)
				{
					murdererContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 1);
				}
				else
				{
					murdererContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 0);
				}
			}
		}
	}
}
