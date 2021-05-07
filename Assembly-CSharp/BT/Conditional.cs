using BT.Conditions;
using UnityEngine;

namespace BT
{
	public sealed class Conditional : Node
	{
		[SerializeField]
		[Condition.Subcomponent(true)]
		private Condition _condition;

		protected override NodeState UpdateDeltatime(Context context)
		{
			if (!_condition.IsSatisfied(context))
			{
				return NodeState.Fail;
			}
			return NodeState.Success;
		}
	}
}
