using System.Collections.Generic;
using ConVar;
using Rust.Ai;
using Rust.AI;
using UnityEngine;
using UnityEngine.AI;

public class BaseNavigator : BaseMonoBehaviour
{
	public enum NavigationType
	{
		None,
		NavMesh,
		AStar
	}

	public enum NavigationSpeed
	{
		Slowest,
		Slow,
		Normal,
		Fast
	}

	protected enum OverrideFacingDirectionMode
	{
		None,
		Direction,
		Entity
	}

	[Header("General")]
	public bool CanNavigateMounted;

	public bool CanUseNavMesh = true;

	public bool CanUseAStar = true;

	public float StoppingDistance = 0.5f;

	public string DefaultArea = "Walkable";

	[Header("Speed")]
	public float Speed = 5f;

	public float Acceleration = 5f;

	public float TurnSpeed = 10f;

	[Header("Speed Fractions")]
	public float SlowestSpeedFraction = 0.16f;

	public float SlowSpeedFraction = 0.3f;

	public float NormalSpeedFraction = 0.5f;

	public float FastSpeedFraction = 1f;

	public float LowHealthSpeedReductionTriggerFraction;

	public float LowHealthMaxSpeedFraction = 0.5f;

	public float SwimmingSpeedMultiplier = 0.25f;

	[Header("AIPoint Usage")]
	public float BestMovementPointMaxDistance = 10f;

	public float BestCoverPointMaxDistance = 20f;

	public float BestRoamPointMaxDistance = 20f;

	public float MaxRoamDistanceFromHome = -1f;

	[Header("Misc")]
	public float MaxWaterDepth = 0.75f;

	public bool SpeedBasedAvoidancePriority;

	private NavMeshPath path;

	private NavMeshQueryFilter navMeshQueryFilter;

	private int defaultAreaMask;

	[InspectorFlags]
	public TerrainTopology.Enum topologyPreference = (TerrainTopology.Enum)96;

	protected bool traversingNavMeshLink;

	protected string currentNavMeshLinkName;

	protected Vector3 currentNavMeshLinkEndPos;

	protected Stack<BasePathNode> currentAStarPath;

	protected BasePathNode targetNode;

	protected float currentSpeedFraction = 1f;

	private float lastSetDestinationTime;

	protected OverrideFacingDirectionMode overrideFacingDirectionMode;

	protected BaseEntity facingDirectionEntity;

	protected bool overrideFacingDirection;

	protected Vector3 facingDirectionOverride;

	protected bool paused;

	public AIMovePointPath Path { get; set; }

	public BasePath AStarGraph { get; set; }

	public NavMeshAgent Agent { get; private set; }

	public BaseCombatEntity BaseEntity { get; private set; }

	public Vector3 Destination { get; private set; }

	public virtual bool IsOnNavMeshLink
	{
		get
		{
			if (Agent.enabled)
			{
				return Agent.isOnOffMeshLink;
			}
			return false;
		}
	}

	public bool Moving => CurrentNavigationType != NavigationType.None;

	public NavigationType CurrentNavigationType { get; private set; }

	[HideInInspector]
	public bool StuckOffNavmesh { get; private set; }

	public virtual bool HasPath
	{
		get
		{
			if (Agent.enabled && Agent.hasPath)
			{
				return true;
			}
			if (currentAStarPath != null)
			{
				return true;
			}
			return false;
		}
	}

	public bool IsOverridingFacingDirection => overrideFacingDirectionMode != OverrideFacingDirectionMode.None;

	public Vector3 FacingDirectionOverride => facingDirectionOverride;

	public int TopologyPreference()
	{
		return (int)topologyPreference;
	}

