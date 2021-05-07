using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public class LightSwordFieldAction : Behaviour
	{
		[SerializeField]
		private LightSwordFieldMove _move;

		[SerializeField]
		private Behaviour _attack;

		public override IEnumerator CRun(AIController controller)
		{
			yield return _move.CRun(controller);
			yield return _attack.CRun(controller);
		}
	}
}
