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
		None = 0,
		NavMesh = 1,
		AStar = 2,
		Custom = 3,
		Base = 4
	}

	public enum NavigationSpeed
	{
		Slowest = 0,
		Slow = 1,
		Normal = 2,
		Fast = 3
	}

	protected enum OverrideFacingDirectionMode
	{
		None = 0,
		Direction = 1,
		Entity = 2
	}

	[ServerVar(Help = "The max step-up height difference for pet base navigation")]
	public static float maxStepUpDistance = 1.7f;

	[ServerVar(Help = "How many frames between base navigation movement updates")]
	public static int baseNavMovementFrameInterval = 2;

	[ServerVar(Help = "How long we are not moving for before trigger the stuck event")]
	public static float stuckTriggerDuration = 10f;

	[ServerVar]
	public static float navTypeHeightOffset = 0.5f;

	[ServerVar]
	public static float navTypeDistance = 1f;

	[Header("General")]
	public bool CanNavigateMounted;

	public bool CanUseNavMesh = true;

	public bool CanUseAStar = true;

	public bool CanUseBaseNav;

	public bool CanUseCustomNav;

	public float StoppingDistance = 0.5f;

	public string DefaultArea = "Walkable";

	[Header("Stuck Detection")]
	public bool TriggerStuckEvent;

	public float StuckDistance = 1f;

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
	public TerrainBiome.Enum biomePreference = (TerrainBiome.Enum)12;

	public bool UseBiomePreference;

	[InspectorFlags]
	public TerrainTopology.Enum topologyPreference = (TerrainTopology.Enum)96;

	public float stuckTimer;

	public Vector3 stuckCheckPosition;

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

	private int frameCount;

	private float accumDelta;

	public AIMovePointPath Path { get; set; }

	public BasePath AStarGraph { get; set; }

	public NavMeshAgent Agent { get; private set; }

	public BaseCombatEntity BaseEntity { get; private set; }

	public Vector3 Destination { get; set; }

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

	public NavigationType LastUsedNavigationType { get; private set; }

	[HideInInspector]
	public bool StuckOffNavmesh { get; private set; }

	public virtual bool HasPath
	{
		get
		{
			if (Agent == null)
			{
				return false;
			}
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
				if (Agent.isOnNavMesh)
				{
					Agent.isStopped = false;
				}
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

	public void ForceToGround()
	{
		CancelInvoke(DelayedForceToGround);
		Invoke(DelayedForceToGround, 0.5f);
	}

	private void DelayedForceToGround()
	{
		int layerMask = 10551296;
		if (UnityEngine.Physics.Raycast(base.transform.position + Vector3.up * 0.5f, Vector3.down, out var hitInfo, 1000f, layerMask))
		{
			BaseEntity.ServerPosition = hitInfo.point;
		}
	}

	public bool PlaceOnNavMesh()
	{
		if (Agent.isOnNavMesh)
		{
			return true;
		}
		bool flag = true;
		float maxRange = (IsSwimming() ? 30f : 6f);
		if (GetNearestNavmeshPosition(base.transform.position + Vector3.one * 2f, out var position, maxRange))
		{
			flag = Warp(position);
		}
		else
		{
			flag = false;
			StuckOffNavmesh = true;
			Debug.LogWarning(string.Concat(base.gameObject.name, " failed to sample navmesh at position ", base.transform.position, " on area: ", DefaultArea), base.gameObject);
		}
		return flag;
	}

	public bool Warp(Vector3 position)
	{
		Agent.Warp(position);
		Agent.enabled = true;
		base.transform.position = position;
		if (!Agent.isOnNavMesh)
		{
			Debug.LogWarning("Agent still not on navmesh after a warp. No navmesh areas matching agent type? Agent type: " + Agent.agentTypeID, base.gameObject);
			StuckOffNavmesh = true;
			return false;
		}
		StuckOffNavmesh = false;
		return true;
	}

	public bool GetNearestNavmeshPosition(Vector3 target, out Vector3 position, float maxRange)
	{
		position = base.transform.position;
		bool result = true;
		if (NavMesh.SamplePosition(target, out var hit, maxRange, defaultAreaMask))
		{
			position = hit.position;
		}
		else
		{
			result = false;
		}
		return result;
	}

	public bool SetBaseDestination(Vector3 pos, float speedFraction)
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
		currentSpeedFraction = speedFraction;
		if (ReachedPosition(pos))
		{
			return true;
		}
		Destination = pos;
		SetCurrentNavigationType(NavigationType.Base);
		return true;
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
		if (AStarPath.FindPath(closestToPoint, newTargetNode, out currentAStarPath, out var _))
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

	public virtual bool SetCustomDestination(Vector3 pos, float speedFraction = 1f, float updateInterval = 0f)
	{
		if (!AI.move)
		{
			return false;
		}
		if (!AI.navthink)
		{
			return false;
		}
		if (!CanUseCustomNav)
		{
			return false;
		}
		paused = false;
		if (ReachedPosition(pos))
		{
			return true;
		}
		currentSpeedFraction = speedFraction;
		SetCurrentNavigationType(NavigationType.Custom);
		return true;
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
		if (updateInterval > 0f && !UpdateIntervalElapsed(updateInterval))
		{
			return true;
		}
		lastSetDestinationTime = UnityEngine.Time.time;
		paused = false;
		currentSpeedFraction = speedFraction;
		if (ReachedPosition(pos))
		{
			return true;
		}
		NavigationType navigationType = NavigationType.NavMesh;
		bool num = CanUseBaseNav && CanUseNavMesh;
		NavigationType navigationType2 = NavigationType.None;
		if (num)
		{
			Vector3 navMeshPos;
			NavigationType navigationType3 = DetermineNavigationType(base.transform.position, out navMeshPos);
			navigationType2 = DetermineNavigationType(pos, out var _);
			if (navigationType2 == NavigationType.NavMesh && navigationType3 == NavigationType.NavMesh && (CurrentNavigationType == NavigationType.None || CurrentNavigationType == NavigationType.Base))
			{
				Warp(navMeshPos);
			}
			if (navigationType2 == NavigationType.Base && navigationType3 != NavigationType.Base)
			{
				BasePet basePet = BaseEntity as BasePet;
				if (basePet != null)
				{
					BasePlayer basePlayer = basePet.Brain.Events.Memory.Entity.Get(5) as BasePlayer;
					if (basePlayer != null)
					{
						BuildingPrivlidge buildingPrivilege = basePlayer.GetBuildingPrivilege(new OBB(pos, base.transform.rotation, BaseEntity.bounds));
						if (buildingPrivilege != null && !buildingPrivilege.IsAuthed(basePlayer) && buildingPrivilege.AnyAuthed())
						{
							return false;
						}
					}
				}
			}
			switch (navigationType2)
			{
			case NavigationType.Base:
				navigationType = ((navigationType3 == NavigationType.Base) ? NavigationType.Base : ((!(Vector3.Distance(BaseEntity.ServerPosition, pos) <= 10f) || !(Mathf.Abs(BaseEntity.ServerPosition.y - pos.y) <= 3f)) ? NavigationType.NavMesh : NavigationType.Base));
				break;
			case NavigationType.NavMesh:
				navigationType = ((navigationType3 == NavigationType.NavMesh) ? NavigationType.NavMesh : NavigationType.Base);
				break;
			}
		}
		else
		{
			navigationType = (CanUseNavMesh ? NavigationType.NavMesh : NavigationType.AStar);
		}
		switch (navigationType)
		{
		case NavigationType.Base:
			return SetBaseDestination(pos, speedFraction);
		case NavigationType.AStar:
			if (AStarGraph != null)
			{
				return SetDestination(AStarGraph, AStarGraph.GetClosestToPoint(pos), speedFraction);
			}
			if (CanUseCustomNav)
			{
				return SetCustomDestination(pos, speedFraction, updateInterval);
			}
			return false;
		default:
		{
			if (AiManager.nav_disable)
			{
				return false;
			}
			if (navmeshSampleDistance > 0f && AI.setdestinationsamplenavmesh)
			{
				if (!NavMesh.SamplePosition(pos, out var hit, navmeshSampleDistance, defaultAreaMask))
				{
					return false;
				}
				pos = hit.position;
			}
			SetCurrentNavigationType(NavigationType.NavMesh);
			if (!Agent.isOnNavMesh)
			{
				return false;
			}
			if (!Agent.isActiveAndEnabled)
			{
				return false;
			}
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
		}
	}

	private NavigationType DetermineNavigationType(Vector3 location, out Vector3 navMeshPos)
	{
		navMeshPos = location;
		int layerMask = 2097152;
		if (UnityEngine.Physics.Raycast(location + Vector3.up * navTypeHeightOffset, Vector3.down, out var _, navTypeDistance, layerMask))
		{
			return NavigationType.Base;
		}
		Vector3 position;
		int result = (GetNearestNavmeshPosition(location + Vector3.up * navTypeHeightOffset, out position, navTypeDistance) ? 1 : 4);
		navMeshPos = position;
		return (NavigationType)result;
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
		return speed switch
		{
			NavigationSpeed.Fast => FastSpeedFraction, 
			NavigationSpeed.Normal => NormalSpeedFraction, 
			NavigationSpeed.Slow => SlowSpeedFraction, 
			NavigationSpeed.Slowest => SlowestSpeedFraction, 
			_ => 1f, 
		};
	}

	public void SetCurrentNavigationType(NavigationType navType)
	{
		if (CurrentNavigationType == NavigationType.None)
		{
			stuckCheckPosition = base.transform.position;
			stuckTimer = 0f;
		}
		CurrentNavigationType = navType;
		if (CurrentNavigationType != 0)
		{
			LastUsedNavigationType = CurrentNavigationType;
		}
		switch (navType)
		{
		case NavigationType.None:
			stuckTimer = 0f;
			break;
		case NavigationType.NavMesh:
			SetNavMeshEnabled(flag: true);
			break;
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
		case NavigationType.Custom:
			StopCustom();
			break;
		}
		SetCurrentNavigationType(NavigationType.None);
		paused = false;
	}

	private void StopNavMesh()
	{
		SetNavMeshEnabled(flag: false);
	}

	private void StopAStar()
	{
		currentAStarPath = null;
		targetNode = null;
	}

	protected virtual void StopCustom()
	{
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
		if (!AI.move || !CanUpdateMovement())
		{
			return;
		}
		Vector3 moveToPosition = base.transform.position;
		if (TriggerStuckEvent)
		{
			stuckTimer += delta;
			if (CurrentNavigationType != 0 && stuckTimer >= stuckTriggerDuration)
			{
				if (Vector3.Distance(base.transform.position, stuckCheckPosition) <= StuckDistance)
				{
					OnStuck();
				}
				stuckTimer = 0f;
				stuckCheckPosition = base.transform.position;
			}
		}
		if (CurrentNavigationType == NavigationType.Base)
		{
			moveToPosition = Destination;
		}
		else if (IsOnNavMeshLink)
		{
			HandleNavMeshLinkTraversal(delta, ref moveToPosition);
		}
		else if (HasPath)
		{
			moveToPosition = GetNextPathPosition();
		}
		else if (CurrentNavigationType == NavigationType.Custom)
		{
			moveToPosition = Destination;
		}
		if (ValidateNextPosition(ref moveToPosition))
		{
			bool swimming = IsSwimming();
			UpdateSpeed(delta, swimming);
			UpdatePositionAndRotation(moveToPosition, delta);
		}
	}

	public virtual void OnStuck()
	{
		BasePet basePet = BaseEntity as BasePet;
		if (basePet != null && basePet.Brain != null)
		{
			basePet.Brain.LoadDefaultAIDesign();
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
		if (CurrentNavigationType == NavigationType.Base)
		{
			frameCount++;
			accumDelta += delta;
			if (frameCount < baseNavMovementFrameInterval)
			{
				return;
			}
			frameCount = 0;
			delta = accumDelta;
			accumDelta = 0f;
			int layerMask = 10551552;
			Vector3 vector = Vector3Ex.Direction2D(Destination, BaseEntity.ServerPosition);
			Vector3 vector2 = BaseEntity.ServerPosition + vector * delta * Agent.speed;
			Vector3 vector3 = BaseEntity.ServerPosition + Vector3.up * maxStepUpDistance;
			Vector3 direction = Vector3Ex.Direction(vector2 + Vector3.up * maxStepUpDistance, BaseEntity.ServerPosition + Vector3.up * maxStepUpDistance);
			float maxDistance = Vector3.Distance(vector3, vector2 + Vector3.up * maxStepUpDistance) + 0.25f;
			if (UnityEngine.Physics.Raycast(vector3, direction, out var hitInfo, maxDistance, layerMask))
			{
				return;
			}
			Vector3 origin = vector2 + Vector3.up * (maxStepUpDistance + 0.3f);
			Vector3 vector4 = vector2;
			if (!UnityEngine.Physics.SphereCast(origin, 0.25f, Vector3.down, out hitInfo, 10f, layerMask))
			{
				return;
			}
			vector4 = hitInfo.point;
			if (vector4.y - BaseEntity.ServerPosition.y > maxStepUpDistance)
			{
				return;
			}
			BaseEntity.ServerPosition = vector4;
			if (ReachedPosition(moveToPosition))
			{
				Stop();
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

	protected bool ReachedPosition(Vector3 position)
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
		Agent.ActivateCurrentOffMeshLink(activated: false);
		Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		if (!(currentNavMeshLinkName == "OpenDoorLink") && !(currentNavMeshLinkName == "JumpRockLink"))
		{
			_ = currentNavMeshLinkName == "JumpFoundationLink";
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
		Agent.ActivateCurrentOffMeshLink(activated: true);
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

	public bool IsPositionABiomePreference(Vector3 position)
	{
		if (!UseBiomePreference)
		{
			return true;
		}
		if (TerrainMeta.BiomeMap != null)
		{
			int num = (int)biomePreference;
			if ((TerrainMeta.BiomeMap.GetBiomeMaxType(position) & num) > 0)
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
