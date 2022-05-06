#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Snowmobile : GroundVehicle, CarPhysics<Snowmobile>.ICar, TriggerHurtNotChild.IHurtTriggerUser, VehicleChassisVisuals<Snowmobile>.IClientWheelUser, IPrefabPreProcess
{
	public CarPhysics<Snowmobile> carPhysics;

	public VehicleTerrainHandler serverTerrainHandler;

	private CarWheel[] wheels;

	public TimeSince timeSinceLastUsed;

	private const float DECAY_TICK_TIME = 60f;

	public float prevTerrainModDrag;

	public TimeSince timeSinceTerrainModCheck;

	[Header("Snowmobile")]
	[SerializeField]
	private Transform centreOfMassTransform;

	[SerializeField]
	private GameObjectRef itemStoragePrefab;

	[SerializeField]
	private VisualCarWheel wheelSkiFL;

	[SerializeField]
	private VisualCarWheel wheelSkiFR;

	[SerializeField]
	private VisualCarWheel wheelTreadFL;

	[SerializeField]
	private VisualCarWheel wheelTreadFR;

	[SerializeField]
	private VisualCarWheel wheelTreadRL;

	[SerializeField]
	private VisualCarWheel wheelTreadRR;

	[SerializeField]
	public CarSettings carSettings;

	[SerializeField]
	public int engineKW = 59;

	[SerializeField]
	public float idleFuelPerSec = 0.03f;

	[SerializeField]
	public float maxFuelPerSec = 0.15f;

	[SerializeField]
	public float airControlStability = 10f;

	[SerializeField]
	public float airControlPower = 40f;

	[SerializeField]
	public float badTerrainDrag = 1f;

	[SerializeField]
	public ProtectionProperties riderProtection;

	[SerializeField]
	public float hurtTriggerMinSpeed = 1f;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerFront;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerRear;

	[Header("Snowmobile Visuals")]
	public float minGroundFXSpeed;

	[SerializeField]
	private SnowmobileChassisVisuals chassisVisuals;

	[SerializeField]
	private VehicleLight[] lights;

	[SerializeField]
	private Transform steeringLeftIK;

	[SerializeField]
	private Transform steeringRightIK;

	[SerializeField]
	private Transform leftFootIK;

	[SerializeField]
	private Transform rightFootIK;

	[SerializeField]
	private Transform starterKey;

	[SerializeField]
	private Vector3 engineOffKeyRot;

	[SerializeField]
	private Vector3 engineOnKeyRot;

	[ServerVar(Help = "How long before a snowmobile loses all its health while outside")]
	public static float outsideDecayMinutes = 1440f;

	[ServerVar(Help = "Allow mounting as a passenger when there's no driver")]
	public static bool allowPassengerOnly = false;

	[ServerVar(Help = "If true, snowmobile goes fast on all terrain types")]
	public static bool allTerrain = false;

	private float _throttle;

	private float _brake;

	private float _steer;

	private float _mass = -1f;

	public const Flags Flag_Slowmode = Flags.Reserved8;

	private EntityRef<StorageContainer> itemStorageInstance;

	private float cachedFuelFraction;

	private const float FORCE_MULTIPLIER = 10f;

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

	public float BrakeInput
	{
		get
		{
			return _brake;
		}
		protected set
		{
			_brake = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public bool IsBraking => BrakeInput > 0f;

	public float SteerInput
	{
		get
		{
			return _steer;
		}
		protected set
		{
			_steer = Mathf.Clamp(value, -1f, 1f);
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

	public override float DriveWheelVelocity
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

	public float MaxSteerAngle => carSettings.maxSteerAngle;

	public bool InSlowMode
	{
		get
		{
			return HasFlag(Flags.Reserved8);
		}
		private set
		{
			if (InSlowMode != value)
			{
				SetFlag(Flags.Reserved8, value);
			}
		}
	}

	private float Mass
	{
		get
		{
			if (base.isServer)
			{
				return rigidBody.mass;
			}
			return _mass;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Snowmobile.OnRpcMessage"))
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
							RPCMessage rPCMessage = default(RPCMessage);
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
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		timeSinceLastUsed = 0f;
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		rigidBody.inertiaTensor = new Vector3(450f, 200f, 200f);
		carPhysics = new CarPhysics<Snowmobile>(this, base.transform, rigidBody, carSettings);
		serverTerrainHandler = new VehicleTerrainHandler(this);
		InvokeRandomized(UpdateClients, 0f, 0.15f, 0.02f);
		InvokeRandomized(SnowmobileDecay, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float speed = GetSpeed();
		carPhysics.FixedUpdate(UnityEngine.Time.fixedDeltaTime, speed);
		serverTerrainHandler.FixedUpdate();
		if (IsOn())
		{
			float fuelPerSecond = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, Mathf.Abs(ThrottleInput));
			engineController.TickFuel(fuelPerSecond);
		}
		engineController.CheckEngineState();
		if (!carPhysics.IsGrounded() && UnityEngine.Physics.Raycast(base.transform.position, Vector3.down, out var hitInfo, 10f, 1218511105, QueryTriggerInteraction.Ignore))
		{
			Vector3 normal = hitInfo.normal;
			Vector3 right = base.transform.right;
			right.y = 0f;
			normal = Vector3.ProjectOnPlane(normal, right);
			float num = Vector3.Angle(normal, Vector3.up);
			float angle = rigidBody.angularVelocity.magnitude * 57.29578f * airControlStability / airControlPower;
			if (num <= 45f)
			{
				Vector3 torque = Vector3.Cross(Quaternion.AngleAxis(angle, rigidBody.angularVelocity) * base.transform.up, normal) * airControlPower * airControlPower;
				rigidBody.AddTorque(torque);
			}
		}
		hurtTriggerFront.gameObject.SetActive(speed > hurtTriggerMinSpeed);
		hurtTriggerRear.gameObject.SetActive(speed < 0f - hurtTriggerMinSpeed);
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		if (!IsDriver(player))
		{
			return;
		}
		timeSinceLastUsed = 0f;
		if (inputState.IsDown(BUTTON.DUCK))
		{
			SteerInput += inputState.MouseDelta().x * 0.1f;
		}
		else
		{
			SteerInput = 0f;
			if (inputState.IsDown(BUTTON.LEFT))
			{
				SteerInput = -1f;
			}
			else if (inputState.IsDown(BUTTON.RIGHT))
			{
				SteerInput = 1f;
			}
		}
		float num = 0f;
		if (inputState.IsDown(BUTTON.FORWARD))
		{
			num = 1f;
		}
		else if (inputState.IsDown(BUTTON.BACKWARD))
		{
			num = -1f;
		}
		ThrottleInput = 0f;
		BrakeInput = 0f;
		if (GetSpeed() > 3f && num < -0.1f)
		{
			ThrottleInput = 0f;
			BrakeInput = 0f - num;
		}
		else
		{
			ThrottleInput = num;
			BrakeInput = 0f;
		}
		if (engineController.IsOff && ((inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD)) || (inputState.IsDown(BUTTON.BACKWARD) && !inputState.WasDown(BUTTON.BACKWARD))))
		{
			engineController.TryStartEngine(player);
		}
	}

	public float GetAdjustedDriveForce(float absSpeed, float topSpeed)
	{
		float maxDriveForce = GetMaxDriveForce();
		float num = MathEx.BiasedLerp(bias: Mathf.Lerp(0.3f, 0.75f, GetPerformanceFraction()), x: 1f - absSpeed / topSpeed);
		return maxDriveForce * num;
	}

	public override float GetModifiedDrag()
	{
		float num = base.GetModifiedDrag();
		if (!allTerrain)
		{
			VehicleTerrainHandler.Surface onSurface = serverTerrainHandler.OnSurface;
			if (serverTerrainHandler.IsGrounded && onSurface != VehicleTerrainHandler.Surface.Frictionless && onSurface != VehicleTerrainHandler.Surface.Sand && onSurface != VehicleTerrainHandler.Surface.Snow && onSurface != VehicleTerrainHandler.Surface.Ice)
			{
				float num2 = Mathf.Max(num, badTerrainDrag);
				num = (prevTerrainModDrag = ((!(num2 <= prevTerrainModDrag)) ? Mathf.MoveTowards(prevTerrainModDrag, num2, 0.33f * (float)timeSinceTerrainModCheck) : prevTerrainModDrag));
			}
			else
			{
				prevTerrainModDrag = 0f;
			}
		}
		timeSinceTerrainModCheck = 0f;
		InSlowMode = num >= badTerrainDrag;
		return num;
	}

	public override float MaxVelocity()
	{
		return Mathf.Max(GetMaxForwardSpeed() * 1.3f, 30f);
	}

	public CarWheel[] GetWheels()
	{
		if (wheels == null)
		{
			wheels = new CarWheel[6] { wheelSkiFL, wheelSkiFR, wheelTreadFL, wheelTreadFR, wheelTreadRL, wheelTreadRR };
		}
		return wheels;
	}

	public float GetWheelsMidPos()
	{
		return (wheelSkiFL.wheelCollider.transform.localPosition.z - wheelTreadRL.wheelCollider.transform.localPosition.z) * 0.5f;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.snowmobile = Facepunch.Pool.Get<ProtoBuf.Snowmobile>();
		info.msg.snowmobile.steerInput = SteerInput;
		info.msg.snowmobile.driveWheelVel = DriveWheelVelocity;
		info.msg.snowmobile.throttleInput = ThrottleInput;
		info.msg.snowmobile.brakeInput = BrakeInput;
		info.msg.snowmobile.storageID = itemStorageInstance.uid;
		info.msg.snowmobile.fuelStorageID = GetFuelSystem().fuelStorageInstance.uid;
	}

	public override int StartingFuelUnits()
	{
		return 0;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned && child.prefabID == itemStoragePrefab.GetEntity().prefabID)
		{
			itemStorageInstance.Set((StorageContainer)child);
		}
	}

	internal override void DoServerDestroy()
	{
		if (vehicle.vehiclesdroploot)
		{
			StorageContainer storageContainer = itemStorageInstance.Get(base.isServer);
			if (storageContainer != null && BaseNetworkableEx.IsValid(storageContainer))
			{
				storageContainer.DropItems();
			}
		}
		base.DoServerDestroy();
	}

	public override bool MeetsEngineRequirements()
	{
		return HasDriver();
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (allowPassengerOnly)
		{
			base.AttemptMount(player, doMountChecks);
		}
		else if (MountEligable(player))
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

	public void SnowmobileDecay()
	{
		if (!IsDead() && !((float)timeSinceLastUsed < 2700f))
		{
			float num = (IsOutside() ? outsideDecayMinutes : float.PositiveInfinity);
			if (!float.IsPositiveInfinity(num))
			{
				float num2 = 1f / num;
				Hurt(MaxHealth() * num2, DamageType.Decay, this, useProtection: false);
			}
		}
	}

	public StorageContainer GetItemContainer()
	{
		BaseEntity baseEntity = itemStorageInstance.Get(base.isServer);
		if (baseEntity != null && BaseNetworkableEx.IsValid(baseEntity))
		{
			return baseEntity as StorageContainer;
		}
		return null;
	}

	private void UpdateClients()
	{
		if (HasDriver())
		{
			byte num = (byte)((ThrottleInput + 1f) * 7f);
			byte b = (byte)(BrakeInput * 15f);
			byte arg = (byte)(num + (b << 4));
			ClientRPC(null, "SnowmobileUpdate", SteerInput, arg, DriveWheelVelocity, GetFuelFraction());
		}
	}

	public override void OnEngineStartFailed()
	{
		ClientRPC(null, "EngineStartFailed");
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		riderProtection.Scale(info.damageTypes);
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (CanBeLooted(player) && IsDriver(player))
		{
			GetFuelSystem().LootFuel(player);
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

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.snowmobile != null)
		{
			itemStorageInstance.uid = info.msg.snowmobile.storageID;
			engineController.FuelSystem.fuelStorageInstance.uid = info.msg.snowmobile.fuelStorageID;
			cachedFuelFraction = info.msg.snowmobile.fuelFraction;
		}
	}

	public float GetMaxDriveForce()
	{
		return (float)engineKW * 10f * GetPerformanceFraction();
	}

	public override float GetMaxForwardSpeed()
	{
		return GetMaxDriveForce() / Mass * 15f;
	}

	public override float GetThrottleInput()
	{
		return ThrottleInput;
	}

	public override float GetBrakeInput()
	{
		return BrakeInput;
	}

	public float GetSteerInput()
	{
		return SteerInput;
	}

	public bool GetSteerModInput()
	{
		return false;
	}

	public float GetPerformanceFraction()
	{
		float t = Mathf.InverseLerp(0.25f, 0.5f, base.healthFraction);
		return Mathf.Lerp(0.5f, 1f, t);
	}

	public float GetFuelFraction()
	{
		if (base.isServer)
		{
			return engineController.FuelSystem.GetFuelFraction();
		}
		return cachedFuelFraction;
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (!PlayerIsMounted(player))
		{
			return !IsOn();
		}
		return true;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && Rust.GameInfo.HasAchievements && !old.HasFlag(Flags.On) && next.HasFlag(Flags.On))
		{
			BasePlayer driver = GetDriver();
			if (driver != null && driver.FindTrigger<TriggerSnowmobileAchievement>() != null)
			{
				driver.GiveAchievement("DRIVE_SNOWMOBILE");
			}
		}
	}
}
