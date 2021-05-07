using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class RandomBehaviour : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BehaviourInfo))]
		private BehaviourInfo.Subcomponents _behaviours;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			if (_behaviours == null)
			{
				Debug.LogError("Behaviours length is null");
				yield break;
			}
			if (_behaviours.components.Length == 0)
			{
				Debug.LogError("Behaviours length is 0");
				yield break;
			}
			BehaviourInfo behaviour = _behaviours.components.Random();
			yield return behaviour.CRun(controller);
			base.result = behaviour.result;
		}
	}
}
