using System;
using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class RunActions : Behaviour
	{
		[SerializeField]
		private Characters.Actions.Action[] _actions;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_actions == null)
			{
				throw new NullReferenceException();
			}
			if (_actions.Length == 0)
			{
				Debug.LogError("The number of actions is 0");
			}
			Characters.Actions.Action[] actions = _actions;
			foreach (Characters.Actions.Action action in actions)
			{
				action.TryStart();
				while (action.running)
				{
					yield return null;
				}
			}
			base.result = Result.Success;
		}
	}
}
