using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Customs.EntSkul
{
	public class EntSkulThornyVine : CharacterOperation
	{
		[SerializeField]
		private int _count = 15;

		[SerializeField]
		private Collider2D _searchRange;

		[Tooltip("가시가 항상 바닥에 나와야해서, 적 기준으로 바로 아래쪽 땅을 찾는데 그 때 땅을 찾기 위한 최대 거리를 의미함")]
		[SerializeField]
		private float _groundFinderDirection = 5f;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper;

		private RayCaster _groundFinder;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		private OperationRunner _operationRunner;

		private void Awake()
		{
			StartCoroutine(_operationRunner.poolObject.CPreloadAsync(_count));
			_overlapper = new NonAllocOverlapper(_count);
			_groundFinder = new RayCaster
			{
				direction = Vector2.down,
				distance = _groundFinderDirection
			};
			_groundFinder.contactFilter.SetLayerMask(Layers.groundMask);
			_searchRange.enabled = false;
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_searchRange.enabled = true;
			_overlapper.OverlapCollider(_searchRange);
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				_searchRange.enabled = false;
				return;
			}
			_searchRange.enabled = false;
			foreach (Target item in components)
			{
				if (!(item.character == null))
				{
					_groundFinder.origin = item.transform.position;
					RaycastHit2D raycastHit2D = _groundFinder.SingleCast();
					if (!raycastHit2D)
					{
						break;
					}
					SpawnOperationRunner(owner, raycastHit2D.point);
				}
			}
		}

		private void SpawnOperationRunner(Character owner, Vector3 position)
		{
			OperationInfos operationInfos = _operationRunner.Spawn().operationInfos;
			operationInfos.transform.SetPositionAndRotation(position, Quaternion.identity);
			operationInfos.Run(owner);
		}
	}
}
