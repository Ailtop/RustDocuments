using System.Collections.Generic;
using ConVar;
using Oxide.Core;
using Rust.AI;
using UnityEngine;

public class AIBrainSenses
{
	public float UpdateInterval = 0.5f;

	public float MemoryDuration = 10f;

	public float LastThreatTimestamp;

	public float TimeInAgressiveState;

	private static BaseEntity[] queryResults = new BaseEntity[64];

	private static BasePlayer[] playerQueryResults = new BasePlayer[64];

	private float nextUpdateTime;

	private BaseEntity owner;

	private BasePlayer playerOwner;

	private IAISenses ownerSenses;

	private float maxRange;

	private float targetLostRange;

	private float visionCone;

	private bool checkVision;

	private bool checkLOS;

	private bool ignoreNonVisionSneakers;

	private float listenRange;

	private bool hostileTargetsOnly;

	private bool senseFriendlies;

	private EntityType senseTypes;

	private IAIAttack ownerAttack;

	public float TimeSinceThreat => UnityEngine.Time.realtimeSinceStartup - LastThreatTimestamp;

	public SimpleAIMemory Memory { get; private set; } = new SimpleAIMemory();


	public float TargetLostRange => targetLostRange;

	public bool ignoreSafeZonePlayers { get; private set; }

	public List<BaseEntity> Players => Memory.Players;

	public void Init(BaseEntity owner, float range, float targetLostRange, float visionCone, bool checkVision, bool checkLOS, bool ignoreNonVisionSneakers, float listenRange, bool hostileTargetsOnly, bool senseFriendlies, bool ignoreSafeZonePlayers, EntityType senseTypes)
	{
		this.owner = owner;
		ownerAttack = owner as IAIAttack;
		playerOwner = owner as BasePlayer;
		maxRange = range;
		this.targetLostRange = targetLostRange;
		this.visionCone = visionCone;
		this.checkVision = checkVision;
		this.checkLOS = checkLOS;
		this.ignoreNonVisionSneakers = ignoreNonVisionSneakers;
		this.listenRange = listenRange;
		this.hostileTargetsOnly = hostileTargetsOnly;
		this.senseFriendlies = senseFriendlies;
		this.ignoreSafeZonePlayers = ignoreSafeZonePlayers;
		this.senseTypes = senseTypes;
		LastThreatTimestamp = UnityEngine.Time.realtimeSinceStartup;
		ownerSenses = owner as IAISenses;
	}

	public void Update()
	{
		if (owner == null || UnityEngine.Time.time < nextUpdateTime)
		{
			return;
		}
		nextUpdateTime = UnityEngine.Time.time + UpdateInterval;
		if (senseTypes == EntityType.Player)
		{
			SensePlayers();
		}
		else
		{
			SenseBrains();
			if (senseTypes.HasFlag(EntityType.Player))
			{
				SensePlayers();
			}
		}
		Memory.Forget(MemoryDuration);
	}

	private void SensePlayers()
	{
		int playersInSphere = BaseEntity.Query.Server.GetPlayersInSphere(owner.transform.position, maxRange, playerQueryResults, AiCaresAbout);
		for (int i = 0; i < playersInSphere; i++)
		{
			BasePlayer ent = playerQueryResults[i];
			Memory.SetKnown(ent, owner, this);
		}
	}

	private void SenseBrains()
	{
		int brainsInSphere = BaseEntity.Query.Server.GetBrainsInSphere(owner.transform.position, maxRange, queryResults, AiCaresAbout);
		for (int i = 0; i < brainsInSphere; i++)
		{
			BaseEntity ent = queryResults[i];
			Memory.SetKnown(ent, owner, this);
		}
	}

