using Characters.Movements;
using Characters.Operations;
using UnityEditor;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class Smash : CharacterHitOperation
	{
		[SerializeField]
		private PushInfo _pushInfo = new PushInfo(true);

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(TargetedOperationInfo))]
		private TargetedOperationInfo.Subcomponents _onCollide;

		private void OnEnd(Push push, Character from, Character to, Push.SmashEndType endType, RaycastHit2D? raycastHit, Movement.CollisionDirection direction)
		{
			if (endType == Push.SmashEndType.Collide)
			{
				StartCoroutine(_onCollide.CRun(from, to));
			}
		}

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character character)
		{
			character.movement.push.ApplySmash(projectile.owner, _pushInfo, OnEnd);
		}
	}
}
