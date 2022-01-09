#define UNITY_ASSERTIONS
using System;
using System.Collections;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Ai;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

public class BaseNpc : BaseCombatEntity
{
	[Flags]
	public enum AiFlags
	{
		Sitting = 0x2,
		Chasing = 0x4,
		Sleeping = 0x8
	}

	public enum Facts
	{
		HasEnemy,
		EnemyRange,
		CanTargetEnemies,
		Health,
		Speed,
		IsTired,
		IsSleeping,
		IsAttackReady,
		IsRoamReady,
		IsAggro,
		WantsToFlee,
		IsHungry,
		FoodRange,
		AttackedLately,
		LoudNoiseNearby,
		CanTargetFood,
		IsMoving,
		IsFleeing,
		IsEating,
		IsAfraid,
		AfraidRange,
		IsUnderHealthThreshold,
		CanNotMove,
		PathToTargetStatus
	}

	public enum EnemyRangeEnum : byte
	{
		AttackRange,
		AggroRange,
		AwareRange,
		OutOfRange
	}

	public enum FoodRangeEnum : byte
	{
		EatRange,
		AwareRange,
		OutOfRange
	}

	public enum AfraidRangeEnum : byte
	{
		InAfraidRange,
		OutOfRange
	}

	public enum HealthEnum : byte
	{
		Fine,
		Medium,
		Low
	}

	public enum SpeedEnum : byte
	{
		StandStill,
		Walk,
		Run
	}

	[Serializable]
	public struct AiStatistics
	{
		public enum FamilyEnum
		{
			Bear,
			Wolf,
			Deer,
			Boar,
			Chicken,
			Horse,
			Zombie,
			Scientist,
			Murderer,
			Player
		}

		[Tooltip("Ai will be less likely to fight animals that are larger than them, and more likely to flee from them.")]
		[Range(0f, 1f)]
		public float Size;

		[Tooltip("How fast we can move")]
		public float Speed;

		[Tooltip("How fast can we accelerate")]
		public float Acceleration;

		[Tooltip("How fast can we turn around")]
		public float TurnSpeed;

		[Tooltip("Determines things like how near we'll allow other species to get")]
		[Range(0f, 1f)]
		public float Tolerance;

		[Tooltip("How far this NPC can see")]
		public float VisionRange;

		[Tooltip("Our vision cone for dot product - a value of -1 means we can see all around us, 0 = only infront ")]
		public float VisionCone;

		[Tooltip("NPCs use distance visibility to basically make closer enemies easier to detect than enemies further away")]
		public AnimationCurve DistanceVisibility;

		[Tooltip("How likely are we to be offensive without being threatened")]
		public float Hostility;

		[Tooltip("How likely are we to defend ourselves when attacked")]
		public float Defensiveness;

		[Tooltip("The range at which we will engage targets")]
		public float AggressionRange;

		[Tooltip("The range at which an aggrified npc will disengage it's current target")]
		public float DeaggroRange;

		[Tooltip("For how long will we chase a target until we give up")]
		public float DeaggroChaseTime;

		[Tooltip("When we deaggro, how long do we wait until we can aggro again.")]
		public float DeaggroCooldown;

		[Tooltip("The threshold of our health fraction where there's a chance that we want to flee")]
		public float HealthThresholdForFleeing;

		[Tooltip("The chance that we will flee when our health threshold is triggered")]
		public float HealthThresholdFleeChance;

		[Tooltip("When we flee, what is the minimum distance we should flee?")]
		public float MinFleeRange;

		[Tooltip("When we flee, what is the maximum distance we should flee?")]
		public float MaxFleeRange;

		[Tooltip("When we flee, what is the maximum time that can pass until we stop?")]
		public float MaxFleeTime;

		[Tooltip("At what range we are afraid of a target that is in our Is Afraid Of list.")]
		public float AfraidRange;

		[Tooltip("The family this npc belong to. Npcs in the same family will not attack each other.")]
		public FamilyEnum Family;

		[Tooltip("List of the types of Npc that we are afraid of.")]
		public FamilyEnum[] IsAfraidOf;

		[Tooltip("The minimum distance this npc will wander when idle.")]
		public float MinRoamRange;

		[Tooltip("The maximum distance this npc will wander when idle.")]
		public float MaxRoamRange;

		[Tooltip("The minimum amount of time between each time we seek a new roam destination (when idle)")]
		public float MinRoamDelay;

		[Tooltip("The maximum amount of time between each time we seek a new roam destination (when idle)")]
		public float MaxRoamDelay;

		[Tooltip("If an npc is mobile, they are allowed to move when idle.")]
		public bool IsMobile;

		[Tooltip("In the range between min and max roam delay, we evaluate the random value through this curve")]
		public AnimationCurve RoamDelayDistribution;

		[Tooltip("For how long do we remember that someone attacked us")]
		public float AttackedMemoryTime;

		[Tooltip("How long should we block movement to make the wakeup animation not look whack?")]
		public float WakeupBlockMoveTime;

		[Tooltip("The maximum water depth this npc willingly will walk into.")]
		public float MaxWaterDepth;

		[Tooltip("The water depth at which they will start swimming.")]
		public float WaterLevelNeck;

		public float WaterLevelNeckOffset;

		[Tooltip("The range we consider using close range weapons.")]
		public float CloseRange;

		[Tooltip("The range we consider using medium range weapons.")]
		public float MediumRange;

		[Tooltip("The range we consider using long range weapons.")]
		public float LongRange;

		[Tooltip("How long can we be out of range of our spawn point before we time out and make our way back home (when idle).")]
		public float OutOfRangeOfSpawnPointTimeout;

		[Tooltip("If this is set to true, then a target must hold special markers (like IsHostile) for the target to be considered for aggressive action.")]
		public bool OnlyAggroMarkedTargets;
	}

	public enum Behaviour
	{
		Idle,
		Wander,
		Attack,
		Flee,
		Eat,
		Sleep,
		RetreatingToCover
	}

	[NonSerialized]
	public Transform ChaseTransform;

	public int agentTypeIndex;

	public bool NewAI;

	public bool LegacyNavigation = true;

	private Vector3 stepDirection;

	private float maxFleeTime;

	private float fleeHealthThresholdPercentage = 1f;

	private float blockEnemyTargetingTimeout = float.NegativeInfinity;

	private float blockFoodTargetingTimeout = float.NegativeInfinity;

	private float aggroTimeout = float.NegativeInfinity;

	private float lastAggroChanceResult;

	private float lastAggroChanceCalcTime;

	private const float aggroChanceRecalcTimeout = 5f;

	private float eatTimeout = float.NegativeInfinity;

