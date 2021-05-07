using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class Earthquake : Behaviour
	{
		[SerializeField]
		private Action _jumpForEarthquake;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _action;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_jumpForEarthquake.TryStart();
			while (_jumpForEarthquake.running)
			{
				yield return null;
			}
			yield return _action.CRun(controller);
			base.result = Result.Done;
		}
	}
}
