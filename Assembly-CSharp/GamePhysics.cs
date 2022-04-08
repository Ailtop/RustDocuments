using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public static class GamePhysics
{
	public const int BufferLength = 8192;

	private static RaycastHit[] hitBuffer = new RaycastHit[8192];

	private static RaycastHit[] hitBufferB = new RaycastHit[8192];

	private static Collider[] colBuffer = new Collider[8192];

	public static bool CheckSphere(Vector3 position, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleTerrainCollision(position, layerMask);
		return UnityEngine.Physics.CheckSphere(position, radius, layerMask, triggerInteraction);
	}

	public static bool CheckCapsule(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleTerrainCollision((start + end) * 0.5f, layerMask);
		return UnityEngine.Physics.CheckCapsule(start, end, radius, layerMask, triggerInteraction);
	}

	public static bool CheckOBB(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleTerrainCollision(obb.position, layerMask);
		return UnityEngine.Physics.CheckBox(obb.position, obb.extents, obb.rotation, layerMask, triggerInteraction);
	}

	public static bool CheckBounds(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		layerMask = HandleTerrainCollision(bounds.center, layerMask);
		return UnityEngine.Physics.CheckBox(bounds.center, bounds.extents, Quaternion.identity, layerMask, triggerInteraction);
	}

	public static bool CheckInsideNonConvexMesh(Vector3 point, int layerMask = -5)
	{
		bool queriesHitBackfaces = UnityEngine.Physics.queriesHitBackfaces;
		UnityEngine.Physics.queriesHitBackfaces = true;
		int num = UnityEngine.Physics.RaycastNonAlloc(point, Vector3.up, hitBuffer, 100f, layerMask);
		int num2 = UnityEngine.Physics.RaycastNonAlloc(point, -Vector3.up, hitBufferB, 100f, layerMask);
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning("CheckInsideNonConvexMesh query is exceeding hitBuffer length.");
			return false;
		}
		if (num2 > hitBufferB.Length)
		{
			Debug.LogWarning("CheckInsideNonConvexMesh query is exceeding hitBufferB length.");
			return false;
		}
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				if (hitBuffer[i].collider == hitBufferB[j].collider)
				{
					UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
					return true;
				}
			}
		}
		UnityEngine.Physics.queriesHitBackfaces = queriesHitBackfaces;
		return false;
	}

	public static bool CheckInsideAnyCollider(Vector3 point, int layerMask = -5)
	{
		if (UnityEngine.Physics.CheckSphere(point, 0f, layerMask))
		{
			return true;
		}
		if (CheckInsideNonConvexMesh(point, layerMask))
		{
			return true;
		}
		if (TerrainMeta.HeightMap != null && TerrainMeta.HeightMap.GetHeight(point) > point.y)
		{
			return true;
		}
		return false;
	}

	public static void OverlapSphere(Vector3 position, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleTerrainCollision(position, layerMask);
		BufferToList(UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction), list);
	}

	public static void CapsuleSweep(Vector3 position0, Vector3 position1, float radius, Vector3 direction, float distance, List<RaycastHit> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleTerrainCollision(position1, layerMask);
		layerMask = HandleTerrainCollision(position1, layerMask);
		HitBufferToList(UnityEngine.Physics.CapsuleCastNonAlloc(position0, position1, radius, direction, hitBuffer, distance, layerMask, triggerInteraction), list);
	}

	public static void OverlapCapsule(Vector3 point0, Vector3 point1, float radius, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleTerrainCollision(point0, layerMask);
		layerMask = HandleTerrainCollision(point1, layerMask);
		BufferToList(UnityEngine.Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction), list);
	}

	public static void OverlapOBB(OBB obb, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleTerrainCollision(obb.position, layerMask);
		BufferToList(UnityEngine.Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction), list);
	}

	public static void OverlapBounds(Bounds bounds, List<Collider> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
	{
		layerMask = HandleTerrainCollision(bounds.center, layerMask);
		BufferToList(UnityEngine.Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction), list);
	}

	private static void BufferToList(int count, List<Collider> list)
	{
		if (count >= colBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding collider buffer length.");
		}
		for (int i = 0; i < count; i++)
		{
			list.Add(colBuffer[i]);
			colBuffer[i] = null;
		}
	}

	public static bool CheckSphere<T>(Vector3 pos, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		OverlapSphere(pos, radius, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public static bool CheckCapsule<T>(Vector3 start, Vector3 end, float radius, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		OverlapCapsule(start, end, radius, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public static bool CheckOBB<T>(OBB obb, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		OverlapOBB(obb, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public static bool CheckBounds<T>(Bounds bounds, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		OverlapBounds(bounds, obj, layerMask, triggerInteraction);
		bool result = CheckComponent<T>(obj);
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	private static bool CheckComponent<T>(List<Collider> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].gameObject.GetComponent<T>() != null)
			{
				return true;
			}
		}
		return false;
	}

	public static void OverlapSphere<T>(Vector3 position, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleTerrainCollision(position, layerMask);
		BufferToList(UnityEngine.Physics.OverlapSphereNonAlloc(position, radius, colBuffer, layerMask, triggerInteraction), list);
	}

	public static void OverlapCapsule<T>(Vector3 point0, Vector3 point1, float radius, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleTerrainCollision(point0, layerMask);
		layerMask = HandleTerrainCollision(point1, layerMask);
		BufferToList(UnityEngine.Physics.OverlapCapsuleNonAlloc(point0, point1, radius, colBuffer, layerMask, triggerInteraction), list);
	}

	public static void OverlapOBB<T>(OBB obb, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleTerrainCollision(obb.position, layerMask);
		BufferToList(UnityEngine.Physics.OverlapBoxNonAlloc(obb.position, obb.extents, colBuffer, obb.rotation, layerMask, triggerInteraction), list);
	}

	public static void OverlapBounds<T>(Bounds bounds, List<T> list, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) where T : Component
	{
		layerMask = HandleTerrainCollision(bounds.center, layerMask);
		BufferToList(UnityEngine.Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, colBuffer, Quaternion.identity, layerMask, triggerInteraction), list);
	}

	private static void BufferToList<T>(int count, List<T> list) where T : Component
	{
		if (count >= colBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding collider buffer length.");
		}
		for (int i = 0; i < count; i++)
		{
			T component = colBuffer[i].gameObject.GetComponent<T>();
			if ((bool)(UnityEngine.Object)component)
			{
				list.Add(component);
			}
			colBuffer[i] = null;
		}
	}

	private static void HitBufferToList(int count, List<RaycastHit> list)
	{
		if (count >= hitBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding collider buffer length.");
		}
		for (int i = 0; i < count; i++)
		{
			list.Add(hitBuffer[i]);
		}
	}

	public static bool Trace(Ray ray, float radius, out RaycastHit hitInfo, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		List<RaycastHit> obj = Facepunch.Pool.GetList<RaycastHit>();
		TraceAllUnordered(ray, radius, obj, maxDistance, layerMask, triggerInteraction);
		if (obj.Count == 0)
		{
			hitInfo = default(RaycastHit);
			Facepunch.Pool.FreeList(ref obj);
			return false;
		}
		Sort(obj);
		hitInfo = obj[0];
		Facepunch.Pool.FreeList(ref obj);
		return true;
	}

	public static void TraceAll(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		TraceAllUnordered(ray, radius, hits, maxDistance, layerMask, triggerInteraction);
		Sort(hits);
	}

	public static void TraceAllUnordered(Ray ray, float radius, List<RaycastHit> hits, float maxDistance = float.PositiveInfinity, int layerMask = -5, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.UseGlobal)
	{
		int num = 0;
		num = ((radius != 0f) ? UnityEngine.Physics.SphereCastNonAlloc(ray, radius, hitBuffer, maxDistance, layerMask, triggerInteraction) : UnityEngine.Physics.RaycastNonAlloc(ray, hitBuffer, maxDistance, layerMask, triggerInteraction));
		if (num == 0)
		{
			return;
		}
		if (num >= hitBuffer.Length)
		{
			Debug.LogWarning("Physics query is exceeding hit buffer length.");
		}
		for (int i = 0; i < num; i++)
		{
			RaycastHit raycastHit = hitBuffer[i];
			if (Verify(raycastHit))
			{
				hits.Add(raycastHit);
			}
		}
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1)
	{
		return LineOfSightInternal(p0, p1, layerMask, radius, padding0, padding1);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding = 0f)
	{
		return LineOfSightInternal(p0, p1, layerMask, radius, padding, padding);
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, Vector3 p2, int layerMask, float radius, float padding = 0f)
	{
		if (LineOfSightInternal(p0, p1, layerMask, radius, padding, 0f))
		{
			return LineOfSightInternal(p1, p2, layerMask, radius, 0f, padding);
		}
		return false;
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int layerMask, float radius, float padding = 0f)
	{
		if (LineOfSightInternal(p0, p1, layerMask, radius, padding, 0f) && LineOfSightInternal(p1, p2, layerMask, radius, 0f, 0f))
		{
			return LineOfSightInternal(p2, p3, layerMask, radius, 0f, padding);
		}
		return false;
	}

	public static bool LineOfSightRadius(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int layerMask, float radius, float padding = 0f)
	{
		if (LineOfSightInternal(p0, p1, layerMask, radius, padding, 0f) && LineOfSightInternal(p1, p2, layerMask, radius, 0f, 0f) && LineOfSightInternal(p2, p3, layerMask, radius, 0f, 0f))
		{
			return LineOfSightInternal(p3, p4, layerMask, radius, 0f, padding);
		}
		return false;
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding0, float padding1)
	{
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding0, padding1);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, int layerMask, float padding = 0f)
	{
		return LineOfSightRadius(p0, p1, layerMask, 0f, padding);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, Vector3 p2, int layerMask, float padding = 0f)
	{
		return LineOfSightRadius(p0, p1, p2, layerMask, 0f, padding);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int layerMask, float padding = 0f)
	{
		return LineOfSightRadius(p0, p1, p2, p3, layerMask, 0f, padding);
	}

	public static bool LineOfSight(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, int layerMask, float padding = 0f)
	{
		return LineOfSightRadius(p0, p1, p2, p3, p4, layerMask, 0f, padding);
	}

	private static bool LineOfSightInternal(Vector3 p0, Vector3 p1, int layerMask, float radius, float padding0, float padding1)
	{
		if (!ValidBounds.Test(p0))
		{
			return false;
		}
		if (!ValidBounds.Test(p1))
		{
			return false;
		}
		Vector3 vector = p1 - p0;
		float magnitude = vector.magnitude;
		if (magnitude <= padding0 + padding1)
		{
			return true;
		}
		Vector3 vector2 = vector / magnitude;
		Ray ray = new Ray(p0 + vector2 * padding0, vector2);
		float maxDistance = magnitude - padding0 - padding1;
		bool flag;
		RaycastHit hitInfo;
		if (((uint)layerMask & 0x800000u) != 0)
		{
			flag = Trace(ray, 0f, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			if (radius > 0f && !flag)
			{
				flag = Trace(ray, radius, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			}
		}
		else
		{
			flag = UnityEngine.Physics.Raycast(ray, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			if (radius > 0f && !flag)
			{
				flag = UnityEngine.Physics.SphereCast(ray, radius, out hitInfo, maxDistance, layerMask, QueryTriggerInteraction.Ignore);
			}
		}
		if (!flag)
		{
			if (ConVar.Vis.lineofsight)
			{
				ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.green, p0, p1);
			}
			return true;
		}
		if (ConVar.Vis.lineofsight)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.line", 60f, Color.red, p0, p1);
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", 60f, Color.white, hitInfo.point, hitInfo.collider.name);
		}
		return false;
	}

	public static bool Verify(RaycastHit hitInfo)
	{
		return Verify(hitInfo.collider, hitInfo.point);
	}

	public static bool Verify(Collider collider, Vector3 point)
	{
		if (collider is TerrainCollider && (bool)TerrainMeta.Collision && TerrainMeta.Collision.GetIgnore(point))
		{
			return false;
		}
		return collider.enabled;
	}

	public static int HandleTerrainCollision(Vector3 position, int layerMask)
	{
		int num = 8388608;
		if ((layerMask & num) != 0 && (bool)TerrainMeta.Collision && TerrainMeta.Collision.GetIgnore(position))
		{
			layerMask &= ~num;
		}
		return layerMask;
	}

	public static void Sort(List<RaycastHit> hits)
	{
		hits.Sort((RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
	}

	public static void Sort(RaycastHit[] hits)
	{
		Array.Sort(hits, (RaycastHit a, RaycastHit b) => a.distance.CompareTo(b.distance));
	}
}
