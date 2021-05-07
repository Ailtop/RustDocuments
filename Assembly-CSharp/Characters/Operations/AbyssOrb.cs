using Characters.Actions;
using Characters.Projectiles;
using UnityEngine;

namespace Characters.Operations
{
	public class AbyssOrb : CharacterOperation
	{
		public enum DirectionType
		{
			RotationOfFirePosition,
			OwnerDirection,
			Constant
		}

		[SerializeField]
		private ChargeAction _chargeAction;

		[Space]
		[SerializeField]
		private float _scaleMin = 0.2f;

		[SerializeField]
		private float _scaleMax = 1f;

		[SerializeField]
		private float _damageMultiplierMin = 0.2f;

		[SerializeField]
		private float _damageMultiplierMax = 1f;

		[Space]
		[SerializeField]
		private Projectile _incompleteProjectile;

		[SerializeField]
		private Projectile _completeProjectile;

		[Space]
		[SerializeField]
		private Transform _fireTransform;

		[SerializeField]
		private bool _flipXByOwnerDirection;

		[SerializeField]
		private bool _flipYByOwnerDirection;

		[Space]
		[SerializeField]
		private DirectionType _directionType;

		[SerializeField]
		private CustomAngle.Reorderable _directions;

		private IAttackDamage _attackDamage;

		public CustomAngle[] directions => _directions.values;

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
			float num = (_scaleMax - _scaleMin) * _chargeAction.chargedPercent + _scaleMin;
			float num2 = (_damageMultiplierMax - _damageMultiplierMin) * _chargeAction.chargedPercent + _damageMultiplierMin;
			Projectile projectile = ((_chargeAction.chargedPercent < 1f) ? _incompleteProjectile : _completeProjectile);
			CustomAngle[] values = _directions.values;
			float attackDamage = _attackDamage.amount * num2;
			bool flipX = false;
			bool flipY = false;
			for (int i = 0; i < values.Length; i++)
			{
				float num3;
				switch (_directionType)
				{
				case DirectionType.RotationOfFirePosition:
					num3 = _fireTransform.rotation.eulerAngles.z + values[i].value;
					if (_fireTransform.lossyScale.x < 0f)
					{
						num3 = (180f - num3) % 360f;
					}
					break;
				case DirectionType.OwnerDirection:
				{
					num3 = values[i].value;
					bool flag = owner.lookingDirection == Character.LookingDirection.Left || _fireTransform.lossyScale.x < 0f;
					flipX = _flipXByOwnerDirection && flag;
					flipY = _flipYByOwnerDirection && flag;
					num3 = (flag ? ((180f - num3) % 360f) : num3);
					break;
				}
				default:
					num3 = values[i].value;
					break;
				}
				Projectile component = projectile.reusable.Spawn(_fireTransform.position).GetComponent<Projectile>();
				component.transform.localScale = Vector3.one * num;
				component.Fire(owner, attackDamage, num3, flipX, flipY);
			}
		}
	}
}
