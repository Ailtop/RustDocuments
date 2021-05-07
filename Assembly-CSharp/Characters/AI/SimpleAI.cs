using System.Collections;
using Characters.AI.Behaviours;
using UnityEngine;

namespace Characters.AI
{
	public sealed class SimpleAI : AIController
	{
		[SerializeField]
		[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
		private Characters.AI.Behaviours.Behaviour _behaviours;

		private new void OnEnable()
		{
			StartCoroutine(CProcess());
		}

		protected override IEnumerator CProcess()
		{
			yield return CPlayStartOption();
			yield return _behaviours.CRun(this);
		}
	}
}
