using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Thief
{
	public class Shuriken : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Jump))]
		private Jump _surikenJump;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _surikenJump.CRun(controller);
			base.result = Result.Done;
		}
	}
}
