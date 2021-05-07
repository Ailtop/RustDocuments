using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class DashBreakAway : Behaviour
	{
		[SerializeField]
		private Action _action;

		public override IEnumerator CRun(AIController controller)
		{
			_action.TryStart();
			while (_action.running)
			{
				yield return null;
			}
		}
	}
}
