using Rust.Ai.HTN.Reasoning;

namespace Rust.Ai.HTN.Murderer.Reasoners
{
	public class PreferredFightingRangeReasoner : INpcReasoner
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
			NpcPlayerInfo target = murdererContext.GetPrimaryEnemyPlayerTarget();
			if (target.Player != null)
			{
				AttackEntity firearm = murdererContext.Domain.GetFirearm();
				if (IsAtPreferredRange(murdererContext, ref target, firearm))
				{
					murdererContext.SetFact(Facts.AtLocationPreferredFightingRange, 1);
				}
				else
				{
					murdererContext.SetFact(Facts.AtLocationPreferredFightingRange, 0);
				}
			}
		}

		public static bool IsAtPreferredRange(MurdererContext context, ref NpcPlayerInfo target, AttackEntity firearm)
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
