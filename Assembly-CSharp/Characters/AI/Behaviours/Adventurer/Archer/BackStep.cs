using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Archer
{
	public class BackStep : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(KeepDistance))]
		private KeepDistance _keepDistance;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			yield return _keepDistance.CRun(controller);
			base.result = Result.Done;
		}

		public bool CanUse()
		{
			return _keepDistance.CanUseBackStep();
		}
	}
}
