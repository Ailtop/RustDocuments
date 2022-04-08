using System.Collections.Generic;
using ConVar;
using UnityEngine;
using UnityEngine.AI;

namespace Rust.Ai;

public class AiLocationManager : FacepunchBehaviour, IServerComponent
{
	public static List<AiLocationManager> Managers = new List<AiLocationManager>();

	[SerializeField]
	public AiLocationSpawner MainSpawner;

	[SerializeField]
	public AiLocationSpawner.SquadSpawnerLocation LocationWhenMainSpawnerIsNull = AiLocationSpawner.SquadSpawnerLocation.None;

	public Transform CoverPointGroup;

	public Transform PatrolPointGroup;

	public CoverPointVolume DynamicCoverPointVolume;

	public bool SnapCoverPointsToGround;

	private List<PathInterestNode> patrolPoints;

	public AiLocationSpawner.SquadSpawnerLocation LocationType
	{
		get
		{
			if (MainSpawner != null)
			{
				return MainSpawner.Location;
			}
			return LocationWhenMainSpawnerIsNull;
		}
	}

	private void Awake()
	{
		Managers.Add(this);
		if (!SnapCoverPointsToGround)
		{
			return;
		}
		AICoverPoint[] componentsInChildren = CoverPointGroup.GetComponentsInChildren<AICoverPoint>();
		foreach (AICoverPoint aICoverPoint in componentsInChildren)
		{
			if (NavMesh.SamplePosition(aICoverPoint.transform.position, out var hit, 4f, -1))
			{
				aICoverPoint.transform.position = hit.position;
			}
		}
	}

	private void OnDestroy()
	{
		Managers.Remove(this);
	}

	public PathInterestNode GetFirstPatrolPointInRange(Vector3 from, float minRange = 10f, float maxRange = 100f)
	{
		if (PatrolPointGroup == null)
		{
			return null;
		}
		if (patrolPoints == null)
		{
			patrolPoints = new List<PathInterestNode>(PatrolPointGroup.GetComponentsInChildren<PathInterestNode>());
		}
		if (patrolPoints.Count == 0)
		{
			return null;
		}
		float num = minRange * minRange;
		float num2 = maxRange * maxRange;
		foreach (PathInterestNode patrolPoint in patrolPoints)
		{
			float sqrMagnitude = (patrolPoint.transform.position - from).sqrMagnitude;
			if (sqrMagnitude >= num && sqrMagnitude <= num2)
			{
				return patrolPoint;
			}
		}
		return null;
	}

	public PathInterestNode GetRandomPatrolPointInRange(Vector3 from, float minRange = 10f, float maxRange = 100f, PathInterestNode currentPatrolPoint = null)
	{
		if (PatrolPointGroup == null)
		{
			return null;
		}
		if (patrolPoints == null)
		{
			patrolPoints = new List<PathInterestNode>(PatrolPointGroup.GetComponentsInChildren<PathInterestNode>());
		}
		if (patrolPoints.Count == 0)
		{
			return null;
		}
		float num = minRange * minRange;
		float num2 = maxRange * maxRange;
		for (int i = 0; i < 20; i++)
		{
			PathInterestNode pathInterestNode = patrolPoints[Random.Range(0, patrolPoints.Count)];
			if (UnityEngine.Time.time < pathInterestNode.NextVisitTime)
			{
				if (pathInterestNode == currentPatrolPoint)
				{
					return null;
				}
				continue;
			}
			float sqrMagnitude = (pathInterestNode.transform.position - from).sqrMagnitude;
			if (sqrMagnitude >= num && sqrMagnitude <= num2)
			{
				pathInterestNode.NextVisitTime = UnityEngine.Time.time + ConVar.AI.npc_patrol_point_cooldown;
				return pathInterestNode;
			}
		}
		return null;
	}
}
