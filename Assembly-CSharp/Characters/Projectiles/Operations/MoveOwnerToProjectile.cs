using Characters.Movements;

namespace Characters.Projectiles.Operations
{
	public class MoveOwnerToProjectile : Operation
	{
		public override void Run(Projectile projectile)
		{
			Movement movement = projectile.owner.movement;
			if (movement.controller.Teleport(projectile.transform.position, -projectile.direction, 5f))
			{
				movement.verticalVelocity = 0f;
			}
		}
	}
}
