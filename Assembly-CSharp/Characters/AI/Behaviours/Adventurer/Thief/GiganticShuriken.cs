using System.Collections;
using Characters.AI.Behaviours.Attacks;
using UnityEditor;
using UnityEngine;

namespace Characters.AI.Behaviours.Adventurer.Thief
{
	public class GiganticShuriken : Behaviour
	{
		[SerializeField]
		private Transform _teleportDestination;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(Teleport))]
		private Teleport _teleportForGiganticShuriken;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(HorizontalProjectileAttack))]
		private HorizontalProjectileAttack _giganticShuriken;

		public override IEnumerator CRun(AIController controller)
		{
			base.result = Result.Doing;
			Bounds bounds = controller.character.movement.controller.collisionState.lastStandingCollider.bounds;
			_teleportDestination.position = (MMMaths.RandomBool() ? new Vector2(bounds.min.x + 1f, bounds.max.y) : new Vector2(bounds.max.x - 1f, bounds.max.y));
			yield return _teleportForGiganticShuriken.CRun(controller);
			yield return _giganticShuriken.CRun(controller);
			base.result = Result.Done;
		}

		public bool CanUse()
		{
			return _giganticShuriken.CanUse();
		}
	}
}
