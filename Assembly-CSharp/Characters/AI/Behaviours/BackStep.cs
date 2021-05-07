using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class BackStep : Behaviour
	{
		[SerializeField]
		private Action _jump;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Idle))]
		private Idle _idle;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_jump.TryStart();
			yield return CWaitJumpEnd();
			yield return _idle.CRun(controller);
			base.result = Result.Done;
		}

		private IEnumerator CWaitJumpEnd()
		{
			while (_jump.running)
			{
				yield return null;
			}
		}

		public bool CanUse()
		{
			return _jump.cooldown.canUse;
		}
	}
}
