using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using Characters.Movements;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class Whirlwind : Behaviour
	{
		[SerializeField]
		private Action _ready;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ChaseAttack))]
		private ChaseAttack _action;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_ready.canUse)
			{
				Character character = controller.character;
				_ready.TryStart();
				while (_ready.running)
				{
					yield return null;
				}
				character.movement.config.type = Movement.Config.Type.AcceleratingWalking;
				yield return _action.CRun(controller);
				character.movement.config.type = Movement.Config.Type.Walking;
				base.result = Result.Done;
			}
		}
	}
}
