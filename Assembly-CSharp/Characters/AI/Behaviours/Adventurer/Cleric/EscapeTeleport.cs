using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Cleric
{
	public class EscapeTeleport : Behaviour
	{
		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Teleport))]
		private Teleport _teleport;

		[Space]
		[SerializeField]
		private Transform _teleportPoint;

		[SerializeField]
		private float _minDistanceWithWall = 1.5f;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Character target = controller.target;
			Bounds bounds = controller.character.movement.controller.collisionState.lastStandingCollider.bounds;
			if (bounds.center.x < target.transform.position.x)
			{
				float x = Random.Range(bounds.min.x + _minDistanceWithWall, target.transform.position.x - 5f);
				_teleportPoint.position = new Vector2(x, bounds.max.y);
			}
			else
			{
				float x2 = Random.Range(target.transform.position.x + 5f, bounds.max.x - _minDistanceWithWall);
				_teleportPoint.position = new Vector2(x2, bounds.max.y);
			}
			yield return _teleport.CRun(controller);
			base.result = Result.Done;
		}

		public bool CanUse()
		{
			return _teleport.CanUse();
		}
	}
}
