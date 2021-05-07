using Characters.AI.Behaviours;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public class BehaviourResult : Condition
	{
		[SerializeField]
		private Characters.AI.Behaviours.Behaviour _behaviour;

		protected override bool Check(AIController controller)
		{
			return _behaviour.result == Characters.AI.Behaviours.Behaviour.Result.Success;
		}
	}
}
