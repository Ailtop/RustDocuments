using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Scientist.Reasoners
{
	public class PreferredFightingRangeReasoner : INpcReasoner
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
			NpcPlayerInfo target = scientistContext.GetPrimaryEnemyPlayerTarget();
			if (target.Player != null)
			{
				AttackEntity firearm = scientistContext.Domain.GetFirearm();
				if (IsAtPreferredRange(scientistContext, ref target, firearm))
				{
					scientistContext.SetFact(Facts.AtLocationPreferredFightingRange, 1);
				}
				else
				{
					scientistContext.SetFact(Facts.AtLocationPreferredFightingRange, 0);
				}
			}
		}

		public static bool IsAtPreferredRange(ScientistContext context, ref NpcPlayerInfo target, AttackEntity firearm)
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

		public static float GetPreferredRange(ScientistContext context, ref NpcPlayerInfo target, AttackEntity firearm)
		{
			if (firearm == null)
			{
				return context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm);
			}
			switch (firearm.effectiveRangeType)
			{
			case NPCPlayerApex.WeaponTypeEnum.CloseRange:
				return context.Body.AiDefinition.Engagement.CloseRangeFirearm(firearm);
			case NPCPlayerApex.WeaponTypeEnum.MediumRange:
				return context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm);
			case NPCPlayerApex.WeaponTypeEnum.LongRange:
				return context.Body.AiDefinition.Engagement.LongRangeFirearm(firearm);
			default:
				return context.Body.AiDefinition.Engagement.CenterOfMediumRangeFirearm(firearm);
			}
		}
	}
}
