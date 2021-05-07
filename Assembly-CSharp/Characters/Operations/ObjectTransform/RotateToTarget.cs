using System.Collections.Generic;
using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.ObjectTransform
{
	public class RotateToTarget : CharacterOperation
	{
		[SerializeField]
		private Transform _object;

		[SerializeField]
		private Collider2D _range;

		[SerializeField]
		private TargetLayer _targetLayer;

		[SerializeField]
		private float _defaultRotation;

		private static readonly NonAllocOverlapper _overlapper;

		static RotateToTarget()
		{
			_overlapper = new NonAllocOverlapper(15);
		}

		public override void Run(Character owner)
		{
			Transform closestTargetTransform = GetClosestTargetTransform(owner);
			if (closestTargetTransform == null)
			{
				float z = ((owner.lookingDirection == Character.LookingDirection.Right) ? _defaultRotation : (_defaultRotation + 180f));
				_object.rotation = Quaternion.Euler(0f, 0f, z);
			}
			else
			{
				Vector3 vector = closestTargetTransform.position - _object.position;
				float z2 = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
				_object.rotation = Quaternion.Euler(0f, 0f, z2);
			}
		}

		private Transform GetClosestTargetTransform(Character owner)
		{
			LayerMask layerMask = _targetLayer.Evaluate(owner.gameObject);
			_overlapper.contactFilter.SetLayerMask(layerMask);
			List<Target> components = _overlapper.OverlapCollider(_range).GetComponents<Target>();
			if (components.Count == 0)
			{
				return null;
			}
			if (components.Count == 1)
			{
				return components[0].transform;
			}
			float num = float.MaxValue;
			int index = 0;
			for (int i = 1; i < components.Count; i++)
			{
				if (!(components[i].character == null))
				{
					float distance = Physics2D.Distance(components[i].character.collider, owner.collider).distance;
					if (num > distance)
					{
						index = i;
						num = distance;
					}
				}
			}
			return components[index].transform;
		}
	}
}
