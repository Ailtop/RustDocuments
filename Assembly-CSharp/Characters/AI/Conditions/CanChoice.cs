using Characters.AI.Behaviours.Pope;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class CanChoice : Condition
	{
		[SerializeField]
		private Choice _choice;

		protected override bool Check(AIController controller)
		{
			return _choice.CanUse();
		}
	}
}
