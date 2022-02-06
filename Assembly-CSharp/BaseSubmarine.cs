#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Sonar;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using VLB;

public class BaseSubmarine : BaseVehicle, IPoolVehicle, IEngineControllerUser, IEntity, IAirSupply
{
	[Serializable]
	public class ParentTriggerInfo
	{
		public TriggerParent trigger;

		public Transform triggerWaterLevel;
	}

	public float targetClimbSpeed;

	private float nextCollisionFXTime;

	public float maxDamageThisTick;

	private float nextCollisionDamageTime;

	private bool prevPrimaryFireInput;

	private bool primaryFireInput;

	private const float DECAY_TICK_TIME = 60f;

	public TimeSince timeSinceLastUsed;

	public TimeSince timeSinceTorpedoFired;

	private TimeSince timeSinceFailRPCSent;

	private float normalDrag;

	private float highDrag;

	[Header("Submarine Main")]
	[SerializeField]
	private Transform centreOfMassTransform;

	[SerializeField]
	public Buoyancy buoyancy;

	[SerializeField]
	public float maxRudderAngle = 35f;

	[SerializeField]
	private Transform rudderVisualTransform;

	[SerializeField]
	private Transform rudderDetailedColliderTransform;

	[SerializeField]
	private Transform propellerTransform;

	[SerializeField]
	public float timeUntilAutoSurface = 300f;

	[SerializeField]
	private Renderer[] interiorRenderers;

	[SerializeField]
	private SonarObject sonarObject;

	[SerializeField]
	private ParentTriggerInfo[] parentTriggers;

	[SerializeField]
	private GameObjectRef fuelStoragePrefab;

	[Header("Submarine Engine & Fuel")]
	[SerializeField]
	public float engineKW = 200f;

	[SerializeField]
	public float turnPower = 0.25f;

	[SerializeField]
	public float engineStartupTime = 0.5f;

	[SerializeField]
	private GameObjectRef itemStoragePrefab;

	[SerializeField]
	public float depthChangeTargetSpeed = 1f;

	[SerializeField]
	public float idleFuelPerSec = 0.03f;

	[SerializeField]
	public float maxFuelPerSec = 0.15f;

	[FormerlySerializedAs("internalAccessFuelTank")]
	[SerializeField]
	private bool internalAccessStorage;

	[Header("Submarine Weaponry")]
	[SerializeField]
	public GameObjectRef torpedoStoragePrefab;

	[SerializeField]
	public Transform torpedoFiringPoint;

	[SerializeField]
	public float maxFireRate = 1.5f;

	[Header("Submarine Audio & FX")]
	[SerializeField]
	protected SubmarineAudio submarineAudio;

	[SerializeField]
	private ParticleSystem fxTorpedoFire;

	[SerializeField]
	private GameObject internalFXContainer;

	[SerializeField]
	private GameObject internalOnFXContainer;

	[SerializeField]
	private ParticleSystem fxIntAmbientBubbleLoop;

	[SerializeField]
	private ParticleSystem fxIntInitialDiveBubbles;

	[SerializeField]
	private ParticleSystem fxIntWaterDropSpray;

	[SerializeField]
	private ParticleSystem fxIntWindowFilm;

	[SerializeField]
	private ParticleSystemContainer fxIntMediumDamage;

	[SerializeField]
	private ParticleSystemContainer fxIntHeavyDamage;

	[SerializeField]
	private GameObject externalFXContainer;

	[SerializeField]
	private GameObject externalOnFXContainer;

	[SerializeField]
	private ParticleSystem fxExtAmbientBubbleLoop;

	[SerializeField]
	private ParticleSystem fxExtInitialDiveBubbles;

	[SerializeField]
	private ParticleSystem fxExtAboveWaterEngineThrustForward;

	[SerializeField]
	private ParticleSystem fxExtAboveWaterEngineThrustReverse;

	[SerializeField]
	private ParticleSystem fxExtUnderWaterEngineThrustForward;

	[SerializeField]
	private ParticleSystem[] fxExtUnderWaterEngineThrustForwardSubs;

	[SerializeField]
	private ParticleSystem fxExtUnderWaterEngineThrustReverse;

	[SerializeField]
	private ParticleSystem[] fxExtUnderWaterEngineThrustReverseSubs;

