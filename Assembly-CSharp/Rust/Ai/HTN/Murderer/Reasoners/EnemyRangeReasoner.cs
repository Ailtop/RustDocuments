using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class EnemyRangeReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			MurdererContext murdererContext = npc.AiDomain.NpcContext as MurdererContext;
			if (murdererContext == null)
			{
				return;
			}
			if (murdererContext.Memory.PrimaryKnownEnemyPlayer.PlayerInfo.Player == null)
			{
				murdererContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
			float sqrMagnitude = (murdererContext.Memory.PrimaryKnownEnemyPlayer.LastKnownPosition - murdererContext.BodyPosition).sqrMagnitude;
			AttackEntity firearm = murdererContext.Domain.GetFirearm();
			float num = murdererContext.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm);
			if (sqrMagnitude <= num)
			{
				murdererContext.SetFact(Facts.EnemyRange, EnemyRange.CloseRange);
				return;
			}
			float num2 = murdererContext.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm);
			if (sqrMagnitude <= num2)
			{
				murdererContext.SetFact(Facts.EnemyRange, EnemyRange.MediumRange);
				return;
			}
			float num3 = murdererContext.Body.AiDefinition.Engagement.SqrLongRangeFirearm(firearm);
			if (sqrMagnitude <= num3)
			{
				murdererContext.SetFact(Facts.EnemyRange, EnemyRange.LongRange);
			}
			else
			{
				murdererContext.SetFact(Facts.EnemyRange, EnemyRange.OutOfRange);
			}
		}
	}
}
