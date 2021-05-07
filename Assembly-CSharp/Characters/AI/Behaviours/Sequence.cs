using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Sequence : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(BehaviourInfo))]
		private BehaviourInfo.Subcomponents _children;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			BehaviourInfo[] components = _children.components;
			foreach (BehaviourInfo child in components)
			{
				yield return child.CRun(controller);
				if (child.result == Result.Fail)
				{
					base.result = Result.Fail;
					yield break;
				}
			}
			base.result = Result.Success;
		}
	}
}
