using System.Collections;
using Characters.AI.Behaviours;
using UnityEngine;

namespace Characters.AI.Adventurer
{
	public sealed class VeteranAI : AIController
	{
		[SerializeField]
		[Characters.AI.Behaviours.Behaviour.Subcomponent(true)]
		private Characters.AI.Behaviours.Behaviour _behaviours;

		public void StartCombat()
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
