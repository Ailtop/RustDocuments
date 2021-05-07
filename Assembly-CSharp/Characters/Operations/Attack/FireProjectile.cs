using Characters.Projectiles;
using Characters.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Operations.Attack
{
	public class FireProjectile : CharacterOperation
	{
		public enum DirectionType
		{
			RotationOfFirePosition,
			OwnerDirection,
			Constant
		}

		[SerializeField]
		private Projectile _projectile;

		[Space]
		[SerializeField]
		private CustomFloat _speedMultiplier = new CustomFloat(1f);

		[SerializeField]
		private CustomFloat _damageMultiplier = new CustomFloat(1f);

		[SerializeField]
		private CustomFloat _scale = new CustomFloat(1f);

		[Space]
		[SerializeField]
		private Transform _fireTransform;

		[SerializeField]
		private bool _group;

		[SerializeField]
		private bool _flipXByOwnerDirection;

		[SerializeField]
		[FormerlySerializedAs("_flipY")]
		private bool _flipYByOwnerDirection;

		[Space]
		[SerializeField]
		private DirectionType _directionType;

		[SerializeField]
		private CustomAngle.Reorderable _directions;

		private IAttackDamage _attackDamage;

		public CustomFloat scale => _scale;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
			if (_fireTransform == null)
			{
				_fireTransform = base.transform;
			}
		}

		public override void Run(Character owner)
		{
			CustomAngle[] values = _directions.values;
			bool flipX = false;
			bool flipY = false;
			HitHistoryManager hitHistoryManager = (_group ? new HitHistoryManager(15) : null);
			for (int i = 0; i < values.Length; i++)
			{
				float num;
				switch (_directionType)
				{
				case DirectionType.RotationOfFirePosition:
					num = _fireTransform.rotation.eulerAngles.z + values[i].value;
					if (_fireTransform.lossyScale.x < 0f)
					{
						num = (180f - num) % 360f;
					}
					break;
				case DirectionType.OwnerDirection:
				{
					num = values[i].value;
					bool flag = owner.lookingDirection == Character.LookingDirection.Left || _fireTransform.lossyScale.x < 0f;
					flipX = _flipXByOwnerDirection && flag;
					flipY = _flipYByOwnerDirection && flag;
					num = (flag ? ((180f - num) % 360f) : num);
					break;
				}
				default:
					num = values[i].value;
					break;
				}
				Projectile component = _projectile.reusable.Spawn(_fireTransform.position).GetComponent<Projectile>();
				component.transform.localScale = Vector3.one * _scale.value;
				component.Fire(owner, _attackDamage.amount * _damageMultiplier.value, num, flipX, flipY, _speedMultiplier.value, _group ? hitHistoryManager : null);
			}
		}
	}
}
