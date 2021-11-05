#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseRidableAnimal : BaseVehicle
{
	public enum RunState
	{
		stopped = 1,
		walk,
		run,
		sprint,
		LAST
	}

	public ItemDefinition onlyAllowedItem;

	public ItemContainer.ContentsType allowedContents = ItemContainer.ContentsType.Generic;

	public int maxStackSize = 1;

	public int numSlots;

	public string lootPanelName = "generic";

	public bool needsBuildingPrivilegeToUse;

	public bool isLootable = true;

	public ItemContainer inventory;

	public const Flags Flag_ForSale = Flags.Reserved2;

	private Vector3 lastMoveDirection;

	public GameObjectRef saddlePrefab;

	public EntityRef saddleRef;

	public Transform movementLOSOrigin;

	public SoundPlayer sprintSounds;

	public SoundPlayer largeWhinny;

	public const Flags Flag_Lead = Flags.Reserved7;

	public const Flags Flag_HasRider = Flags.On;

	[Header("Purchase")]
	public ItemDefinition purchaseToken;

	public GameObjectRef eatEffect;

	public GameObjectRef CorpsePrefab;

	[Header("Obstacles")]
	public Transform animalFront;

	public float obstacleDetectionRadius = 0.25f;

	public float maxWaterDepth = 1.5f;

	public float roadSpeedBonus = 2f;

	public float maxWallClimbSlope = 53f;

	public float maxStepHeight = 1f;

	public float maxStepDownHeight = 1.35f;

	[Header("Movement")]
	public RunState currentRunState = RunState.stopped;

	public float walkSpeed = 2f;

	public float trotSpeed = 7f;

	public float runSpeed = 14f;

	public float turnSpeed = 30f;

	public float maxSpeed = 5f;

	public Transform[] groundSampleOffsets;

	[Header("Dung")]
	public ItemDefinition Dung;

	public float CaloriesToDigestPerHour = 100f;

	public float DungProducedPerCalorie = 0.001f;

	private float pendingDungCalories;

	private float dungProduction;

	[Header("Stamina")]
	public float staminaSeconds = 10f;

	public float currentMaxStaminaSeconds = 10f;

	public float maxStaminaSeconds = 20f;

	public float staminaCoreLossRatio = 0.1f;

	public float staminaCoreSpeedBonus = 3f;

	public float staminaReplenishRatioMoving = 0.5f;

	public float staminaReplenishRatioStanding = 1f;

	public float calorieToStaminaRatio = 0.1f;

	public float hydrationToStaminaRatio = 0.5f;

	public float maxStaminaCoreFromWater = 0.5f;

	public bool debugMovement = true;

	private const float normalOffsetDist = 0.15f;

	private Vector3[] normalOffsets = new Vector3[7]
	{
		new Vector3(0.15f, 0f, 0f),
		new Vector3(-0.15f, 0f, 0f),
		new Vector3(0f, 0f, 0.15f),
		new Vector3(0f, 0f, 0.3f),
		new Vector3(0f, 0f, 0.6f),
		new Vector3(0.15f, 0f, 0.3f),
		new Vector3(-0.15f, 0f, 0.3f)
	};

	[ServerVar(Help = "How long before a horse dies unattended")]
	public static float decayminutes = 180f;

	public float currentSpeed;

	public float desiredRotation;

	public float animalPitchClamp = 90f;

	public float animalRollClamp;

	public static Queue<BaseRidableAnimal> _processQueue = new Queue<BaseRidableAnimal>();

	[ServerVar]
	[Help("How many miliseconds to budget for processing ridable animals per frame")]
	public static float framebudgetms = 1f;

	[ServerVar]
	[Help("Scale all ridable animal dung production rates by this value. 0 will disable dung production.")]
	public static float dungTimeScale = 1f;

	private BaseEntity leadTarget;

	public float nextDecayTime;

	private float lastMovementUpdateTime = -1f;

	private bool inQueue;

	protected float nextEatTime;

	public float lastEatTime = float.NegativeInfinity;

	public float lastInputTime;

	private float forwardHeldSeconds;

	private float backwardHeldSeconds;

	private float sprintHeldSeconds;

	private float lastSprintPressedTime;

	private float lastForwardPressedTime;

	private float lastBackwardPressedTime;

	private float timeInMoveState;

	protected bool onIdealTerrain;

	private float nextIdealTerrainCheckTime;

	private float nextStandTime;

	private InputState aiInputState;

	public Vector3 currentVelocity;

	private Vector3 averagedUp = Vector3.up;

	private float nextGroundNormalUpdateTime;

	private Vector3 targetUp = Vector3.up;

	private float nextObstacleCheckTime;

	private float cachedObstacleDistance = float.PositiveInfinity;

	private const int maxObstacleCheckSpeed = 10;

	private float timeAlive;

	private TimeUntil dropUntilTime;

	public override bool IsNpc => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseRidableAnimal.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 2333451803u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Claim "));
				}
				using (TimeWarning.New("RPC_Claim"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2333451803u, "RPC_Claim", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_Claim(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Claim");
					}
				}
				return true;
			}
			if (rpc == 3653170552u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Lead "));
				}
				using (TimeWarning.New("RPC_Lead"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3653170552u, "RPC_Lead", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							RPC_Lead(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Lead");
					}
				}
				return true;
			}
			if (rpc == 331989034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenLoot "));
				}
				using (TimeWarning.New("RPC_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(331989034u, "RPC_OpenLoot", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage rpc2 = rPCMessage;
							RPC_OpenLoot(rpc2);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsForSale()
	{
		return HasFlag(Flags.Reserved2);
	}

	public void ContainerServerInit()
	{
		if (inventory == null)
		{
			CreateInventory(true);
			OnInventoryFirstCreated(inventory);
		}
	}

	public void CreateInventory(bool giveUID)
	{
		inventory = new ItemContainer();
		inventory.entityOwner = this;
		inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		inventory.SetOnlyAllowedItem(onlyAllowedItem);
		inventory.maxStackSize = maxStackSize;
		inventory.ServerInitialize(null, numSlots);
		inventory.canAcceptItem = ItemFilter;
		if (giveUID)
		{
			inventory.GiveUID();
		}
		inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		inventory.onDirty += OnInventoryDirty;
	}

	public void SaveContainer(SaveInfo info)
	{
		if (info.forDisk)
		{
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public virtual void OnInventoryFirstCreated(ItemContainer container)
	{
	}

	public virtual void OnInventoryDirty()
	{
	}

	public virtual void OnItemAddedOrRemoved(Item item, bool added)
	{
	}

	public bool ItemFilter(Item item, int targetSlot)
	{
		return CanAnimalAcceptItem(item, targetSlot);
	}

	public virtual bool CanAnimalAcceptItem(Item item, int targetSlot)
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract() && CanOpenStorage(player) && (!needsBuildingPrivilegeToUse || player.CanBuild()) && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(this))
			{
				player.inventory.loot.AddContainer(inventory);
				player.inventory.loot.SendImmediate();
				player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", lootPanelName);
				SendNetworkUpdate();
			}
		}
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
	}

	public virtual bool CanOpenStorage(BasePlayer player)
	{
		if (!HasFlag(Flags.On))
		{
			return true;
		}
		if (PlayerIsMounted(player))
		{
			return true;
		}
		return false;
	}

	public void LoadContainer(LoadInfo info)
	{
		if (info.fromDisk && info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
				inventory.capacity = numSlots;
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public float GetBreathingDelay()
	{
		switch (currentRunState)
		{
		default:
			return -1f;
		case RunState.walk:
			return 8f;
		case RunState.run:
			return 5f;
		case RunState.sprint:
			return 2.5f;
		}
	}

	public bool IsLeading()
	{
		return HasFlag(Flags.Reserved7);
	}

	public static float UnitsToKPH(float unitsPerSecond)
	{
		return unitsPerSecond * 60f * 60f / 1000f;
	}

	public static void ProcessQueue()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = framebudgetms / 1000f;
		while (_processQueue.Count > 0 && UnityEngine.Time.realtimeSinceStartup < realtimeSinceStartup + num)
		{
			BaseRidableAnimal baseRidableAnimal = _processQueue.Dequeue();
			if (baseRidableAnimal != null)
			{
				baseRidableAnimal.BudgetedUpdate();
				baseRidableAnimal.inQueue = false;
			}
		}
	}

	public void SetLeading(BaseEntity newLeadTarget)
	{
		leadTarget = newLeadTarget;
		SetFlag(Flags.Reserved7, leadTarget != null);
	}

	public override float GetNetworkTime()
	{
		return lastMovementUpdateTime;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		SaveContainer(info);
	}

	private void OnPhysicsNeighbourChanged()
	{
		Invoke(DelayedDropToGround, UnityEngine.Time.fixedDeltaTime);
	}

	public void DelayedDropToGround()
	{
		DropToGround(base.transform.position, true);
		UpdateGroundNormal(true);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		LoadContainer(info);
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (!IsForSale())
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public virtual void LeadingChanged()
	{
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Claim(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && Interface.CallHook("OnRidableAnimalClaim", this, player) == null && IsForSale())
		{
			Item item = GetPurchaseToken(player);
			if (item != null)
			{
				item.UseItem();
				SetFlag(Flags.Reserved2, false);
				AttemptMount(player, false);
				Interface.CallHook("OnRidableAnimalClaimed", this, player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Lead(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && !HasDriver() && !IsForSale())
		{
			bool num = IsLeading();
			bool flag = msg.read.Bit();
			if (num != flag && Interface.CallHook("OnHorseLead", this, player) == null)
			{
				SetLeading(flag ? player : null);
				LeadingChanged();
			}
		}
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		SetFlag(Flags.On, true, true);
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		SetFlag(Flags.On, false, true);
	}

	public void SetDecayActive(bool isActive)
	{
		if (isActive)
		{
			InvokeRandomized(AnimalDecay, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
		}
		else
		{
			CancelInvoke(AnimalDecay);
		}
	}

	public float TimeUntilNextDecay()
	{
		return nextDecayTime - UnityEngine.Time.time;
	}

	public void AddDecayDelay(float amount)
	{
		if (nextDecayTime < UnityEngine.Time.time)
		{
			nextDecayTime = UnityEngine.Time.time + 5f;
		}
		nextDecayTime += amount;
		if (ConVar.Global.developer > 0)
		{
			Debug.Log("Add Decay Delay ! amount is " + amount + "time until next decay : " + (nextDecayTime - UnityEngine.Time.time));
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (!IsForSale())
		{
			base.Hurt(info);
		}
	}

	public void AnimalDecay()
	{
		if (base.healthFraction == 0f || base.IsDestroyed || UnityEngine.Time.time < lastInputTime + 600f || UnityEngine.Time.time < lastEatTime + 600f || IsForSale())
		{
			return;
		}
		if (UnityEngine.Time.time < nextDecayTime)
		{
			if (ConVar.Global.developer > 0)
			{
				Debug.Log("Skipping animal decay due to hitching");
			}
		}
		else
		{
			float num = 1f / decayminutes;
			float num2 = ((!IsOutside()) ? 1f : 0.5f);
			Hurt(MaxHealth() * num * num2, DamageType.Decay, this, false);
		}
	}

	public void UseStamina(float amount)
	{
		if (onIdealTerrain)
		{
			amount *= 0.5f;
		}
		staminaSeconds -= amount;
		if (staminaSeconds <= 0f)
		{
			staminaSeconds = 0f;
		}
	}

	public bool CanInitiateSprint()
	{
		return staminaSeconds > 4f;
	}

	public bool CanSprint()
	{
		return staminaSeconds > 0f;
	}

	public void ReplenishStamina(float amount)
	{
		float num = 1f + Mathf.InverseLerp(maxStaminaSeconds * 0.5f, maxStaminaSeconds, currentMaxStaminaSeconds);
		amount *= num;
		amount = Mathf.Min(currentMaxStaminaSeconds - staminaSeconds, amount);
		float num2 = Mathf.Min(currentMaxStaminaSeconds - staminaCoreLossRatio * amount, amount * staminaCoreLossRatio);
		currentMaxStaminaSeconds = Mathf.Clamp(currentMaxStaminaSeconds - num2, 0f, maxStaminaSeconds);
		staminaSeconds = Mathf.Clamp(staminaSeconds + num2 / staminaCoreLossRatio, 0f, currentMaxStaminaSeconds);
	}

	public virtual float ReplenishRatio()
	{
		return 1f;
	}

	public void ReplenishStaminaCore(float calories, float hydration)
	{
		float num = calories * calorieToStaminaRatio;
		float b = hydration * hydrationToStaminaRatio;
		float num2 = ReplenishRatio();
		b = Mathf.Min(maxStaminaCoreFromWater - currentMaxStaminaSeconds, b);
		if (b < 0f)
		{
			b = 0f;
		}
		float num3 = num + b * num2;
		currentMaxStaminaSeconds = Mathf.Clamp(currentMaxStaminaSeconds + num3, 0f, maxStaminaSeconds);
		staminaSeconds = Mathf.Clamp(staminaSeconds + num3, 0f, currentMaxStaminaSeconds);
	}

	public void UpdateStamina(float delta)
	{
		if (currentRunState == RunState.sprint)
		{
			UseStamina(delta);
		}
		else if (currentRunState == RunState.run)
		{
			ReplenishStamina(staminaReplenishRatioMoving * delta);
		}
		else
		{
			ReplenishStamina(staminaReplenishRatioStanding * delta);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		RiderInput(inputState, player);
	}

	public void DismountHeavyPlayers()
	{
		if (HasFlag(Flags.On))
		{
			BasePlayer driver = GetDriver();
			if ((bool)driver && IsPlayerTooHeavy(driver))
			{
				DismountAllPlayers();
			}
		}
	}

	public BaseMountable GetSaddle()
	{
		if (!saddleRef.IsValid(base.isServer))
		{
			return null;
		}
		return saddleRef.Get(base.isServer).GetComponent<BaseMountable>();
	}

	public void BudgetedUpdate()
	{
		DismountHeavyPlayers();
		UpdateOnIdealTerrain();
		UpdateStamina(UnityEngine.Time.fixedDeltaTime);
		if (currentRunState == RunState.stopped)
		{
			EatNearbyFood();
		}
		if (lastMovementUpdateTime == -1f)
		{
			lastMovementUpdateTime = UnityEngine.Time.realtimeSinceStartup;
		}
		float delta = UnityEngine.Time.realtimeSinceStartup - lastMovementUpdateTime;
		UpdateMovement(delta);
		lastMovementUpdateTime = UnityEngine.Time.realtimeSinceStartup;
		UpdateDung(delta);
	}

	public void ApplyDungCalories(float calories)
	{
		pendingDungCalories += calories;
	}

	private void UpdateDung(float delta)
	{
		if (!(Dung == null) && !Mathf.Approximately(dungTimeScale, 0f))
		{
			float num = Mathf.Min(pendingDungCalories * delta, CaloriesToDigestPerHour / 3600f * delta) * DungProducedPerCalorie;
			dungProduction += num;
			pendingDungCalories -= num;
			if (dungProduction >= 1f)
			{
				DoDung();
			}
		}
	}

	private void DoDung()
	{
		dungProduction -= 1f;
		ItemManager.Create(Dung, 1, 0uL).Drop(base.transform.position + -base.transform.forward + Vector3.up * 1.1f + UnityEngine.Random.insideUnitSphere * 0.1f, -base.transform.forward);
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		timeAlive += UnityEngine.Time.fixedDeltaTime;
		if (!inQueue)
		{
			_processQueue.Enqueue(this);
			inQueue = true;
		}
	}

	public float StaminaCoreFraction()
	{
		return Mathf.InverseLerp(0f, maxStaminaSeconds, currentMaxStaminaSeconds);
	}

	public void DoEatEvent()
	{
		ClientRPC(null, "Eat");
	}

	public void ReplenishFromFood(ItemModConsumable consumable)
	{
		if ((bool)consumable)
		{
			ClientRPC(null, "Eat");
			lastEatTime = UnityEngine.Time.time;
			float ifType = consumable.GetIfType(MetabolismAttribute.Type.Calories);
			float ifType2 = consumable.GetIfType(MetabolismAttribute.Type.Hydration);
			float num = consumable.GetIfType(MetabolismAttribute.Type.Health) + consumable.GetIfType(MetabolismAttribute.Type.HealthOverTime);
			ApplyDungCalories(ifType);
			ReplenishStaminaCore(ifType, ifType2);
			Heal(num * 4f);
		}
	}

	public virtual void EatNearbyFood()
	{
		if (UnityEngine.Time.time < nextEatTime)
		{
			return;
		}
		float num = StaminaCoreFraction();
		nextEatTime = UnityEngine.Time.time + UnityEngine.Random.Range(2f, 3f) + Mathf.InverseLerp(0.5f, 1f, num) * 4f;
		if (num >= 1f)
		{
			return;
		}
		List<BaseEntity> obj = Facepunch.Pool.GetList<BaseEntity>();
		Vis.Entities(base.transform.position + base.transform.forward * 1.5f, 2f, obj, 67109377);
		obj.Sort((BaseEntity a, BaseEntity b) => (b is DroppedItem).CompareTo(a is DroppedItem));
		foreach (BaseEntity item in obj)
		{
			if (item.isClient)
			{
				continue;
			}
			DroppedItem droppedItem = item as DroppedItem;
			if ((bool)droppedItem && droppedItem.item != null && droppedItem.item.info.category == ItemCategory.Food)
			{
				ItemModConsumable component = droppedItem.item.info.GetComponent<ItemModConsumable>();
				if ((bool)component)
				{
					ReplenishFromFood(component);
					droppedItem.item.UseItem();
					if (droppedItem.item.amount <= 0)
					{
						droppedItem.Kill();
					}
					break;
				}
			}
			CollectibleEntity collectibleEntity = item as CollectibleEntity;
			if ((bool)collectibleEntity && collectibleEntity.IsFood())
			{
				collectibleEntity.DoPickup(null);
				break;
			}
			GrowableEntity growableEntity = item as GrowableEntity;
			if ((bool)growableEntity && growableEntity.CanPick())
			{
				growableEntity.PickFruit(null);
				break;
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public void SwitchMoveState(RunState newState)
	{
		if (newState != currentRunState)
		{
			currentRunState = newState;
			timeInMoveState = 0f;
			SetFlag(Flags.Reserved8, currentRunState == RunState.sprint, false, false);
			MarkObstacleDistanceDirty();
		}
	}

	public void UpdateOnIdealTerrain()
	{
		if (!(UnityEngine.Time.time < nextIdealTerrainCheckTime))
		{
			nextIdealTerrainCheckTime = UnityEngine.Time.time + UnityEngine.Random.Range(1f, 2f);
			onIdealTerrain = false;
			if (TerrainMeta.TopologyMap != null && ((uint)TerrainMeta.TopologyMap.GetTopology(base.transform.position) & 0x800u) != 0)
			{
				onIdealTerrain = true;
			}
		}
	}

	public float MoveStateToVelocity(RunState stateToCheck)
	{
		float num = 0f;
		switch (stateToCheck)
		{
		default:
			return 0f;
		case RunState.walk:
			return GetWalkSpeed();
		case RunState.run:
			return GetTrotSpeed();
		case RunState.sprint:
			return GetRunSpeed();
		}
	}

	public float GetDesiredVelocity()
	{
		return MoveStateToVelocity(currentRunState);
	}

	public RunState StateFromSpeed(float speedToUse)
	{
		if (speedToUse <= MoveStateToVelocity(RunState.stopped))
		{
			return RunState.stopped;
		}
		if (speedToUse <= MoveStateToVelocity(RunState.walk))
		{
			return RunState.walk;
		}
		if (speedToUse <= MoveStateToVelocity(RunState.run))
		{
			return RunState.run;
		}
		return RunState.sprint;
	}

	public void ModifyRunState(int dir)
	{
		if ((currentRunState != RunState.stopped || dir >= 0) && (currentRunState != RunState.sprint || dir <= 0))
		{
			RunState newState = currentRunState + dir;
			SwitchMoveState(newState);
		}
	}

	public bool CanStand()
	{
		if (nextStandTime > UnityEngine.Time.time)
		{
			return false;
		}
		if (mountPoints[0].mountable == null)
		{
			return false;
		}
		bool flag = false;
		List<Collider> obj = Facepunch.Pool.GetList<Collider>();
		Vis.Colliders(mountPoints[0].mountable.eyePositionOverride.transform.position - base.transform.forward * 1f, 2f, obj, 2162689);
		if (obj.Count > 0)
		{
			flag = true;
		}
		Facepunch.Pool.FreeList(ref obj);
		return !flag;
	}

	public void DoDebugMovement()
	{
		if (aiInputState == null)
		{
			aiInputState = new InputState();
		}
		if (!debugMovement)
		{
			aiInputState.current.buttons &= -3;
			aiInputState.current.buttons &= -9;
			aiInputState.current.buttons &= -129;
		}
		else
		{
			aiInputState.current.buttons |= 2;
			aiInputState.current.buttons |= 8;
			aiInputState.current.buttons |= 128;
			RiderInput(aiInputState, null);
		}
	}

	public virtual void RiderInput(InputState inputState, BasePlayer player)
	{
		float value = UnityEngine.Time.time - lastInputTime;
		lastInputTime = UnityEngine.Time.time;
		value = Mathf.Clamp(value, 0f, 1f);
		Vector3 zero = Vector3.zero;
		timeInMoveState += value;
		if (inputState == null)
		{
			return;
		}
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			lastForwardPressedTime = UnityEngine.Time.time;
			forwardHeldSeconds += value;
		}
		else
		{
			forwardHeldSeconds = 0f;
		}
		if (inputState.IsDown(BUTTON.BACKWARD))
		{
			lastBackwardPressedTime = UnityEngine.Time.time;
			backwardHeldSeconds += value;
		}
		else
		{
			backwardHeldSeconds = 0f;
		}
		if (inputState.IsDown(BUTTON.SPRINT))
		{
			lastSprintPressedTime = UnityEngine.Time.time;
			sprintHeldSeconds += value;
		}
		else
		{
			sprintHeldSeconds = 0f;
		}
		if (inputState.IsDown(BUTTON.DUCK) && CanStand() && (currentRunState == RunState.stopped || (currentRunState == RunState.walk && currentSpeed < 1f)))
		{
			ClientRPC(null, "Stand");
			nextStandTime = UnityEngine.Time.time + 3f;
			currentSpeed = 0f;
		}
		if (UnityEngine.Time.time < nextStandTime)
		{
			forwardHeldSeconds = 0f;
			backwardHeldSeconds = 0f;
		}
		if (forwardHeldSeconds > 0f)
		{
			if (currentRunState == RunState.stopped)
			{
				SwitchMoveState(RunState.walk);
			}
			else if (currentRunState == RunState.walk)
			{
				if (sprintHeldSeconds > 0f)
				{
					SwitchMoveState(RunState.run);
				}
			}
			else if (currentRunState == RunState.run && sprintHeldSeconds > 1f && CanInitiateSprint())
			{
				SwitchMoveState(RunState.sprint);
			}
		}
		else if (backwardHeldSeconds > 1f)
		{
			ModifyRunState(-1);
			backwardHeldSeconds = 0.1f;
		}
		else if (backwardHeldSeconds == 0f && forwardHeldSeconds == 0f && timeInMoveState > 1f && currentRunState != RunState.stopped)
		{
			ModifyRunState(-1);
		}
		if (currentRunState == RunState.sprint && (!CanSprint() || UnityEngine.Time.time - lastSprintPressedTime > 5f))
		{
			ModifyRunState(-1);
		}
		if (inputState.IsDown(BUTTON.RIGHT))
		{
			if (currentRunState == RunState.stopped)
			{
				ModifyRunState(1);
			}
			desiredRotation = 1f;
		}
		else if (inputState.IsDown(BUTTON.LEFT))
		{
			if (currentRunState == RunState.stopped)
			{
				ModifyRunState(1);
			}
			desiredRotation = -1f;
		}
		else
		{
			desiredRotation = 0f;
		}
	}

	public override float MaxVelocity()
	{
		return maxSpeed * 1.5f;
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			angle -= 360f;
		}
		return angle;
	}

	public void UpdateGroundNormal(bool force = false)
	{
		if (UnityEngine.Time.time >= nextGroundNormalUpdateTime || force)
		{
			nextGroundNormalUpdateTime = UnityEngine.Time.time + UnityEngine.Random.Range(0.2f, 0.3f);
			targetUp = averagedUp;
			Transform[] array = groundSampleOffsets;
			for (int i = 0; i < array.Length; i++)
			{
				Vector3 pos;
				Vector3 normal;
				if (TransformUtil.GetGroundInfo(array[i].position + Vector3.up * 2f, out pos, out normal, 4f, 278986753))
				{
					targetUp += normal;
				}
				else
				{
					targetUp += Vector3.up;
				}
			}
			targetUp /= (float)(groundSampleOffsets.Length + 1);
		}
		averagedUp = Vector3.Lerp(averagedUp, targetUp, UnityEngine.Time.deltaTime * 2f);
	}

	public void MarkObstacleDistanceDirty()
	{
		nextObstacleCheckTime = 0f;
	}

	public float GetObstacleDistance()
	{
		if (UnityEngine.Time.time >= nextObstacleCheckTime)
		{
			float desiredVelocity = GetDesiredVelocity();
			if (currentSpeed > 0f || desiredVelocity > 0f)
			{
				cachedObstacleDistance = ObstacleDistanceCheck(Mathf.Max(desiredVelocity, 2f));
			}
			nextObstacleCheckTime = UnityEngine.Time.time + UnityEngine.Random.Range(0.25f, 0.35f);
		}
		return cachedObstacleDistance;
	}

	public float ObstacleDistanceCheck(float speed = 10f)
	{
		Vector3 position = base.transform.position;
		int num = Mathf.Max(2, Mathf.Min((int)speed, 10));
		float num2 = 0.5f;
		int num3 = Mathf.CeilToInt((float)num / num2);
		float num4 = 0f;
		Vector3 vector = QuaternionEx.LookRotationForcedUp(base.transform.forward, Vector3.up) * Vector3.forward;
		Vector3 vector2 = movementLOSOrigin.transform.position;
		vector2.y = base.transform.position.y;
		Vector3 up = base.transform.up;
		for (int i = 0; i < num3; i++)
		{
			float num5 = num2;
			bool flag = false;
			float num6 = 0f;
			Vector3 pos = Vector3.zero;
			Vector3 normal = Vector3.up;
			Vector3 vector3 = vector2;
			Vector3 origin = vector3 + Vector3.up * (maxStepHeight + obstacleDetectionRadius);
			Vector3 vector4 = vector3 + vector * num5;
			float num7 = maxStepDownHeight + obstacleDetectionRadius;
			RaycastHit hitInfo;
			if (UnityEngine.Physics.SphereCast(origin, obstacleDetectionRadius, vector, out hitInfo, num5, 1486954753))
			{
				num6 = hitInfo.distance;
				pos = hitInfo.point;
				normal = hitInfo.normal;
				flag = true;
			}
			if (!flag)
			{
				if (!TransformUtil.GetGroundInfo(vector4 + Vector3.up * 2f, out pos, out normal, 2f + num7, 278986753))
				{
					return num4;
				}
				num6 = Vector3.Distance(vector3, pos);
				if (WaterLevel.Test(pos + Vector3.one * maxWaterDepth, true, this))
				{
					normal = -base.transform.forward;
					return num4;
				}
				flag = true;
			}
			if (flag)
			{
				float num8 = Vector3.Angle(up, normal);
				float num9 = Vector3.Angle(normal, Vector3.up);
				if (num8 > maxWallClimbSlope || num9 > maxWallClimbSlope)
				{
					Vector3 vector5 = normal;
					float num10 = pos.y;
					int num11 = 1;
					for (int j = 0; j < normalOffsets.Length; j++)
					{
						Vector3 vector6 = vector4 + normalOffsets[j].x * base.transform.right;
						float num12 = maxStepHeight * 2.5f;
						Vector3 pos2;
						Vector3 normal2;
						if (TransformUtil.GetGroundInfo(vector6 + Vector3.up * num12 + normalOffsets[j].z * base.transform.forward, out pos2, out normal2, num7 + num12, 278986753))
						{
							num11++;
							vector5 += normal2;
							num10 += pos2.y;
						}
					}
					num10 /= (float)num11;
					vector5.Normalize();
					float num13 = Vector3.Angle(up, vector5);
					num9 = Vector3.Angle(vector5, Vector3.up);
					if (num13 > maxWallClimbSlope || num9 > maxWallClimbSlope || Mathf.Abs(num10 - vector4.y) > maxStepHeight)
					{
						return num4;
					}
				}
			}
			num4 += num6;
			vector = QuaternionEx.LookRotationForcedUp(base.transform.forward, normal) * Vector3.forward;
			vector2 = pos;
		}
		return num4;
	}

	public virtual void MarkDistanceTravelled(float amount)
	{
	}

	public void UpdateMovement(float delta)
	{
		float num = WaterFactor();
		if (num > 1f && !base.IsDestroyed)
		{
			Kill();
			return;
		}
		if (desiredRotation != 0f)
		{
			MarkObstacleDistanceDirty();
		}
		if (num >= 0.3f && currentRunState > RunState.run)
		{
			currentRunState = RunState.run;
		}
		else if (num >= 0.45f && currentRunState > RunState.walk)
		{
			currentRunState = RunState.walk;
		}
		if (UnityEngine.Time.time - lastInputTime > 3f && !IsLeading())
		{
			currentRunState = RunState.stopped;
			desiredRotation = 0f;
		}
		if ((HasDriver() && IsLeading()) || leadTarget == null)
		{
			SetLeading(null);
		}
		if (IsLeading())
		{
			Vector3 position = leadTarget.transform.position;
			Vector3 lhs = Vector3Ex.Direction2D(base.transform.position + base.transform.right * 1f, base.transform.position);
			Vector3 lhs2 = Vector3Ex.Direction2D(base.transform.position + base.transform.forward * 0.01f, base.transform.position);
			Vector3 rhs = Vector3Ex.Direction2D(position, base.transform.position);
			float value = Vector3.Dot(lhs, rhs);
			float num2 = Vector3.Dot(lhs2, rhs);
			bool flag = Vector3Ex.Distance2D(position, base.transform.position) > 2.5f;
			bool num3 = Vector3Ex.Distance2D(position, base.transform.position) > 10f;
			if (flag || num2 < 0.95f)
			{
				float num4 = Mathf.InverseLerp(0f, 1f, value);
				float num5 = 1f - Mathf.InverseLerp(-1f, 0f, value);
				desiredRotation = 0f;
				desiredRotation += num4 * 1f;
				desiredRotation += num5 * -1f;
				if (Mathf.Abs(desiredRotation) < 0.001f)
				{
					desiredRotation = 0f;
				}
				if (flag)
				{
					SwitchMoveState(RunState.walk);
				}
				else
				{
					SwitchMoveState(RunState.stopped);
				}
			}
			else
			{
				desiredRotation = 0f;
				SwitchMoveState(RunState.stopped);
			}
			if (num3)
			{
				SetLeading(null);
				SwitchMoveState(RunState.stopped);
			}
		}
		float obstacleDistance = GetObstacleDistance();
		RunState runState = StateFromSpeed(obstacleDistance * GetRunSpeed());
		if (runState < currentRunState)
		{
			SwitchMoveState(runState);
		}
		float desiredVelocity = GetDesiredVelocity();
		Vector3 direction = Vector3.forward * Mathf.Sign(desiredVelocity);
		float num6 = Mathf.InverseLerp(0.85f, 1f, obstacleDistance);
		float num7 = Mathf.InverseLerp(1.25f, 10f, obstacleDistance);
		float num8 = 1f - Mathf.InverseLerp(20f, 45f, Vector3.Angle(Vector3.up, averagedUp));
		num7 = num6 * 0.1f + num7 * 0.9f;
		float num9 = Mathf.Min(Mathf.Clamp01(Mathf.Min(num8 + 0.2f, num7)) * GetRunSpeed(), desiredVelocity);
		float num10 = ((num9 < currentSpeed) ? 3f : 1f);
		if (Mathf.Abs(currentSpeed) < 2f && desiredVelocity == 0f)
		{
			currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, delta * 3f);
		}
		else
		{
			currentSpeed = Mathf.Lerp(currentSpeed, num9, delta * num10);
		}
		if (num7 == 0f)
		{
			currentSpeed = 0f;
		}
		float num11 = 1f - Mathf.InverseLerp(2f, 7f, currentSpeed);
		num11 = (num11 + 1f) / 2f;
		if (desiredRotation != 0f)
		{
			Vector3 position2 = animalFront.transform.position;
			Quaternion rotation = base.transform.rotation;
			base.transform.Rotate(Vector3.up, desiredRotation * delta * turnSpeed * num11);
			if (!IsLeading() && Vis.AnyColliders(animalFront.transform.position, obstacleDetectionRadius * 0.25f, 1503731969))
			{
				base.transform.rotation = rotation;
			}
		}
		Vector3 vector = base.transform.TransformDirection(direction);
		Vector3 normalized = vector.normalized;
		float num12 = currentSpeed * delta;
		Vector3 vector2 = base.transform.position + normalized * num12 * Mathf.Sign(currentSpeed);
		currentVelocity = vector * currentSpeed;
		UpdateGroundNormal();
		if (!(currentSpeed > 0f) && !(timeAlive < 2f) && !((float)dropUntilTime > 0f))
		{
			return;
		}
		Vector3 vector3 = base.transform.position + base.transform.InverseTransformPoint(animalFront.transform.position).y * base.transform.up;
		RaycastHit hitInfo;
		bool flag2 = UnityEngine.Physics.SphereCast(animalFront.transform.position, obstacleDetectionRadius, normalized, out hitInfo, num12, 1503731969);
		bool flag3 = UnityEngine.Physics.SphereCast(base.transform.position + base.transform.InverseTransformPoint(animalFront.transform.position).y * base.transform.up, obstacleDetectionRadius, normalized, out hitInfo, num12, 1503731969);
		if (!Vis.AnyColliders(animalFront.transform.position + normalized * num12, obstacleDetectionRadius, 1503731969) && !flag2 && !flag3)
		{
			if (DropToGround(vector2 + Vector3.up * maxStepHeight))
			{
				MarkDistanceTravelled(num12);
			}
			else
			{
				currentSpeed = 0f;
			}
		}
		else
		{
			currentSpeed = 0f;
		}
	}

	public bool DropToGround(Vector3 targetPos, bool force = false)
	{
		float range = (force ? 10000f : (maxStepHeight + maxStepDownHeight));
		Vector3 pos;
		Vector3 normal;
		if (TransformUtil.GetGroundInfo(targetPos, out pos, out normal, range, 278986753))
		{
			if (UnityEngine.Physics.CheckSphere(pos + Vector3.up * 1f, 0.2f, 278986753))
			{
				return false;
			}
			base.transform.position = pos;
			Vector3 eulerAngles = QuaternionEx.LookRotationForcedUp(base.transform.forward, averagedUp).eulerAngles;
			if (eulerAngles.z > 180f)
			{
				eulerAngles.z -= 360f;
			}
			else if (eulerAngles.z < -180f)
			{
				eulerAngles.z += 360f;
			}
			eulerAngles.z = Mathf.Clamp(eulerAngles.z, -10f, 10f);
			base.transform.rotation = Quaternion.Euler(eulerAngles);
			return true;
		}
		return false;
	}

	public void DoNetworkUpdate()
	{
		SendNetworkUpdate();
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(false);
	}

	public override void ServerInit()
	{
		ContainerServerInit();
		base.ServerInit();
		InvokeRepeating(DoNetworkUpdate, UnityEngine.Random.Range(0f, 0.2f), 0.333f);
		SetDecayActive(true);
		if (debugMovement)
		{
			InvokeRandomized(DoDebugMovement, 0f, 0.1f, 0.1f);
		}
	}

	public override void OnKilled(HitInfo hitInfo = null)
	{
		Assert.IsTrue(base.isServer, "OnKilled called on client!");
		BaseCorpse baseCorpse = DropCorpse(CorpsePrefab.resourcePath);
		if ((bool)baseCorpse)
		{
			SetupCorpse(baseCorpse);
			baseCorpse.Spawn();
			baseCorpse.TakeChildren(this);
		}
		Invoke(base.KillMessage, 0.5f);
		base.OnKilled(hitInfo);
	}

	public virtual void SetupCorpse(BaseCorpse corpse)
	{
		corpse.flags = flags;
		LootableCorpse component = corpse.GetComponent<LootableCorpse>();
		if ((bool)component)
		{
			component.TakeFrom(inventory);
		}
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return currentVelocity;
	}

	public void UpdateDropToGroundForDuration(float duration)
	{
		dropUntilTime = duration;
	}

	public bool PlayerHasToken(BasePlayer player)
	{
		return GetPurchaseToken(player) != null;
	}

	public Item GetPurchaseToken(BasePlayer player)
	{
		int itemid = purchaseToken.itemid;
		return player.inventory.FindItemID(itemid);
	}

	public virtual float GetWalkSpeed()
	{
		return walkSpeed;
	}

	public virtual float GetTrotSpeed()
	{
		return trotSpeed;
	}

	public virtual float GetRunSpeed()
	{
		if (base.isServer)
		{
			float runSpeed2 = runSpeed;
			float num = Mathf.InverseLerp(maxStaminaSeconds * 0.5f, maxStaminaSeconds, currentMaxStaminaSeconds) * staminaCoreSpeedBonus;
			float num2 = (onIdealTerrain ? roadSpeedBonus : 0f);
			return runSpeed + num + num2;
		}
		return runSpeed;
	}

	public bool IsPlayerTooHeavy(BasePlayer player)
	{
		return player.Weight >= 10f;
	}
}
