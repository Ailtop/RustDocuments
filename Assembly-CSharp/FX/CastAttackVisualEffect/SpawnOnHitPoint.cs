using Characters;
using UnityEngine;

namespace FX.CastAttackVisualEffect
{
	public class SpawnOnHitPoint : CastAttackVisualEffect
	{
		[SerializeField]
		private EffectInfo _normal;

		[SerializeField]
		private EffectInfo _critical;

		private void Awake()
		{
			if (_critical.effect == null)
			{
				_critical = _normal;
			}
		}

		public override void Spawn(Vector3 position)
		{
			_normal.Spawn(position);
		}

		public override void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit)
		{
			_normal.Spawn(raycastHit.point, owner).transform.localScale = ((owner.lookingDirection == Character.LookingDirection.Right) ? Vector3.one : new Vector3(-1f, 1f, 1f));
		}

		public override void Spawn(Character owner, Collider2D collider, Vector2 origin, Vector2 direction, float distance, RaycastHit2D raycastHit, Damage damage, ITarget target)
		{
			(damage.critical ? _critical : _normal).Spawn(raycastHit.point, owner);
		}
	}
}
