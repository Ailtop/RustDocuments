using Characters.Operations;
using FX;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Projectiles.Operations
{
	public sealed class SummonOperationRunnerOnHitPoint : HitOperation
	{
		private static short spriteLayer = short.MinValue;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private CustomFloat _offsetX;

		[SerializeField]
		private CustomFloat _offsetY;

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

		[Header("Interporlation")]
		[Tooltip("콜라이더 끝에 걸쳤을 때 자연스럽게 보이기 위해 위치 보간")]
		[SerializeField]
		private bool _interpolatedPosition;

		[SerializeField]
		private float _interpolatedSize;

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			Character owner = projectile.owner;
			Vector2 vector = raycastHit.point + (Vector2)_noise.Evaluate();
			vector = new Vector2(vector.x + _offsetX.value, vector.y + _offsetY.value);
			if (_interpolatedPosition)
			{
				GetInterpolatedPosition(projectile, raycastHit, ref vector);
			}
			Vector3 euler = new Vector3(0f, 0f, _angle.value);
			bool num = _flipXByLookingDirection && owner.lookingDirection == Character.LookingDirection.Left;
			if (num)
			{
				euler.z = (180f - euler.z) % 360f;
			}
			OperationRunner operationRunner = _operationRunner.Spawn();
			OperationInfos operationInfos = operationRunner.operationInfos;
			operationInfos.transform.SetPositionAndRotation(vector, Quaternion.Euler(euler));
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
			operationInfos.Run(owner);
			if (_attachToOwner)
			{
				operationInfos.transform.parent = base.transform;
			}
		}

		private void GetInterpolatedPosition(Projectile projectile, RaycastHit2D hit, ref Vector2 result)
		{
			float num = _interpolatedSize / 2f;
			Bounds bounds = hit.collider.bounds;
			Vector2 point = hit.point;
			float num2 = projectile.transform.position.x + num;
			float num3 = projectile.transform.position.x - num;
			if (num2 > bounds.max.x)
			{
				result = new Vector2(bounds.max.x - num, result.y);
			}
			if (num3 < bounds.min.x)
			{
				result = new Vector2(bounds.min.x + num, result.y);
			}
		}
	}
}
