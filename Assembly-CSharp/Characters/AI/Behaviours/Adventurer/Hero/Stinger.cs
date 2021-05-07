using System.Collections;
using Characters.Actions;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Hero
{
	public class Stinger : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(MoveToDestination))]
		private MoveToDestination _moveToDestination;

		[SerializeField]
		private Action _ready;

		[SerializeField]
		private Action _attack;

		[SerializeField]
		private Action _end;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character character = controller.character;
			Character target = controller.target;
			Bounds bounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			if (target.transform.position.x > character.transform.position.x)
			{
				controller.destination = new Vector2(bounds.max.x - 1f, bounds.max.y);
			}
			else
			{
				controller.destination = new Vector2(bounds.min.x + 1f, bounds.max.y);
			}
			_ready.TryStart();
			while (_ready.running)
			{
				if (base.result != Result.Doing)
				{
					yield break;
				}
				yield return null;
			}
			_attack.TryStart();
			yield return _moveToDestination.CRun(controller);
			_end.TryStart();
			while (_end.running)
			{
				if (base.result != Result.Doing)
				{
					yield break;
				}
				yield return null;
			}
			base.result = Result.Done;
		}
	}
}
