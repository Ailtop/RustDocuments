using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class EnemyRangeReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistContext scientistContext = npc.AiDomain.NpcContext as ScientistContext;
			if (scientistContext == null)
			{
				return;
			}
			if (scientistContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
			{
				scientistContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
			float sqrMagnitude = (scientistContext.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition - scientistContext.BodyPosition).sqrMagnitude;
			AttackEntity firearm = scientistContext.Domain.GetFirearm();
			float num = scientistContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm);
			if (sqrMagnitude <= num)
			{
				scientistContext.SetFact(Facts.EnemyRange, EnemyRange.CloseRange);
				return;
			}
			float num2 = scientistContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm);
			if (sqrMagnitude <= num2)
			{
				scientistContext.SetFact(Facts.EnemyRange, EnemyRange.MediumRange);
				return;
			}
			float num3 = scientistContext.Body.AiDefinition.Engagement.SqrLongRangeFirearm(firearm);
			if (sqrMagnitude <= num3)
			{
				scientistContext.SetFact(Facts.EnemyRange, EnemyRange.LongRange);
			}
			else
			{
				scientistContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
		}
	}
}
