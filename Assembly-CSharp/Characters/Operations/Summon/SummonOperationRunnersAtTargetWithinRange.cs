using System;
using System.Collections.Generic;
using FX;
using PhysicsUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Operations.Summon
{
	public class SummonOperationRunnersAtTargetWithinRange : CharacterOperation
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

		public enum FindingMethod
		{
			Random,
			CloseToFar,
			FarToClose
		}

		private static short spriteLayer = short.MinValue;

		private NonAllocOverlapper _overlapper;

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

		[Header("Special Settings")]
		[SerializeField]
		private Collider2D _collider;

		[Tooltip("콜라이더 최적화 여부, Composite Collider등 특별한 경우가 아니면 true로 유지")]
		[SerializeField]
		private bool _optimizedCollider = true;

		[SerializeField]
		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		[Tooltip("범위 내 감지가능한 최대 적의 수, 프롭을 포함하지 않으므로 128로 충분")]
		[SerializeField]
		private int _maxCount = 128;

		[SerializeField]
		[Tooltip("스폰될 오퍼레이션러너의 최대 개수")]
		private int _totalOperationCount;

		[SerializeField]
		[Tooltip("하나의 적에게 중첩되어 스폰될 수 있는 최대 개수")]
		private int _maxCountPerUnit = 1;

		[SerializeField]
		private FindingMethod _method;

		[SerializeField]
		[Tooltip("Close To Far, Far To Close 계산 시 기준점이 될 위치, 비워둘 경우 콜라이더의 중심점을 기준으로 함")]
		private Transform _sortOrigin;

		[Space]
		[SerializeField]
		private bool _copyAttackDamage;

		private AttackDamage _attackDamage;

		public override void Initialize()
		{
			_attackDamage = GetComponentInParent<AttackDamage>();
		}

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(_maxCount);
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_collider.enabled = true;
			_overlapper.OverlapCollider(_collider);
			Vector3 origin = ((_sortOrigin != null) ? _sortOrigin.position : _collider.bounds.center);
			if (_optimizedCollider)
			{
				_collider.enabled = false;
			}
			if (_overlapper.results.Count == 0)
			{
				return;
			}
			List<Character> list = new List<Character>(_overlapper.results.Count);
			for (int i = 0; i < _overlapper.results.Count; i++)
			{
				Target component = _overlapper.results[i].GetComponent<Target>();
				if (!(component == null) && !(component.character == null) && component.character.liveAndActive && !(component.character == owner))
				{
					list.Add(component.character);
				}
			}
			if (list.Count == 0)
			{
				return;
			}
			switch (_method)
			{
			case FindingMethod.Random:
				list.PseudoShuffle();
				break;
			case FindingMethod.CloseToFar:
				list.Sort(delegate(Character x, Character y)
				{
					Vector3 vector = origin - x.collider.bounds.center;
					Vector3 vector2 = origin - y.collider.bounds.center;
					return vector.sqrMagnitude.CompareTo(vector2.sqrMagnitude);
				});
				break;
			case FindingMethod.FarToClose:
				list.Sort(delegate(Character x, Character y)
				{
					Vector3 vector3 = origin - x.collider.bounds.center;
					return (origin - y.collider.bounds.center).sqrMagnitude.CompareTo(vector3.sqrMagnitude);
				});
				break;
			}
			int num = _totalOperationCount;
			for (int j = 0; j < _maxCountPerUnit; j++)
			{
				for (int k = 0; k < list.Count; k++)
				{
					SpawnTo(owner, list[k]);
					num--;
					if (num == 0)
					{
						return;
					}
				}
			}
		}

		public void SpawnTo(Character owner, Character target)
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
