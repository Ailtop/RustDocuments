using System;
using Characters.Operations;
using PhysicsUtils;
using UnityEngine;
using UnityEngine.Rendering;

namespace Characters.Projectiles.Operations
{
	public class SpreadOperationRunner : HitOperation
	{
		private static short spriteLayer;

		[Tooltip("오퍼레이션 프리팹")]
		[SerializeField]
		internal OperationRunner _operationRunner;

		[SerializeField]
		private LayerMask _groundMask;

		[SerializeField]
		private float _distance;

		private static readonly NonAllocCaster _nonAllocCaster;

		static SpreadOperationRunner()
		{
			spriteLayer = short.MinValue;
			_nonAllocCaster = new NonAllocCaster(1);
		}

		public override void Run(Projectile projectile, RaycastHit2D raycastHit)
		{
			Character owner = projectile.owner;
			ValueTuple<bool, RaycastHit2D> valueTuple = TryRayCast(projectile.transform.position, Vector2.left);
			bool item = valueTuple.Item1;
			RaycastHit2D item2 = valueTuple.Item2;
			if (item)
			{
				SpreadLeft(item2.point, owner);
			}
			ValueTuple<bool, RaycastHit2D> valueTuple2 = TryRayCast(projectile.transform.position, Vector2.right);
			bool item3 = valueTuple2.Item1;
			RaycastHit2D item4 = valueTuple2.Item2;
			if (item3)
			{
				SpreadRight(item4.point, owner);
			}
			ValueTuple<bool, RaycastHit2D> valueTuple3 = TryRayCast(projectile.transform.position, Vector2.up);
			bool item5 = valueTuple3.Item1;
			RaycastHit2D item6 = valueTuple3.Item2;
			if (item5)
			{
				SpreadUp(item6.point, owner);
			}
			ValueTuple<bool, RaycastHit2D> valueTuple4 = TryRayCast(projectile.transform.position, Vector2.down);
			bool item7 = valueTuple4.Item1;
			RaycastHit2D item8 = valueTuple4.Item2;
			if (item7)
			{
				SpreadDown(item8.point, owner);
			}
		}

		private ValueTuple<bool, RaycastHit2D> TryRayCast(Vector2 origin, Vector2 direction)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, direction, _distance).results;
			if (results.Count > 0)
			{
				return new ValueTuple<bool, RaycastHit2D>(true, results[0]);
			}
			return new ValueTuple<bool, RaycastHit2D>(false, default(RaycastHit2D));
		}

		private void SpreadUp(Vector2 origin, Character owner)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, Vector2.up, _distance).results;
			if (results.Count != 0)
			{
				RaycastHit2D raycastHit2D = results[0];
				OperationRunner operationRunner = _operationRunner.Spawn();
				OperationInfos operationInfos = operationRunner.operationInfos;
				if (raycastHit2D.collider.gameObject.layer == 17)
				{
					operationInfos.transform.SetPositionAndRotation(raycastHit2D.point, Quaternion.Euler(0f, 0f, 0f));
				}
				else
				{
					operationInfos.transform.SetPositionAndRotation(raycastHit2D.point, Quaternion.Euler(0f, 0f, 180f));
				}
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
				SortingGroup component = operationRunner.GetComponent<SortingGroup>();
				if (component != null)
				{
					component.sortingOrder = spriteLayer++;
				}
				operationInfos.Run(owner);
			}
		}

		private void SpreadDown(Vector2 origin, Character owner)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, Vector2.down, _distance).results;
			if (results.Count != 0)
			{
				RaycastHit2D raycastHit2D = results[0];
				OperationRunner operationRunner = _operationRunner.Spawn();
				OperationInfos operationInfos = operationRunner.operationInfos;
				operationInfos.transform.SetPositionAndRotation(raycastHit2D.point, Quaternion.Euler(0f, 0f, 0f));
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
				SortingGroup component = operationRunner.GetComponent<SortingGroup>();
				if (component != null)
				{
					component.sortingOrder = spriteLayer++;
				}
				operationInfos.Run(owner);
			}
		}

		private void SpreadLeft(Vector2 origin, Character owner)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, Vector2.left, _distance).results;
			if (results.Count != 0)
			{
				RaycastHit2D raycastHit2D = results[0];
				OperationRunner operationRunner = _operationRunner.Spawn();
				OperationInfos operationInfos = operationRunner.operationInfos;
				operationInfos.transform.SetPositionAndRotation(raycastHit2D.point, Quaternion.Euler(0f, 0f, 270f));
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
				SortingGroup component = operationRunner.GetComponent<SortingGroup>();
				if (component != null)
				{
					component.sortingOrder = spriteLayer++;
				}
				operationInfos.Run(owner);
			}
		}

		private void SpreadRight(Vector2 origin, Character owner)
		{
			_nonAllocCaster.contactFilter.SetLayerMask(_groundMask);
			ReadonlyBoundedList<RaycastHit2D> results = _nonAllocCaster.RayCast(origin, Vector2.right, _distance).results;
			if (results.Count != 0)
			{
				RaycastHit2D raycastHit2D = results[0];
				OperationRunner operationRunner = _operationRunner.Spawn();
				OperationInfos operationInfos = operationRunner.operationInfos;
				operationInfos.transform.SetPositionAndRotation(raycastHit2D.point, Quaternion.Euler(0f, 0f, 90f));
				operationInfos.transform.localScale = new Vector3(1f, 1f, 1f);
				SortingGroup component = operationRunner.GetComponent<SortingGroup>();
				if (component != null)
				{
					component.sortingOrder = spriteLayer++;
				}
				operationInfos.Run(owner);
			}
		}
	}
}
