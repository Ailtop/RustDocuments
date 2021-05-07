using System;
using FX;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Operations.Summon
{
	public class SummonOperationRunnerAtTarget : TargetedCharacterOperation
	{
		[Serializable]
		public class PositionInfo
		{
			public enum Pivot
			{
				Center,
				TopLeft,
				Top,
				TopRight,
				Left,
				Right,
				BottomLeft,
				Bottom,
				BottomRight,
				Custom
			}

			private static readonly EnumArray<Pivot, Vector2> _pivotValues = new EnumArray<Pivot, Vector2>(new Vector2(0f, 0f), new Vector2(-0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-0.5f, 0f), new Vector2(0f, 0.5f), new Vector2(-0.5f, -0.5f), new Vector2(0f, -0.5f), new Vector2(0.5f, -0.5f), new Vector2(0f, 0f));

			[SerializeField]
			private Pivot _pivot;

			[SerializeField]
			[HideInInspector]
			private Vector2 _pivotValue;

			public Pivot pivot => _pivot;

			public Vector2 pivotValue => _pivotValue;

			public PositionInfo()
			{
				_pivot = Pivot.Center;
				_pivotValue = Vector2.zero;
			}

			public PositionInfo(bool attach, bool layerOnly, int layerOrderOffset, Pivot pivot)
			{
				_pivot = pivot;
				_pivotValue = _pivotValues[pivot];
			}
		}

		private static short spriteLayer = short.MinValue;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private PositionInfo _positionInfo;

		[SerializeField]
		private bool _attachToTarget;

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

		private AttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<AttackDamage>();
		}

		public override void Run(Character owner, Character target)
		{
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
			if (_copyAttackDamage && _attackDamage != null)
			{
				operationRunner.attackDamage.minAttackDamage = _attackDamage.minAttackDamage;
				operationRunner.attackDamage.maxAttackDamage = _attackDamage.maxAttackDamage;
			}
			Vector3 position = target.transform.position;
			position.x += target.collider.offset.x;
			position.y += target.collider.offset.y;
			Vector3 size = target.collider.bounds.size;
			size.x *= _positionInfo.pivotValue.x;
			size.y *= _positionInfo.pivotValue.y;
			operationInfos.transform.SetPositionAndRotation(position + size, Quaternion.Euler(euler));
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
			if (_attachToTarget)
			{
				operationInfos.transform.parent = target.transform;
			}
			operationInfos.Run(owner);
		}
	}
}
