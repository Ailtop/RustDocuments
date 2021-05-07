using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class WanderForDuration : Wander
	{
		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			_move.direction = (MMMaths.RandomBool() ? Vector2.left : Vector2.right);
			yield return _move.CRun(controller);
		}
	}
}
