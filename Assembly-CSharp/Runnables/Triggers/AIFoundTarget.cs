using Characters.AI;
using UnityEngine;

namespace Runnables.Triggers
{
	public class AIFoundTarget : Trigger
	{
		[SerializeField]
		private AIController _ai;

		protected override bool Check()
		{
			return _ai.target != null;
		}
	}
}
