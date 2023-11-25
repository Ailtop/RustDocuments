using Rust;

namespace UnityEngine;

public static class ColliderEx
{
	public static PhysicMaterial GetMaterialAt(this Collider obj, Vector3 pos)
	{
		if (obj == null)
		{
			return TerrainMeta.Config.WaterMaterial;
		}
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
			return GameObjectEx.IsOnLayer(col.gameObject, rustLayer);
		}
		return false;
	}

	public static bool IsOnLayer(this Collider col, int layer)
	{
		if (col != null)
		{
			return GameObjectEx.IsOnLayer(col.gameObject, layer);
		}
		return false;
	}

	public static float GetRadius(this Collider col, Vector3 transformScale)
	{
		float result = 1f;
		if (col is SphereCollider sphereCollider)
		{
			result = sphereCollider.radius * transformScale.Max();
		}
		else if (col is BoxCollider boxCollider)
		{
			result = Vector3.Scale(boxCollider.size, transformScale).Max() * 0.5f;
		}
		else if (col is CapsuleCollider { direction: var direction } capsuleCollider)
		{
			float num = direction switch
			{
				0 => transformScale.y, 
				1 => transformScale.x, 
				_ => transformScale.x, 
			};
			result = capsuleCollider.radius * num;
		}
		else if (col is MeshCollider { bounds: var bounds })
		{
			result = Vector3.Scale(bounds.size, transformScale).Max() * 0.5f;
		}
		return result;
	}
}
