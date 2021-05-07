using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Characters.Operations;
using FX;
using PhysicsUtils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Projectiles.Operations
{
	public class SummonOperationRunnersOnGround : Operation
	{
		private const int _maxTerrainCount = 16;

		private static short spriteLayer = short.MinValue;

		[SerializeField]
		private BoxCollider2D _terrainFindingRange;

		[Tooltip("플랫폼도 포함할 것인지")]
		[SerializeField]
		private bool _includePlatform = true;

		[SerializeField]
		[Tooltip("오퍼레이션 하나의 너비, 즉 스폰 간격")]
		private float _width;

		[SerializeField]
		private Transform _orderOrigin;

		[Space]
		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[Space]
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

		private NonAllocOverlapper _overlapper;

		[TupleElementNames(new string[] { "a", "b" })]
		private List<ValueTuple<float2, float2>> _surfaces = new List<ValueTuple<float2, float2>>(16);

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(16);
			int num = 262144;
			if (_includePlatform)
			{
				num |= 0x20000;
			}
			_overlapper.contactFilter.SetLayerMask(num);
			_terrainFindingRange.enabled = false;
			if (_orderOrigin == null)
			{
				_orderOrigin = base.transform;
			}
		}

		private void FindSurfaces()
		{
			_terrainFindingRange.enabled = true;
			_overlapper.OverlapCollider(_terrainFindingRange);
			float x = _terrainFindingRange.bounds.min.x;
			float x2 = _terrainFindingRange.bounds.max.x;
			_terrainFindingRange.enabled = false;
			_surfaces.Clear();
			if (_overlapper.results.Count != 0)
			{
				for (int i = 0; i < _overlapper.results.Count; i++)
				{
					Bounds bounds = _overlapper.results[i].bounds;
					float2 item = bounds.GetMostLeftTop();
					float2 item2 = bounds.GetMostRightTop();
					item.x = Mathf.Max(item.x, x);
					item2.x = Mathf.Min(item2.x, x2);
					_surfaces.Add(new ValueTuple<float2, float2>(item, item2));
				}
			}
		}

		public override void Run(Projectile projectile)
		{
			FindSurfaces();
			if (_surfaces.Count != 0)
			{
				SpawnAtOnce(projectile.owner);
			}
		}

		private void SpawnAtOnce(Character owner)
		{
			for (int i = 0; i < _surfaces.Count; i++)
			{
				ValueTuple<float2, float2> valueTuple = _surfaces[i];
				float num = (valueTuple.Item2.x - valueTuple.Item1.x) / _width;
				float num2 = num - (float)(int)num;
				float2 item = valueTuple.Item1;
				item.x = valueTuple.Item1.x + num2 * _width / 2f;
				for (int j = 0; (float)j < num; j++)
				{
					float2 position = item + _noise.EvaluateAsVector2();
					position.x += _width * (float)j;
					Spawn(owner, position);
				}
			}
		}

		private void Spawn(Character owner, float2 position)
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
			operationInfos.transform.SetPositionAndRotation(new Vector3(position.x, position.y), Quaternion.Euler(euler));
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
		}
	}
}