	[SerializeField]
	private ParticleSystem fxExtBowWave;

	[SerializeField]
	private ParticleSystem fxExtWakeEffect;

	[SerializeField]
	public GameObjectRef aboveWatercollisionEffect;

	[SerializeField]
	public GameObjectRef underWatercollisionEffect;

	[SerializeField]
	private VolumetricLightBeam spotlightVolumetrics;

	[SerializeField]
	private float mountedAlphaInside = 0.04f;

	[SerializeField]
	private float mountedAlphaOutside = 0.015f;

	[ServerVar(Help = "How long before a submarine loses all its health while outside. If it's in deep water, deepwaterdecayminutes is used")]
	public static float outsidedecayminutes = 180f;

	[ServerVar(Help = "How long before a submarine loses all its health while in deep water")]
	public static float deepwaterdecayminutes = 120f;

	[ServerVar(Help = "How long a submarine can stay underwater until players start taking damage from low oxygen")]
	public static float oxygenminutes = 10f;

	public const Flags Flag_Ammo = Flags.Reserved6;

	private float _throttle;

	private float _rudder;

	private float _upDown;

	private float _oxygen = 1f;

	public VehicleEngineController<BaseSubmarine> engineController;

	public float cachedFuelAmount;

	protected Vector3 steerAngle;

	public float waterSurfaceY;

	public float curSubDepthY;

	public EntityRef<StorageContainer> torpedoStorageInstance;

	private EntityRef<StorageContainer> itemStorageInstance;

	public int waterLayerMask;

	public ItemModGiveOxygen.AirSupplyType AirType => ItemModGiveOxygen.AirSupplyType.Submarine;

	public VehicleEngineController<BaseSubmarine>.EngineState EngineState => engineController.CurEngineState;

	public Vector3 Velocity { get; private set; }

	public bool LightsAreOn => HasFlag(Flags.Reserved5);

	public bool HasAmmo => HasFlag(Flags.Reserved6);

