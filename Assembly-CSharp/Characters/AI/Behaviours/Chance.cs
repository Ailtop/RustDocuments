using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Chance : Decorator
	{
		[SerializeField]
		[Range(0f, 1f)]
		private float _successChance;

		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (MMMaths.Chance(_successChance))
			{
				yield return _behaviour.CRun(controller);
				base.result = Result.Success;
			}
			else
			{
				base.result = Result.Fail;
			}
		}
	}
}
