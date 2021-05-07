using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Thief
{
	public class ShadowStep : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TeleportBehind))]
		private TeleportBehind _teleportBehind;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _teleportBehind.CRun(controller);
			base.result = Result.Done;
		}
	}
}
