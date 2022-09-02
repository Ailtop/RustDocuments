#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConVar;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseAIBrain : EntityComponent<BaseEntity>, IPet, IAISleepable, IAIDesign, IAIGroupable, IAIEventListener
{
	public class BaseAttackState : BasicAIState
	{
		private IAIAttack attack;

		public BaseAttackState()
			: base(AIState.Attack)
		{
			base.AgrresiveState = true;
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			attack = entity as IAIAttack;
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity != null)
			{
				Vector3 aimDirection = GetAimDirection(brain.Navigator.transform.position, baseEntity.transform.position);
				brain.Navigator.SetFacingDirectionOverride(aimDirection);
				if (attack.CanAttack(baseEntity))
				{
					StartAttacking(baseEntity);
				}
				brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Fast);
			}
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			brain.Navigator.ClearFacingDirectionOverride();
			brain.Navigator.Stop();
			StopAttacking();
		}

		private void StopAttacking()
		{
			attack.StopAttacking();
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (attack == null)
			{
				return StateStatus.Error;
			}
			if (baseEntity == null)
			{
				brain.Navigator.ClearFacingDirectionOverride();
				StopAttacking();
				return StateStatus.Finished;
			}
			if (brain.Senses.ignoreSafeZonePlayers)
			{
				BasePlayer basePlayer = baseEntity as BasePlayer;
				if (basePlayer != null && basePlayer.InSafeZone())
				{
					return StateStatus.Error;
				}
			}
			if (!brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Fast, 0.25f))
			{
				return StateStatus.Error;
			}
			Vector3 aimDirection = GetAimDirection(brain.Navigator.transform.position, baseEntity.transform.position);
			brain.Navigator.SetFacingDirectionOverride(aimDirection);
			if (attack.CanAttack(baseEntity))
			{
				StartAttacking(baseEntity);
			}
			else
			{
				StopAttacking();
			}
			return StateStatus.Running;
		}

		private static Vector3 GetAimDirection(Vector3 from, Vector3 target)
		{
			return Vector3Ex.Direction2D(target, from);
		}

		private void StartAttacking(BaseEntity entity)
		{
			attack.StartAttacking(entity);
		}
	}

	public class BaseChaseState : BasicAIState
	{
		public BaseChaseState()
			: base(AIState.Chase)
		{
			base.AgrresiveState = true;
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity != null)
			{
				brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Fast);
			}
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				Stop();
				return StateStatus.Error;
			}
			if (!brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Fast, 0.25f))
			{
				return StateStatus.Error;
			}
			if (!brain.Navigator.Moving)
			{
				return StateStatus.Finished;
			}
			return StateStatus.Running;
		}
	}

	public class BaseCooldownState : BasicAIState
	{
		public BaseCooldownState()
			: base(AIState.Cooldown)
		{
		}
	}

	public class BaseDismountedState : BasicAIState
	{
		public BaseDismountedState()
			: base(AIState.Dismounted)
		{
		}
	}

	public class BaseFleeState : BasicAIState
	{
		private float nextInterval = 2f;

		private float stopFleeDistance;

		public BaseFleeState()
			: base(AIState.Flee)
		{
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity != null)
			{
				stopFleeDistance = UnityEngine.Random.Range(80f, 100f) + Mathf.Clamp(Vector3Ex.Distance2D(brain.Navigator.transform.position, baseEntity.transform.position), 0f, 50f);
			}
			FleeFrom(brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot), entity);
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				return StateStatus.Finished;
			}
			if (Vector3Ex.Distance2D(brain.Navigator.transform.position, baseEntity.transform.position) >= stopFleeDistance)
			{
				return StateStatus.Finished;
			}
			if ((brain.Navigator.UpdateIntervalElapsed(nextInterval) || !brain.Navigator.Moving) && !FleeFrom(baseEntity, entity))
			{
				return StateStatus.Error;
			}
			return StateStatus.Running;
		}

		private bool FleeFrom(BaseEntity fleeFromEntity, BaseEntity thisEntity)
		{
			if (thisEntity == null || fleeFromEntity == null)
			{
				return false;
			}
			nextInterval = UnityEngine.Random.Range(3f, 6f);
			if (!brain.PathFinder.GetBestFleePosition(brain.Navigator, brain.Senses, fleeFromEntity, brain.Events.Memory.Position.Get(4), 50f, 100f, out var result))
			{
				return false;
			}
			bool num = brain.Navigator.SetDestination(result, BaseNavigator.NavigationSpeed.Fast);
			if (!num)
			{
				Stop();
			}
			return num;
		}
	}

	public class BaseFollowPathState : BasicAIState
	{
		private AIMovePointPath path;

		private StateStatus status;

		private AIMovePoint currentTargetPoint;

		private float currentWaitTime;

		private AIMovePointPath.PathDirection pathDirection;

		private int currentNodeIndex;

		public BaseFollowPathState()
			: base(AIState.FollowPath)
		{
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			status = StateStatus.Error;
			brain.Navigator.SetBrakingEnabled(flag: false);
			path = brain.Navigator.Path;
			if (path == null)
			{
				AIInformationZone forPoint = AIInformationZone.GetForPoint(entity.ServerPosition);
				if (forPoint == null)
				{
					return;
				}
				path = forPoint.GetNearestPath(entity.ServerPosition);
				if (path == null)
				{
					return;
				}
			}
			currentNodeIndex = path.FindNearestPointIndex(entity.ServerPosition);
			currentTargetPoint = path.FindNearestPoint(entity.ServerPosition);
			if (!(currentTargetPoint == null))
			{
				status = StateStatus.Running;
				currentWaitTime = 0f;
				brain.Navigator.SetDestination(currentTargetPoint.transform.position, BaseNavigator.NavigationSpeed.Slow);
			}
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			brain.Navigator.ClearFacingDirectionOverride();
			brain.Navigator.SetBrakingEnabled(flag: true);
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (!brain.Navigator.Moving)
			{
				if (currentWaitTime <= 0f && currentTargetPoint.HasLookAtPoints())
				{
					Transform randomLookAtPoint = currentTargetPoint.GetRandomLookAtPoint();
					if (randomLookAtPoint != null)
					{
						brain.Navigator.SetFacingDirectionOverride(Vector3Ex.Direction2D(randomLookAtPoint.transform.position, entity.ServerPosition));
					}
				}
				if (currentTargetPoint.WaitTime > 0f)
				{
					currentWaitTime += delta;
				}
				if (currentTargetPoint.WaitTime <= 0f || currentWaitTime >= currentTargetPoint.WaitTime)
				{
					brain.Navigator.ClearFacingDirectionOverride();
					currentWaitTime = 0f;
					int num = currentNodeIndex;
					currentNodeIndex = path.GetNextPointIndex(currentNodeIndex, ref pathDirection);
					currentTargetPoint = path.GetPointAtIndex(currentNodeIndex);
					if ((!(currentTargetPoint != null) || currentNodeIndex != num) && (currentTargetPoint == null || !brain.Navigator.SetDestination(currentTargetPoint.transform.position, BaseNavigator.NavigationSpeed.Slow)))
					{
						return StateStatus.Error;
					}
				}
			}
			else if (currentTargetPoint != null)
			{
				brain.Navigator.SetDestination(currentTargetPoint.transform.position, BaseNavigator.NavigationSpeed.Slow, 1f);
			}
			return StateStatus.Running;
		}
	}

	public class BaseIdleState : BasicAIState
	{
		public BaseIdleState()
			: base(AIState.Idle)
		{
		}
	}

	public class BaseMountedState : BasicAIState
	{
		public BaseMountedState()
			: base(AIState.Mounted)
		{
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			brain.Navigator.Stop();
		}
	}

	public class BaseMoveTorwardsState : BasicAIState
	{
		public BaseMoveTorwardsState()
			: base(AIState.MoveTowards)
		{
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			BaseEntity baseEntity = brain.Events.Memory.Entity.Get(brain.Events.CurrentInputMemorySlot);
			if (baseEntity == null)
			{
				Stop();
				return StateStatus.Error;
			}
			if (!brain.Navigator.SetDestination(baseEntity.transform.position, BaseNavigator.NavigationSpeed.Normal, 0.25f))
			{
				return StateStatus.Error;
			}
			if (!brain.Navigator.Moving)
			{
				return StateStatus.Finished;
			}
			return StateStatus.Running;
		}
	}

	public class BaseNavigateHomeState : BasicAIState
	{
		private StateStatus status;

		public BaseNavigateHomeState()
			: base(AIState.NavigateHome)
		{
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			Vector3 pos = brain.Events.Memory.Position.Get(4);
			status = StateStatus.Running;
			if (!brain.Navigator.SetDestination(pos, BaseNavigator.NavigationSpeed.Normal))
			{
				status = StateStatus.Error;
			}
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			Stop();
		}

		private void Stop()
		{
			brain.Navigator.Stop();
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			if (status == StateStatus.Error)
			{
				return status;
			}
			if (!brain.Navigator.Moving)
			{
				return StateStatus.Finished;
			}
			return StateStatus.Running;
		}
	}

	public class BasePatrolState : BasicAIState
	{
		public BasePatrolState()
			: base(AIState.Patrol)
		{
		}
	}

	public class BaseRoamState : BasicAIState
	{
		private float nextRoamPositionTime = -1f;

		private float lastDestinationTime;

		public BaseRoamState()
			: base(AIState.Roam)
		{
		}

		public override float GetWeight()
		{
			return 0f;
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			nextRoamPositionTime = -1f;
			lastDestinationTime = UnityEngine.Time.time;
		}

		public virtual Vector3 GetDestination()
		{
			return Vector3.zero;
		}

		public virtual Vector3 GetForwardDirection()
		{
			return Vector3.forward;
		}

		public virtual void SetDestination(Vector3 destination)
		{
		}

		public override void DrawGizmos()
		{
			base.DrawGizmos();
			brain.PathFinder.DebugDraw();
		}

		public virtual Vector3 GetRoamAnchorPosition()
		{
			if (brain.Navigator.MaxRoamDistanceFromHome > -1f)
			{
				return brain.Events.Memory.Position.Get(4);
			}
			return brain.GetBaseEntity().transform.position;
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			bool flag = UnityEngine.Time.time - lastDestinationTime > 25f;
			if ((Vector3.Distance(GetDestination(), entity.transform.position) < 2f || flag) && nextRoamPositionTime == -1f)
			{
				nextRoamPositionTime = UnityEngine.Time.time + UnityEngine.Random.Range(5f, 10f);
			}
			if (nextRoamPositionTime != -1f && UnityEngine.Time.time > nextRoamPositionTime)
			{
				AIMovePoint bestRoamPoint = brain.PathFinder.GetBestRoamPoint(GetRoamAnchorPosition(), entity.ServerPosition, GetForwardDirection(), brain.Navigator.MaxRoamDistanceFromHome, brain.Navigator.BestRoamPointMaxDistance);
				if ((bool)bestRoamPoint)
				{
					float num = Vector3.Distance(bestRoamPoint.transform.position, entity.transform.position) / 1.5f;
					bestRoamPoint.SetUsedBy(entity, num + 11f);
				}
				lastDestinationTime = UnityEngine.Time.time;
				Vector3 insideUnitSphere = UnityEngine.Random.insideUnitSphere;
				insideUnitSphere.y = 0f;
				insideUnitSphere.Normalize();
				Vector3 destination = ((bestRoamPoint == null) ? entity.transform.position : (bestRoamPoint.transform.position + insideUnitSphere * bestRoamPoint.radius));
				SetDestination(destination);
				nextRoamPositionTime = -1f;
			}
			return StateStatus.Running;
		}
	}

	public class BaseSleepState : BasicAIState
	{
		private StateStatus status = StateStatus.Error;

		public BaseSleepState()
			: base(AIState.Sleep)
		{
		}

		public override void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateEnter(brain, entity);
			status = StateStatus.Error;
			if (entity is IAISleep iAISleep)
			{
				iAISleep.StartSleeping();
				status = StateStatus.Running;
			}
		}

		public override void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			base.StateLeave(brain, entity);
			if (entity is IAISleep iAISleep)
			{
				iAISleep.StopSleeping();
			}
		}

		public override StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			base.StateThink(delta, brain, entity);
			return status;
		}
	}

	public class BasicAIState
	{
		public BaseAIBrain brain;

		protected float _lastStateExitTime;

		public AIState StateType { get; private set; }

		public float TimeInState { get; private set; }

		public bool AgrresiveState { get; protected set; }

		public virtual void StateEnter(BaseAIBrain brain, BaseEntity entity)
		{
			TimeInState = 0f;
		}

		public virtual StateStatus StateThink(float delta, BaseAIBrain brain, BaseEntity entity)
		{
			TimeInState += delta;
			return StateStatus.Running;
		}

		public virtual void StateLeave(BaseAIBrain brain, BaseEntity entity)
		{
			TimeInState = 0f;
			_lastStateExitTime = UnityEngine.Time.time;
		}

		public virtual bool CanInterrupt()
		{
			return true;
		}

		public virtual bool CanEnter()
		{
			return true;
		}

		public virtual bool CanLeave()
		{
			return CanInterrupt();
		}

		public virtual float GetWeight()
		{
			return 0f;
		}

		public float TimeSinceState()
		{
			return UnityEngine.Time.time - _lastStateExitTime;
		}

		public BasicAIState(AIState state)
		{
			StateType = state;
		}

		public void Reset()
		{
			TimeInState = 0f;
		}

		public bool IsInState()
		{
			if (brain != null && brain.CurrentState != null)
			{
				return brain.CurrentState == this;
			}
			return false;
		}

		public virtual void DrawGizmos()
		{
		}
	}

	public bool UseQueuedMovementUpdates;

	public bool AllowedToSleep = true;

	public AIDesignSO DefaultDesignSO;

	public List<AIDesignSO> Designs = new List<AIDesignSO>();

	public ProtoBuf.AIDesign InstanceSpecificDesign;

	public float SenseRange = 10f;

	public float AttackRangeMultiplier = 1f;

	public float TargetLostRange = 40f;

	public float VisionCone = -0.8f;

	public bool CheckVisionCone;

	public bool CheckLOS;

	public bool IgnoreNonVisionSneakers = true;

	public float ListenRange;

	public EntityType SenseTypes;

	public bool HostileTargetsOnly;

	public bool IgnoreSafeZonePlayers;

	public int MaxGroupSize;

	public float MemoryDuration = 10f;

	public bool RefreshKnownLOS;

	public Vector3 mainInterestPoint;

	public bool UseAIDesign;

	public bool Pet;

	public List<IAIGroupable> groupMembers = new List<IAIGroupable>();

	[Header("Healing")]
	public bool CanUseHealingItems;

	public float HealChance = 0.5f;

	public float HealBelowHealthFraction = 0.5f;

	protected int loadedDesignIndex;

	public int currentStateContainerID = -1;

	private float lastMovementTickTime;

	public bool sleeping;

	private bool disabled;

	public Dictionary<AIState, BasicAIState> states;

	protected float thinkRate = 0.25f;

	protected float lastThinkTime;

	public BasicAIState CurrentState { get; set; }

	public AIThinkMode ThinkMode { get; set; } = AIThinkMode.Interval;


	public float Age { get; set; }

	public AIBrainSenses Senses { get; set; } = new AIBrainSenses();


	public BasePathFinder PathFinder { get; set; }

	public AIEvents Events { get; set; }

	public AIDesign AIDesign { get; set; }

	public BasePlayer DesigningPlayer { get; set; }

	public BasePlayer OwningPlayer { get; set; }

	public bool IsGroupLeader { get; set; }

	public bool IsGrouped { get; set; }

	public IAIGroupable GroupLeader { get; set; }

	public BaseNavigator Navigator { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseAIBrain.OnRpcMessage"))
		{
			if (rpc == 66191493 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RequestAIDesign "));
				}
				using (TimeWarning.New("RequestAIDesign"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg2 = rPCMessage;
							RequestAIDesign(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RequestAIDesign");
					}
				}
				return true;
			}
			if (rpc == 2122228512 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - StopAIDesign "));
				}
				using (TimeWarning.New("StopAIDesign"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg3 = rPCMessage;
							StopAIDesign(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in StopAIDesign");
					}
				}
				return true;
			}
			if (rpc == 657290375 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SubmitAIDesign "));
				}
				using (TimeWarning.New("SubmitAIDesign"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg4 = rPCMessage;
							SubmitAIDesign(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SubmitAIDesign");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsPet()
	{
		return Pet;
	}

	public void SetPetOwner(BasePlayer player)
	{
		BaseEntity baseEntity = (player.PetEntity = GetBaseEntity());
		baseEntity.OwnerID = player.userID;
		BasePet.ActivePetByOwnerID[player.userID] = baseEntity as BasePet;
	}

	public bool IsOwnedBy(BasePlayer player)
	{
		if (OwningPlayer == null)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if ((object)this == null)
		{
			return false;
		}
		return OwningPlayer == player;
	}

	public bool IssuePetCommand(PetCommandType cmd, int param, Ray? ray)
	{
		if (ray.HasValue)
		{
			int layerMask = 10551296;
			if (UnityEngine.Physics.Raycast(ray.Value, out var hitInfo, 75f, layerMask))
			{
				Events.Memory.Position.Set(hitInfo.point, 6);
			}
			else
			{
				Events.Memory.Position.Set(base.transform.position, 6);
			}
		}
		switch (cmd)
		{
		case PetCommandType.LoadDesign:
			if (param < 0 || param >= Designs.Count)
			{
				return false;
			}
			LoadAIDesign(AIDesigns.GetByNameOrInstance(Designs[param].Filename, InstanceSpecificDesign), null, param);
			return true;
		case PetCommandType.SetState:
		{
			AIStateContainer stateContainerByID = AIDesign.GetStateContainerByID(param);
			if (stateContainerByID == null)
			{
				return false;
			}
			return SwitchToState(stateContainerByID.State, param);
		}
		case PetCommandType.Destroy:
			GetBaseEntity().Kill();
			return true;
		default:
			return false;
		}
	}

	public void ForceSetAge(float age)
	{
		Age = age;
	}

	public int LoadedDesignIndex()
	{
		return loadedDesignIndex;
	}

	public void SetEnabled(bool flag)
	{
		disabled = !flag;
	}

	bool IAIDesign.CanPlayerDesignAI(BasePlayer player)
	{
		return PlayerCanDesignAI(player);
	}

	private bool PlayerCanDesignAI(BasePlayer player)
	{
		if (!AI.allowdesigning)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (!UseAIDesign)
		{
			return false;
		}
		if (DesigningPlayer != null)
		{
			return false;
		}
		if (!player.IsDeveloper)
		{
			return false;
		}
		return true;
	}

	[BaseEntity.RPC_Server]
	private void RequestAIDesign(BaseEntity.RPCMessage msg)
	{
		if (UseAIDesign && !(msg.player == null) && AIDesign != null && PlayerCanDesignAI(msg.player))
		{
			msg.player.designingAIEntity = GetBaseEntity();
			msg.player.ClientRPCPlayer(null, msg.player, "StartDesigningAI", AIDesign.ToProto(currentStateContainerID));
			DesigningPlayer = msg.player;
			SetOwningPlayer(msg.player);
		}
	}

	[BaseEntity.RPC_Server]
	private void SubmitAIDesign(BaseEntity.RPCMessage msg)
	{
		ProtoBuf.AIDesign aIDesign = ProtoBuf.AIDesign.Deserialize(msg.read);
		if (!LoadAIDesign(aIDesign, msg.player, loadedDesignIndex))
		{
			return;
		}
		SaveDesign();
		if (aIDesign.scope == 2)
		{
			return;
		}
		BaseEntity baseEntity = GetBaseEntity();
		BaseEntity[] array = BaseEntity.Util.FindTargets(baseEntity.ShortPrefabName, onlyPlayers: false);
		if (array == null || array.Length == 0)
		{
			return;
		}
		BaseEntity[] array2 = array;
		foreach (BaseEntity baseEntity2 in array2)
		{
			if (baseEntity2 == null || baseEntity2 == baseEntity)
			{
				continue;
			}
			EntityComponentBase[] components = baseEntity2.Components;
			if (components == null)
			{
				continue;
			}
			EntityComponentBase[] array3 = components;
			for (int j = 0; j < array3.Length; j++)
			{
				if (array3[j] is IAIDesign iAIDesign)
				{
					iAIDesign.LoadAIDesign(aIDesign, null);
					break;
				}
			}
		}
	}

	void IAIDesign.StopDesigning()
	{
		ClearDesigningPlayer();
	}

	void IAIDesign.LoadAIDesign(ProtoBuf.AIDesign design, BasePlayer player)
	{
		LoadAIDesign(design, player, loadedDesignIndex);
	}

	public bool LoadDefaultAIDesign()
	{
		if (loadedDesignIndex == 0)
		{
			return true;
		}
		return LoadAIDesignAtIndex(0);
	}

	public bool LoadAIDesignAtIndex(int index)
	{
		if (Designs == null)
		{
			return false;
		}
		if (index < 0 || index >= Designs.Count)
		{
			return false;
		}
		return LoadAIDesign(AIDesigns.GetByNameOrInstance(Designs[index].Filename, InstanceSpecificDesign), null, index);
	}

	public virtual void OnAIDesignLoadedAtIndex(int index)
	{
	}

	protected bool LoadAIDesign(ProtoBuf.AIDesign design, BasePlayer player, int index)
	{
		if (design == null)
		{
			Debug.LogError(GetBaseEntity().gameObject.name + " failed to load AI design!");
			return false;
		}
		if (player != null)
		{
			AIDesignScope scope = (AIDesignScope)design.scope;
			if (scope == AIDesignScope.Default && !player.IsDeveloper)
			{
				return false;
			}
			if (scope == AIDesignScope.EntityServerWide && !player.IsDeveloper && !player.IsAdmin)
			{
				return false;
			}
		}
		if (AIDesign == null)
		{
			return false;
		}
		AIDesign.Load(design, base.baseEntity);
		AIStateContainer defaultStateContainer = AIDesign.GetDefaultStateContainer();
		if (defaultStateContainer != null)
		{
			SwitchToState(defaultStateContainer.State, defaultStateContainer.ID);
		}
		loadedDesignIndex = index;
		OnAIDesignLoadedAtIndex(loadedDesignIndex);
		return true;
	}

	public void SaveDesign()
	{
		if (AIDesign == null)
		{
			return;
		}
		ProtoBuf.AIDesign aIDesign = AIDesign.ToProto(currentStateContainerID);
		string text = "cfg/ai/";
		string filename = Designs[loadedDesignIndex].Filename;
		switch (AIDesign.Scope)
		{
		case AIDesignScope.Default:
			text += filename;
			try
			{
				using (FileStream stream2 = File.Create(text))
				{
					ProtoBuf.AIDesign.Serialize(stream2, aIDesign);
				}
				AIDesigns.RefreshCache(filename, aIDesign);
				break;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error trying to save default AI Design: " + text);
				break;
			}
		case AIDesignScope.EntityServerWide:
			filename += "_custom";
			text += filename;
			try
			{
				using (FileStream stream = File.Create(text))
				{
					ProtoBuf.AIDesign.Serialize(stream, aIDesign);
				}
				AIDesigns.RefreshCache(filename, aIDesign);
				break;
			}
			catch (Exception)
			{
				Debug.LogWarning("Error trying to save server-wide AI Design: " + text);
				break;
			}
		case AIDesignScope.EntityInstance:
			break;
		}
	}

	[BaseEntity.RPC_Server]
	private void StopAIDesign(BaseEntity.RPCMessage msg)
	{
		if (msg.player == DesigningPlayer)
		{
			ClearDesigningPlayer();
		}
	}

	private void ClearDesigningPlayer()
	{
		DesigningPlayer = null;
	}

	public void SetOwningPlayer(BasePlayer owner)
	{
		OwningPlayer = owner;
		Events.Memory.Entity.Set(OwningPlayer, 5);
		IPet pet;
		if ((pet = this) != null && pet.IsPet())
		{
			pet.SetPetOwner(owner);
			owner.Pet = pet;
		}
	}

	public virtual bool ShouldServerThink()
	{
		if (ThinkMode == AIThinkMode.Interval && UnityEngine.Time.time > lastThinkTime + thinkRate)
		{
			return true;
		}
		return false;
	}

	public virtual void DoThink()
	{
		float delta = UnityEngine.Time.time - lastThinkTime;
		Think(delta);
	}

	public List<AIState> GetStateList()
	{
		return states.Keys.ToList();
	}

	public void Start()
	{
		AddStates();
		InitializeAI();
	}

	public virtual void AddStates()
	{
		states = new Dictionary<AIState, BasicAIState>();
	}

	public virtual void InitializeAI()
	{
		BaseEntity baseEntity = GetBaseEntity();
		baseEntity.HasBrain = true;
		Navigator = GetComponent<BaseNavigator>();
		if (UseAIDesign)
		{
			AIDesign = new AIDesign();
			AIDesign.SetAvailableStates(GetStateList());
			if (Events == null)
			{
				Events = new AIEvents();
			}
			bool senseFriendlies = MaxGroupSize > 0;
			Senses.Init(baseEntity, MemoryDuration, SenseRange, TargetLostRange, VisionCone, CheckVisionCone, CheckLOS, IgnoreNonVisionSneakers, ListenRange, HostileTargetsOnly, senseFriendlies, IgnoreSafeZonePlayers, SenseTypes, RefreshKnownLOS);
			if (DefaultDesignSO == null && Designs.Count == 0)
			{
				Debug.LogWarning("Brain on " + base.gameObject.name + " is trying to load a null AI design!");
				return;
			}
			Events.Memory.Position.Set(base.transform.position, 4);
			if (Designs.Count == 0)
			{
				Designs.Add(DefaultDesignSO);
			}
			loadedDesignIndex = 0;
			LoadAIDesign(AIDesigns.GetByNameOrInstance(Designs[loadedDesignIndex].Filename, InstanceSpecificDesign), null, loadedDesignIndex);
			AIInformationZone forPoint = AIInformationZone.GetForPoint(base.transform.position, fallBackToNearest: false);
			if (forPoint != null)
			{
				forPoint.RegisterSleepableEntity(this);
			}
		}
		BaseEntity.Query.Server.AddBrain(baseEntity);
		StartMovementTick();
	}

	public BaseEntity GetBrainBaseEntity()
	{
		return GetBaseEntity();
	}

	public virtual void OnDestroy()
	{
		if (!Rust.Application.isQuitting)
		{
			BaseEntity.Query.Server.RemoveBrain(GetBaseEntity());
			AIInformationZone aIInformationZone = null;
			HumanNPC humanNPC = GetBaseEntity() as HumanNPC;
			if (humanNPC != null)
			{
				aIInformationZone = humanNPC.VirtualInfoZone;
			}
			if (aIInformationZone == null)
			{
				aIInformationZone = AIInformationZone.GetForPoint(base.transform.position);
			}
			if (aIInformationZone != null)
			{
				aIInformationZone.UnregisterSleepableEntity(this);
			}
			LeaveGroup();
		}
	}

	private void StartMovementTick()
	{
		CancelInvoke(TickMovement);
		InvokeRandomized(TickMovement, 1f, 0.1f, 0.0100000007f);
	}

	private void StopMovementTick()
	{
		CancelInvoke(TickMovement);
	}

	public void TickMovement()
	{
		if (BasePet.queuedMovementsAllowed && UseQueuedMovementUpdates && Navigator != null)
		{
			if (BasePet.onlyQueueBaseNavMovements && Navigator.CurrentNavigationType != BaseNavigator.NavigationType.Base)
			{
				DoMovementTick();
				return;
			}
			BasePet basePet = GetBaseEntity() as BasePet;
			if (basePet != null && !basePet.inQueue)
			{
				BasePet._movementProcessQueue.Enqueue(basePet);
				basePet.inQueue = true;
			}
		}
		else
		{
			DoMovementTick();
		}
	}

	public void DoMovementTick()
	{
		float delta = UnityEngine.Time.realtimeSinceStartup - lastMovementTickTime;
		lastMovementTickTime = UnityEngine.Time.realtimeSinceStartup;
		if (Navigator != null)
		{
			Navigator.Think(delta);
		}
	}

	public void AddState(BasicAIState newState)
	{
		if (states.ContainsKey(newState.StateType))
		{
			Debug.LogWarning("Trying to add duplicate state: " + newState.StateType.ToString() + " to " + GetBaseEntity().PrefabName);
			return;
		}
		newState.brain = this;
		newState.Reset();
		states.Add(newState.StateType, newState);
	}

	public bool SwitchToState(AIState newState, int stateContainerID = -1)
	{
		if (states.ContainsKey(newState))
		{
			bool num = SwitchToState(states[newState], stateContainerID);
			if (num)
			{
				OnStateChanged();
			}
			return num;
		}
		return false;
	}

	private bool SwitchToState(BasicAIState newState, int stateContainerID = -1)
	{
		if (newState == null || !newState.CanEnter())
		{
			return false;
		}
		if (CurrentState != null)
		{
			if (!CurrentState.CanLeave())
			{
				return false;
			}
			if (CurrentState == newState && !UseAIDesign)
			{
				return false;
			}
			CurrentState.StateLeave(this, GetBaseEntity());
		}
		AddEvents(stateContainerID);
		CurrentState = newState;
		CurrentState.StateEnter(this, GetBaseEntity());
		currentStateContainerID = stateContainerID;
		return true;
	}

	protected virtual void OnStateChanged()
	{
	}

	private void AddEvents(int stateContainerID)
	{
		if (UseAIDesign && AIDesign != null)
		{
			Events.Init(this, AIDesign.GetStateContainerByID(stateContainerID), base.baseEntity, Senses);
		}
	}

	public virtual void Think(float delta)
	{
		if (this == null || !AI.think)
		{
			return;
		}
		lastThinkTime = UnityEngine.Time.time;
		if (sleeping || disabled)
		{
			return;
		}
		Age += delta;
		if (UseAIDesign)
		{
			Senses.Update();
			UpdateGroup();
		}
		if (CurrentState != null)
		{
			UpdateAgressionTimer(delta);
			StateStatus stateStatus = CurrentState.StateThink(delta, this, GetBaseEntity());
			if (Events != null)
			{
				Events.Tick(delta, stateStatus);
			}
		}
		if (UseAIDesign || (CurrentState != null && !CurrentState.CanLeave()))
		{
			return;
		}
		float num = 0f;
		BasicAIState basicAIState = null;
		foreach (BasicAIState value in states.Values)
		{
			if (value != null && value.CanEnter())
			{
				float weight = value.GetWeight();
				if (weight > num)
				{
					num = weight;
					basicAIState = value;
				}
			}
		}
		if (basicAIState != CurrentState)
		{
			SwitchToState(basicAIState);
		}
	}

	private void UpdateAgressionTimer(float delta)
	{
		if (CurrentState == null)
		{
			Senses.TimeInAgressiveState = 0f;
		}
		else if (CurrentState.AgrresiveState)
		{
			Senses.TimeInAgressiveState += delta;
		}
		else
		{
			Senses.TimeInAgressiveState = 0f;
		}
	}

	bool IAISleepable.AllowedToSleep()
	{
		return AllowedToSleep;
	}

	void IAISleepable.SleepAI()
	{
		if (!sleeping)
		{
			sleeping = true;
			if (Navigator != null)
			{
				Navigator.Pause();
			}
			StopMovementTick();
		}
	}

	void IAISleepable.WakeAI()
	{
		if (sleeping)
		{
			sleeping = false;
			if (Navigator != null)
			{
				Navigator.Resume();
			}
			StartMovementTick();
		}
	}

	private void UpdateGroup()
	{
		if (!AI.groups || MaxGroupSize <= 0 || InGroup() || Senses.Memory.Friendlies.Count <= 0)
		{
			return;
		}
		IAIGroupable iAIGroupable = null;
		foreach (BaseEntity friendly in Senses.Memory.Friendlies)
		{
			if (friendly == null)
			{
				continue;
			}
			IAIGroupable component = friendly.GetComponent<IAIGroupable>();
			if (component != null)
			{
				if (component.InGroup() && component.AddMember(this))
				{
					break;
				}
				if (iAIGroupable == null && !component.InGroup())
				{
					iAIGroupable = component;
				}
			}
		}
		if (!InGroup() && iAIGroupable != null)
		{
			AddMember(iAIGroupable);
		}
	}

	public bool AddMember(IAIGroupable member)
	{
		if (InGroup() && !IsGroupLeader)
		{
			return GroupLeader.AddMember(member);
		}
		if (MaxGroupSize <= 0)
		{
			return false;
		}
		if (groupMembers.Contains(member))
		{
			return true;
		}
		if (groupMembers.Count + 1 >= MaxGroupSize)
		{
			return false;
		}
		groupMembers.Add(member);
		IsGrouped = true;
		IsGroupLeader = true;
		GroupLeader = this;
		BaseEntity baseEntity = GetBaseEntity();
		Events.Memory.Entity.Set(baseEntity, 6);
		member.JoinGroup(this, baseEntity);
		return true;
	}

	public void JoinGroup(IAIGroupable leader, BaseEntity leaderEntity)
	{
		Events.Memory.Entity.Set(leaderEntity, 6);
		GroupLeader = leader;
		IsGroupLeader = false;
		IsGrouped = true;
	}

	public void SetGroupRoamRootPosition(Vector3 rootPos)
	{
		if (IsGroupLeader)
		{
			foreach (IAIGroupable groupMember in groupMembers)
			{
				groupMember.SetGroupRoamRootPosition(rootPos);
			}
		}
		Events.Memory.Position.Set(rootPos, 5);
	}

	public bool InGroup()
	{
		return IsGrouped;
	}

	public void LeaveGroup()
	{
		if (!InGroup())
		{
			return;
		}
		if (IsGroupLeader)
		{
			if (groupMembers.Count == 0)
			{
				return;
			}
			IAIGroupable iAIGroupable = groupMembers[0];
			if (iAIGroupable == null)
			{
				return;
			}
			RemoveMember(iAIGroupable);
			for (int num = groupMembers.Count - 1; num >= 0; num--)
			{
				IAIGroupable iAIGroupable2 = groupMembers[num];
				if (iAIGroupable2 != null && iAIGroupable2 != iAIGroupable)
				{
					RemoveMember(iAIGroupable2);
					iAIGroupable.AddMember(iAIGroupable2);
				}
			}
			groupMembers.Clear();
		}
		else if (GroupLeader != null)
		{
			GroupLeader.RemoveMember(GetComponent<IAIGroupable>());
		}
	}

	public void RemoveMember(IAIGroupable member)
	{
		if (member != null && IsGroupLeader && groupMembers.Contains(member))
		{
			groupMembers.Remove(member);
			member.SetUngrouped();
			if (groupMembers.Count == 0)
			{
				SetUngrouped();
			}
		}
	}

	public void SetUngrouped()
	{
		IsGrouped = false;
		IsGroupLeader = false;
		GroupLeader = null;
	}

	private void SendStateChangeEvent(int previousStateID, int newStateID, int sourceEventID)
	{
		if (DesigningPlayer != null)
		{
			DesigningPlayer.ClientRPCPlayer(null, DesigningPlayer, "OnDebugAIEventTriggeredStateChange", previousStateID, newStateID, sourceEventID);
		}
	}

	public void EventTriggeredStateChange(int newStateContainerID, int sourceEventID)
	{
		if (AIDesign != null && newStateContainerID != -1)
		{
			AIStateContainer stateContainerByID = AIDesign.GetStateContainerByID(newStateContainerID);
			int previousStateID = currentStateContainerID;
			SwitchToState(stateContainerByID.State, newStateContainerID);
			SendStateChangeEvent(previousStateID, currentStateContainerID, sourceEventID);
		}
	}
}
