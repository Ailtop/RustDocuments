using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Hero
{
	public class SwordAuraWave : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attack;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_attack.CanUse())
			{
				yield return _attack.CRun(controller);
				base.result = Result.Done;
			}
		}
	}
}
