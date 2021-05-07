using System.Collections;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Hero
{
	public class Dash : Behaviour
	{
		[SerializeField]
		private AttachAbility _bonusStats;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _dash;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character target = controller.target;
			controller.destination = target.transform.position;
			_bonusStats.Run(character);
			yield return _dash.CRun(controller);
			_bonusStats.Stop();
			base.result = Result.Done;
		}
	}
}
