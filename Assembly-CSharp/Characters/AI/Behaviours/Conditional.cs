using System.Collections;
using Characters.AI.Conditions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Conditional : Decorator
	{
		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _condition;

		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_condition.IsSatisfied(controller))
			{
				yield return _behaviour.CRun(controller);
				base.result = _behaviour.result;
			}
			else
			{
				base.result = Result.Fail;
			}
		}
	}
}
