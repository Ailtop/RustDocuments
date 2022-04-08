using Rust;

namespace UnityEngine;

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

	public static Vector3 GetLocalCentre(this Collider col)
	{
		if (col is SphereCollider sphereCollider)
		{
			return sphereCollider.center;
		}
		if (col is BoxCollider boxCollider)
		{
			return boxCollider.center;
		}
		if (col is CapsuleCollider capsuleCollider)
		{
			return capsuleCollider.center;
		}
		_ = col is MeshCollider;
		return Vector3.zero;
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
		else if (col is CapsuleCollider capsuleCollider)
		{
			float num = capsuleCollider.direction switch
			{
				0 => transformScale.y, 
				1 => transformScale.x, 
				_ => transformScale.x, 
			};
			result = capsuleCollider.radius * num;
		}
		else if (col is MeshCollider meshCollider)
		{
			result = Vector3.Scale(meshCollider.bounds.size, transformScale).Max() * 0.5f;
		}
		return result;
	}
}