	public virtual void Init(BaseCombatEntity entity, NavMeshAgent agent)
	{
		defaultAreaMask = 1 << NavMesh.GetAreaFromName(DefaultArea);
		BaseEntity = entity;
		Agent = agent;
		if (Agent != null)
		{
			Agent.acceleration = Acceleration;
			Agent.angularSpeed = TurnSpeed;
		}
		navMeshQueryFilter = default(NavMeshQueryFilter);
		navMeshQueryFilter.agentTypeID = Agent.agentTypeID;
		navMeshQueryFilter.areaMask = defaultAreaMask;
		path = new NavMeshPath();
		SetCurrentNavigationType(NavigationType.None);
	}

	public void SetNavMeshEnabled(bool flag)
	{
		if (Agent == null || Agent.enabled == flag)
		{
			return;
		}
		if (AiManager.nav_disable)
		{
			Agent.enabled = false;
			return;
		}
		if (Agent.enabled)
		{
			if (flag)
			{
				Agent.isStopped = false;
			}
			else if (Agent.isOnNavMesh)
			{
				Agent.isStopped = true;
			}
		}
		Agent.enabled = flag;
		if (flag && CanEnableNavMeshNavigation())
		{
			PlaceOnNavMesh();
		}
	}

	protected virtual bool CanEnableNavMeshNavigation()
	{
		if (!CanUseNavMesh)
		{
			return false;
		}
		return true;
	}

	protected virtual bool CanUpdateMovement()
	{
		if (BaseEntity != null && !BaseEntity.IsAlive())
		{
			return false;
		}
		return true;
	}

	public bool PlaceOnNavMesh()
	{
		if (Agent.isOnNavMesh)
		{
			return true;
		}
		bool flag = true;
		float maxRange = (IsSwimming() ? 30f : 6f);
		Vector3 position;
		if (GetNearestNavmeshPosition(base.transform.position + Vector3.one * 2f, out position, maxRange))
		{
			Agent.Warp(position);
			Agent.enabled = true;
			base.transform.position = position;
			if (!Agent.isOnNavMesh)
			{
				Debug.LogWarning("Agent still not on navmesh after a warp. No navmesh areas matching agent type? Agent type: " + Agent.agentTypeID, base.gameObject);
				flag = false;
				StuckOffNavmesh = true;
			}
			else
			{
				StuckOffNavmesh = false;
				flag = true;
			}
		}
		else
		{
			flag = false;
			StuckOffNavmesh = true;
			Debug.LogWarning(string.Concat(base.gameObject.name, " failed to sample navmesh at position ", base.transform.position, " on area: ", DefaultArea), base.gameObject);
		}
		return flag;
	}

	public bool GetNearestNavmeshPosition(Vector3 target, out Vector3 position, float maxRange)
	{
		position = base.transform.position;
		bool result = true;
		NavMeshHit hit;
		if (NavMesh.SamplePosition(target, out hit, maxRange, defaultAreaMask))
		{
			position = hit.position;
		}
		else
		{
			result = false;
		}
		return result;
	}

	public bool SetDestination(BasePath path, BasePathNode newTargetNode, float speedFraction)
	{
		if (!AI.move)
		{
			return false;
		}
		if (!AI.navthink)
		{
			return false;
		}
		paused = false;
		if (!CanUseAStar)
		{
			return false;
		}
		if (newTargetNode == targetNode && HasPath)
		{
			return true;
		}
		if (ReachedPosition(newTargetNode.transform.position))
		{
			return true;
		}
		BasePathNode closestToPoint = path.GetClosestToPoint(base.transform.position);
		if (closestToPoint == null || closestToPoint.transform == null)
		{
			return false;
		}
		float pathCost;
		if (AStarPath.FindPath(closestToPoint, newTargetNode, out currentAStarPath, out pathCost))
		{
			currentSpeedFraction = speedFraction;
			targetNode = newTargetNode;
			SetCurrentNavigationType(NavigationType.AStar);
			Destination = newTargetNode.transform.position;
			return true;
		}
		return false;
	}

	public bool SetDestination(Vector3 pos, NavigationSpeed speed, float updateInterval = 0f, float navmeshSampleDistance = 0f)
	{
		return SetDestination(pos, GetSpeedFraction(speed), updateInterval, navmeshSampleDistance);
	}

