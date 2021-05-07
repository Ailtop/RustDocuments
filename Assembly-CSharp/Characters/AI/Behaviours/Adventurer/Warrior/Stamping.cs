using System.Collections;
using Characters.Actions;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Warrior
{
	public class Stamping : Behaviour
	{
		[SerializeField]
		private Action _stampingReady;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Jump))]
		private Jump _jump;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(ActionAttack))]
		private ActionAttack _stamping;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character target = controller.target;
			_stampingReady.TryStart();
			while (_stampingReady.running)
			{
				yield return null;
			}
			character.ForceToLookAt(target.transform.position.x);
			yield return _jump.CRun(controller);
			base.result = Result.Done;
		}
	}
}
