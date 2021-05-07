using Characters.Abilities.Customs;
using FX;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Operations.Customs.BombSkul
{
	public class SummonSmallBomb : CharacterOperation
	{
		private static short spriteLayer = short.MinValue;

		[SerializeField]
		private BombSkulPassiveComponent _passvieComponent;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private CustomFloat _scale = new CustomFloat(1f);

		[SerializeField]
		private CustomAngle _angle;

		[SerializeField]
		private PositionNoise _noise;

		[Tooltip("주인이 바라보고 있는 방향에 따라 X축으로 플립")]
		[SerializeField]
		private bool _flipXByLookingDirection;

		[Tooltip("X축 플립")]
		[SerializeField]
		private bool _flipX;

		[SerializeField]
		private bool _copyAttackDamage;

		[SerializeField]
		private Vector2 _minVelocity;

		[SerializeField]
		private Vector2 _maxVelocity;

		private AttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<AttackDamage>();
		}

		public override void Run(Character owner)
		{
			Vector3 position = ((_spawnPosition == null) ? base.transform.position : (_spawnPosition.position + _noise.Evaluate()));
			Vector3 euler = new Vector3(0f, 0f, _angle.value);
			bool num = _flipXByLookingDirection && owner.lookingDirection == Character.LookingDirection.Left;
			if (num)
			{
				euler.z = (180f - euler.z) % 360f;
			}
			if (_flipX)
			{
				euler.z = (180f - euler.z) % 360f;
			}
			OperationRunner operationRunner = _operationRunner.Spawn();
			_passvieComponent.RegisterSmallBomb(operationRunner);
			OperationInfos operationInfos = operationRunner.operationInfos;
			operationInfos.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
			if (_copyAttackDamage && _attackDamage != null)
			{
				operationRunner.attackDamage.minAttackDamage = _attackDamage.minAttackDamage;
				operationRunner.attackDamage.maxAttackDamage = _attackDamage.maxAttackDamage;
			}
			SortingGroup component = operationRunner.GetComponent<SortingGroup>();
			if (component != null)
			{
				component.sortingOrder = spriteLayer++;
			}
			if (num)
			{
				operationInfos.transform.localScale = new Vector3(1f, -1f, 1f) * _scale.value;
			}
			else
			{
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f) * _scale.value;
			}
			if (_flipX)
			{
				operationInfos.transform.localScale = new Vector3(1f, -1f, 1f) * _scale.value;
			}
			operationRunner.GetComponent<Rigidbody2D>().velocity = MMMaths.RandomVector2(_minVelocity, _maxVelocity);
		}
	}
}
