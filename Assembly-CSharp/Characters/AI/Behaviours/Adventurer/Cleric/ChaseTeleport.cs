using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Cleric
{
	public class ChaseTeleport : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Teleport))]
		private Teleport _teleport;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _teleport.CRun(controller);
			base.result = Result.Done;
		}
	}
}
