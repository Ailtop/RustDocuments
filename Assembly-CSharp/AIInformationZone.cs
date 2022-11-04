using System;
using System.Collections.Generic;
using ConVar;
using UnityEngine;
using UnityEngine.AI;

public class AIInformationZone : BaseMonoBehaviour, IServerComponent
{
	public bool ShouldSleepAI;

	public bool Virtual;

	public bool UseCalculatedCoverDistances = true;

	public static List<AIInformationZone> zones = new List<AIInformationZone>();

	public List<AICoverPoint> coverPoints = new List<AICoverPoint>();

	public List<AIMovePoint> movePoints = new List<AIMovePoint>();

	private AICoverPoint[] coverPointArray;

	private AIMovePoint[] movePointArray;

	public List<NavMeshLink> navMeshLinks = new List<NavMeshLink>();

	public List<AIMovePointPath> paths = new List<AIMovePointPath>();

	public Bounds bounds;

	private AIInformationGrid grid;

	private List<IAISleepable> sleepables = new List<IAISleepable>();

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

	public bool Sleeping { get; private set; }

	public int SleepingCount
	{
		get
		{
			if (!Sleeping)
			{
				return 0;
			}
			return sleepables.Count;
		}
	}

	public static AIInformationZone Merge(List<AIInformationZone> zones, GameObject newRoot)
	{
		if (zones == null)
		{
			return null;
		}
		AIInformationZone aIInformationZone = newRoot.AddComponent<AIInformationZone>();
		aIInformationZone.UseCalculatedCoverDistances = false;
		foreach (AIInformationZone zone in zones)
		{
			if (zone == null)
			{
				continue;
			}
			foreach (AIMovePoint movePoint in zone.movePoints)
			{
				aIInformationZone.AddMovePoint(movePoint);
				movePoint.transform.SetParent(newRoot.transform);
			}
			foreach (AICoverPoint coverPoint in zone.coverPoints)
			{
				aIInformationZone.AddCoverPoint(coverPoint);
				coverPoint.transform.SetParent(newRoot.transform);
			}
		}
		aIInformationZone.bounds = EncapsulateBounds(zones);
		aIInformationZone.bounds.extents += new Vector3(5f, 0f, 5f);
		aIInformationZone.bounds.center -= aIInformationZone.transform.position;
		for (int num = zones.Count - 1; num >= 0; num--)
		{
			AIInformationZone aIInformationZone2 = zones[num];
			if (!(aIInformationZone2 == null))
			{
				UnityEngine.Object.Destroy(aIInformationZone2);
			}
		}
		return aIInformationZone;
	}

	public static Bounds EncapsulateBounds(List<AIInformationZone> zones)
	{
		Bounds result = default(Bounds);
		result.center = zones[0].transform.position;
		foreach (AIInformationZone zone in zones)
		{
			if (!(zone == null))
			{
				Vector3 center = zone.bounds.center + zone.transform.position;
				Bounds bounds = zone.bounds;
				bounds.center = center;
				result.Encapsulate(bounds);
			}
		}
		return result;
	}

	public void Start()
	{
		AddInitialPoints();
		areaBox = new OBB(base.transform.position, base.transform.lossyScale, base.transform.rotation, bounds);
		zones.Add(this);
		grid = GetComponent<AIInformationGrid>();
		if (grid != null)
		{
			grid.Init();
		}
	}

	public void RegisterSleepableEntity(IAISleepable sleepable)
	{
		if (sleepable != null && sleepable.AllowedToSleep() && !sleepables.Contains(sleepable))
		{
			sleepables.Add(sleepable);
			if (Sleeping && sleepable.AllowedToSleep())
			{
				sleepable.SleepAI();
			}
		}
	}

	public void UnregisterSleepableEntity(IAISleepable sleepable)
	{
		if (sleepable != null)
		{
			sleepables.Remove(sleepable);
		}
	}

	public void SleepAI()
	{
		if (!AI.sleepwake || !ShouldSleepAI)
		{
			return;
		}
		foreach (IAISleepable sleepable in sleepables)
		{
			sleepable?.SleepAI();
		}
		Sleeping = true;
	}

	public void WakeAI()
	{
		foreach (IAISleepable sleepable in sleepables)
		{
			sleepable?.WakeAI();
		}
		Sleeping = false;
	}

	private void AddCoverPoint(AICoverPoint point)
	{
		if (!coverPoints.Contains(point))
		{
			coverPoints.Add(point);
			MarkDirty();
		}
	}

	private void RemoveCoverPoint(AICoverPoint point, bool markDirty = true)
	{
		coverPoints.Remove(point);
		if (markDirty)
		{
			MarkDirty();
		}
	}

	private void AddMovePoint(AIMovePoint point)
	{
		if (!movePoints.Contains(point))
		{
			movePoints.Add(point);
			MarkDirty();
		}
	}

	private void RemoveMovePoint(AIMovePoint point, bool markDirty = true)
	{
		movePoints.Remove(point);
		if (markDirty)
		{
			MarkDirty();
		}
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
		return true;
	}

