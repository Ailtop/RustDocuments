using System.Collections.Generic;
using ConVar;
using UnityEngine;
using UnityEngine.AI;

public class AIInformationZone : BaseMonoBehaviour, IServerComponent
{
	public static List<AIInformationZone> zones = new List<AIInformationZone>();

	public List<AICoverPoint> coverPoints = new List<AICoverPoint>();

	public List<AIMovePoint> movePoints = new List<AIMovePoint>();

	public List<NavMeshLink> navMeshLinks = new List<NavMeshLink>();

	public Bounds bounds;

	private OBB areaBox;

	private bool isDirty = true;

	private int processIndex;

	private int halfPaths;

	private int pathSuccesses;

	private int pathFails;

	private static bool lastFrameAnyDirty = false;

	private static float rebuildStartTime = 0f;

	public static float buildTimeTest = 0f;

	private static float lastNavmeshBuildTime = 0f;

	public void OnValidate()
	{
	}

	public void AddCoverPoint(AICoverPoint point)
	{
		if (!coverPoints.Contains(point))
		{
			coverPoints.Add(point);
			MarkDirty();
		}
	}

	public void RemoveCoverPoint(AICoverPoint point)
	{
		coverPoints.Remove(point);
		MarkDirty();
	}

	public void AddMovePoint(AIMovePoint point)
	{
		if (!movePoints.Contains(point))
		{
			movePoints.Add(point);
			MarkDirty();
		}
	}

	public void RemoveMovePoint(AIMovePoint point)
	{
		movePoints.Remove(point);
		MarkDirty();
	}

	public void MarkDirty(bool completeRefresh = false)
	{
		isDirty = true;
		processIndex = 0;
		halfPaths = 0;
		pathSuccesses = 0;
		pathFails = 0;
		if (!completeRefresh)
		{
			return;
		}
		Debug.Log("AIInformationZone performing complete refresh, please wait...");
		foreach (AIMovePoint movePoint in movePoints)
		{
			movePoint.distances.Clear();
			movePoint.distancesToCover.Clear();
		}
	}

	private bool PassesBudget(float startTime, float budgetSeconds)
	{
		if (UnityEngine.Time.realtimeSinceStartup - startTime > budgetSeconds)
		{
			return false;
		}
		return true;
	}