	public bool SetDestination(Vector3 pos, float speedFraction = 1f, float updateInterval = 0f, float navmeshSampleDistance = 0f)
	{
		if (!AI.move)
		{
			return false;
		}
		if (!AI.navthink)
		{
			return false;
		}
		paused = false;
		if (!CanUseNavMesh)
		{
			if (CanUseAStar && AStarGraph != null)
			{
				return SetDestination(AStarGraph, AStarGraph.GetClosestToPoint(pos), speedFraction);
			}
			return false;
		}
		if (AiManager.nav_disable)
		{
			return false;
		}
		if (updateInterval > 0f && !UpdateIntervalElapsed(updateInterval))
		{
			return true;
		}
		lastSetDestinationTime = UnityEngine.Time.time;
		currentSpeedFraction = speedFraction;
		if (ReachedPosition(pos))
		{
			return true;
		}
		if (navmeshSampleDistance > 0f && AI.setdestinationsamplenavmesh)
		{
			NavMeshHit hit;
			if (!NavMesh.SamplePosition(pos, out hit, navmeshSampleDistance, defaultAreaMask))
			{
				return false;
			}
			pos = hit.position;
		}
		SetCurrentNavigationType(NavigationType.NavMesh);
		Destination = pos;
		bool flag;
		if (AI.usecalculatepath)
		{
			flag = NavMesh.CalculatePath(base.transform.position, Destination, navMeshQueryFilter, path);
			if (flag)
			{
				Agent.SetPath(path);
			}
			else if (AI.usesetdestinationfallback)
			{
				flag = Agent.SetDestination(Destination);
			}
		}
		else
		{
			flag = Agent.SetDestination(Destination);
		}
		if (flag && SpeedBasedAvoidancePriority)
		{
			Agent.avoidancePriority = Random.Range(0, 21) + Mathf.FloorToInt(speedFraction * 80f);
		}
		return flag;
	}

	public void SetCurrentSpeed(NavigationSpeed speed)
	{
		currentSpeedFraction = GetSpeedFraction(speed);
	}

	public bool UpdateIntervalElapsed(float updateInterval)
	{
		if (updateInterval <= 0f)
		{
			return true;
		}
		return UnityEngine.Time.time - lastSetDestinationTime >= updateInterval;
	}

	public float GetSpeedFraction(NavigationSpeed speed)
	{
		switch (speed)
		{
		case NavigationSpeed.Fast:
			return FastSpeedFraction;
		case NavigationSpeed.Normal:
			return NormalSpeedFraction;
		case NavigationSpeed.Slow:
			return SlowSpeedFraction;
		case NavigationSpeed.Slowest:
			return SlowestSpeedFraction;
		default:
			return 1f;
		}
	}

	private void SetCurrentNavigationType(NavigationType navType)
	{
		CurrentNavigationType = navType;
		if (navType == NavigationType.NavMesh)
		{
			SetNavMeshEnabled(true);
		}
	}

	public void Pause()
	{
		if (CurrentNavigationType != 0)
		{
			Stop();
			paused = true;
		}
	}

	public void Resume()
	{
		if (paused)
		{
			SetDestination(Destination, currentSpeedFraction);
			paused = false;
		}
	}

	public void Stop()
	{
		switch (CurrentNavigationType)
		{
		case NavigationType.AStar:
			StopAStar();
			break;
		case NavigationType.NavMesh:
			StopNavMesh();
			break;
		}
		SetCurrentNavigationType(NavigationType.None);
		paused = false;
	}

	private void StopNavMesh()
	{
		SetNavMeshEnabled(false);
	}

	private void StopAStar()
	{
		currentAStarPath = null;
		targetNode = null;
	}

	public void Think(float delta)
	{
		if (AI.move && AI.navthink && !(BaseEntity == null))
		{
			UpdateNavigation(delta);
		}
	}

	public void UpdateNavigation(float delta)
	{
		UpdateMovement(delta);
	}

