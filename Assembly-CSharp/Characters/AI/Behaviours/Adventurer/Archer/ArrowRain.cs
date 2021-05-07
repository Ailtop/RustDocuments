using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Archer
{
	public class ArrowRain : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _motion;

		[SerializeField]
		private Action _shot;

		[SerializeField]
		private float _delay;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _motion.CRun(controller);
			yield return controller.character.chronometer.master.WaitForSeconds(_delay);
			_shot.TryStart();
			base.result = Result.Done;
		}
	}
}