	public bool ProcessDistancesAttempt()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float budgetSeconds = AIThinkManager.framebudgetms / 1000f * 0.25f;
		if (realtimeSinceStartup < lastNavmeshBuildTime + 60f)
		{
			budgetSeconds = 0.1f;
		}
		int areaMask = 1 << NavMesh.GetAreaFromName("HumanNPC");
		NavMeshPath navMeshPath = new NavMeshPath();
		while (PassesBudget(realtimeSinceStartup, budgetSeconds))
		{
			AIMovePoint aIMovePoint = movePoints[processIndex];
			bool flag = true;
			int num = 0;
			for (int num2 = aIMovePoint.distances.Keys.Count - 1; num2 >= 0; num2--)
			{
				AIMovePoint aIMovePoint2 = aIMovePoint.distances.Keys[num2];
				if (!movePoints.Contains(aIMovePoint2))
				{
					aIMovePoint.distances.Remove(aIMovePoint2);
				}
			}
			for (int num3 = aIMovePoint.distancesToCover.Keys.Count - 1; num3 >= 0; num3--)
			{
				AICoverPoint aICoverPoint = aIMovePoint.distancesToCover.Keys[num3];
				if (!coverPoints.Contains(aICoverPoint))
				{
					num++;
					aIMovePoint.distancesToCover.Remove(aICoverPoint);
				}
			}
			foreach (AICoverPoint coverPoint in coverPoints)
			{
				if (aIMovePoint.distancesToCover.Contains(coverPoint))
				{
					continue;
				}
				float num4 = -1f;
				if (Vector3.Distance(aIMovePoint.transform.position, coverPoint.transform.position) > 40f)
				{
					num4 = -2f;
				}
				else if (NavMesh.CalculatePath(aIMovePoint.transform.position, coverPoint.transform.position, areaMask, navMeshPath) && navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					int num5 = navMeshPath.corners.Length;
					if (num5 > 1)
					{
						Vector3 a = navMeshPath.corners[0];
						float num6 = 0f;
						for (int i = 0; i < num5; i++)
						{
							Vector3 vector = navMeshPath.corners[i];
							num6 += Vector3.Distance(a, vector);
							a = vector;
						}
						num4 = num6;
						pathSuccesses++;
					}
					else
					{
						num4 = Vector3.Distance(aIMovePoint.transform.position, coverPoint.transform.position);
						halfPaths++;
					}
				}
				else
				{
					pathFails++;
					num4 = -2f;
				}
				aIMovePoint.distancesToCover.Add(coverPoint, num4);
				if (!PassesBudget(realtimeSinceStartup, budgetSeconds))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				processIndex++;
			}
			if (processIndex >= movePoints.Count - 1)
			{
				break;
			}
		}
		return processIndex == movePoints.Count - 1;
	}

	public static void BudgetedTick()
	{
		if (!AI.move || UnityEngine.Time.realtimeSinceStartup < buildTimeTest)
		{
			return;
		}
		bool flag = false;
		foreach (AIInformationZone zone in zones)
		{
			if (zone.isDirty)
			{
				flag = true;
				bool isDirty2 = zone.isDirty;
				zone.isDirty = !zone.ProcessDistancesAttempt();
				break;
			}
		}
		if (Global.developer > 0)
		{
			if (flag && !lastFrameAnyDirty)
			{
				Debug.Log("AIInformationZones rebuilding...");
				rebuildStartTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if (lastFrameAnyDirty && !flag)
			{
				Debug.Log("AIInformationZone rebuild complete! Duration : " + (UnityEngine.Time.realtimeSinceStartup - rebuildStartTime) + " seconds.");
			}
		}
		lastFrameAnyDirty = flag;
	}

	private float PathDistance(int count, ref Vector3[] path, float maxDistance)
	{
		if (count < 2)
		{
			return 0f;
		}
		Vector3 a = path[0];
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = path[i];
			num += Vector3.Distance(a, vector);
			a = vector;
			if (num > maxDistance)
			{
				return num;
			}
		}
		return num;
	}

	public void NavmeshBuildingComplete()
	{
		lastNavmeshBuildTime = UnityEngine.Time.realtimeSinceStartup;
		buildTimeTest = UnityEngine.Time.realtimeSinceStartup + 15f;
		MarkDirty(true);
	}

	public void Start()
	{
		AddInitialPoints();
		areaBox = new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, bounds);
		zones.Add(this);
	}

	public Vector3 ClosestPointTo(Vector3 target)
	{
		return areaBox.ClosestPoint(target);
	}

	public void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		Gizmos.DrawCube(bounds.center, bounds.size);
	}

	public void AddInitialPoints()
	{
		AICoverPoint[] componentsInChildren = base.transform.GetComponentsInChildren<AICoverPoint>();
		foreach (AICoverPoint point in componentsInChildren)
		{
			AddCoverPoint(point);
		}
		AIMovePoint[] componentsInChildren2 = base.transform.GetComponentsInChildren<AIMovePoint>(true);
		foreach (AIMovePoint point2 in componentsInChildren2)
		{
			AddMovePoint(point2);
		}
	}

	public static AIInformationZone GetForPoint(Vector3 point, BaseEntity from = null)
	{
		if (zones == null || zones.Count == 0)
		{
			return null;
		}
		foreach (AIInformationZone zone in zones)
		{
			if (zone.areaBox.Contains(point))
			{
				return zone;
			}
		}
		float num = float.PositiveInfinity;
		AIInformationZone result = zones[0];
		foreach (AIInformationZone zone2 in zones)
		{
			float num2 = Vector3.Distance(zone2.transform.position, point);
			if (num2 < num)
			{
				num = num2;
				result = zone2;
			}
		}
		return result;
	}

	public AIMovePoint GetBestMovePointNear(Vector3 targetPosition, Vector3 fromPosition, float minRange, float maxRange, bool checkLOS = false, BaseEntity forObject = null, bool returnClosest = false)
	{
		AIMovePoint aIMovePoint = null;
		AIMovePoint result = null;
		float num = -1f;
		float num2 = float.PositiveInfinity;
		foreach (AIMovePoint movePoint in movePoints)
		{
			float num3 = 0f;
			Vector3 position = movePoint.transform.position;
			float num4 = Vector3.Distance(targetPosition, position);
			if (num4 > maxRange || !movePoint.transform.parent.gameObject.activeSelf || (!(fromPosition.y < WaterSystem.OceanLevel) && movePoint.transform.position.y < WaterSystem.OceanLevel))
			{
				continue;
			}
			num3 += (movePoint.CanBeUsedBy(forObject) ? 100f : 0f);
			num3 += (1f - Mathf.InverseLerp(minRange, maxRange, num4)) * 100f;
			if (!(num3 < num))
			{
				if (num4 < num2)
				{
					result = movePoint;
				}
				if ((!checkLOS || !UnityEngine.Physics.Linecast(targetPosition + Vector3.up * 1f, position + Vector3.up * 1f, 1218519297, QueryTriggerInteraction.Ignore)) && num3 > num)
				{
					aIMovePoint = movePoint;
					num = num3;
				}
			}
		}
		if (aIMovePoint == null && returnClosest)
		{
			return result;
		}
		return aIMovePoint;
	}

	public Vector3 GetBestPositionNear(Vector3 targetPosition, Vector3 fromPosition, float minRange, float maxRange, bool checkLOS = false)
	{
		AIMovePoint aIMovePoint = null;
		float num = -1f;
		foreach (AIMovePoint movePoint in movePoints)
		{
			float num2 = 0f;
			Vector3 position = movePoint.transform.position;
			float num3 = Vector3.Distance(targetPosition, position);
			if (!(num3 > maxRange) && movePoint.transform.parent.gameObject.activeSelf)
			{
				num2 += (1f - Mathf.InverseLerp(minRange, maxRange, num3)) * 100f;
				if ((!checkLOS || !UnityEngine.Physics.Linecast(targetPosition + Vector3.up * 1f, position + Vector3.up * 1f, 1218650369, QueryTriggerInteraction.Ignore)) && num2 > num)
				{
					aIMovePoint = movePoint;
					num = num2;
				}
			}
		}
		if (aIMovePoint != null)
		{
			return aIMovePoint.transform.position;
		}
		return targetPosition;
	}

	public AIMovePoint GetClosestRaw(Vector3 pos, bool onlyIncludeWithCover = false)
	{
		AIMovePoint result = null;
		float num = float.PositiveInfinity;
		foreach (AIMovePoint movePoint in movePoints)
		{
			if (!onlyIncludeWithCover || movePoint.distancesToCover.Count != 0)
			{
				float num2 = Vector3.Distance(movePoint.transform.position, pos);
				if (num2 < num)
				{
					num = num2;
					result = movePoint;
				}
			}
		}
		return result;
	}

	public AICoverPoint GetBestCoverPoint(Vector3 currentPosition, Vector3 hideFromPosition, float minRange = 0f, float maxRange = 20f, BaseEntity forObject = null)
	{
		AICoverPoint aICoverPoint = null;
		float num = 0f;
		AIMovePoint closestRaw = GetClosestRaw(currentPosition, true);
		foreach (AICoverPoint coverPoint in coverPoints)
		{
			if (coverPoint.InUse() && !coverPoint.IsUsedBy(forObject))
			{
				continue;
			}
			Vector3 position = coverPoint.transform.position;
			Vector3 normalized = (hideFromPosition - position).normalized;
			float num2 = Vector3.Dot(coverPoint.transform.forward, normalized);
			if (num2 < 1f - coverPoint.coverDot)
			{
				continue;
			}
			float num3 = -1f;
			if (closestRaw != null && closestRaw.distancesToCover.Contains(coverPoint) && !isDirty)
			{
				num3 = closestRaw.distancesToCover[coverPoint];
				if (num3 == -2f)
				{
					continue;
				}
			}
			else
			{
				num3 = Vector3.Distance(currentPosition, position);
			}
			float num4 = 0f;
			if (minRange > 0f)
			{
				num4 -= (1f - Mathf.InverseLerp(0f, minRange, num3)) * 100f;
			}
			float value = Mathf.Abs(position.y - currentPosition.y);
			num4 += (1f - Mathf.InverseLerp(1f, 5f, value)) * 500f;
			num4 += Mathf.InverseLerp(1f - coverPoint.coverDot, 1f, num2) * 50f;
			num4 += (1f - Mathf.InverseLerp(2f, maxRange, num3)) * 100f;
			float num5 = 1f - Mathf.InverseLerp(4f, 10f, Vector3.Distance(currentPosition, hideFromPosition));
			float value2 = Vector3.Dot((coverPoint.transform.position - currentPosition).normalized, normalized);
			num4 -= Mathf.InverseLerp(-1f, 0.25f, value2) * 50f * num5;
			if (num4 > num)
			{
				aICoverPoint = coverPoint;
				num = num4;
			}
		}
		if ((bool)aICoverPoint)
		{
			return aICoverPoint;
		}
		return null;
	}

	public NavMeshLink GetClosestNavMeshLink(Vector3 pos)
	{
		NavMeshLink result = null;
		float num = float.PositiveInfinity;
		foreach (NavMeshLink navMeshLink in navMeshLinks)
		{
			float num2 = Vector3.Distance(navMeshLink.gameObject.transform.position, pos);
			if (num2 < num)
			{
				result = navMeshLink;
				num = num2;
				if (num2 < 0.25f)
				{
					return result;
				}
			}
		}
		return result;
	}
}
