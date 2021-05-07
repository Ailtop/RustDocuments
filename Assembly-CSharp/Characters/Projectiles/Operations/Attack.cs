using Characters.Operations;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class Attack : CharacterHitOperation
	{
		[SerializeField]
		protected HitInfo _hitInfo = new HitInfo(Damage.AttackType.Ranged);

		[SerializeField]
		protected ChronoInfo _chrono;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit, Character character)
		{
			Damage damage = projectile.owner.stat.GetDamage(projectile.baseDamage, raycastHit.point, _hitInfo);
			projectile.owner.Attack(character, ref damage);
			_chrono.ApplyTo(character);
		}
	}
}