	private float wakeUpBlockMoveTimeout = float.NegativeInfinity;

	private BaseEntity blockTargetingThisEnemy;

	[NonSerialized]
	public float waterDepth;

	[NonSerialized]
	public bool swimming;

	[NonSerialized]
	public bool wasSwimming;

	private static readonly AnimationCurve speedFractionResponse = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private bool _traversingNavMeshLink;

	private OffMeshLinkData _currentNavMeshLink;

	private string _currentNavMeshLinkName;

	private float _currentNavMeshLinkTraversalTime;

	private float _currentNavMeshLinkTraversalTimeDelta;

	private Quaternion _currentNavMeshLinkOrientation;

	private Vector3 _currentNavMeshLinkEndPos;

	public float nextAttackTime;

	[SerializeField]
	[InspectorFlags]
	public TerrainTopology.Enum topologyPreference = (TerrainTopology.Enum)96;

	[InspectorFlags]
	public AiFlags aiFlags;

	[NonSerialized]
	public byte[] CurrentFacts = new byte[Enum.GetValues(typeof(Facts)).Length];

	[Header("NPC Senses")]
	public int ForgetUnseenEntityTime = 10;

	public float SensesTickRate = 0.5f;

	[NonSerialized]
	public BaseEntity[] SensesResults = new BaseEntity[64];

	private float lastTickTime;

	public float playerTargetDecisionStartTime;

	private float animalTargetDecisionStartTime;

	private bool isAlreadyCheckingPathPending;

	private int numPathPendingAttempts;

	private float accumPathPendingDelay;

	public const float TickRate = 0.1f;

	private Vector3 lastStuckPos;

	private float nextFlinchTime;

	private float _lastHeardGunshotTime = float.NegativeInfinity;

	[Header("BaseNpc")]
	public GameObjectRef CorpsePrefab;

	public AiStatistics Stats;

	public Vector3 AttackOffset;

	public float AttackDamage = 20f;

	public DamageType AttackDamageType = DamageType.Bite;

	[Tooltip("Stamina to use per attack")]
	public float AttackCost = 0.1f;

	[Tooltip("How often can we attack")]
	public float AttackRate = 1f;

	[Tooltip("Maximum Distance for an attack")]
	public float AttackRange = 1f;

	public NavMeshAgent NavAgent;

	public LayerMask movementMask = 429990145;

	public float stuckDuration;

	public float lastStuckTime;

	public float idleDuration;

	private bool _isDormant;

	private float lastSetDestinationTime;

	[NonSerialized]
	public StateTimer BusyTimer;

	[NonSerialized]
	public float Sleep;

	[NonSerialized]
	public VitalLevel Stamina;

	[NonSerialized]
	public VitalLevel Energy;

	[NonSerialized]
	public VitalLevel Hydration;

	public int AgentTypeIndex
	{
		get
		{
			return agentTypeIndex;
		}
		set
		{
			agentTypeIndex = value;
		}
	}

	public bool IsStuck { get; set; }

	public bool AgencyUpdateRequired { get; set; }

	public bool IsOnOffmeshLinkAndReachedNewCoord { get; set; }

	public float GetAttackRate => AttackRate;

	public bool IsSitting
	{
		get
		{
			return HasAiFlag(AiFlags.Sitting);
		}
		set
		{
			SetAiFlag(AiFlags.Sitting, value);
		}
	}

	public bool IsChasing
	{
		get
		{
			return HasAiFlag(AiFlags.Chasing);
		}
		set
		{
			SetAiFlag(AiFlags.Chasing, value);
		}
	}

	public bool IsSleeping
	{
		get
		{
			return HasAiFlag(AiFlags.Sleeping);
		}
		set
		{
			SetAiFlag(AiFlags.Sleeping, value);
		}
	}

	public float SecondsSinceLastHeardGunshot => UnityEngine.Time.time - _lastHeardGunshotTime;

	public Vector3 LastHeardGunshotDirection { get; set; }

	public float TargetSpeed { get; set; }

	public override bool IsNpc => true;

	public bool IsDormant
	{
		get
		{
			return _isDormant;
		}
		set
		{
			_isDormant = value;
			if (_isDormant)
			{
				StopMoving();
				Pause();
			}
			else if (GetNavAgent == null || AiManager.nav_disable)
			{
				IsDormant = true;
			}
			else
			{
				Resume();
			}
		}
	}

	public float SecondsSinceLastSetDestination => UnityEngine.Time.time - lastSetDestinationTime;

	public float LastSetDestinationTime => lastSetDestinationTime;

	public Vector3 Destination
	{
		get
		{
			if (IsNavRunning())
			{
				return GetNavAgent.destination;
			}
			return Entity.ServerPosition;
		}
		set
		{
			if (IsNavRunning())
			{
				GetNavAgent.destination = value;
				lastSetDestinationTime = UnityEngine.Time.time;
			}
		}
	}

	public bool IsStopped
	{
		get
		{
			if (IsNavRunning())
			{
				return GetNavAgent.isStopped;
			}
			return true;
		}
		set
		{
			if (IsNavRunning())
			{
				if (value)
				{
					GetNavAgent.destination = ServerPosition;
				}
				GetNavAgent.isStopped = value;
			}
		}
	}

	public bool AutoBraking
	{
		get
		{
			if (IsNavRunning())
			{
				return GetNavAgent.autoBraking;
			}
			return false;
		}
		set
		{
			if (IsNavRunning())
			{
				GetNavAgent.autoBraking = value;
			}
		}
	}

	public bool HasPath
	{
		get
		{
			if (IsNavRunning())
			{
				return GetNavAgent.hasPath;
			}
			return false;
		}
	}

	public BaseEntity AttackTarget { get; set; }

	public Memory.SeenInfo AttackTargetMemory { get; set; }

	public BaseEntity FoodTarget { get; set; }

	public BaseCombatEntity CombatTarget => AttackTarget as BaseCombatEntity;

	public Vector3 SpawnPosition { get; set; }

	public float AttackTargetVisibleFor => 0f;

	public float TimeAtDestination => 0f;

	public BaseCombatEntity Entity => this;

	public NavMeshAgent GetNavAgent
	{
		get
		{
			if (base.isClient)
			{
				return null;
			}
			if (NavAgent == null)
			{
				NavAgent = GetComponent<NavMeshAgent>();
				if (NavAgent == null)
				{
					Debug.LogErrorFormat("{0} has no nav agent!", base.name);
				}
			}
			return NavAgent;
		}
	}

	public AiStatistics GetStats => Stats;

	public float GetAttackRange => AttackRange;

	public Vector3 GetAttackOffset => AttackOffset;

