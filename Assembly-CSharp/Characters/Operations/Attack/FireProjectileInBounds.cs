using Characters.Projectiles;
using FX;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class FireProjectileInBounds : CharacterOperation
	{
		public enum DirectionType
		{
			OwnerDirection,
			Constant,
			RotationOfReferenceTransform
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
		private Collider2D _area;

		[SerializeField]
		private EffectInfo _spawnEffect;

		[Space]
		[SerializeField]
		private bool _flipXByOwnerDirection;

		[SerializeField]
		private bool _flipYByOwnerDirection;

		[Space]
		[SerializeField]
		private DirectionType _directionType;

		[Tooltip("DirectionType을 ReferenceTransform으로 설정했을 경우 이 Transform을 참조합니다.")]
		[SerializeField]
		private Transform _rotationReference;

		[SerializeField]
		private CustomAngle.Reorderable _directions = new CustomAngle.Reorderable(new CustomAngle(0f));

		private IAttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
			if (_rotationReference == null)
			{
				_rotationReference = base.transform;
			}
		}

		public override void Run(Character owner)
		{
			CustomAngle[] values = _directions.values;
			bool flipX = false;
			bool flipY = false;
			for (int i = 0; i < values.Length; i++)
			{
				float num;
				switch (_directionType)
				{
				case DirectionType.RotationOfReferenceTransform:
					num = _rotationReference.rotation.eulerAngles.z + values[i].value;
					if (_rotationReference.lossyScale.x < 0f)
					{
						num = (180f - num) % 360f;
					}
					break;
				case DirectionType.OwnerDirection:
				{
					num = values[i].value;
					bool flag = owner.lookingDirection == Character.LookingDirection.Left || _area.transform.lossyScale.x < 0f;
					flipX = _flipXByOwnerDirection && flag;
					flipY = _flipYByOwnerDirection && flag;
					num = (flag ? ((180f - num) % 360f) : num);
					break;
				}
				default:
					num = values[i].value;
					break;
				}
				Projectile component = _projectile.reusable.Spawn(MMMaths.RandomPointWithinBounds(_area.bounds)).GetComponent<Projectile>();
				component.transform.localScale = Vector3.one * _scale.value;
				component.Fire(owner, _attackDamage.amount * _damageMultiplier.value, num, flipX, flipY, _speedMultiplier.value);
				_spawnEffect.Spawn(component.transform.position);
			}
		}
	}
}
