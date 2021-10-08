#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Modular;
using UnityEngine;
using UnityEngine.Assertions;

public class ModularCar : BaseModularVehicle, TriggerHurtNotChild.IHurtTriggerUser, TakeCollisionDamage.ICanRestoreVelocity, IVehicleLockUser, IEngineControllerUser, IEntity
{
	private class DriverSeatInputs
	{
		public float steerInput;

		public bool steerMod;

		public float brakeInput;

		public float throttleInput;
	}

	[Serializable]
	public class Wheel
	{
		public WheelCollider wheelCollider;

		public Transform visualWheel;

		public Transform visualWheelSteering;

		public bool steerWheel;

		public bool brakeWheel = true;

		public bool powerWheel = true;

		public bool visualPowerWheel = true;

		public ParticleSystem snowFX;

		public ParticleSystem sandFX;

		public ParticleSystem dirtFX;

		public ParticleSystem waterFX;

		public ParticleSystem snowSpinFX;

		public ParticleSystem sandSpinFX;

		public ParticleSystem dirtSpinFX;

		public ParticleSystem asphaltSpinFX;
	}

	[Serializable]
	public class SpawnSettings
	{
		[Tooltip("Must be true to use any of these settings.")]
		public bool useSpawnSettings;

		[Tooltip("Specify a list of possible module configurations that'll automatically spawn with this vehicle.")]
		public ModularCarPresetConfig[] configurationOptions;

		[Tooltip("Min health % at spawn for any modules that spawn with this chassis.")]
		public float minStartHealthPercent = 0.15f;

		[Tooltip("Max health  % at spawn for any modules that spawn with this chassis.")]
		public float maxStartHealthPercent = 0.5f;
	}

	public static HashSet<ModularCar> allCarsList = new HashSet<ModularCar>();

	public readonly ListDictionary<BaseMountable, DriverSeatInputs> driverSeatInputs = new ListDictionary<BaseMountable, DriverSeatInputs>();

	public ModularCarPhysics carPhysics;

	public VehicleTerrainHandler serverTerrainHandler;

	public float nextCollisionFXTime;

	public float nextCollisionDamageTime;

	public float lastEngineOnTime;

	private const float DECAY_TICK_TIME = 60f;

	private const float INSIDE_DECAY_MULTIPLIER = 0.1f;

	private const float CORPSE_DECAY_MINUTES = 5f;

	public Vector3 prevPosition;

	public Quaternion prevRotation;

	public float deathDamageCounter;

	private const float DAMAGE_TO_GIB = 600f;

	public TimeSince timeSinceDeath;

	private const float IMMUNE_TIME = 1f;

	public Dictionary<BaseEntity, float> damageSinceLastTick = new Dictionary<BaseEntity, float>();

	public readonly Vector3 groundedCOMMultiplier = new Vector3(0.25f, 0.3f, 0.25f);

	public readonly Vector3 airbourneCOMMultiplier = new Vector3(0.25f, 0.75f, 0.25f);

	public Vector3 prevCOMMultiplier;

	[Header("Modular Car")]
	public ModularCarChassisVisuals chassisVisuals;

	public Wheel wheelFL;

	public Wheel wheelFR;

	public Wheel wheelRL;

	public Wheel wheelRR;

	public ItemDefinition carKeyDefinition;

	[SerializeField]
	public ModularCarSettings carSettings;

	[SerializeField]
	public float hurtTriggerMinSpeed = 1f;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerFront;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerRear;

	[SerializeField]
	public ProtectionProperties immortalProtection;

	[SerializeField]
	public ProtectionProperties mortalProtection;

	[Header("Spawn")]
	[SerializeField]
	public SpawnSettings spawnSettings;

	[Header("Fuel")]
	[SerializeField]
	public GameObjectRef fuelStoragePrefab;

	[SerializeField]
	public Transform fuelStoragePoint;

	[Header("Audio/FX")]
	[SerializeField]
	public ModularCarAudio carAudio;

	[SerializeField]
	public GameObjectRef collisionEffect;

	[SerializeField]
	[HideInInspector]
	public MeshRenderer[] damageShowingRenderers;

	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population = 3f;