	private void UpdateMovement(float delta)
	{
		if (AI.move && CanUpdateMovement())
		{
			Vector3 moveToPosition = base.transform.position;
			if (IsOnNavMeshLink)
			{
				HandleNavMeshLinkTraversal(delta, ref moveToPosition);
			}
			else if (HasPath)
			{
				moveToPosition = GetNextPathPosition();
			}
			if (ValidateNextPosition(ref moveToPosition))
			{
				bool swimming = IsSwimming();
				UpdateSpeed(delta, swimming);
				UpdatePositionAndRotation(moveToPosition, delta);
			}
		}
	}

	public virtual bool IsSwimming()
	{
		return false;
	}

	private Vector3 GetNextPathPosition()
	{
		if (currentAStarPath != null && currentAStarPath.Count > 0)
		{
			return currentAStarPath.Peek().transform.position;
		}
		return Agent.nextPosition;
	}

	private bool ValidateNextPosition(ref Vector3 moveToPosition)
	{
		bool flag = ValidBounds.Test(moveToPosition);
		if (BaseEntity != null && !flag && base.transform != null && !BaseEntity.IsDestroyed)
		{
			Debug.Log(string.Concat("Invalid NavAgent Position: ", this, " ", moveToPosition.ToString(), " (destroying)"));
			BaseEntity.Kill();
			return false;
		}
		return true;
	}

	private void UpdateSpeed(float delta, bool swimming)
	{
		float num = GetTargetSpeed();
		if (LowHealthSpeedReductionTriggerFraction > 0f && BaseEntity.healthFraction <= LowHealthSpeedReductionTriggerFraction)
		{
			num = Mathf.Min(num, Speed * LowHealthMaxSpeedFraction);
		}
		Agent.speed = num * (swimming ? SwimmingSpeedMultiplier : 1f);
	}

	protected virtual float GetTargetSpeed()
	{
		return Speed * currentSpeedFraction;
	}

