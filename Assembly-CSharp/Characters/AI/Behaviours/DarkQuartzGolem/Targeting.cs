using System;
using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.DarkQuartzGolem
{
	public class Targeting : Behaviour, IPattern
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		public bool CanUse()
		{
			throw new NotImplementedException();
		}

		public bool CanUse(AIController controller)
		{
			Character target = controller.target;
			Character character = controller.character;
			Collider2D lastStandingCollider = target.movement.controller.collisionState.lastStandingCollider;
			Collider2D lastStandingCollider2 = character.movement.controller.collisionState.lastStandingCollider;
			if (lastStandingCollider != lastStandingCollider2)
			{
				return true;
			}
			return false;
		}

		public override IEnumerator CRun(AIController controller)
		{
			while (controller.target == null || controller.target.movement == null || !controller.target.movement.isGrounded)
			{
				yield return null;
			}
			yield return _attack.CRun(controller);
		}
	}
}
