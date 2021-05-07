using Characters.AI.Behaviours.Hero;
using UnityEngine;

namespace Characters.AI.Conditions
{
	public class BehaviourCoolTime : Condition
	{
		[SerializeField]
		private BehaviourTemplate _behaviour;

		protected override bool Check(AIController controller)
		{
			return _behaviour.canUse;
		}
	}
}
