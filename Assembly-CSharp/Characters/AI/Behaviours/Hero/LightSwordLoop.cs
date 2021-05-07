using System.Collections;
using Characters.AI.Hero.LightSwords;
using UnityEngine;

namespace Characters.AI.Behaviours.Hero
{
	public sealed class LightSwordLoop : Decorator
	{
		[SerializeField]
		private LightSwordFieldHelper _helper;

		[SerializeField]
		[Subcomponent(true)]
		private Behaviour _behaviour;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			while (_helper.GetActivatedSwordCount() > 0)
			{
				yield return _behaviour.CRun(controller);
			}
			base.result = Result.Success;
		}
	}
}
