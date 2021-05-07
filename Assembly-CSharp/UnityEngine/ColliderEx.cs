using Rust;

namespace UnityEngine
{
	public static class ColliderEx
	{
		public static PhysicMaterial GetMaterialAt(this Collider obj, Vector3 pos)
		{
			if (obj is TerrainCollider)
			{
				return TerrainMeta.Physics.GetMaterial(pos);
			}
			return obj.sharedMaterial;
		}

		public static bool IsOnLayer(this Collider col, Layer rustLayer)
		{
			if (col != null)
			{
				return col.gameObject.IsOnLayer(rustLayer);
			}
			return false;
		}

		public static bool IsOnLayer(this Collider col, int layer)
		{
			if (col != null)
			{
				return col.gameObject.IsOnLayer(layer);
			}
			return false;
		}
	}
}
