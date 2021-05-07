using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Magician
{
	public class FireballCombo : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(CircularProjectileAttack))]
		private CircularProjectileAttack _circleFireBall;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _circleFireBall.CRun(controller);
			base.result = Result.Done;
		}
	}
}
