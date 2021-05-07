using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class KnockbackTo : CharacterHitOperation
	{
		[Header("Destination")]
		[SerializeField]
		private Collider2D _targetPlace;

		[SerializeField]
		private Transform _targetPoint;

		[Header("Force")]
		[SerializeField]
		private Curve _curve;

		[SerializeField]
		private bool _ignoreOtherForce = true;

		[SerializeField]
		private bool _expireOnGround;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character target)
		{
			Vector2 vector = ((!(_targetPlace != null)) ? ((Vector2)_targetPoint.position) : MMMaths.RandomPointWithinBounds(_targetPlace.bounds));
			Vector2 force = vector - (Vector2)target.transform.position;
			target.movement.push.ApplyKnockback(projectile.owner, force, _curve, _ignoreOtherForce, _expireOnGround);
		}
	}
}
