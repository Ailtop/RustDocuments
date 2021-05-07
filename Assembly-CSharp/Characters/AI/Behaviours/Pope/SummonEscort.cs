using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Pope
{
	public sealed class SummonEscort : Behaviour
	{
		[SerializeField]
		private RunAction _attack;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveHandler))]
		private MoveHandler _moveHandler;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _moveHandler.CMove(controller);
			yield return _attack.CRun(controller);
			base.result = Result.Success;
		}
	}
}
