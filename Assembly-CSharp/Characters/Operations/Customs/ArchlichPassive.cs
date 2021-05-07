using System;
using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Customs
{
	public class ArchlichPassive : CharacterOperation
	{
		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		[SerializeField]
		private Collider2D _searchRange;

		private const int _spawnCount = 5;

		private const int _radius = 2;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper;

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(5);
			_searchRange.enabled = false;
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_searchRange.enabled = true;
			_overlapper.OverlapCollider(_searchRange);
			_searchRange.enabled = false;
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				return;
			}
			float num = UnityEngine.Random.value * (float)Math.PI * 2f;
			foreach (Target item in components)
			{
				Bounds bounds = item.collider.bounds;
				Vector3 center = bounds.center;
				float num3 = (bounds.size.x + bounds.size.y) / 2f;
				int num2 = 5 - components.Count + 1;
				for (int i = 0; i < num2; i++)
				{
					num += (1f + UnityEngine.Random.value) / (float)(num2 * 2) * (float)Math.PI * 2f;
					Vector3 position = center;
					position.x += Mathf.Cos(num) * 2f;
					position.y += Mathf.Sin(num) * 2f;
					OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
					operationInfos.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, num * 57.29578f + 180f));
					operationInfos.Initialize();
					operationInfos.Run(owner);
				}
			}
		}
	}
}
