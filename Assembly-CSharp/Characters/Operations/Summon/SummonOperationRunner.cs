using FX;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Operations.Summon
{
	public class SummonOperationRunner : CharacterOperation
	{
		private static short spriteLayer = short.MinValue;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[Space]
		[SerializeField]
		private Transform _spawnPosition;

		[SerializeField]
		private CustomFloat _scale = new CustomFloat(1f);

		[SerializeField]
		private CustomAngle _angle;

		[SerializeField]
		private PositionNoise _noise;

		[Space]
		[Tooltip("주인이 바라보고 있는 방향에 따라 X축으로 플립")]
		[SerializeField]
		private bool _flipXByLookingDirection;

		[Tooltip("X축 플립")]
		[SerializeField]
		private bool _flipX;

		[Space]
		[SerializeField]
		private bool _snapToGround;

		[SerializeField]
		[Tooltip("땅을 찾기 위해 소환지점으로부터 아래로 탐색할 거리. 실패 시 그냥 소환 지점에 소환됨")]
		private float _groundFindingDistance = 7f;

		[Space]
		[Tooltip("체크하면 주인에 부착되며, 같이 움직임")]
		[SerializeField]
		private bool _attachToOwner;

		[SerializeField]
		private bool _copyAttackDamage;

		private AttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<AttackDamage>();
		}

		public override void Run(Character owner)
		{
			Vector3 vector = ((_spawnPosition == null) ? base.transform.position : _spawnPosition.position);
			if (_snapToGround)
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, Vector2.down, _groundFindingDistance, Layers.groundMask);
				if ((bool)raycastHit2D)
				{
					vector = raycastHit2D.point;
				}
			}
			vector += _noise.Evaluate();
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
			OperationInfos operationInfos = operationRunner.operationInfos;
			operationInfos.transform.SetPositionAndRotation(vector, Quaternion.Euler(euler));
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
			operationInfos.Run(owner);
			if (_attachToOwner)
			{
				operationInfos.transform.parent = base.transform;
			}
		}
	}
}
