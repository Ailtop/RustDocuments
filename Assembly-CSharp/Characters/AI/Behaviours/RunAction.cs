using System.Collections;
using Characters.Actions;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public sealed class RunAction : Behaviour
	{
		[SerializeField]
		private Action _action;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (!_action.TryStart())
			{
				base.result = Result.Fail;
				yield break;
			}
			while (_action.running && base.result == Result.Doing)
			{
				yield return null;
			}
			base.result = Result.Success;
		}
	}
}
