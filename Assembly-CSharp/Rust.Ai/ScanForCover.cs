using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	[FriendlyName("Scan for Cover", "Scanning for cover volumes and the cover points within the relevant ones.")]
	public sealed class ScanForCover : BaseAction
	{
		[ApexSerialization]
		public float MaxDistanceToCover = 15f;

		[ApexSerialization]
		public float CoverArcThreshold = -0.75f;

		public override void DoExecute(BaseContext ctx)
		{
			if (SingletonComponent<AiManager>.Instance == null || !SingletonComponent<AiManager>.Instance.enabled || !SingletonComponent<AiManager>.Instance.UseCover || ctx.AIAgent.AttackTarget == null)
			{
				return;
			}
			NPCHumanContext nPCHumanContext = ctx as NPCHumanContext;
			if (nPCHumanContext == null)
			{
				return;
			}
			if (nPCHumanContext.sampledCoverPoints.Count > 0)
			{
				nPCHumanContext.sampledCoverPoints.Clear();
				nPCHumanContext.sampledCoverPointTypes.Clear();
			}
			if (!(nPCHumanContext.AIAgent.AttackTarget is BasePlayer))
			{
				return;
			}
			if (nPCHumanContext.CurrentCoverVolume == null || !nPCHumanContext.CurrentCoverVolume.Contains(nPCHumanContext.Position))
			{
				nPCHumanContext.CurrentCoverVolume = SingletonComponent<AiManager>.Instance.GetCoverVolumeContaining(nPCHumanContext.Position);
				if (nPCHumanContext.CurrentCoverVolume == null)
				{
					nPCHumanContext.CurrentCoverVolume = AiManager.CreateNewCoverVolume(nPCHumanContext.Position, null);
				}
			}
			if (nPCHumanContext.CurrentCoverVolume != null)
			{
				foreach (CoverPoint coverPoint in nPCHumanContext.CurrentCoverVolume.CoverPoints)
				{
					if (!coverPoint.IsReserved)
					{
						Vector3 position = coverPoint.Position;
						if (!((nPCHumanContext.Position - position).sqrMagnitude > MaxDistanceToCover))
						{
							Vector3 normalized = (position - nPCHumanContext.AIAgent.AttackTargetMemory.Position).normalized;
							if (ProvidesCoverFromDirection(coverPoint, normalized, CoverArcThreshold))
							{
								nPCHumanContext.sampledCoverPointTypes.Add(coverPoint.NormalCoverType);
								nPCHumanContext.sampledCoverPoints.Add(coverPoint);
							}
						}
					}
				}
			}
		}

		public static bool ProvidesCoverFromDirection(CoverPoint cp, Vector3 directionTowardCover, float arcThreshold)
		{
			if (Vector3.Dot(cp.Normal, directionTowardCover) < arcThreshold)
			{
				return true;
			}
			return false;
		}
	}
}
