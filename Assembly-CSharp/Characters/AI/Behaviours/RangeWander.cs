using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours
{
	public class RangeWander : Wander
	{
		[SerializeField]
		[MinMaxSlider(1f, 10f)]
		private Vector2 _range;

		private Vector2? _center;

		public Vector2? center
		{
			get
			{
				return _center;
			}
			set
			{
				_center = value;
			}
		}

		public override IEnumerator CRun(AIController controller)
		{
			Character character = controller.character;
			base.result = Result.Doing;
			Bounds platformBounds = character.movement.controller.collisionState.lastStandingCollider.bounds;
			bool right = true;
			while (true)
			{
				yield return null;
				if (CheckStopWander(controller))
				{
					break;
				}
				if (Precondition.CanMove(character))
				{
					if (!_center.HasValue)
					{
						_center = character.transform.position;
					}
					float num = Random.Range(_range.x, _range.y);
					float x = ((!right) ? Mathf.Max(_center.Value.x - num, platformBounds.min.x) : Mathf.Min(_center.Value.x + num, platformBounds.max.x));
					controller.destination = new Vector2(x, 0f);
					yield return _move.CRun(controller);
					right = !right;
				}
			}
		}

		private bool CheckStopWander(AIController controller)
		{
			if (Precondition.CanChase(controller.character, controller.target))
			{
				base.result = Result.Done;
				return true;
			}
			if ((bool)controller.FindClosestPlayerBody(_sightRange))
			{
				base.result = Result.Done;
				return true;
			}
			return false;
		}
	}
}
