using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Selector : Decorator
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
				if (child.result == Result.Success)
				{
					base.result = Result.Success;
					yield break;
				}
			}
			base.result = Result.Fail;
		}
	}
}
