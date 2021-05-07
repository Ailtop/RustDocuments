using Characters.Operations;
using FX;
using UnityEngine;

namespace Characters.Projectiles.Operations
{
	public sealed class SummonOperationRunner : Operation
	{
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

		[Tooltip("체크하면 주인에 부착되며, 같이 움직임")]
		[SerializeField]
		private bool _attachToOwner;

		public override void Run(Projectile projectile)
		{
			Character owner = projectile.owner;
			Vector3 position = ((_spawnPosition == null) ? base.transform.position : (_spawnPosition.position + _noise.Evaluate()));
			Vector3 euler = new Vector3(0f, 0f, _angle.value);
			bool num = _flipXByLookingDirection && owner.lookingDirection == Character.LookingDirection.Left;
			if (num)
			{
				euler.z = (180f - euler.z) % 360f;
			}
			OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
			operationInfos.transform.SetPositionAndRotation(position, Quaternion.Euler(euler));
			if (num)
			{
				operationInfos.transform.localScale = new Vector3(1f, -1f, 1f) * _scale.value;
			}
			else
			{
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f) * _scale.value;
			}
			operationInfos.Run(owner);
			if (_attachToOwner)
			{
				operationInfos.transform.parent = base.transform;
			}
		}
	}
}
