using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class DrinkPotion : Behaviour
	{
		[SerializeField]
		private Action _action;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_action.TryStart();
			while (base.result == Result.Doing && _action.running)
			{
				yield return null;
			}
			base.result = Result.Done;
		}
	}
}
