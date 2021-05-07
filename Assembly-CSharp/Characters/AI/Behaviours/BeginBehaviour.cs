using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class BeginBehaviour : Decorator
	{
		[SerializeField]
		private Behaviour _behaviour;

		[SerializeField]
		private Behaviour _nextBehaviour;

		public override IEnumerator CRun(AIController controller)
		{
			yield return _behaviour.CRun(controller);
			yield return _nextBehaviour.CRun(controller);
		}
	}
}