	[ServerVar(Help = "How many minutes before a ModularCar loses all its health while outside")]
	public static float outsidedecayminutes = 216f;

	public const BUTTON MouseSteerButton = BUTTON.DUCK;

	public const BUTTON RapidSteerButton = BUTTON.SPRINT;

	public ModularCarLock carLock;

	public VehicleEngineController engineController;

	public VehicleEngineController.EngineState lastSetEngineState;

	public EntityFuelSystem fuelSystem;

	public float cachedFuelFraction;

	public VehicleTerrainHandler.Surface OnSurface
	{
		get
		{
			if (serverTerrainHandler == null)
			{
				return VehicleTerrainHandler.Surface.Default;
			}
			return serverTerrainHandler.OnSurface;
		}
	}

	public override bool AlwaysAllowBradleyTargeting => true;

	public float DriveWheelVelocity
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.DriveWheelVelocity;
			}
			return 0f;
		}
	}

	public float DriveWheelSlip
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.DriveWheelSlip;
			}
			return 0f;
		}
	}

	public float SteerAngle
	{
		get
		{
			if (base.isServer)
			{
				return carPhysics.SteerAngle;
			}
			return 0f;
		}
	}

	public new ItemDefinition AssociatedItemDef => repair.itemTarget;

	public float MaxSteerAngle => carSettings.maxSteerAngle;

	public override bool IsLockable => carLock.HasALock;

	public VehicleEngineController.EngineState CurEngineState => engineController.CurEngineState;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ModularCar.OnRpcMessage"))
		{
			if (rpc == 1851540757 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenFuel "));
				}
				using (TimeWarning.New("RPC_OpenFuel"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							RPC_OpenFuel(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenFuel");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			ProcessCollision(collision);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		carPhysics = new ModularCarPhysics(this, base.transform, rigidBody, carSettings);
		serverTerrainHandler = new VehicleTerrainHandler(this);
		if (!Rust.Application.isLoadingSave)
		{
			SpawnPreassignedModules();
		}
		lastEngineOnTime = UnityEngine.Time.realtimeSinceStartup;
		allCarsList.Add(this);
		InvokeRandomized(UpdateClients, 0f, 0.15f, 0.02f);
		InvokeRandomized(DecayTick, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	public override void DoServerDestroy()
	{
		base.DoServerDestroy();
		allCarsList.Remove(this);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		carLock.EnableCentralLockingIfNoDriver();
		if (IsDead())
		{
			Kill();
		}
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return fuelSystem;
	}

	public float GetPlayerDamageMultiplier()
	{
		return Mathf.Abs(GetSpeed()) * 1f;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
		if (!base.isClient && !hurtEntity.IsDestroyed)
		{
			Vector3 vector = hurtEntity.GetLocalVelocity() - base.Velocity;
			Vector3 position = ClosestPoint(hurtEntity.transform.position);
			Vector3 vector2 = hurtEntity.RealisticMass * vector;
			rigidBody.AddForceAtPosition(vector2 * 1.25f, position, ForceMode.Impulse);
			QueueCollisionDamage(this, vector2.magnitude * 0.75f / UnityEngine.Time.deltaTime);
			carPhysics.SetTempDrag(2.25f, 1f);
		}
	}

	public float GetSteerInput()
	{
		float num = 0f;
		BufferList<DriverSeatInputs> values = driverSeatInputs.Values;
		for (int i = 0; i < values.Count; i++)
		{
			num += values[i].steerInput;
		}
		return Mathf.Clamp(num, -1f, 1f);
	}

	public bool GetSteerModInput()
	{
		BufferList<DriverSeatInputs> values = driverSeatInputs.Values;
		for (int i = 0; i < values.Count; i++)
		{
			if (values[i].steerMod)
			{
				return true;
			}
		}
		return false;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (base.isClient || !isSpawned)
		{
			return;
		}
		float speed = GetSpeed();
		carPhysics.FixedUpdate(UnityEngine.Time.fixedDeltaTime, speed);
		if (CurEngineState != 0 && !CanRunEngines())
		{
			engineController.StopEngine();
		}
		hurtTriggerFront.gameObject.SetActive(speed > hurtTriggerMinSpeed);
		hurtTriggerRear.gameObject.SetActive(speed < 0f - hurtTriggerMinSpeed);
		if (serverTerrainHandler != null)
		{
			serverTerrainHandler.FixedUpdate();
		}
		SetFlag(Flags.Reserved7, rigidBody.position == prevPosition && rigidBody.rotation == prevRotation);
		prevPosition = rigidBody.position;
		prevRotation = rigidBody.rotation;
		if (IsMoving())
		{
			Vector3 cOMMultiplier = GetCOMMultiplier();
			if (cOMMultiplier != prevCOMMultiplier)
			{
				rigidBody.centerOfMass = Vector3.Scale(realLocalCOM, cOMMultiplier);
				prevCOMMultiplier = cOMMultiplier;
			}
		}
		if (!(UnityEngine.Time.time >= nextCollisionDamageTime))
		{
			return;
		}
		nextCollisionDamageTime = UnityEngine.Time.time + 0.33f;
		foreach (KeyValuePair<BaseEntity, float> item in damageSinceLastTick)
		{
			DoCollisionDamage(item.Key, item.Value);
		}
		damageSinceLastTick.Clear();
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		MountPointInfo playerSeatInfo = GetPlayerSeatInfo(player);
		if (!playerSeatInfo.isDriver)
		{
			return;
		}
		if (!this.driverSeatInputs.Contains(playerSeatInfo.mountable))
		{
			this.driverSeatInputs.Add(playerSeatInfo.mountable, new DriverSeatInputs());
		}
		DriverSeatInputs driverSeatInputs = this.driverSeatInputs[playerSeatInfo.mountable];
		if (inputState.IsDown(BUTTON.DUCK))
		{
			driverSeatInputs.steerInput += inputState.MouseDelta().x * 0.1f;
		}
		else
		{
			driverSeatInputs.steerInput = 0f;
			if (inputState.IsDown(BUTTON.LEFT))
			{
				driverSeatInputs.steerInput = -1f;
			}
			else if (inputState.IsDown(BUTTON.RIGHT))
			{
				driverSeatInputs.steerInput = 1f;
			}
		}
		driverSeatInputs.steerMod = inputState.IsDown(BUTTON.SPRINT);
		float num = 0f;
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			num = 1f;
		}
		else if (inputState.IsDown(BUTTON.BACKWARD))
		{
			num = -1f;
		}
		driverSeatInputs.throttleInput = 0f;
		driverSeatInputs.brakeInput = 0f;
		if (GetSpeed() > 3f && num < -0.1f)
		{
			driverSeatInputs.throttleInput = 0f;
			driverSeatInputs.brakeInput = 0f - num;
		}
		else
		{
			driverSeatInputs.throttleInput = num;
			driverSeatInputs.brakeInput = 0f;
		}
		for (int i = 0; i < base.NumAttachedModules; i++)
		{
			base.AttachedModuleEntities[i].PlayerServerInput(inputState, player);
		}
		if (engineController.IsOff && ((inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD)) || (inputState.IsDown(BUTTON.BACKWARD) && !inputState.WasDown(BUTTON.BACKWARD))))
		{
			engineController.TryStartEngine(player);
		}
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		DriverSeatInputs val;
		if (driverSeatInputs.TryGetValue(seat, out val))
		{
			driverSeatInputs.Remove(seat);
		}
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			if (attachedModuleEntity != null)
			{
				attachedModuleEntity.OnPlayerDismountedVehicle(player);
			}
		}
		carLock.EnableCentralLockingIfNoDriver();
	}

	public override void SpawnSubEntities()
	{
		base.SpawnSubEntities();
		if (!Rust.Application.isLoadingSave)
		{
			fuelSystem.SpawnFuelStorage(fuelStoragePrefab, fuelStoragePoint);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.modularCar = Facepunch.Pool.Get<ProtoBuf.ModularCar>();
		info.msg.modularCar.steerAngle = SteerAngle;
		info.msg.modularCar.driveWheelVel = DriveWheelVelocity;
		info.msg.modularCar.throttleInput = GetThrottleInput();
		info.msg.modularCar.brakeInput = GetBrakeInput();
		info.msg.modularCar.fuelStorageID = fuelSystem.fuelStorageInstance.uid;
		info.msg.modularCar.fuelFraction = GetFuelFraction();
		info.msg.modularCar.lockID = carLock.LockID;
	}

	public override void Hurt(HitInfo info)
	{
		if (!IsDead() && info.damageTypes.Get(DamageType.Decay) == 0f)
		{
			PropagateDamageToModules(info, 0.5f / (float)base.NumAttachedModules, 0.9f / (float)base.NumAttachedModules, null);
		}
		base.Hurt(info);
	}

	public int TryUseFuel(float seconds, float fuelUsedPerSecond)
	{
		return fuelSystem.TryUseFuel(seconds, fuelUsedPerSecond);
	}

	public override bool MountEligable(BasePlayer player)
	{
		if (!base.MountEligable(player))
		{
			return false;
		}
		ModularCarSeat modularCarSeat = GetIdealMountPointFor(player) as ModularCarSeat;
		if (modularCarSeat != null && !modularCarSeat.associatedSeatingModule.DoorsAreLockable)
		{
			return true;
		}
		return PlayerCanUseThis(player, ModularCarLock.LockType.Door);
	}

	public override bool IsComplete()
	{
		if (HasAnyEngines() && HasDriverMountPoints())
		{
			return !IsDead();
		}
		return false;
	}

	public void DoDecayDamage(float damage)
	{
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			if (!attachedModuleEntity.IsDestroyed)
			{
				attachedModuleEntity.Hurt(damage, DamageType.Decay);
			}
		}
		if (!base.HasAnyModules)
		{
			Hurt(damage, DamageType.Decay);
		}
	}

	public float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float num = 0f;
		for (int i = 0; i < base.AttachedModuleEntities.Count; i++)
		{
			num += base.AttachedModuleEntities[i].GetAdjustedDriveForce(absSpeed, topSpeed);
		}
		return RollOffDriveForce(num);
	}

	public bool HasAnyEngines()
	{
		for (int i = 0; i < base.AttachedModuleEntities.Count; i++)
		{
			if (base.AttachedModuleEntities[i].HasAnEngine)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyWorkingEngines()
	{
		return GetMaxDriveForce() > 0f;
	}

	public bool CanRunEngines()
	{
		if (HasAnyWorkingEngines() && HasDriver() && fuelSystem.HasFuel() && !waterlogged)
		{
			return !IsDead();
		}
		return false;
	}

	public void OnEngineStartFailed()
	{
		bool arg = !HasAnyWorkingEngines() || waterlogged;
		ClientRPC(null, "EngineStartFailed", arg);
	}

	public bool AdminFixUp(int tier)
	{
		if (IsDead())
		{
			return false;
		}
		fuelSystem.AdminFillFuel();
		SetHealth(MaxHealth());
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			attachedModuleEntity.AdminFixUp(tier);
		}
		SendNetworkUpdate();
		return true;
	}

	public override void ModuleHurt(BaseVehicleModule hurtModule, HitInfo info)
	{
		if (IsDead())
		{
			if ((float)timeSinceDeath > 1f)
			{
				for (int i = 0; i < info.damageTypes.types.Length; i++)
				{
					deathDamageCounter += info.damageTypes.types[i];
				}
			}
			if (deathDamageCounter > 600f && !base.IsDestroyed)
			{
				Kill(DestroyMode.Gib);
			}
		}
		else if (hurtModule.PropagateDamage && info.damageTypes.Get(DamageType.Decay) == 0f)
		{
			PropagateDamageToModules(info, 0.15f, 0.4f, hurtModule);
		}
	}

	public void PropagateDamageToModules(HitInfo info, float minPropagationPercent, float maxPropagationPercent, BaseVehicleModule ignoreModule)
	{
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			if (attachedModuleEntity == ignoreModule || attachedModuleEntity.Health() <= 0f)
			{
				continue;
			}
			if (IsDead())
			{
				break;
			}
			float num = UnityEngine.Random.Range(minPropagationPercent, maxPropagationPercent);
			for (int i = 0; i < info.damageTypes.types.Length; i++)
			{
				float num2 = info.damageTypes.types[i];
				if (num2 > 0f)
				{
					attachedModuleEntity.AcceptPropagatedDamage(num2 * num, (DamageType)i, info.Initiator, info.UseProtection);
				}
				if (IsDead())
				{
					break;
				}
			}
		}
	}

	public override void ModuleReachedZeroHealth()
	{
		if (IsDead())
		{
			return;
		}
		bool flag = true;
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			if (attachedModuleEntity.health > 0f)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			Die();
		}
	}

	public override void OnKilled(HitInfo info)
	{
		DismountAllPlayers();
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			attachedModuleEntity.repair.enabled = false;
		}
		if (carLock != null)
		{
			carLock.RemoveLock();
		}
		timeSinceDeath = 0f;
		if (vehicle.carwrecks)
		{
			if (!base.HasAnyModules)
			{
				Kill(DestroyMode.Gib);
			}
			else
			{
				SendNetworkUpdate();
			}
		}
		else
		{
			Kill(DestroyMode.Gib);
		}
	}

	public void RemoveLock()
	{
		carLock.RemoveLock();
	}

	public void RestoreVelocity(Vector3 vel)
	{
		if (rigidBody.velocity.sqrMagnitude < vel.sqrMagnitude)
		{
			vel.y = rigidBody.velocity.y;
			rigidBody.velocity = vel;
		}
	}

	public override Vector3 GetCOMMultiplier()
	{
		if (carPhysics == null || !carPhysics.IsGrounded() || !IsOn())
		{
			return airbourneCOMMultiplier;
		}
		return groundedCOMMultiplier;
	}

	public void UpdateClients()
	{
		if (HasDriver())
		{
			byte num = (byte)((GetThrottleInput() + 1f) * 7f);
			byte b = (byte)(GetBrakeInput() * 15f);
			byte arg = (byte)(num + (b << 4));
			byte arg2 = (byte)(GetFuelFraction() * 255f);
			ClientRPC(null, "ModularCarUpdate", SteerAngle, arg, DriveWheelVelocity, arg2);
		}
	}

	public void DecayTick()
	{
		if (base.IsDestroyed || IsOn() || immuneToDecay || UnityEngine.Time.time < lastEngineOnTime + 600f)
		{
			return;
		}
		float num = 1f;
		if (IsDead())
		{
			int num2 = Mathf.Max(1, base.AttachedModuleEntities.Count);
			num /= 5f * (float)num2;
			DoDecayDamage(600f * num);
			return;
		}
		num /= outsidedecayminutes;
		if (!IsOutside())
		{
			num *= 0.1f;
		}
		float num3 = (base.HasAnyModules ? base.AttachedModuleEntities.Max((BaseVehicleModule module) => module.MaxHealth()) : MaxHealth());
		DoDecayDamage(num3 * num);
	}

	public void ProcessCollision(Collision collision)
	{
		if (base.isClient || collision == null || collision.gameObject == null || collision.gameObject == null)
		{
			return;
		}
		ContactPoint contact = collision.GetContact(0);
		BaseEntity baseEntity = null;
		if (contact.otherCollider.attachedRigidbody == rigidBody)
		{
			baseEntity = GameObjectEx.ToBaseEntity(contact.otherCollider);
		}
		else if (contact.thisCollider.attachedRigidbody == rigidBody)
		{
			baseEntity = GameObjectEx.ToBaseEntity(contact.thisCollider);
		}
		if (baseEntity != null)
		{
			float forceMagnitude = collision.impulse.magnitude / UnityEngine.Time.fixedDeltaTime;
			if (QueueCollisionDamage(baseEntity, forceMagnitude) > 0f)
			{
				ShowCollisionFX(collision);
			}
		}
	}

	public float QueueCollisionDamage(BaseEntity hitEntity, float forceMagnitude)
	{
		float num = Mathf.InverseLerp(20000f, 2500000f, forceMagnitude);
		if (num > 0f)
		{
			float num2 = Mathf.Lerp(1f, 200f, num);
			float value;
			if (damageSinceLastTick.TryGetValue(hitEntity, out value))
			{
				if (value < num2)
				{
					damageSinceLastTick[hitEntity] = num2;
				}
			}
			else
			{
				damageSinceLastTick[hitEntity] = num2;
			}
		}
		return num;
	}

	public void DoCollisionDamage(BaseEntity hitEntity, float damage)
	{
		BaseVehicleModule baseVehicleModule;
		if ((object)(baseVehicleModule = hitEntity as BaseVehicleModule) != null)
		{
			baseVehicleModule.Hurt(damage, DamageType.Collision, this, false);
		}
		else
		{
			if (!(hitEntity == this))
			{
				return;
			}
			if (!base.HasAnyModules)
			{
				Hurt(damage, DamageType.Collision, this, false);
				return;
			}
			float amount = damage / (float)base.NumAttachedModules;
			foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
			{
				attachedModuleEntity.AcceptPropagatedDamage(amount, DamageType.Collision, this, false);
			}
		}
	}

	public void ShowCollisionFX(Collision collision)
	{
		if (!(UnityEngine.Time.time < nextCollisionFXTime))
		{
			nextCollisionFXTime = UnityEngine.Time.time + 0.25f;
			if (collisionEffect.isValid)
			{
				Vector3 point = collision.GetContact(0).point;
				point += (base.transform.position - point) * 0.25f;
				Effect.server.Run(collisionEffect.resourcePath, point, base.transform.up);
			}
		}
	}

	public void SpawnPreassignedModules()
	{
		if (!spawnSettings.useSpawnSettings || CollectionEx.IsNullOrEmpty(spawnSettings.configurationOptions))
		{
			return;
		}
		ModularCarPresetConfig modularCarPresetConfig = spawnSettings.configurationOptions[UnityEngine.Random.Range(0, spawnSettings.configurationOptions.Length)];
		if (Interface.CallHook("OnVehicleModulesAssign", this, modularCarPresetConfig.socketItemDefs) != null)
		{
			return;
		}
		for (int i = 0; i < modularCarPresetConfig.socketItemDefs.Length; i++)
		{
			ItemModVehicleModule itemModVehicleModule = modularCarPresetConfig.socketItemDefs[i];
			if (itemModVehicleModule != null && base.Inventory.SocketsAreFree(i, itemModVehicleModule.socketsTaken))
			{
				itemModVehicleModule.doNonUserSpawn = true;
				Item item = ItemManager.Create(itemModVehicleModule.GetComponent<ItemDefinition>(), 1, 0uL);
				float num = UnityEngine.Random.Range(spawnSettings.minStartHealthPercent, spawnSettings.maxStartHealthPercent);
				item.condition = item.maxCondition * num;
				if (!TryAddModule(item))
				{
					item.Remove();
				}
			}
		}
		Interface.CallHook("OnVehicleModulesAssigned", this, modularCarPresetConfig.socketItemDefs);
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanBeLooted(player))
		{
			fuelSystem.LootFuel(player);
		}
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			VehicleModuleSeating vehicleModuleSeating;
			if (attachedModuleEntity.HasSeating && (object)(vehicleModuleSeating = attachedModuleEntity as VehicleModuleSeating) != null && vehicleModuleSeating.IsOnThisModule(player))
			{
				attachedModuleEntity.ScaleDamageForPlayer(player, info);
			}
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(process, rootObj, name, serverside, clientside, bundling);
		damageShowingRenderers = GetComponentsInChildren<MeshRenderer>();
	}

	public override void InitShared()
	{
		base.InitShared();
		engineController = new VehicleEngineController(this, base.isServer, carSettings.engineStartupTime);
		fuelSystem = new EntityFuelSystem(this, base.isServer);
		carLock = new ModularCarLock(this, base.isServer);
	}

	public override float MaxHealth()
	{
		return AssociatedItemDef.condition.max;
	}

	public override float StartHealth()
	{
		return AssociatedItemDef.condition.max;
	}

	public float TotalHealth()
	{
		float num = 0f;
		for (int i = 0; i < base.AttachedModuleEntities.Count; i++)
		{
			num += base.AttachedModuleEntities[i].Health();
		}
		return num;
	}

	public float TotalMaxHealth()
	{
		float num = 0f;
		for (int i = 0; i < base.AttachedModuleEntities.Count; i++)
		{
			num += base.AttachedModuleEntities[i].MaxHealth();
		}
		return num;
	}

	public override float GetMaxForwardSpeed()
	{
		float num = GetMaxDriveForce() / base.TotalMass * 30f;
		return Mathf.Pow(0.9945f, num) * num;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.modularCar != null)
		{
			carLock.LockID = info.msg.modularCar.lockID;
			fuelSystem.fuelStorageInstance.uid = info.msg.modularCar.fuelStorageID;
			cachedFuelFraction = info.msg.modularCar.fuelFraction;
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (old != next)
		{
			RefreshEngineState();
		}
	}

	public float GetThrottleInput()
	{
		if (base.isServer)
		{
			float num = 0f;
			BufferList<DriverSeatInputs> values = driverSeatInputs.Values;
			for (int i = 0; i < values.Count; i++)
			{
				num += values[i].throttleInput;
			}
			return Mathf.Clamp(num, -1f, 1f);
		}
		return 0f;
	}

	public float GetBrakeInput()
	{
		if (base.isServer)
		{
			float num = 0f;
			BufferList<DriverSeatInputs> values = driverSeatInputs.Values;
			for (int i = 0; i < values.Count; i++)
			{
				num += values[i].brakeInput;
			}
			return Mathf.Clamp01(num);
		}
		return 0f;
	}

	public float GetMaxDriveForce()
	{
		float num = 0f;
		for (int i = 0; i < base.AttachedModuleEntities.Count; i++)
		{
			num += base.AttachedModuleEntities[i].GetMaxDriveForce();
		}
		return RollOffDriveForce(num);
	}

	public float GetFuelFraction()
	{
		if (base.isServer)
		{
			return fuelSystem.GetFuelFraction();
		}
		return cachedFuelFraction;
	}

	public bool PlayerHasUnlockPermission(BasePlayer player)
	{
		return carLock.PlayerHasUnlockPermission(player);
	}

	public override bool PlayerCanUseThis(BasePlayer player, ModularCarLock.LockType lockType)
	{
		return carLock.PlayerCanUseThis(player, lockType);
	}

	public bool PlayerCanDestroyLock(BasePlayer player, BaseVehicleModule viaModule)
	{
		return carLock.PlayerCanDestroyLock(viaModule);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (!PlayerCanUseThis(player, ModularCarLock.LockType.General))
		{
			return false;
		}
		return !IsOn();
	}

	public override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (pusher.isMounted || pusher.IsSwimming())
		{
			return false;
		}
		if (pusher.InSafeZone() && !carLock.PlayerHasUnlockPermission(pusher))
		{
			return false;
		}
		return !pusher.IsStandingOnEntity(this, 8192);
	}

	public bool RefreshEngineState()
	{
		if (lastSetEngineState == CurEngineState)
		{
			return false;
		}
		if (base.isServer && CurEngineState == VehicleEngineController.EngineState.Off)
		{
			lastEngineOnTime = UnityEngine.Time.time;
		}
		foreach (BaseVehicleModule attachedModuleEntity in base.AttachedModuleEntities)
		{
			attachedModuleEntity.OnEngineStateChanged(lastSetEngineState, CurEngineState);
		}
		lastSetEngineState = CurEngineState;
		return true;
	}

	public float RollOffDriveForce(float driveForce)
	{
		return Mathf.Pow(0.9999175f, driveForce) * driveForce;
	}

	public void RefreshChassisProtectionState()
	{
		if (base.HasAnyModules)
		{
			baseProtection = immortalProtection;
			if (base.isServer)
			{
				SetHealth(MaxHealth());
			}
		}
		else
		{
			baseProtection = mortalProtection;
		}
	}

	public override void ModuleEntityAdded(BaseVehicleModule addedModule)
	{
		base.ModuleEntityAdded(addedModule);
		RefreshChassisProtectionState();
	}

	public override void ModuleEntityRemoved(BaseVehicleModule removedModule)
	{
		base.ModuleEntityRemoved(removedModule);
		RefreshChassisProtectionState();
	}

	void IEngineControllerUser.Invoke(Action action, float time)
	{
		Invoke(action, time);
	}

	void IEngineControllerUser.CancelInvoke(Action action)
	{
		CancelInvoke(action);
	}
}
