using PhysicsUtils;
using UnityEngine;

namespace Characters.Operations.SetPosition
{
	public class ToObject : Policy
	{
		[SerializeField]
		private LayerMask _groundMask = Layers.groundMask;

		[SerializeField]
		private GameObject _object;

		[SerializeField]
		private bool _onPlatform;

		private static NonAllocCaster _belowCaster;

		static ToObject()
		{
			_belowCaster = new NonAllocCaster(1);
		}

		public override Vector2 GetPosition()
		{
			if (!_onPlatform)
			{
				return _object.transform.position;
			}
			return GetProjectionPointToPlatform(_groundMask);
		}

		private Vector2 GetProjectionPointToPlatform(LayerMask layerMask, float distance = 100f)
		{
			_belowCaster.contactFilter.SetLayerMask(layerMask);
			_belowCaster.RayCast(_object.transform.position, Vector2.down, distance);
			ReadonlyBoundedList<RaycastHit2D> results = _belowCaster.results;
			if (results.Count < 0)
			{
				return _object.transform.position;
			}
			int index = 0;
			float num = results[0].distance;
			for (int i = 1; i < results.Count; i++)
			{
				float distance2 = results[i].distance;
				if (distance2 < num)
				{
					num = distance2;
					index = i;
				}
			}
			return results[index].point;
		}
	}
}
