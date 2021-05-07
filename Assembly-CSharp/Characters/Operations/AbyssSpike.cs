using System.Collections;
using System.Collections.Generic;
using Characters.Actions;
using Characters.Projectiles;
using FX;
using UnityEngine;

namespace Characters.Operations
{
	public class AbyssSpike : CharacterOperation
	{
		public enum DirectionType
		{
			OwnerDirection,
			Constant
		}

		[SerializeField]
		private ChargeAction _chargeAction;

		[Space]
		[SerializeField]
		[Tooltip("차지를 하나도 안 했을 때 프로젝타일 개수")]
		private int _projectileCountMin = 1;

		[SerializeField]
		[Tooltip("풀차지 했을 때 프로젝타일 개수")]
		private int _projectileCountMax = 10;

		[Space]
		[Tooltip("프로젝타일 발사 간격")]
		[SerializeField]
		private CustomFloat _fireInterval;

		[Space]
		[SerializeField]
		private EffectInfo _spawnEffect;

		[SerializeField]
		private Projectile _incompleteProjectile;

		[SerializeField]
		private Projectile _completeProjectile;

		[SerializeField]
		private bool _flipXByOwnerDirection;

		[SerializeField]
		private bool _flipYByOwnerDirection;

		[SerializeField]
		private DirectionType _directionType;

		[SerializeField]
		private CustomAngle.Reorderable _directions = new CustomAngle.Reorderable(new CustomAngle(0f));

		private IAttackDamage _attackDamage;

		[SerializeField]
		private Collider2D _area;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
		}

		public override void Run(Character owner)
		{
			StartCoroutine(CFire(owner));
		}

		private IEnumerator CFire(Character owner)
		{
			int count = (int)((float)(_projectileCountMax - _projectileCountMin) * _chargeAction.chargedPercent) + _projectileCountMin;
			Projectile projectile = ((_chargeAction.chargedPercent < 1f) ? _incompleteProjectile : _completeProjectile);
			Bounds bounds = _area.bounds;
			Character.LookingDirection lookingDirection = owner.lookingDirection;
			for (int i = 0; i < count; i++)
			{
				Fire(owner, projectile, bounds, lookingDirection);
				yield return owner.chronometer.animation.WaitForSeconds(_fireInterval.value);
			}
		}

		private void Fire(Character owner, Projectile projectile, Bounds bounds, Character.LookingDirection lookingDirection)
		{
			CustomAngle[] values = _directions.values;
			List<Vector2> list = new List<Vector2>(values.Length);
			for (int i = 0; i < values.Length; i++)
			{
				list.Add(MMMaths.RandomPointWithinBounds(bounds));
			}
			if (_directionType == DirectionType.OwnerDirection)
			{
				for (int j = 0; j < values.Length; j++)
				{
					float value = values[j].value;
					if (_spawnEffect != null)
					{
						_spawnEffect.Spawn(list[j], owner, value);
					}
					bool flag = lookingDirection == Character.LookingDirection.Left;
					bool flipX = _flipXByOwnerDirection && flag;
					bool flipY = _flipYByOwnerDirection && flag;
					value = (flag ? ((180f - value) % 360f) : value);
					projectile.reusable.Spawn(list[j]).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, value, flipX, flipY);
				}
				return;
			}
			for (int k = 0; k < values.Length; k++)
			{
				float value = values[k].value;
				if (_spawnEffect != null)
				{
					_spawnEffect.Spawn(list[k], owner, value);
				}
				if (_area.transform.lossyScale.x < 0f)
				{
					value = (180f - value) % 360f;
				}
				projectile.reusable.Spawn(list[k]).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, value);
			}
		}
	}
}
