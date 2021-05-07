using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Hero
{
	public class ComboAttack : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character target = controller.target;
			controller.character.ForceToLookAt(target.transform.position.x);
			yield return _attack.CRun(controller);
			base.result = Result.Done;
		}
	}
}
