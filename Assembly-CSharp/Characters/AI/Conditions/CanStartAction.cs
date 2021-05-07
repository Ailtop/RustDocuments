using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public sealed class CanStartAction : Condition
	{
		[SerializeField]
		private Action _action;

		protected override bool Check(AIController controller)
		{
			return _action.canUse;
		}
	}
}
