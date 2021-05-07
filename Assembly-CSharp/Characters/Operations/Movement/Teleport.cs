using System.Collections.Generic;
using Level;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.Movement
{
	public class Teleport : CharacterOperation
	{
		private enum Type
		{
			TeleportUponGround,
			Teleport
		}

		private enum DistanceType
		{
			Constant,
			TargetDistance,
			Range
		}

		[SerializeField]
		private Type _type;

		[SerializeField]
		private DistanceType _distanceType;

		[SerializeField]
		private Collider2D _distanceArea;

		[SerializeField]
		private float _minDistance;

		[SerializeField]
		private float _maxDistance;

		[SerializeField]
		private Transform _targetPosition;

		private static readonly NonAllocOverlapper _nonAllocOverlapper;

		static Teleport()
		{
			_nonAllocOverlapper = new NonAllocOverlapper(15);
		}

		private void Awake()
		{
			if (_maxDistance <= 0f)
			{
				_maxDistance = float.PositiveInfinity;
			}
		}

		public override void Run(Character owner)
		{
			if (_distanceType == DistanceType.Range)
			{
				TeleportByRange(owner);
			}
			else if (_distanceType == DistanceType.Constant)
			{
				TeleportByDistanceInPlatform(owner);
			}
			else if (_distanceType == DistanceType.TargetDistance)
			{
				TeleportByTarget(owner);
			}
		}

		private void TeleportByRange(Character owner)
		{
			if (_type == Type.Teleport)
			{
				_targetPosition.position = MMMaths.RandomVector3(_distanceArea.bounds.min, _distanceArea.bounds.max);
				owner.movement.controller.Teleport(_targetPosition.position);
				return;
			}
			_nonAllocOverlapper.contactFilter.SetLayerMask(Layers.groundMask);
			ReadonlyBoundedList<Collider2D> results = _nonAllocOverlapper.OverlapCollider(_distanceArea).results;
			if (results.Count == 0)
			{
				Debug.LogError("Failed to teleport, you can widen distanceArea");
				return;
			}
			List<Collider2D> list = new List<Collider2D>(results.Count);
			Collider2D lastStandingCollider = owner.movement.controller.collisionState.lastStandingCollider;
			foreach (Collider2D item in results)
			{
				if (Map.Instance.bounds.min.x > item.bounds.min.x || Map.Instance.bounds.max.x < item.bounds.max.x)
				{
					continue;
				}
				ColliderDistance2D colliderDistance2D = Physics2D.Distance(item, owner.collider);
				if (lastStandingCollider == item)
				{
					if (owner.transform.position.x + _minDistance < lastStandingCollider.bounds.max.x || owner.transform.position.x - _minDistance > lastStandingCollider.bounds.min.x)
					{
						list.Add(item);
					}
				}
				else if (colliderDistance2D.distance >= _minDistance)
				{
					list.Add(item);
				}
			}
			if (list.Count == 0)
			{
				Debug.LogError("Failed to teleport, you can widen distanceArea");
				return;
			}
			int index = Random.Range(0, list.Count);
			Bounds bounds = list[index].bounds;
			if (bounds == lastStandingCollider.bounds)
			{
				bool num = owner.transform.position.x + _minDistance < lastStandingCollider.bounds.max.x;
				bool flag = owner.transform.position.x - _minDistance > lastStandingCollider.bounds.min.x;
				float x = ((!num || (flag && !MMMaths.RandomBool())) ? Random.Range(owner.transform.position.x + _minDistance, Mathf.Min(lastStandingCollider.bounds.max.x, owner.transform.position.x + _maxDistance)) : Random.Range(Mathf.Max(lastStandingCollider.bounds.min.x, owner.transform.position.x - _maxDistance), owner.transform.position.x - _minDistance));
				_targetPosition.position = new Vector2(x, lastStandingCollider.bounds.max.y);
			}
			else
			{
				_targetPosition.position = new Vector2(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y);
			}
			owner.movement.controller.TeleportUponGround(_targetPosition.position);
		}

		private void TeleportByTarget(Character owner)
		{
			if (_type == Type.Teleport)
			{
				owner.movement.controller.Teleport(_targetPosition.position);
			}
			else
			{
				owner.movement.controller.TeleportUponGround(FilterDest(owner, _targetPosition.position));
			}
		}

		private float FilterDestX(float extends, float position)
		{
			Bounds bounds = Map.Instance.bounds;
			if (position + extends >= bounds.max.x)
			{
				return position - extends;
			}
			if (position - extends <= bounds.min.x)
			{
				return position + extends;
			}
			return position;
		}

		private Vector2 FilterDest(Character owner, Vector2 position)
		{
			return new Vector2(FilterDestX(owner.collider.bounds.extents.x, position.x), position.y);
		}

		private void TeleportByDistanceInPlatform(Character owner)
		{
		}
	}
}
