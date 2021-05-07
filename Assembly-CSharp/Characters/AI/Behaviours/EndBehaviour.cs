using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class EndBehaviour : Decorator
	{
		[SerializeField]
		private Behaviour _behaviour;

		[SerializeField]
		private Behaviour _beforeBehaviour;

		public override IEnumerator CRun(AIController controller)
		{
			yield return _beforeBehaviour.CRun(controller);
			yield return _behaviour.CRun(controller);
		}
	}
}
