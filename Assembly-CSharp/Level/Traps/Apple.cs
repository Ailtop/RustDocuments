using Characters;
using Characters.Projectiles;
using UnityEngine;

namespace Level.Traps
{
	public class Apple : DestructibleObject
	{
		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private Collider2D _collider;

		public override Collider2D collider => _collider;

		public override void Hit(Character from, ref Damage damage, Vector2 force)
		{
			if (damage.amount != 0.0)
			{
				float direction = Mathf.Atan2(force.y, force.x) * 57.29578f + Random.Range(-10f, 10f);
				float num = Mathf.Clamp(force.magnitude, 2f, 6f);
				_projectile.reusable.Spawn(base.transform.position).GetComponent<Projectile>().Fire(from, num, direction, false, false, num);
				Object.Destroy(base.gameObject);
			}
		}
	}
}
