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
			SphereCollider sphereCollider;
			if ((object)(sphereCollider = col as SphereCollider) != null)
			{
				return sphereCollider.center;
			}
			BoxCollider boxCollider;
			if ((object)(boxCollider = col as BoxCollider) != null)
			{
				return boxCollider.center;
			}
			CapsuleCollider capsuleCollider;
			if ((object)(capsuleCollider = col as CapsuleCollider) != null)
			{
				return capsuleCollider.center;
			}
			bool flag = col is MeshCollider;
			return Vector3.zero;
		}

		public static float GetRadius(this Collider col, Vector3 transformScale)
		{
			float result = 1f;
			SphereCollider sphereCollider;
			BoxCollider boxCollider;
			CapsuleCollider capsuleCollider;
			MeshCollider meshCollider;
			if ((object)(sphereCollider = col as SphereCollider) != null)
			{
				result = sphereCollider.radius * transformScale.Max();
			}
			else if ((object)(boxCollider = col as BoxCollider) != null)
			{
				result = Vector3.Scale(boxCollider.size, transformScale).Max() * 0.5f;
			}
			else if ((object)(capsuleCollider = col as CapsuleCollider) != null)
			{
				float num;
				switch (capsuleCollider.direction)
				{
				case 0:
					num = transformScale.y;
					break;
				case 1:
					num = transformScale.x;
					break;
				default:
					num = transformScale.x;
					break;
				}
				result = capsuleCollider.radius * num;
			}
			else if ((object)(meshCollider = col as MeshCollider) != null)
			{
				result = Vector3.Scale(meshCollider.bounds.size, transformScale).Max() * 0.5f;
			}
			return result;
		}
	}
}
