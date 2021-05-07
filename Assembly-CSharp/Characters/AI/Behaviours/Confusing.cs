using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class Confusing : Behaviour
	{
		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _speedBonusDuration;

		[SerializeField]
		[MinMaxSlider(0f, 10f)]
		private Vector2 _turnCount;

		[SerializeField]
		[Move.Subcomponent(true)]
		private MoveForDuration _moveForDuration;

		private static Stat.Values _movementspeedBonus = new Stat.Values(new Stat.Value(Stat.Category.Percent, Stat.Kind.MovementSpeed, 1.0));

		[SerializeField]
		private bool allowBackward = true;

		private void Start()
		{
			_childs = new List<Behaviour> { _moveForDuration };
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			Collider2D platform = character.movement.controller.collisionState.lastStandingCollider;
			while (platform == null)
			{
				yield return null;
				platform = character.movement.controller.collisionState.lastStandingCollider;
			}
			int count = Random.Range((int)_turnCount.x, (int)_turnCount.y);
			base.result = Result.Doing;
			while (count-- > 0 && base.result == Result.Doing)
			{
				float duration = Random.Range(_speedBonusDuration.x, _speedBonusDuration.y);
				Vector2 vector = ((Mathf.Abs(character.transform.position.x - platform.bounds.max.x) < 1f) ? Vector2.left : ((Mathf.Abs(character.transform.position.x - platform.bounds.min.x) < 1f) ? Vector2.right : ((!MMMaths.RandomBool()) ? Vector2.left : Vector2.right)));
				if (allowBackward && MMMaths.RandomBool())
				{
					character.movement.moveBackward = true;
				}
				character.stat.DetachTimedValues(_movementspeedBonus);
				character.stat.AttachTimedValues(_movementspeedBonus, duration);
				_moveForDuration.direction = vector;
				character.movement.move = vector;
				yield return _moveForDuration.CRun(controller);
				character.movement.moveBackward = false;
			}
			character.stat.DetachTimedValues(_movementspeedBonus);
		}
	}
}
