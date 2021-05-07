using System.Collections.Generic;
using PhysicsUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Operations
{
	public class FingerOfThunderbolt : CharacterOperation
	{
		[SerializeField]
		[FormerlySerializedAs("_serachRange")]
		private Collider2D _searchRange;

		private TargetLayer _layer = new TargetLayer(0, false, true, false, false);

		private NonAllocOverlapper _overlapper;

		private RayCaster _groundFinder;

		[SerializeField]
		private Transform _thunderboltPosition;

		[SerializeField]
		[UnityEditor.Subcomponent(typeof(OperationInfo))]
		private OperationInfo.Subcomponents _operations;

		private void Awake()
		{
			_overlapper = new NonAllocOverlapper(5);
			_groundFinder = new RayCaster
			{
				direction = Vector2.down,
				distance = 5f
			};
			_groundFinder.contactFilter.SetLayerMask(Layers.groundMask);
			_searchRange.enabled = false;
		}

		private void OnEnable()
		{
			_thunderboltPosition.transform.parent = null;
		}

		protected override void OnDestroy()
		{
			Object.Destroy(_thunderboltPosition.gameObject);
		}

		public override void Initialize()
		{
			base.Initialize();
			_operations.Initialize();
		}

		public override void Run(Character owner)
		{
			_overlapper.contactFilter.SetLayerMask(_layer.Evaluate(owner.gameObject));
			_searchRange.enabled = true;
			_overlapper.OverlapCollider(_searchRange);
			List<Target> components = _overlapper.results.GetComponents<Collider2D, Target>();
			if (components.Count == 0)
			{
				_overlapper.contactFilter.SetLayerMask(2048);
				_overlapper.OverlapCollider(_searchRange);
				components = _overlapper.results.GetComponents<Collider2D, Target>();
				if (components.Count == 0)
				{
					_searchRange.enabled = false;
					return;
				}
			}
			_searchRange.enabled = false;
			Target target = components.Random();
			_groundFinder.origin = target.transform.position;
			RaycastHit2D raycastHit2D = _groundFinder.SingleCast();
			if ((bool)raycastHit2D)
			{
				_thunderboltPosition.position = raycastHit2D.point;
				StartCoroutine(_operations.CRun(owner));
			}
		}
	}
}
