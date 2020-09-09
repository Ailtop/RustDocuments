using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
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
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext != null)
			{
				if (scientistJunkpileContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player != null && (ScientistJunkpileDomain.JunkpileNavigateToLastKnownLocationOfPrimaryEnemyPlayer.GetDestination(scientistJunkpileContext) - scientistJunkpileContext.Body.transform.position).sqrMagnitude < 1f)
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 1);
				}
				else
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationLastKnownLocationOfPrimaryEnemyPlayer, 0);
				}
			}
		}
	}
}