	public float ThrottleInput
	{
		get
		{
			if (!engineController.IsOn)
			{
				return 0f;
			}
			return _throttle;
		}
		protected set
		{
			_throttle = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public float RudderInput
	{
		get
		{
			return _rudder;
		}
		protected set
		{
			_rudder = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public float UpDownInput
	{
		get
		{
			if (base.isServer)
			{
				if ((float)timeSinceLastUsed >= timeUntilAutoSurface)
				{
					return 0.15f;
				}
				if (!engineController.IsOn)
				{
					return Mathf.Max(0f, _upDown);
				}
				return _upDown;
			}
			return _upDown;
		}
		protected set
		{
			_upDown = Mathf.Clamp(value, -1f, 1f);
		}
	}

	public float Oxygen
	{
		get
		{
			return _oxygen;
		}
		protected set
		{
			_oxygen = Mathf.Clamp(value, 0f, 1f);
		}
	}

	protected float PhysicalRudderAngle
	{
		get
		{
			float num = rudderDetailedColliderTransform.localEulerAngles.y;
			if (num > 180f)
			{
				num -= 360f;
			}
			return num;
		}
	}

	protected bool IsInWater => curSubDepthY > 0.2f;

	protected bool IsSurfaced => curSubDepthY < 1.1f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseSubmarine.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
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
							rPCMessage = default(RPCMessage);
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
			if (rpc == 924237371 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenItemStorage "));
				}
				using (TimeWarning.New("RPC_OpenItemStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(924237371u, "RPC_OpenItemStorage", this, player, 3f))
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
							RPC_OpenItemStorage(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_OpenItemStorage");
					}
				}
				return true;
			}
			if (rpc == 2181221870u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenTorpedoStorage "));
				}
				using (TimeWarning.New("RPC_OpenTorpedoStorage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2181221870u, "RPC_OpenTorpedoStorage", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							RPC_OpenTorpedoStorage(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_OpenTorpedoStorage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		timeSinceLastUsed = timeUntilAutoSurface;
		buoyancy.buoyancyScale = 1f;
		normalDrag = rigidBody.drag;
		highDrag = normalDrag * 2.5f;
		Oxygen = 1f;
		InvokeRandomized(UpdateClients, 0f, 0.15f, 0.02f);
		InvokeRandomized(SubmarineDecay, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer)
		{
			if (isSpawned)
			{
				GetFuelSystem().CheckNewChild(child);
			}
			if (child.prefabID == itemStoragePrefab.GetEntity().prefabID)
			{
				itemStorageInstance.Set((StorageContainer)child);
			}
			if (child.prefabID == torpedoStoragePrefab.GetEntity().prefabID)
			{
				torpedoStorageInstance.Set((StorageContainer)child);
			}
		}
	}

	private void ServerFlagsChanged(Flags old, Flags next)
	{
		if (next.HasFlag(Flags.On) && !old.HasFlag(Flags.On))
		{
			SetFlag(Flags.Reserved5, true);
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			StorageContainer storageContainer = itemStorageInstance.Get(base.isServer);
			if (storageContainer != null && BaseEntityEx.IsValid(storageContainer))
			{
				storageContainer.DropItems();
			}
		}
		base.DoServerDestroy();
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (!base.isClient)
		{
			ProcessCollision(collision);
		}
	}

	public override float MaxVelocity()
	{
		return 10f;
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return engineController.FuelSystem;
	}

	public override int StartingFuelUnits()
	{
		return 50;
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (CanMount(player) && MountEligable(player))
		{
			BaseMountable baseMountable = (HasDriver() ? GetIdealMountPointFor(player) : mountPoints[0].mountable);
			if (baseMountable != null)
			{
				baseMountable.AttemptMount(player, doMountChecks);
			}
			if (PlayerIsMounted(player))
			{
				PlayerMounted(player, baseMountable);
			}
		}
	}

	public void OnPoolDestroyed()
	{
		Kill(DestroyMode.Gib);
	}

	public void WakeUp()
	{
		if (rigidBody != null)
		{
			rigidBody.WakeUp();
			rigidBody.AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
		}
		if (buoyancy != null)
		{
			buoyancy.Wake();
		}
	}

	public override void OnKilled(HitInfo info)
	{
		DamageType majorityDamageType = info.damageTypes.GetMajorityDamageType();
		if (majorityDamageType == DamageType.Explosion || majorityDamageType == DamageType.AntiVehicle)
		{
			foreach (MountPointInfo mountPoint in mountPoints)
			{
				if (mountPoint.mountable != null)
				{
					BasePlayer mounted = mountPoint.mountable.GetMounted();
					if (mounted != null)
					{
						mounted.Hurt(10000f, DamageType.Explosion, this, false);
					}
				}
			}
		}
		base.OnKilled(info);
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (!base.IsMovingOrOn)
		{
			Velocity = Vector3.zero;
			targetClimbSpeed = 0f;
			buoyancy.ArtificialHeight = null;
			return;
		}
		Velocity = GetLocalVelocity();
		UpdateWaterInfo();
		buoyancy.ArtificialHeight = waterSurfaceY;
		rigidBody.drag = (HasDriver() ? normalDrag : highDrag);
		float num = 2f;
		if (IsSurfaced)
		{
			float num2 = 20f * num;
			if (Oxygen < 0.5f)
			{
				Oxygen = 0.5f;
			}
			else
			{
				Oxygen += UnityEngine.Time.deltaTime / num2;
			}
		}
		else if (AnyMounted())
		{
			float num3 = oxygenminutes * 60f * num;
			Oxygen -= UnityEngine.Time.deltaTime / num3;
		}
		engineController.CheckEngineState();
		if (engineController.IsOn)
		{
			float fuelPerSecond = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, Mathf.Abs(ThrottleInput));
			engineController.TickFuel(fuelPerSecond);
		}
		if (IsInWater)
		{
			float num4 = depthChangeTargetSpeed * UpDownInput;
			targetClimbSpeed = Mathf.MoveTowards(maxDelta: (((!(UpDownInput > 0f) || !(num4 > targetClimbSpeed) || !(targetClimbSpeed > 0f)) && (!(UpDownInput < 0f) || !(num4 < targetClimbSpeed) || !(targetClimbSpeed < 0f))) ? 4f : 0.7f) * UnityEngine.Time.fixedDeltaTime, current: targetClimbSpeed, target: num4);
			float num5 = rigidBody.velocity.y - targetClimbSpeed;
			float value = buoyancy.buoyancyScale - num5 * 50f * UnityEngine.Time.fixedDeltaTime;
			buoyancy.buoyancyScale = Mathf.Clamp(value, 0.01f, 1f);
			Vector3 torque = Vector3.Cross(Quaternion.AngleAxis(rigidBody.angularVelocity.magnitude * 57.29578f * 10f / 200f, rigidBody.angularVelocity) * base.transform.up, Vector3.up) * 200f * 200f;
			rigidBody.AddTorque(torque);
			float num6 = 0.1f;
			rigidBody.AddForce(Vector3.up * (0f - num5) * num6, ForceMode.VelocityChange);
		}
		else
		{
			float b = 0f;
			buoyancy.buoyancyScale = Mathf.Lerp(buoyancy.buoyancyScale, b, UnityEngine.Time.fixedDeltaTime);
		}
		if (IsOn() && IsInWater)
		{
			rigidBody.AddForce(base.transform.forward * engineKW * 40f * ThrottleInput, ForceMode.Force);
			float num7 = turnPower * rigidBody.mass * rigidBody.angularDrag;
			float speed = GetSpeed();
			float num8 = Mathf.Min(Mathf.Abs(speed) * 0.6f, 6f) + 4f;
			float num9 = num7 * RudderInput * num8;
			if (speed < -1f)
			{
				num9 *= -1f;
			}
			rigidBody.AddTorque(base.transform.up * num9, ForceMode.Force);
		}
		UpdatePhysicalRudder(RudderInput, UnityEngine.Time.fixedDeltaTime);
		if (UnityEngine.Time.time >= nextCollisionDamageTime && maxDamageThisTick > 0f)
		{
			nextCollisionDamageTime = UnityEngine.Time.time + 0.33f;
			Hurt(maxDamageThisTick, DamageType.Collision, this, false);
			maxDamageThisTick = 0f;
		}
		StorageContainer torpedoContainer = GetTorpedoContainer();
		if (torpedoContainer != null)
		{
			bool b2 = torpedoContainer.inventory.HasAmmo(AmmoTypes.TORPEDO);
			SetFlag(Flags.Reserved6, b2);
		}
		BasePlayer driver = GetDriver();
		if (driver != null && primaryFireInput)
		{
			bool flag = true;
			if (IsInWater && (float)timeSinceTorpedoFired >= maxFireRate)
			{
				float minSpeed = GetSpeed() + 2f;
				ServerProjectile projectile;
				if (BaseMountable.TryFireProjectile(torpedoContainer, AmmoTypes.TORPEDO, torpedoFiringPoint.position, torpedoFiringPoint.forward, driver, 1f, minSpeed, out projectile))
				{
					timeSinceTorpedoFired = 0f;
					flag = false;
					driver.MarkHostileFor();
					ClientRPC(null, "TorpedoFired");
				}
			}
			if (!prevPrimaryFireInput && flag && (float)timeSinceFailRPCSent > 0.5f)
			{
				timeSinceFailRPCSent = 0f;
				ClientRPCPlayer(null, driver, "TorpedoFireFailed");
			}
		}
		else if (driver == null)
		{
			primaryFireInput = false;
		}
		prevPrimaryFireInput = primaryFireInput;
		if ((float)timeSinceLastUsed > 300f && LightsAreOn)
		{
			SetFlag(Flags.Reserved5, false);
		}
		for (int i = 0; i < parentTriggers.Length; i++)
		{
			float num10 = parentTriggers[i].triggerWaterLevel.position.y - base.transform.position.y;
			bool flag2 = curSubDepthY - num10 <= 0f;
			if (flag2 != parentTriggers[i].trigger.enabled)
			{
				parentTriggers[i].trigger.enabled = flag2;
			}
		}
	}

	public override void LightToggle(BasePlayer player)
	{
		if (IsDriver(player))
		{
			SetFlag(Flags.Reserved5, !LightsAreOn);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		timeSinceLastUsed = 0f;
		if (IsDriver(player))
		{
			if (inputState.IsDown(BUTTON.SPRINT))
			{
				UpDownInput = 1f;
			}
			else if (inputState.IsDown(BUTTON.DUCK))
			{
				UpDownInput = -1f;
			}
			else
			{
				UpDownInput = 0f;
			}
			if (inputState.IsDown(BUTTON.FORWARD))
			{
				ThrottleInput = 1f;
			}
			else if (inputState.IsDown(BUTTON.BACKWARD))
			{
				ThrottleInput = -1f;
			}
			else
			{
				ThrottleInput = 0f;
			}
			if (inputState.IsDown(BUTTON.LEFT))
			{
				RudderInput = -1f;
			}
			else if (inputState.IsDown(BUTTON.RIGHT))
			{
				RudderInput = 1f;
			}
			else
			{
				RudderInput = 0f;
			}
			primaryFireInput = inputState.IsDown(BUTTON.FIRE_PRIMARY);
			if (engineController.IsOff && ((inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD)) || (inputState.IsDown(BUTTON.BACKWARD) && !inputState.WasDown(BUTTON.BACKWARD)) || (inputState.IsDown(BUTTON.SPRINT) && !inputState.WasDown(BUTTON.SPRINT)) || (inputState.IsDown(BUTTON.DUCK) && !inputState.WasDown(BUTTON.DUCK))))
			{
				engineController.TryStartEngine(player);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.submarine = Facepunch.Pool.Get<Submarine>();
		info.msg.submarine.throttle = ThrottleInput;
		info.msg.submarine.upDown = UpDownInput;
		info.msg.submarine.rudder = RudderInput;
		info.msg.submarine.fuelStorageID = GetFuelSystem().fuelStorageInstance.uid;
		info.msg.submarine.fuelAmount = GetFuelAmount();
		info.msg.submarine.torpedoStorageID = torpedoStorageInstance.uid;
		info.msg.submarine.oxygen = Oxygen;
		info.msg.submarine.itemStorageID = itemStorageInstance.uid;
	}

	public bool MeetsEngineRequirements()
	{
		return AnyMounted();
	}

	public void OnEngineStartFailed()
	{
		ClientRPC(null, "EngineStartFailed");
	}

	public StorageContainer GetTorpedoContainer()
	{
		BaseEntity baseEntity = torpedoStorageInstance.Get(base.isServer);
		if (baseEntity != null && BaseEntityEx.IsValid(baseEntity))
		{
			return baseEntity as StorageContainer;
		}
		return null;
	}

	public StorageContainer GetItemContainer()
	{
		BaseEntity baseEntity = itemStorageInstance.Get(base.isServer);
		if (baseEntity != null && BaseEntityEx.IsValid(baseEntity))
		{
			return baseEntity as StorageContainer;
		}
		return null;
	}

	private void ProcessCollision(Collision collision)
	{
		if (!base.isClient && collision != null && !(collision.gameObject == null) && !(collision.gameObject == null))
		{
			float value = collision.impulse.magnitude / UnityEngine.Time.fixedDeltaTime;
			float num = Mathf.InverseLerp(100000f, 2500000f, value);
			if (num > 0f)
			{
				float b = Mathf.Lerp(1f, 200f, num);
				maxDamageThisTick = Mathf.Max(maxDamageThisTick, b);
			}
			if (num > 0f)
			{
				ShowCollisionFX(collision);
			}
		}
	}

	public void UpdateClients()
	{
		if (HasDriver())
		{
			byte num = (byte)((ThrottleInput + 1f) * 7f);
			byte b = (byte)((UpDownInput + 1f) * 7f);
			byte arg = (byte)(num + (b << 4));
			int arg2 = Mathf.CeilToInt(GetFuelAmount());
			ClientRPC(null, "SubmarineUpdate", RudderInput, arg, arg2, Oxygen);
		}
	}

	private void ShowCollisionFX(Collision collision)
	{
		if (!(UnityEngine.Time.time < nextCollisionFXTime))
		{
			nextCollisionFXTime = UnityEngine.Time.time + 0.25f;
			GameObjectRef gameObjectRef = ((curSubDepthY > 2f) ? underWatercollisionEffect : aboveWatercollisionEffect);
			if (gameObjectRef.isValid)
			{
				Vector3 point = collision.GetContact(0).point;
				point += (base.transform.position - point) * 0.25f;
				Effect.server.Run(gameObjectRef.resourcePath, point, base.transform.up);
			}
		}
	}

	public void SubmarineDecay()
	{
		BaseBoat.WaterVehicleDecay(this, 60f, timeSinceLastUsed, outsidedecayminutes, deepwaterdecayminutes);
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player))
		{
			GetFuelSystem().LootFuel(player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenTorpedoStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player) && PlayerIsMounted(player))
		{
			StorageContainer torpedoContainer = GetTorpedoContainer();
			if (torpedoContainer != null)
			{
				torpedoContainer.PlayerOpenLoot(player);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_OpenItemStorage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player))
		{
			StorageContainer itemContainer = GetItemContainer();
			if (itemContainer != null)
			{
				itemContainer.PlayerOpenLoot(player);
			}
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		waterLayerMask = LayerMask.GetMask("Water");
		engineController = new VehicleEngineController<BaseSubmarine>(this, base.isServer, engineStartupTime, fuelStoragePrefab);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.submarine != null)
		{
			ThrottleInput = info.msg.submarine.throttle;
			UpDownInput = info.msg.submarine.upDown;
			RudderInput = info.msg.submarine.rudder;
			engineController.FuelSystem.fuelStorageInstance.uid = info.msg.submarine.fuelStorageID;
			cachedFuelAmount = info.msg.submarine.fuelAmount;
			torpedoStorageInstance.uid = info.msg.submarine.torpedoStorageID;
			Oxygen = info.msg.submarine.oxygen;
			itemStorageInstance.uid = info.msg.submarine.itemStorageID;
			UpdatePhysicalRudder(RudderInput, 0f);
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (old != next && base.isServer)
		{
			ServerFlagsChanged(old, next);
		}
	}

	public override float WaterFactorForPlayer(BasePlayer player)
	{
		return 0f;
	}

	public override float AirFactor()
	{
		return Oxygen;
	}

	public override bool BlocksWaterFor(BasePlayer player)
	{
		return true;
	}

	public float GetFuelAmount()
	{
		if (base.isServer)
		{
			return engineController.FuelSystem.GetFuelAmount();
		}
		return cachedFuelAmount;
	}

	public float GetSpeed()
	{
		if (IsStationary())
		{
			return 0f;
		}
		return Vector3.Dot(Velocity, base.transform.forward);
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (PlayerIsMounted(player))
		{
			return true;
		}
		if (internalAccessStorage)
		{
			return false;
		}
		return !IsOn();
	}

	public float GetAirTimeRemaining()
	{
		if (Oxygen <= 0.5f)
		{
			return 0f;
		}
		return Mathf.InverseLerp(0.5f, 1f, Oxygen) * oxygenminutes * 60f;
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (pusher.isMounted || pusher.IsSwimming() || !pusher.IsOnGround())
		{
			return false;
		}
		return !pusher.IsStandingOnEntity(this, 8192);
	}

	private void UpdatePhysicalRudder(float turnInput, float deltaTime)
	{
		float num = (0f - turnInput) * maxRudderAngle;
		float y = ((!base.IsMovingOrOn) ? num : Mathf.MoveTowards(PhysicalRudderAngle, num, 200f * deltaTime));
		Quaternion localRotation = Quaternion.Euler(0f, y, 0f);
		if (base.isClient)
		{
			rudderVisualTransform.localRotation = localRotation;
		}
		rudderDetailedColliderTransform.localRotation = localRotation;
	}

	private bool CanMount(BasePlayer player)
	{
		return !player.IsDead();
	}

	private void UpdateWaterInfo()
	{
		waterSurfaceY = GetWaterSurfaceY();
		curSubDepthY = waterSurfaceY - base.transform.position.y;
	}

	private float GetWaterSurfaceY()
	{
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(base.transform.position - Vector3.up * 1.5f, Vector3.up, out hitInfo, 5f, waterLayerMask, QueryTriggerInteraction.Collide))
		{
			return hitInfo.point.y;
		}
		WaterLevel.WaterInfo waterInfo = WaterLevel.GetWaterInfo(base.transform.position, true, this);
		if (!waterInfo.isValid)
		{
			return base.transform.position.y - 1f;
		}
		return waterInfo.surfaceLevel;
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
