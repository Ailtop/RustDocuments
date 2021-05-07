using Characters.Projectiles;
using Characters.Utils;
using UnityEngine;

namespace Characters.Operations.Attack
{
	public class MultipleFireProjectile : CharacterOperation
	{
		public enum DirectionType
		{
			RotationOfFirePosition,
			RotationOfCenter,
			OwnerDirection,
			Constant
		}

		[SerializeField]
		private Projectile _projectile;

		[SerializeField]
		private Transform _fireTransformsParent;

		[SerializeField]
		private bool _group;

		[SerializeField]
		private DirectionType _directionType;

		[SerializeField]
		private CustomAngle.Reorderable _directions;

		[SerializeField]
		private IAttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<IAttackDamage>();
		}

		public override void Run(Character owner)
		{
			CustomAngle[] values = _directions.values;
			HitHistoryManager hitHistoryManager = (_group ? new HitHistoryManager(15) : null);
			foreach (Transform item in _fireTransformsParent)
			{
				if (_directionType == DirectionType.RotationOfFirePosition)
				{
					for (int i = 0; i < values.Length; i++)
					{
						_projectile.reusable.Spawn(item.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, item.localRotation.eulerAngles.z + values[i].value, item.lossyScale.x < 0f);
					}
				}
				else if (_directionType == DirectionType.OwnerDirection)
				{
					for (int j = 0; j < values.Length; j++)
					{
						_projectile.reusable.Spawn(item.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, values[j].value, owner.lookingDirection == Character.LookingDirection.Left);
					}
				}
				else
				{
					for (int k = 0; k < values.Length; k++)
					{
						_projectile.reusable.Spawn(item.position).GetComponent<Projectile>().Fire(owner, _attackDamage.amount, values[k].value, false, false, 1f, _group ? hitHistoryManager : null);
					}
				}
			}
		}
	}
}
