using Apex.Serialization;
using UnityEngine;

namespace Rust.Ai
{
	public class LookAtRandomPoint : BaseAction
	{
		[ApexSerialization]
		public float MinTimeout = 5f;

		[ApexSerialization]
		public float MaxTimeout = 20f;

		public override void DoExecute(BaseContext context)
		{
			(context as NPCHumanContext)?.Human.LookAtRandomPoint(Random.Range(MinTimeout, MaxTimeout));
		}
	}
}