	protected virtual void UpdatePositionAndRotation(Vector3 moveToPosition, float delta)
	{
		if (CurrentNavigationType == NavigationType.AStar && currentAStarPath != null && currentAStarPath.Count > 0)
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, moveToPosition, Agent.speed * delta);
			BaseEntity.ServerPosition = base.transform.localPosition;
			if (ReachedPosition(moveToPosition))
			{
				currentAStarPath.Pop();
				if (currentAStarPath.Count == 0)
				{
					Stop();
					return;
				}
				moveToPosition = currentAStarPath.Peek().transform.position;
			}
		}
		if (CurrentNavigationType == NavigationType.NavMesh)
		{
			if (ReachedPosition(Agent.destination))
			{
				Stop();
			}
			if (BaseEntity != null)
			{
				BaseEntity.ServerPosition = moveToPosition;
			}
		}
		if (overrideFacingDirectionMode != 0)
		{
			ApplyFacingDirectionOverride();
		}
	}

	public virtual void ApplyFacingDirectionOverride()
	{
	}

	public void SetFacingDirectionEntity(BaseEntity entity)
	{
		overrideFacingDirectionMode = OverrideFacingDirectionMode.Entity;
		facingDirectionEntity = entity;
	}

	public void SetFacingDirectionOverride(Vector3 direction)
	{
		overrideFacingDirectionMode = OverrideFacingDirectionMode.Direction;
		overrideFacingDirection = true;
		facingDirectionOverride = direction;
	}

	public void ClearFacingDirectionOverride()
	{
		overrideFacingDirectionMode = OverrideFacingDirectionMode.None;
		overrideFacingDirection = false;
		facingDirectionEntity = null;
	}

	private bool ReachedPosition(Vector3 position)
	{
		return Vector3.Distance(position, base.transform.position) <= StoppingDistance;
	}

	private void HandleNavMeshLinkTraversal(float delta, ref Vector3 moveToPosition)
	{
		if (!traversingNavMeshLink)
		{
			HandleNavMeshLinkTraversalStart(delta);
		}
		HandleNavMeshLinkTraversalTick(delta, ref moveToPosition);
		if (IsNavMeshLinkTraversalComplete(delta, ref moveToPosition))
		{
			CompleteNavMeshLink();
		}
	}

	private bool HandleNavMeshLinkTraversalStart(float delta)
	{
		OffMeshLinkData currentOffMeshLinkData = Agent.currentOffMeshLinkData;
		if (!currentOffMeshLinkData.valid || !currentOffMeshLinkData.activated)
		{
			return false;
		}
		Vector3 normalized = (currentOffMeshLinkData.endPos - currentOffMeshLinkData.startPos).normalized;
		normalized.y = 0f;
		Vector3 desiredVelocity = Agent.desiredVelocity;
		desiredVelocity.y = 0f;
		if (Vector3.Dot(desiredVelocity, normalized) < 0.1f)
		{
			CompleteNavMeshLink();
			return false;
		}
		currentNavMeshLinkName = currentOffMeshLinkData.linkType.ToString();
		Vector3 vector = ((BaseEntity != null) ? BaseEntity.ServerPosition : base.transform.position);
		if ((vector - currentOffMeshLinkData.startPos).sqrMagnitude > (vector - currentOffMeshLinkData.endPos).sqrMagnitude)
		{
			currentNavMeshLinkEndPos = currentOffMeshLinkData.startPos;
		}
		else
		{
			currentNavMeshLinkEndPos = currentOffMeshLinkData.endPos;
		}
		traversingNavMeshLink = true;
		Agent.ActivateCurrentOffMeshLink(false);
		Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		if (!(currentNavMeshLinkName == "OpenDoorLink") && !(currentNavMeshLinkName == "JumpRockLink"))
		{
			bool flag = currentNavMeshLinkName == "JumpFoundationLink";
		}
		return true;
	}

	private void HandleNavMeshLinkTraversalTick(float delta, ref Vector3 moveToPosition)
	{
		if (currentNavMeshLinkName == "OpenDoorLink")
		{
			moveToPosition = Vector3.MoveTowards(moveToPosition, currentNavMeshLinkEndPos, Agent.speed * delta);
		}
		else if (currentNavMeshLinkName == "JumpRockLink")
		{
			moveToPosition = Vector3.MoveTowards(moveToPosition, currentNavMeshLinkEndPos, Agent.speed * delta);
		}
		else if (currentNavMeshLinkName == "JumpFoundationLink")
		{
			moveToPosition = Vector3.MoveTowards(moveToPosition, currentNavMeshLinkEndPos, Agent.speed * delta);
		}
		else
		{
			moveToPosition = Vector3.MoveTowards(moveToPosition, currentNavMeshLinkEndPos, Agent.speed * delta);
		}
	}

	private bool IsNavMeshLinkTraversalComplete(float delta, ref Vector3 moveToPosition)
	{
		if ((moveToPosition - currentNavMeshLinkEndPos).sqrMagnitude < 0.01f)
		{
			moveToPosition = currentNavMeshLinkEndPos;
			traversingNavMeshLink = false;
			currentNavMeshLinkName = string.Empty;
			CompleteNavMeshLink();
			return true;
		}
		return false;
	}

	private void CompleteNavMeshLink()
	{
		Agent.ActivateCurrentOffMeshLink(true);
		Agent.CompleteOffMeshLink();
		Agent.isStopped = false;
		Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
	}

	public bool IsPositionATopologyPreference(Vector3 position)
	{
		if (TerrainMeta.TopologyMap != null)
		{
			int topology = TerrainMeta.TopologyMap.GetTopology(position);
			if ((TopologyPreference() & topology) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAcceptableWaterDepth(Vector3 pos)
	{
		return WaterLevel.GetOverallWaterDepth(pos) <= MaxWaterDepth;
	}

	public void SetBrakingEnabled(bool flag)
	{
		Agent.autoBraking = flag;
	}
}
