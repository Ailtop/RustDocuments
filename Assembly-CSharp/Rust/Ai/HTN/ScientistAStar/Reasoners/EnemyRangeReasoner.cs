using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistAStar.Reasoners
{
	public class EnemyRangeReasoner : INpcReasoner
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
			if (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
			{
				scientistAStarContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
			float sqrMagnitude = (scientistAStarContext.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition - scientistAStarContext.BodyPosition).sqrMagnitude;
			AttackEntity firearm = scientistAStarContext.Domain.GetFirearm();
			float num = scientistAStarContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm);
			if (sqrMagnitude <= num)
			{
				scientistAStarContext.SetFact(Facts.EnemyRange, EnemyRange.CloseRange);
				return;
			}
			float num2 = scientistAStarContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm);
			if (sqrMagnitude <= num2)
			{
				scientistAStarContext.SetFact(Facts.EnemyRange, EnemyRange.MediumRange);
				return;
			}
			float num3 = scientistAStarContext.Body.AiDefinition.Engagement.SqrLongRangeFirearm(firearm);
			if (sqrMagnitude <= num3)
			{
				scientistAStarContext.SetFact(Facts.EnemyRange, EnemyRange.LongRange);
			}
			else
			{
				scientistAStarContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
		}
	}
}