	public float GetStamina => Stamina.Level;

	public float GetEnergy => Energy.Level;

	public float GetAttackCost => AttackCost;

	public float GetSleep => Sleep;

	public Vector3 CurrentAimAngles => base.transform.forward;

	public float GetStuckDuration => stuckDuration;

	public float GetLastStuckTime => lastStuckTime;

	public Vector3 AttackPosition => ServerPosition + base.transform.TransformDirection(AttackOffset);

	public Vector3 CrouchedAttackPosition => AttackPosition;

	public float currentBehaviorDuration => 0f;

	public Behaviour CurrentBehaviour { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseNpc.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void UpdateDestination(Vector3 position)
	{
		if (IsStopped)
		{
			IsStopped = false;
		}
		if ((Destination - position).sqrMagnitude > 0.0100000007f)
		{
			Destination = position;
		}
		ChaseTransform = null;
	}

	public void UpdateDestination(Transform tx)
	{
		IsStopped = false;
		ChaseTransform = tx;
	}

	public void StopMoving()
	{
		IsStopped = true;
		ChaseTransform = null;
		SetFact(Facts.PathToTargetStatus, 0);
	}

	public override void ApplyInheritedVelocity(Vector3 velocity)
	{
		ServerPosition = GetNewNavPosWithVelocity(this, velocity);
	}

	public static Vector3 GetNewNavPosWithVelocity(BaseEntity ent, Vector3 velocity)
	{
		BaseEntity baseEntity = ent.GetParentEntity();
		if (baseEntity != null)
		{
			velocity = baseEntity.transform.InverseTransformDirection(velocity);
		}
		Vector3 targetPosition = ent.ServerPosition + velocity * UnityEngine.Time.fixedDeltaTime;
		NavMeshHit hit;
		NavMesh.Raycast(ent.ServerPosition, targetPosition, out hit, -1);
		if (!hit.position.IsNaNOrInfinity())
		{
			return hit.position;
		}
		return ent.ServerPosition;
	}

	public override string DebugText()
	{
		return string.Concat(string.Concat(string.Concat(base.DebugText() + $"\nBehaviour: {CurrentBehaviour}", $"\nAttackTarget: {AttackTarget}"), $"\nFoodTarget: {FoodTarget}"), $"\nSleep: {Sleep:0.00}");
	}

	public void TickAi()
	{
		if (!AI.think)
		{
			return;
		}
		if (TerrainMeta.WaterMap != null)
		{
			waterDepth = TerrainMeta.WaterMap.GetDepth(ServerPosition);
			wasSwimming = swimming;
			swimming = waterDepth > Stats.WaterLevelNeck * 0.25f;
		}
		else
		{
			wasSwimming = false;
			swimming = false;
			waterDepth = 0f;
		}
		using (TimeWarning.New("TickNavigation"))
		{
			TickNavigation();
		}
		if (!AiManager.ai_dormant || GetNavAgent.enabled || CurrentBehaviour == Behaviour.Sleep || NewAI)
		{
			using (TimeWarning.New("TickMetabolism"))
			{
				TickSleep();
				TickMetabolism();
				TickSpeed();
			}
		}
	}

	private void TickSpeed()
	{
		if (!LegacyNavigation)
		{
			return;
		}
		float speed = Stats.Speed;
		if (NewAI)
		{
			speed = (swimming ? ToSpeed(SpeedEnum.Walk) : TargetSpeed);
			speed *= 0.5f + base.healthFraction * 0.5f;
			NavAgent.speed = Mathf.Lerp(NavAgent.speed, speed, 0.5f);
			NavAgent.angularSpeed = Stats.TurnSpeed;
			NavAgent.acceleration = Stats.Acceleration;
			return;
		}
		speed *= 0.5f + base.healthFraction * 0.5f;
		if (CurrentBehaviour == Behaviour.Idle)
		{
			speed *= 0.2f;
		}
		if (CurrentBehaviour == Behaviour.Eat)
		{
			speed *= 0.3f;
		}
		float time = Mathf.Min(NavAgent.speed / Stats.Speed, 1f);
		time = speedFractionResponse.Evaluate(time);
		float num = 1f - 0.9f * Vector3.Angle(base.transform.forward, (NavAgent.nextPosition - ServerPosition).normalized) / 180f * time * time;
		speed *= num;
		NavAgent.speed = Mathf.Lerp(NavAgent.speed, speed, 0.5f);
		NavAgent.angularSpeed = Stats.TurnSpeed * (1.1f - time);
		NavAgent.acceleration = Stats.Acceleration;
	}

	protected virtual void TickMetabolism()
	{
		float num = 0.000166666665f;
		if (CurrentBehaviour == Behaviour.Sleep)
		{
			num *= 0.01f;
		}
		if (NavAgent.desiredVelocity.sqrMagnitude > 0.1f)
		{
			num *= 2f;
		}
		Energy.Add(num * 0.1f * -1f);
		if (Stamina.TimeSinceUsed > 5f)
		{
			float num2 = 71f / (339f * (float)Math.PI);
			Stamina.Add(0.1f * num2);
		}
		float secondsSinceAttacked = base.SecondsSinceAttacked;
		float num3 = 60f;
	}

	public virtual bool WantsToEat(BaseEntity best)
	{
		object obj = Interface.CallHook("CanNpcEat", this, best);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (!best.HasTrait(TraitFlag.Food))
		{
			return false;
		}
		if (best.HasTrait(TraitFlag.Alive))
		{
			return false;
		}
		return true;
	}

	public virtual float FearLevel(BaseEntity ent)
	{
		float num = 0f;
		BaseNpc baseNpc = ent as BaseNpc;
		if (baseNpc != null && baseNpc.Stats.Size > Stats.Size)
		{
			if (baseNpc.WantsToAttack(this) > 0.25f)
			{
				num += 0.2f;
			}
			if (baseNpc.AttackTarget == this)
			{
				num += 0.3f;
			}
			if (baseNpc.CurrentBehaviour == Behaviour.Attack)
			{
				num *= 1.5f;
			}
			if (baseNpc.CurrentBehaviour == Behaviour.Sleep)
			{
				num *= 0.1f;
			}
		}
		if (ent as BasePlayer != null)
		{
			num += 1f;
		}
		return num;
	}

	public virtual float HateLevel(BaseEntity ent)
	{
		return 0f;
	}

	protected virtual void TickSleep()
	{
		if (CurrentBehaviour == Behaviour.Sleep)
		{
			IsSleeping = true;
			Sleep += 0.000333333359f;
		}
		else
		{
			IsSleeping = false;
			Sleep -= 2.77777781E-05f;
		}
		Sleep = Mathf.Clamp01(Sleep);
	}

	public void TickNavigationWater()
	{
		if (!LegacyNavigation || !AI.move || !IsNavRunning())
		{
			return;
		}
		if (IsDormant || !syncPosition)
		{
			StopMoving();
			return;
		}
		Vector3 moveToPosition = base.transform.position;
		stepDirection = Vector3.zero;
		if ((bool)ChaseTransform)
		{
			TickChase();
		}
		if (NavAgent.isOnOffMeshLink)
		{
			HandleNavMeshLinkTraversal(0.1f, ref moveToPosition);
		}
		else if (NavAgent.hasPath)
		{
			TickFollowPath(ref moveToPosition);
		}
		if (ValidateNextPosition(ref moveToPosition))
		{
			moveToPosition.y = 0f - Stats.WaterLevelNeck;
			UpdatePositionAndRotation(moveToPosition);
			TickIdle();
			TickStuck();
		}
	}

	public void TickNavigation()
	{
		if (!LegacyNavigation || !AI.move || !IsNavRunning())
		{
			return;
		}
		if (IsDormant || !syncPosition)
		{
			StopMoving();
			return;
		}
		Vector3 moveToPosition = base.transform.position;
		stepDirection = Vector3.zero;
		if ((bool)ChaseTransform)
		{
			TickChase();
		}
		if (NavAgent.isOnOffMeshLink)
		{
			HandleNavMeshLinkTraversal(0.1f, ref moveToPosition);
		}
		else if (NavAgent.hasPath)
		{
			TickFollowPath(ref moveToPosition);
		}
		if (ValidateNextPosition(ref moveToPosition))
		{
			UpdatePositionAndRotation(moveToPosition);
			TickIdle();
			TickStuck();
		}
	}

	private void TickChase()
	{
		Vector3 position = ChaseTransform.position;
		Vector3 vector = base.transform.position - position;
		if ((double)vector.magnitude < 5.0)
		{
			position += vector.normalized * AttackOffset.z;
		}
		if ((NavAgent.destination - position).sqrMagnitude > 0.0100000007f)
		{
			NavAgent.SetDestination(position);
		}
	}

	private void HandleNavMeshLinkTraversal(float delta, ref Vector3 moveToPosition)
	{
		if (_traversingNavMeshLink || HandleNavMeshLinkTraversalStart(delta))
		{
			HandleNavMeshLinkTraversalTick(delta, ref moveToPosition);
			if (!IsNavMeshLinkTraversalComplete(delta, ref moveToPosition))
			{
				_currentNavMeshLinkTraversalTimeDelta += delta;
			}
		}
	}

	private bool HandleNavMeshLinkTraversalStart(float delta)
	{
		OffMeshLinkData currentOffMeshLinkData = NavAgent.currentOffMeshLinkData;
		if (!currentOffMeshLinkData.valid || !currentOffMeshLinkData.activated || currentOffMeshLinkData.offMeshLink == null)
		{
			return false;
		}
		Vector3 normalized = (currentOffMeshLinkData.endPos - currentOffMeshLinkData.startPos).normalized;
		normalized.y = 0f;
		Vector3 desiredVelocity = NavAgent.desiredVelocity;
		desiredVelocity.y = 0f;
		if (Vector3.Dot(desiredVelocity, normalized) < 0.1f)
		{
			CompleteNavMeshLink();
			return false;
		}
		_currentNavMeshLink = currentOffMeshLinkData;
		_currentNavMeshLinkName = _currentNavMeshLink.linkType.ToString();
		if (currentOffMeshLinkData.offMeshLink.biDirectional)
		{
			if ((currentOffMeshLinkData.endPos - ServerPosition).sqrMagnitude < 0.05f)
			{
				_currentNavMeshLinkEndPos = currentOffMeshLinkData.startPos;
				_currentNavMeshLinkOrientation = Quaternion.LookRotation(currentOffMeshLinkData.startPos + Vector3.up * (currentOffMeshLinkData.endPos.y - currentOffMeshLinkData.startPos.y) - currentOffMeshLinkData.endPos);
			}
			else
			{
				_currentNavMeshLinkEndPos = currentOffMeshLinkData.endPos;
				_currentNavMeshLinkOrientation = Quaternion.LookRotation(currentOffMeshLinkData.endPos + Vector3.up * (currentOffMeshLinkData.startPos.y - currentOffMeshLinkData.endPos.y) - currentOffMeshLinkData.startPos);
			}
		}
		else
		{
			_currentNavMeshLinkEndPos = currentOffMeshLinkData.endPos;
			_currentNavMeshLinkOrientation = Quaternion.LookRotation(currentOffMeshLinkData.endPos + Vector3.up * (currentOffMeshLinkData.startPos.y - currentOffMeshLinkData.endPos.y) - currentOffMeshLinkData.startPos);
		}
		_traversingNavMeshLink = true;
		NavAgent.ActivateCurrentOffMeshLink(false);
		NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
		float num = Mathf.Max(NavAgent.speed, 2.8f);
		float magnitude = (_currentNavMeshLink.startPos - _currentNavMeshLink.endPos).magnitude;
		_currentNavMeshLinkTraversalTime = magnitude / num;
		_currentNavMeshLinkTraversalTimeDelta = 0f;
		if (!(_currentNavMeshLinkName == "OpenDoorLink") && !(_currentNavMeshLinkName == "JumpRockLink"))
		{
			bool flag = _currentNavMeshLinkName == "JumpFoundationLink";
		}
		return true;
	}

	private void HandleNavMeshLinkTraversalTick(float delta, ref Vector3 moveToPosition)
	{
		if (_currentNavMeshLinkName == "OpenDoorLink")
		{
			moveToPosition = Vector3.Lerp(_currentNavMeshLink.startPos, _currentNavMeshLink.endPos, _currentNavMeshLinkTraversalTimeDelta);
		}
		else if (_currentNavMeshLinkName == "JumpRockLink")
		{
			moveToPosition = Vector3.Lerp(_currentNavMeshLink.startPos, _currentNavMeshLink.endPos, _currentNavMeshLinkTraversalTimeDelta);
		}
		else if (_currentNavMeshLinkName == "JumpFoundationLink")
		{
			moveToPosition = Vector3.Lerp(_currentNavMeshLink.startPos, _currentNavMeshLink.endPos, _currentNavMeshLinkTraversalTimeDelta);
		}
		else
		{
			moveToPosition = Vector3.Lerp(_currentNavMeshLink.startPos, _currentNavMeshLink.endPos, _currentNavMeshLinkTraversalTimeDelta);
		}
	}

	private bool IsNavMeshLinkTraversalComplete(float delta, ref Vector3 moveToPosition)
	{
		if (_currentNavMeshLinkTraversalTimeDelta >= _currentNavMeshLinkTraversalTime)
		{
			moveToPosition = _currentNavMeshLink.endPos;
			_traversingNavMeshLink = false;
			_currentNavMeshLink = default(OffMeshLinkData);
			_currentNavMeshLinkTraversalTime = 0f;
			_currentNavMeshLinkTraversalTimeDelta = 0f;
			_currentNavMeshLinkName = string.Empty;
			_currentNavMeshLinkOrientation = Quaternion.identity;
			CompleteNavMeshLink();
			return true;
		}
		return false;
	}

	private void CompleteNavMeshLink()
	{
		NavAgent.ActivateCurrentOffMeshLink(true);
		NavAgent.CompleteOffMeshLink();
		NavAgent.isStopped = false;
		NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
	}

	private void TickFollowPath(ref Vector3 moveToPosition)
	{
		moveToPosition = NavAgent.nextPosition;
		stepDirection = NavAgent.desiredVelocity.normalized;
	}

	private bool ValidateNextPosition(ref Vector3 moveToPosition)
	{
		if (!ValidBounds.Test(moveToPosition) && base.transform != null && !base.IsDestroyed)
		{
			Debug.Log(string.Concat("Invalid NavAgent Position: ", this, " ", moveToPosition, " (destroying)"));
			Kill();
			return false;
		}
		return true;
	}

	private void UpdatePositionAndRotation(Vector3 moveToPosition)
	{
		ServerPosition = moveToPosition;
		UpdateAiRotation();
	}

	private void TickIdle()
	{
		if (CurrentBehaviour == Behaviour.Idle)
		{
			idleDuration += 0.1f;
		}
		else
		{
			idleDuration = 0f;
		}
	}

	public void TickStuck()
	{
		if (IsNavRunning() && !NavAgent.isStopped && (lastStuckPos - ServerPosition).sqrMagnitude < 0.0625f && AttackReady())
		{
			stuckDuration += 0.1f;
			if (stuckDuration >= 5f && Mathf.Approximately(lastStuckTime, 0f))
			{
				lastStuckTime = UnityEngine.Time.time;
				OnBecomeStuck();
			}
			return;
		}
		stuckDuration = 0f;
		lastStuckPos = ServerPosition;
		if (UnityEngine.Time.time - lastStuckTime > 5f)
		{
			lastStuckTime = 0f;
			OnBecomeUnStuck();
		}
	}

	public void OnBecomeStuck()
	{
		IsStuck = true;
	}

	public void OnBecomeUnStuck()
	{
		IsStuck = false;
	}

	public void UpdateAiRotation()
	{
		if (!IsNavRunning() || CurrentBehaviour == Behaviour.Sleep)
		{
			return;
		}
		if (_traversingNavMeshLink)
		{
			Vector3 vector = ((ChaseTransform != null) ? (ChaseTransform.localPosition - ServerPosition) : ((!(AttackTarget != null)) ? (NavAgent.destination - ServerPosition) : (AttackTarget.ServerPosition - ServerPosition)));
			if (vector.sqrMagnitude > 1f)
			{
				vector = _currentNavMeshLinkEndPos - ServerPosition;
			}
			if (vector.sqrMagnitude > 0.001f)
			{
				ServerRotation = _currentNavMeshLinkOrientation;
				return;
			}
		}
		else if ((NavAgent.destination - ServerPosition).sqrMagnitude > 1f)
		{
			Vector3 forward = stepDirection;
			if (forward.sqrMagnitude > 0.001f)
			{
				ServerRotation = Quaternion.LookRotation(forward);
				return;
			}
		}
		if ((bool)ChaseTransform && CurrentBehaviour == Behaviour.Attack)
		{
			Vector3 vector2 = ChaseTransform.localPosition - ServerPosition;
			float sqrMagnitude = vector2.sqrMagnitude;
			if (sqrMagnitude < 9f && sqrMagnitude > 0.001f)
			{
				ServerRotation = Quaternion.LookRotation(vector2.normalized);
			}
		}
		else if ((bool)AttackTarget && CurrentBehaviour == Behaviour.Attack)
		{
			Vector3 vector3 = AttackTarget.ServerPosition - ServerPosition;
			float sqrMagnitude2 = vector3.sqrMagnitude;
			if (sqrMagnitude2 < 9f && sqrMagnitude2 > 0.001f)
			{
				ServerRotation = Quaternion.LookRotation(vector3.normalized);
			}
		}
	}

	public bool AttackReady()
	{
		return UnityEngine.Time.realtimeSinceStartup >= nextAttackTime;
	}

	public virtual void StartAttack()
	{
		if ((bool)AttackTarget && AttackReady() && Interface.CallHook("OnNpcAttack", this, AttackTarget) == null && !((AttackTarget.ServerPosition - ServerPosition).magnitude > AttackRange))
		{
			nextAttackTime = UnityEngine.Time.realtimeSinceStartup + AttackRate;
			BaseCombatEntity combatTarget = CombatTarget;
			if ((bool)combatTarget)
			{
				combatTarget.Hurt(AttackDamage, AttackDamageType, this);
				Stamina.Use(AttackCost);
				BusyTimer.Activate(0.5f);
				SignalBroadcast(Signal.Attack);
				ClientRPC(null, "Attack", AttackTarget.ServerPosition);
			}
		}
	}

	public void Attack(BaseCombatEntity target)
	{
		if (!(target == null))
		{
			Vector3 vector = target.ServerPosition - ServerPosition;
			if (vector.magnitude > 0.001f)
			{
				ServerRotation = Quaternion.LookRotation(vector.normalized);
			}
			nextAttackTime = UnityEngine.Time.realtimeSinceStartup + AttackRate;
			target.Hurt(AttackDamage, AttackDamageType, this);
			Stamina.Use(AttackCost);
			SignalBroadcast(Signal.Attack);
			ClientRPC(null, "Attack", target.ServerPosition);
		}
	}

	public virtual void Eat()
	{
		if ((bool)FoodTarget)
		{
			BusyTimer.Activate(0.5f);
			FoodTarget.Eat(this, 0.5f);
			StartEating(UnityEngine.Random.value * 5f + 0.5f);
			ClientRPC(null, "Eat", FoodTarget.transform.position);
		}
	}

	public virtual void AddCalories(float amount)
	{
		Energy.Add(amount / 1000f);
	}

	public virtual void Startled()
	{
		ClientRPC(null, "Startled", base.transform.position);
	}

	private bool IsAfraid()
	{
		SetFact(Facts.IsAfraid, 0);
		return false;
	}

	protected bool IsAfraidOf(AiStatistics.FamilyEnum family)
	{
		AiStatistics.FamilyEnum[] isAfraidOf = Stats.IsAfraidOf;
		foreach (AiStatistics.FamilyEnum familyEnum in isAfraidOf)
		{
			if (family == familyEnum)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckHealthThresholdToFlee()
	{
		if (base.healthFraction > Stats.HealthThresholdForFleeing)
		{
			if (Stats.HealthThresholdForFleeing < 1f)
			{
				SetFact(Facts.IsUnderHealthThreshold, 0);
				return false;
			}
			if (GetFact(Facts.HasEnemy) == 1)
			{
				SetFact(Facts.IsUnderHealthThreshold, 0);
				return false;
			}
		}
		bool flag = UnityEngine.Random.value < Stats.HealthThresholdFleeChance;
		SetFact(Facts.IsUnderHealthThreshold, (byte)(flag ? 1u : 0u));
		return flag;
	}

	private void TickBehaviourState()
	{
		if (GetFact(Facts.WantsToFlee) == 1 && IsNavRunning() && NavAgent.pathStatus == NavMeshPathStatus.PathComplete && UnityEngine.Time.realtimeSinceStartup - (maxFleeTime - Stats.MaxFleeTime) > 0.5f)
		{
			TickFlee();
		}
		if (GetFact(Facts.CanTargetEnemies) == 0)
		{
			TickBlockEnemyTargeting();
		}
		if (GetFact(Facts.CanTargetFood) == 0)
		{
			TickBlockFoodTargeting();
		}
		if (GetFact(Facts.IsAggro) == 1)
		{
			TickAggro();
		}
		if (GetFact(Facts.IsEating) == 1)
		{
			TickEating();
		}
		if (GetFact(Facts.CanNotMove) == 1)
		{
			TickWakeUpBlockMove();
		}
	}

	private void WantsToFlee()
	{
		if (GetFact(Facts.WantsToFlee) != 1 && IsNavRunning())
		{
			SetFact(Facts.WantsToFlee, 1);
			maxFleeTime = UnityEngine.Time.realtimeSinceStartup + Stats.MaxFleeTime;
		}
	}

	private void TickFlee()
	{
	}

	public bool BlockEnemyTargeting(float timeout)
	{
		if (GetFact(Facts.CanTargetEnemies) == 0)
		{
			return false;
		}
		SetFact(Facts.CanTargetEnemies, 0);
		blockEnemyTargetingTimeout = UnityEngine.Time.realtimeSinceStartup + timeout;
		blockTargetingThisEnemy = AttackTarget;
		return true;
	}

	private void TickBlockEnemyTargeting()
	{
		if (GetFact(Facts.CanTargetEnemies) != 1 && UnityEngine.Time.realtimeSinceStartup > blockEnemyTargetingTimeout)
		{
			SetFact(Facts.CanTargetEnemies, 1);
		}
	}

	public bool BlockFoodTargeting(float timeout)
	{
		if (GetFact(Facts.CanTargetFood) == 0)
		{
			return false;
		}
		SetFact(Facts.CanTargetFood, 0);
		blockFoodTargetingTimeout = UnityEngine.Time.realtimeSinceStartup + timeout;
		return true;
	}

	private void TickBlockFoodTargeting()
	{
		if (GetFact(Facts.CanTargetFood) != 1 && UnityEngine.Time.realtimeSinceStartup > blockFoodTargetingTimeout)
		{
			SetFact(Facts.CanTargetFood, 1);
		}
	}

	public bool TryAggro(EnemyRangeEnum range)
	{
		if (Mathf.Approximately(Stats.Hostility, 0f) && Mathf.Approximately(Stats.Defensiveness, 0f))
		{
			return false;
		}
		if (GetFact(Facts.IsAggro) == 0 && (range == EnemyRangeEnum.AggroRange || range == EnemyRangeEnum.AttackRange))
		{
			float a = ((range == EnemyRangeEnum.AttackRange) ? 1f : Stats.Defensiveness);
			a = Mathf.Max(a, Stats.Hostility);
			if (UnityEngine.Time.realtimeSinceStartup > lastAggroChanceCalcTime + 5f)
			{
				lastAggroChanceResult = UnityEngine.Random.value;
				lastAggroChanceCalcTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if (lastAggroChanceResult < a)
			{
				return StartAggro(Stats.DeaggroChaseTime);
			}
		}
		return false;
	}

	public bool StartAggro(float timeout)
	{
		if (GetFact(Facts.IsAggro) == 1)
		{
			return false;
		}
		SetFact(Facts.IsAggro, 1);
		aggroTimeout = UnityEngine.Time.realtimeSinceStartup + timeout;
		return true;
	}

	private void TickAggro()
	{
	}

	public bool StartEating(float timeout)
	{
		if (GetFact(Facts.IsEating) == 1)
		{
			return false;
		}
		SetFact(Facts.IsEating, 1);
		eatTimeout = UnityEngine.Time.realtimeSinceStartup + timeout;
		return true;
	}

	private void TickEating()
	{
		if (GetFact(Facts.IsEating) != 0 && UnityEngine.Time.realtimeSinceStartup > eatTimeout)
		{
			SetFact(Facts.IsEating, 0);
		}
	}

	public bool WakeUpBlockMove(float timeout)
	{
		if (GetFact(Facts.CanNotMove) == 1)
		{
			return false;
		}
		SetFact(Facts.CanNotMove, 1);
		wakeUpBlockMoveTimeout = UnityEngine.Time.realtimeSinceStartup + timeout;
		return true;
	}

	private void TickWakeUpBlockMove()
	{
		if (GetFact(Facts.CanNotMove) != 0 && UnityEngine.Time.realtimeSinceStartup > wakeUpBlockMoveTimeout)
		{
			SetFact(Facts.CanNotMove, 0);
		}
	}

	private void OnFactChanged(Facts fact, byte oldValue, byte newValue)
	{
		switch (fact)
		{
		case Facts.IsSleeping:
			if (newValue > 0)
			{
				CurrentBehaviour = Behaviour.Sleep;
				SetFact(Facts.CanTargetEnemies, 0, false);
				SetFact(Facts.CanTargetFood, 0);
			}
			else
			{
				CurrentBehaviour = Behaviour.Idle;
				SetFact(Facts.CanTargetEnemies, 1);
				SetFact(Facts.CanTargetFood, 1);
				WakeUpBlockMove(Stats.WakeupBlockMoveTime);
				TickSenses();
			}
			break;
		case Facts.IsAggro:
			if (newValue > 0)
			{
				CurrentBehaviour = Behaviour.Attack;
			}
			else
			{
				BlockEnemyTargeting(Stats.DeaggroCooldown);
			}
			break;
		case Facts.FoodRange:
			if (newValue == 0)
			{
				CurrentBehaviour = Behaviour.Eat;
			}
			break;
		case Facts.Speed:
			switch (newValue)
			{
			case 0:
				StopMoving();
				CurrentBehaviour = Behaviour.Idle;
				break;
			case 1:
				IsStopped = false;
				CurrentBehaviour = Behaviour.Wander;
				break;
			default:
				IsStopped = false;
				break;
			}
			break;
		case Facts.IsEating:
			if (newValue == 0)
			{
				FoodTarget = null;
			}
			break;
		case Facts.CanTargetEnemies:
			if (newValue == 1)
			{
				blockTargetingThisEnemy = null;
			}
			break;
		}
	}

	public int TopologyPreference()
	{
		return (int)topologyPreference;
	}

	public bool HasAiFlag(AiFlags f)
	{
		return (aiFlags & f) == f;
	}

	public void SetAiFlag(AiFlags f, bool set)
	{
		AiFlags num = aiFlags;
		if (set)
		{
			aiFlags |= f;
		}
		else
		{
			aiFlags &= ~f;
		}
		if (num != aiFlags && base.isServer)
		{
			SendNetworkUpdate();
		}
	}

	public void InitFacts()
	{
		SetFact(Facts.CanTargetEnemies, 1);
		SetFact(Facts.CanTargetFood, 1);
	}

	public byte GetFact(Facts fact)
	{
		return CurrentFacts[(int)fact];
	}

	public void SetFact(Facts fact, byte value, bool triggerCallback = true, bool onlyTriggerCallbackOnDiffValue = true)
	{
		byte b = CurrentFacts[(int)fact];
		CurrentFacts[(int)fact] = value;
		if (triggerCallback && value != b)
		{
			OnFactChanged(fact, b, value);
		}
	}

	public EnemyRangeEnum ToEnemyRangeEnum(float range)
	{
		if (range <= AttackRange)
		{
			return EnemyRangeEnum.AttackRange;
		}
		if (range <= Stats.AggressionRange)
		{
			return EnemyRangeEnum.AggroRange;
		}
		if (range >= Stats.DeaggroRange && GetFact(Facts.IsAggro) > 0)
		{
			return EnemyRangeEnum.OutOfRange;
		}
		if (range <= Stats.VisionRange)
		{
			return EnemyRangeEnum.AwareRange;
		}
		return EnemyRangeEnum.OutOfRange;
	}

	public float GetActiveAggressionRangeSqr()
	{
		if (GetFact(Facts.IsAggro) == 1)
		{
			return Stats.DeaggroRange * Stats.DeaggroRange;
		}
		return Stats.AggressionRange * Stats.AggressionRange;
	}

	public FoodRangeEnum ToFoodRangeEnum(float range)
	{
		if (range <= 0.5f)
		{
			return FoodRangeEnum.EatRange;
		}
		if (range <= Stats.VisionRange)
		{
			return FoodRangeEnum.AwareRange;
		}
		return FoodRangeEnum.OutOfRange;
	}

	public AfraidRangeEnum ToAfraidRangeEnum(float range)
	{
		if (range <= Stats.AfraidRange)
		{
			return AfraidRangeEnum.InAfraidRange;
		}
		return AfraidRangeEnum.OutOfRange;
	}

	public HealthEnum ToHealthEnum(float healthNormalized)
	{
		if (healthNormalized >= 0.75f)
		{
			return HealthEnum.Fine;
		}
		if (healthNormalized >= 0.25f)
		{
			return HealthEnum.Medium;
		}
		return HealthEnum.Low;
	}

	public byte ToIsTired(float energyNormalized)
	{
		bool flag = GetFact(Facts.IsSleeping) == 1;
		if (!flag && energyNormalized < 0.1f)
		{
			return 1;
		}
		if (flag && energyNormalized < 0.5f)
		{
			return 1;
		}
		return 0;
	}

	public SpeedEnum ToSpeedEnum(float speed)
	{
		if (speed <= 0.01f)
		{
			return SpeedEnum.StandStill;
		}
		if (speed <= 0.18f)
		{
			return SpeedEnum.Walk;
		}
		return SpeedEnum.Run;
	}

	public float ToSpeed(SpeedEnum speed)
	{
		switch (speed)
		{
		case SpeedEnum.StandStill:
			return 0f;
		case SpeedEnum.Walk:
			return 0.18f * Stats.Speed;
		default:
			return Stats.Speed;
		}
	}

	public byte GetPathStatus()
	{
		if (!IsNavRunning())
		{
			return 2;
		}
		return (byte)NavAgent.pathStatus;
	}

	public NavMeshPathStatus ToPathStatus(byte value)
	{
		return (NavMeshPathStatus)value;
	}

	private void TickSenses()
	{
		if (Query.Server != null && !IsDormant)
		{
			if (UnityEngine.Time.realtimeSinceStartup > lastTickTime + SensesTickRate)
			{
				TickHearing();
				TickSmell();
				lastTickTime = UnityEngine.Time.realtimeSinceStartup;
			}
			if (!AI.animal_ignore_food)
			{
				TickFoodAwareness();
			}
			UpdateSelfFacts();
		}
	}

	private void TickHearing()
	{
		SetFact(Facts.LoudNoiseNearby, 0);
	}

	private void TickSmell()
	{
	}

	private float DecisionMomentumPlayerTarget()
	{
		float num = UnityEngine.Time.time - playerTargetDecisionStartTime;
		if (num > 1f)
		{
			return 0f;
		}
		return num;
	}

	private float DecisionMomentumAnimalTarget()
	{
		float num = UnityEngine.Time.time - animalTargetDecisionStartTime;
		if (num > 1f)
		{
			return 0f;
		}
		return num;
	}

	private void TickFoodAwareness()
	{
		if (GetFact(Facts.CanTargetFood) == 0)
		{
			FoodTarget = null;
			SetFact(Facts.FoodRange, 2);
		}
		else
		{
			SelectFood();
		}
	}

	private void SelectFood()
	{
	}

	private void SelectClosestFood()
	{
	}

	private void UpdateSelfFacts()
	{
	}

	private byte IsMoving()
	{
		return (byte)((IsNavRunning() && NavAgent.hasPath && NavAgent.remainingDistance > NavAgent.stoppingDistance && !IsStuck && GetFact(Facts.Speed) != 0) ? 1u : 0u);
	}

	private static bool AiCaresAbout(BaseEntity ent)
	{
		if (ent is BasePlayer)
		{
			return true;
		}
		if (ent is BaseNpc)
		{
			return true;
		}
		if (!AI.animal_ignore_food)
		{
			if (ent is WorldItem)
			{
				return true;
			}
			if (ent is BaseCorpse)
			{
				return true;
			}
			if (ent is CollectibleEntity)
			{
				return true;
			}
		}
		return false;
	}

	private static bool WithinVisionCone(BaseNpc npc, BaseEntity other)
	{
		if (Mathf.Approximately(npc.Stats.VisionCone, -1f))
		{
			return true;
		}
		Vector3 normalized = (other.ServerPosition - npc.ServerPosition).normalized;
		if (Vector3.Dot(npc.transform.forward, normalized) < npc.Stats.VisionCone)
		{
			return false;
		}
		return true;
	}

	public void SetTargetPathStatus(float pendingDelay = 0.05f)
	{
		if (!isAlreadyCheckingPathPending)
		{
			if (NavAgent.pathPending && numPathPendingAttempts < 10)
			{
				isAlreadyCheckingPathPending = true;
				Invoke(DelayedTargetPathStatus, pendingDelay);
			}
			else
			{
				numPathPendingAttempts = 0;
				accumPathPendingDelay = 0f;
				SetFact(Facts.PathToTargetStatus, GetPathStatus());
			}
		}
	}

	private void DelayedTargetPathStatus()
	{
		accumPathPendingDelay += 0.1f;
		isAlreadyCheckingPathPending = false;
		SetTargetPathStatus(accumPathPendingDelay);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (NavAgent == null)
		{
			NavAgent = GetComponent<NavMeshAgent>();
		}
		if (NavAgent != null)
		{
			NavAgent.updateRotation = false;
			NavAgent.updatePosition = false;
			if (!LegacyNavigation)
			{
				base.transform.gameObject.GetComponent<BaseNavigator>().Init(this, NavAgent);
			}
		}
		IsStuck = false;
		AgencyUpdateRequired = false;
		IsOnOffmeshLinkAndReachedNewCoord = false;
		InvokeRandomized(TickAi, 0.1f, 0.1f, 0.00500000035f);
		Sleep = UnityEngine.Random.Range(0.5f, 1f);
		Stamina.Level = UnityEngine.Random.Range(0.1f, 1f);
		Energy.Level = UnityEngine.Random.Range(0.5f, 1f);
		Hydration.Level = UnityEngine.Random.Range(0.5f, 1f);
		if (NewAI)
		{
			InitFacts();
			fleeHealthThresholdPercentage = Stats.HealthThresholdForFleeing;
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
	}

	public override void OnKilled(HitInfo hitInfo = null)
	{
		Assert.IsTrue(base.isServer, "OnKilled called on client!");
		BaseCorpse baseCorpse = DropCorpse(CorpsePrefab.resourcePath);
		if ((bool)baseCorpse)
		{
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(this);
		}
		Invoke(base.KillMessage, 0.5f);
	}

	public override void OnSensation(Sensation sensation)
	{
	}

	protected virtual void OnSenseGunshot(Sensation sensation)
	{
		_lastHeardGunshotTime = UnityEngine.Time.time;
		LastHeardGunshotDirection = (sensation.Position - base.transform.localPosition).normalized;
		if (CurrentBehaviour != Behaviour.Attack)
		{
			CurrentBehaviour = Behaviour.Flee;
		}
	}

	public bool IsNavRunning()
	{
		if (GetNavAgent != null && GetNavAgent.enabled)
		{
			return GetNavAgent.isOnNavMesh;
		}
		return false;
	}

	public void Pause()
	{
		if (GetNavAgent != null && GetNavAgent.enabled)
		{
			GetNavAgent.enabled = false;
		}
	}

	public void Resume()
	{
		if (!GetNavAgent.isOnNavMesh)
		{
			StartCoroutine(TryForceToNavmesh());
		}
		else
		{
			GetNavAgent.enabled = true;
		}
	}

	private IEnumerator TryForceToNavmesh()
	{
		yield return null;
		int numTries = 0;
		float waitForRetryTime2 = 1f;
		float maxDistanceMultiplier = 2f;
		if (SingletonComponent<DynamicNavMesh>.Instance != null)
		{
			while (SingletonComponent<DynamicNavMesh>.Instance.IsBuilding)
			{
				yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
				waitForRetryTime2 += 0.5f;
			}
		}
		waitForRetryTime2 = 1f;
		for (; numTries < 4; numTries++)
		{
			if (!GetNavAgent.isOnNavMesh)
			{
				NavMeshHit hit;
				if (NavMesh.SamplePosition(ServerPosition, out hit, GetNavAgent.height * maxDistanceMultiplier, GetNavAgent.areaMask))
				{
					ServerPosition = hit.position;
					GetNavAgent.Warp(ServerPosition);
					GetNavAgent.enabled = true;
					yield break;
				}
				yield return CoroutineEx.waitForSecondsRealtime(waitForRetryTime2);
				maxDistanceMultiplier *= 1.5f;
				waitForRetryTime2 *= 1.5f;
				continue;
			}
			GetNavAgent.enabled = true;
			yield break;
		}
		Debug.LogWarningFormat("Failed to spawn {0} on a valid navmesh.", base.name);
		DieInstantly();
	}

	public float GetWantsToAttack(BaseEntity target)
	{
		object obj = Interface.CallHook("IOnNpcTarget", this, target);
		if (obj is float)
		{
			return (float)obj;
		}
		return WantsToAttack(target);
	}

	public bool BusyTimerActive()
	{
		return BusyTimer.IsActive;
	}

	public void SetBusyFor(float dur)
	{
		BusyTimer.Activate(dur);
	}

	internal float WantsToAttack(BaseEntity target)
	{
		if (target == null)
		{
			return 0f;
		}
		if (CurrentBehaviour == Behaviour.Sleep)
		{
			return 0f;
		}
		if (!target.HasAnyTrait(TraitFlag.Animal | TraitFlag.Human))
		{
			return 0f;
		}
		if (target.GetType() == GetType())
		{
			return 1f - Stats.Tolerance;
		}
		return 1f;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseNPC = Facepunch.Pool.Get<BaseNPC>();
		info.msg.baseNPC.flags = (int)aiFlags;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseNPC != null)
		{
			aiFlags = (AiFlags)info.msg.baseNPC.flags;
		}
	}

	public override float MaxVelocity()
	{
		return Stats.Speed;
	}
}
