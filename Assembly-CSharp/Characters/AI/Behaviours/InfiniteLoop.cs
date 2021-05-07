using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class InfiniteLoop : Decorator
	{
		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			while (true)
			{
				yield return _behaviour.CRun(controller);
			}
		}
	}
}
