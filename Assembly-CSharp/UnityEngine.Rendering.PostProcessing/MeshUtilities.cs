#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace UnityEngine.Rendering.PostProcessing;

internal static class MeshUtilities
{
	private static Dictionary<PrimitiveType, Mesh> s_Primitives;

	private static Dictionary<Type, PrimitiveType> s_ColliderPrimitives;

	static MeshUtilities()
	{
		s_Primitives = new Dictionary<PrimitiveType, Mesh>();
		s_ColliderPrimitives = new Dictionary<Type, PrimitiveType>
		{
			{
				typeof(BoxCollider),
				PrimitiveType.Cube
			},
			{
				typeof(SphereCollider),
				PrimitiveType.Sphere
			},
			{
				typeof(CapsuleCollider),
				PrimitiveType.Capsule
			}
		};
	}

	internal static Mesh GetColliderMesh(Collider collider)
	{
		Type type = collider.GetType();
		if (type == typeof(MeshCollider))
		{
			return ((MeshCollider)collider).sharedMesh;
		}
		Assert.IsTrue(s_ColliderPrimitives.ContainsKey(type), "Unknown collider");
		return GetPrimitive(s_ColliderPrimitives[type]);
	}

	internal static Mesh GetPrimitive(PrimitiveType primitiveType)
	{
		if (!s_Primitives.TryGetValue(primitiveType, out var value))
		{
			value = GetBuiltinMesh(primitiveType);
			s_Primitives.Add(primitiveType, value);
		}
		return value;
	}

	private static Mesh GetBuiltinMesh(PrimitiveType primitiveType)
	{
		GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
		Mesh sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		RuntimeUtilities.Destroy(gameObject);
		return sharedMesh;
	}
}
