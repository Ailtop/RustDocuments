using System;
using UnityEngine;

namespace Rust.Ai.HTN.Murderer
{
	[Serializable]
	public class MurdererMemory : BaseNpcMemory
	{
		[NonSerialized]
		public MurdererContext MurdererContext;

		public Vector3 CachedPreferredDistanceDestination;

		public float CachedPreferredDistanceDestinationTime;

		public Vector3 CachedRoamDestination;

		public float CachedRoamDestinationTime;

		public override BaseNpcDefinition Definition => MurdererContext.Body.AiDefinition;

		public MurdererMemory(MurdererContext context)
			: base(context)
		{
			MurdererContext = context;
		}

		public override void ResetState()
		{
			base.ResetState();
		}

		protected override void OnSetPrimaryKnownEnemyPlayer(ref EnemyPlayerInfo info)
		{
			base.OnSetPrimaryKnownEnemyPlayer(ref info);
			if ((info.LastKnownPosition - MurdererContext.BodyPosition).sqrMagnitude > 1f)
			{
				MurdererContext.HasVisitedLastKnownEnemyPlayerLocation = false;
			}
		}
	}
}
