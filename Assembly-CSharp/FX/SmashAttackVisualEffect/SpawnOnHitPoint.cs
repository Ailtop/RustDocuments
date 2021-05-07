using Characters;
using Characters.Movements;
using UnityEngine;

namespace FX.SmashAttackVisualEffect
{
	public class SpawnOnHitPoint : SmashAttackVisualEffect
	{
		[SerializeField]
		private bool _referSmashDirection;

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

		public override void Spawn(Character owner, Push push, RaycastHit2D raycastHit, Movement.CollisionDirection direction, Damage damage, ITarget target)
		{
			Vector3 zero = Vector3.zero;
			Vector3 min = owner.collider.bounds.min;
			Vector3 max = owner.collider.bounds.max;
			switch (direction)
			{
			case Movement.CollisionDirection.Above:
				zero.x = Random.Range(min.x, max.x);
				zero.y = max.y;
				break;
			case Movement.CollisionDirection.Below:
				zero.x = Random.Range(min.x, max.x);
				zero.y = min.y;
				break;
			case Movement.CollisionDirection.Left:
				zero.x = min.x;
				zero.y = Random.Range(min.y, max.y);
				break;
			case Movement.CollisionDirection.Right:
				zero.x = max.x;
				zero.y = Random.Range(min.y, max.y);
				break;
			}
			EffectInfo obj = (damage.critical ? _critical : _normal);
			float extraAngle = (_referSmashDirection ? (Mathf.Atan2(push.direction.y, push.direction.x) * 57.29578f) : 0f);
			Vector3 scale = ((owner.lookingDirection == Character.LookingDirection.Right) ? Vector3.one : new Vector3(-1f, 1f, 1f));
			obj.Spawn(zero, extraAngle).transform.localScale.Scale(scale);
		}
	}
}
