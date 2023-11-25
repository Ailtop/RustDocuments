using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace CompanionServer.Cameras;

internal static class BurstUtil
{
	private struct RaycastHitPublic
	{
		public Vector3 m_Point;

		public Vector3 m_Normal;

		public uint m_FaceID;

		public float m_Distance;

		public Vector2 m_UV;

		public int m_Collider;
	}

	public unsafe static ref readonly T GetReadonly<T>(this in NativeArray<T> array, int index) where T : unmanaged
	{
		T* unsafeReadOnlyPtr = (T*)array.GetUnsafeReadOnlyPtr();
		return ref unsafeReadOnlyPtr[index];
	}

	public unsafe static ref T Get<T>(this in NativeArray<T> array, int index) where T : unmanaged
	{
		T* unsafePtr = (T*)array.GetUnsafePtr();
		return ref unsafePtr[index];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetColliderId(this RaycastHit hit)
	{
		return hit.colliderInstanceID;
	}

	public unsafe static Collider GetCollider(int colliderInstanceId)
	{
		RaycastHitPublic raycastHitPublic = default(RaycastHitPublic);
		raycastHitPublic.m_Collider = colliderInstanceId;
		RaycastHitPublic raycastHitPublic2 = raycastHitPublic;
		return ((RaycastHit*)(&raycastHitPublic2))->collider;
	}
}
