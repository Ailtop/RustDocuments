using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
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
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext != null)
			{
				if (bearContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null && (BearDomain.BearNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(bearContext) - bearContext.Body.transform.position).sqrMagnitude < 1f)
				{
					bearContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 1);
				}
				else
				{
					bearContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 0);
				}
			}
		}
	}
}
