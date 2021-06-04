using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.ScientistJunkpile.Reasoners
{
	public class PreferredFightingRangeReasoner : INpcReasoner
	{
		public float TickFrequency { get; set; }

		public float LastTickTime { get; set; }

		public void Tick(IHTNAgent npc, float deltaTime, float time)
		{
			ScientistJunkpileContext scientistJunkpileContext = npc.AiDomain.NpcContext as ScientistJunkpileContext;
			if (scientistJunkpileContext == null)
			{
				return;
			}
			NpcPlayerInfo target = scientistJunkpileContext.GetPrimaryEnemyPlayerTarget();
			if (target.Player != null)
			{
				AttackEntity firearm = scientistJunkpileContext.Domain.GetFirearm();
				if (IsAtPreferredRange(scientistJunkpileContext, ref target, firearm))
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationPreferredFightingRange, 1);
				}
				else
				{
					scientistJunkpileContext.SetFact(Facts.AtLocationPreferredFightingRange, 0);
				}
			}
		}

		public static bool IsAtPreferredRange(ScientistJunkpileContext context, ref NpcPlayerInfo target, AttackEntity firearm)
		{
			if (firearm == null)
			{
				return false;
			}
			switch (firearm.effectiveRangeType)
			{
			case NPCPlayerApex.WeaponTypeEnum.CloseRange:
				return target.SqrDistance <= context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm);
			case NPCPlayerApex.WeaponTypeEnum.MediumRange:
				if (target.SqrDistance <= context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm))
				{
					return target.SqrDistance > context.Body.AiDefinition.Engagement.SqrCloseRangeFirearm(firearm);
				}
				return false;
			case NPCPlayerApex.WeaponTypeEnum.LongRange:
				if (target.SqrDistance < context.Body.AiDefinition.Engagement.SqrLongRangeFirearm(firearm))
				{
					return target.SqrDistance > context.Body.AiDefinition.Engagement.SqrMediumRangeFirearm(firearm);
				}
				return false;
			default:
				return false;
			}
		}
	}
}
