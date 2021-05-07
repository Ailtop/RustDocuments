using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Archer
{
	public class BirdHunt : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _attackMotion;

		[SerializeField]
		private Action _shot;

		[SerializeField]
		private float _delay = 0.5f;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _attackMotion.CRun(controller);
			yield return Chronometer.global.WaitForSeconds(_delay);
			_shot.TryStart();
			base.result = Result.Done;
		}
	}
}
