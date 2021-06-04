using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Bear.Reasoners
{
	public class EnemyRangeReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			BearContext bearContext = npc.AiDomain.NpcContext as BearContext;
			if (bearContext == null)
			{
				return;
			}
			if (bearContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
			{
				bearContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
			float sqrMagnitude = (bearContext.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition - bearContext.BodyPosition).sqrMagnitude;
			float sqrCloseRange = bearContext.Body.AiDefinition.Engagement.SqrCloseRange;
			if (sqrMagnitude <= sqrCloseRange)
			{
				bearContext.SetFact(Facts.EnemyRange, EnemyRange.CloseRange);
				return;
			}
			float sqrMediumRange = bearContext.Body.AiDefinition.Engagement.SqrMediumRange;
			if (sqrMagnitude <= sqrMediumRange)
			{
				bearContext.SetFact(Facts.EnemyRange, EnemyRange.MediumRange);
				return;
			}
			float sqrLongRange = bearContext.Body.AiDefinition.Engagement.SqrLongRange;
			if (sqrMagnitude <= sqrLongRange)
			{
				bearContext.SetFact(Facts.EnemyRange, EnemyRange.LongRange);
			}
			else
			{
				bearContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
		}
	}
}
