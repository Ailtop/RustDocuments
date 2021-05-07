using Characters.Utils;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public class FireProjectile : Operation
	{
		public enum DirectionType
		{
			RotationOfFirePosition,
			OwnerDirection,
			Constant
		}

		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private CustomFloat _speedMultiplier = new CustomFloat(1f);

		[SerializeField]
		private CustomFloat _damageMultiplier = new CustomFloat(1f);

		[SerializeField]
		private Transform _fireTransform;

		[SerializeField]
		private bool _group;

		[SerializeField]
		private DirectionType _directionType;

		[SerializeField]
		private CustomAngle.Reorderable _directions;

		public CustomAngle[] directions => _directions.values;

		private void Awake()
		{
			if (_fireTransform == null)
			{
				_fireTransform = base.transform;
			}
		}

		public override void Run(Projectile projectile)
		{
			Character owner = projectile.owner;
			CustomAngle[] values = _directions.values;
			float attackDamage = projectile.baseDamage * _damageMultiplier.value;
			HitHistoryManager hitHistoryManager = (_group ? new HitHistoryManager(15) : null);
			for (int i = 0; i < values.Length; i++)
			{
				float direction;
				bool flipX;
				switch (_directionType)
				{
				case DirectionType.RotationOfFirePosition:
					direction = _fireTransform.rotation.eulerAngles.z + values[i].value;
					flipX = _fireTransform.lossyScale.x < 0f;
					break;
				case DirectionType.OwnerDirection:
					direction = values[i].value;
					flipX = owner.lookingDirection == Character.LookingDirection.Left;
					break;
				default:
					direction = values[i].value;
					flipX = false;
					break;
				}
				_projectile.reusable.Spawn(_fireTransform.position).GetComponent<Projectile>().Fire(owner, attackDamage, direction, flipX, false, _speedMultiplier.value, _group ? hitHistoryManager : null);
			}
		}
	}
}
