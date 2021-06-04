using UnityEngine;

namespace Rust.Ai
{
	public struct AiStatement_EnemyEngaged : IAiStatement
	{
		public BasePlayer Enemy;

		public float Score;

		public Vector3? LastKnownPosition;
	}
}
