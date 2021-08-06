using Apex.AI;
using Apex.Serialization;
using ConVar;
using UnityEngine;

namespace Rust.Ai
{
	public class CoverScorer : OptionScorerBase<CoverPoint>
	{
		[ApexSerialization]
		[Range(-1f, 1f)]
		public float coverFromPointArcThreshold = -0.8f;

		public override float Score(IAIContext context, CoverPoint option)
		{
			return Evaluate(context as CoverContext, option, coverFromPointArcThreshold);
		}

		public static float Evaluate(CoverContext c, CoverPoint option, float arcThreshold)
		{
			if (c != null)
			{
				if (option.IsReserved || option.IsCompromised)
				{
					return 0f;
				}
				Vector3 serverPosition = c.Self.Entity.ServerPosition;
				if (option.ProvidesCoverFromPoint(c.DangerPoint, arcThreshold))
				{
					Vector3 dirCover = option.Position - serverPosition;
					Vector3 dirDanger = c.DangerPoint - serverPosition;
					float directness = Vector3.Dot(dirCover.normalized, dirDanger.normalized);
					float result;
					if (EvaluateAdvancement(c, option, dirCover, dirDanger, directness, out result))
					{
						return result;
					}
					if (EvaluateRetreat(c, option, dirCover, dirDanger, ref directness, out result))
					{
						return result;
					}
					if (EvaluateFlanking(c, option, dirCover, dirDanger, directness, out result))
					{
						return result;
					}
				}
			}
			return 0f;
		}

		private static bool EvaluateAdvancement(CoverContext c, CoverPoint option, Vector3 dirCover, Vector3 dirDanger, float directness, out float result)
		{
			result = 0f;
			if (directness > 0.5f && dirCover.sqrMagnitude > dirDanger.sqrMagnitude)
			{
				return false;
			}
			if (directness >= 0.5f)
			{
				float sqrMagnitude = dirCover.sqrMagnitude;
				if (sqrMagnitude > dirDanger.sqrMagnitude)
				{
					return false;
				}
				float num = directness;
				if (num > c.BestAdvanceValue)
				{
					if (ConVar.AI.npc_cover_use_path_distance && c.Self.IsNavRunning() && c.Self.AttackTarget != null)
					{
						NPCPlayerApex nPCPlayerApex = c.Self as NPCPlayerApex;
						if (nPCPlayerApex != null && !nPCPlayerApex.PathDistanceIsValid(c.Self.AttackTarget.ServerPosition, option.Position))
						{
							return false;
						}
					}
					if ((option.Position - c.DangerPoint).sqrMagnitude < sqrMagnitude)
					{
						num *= 0.9f;
					}
					c.BestAdvanceValue = num;
					c.BestAdvanceCP = option;
					result = c.BestAdvanceValue;
					return true;
				}
			}
			return false;
		}

		private static bool EvaluateRetreat(CoverContext c, CoverPoint option, Vector3 dirCover, Vector3 dirDanger, ref float directness, out float result)
		{
			result = 0f;
			if (directness <= -0.5f)
			{
				NPCPlayerApex nPCPlayerApex = c.Self as NPCPlayerApex;
				if (nPCPlayerApex == null)
				{
					return false;
				}
				if (dirCover.sqrMagnitude < nPCPlayerApex.MinDistanceToRetreatCover * nPCPlayerApex.MinDistanceToRetreatCover)
				{
					directness = -0.49f;
					return false;
				}
				float num = directness * -1f;
				if (num > c.BestRetreatValue)
				{
					c.BestRetreatValue = num;
					c.BestRetreatCP = option;
					result = c.BestRetreatValue;
					return true;
				}
			}
			return false;
		}

		private static bool EvaluateFlanking(CoverContext c, CoverPoint option, Vector3 dirCover, Vector3 dirDanger, float directness, out float result)
		{
			result = 0f;
			if (directness > -0.5f && directness < 0.5f)
			{
				float num = 1f - Mathf.Abs(directness);
				if (num > c.BestFlankValue)
				{
					if (ConVar.AI.npc_cover_use_path_distance && c.Self.IsNavRunning() && c.Self.AttackTarget != null)
					{
						NPCPlayerApex nPCPlayerApex = c.Self as NPCPlayerApex;
						if (nPCPlayerApex != null && !nPCPlayerApex.PathDistanceIsValid(c.Self.AttackTarget.ServerPosition, option.Position))
						{
							return false;
						}
					}
					c.BestFlankValue = 0.1f - Mathf.Abs(num);
					c.BestFlankCP = option;
					result = c.BestFlankValue;
					return true;
				}
			}
			return false;
		}
	}
}