	private bool ProcessDistances()
	{
		if (!UseCalculatedCoverDistances)
		{
			return true;
		}
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
				if (coverPoint == null || aIMovePoint.distancesToCover.Contains(coverPoint))
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
		return processIndex >= movePoints.Count - 1;
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
				_ = zone.isDirty;
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

	public void NavmeshBuildingComplete()
	{
		lastNavmeshBuildTime = UnityEngine.Time.realtimeSinceStartup;
		buildTimeTest = UnityEngine.Time.realtimeSinceStartup + 15f;
		MarkDirty(completeRefresh: true);
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
		AIMovePoint[] componentsInChildren2 = base.transform.GetComponentsInChildren<AIMovePoint>(includeInactive: true);
		foreach (AIMovePoint point2 in componentsInChildren2)
		{
			AddMovePoint(point2);
		}
		RefreshPointArrays();
		NavMeshLink[] componentsInChildren3 = base.transform.GetComponentsInChildren<NavMeshLink>(includeInactive: true);
		navMeshLinks.AddRange(componentsInChildren3);
		AIMovePointPath[] componentsInChildren4 = base.transform.GetComponentsInChildren<AIMovePointPath>();
		paths.AddRange(componentsInChildren4);
	}

	private void RefreshPointArrays()
	{
		movePointArray = movePoints?.ToArray();
		coverPointArray = coverPoints?.ToArray();
	}

	public void AddDynamicAIPoints(AIMovePoint[] movePoints, AICoverPoint[] coverPoints, Func<Vector3, bool> validatePoint = null)
	{
		if (movePoints != null)
		{
			foreach (AIMovePoint aIMovePoint in movePoints)
			{
				if (!(aIMovePoint == null) && (validatePoint == null || (validatePoint != null && validatePoint(aIMovePoint.transform.position))))
				{
					AddMovePoint(aIMovePoint);
				}
			}
		}
		if (coverPoints != null)
		{
			foreach (AICoverPoint aICoverPoint in coverPoints)
			{
				if (!(aICoverPoint == null) && (validatePoint == null || (validatePoint != null && validatePoint(aICoverPoint.transform.position))))
				{
					AddCoverPoint(aICoverPoint);
				}
			}
		}
		RefreshPointArrays();
	}

	public void RemoveDynamicAIPoints(AIMovePoint[] movePoints, AICoverPoint[] coverPoints)
	{
		if (movePoints != null)
		{
			foreach (AIMovePoint aIMovePoint in movePoints)
			{
				if (!(aIMovePoint == null))
				{
					RemoveMovePoint(aIMovePoint, markDirty: false);
				}
			}
		}
		if (coverPoints != null)
		{
			foreach (AICoverPoint aICoverPoint in coverPoints)
			{
				if (!(aICoverPoint == null))
				{
					RemoveCoverPoint(aICoverPoint, markDirty: false);
				}
			}
		}
		MarkDirty();
		RefreshPointArrays();
	}

	public AIMovePointPath GetNearestPath(Vector3 position)
	{
		if (paths == null || paths.Count == 0)
		{
			return null;
		}
		float num = float.MaxValue;
		AIMovePointPath result = null;
		foreach (AIMovePointPath path in paths)
		{
			foreach (AIMovePoint point in path.Points)
			{
				float num2 = Vector3.SqrMagnitude(point.transform.position - position);
				if (num2 < num)
				{
					num = num2;
					result = path;
				}
			}
		}
		return result;
	}

	public static AIInformationZone GetForPoint(Vector3 point, bool fallBackToNearest = true)
	{
		if (zones == null || zones.Count == 0)
		{
			return null;
		}
		foreach (AIInformationZone zone in zones)
		{
			if (!(zone == null) && !zone.Virtual && zone.areaBox.Contains(point))
			{
				return zone;
			}
		}
		if (!fallBackToNearest)
		{
			return null;
		}
		float num = float.PositiveInfinity;
		AIInformationZone aIInformationZone = zones[0];
		foreach (AIInformationZone zone2 in zones)
		{
			if (!(zone2 == null) && !(zone2.transform == null) && !zone2.Virtual)
			{
				float num2 = Vector3.Distance(zone2.transform.position, point);
				if (num2 < num)
				{
					num = num2;
					aIInformationZone = zone2;
				}
			}
		}
		if (aIInformationZone.Virtual)
		{
			aIInformationZone = null;
		}
		return aIInformationZone;
	}

	public bool PointInside(Vector3 point)
	{
		return areaBox.Contains(point);
	}

	public AIMovePoint GetBestMovePointNear(Vector3 targetPosition, Vector3 fromPosition, float minRange, float maxRange, bool checkLOS = false, BaseEntity forObject = null, bool returnClosest = false)
	{
		AIPoint aIPoint = null;
		AIPoint aIPoint2 = null;
		float num = -1f;
		float num2 = float.PositiveInfinity;
		int pointCount;
		AIPoint[] movePointsInRange = GetMovePointsInRange(targetPosition, maxRange, out pointCount);
		if (movePointsInRange == null || pointCount <= 0)
		{
			return null;
		}
		for (int i = 0; i < pointCount; i++)
		{
			AIPoint aIPoint3 = movePointsInRange[i];
			if (!aIPoint3.transform.parent.gameObject.activeSelf || (!(fromPosition.y < WaterSystem.OceanLevel) && aIPoint3.transform.position.y < WaterSystem.OceanLevel))
			{
				continue;
			}
			float num3 = 0f;
			Vector3 position = aIPoint3.transform.position;
			float num4 = Vector3.Distance(targetPosition, position);
			if (num4 < num2)
			{
				aIPoint2 = aIPoint3;
				num2 = num4;
			}
			if (!(num4 > maxRange))
			{
				num3 += (aIPoint3.CanBeUsedBy(forObject) ? 100f : 0f);
				num3 += (1f - Mathf.InverseLerp(minRange, maxRange, num4)) * 100f;
				if (!(num3 < num) && (!checkLOS || !UnityEngine.Physics.Linecast(targetPosition + Vector3.up * 1f, position + Vector3.up * 1f, 1218519297, QueryTriggerInteraction.Ignore)) && num3 > num)
				{
					aIPoint = aIPoint3;
					num = num3;
				}
			}
		}
		if (aIPoint == null && returnClosest)
		{
			return aIPoint2 as AIMovePoint;
		}
		return aIPoint as AIMovePoint;
	}

	public AIPoint[] GetMovePointsInRange(Vector3 currentPos, float maxRange, out int pointCount)
	{
		pointCount = 0;
		AIMovePoint[] movePointsInRange;
		if (grid != null && AI.usegrid)
		{
			movePointsInRange = grid.GetMovePointsInRange(currentPos, maxRange, out pointCount);
		}
		else
		{
			movePointsInRange = movePointArray;
			if (movePointsInRange != null)
			{
				pointCount = movePointsInRange.Length;
			}
		}
		return movePointsInRange;
	}

	private AIMovePoint GetClosestRaw(Vector3 pos, bool onlyIncludeWithCover = false)
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

	public AICoverPoint GetBestCoverPoint(Vector3 currentPosition, Vector3 hideFromPosition, float minRange = 0f, float maxRange = 20f, BaseEntity forObject = null, bool allowObjectToReuse = true)
	{
		AICoverPoint aICoverPoint = null;
		float num = 0f;
		AIMovePoint closestRaw = GetClosestRaw(currentPosition, onlyIncludeWithCover: true);
		int pointCount;
		AICoverPoint[] coverPointsInRange = GetCoverPointsInRange(currentPosition, maxRange, out pointCount);
		if (coverPointsInRange == null || pointCount <= 0)
		{
			return null;
		}
		for (int i = 0; i < pointCount; i++)
		{
			AICoverPoint aICoverPoint2 = coverPointsInRange[i];
			Vector3 position = aICoverPoint2.transform.position;
			Vector3 normalized = (hideFromPosition - position).normalized;
			float num2 = Vector3.Dot(aICoverPoint2.transform.forward, normalized);
			if (num2 < 1f - aICoverPoint2.coverDot)
			{
				continue;
			}
			float num3 = -1f;
			if (UseCalculatedCoverDistances && closestRaw != null && closestRaw.distancesToCover.Contains(aICoverPoint2) && !isDirty)
			{
				num3 = closestRaw.distancesToCover[aICoverPoint2];
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
			if (aICoverPoint2.InUse())
			{
				bool flag = aICoverPoint2.IsUsedBy(forObject);
				if (!(allowObjectToReuse && flag))
				{
					num4 -= 1000f;
				}
			}
			if (minRange > 0f)
			{
				num4 -= (1f - Mathf.InverseLerp(0f, minRange, num3)) * 100f;
			}
			float value = Mathf.Abs(position.y - currentPosition.y);
			num4 += (1f - Mathf.InverseLerp(1f, 5f, value)) * 500f;
			num4 += Mathf.InverseLerp(1f - aICoverPoint2.coverDot, 1f, num2) * 50f;
			num4 += (1f - Mathf.InverseLerp(2f, maxRange, num3)) * 100f;
			float num5 = 1f - Mathf.InverseLerp(4f, 10f, Vector3.Distance(currentPosition, hideFromPosition));
			float value2 = Vector3.Dot((aICoverPoint2.transform.position - currentPosition).normalized, normalized);
			num4 -= Mathf.InverseLerp(-1f, 0.25f, value2) * 50f * num5;
			if (num4 > num)
			{
				aICoverPoint = aICoverPoint2;
				num = num4;
			}
		}
		if ((bool)aICoverPoint)
		{
			return aICoverPoint;
		}
		return null;
	}

	private AICoverPoint[] GetCoverPointsInRange(Vector3 position, float maxRange, out int pointCount)
	{
		pointCount = 0;
		AICoverPoint[] coverPointsInRange;
		if (grid != null && AI.usegrid)
		{
			coverPointsInRange = grid.GetCoverPointsInRange(position, maxRange, out pointCount);
		}
		else
		{
			coverPointsInRange = coverPointArray;
			if (coverPointsInRange != null)
			{
				pointCount = coverPointsInRange.Length;
			}
		}
		return coverPointsInRange;
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
