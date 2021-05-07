using UnityEngine;

namespace Rust.Ai
{
	public struct AiAnswer_ShareEnemyTarget : IAiAnswer
	{
		public BasePlayer PlayerTarget;

		public Vector3? LastKnownPosition;

		public NPCPlayerApex Source { get; set; }
	}
}
