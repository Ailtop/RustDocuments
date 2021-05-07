using Characters.AI.Conditions;
using UnityEngine;

namespace Characters.AI.Hero
{
	public class Trigger : MonoBehaviour
	{
		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _backslashCondition;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _verticalPierceCondition;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _backstepCondition;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _inShortRange;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _inMiddleRange;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _dashBreakAwayCondition;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _behaviourECondition;

		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _behaviourJCondition;

		public bool InShortRange(AIController controller)
		{
			return _inShortRange.IsSatisfied(controller);
		}

		public bool InMiddleRange(AIController controller)
		{
			return _inMiddleRange.IsSatisfied(controller);
		}

		public bool CanRunBackSlash(AIController controller)
		{
			return _backslashCondition.IsSatisfied(controller);
		}

		public bool CanRunVerticalPierce(AIController controller)
		{
			return _verticalPierceCondition.IsSatisfied(controller);
		}

		public bool CanRunDashBreakAway(AIController controller)
		{
			return _dashBreakAwayCondition.IsSatisfied(controller);
		}

		public bool ShouldBackStep(AIController controller)
		{
			return _backstepCondition.IsSatisfied(controller);
		}

		public bool CanRunBehavourE(AIController controller)
		{
			return _behaviourECondition.IsSatisfied(controller);
		}

		public bool CanRunBehavourJ(AIController controller)
		{
			return _behaviourECondition.IsSatisfied(controller);
		}
	}
}
