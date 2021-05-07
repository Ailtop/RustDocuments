using System;
using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.DarkQuartzGolem
{
	public class Melee : Behaviour, IPattern
	{
		[SerializeField]
		internal Collider2D trigger;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		public bool CanUse()
		{
			throw new NotImplementedException();
		}

		public bool CanUse(AIController controller)
		{
			return controller.FindClosestPlayerBody(trigger) != null;
		}

		public override IEnumerator CRun(AIController controller)
		{
			yield return _attack.CRun(controller);
		}
	}
}
