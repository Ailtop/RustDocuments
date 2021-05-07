using System;
using System.Collections;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer
{
	public class Runaway : Behaviour
	{
		[Serializable]
		private class Config
		{
			[SerializeField]
			internal Transform destination;

			[SerializeField]
			internal int height;
		}

		[SerializeField]
		private Behaviour _move;

		[SerializeField]
		private Config _config;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			SetDestination(controller);
			controller.character.ForceToLookAt(controller.destination.x);
			yield return _move.CRun(controller);
			base.result = Result.Done;
		}

		private void SetDestination(AIController controller)
		{
			Bounds bounds = controller.character.movement.controller.collisionState.lastStandingCollider.bounds;
			Character character = controller.character;
			Character target = controller.target;
			float x = character.collider.bounds.size.x;
			if (target.transform.position.x < bounds.center.x)
			{
				controller.destination = new Vector2(bounds.max.x - x, bounds.max.y + (float)_config.height);
			}
			else
			{
				controller.destination = new Vector2(bounds.min.x + x, bounds.max.y + (float)_config.height);
			}
			if (_config.destination != null)
			{
				_config.destination.position = controller.destination;
			}
		}
	}
}