	private bool AiCaresAbout(BaseEntity entity)
	{
		if (entity == null)
		{
			return false;
		}
		if (!entity.isServer)
		{
			return false;
		}
		if (entity.EqualNetID(owner))
		{
			return false;
		}
		if (entity.Health() <= 0f)
		{
			return false;
		}
		if (!IsValidSenseType(entity))
		{
			return false;
		}
		BaseCombatEntity baseCombatEntity = entity as BaseCombatEntity;
		BasePlayer basePlayer = entity as BasePlayer;
		if (ignoreSafeZonePlayers && basePlayer != null && basePlayer.InSafeZone())
		{
			return false;
		}
		if (listenRange > 0f && baseCombatEntity != null && baseCombatEntity.TimeSinceLastNoise <= 1f && baseCombatEntity.CanLastNoiseBeHeard(owner.transform.position, listenRange))
		{
			return true;
		}
		if (senseFriendlies && ownerSenses != null && ownerSenses.IsFriendly(entity))
		{
			return true;
		}
		float num = float.PositiveInfinity;
		if (baseCombatEntity != null && AI.accuratevisiondistance)
		{
			num = Vector3.Distance(owner.transform.position, baseCombatEntity.transform.position);
			if (num > maxRange)
			{
				return false;
			}
		}
		if (checkVision && !IsTargetInVision(entity))
		{
			if (!ignoreNonVisionSneakers)
			{
				return false;
			}
			if (basePlayer != null && !basePlayer.IsNpc)
			{
				if (!AI.accuratevisiondistance)
				{
					num = Vector3.Distance(owner.transform.position, basePlayer.transform.position);
				}
				if ((basePlayer.IsDucked() && num >= 4f) || num >= 15f)
				{
					return false;
				}
			}
		}
		if (hostileTargetsOnly && baseCombatEntity != null && !baseCombatEntity.IsHostile())
		{
			return false;
		}
		if (checkLOS && ownerAttack != null)
		{
			bool flag = ownerAttack.CanSeeTarget(entity);
			Memory.SetLOS(entity, flag);
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	private bool IsValidSenseType(BaseEntity ent)
	{
		BasePlayer basePlayer = ent as BasePlayer;
		if (basePlayer != null)
		{
			if (basePlayer.IsNpc)
			{
				if (senseTypes.HasFlag(EntityType.BasePlayerNPC))
				{
					return true;
				}
			}
			else if (senseTypes.HasFlag(EntityType.Player))
			{
				return true;
			}
		}
		if (senseTypes.HasFlag(EntityType.NPC) && ent is BaseNpc)
		{
			return true;
		}
		if (senseTypes.HasFlag(EntityType.WorldItem) && ent is WorldItem)
		{
			return true;
		}
		if (senseTypes.HasFlag(EntityType.Corpse) && ent is BaseCorpse)
		{
			return true;
		}
		if (senseTypes.HasFlag(EntityType.TimedExplosive) && ent is TimedExplosive)
		{
			return true;
		}
		if (senseTypes.HasFlag(EntityType.Chair) && ent is BaseChair)
		{
			return true;
		}
		return false;
	}

	private bool IsTargetInVision(BaseEntity target)
	{
		Vector3 rhs = Vector3Ex.Direction(target.transform.position, owner.transform.position);
		return Vector3.Dot((playerOwner != null) ? playerOwner.eyes.BodyForward() : owner.transform.forward, rhs) >= visionCone;
	}

	public BaseEntity GetNearestPlayer(float rangeFraction)
	{
		return GetNearest(Memory.Players, rangeFraction);
	}

	public BaseEntity GetNearestThreat(float rangeFraction)
	{
		return GetNearest(Memory.Threats, rangeFraction);
	}

	public BaseEntity GetNearestTarget(float rangeFraction)
	{
		return GetNearest(Memory.Targets, rangeFraction);
	}

	private BaseEntity GetNearest(List<BaseEntity> entities, float rangeFraction)
	{
		if (entities == null || entities.Count == 0)
		{
			return null;
		}
		float num = float.PositiveInfinity;
		BaseEntity result = null;
		foreach (BaseEntity entity in entities)
		{
			if (!(entity == null) && !(entity.Health() <= 0f))
			{
				if (Interface.CallHook("OnNpcTarget", owner, entity) != null)
				{
					return null;
				}
				float num2 = Vector3.Distance(entity.transform.position, owner.transform.position);
				if (num2 <= rangeFraction * maxRange && num2 < num)
				{
					result = entity;
				}
			}
		}
		return result;
	}
}
