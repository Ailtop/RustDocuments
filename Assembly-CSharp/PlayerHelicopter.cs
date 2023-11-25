#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerHelicopter : BaseHelicopter, IEngineControllerUser, IEntity, SamSite.ISamSiteTarget
{
	[Serializable]
	public class Wheel
	{
		public WheelCollider wheelCollider;

		public Transform visualBone;

		public bool steering;

		public bool groundedTest = true;

		public Flags groundedFlag = Flags.Reserved1;

		[NonSerialized]
		public float wheelVel;

		[NonSerialized]
		public Vector3 wheelRot = Vector3.zero;

		public bool IsGrounded(PlayerHelicopter parent)
		{
			if (parent.isServer)
			{
				return wheelCollider.isGrounded;
			}
			return parent.HasFlag(groundedFlag);
		}
	}

	[SerializeField]
	[Header("Player Helicopter")]
	public Wheel[] wheels;

	[SerializeField]
	public Transform waterSample;

	public PlayerHeliSounds playerHeliSounds;

	[SerializeField]
	private Transform joystickPositionLeft;

	[SerializeField]
	private Transform joystickPositionRight;

	[SerializeField]
	private Transform leftFootPosition;

	[SerializeField]
	private Transform rightFootPosition;

	[SerializeField]
	private Transform passengerJoystickPositionRight;

	[SerializeField]
	protected Animator animator;

	[SerializeField]
	public float maxRotorSpeed = 10f;

	[SerializeField]
	public float timeUntilMaxRotorSpeed = 7f;

	[SerializeField]
	public float rotorBlurThreshold = 8f;

	[SerializeField]
	private Transform mainRotorBlurBone;

	[SerializeField]
	private Renderer mainRotorBlurMesh;

	[SerializeField]
	private Transform mainRotorBladesBone;

	[SerializeField]
	private Renderer[] mainRotorBladeMeshes;

	[SerializeField]
	private Transform rearRotorBladesBone;

	[SerializeField]
	private Renderer[] rearRotorBladeMeshes;

	[SerializeField]
	private Transform rearRotorBlurBone;

	[SerializeField]
	private Renderer rearRotorBlurMesh;

	[SerializeField]
	public float motorForceConstant = 150f;

	[SerializeField]
	public float brakeForceConstant = 500f;

	[SerializeField]
	private GameObject preventBuildingObject;

	[SerializeField]
	public float maxPitchAnim = 1f;

	[SerializeField]
	public float maxRollAnim = 1f;

	[SerializeField]
	public float maxYawAnim = 1f;

	[SerializeField]
	[Header("Fuel")]
	public GameObjectRef fuelStoragePrefab;

	[SerializeField]
	public float fuelPerSec = 0.25f;

	[SerializeField]
	public float fuelGaugeMax = 100f;

	[ServerVar(Help = "How long before a player helicopter loses all its health while outside")]
	public static float outsidedecayminutes = 480f;

	[ServerVar(Help = "How long before a player helicopter loses all its health while indoors")]
	public static float insidedecayminutes = 2880f;

	public VehicleEngineController<PlayerHelicopter> engineController;

	protected const Flags WHEEL_GROUNDED_LR = Flags.Reserved1;

	protected const Flags WHEEL_GROUNDED_RR = Flags.Reserved2;

	protected const Flags WHEEL_GROUNDED_FRONT = Flags.Reserved3;

	protected const Flags RADAR_WARNING_FLAG = Flags.Reserved12;

	protected const Flags RADAR_LOCK_FLAG = Flags.Reserved13;

	protected const Flags ENGINE_STARTING_FLAG = Flags.Reserved4;

	public TimeSince timeSinceCachedFuelFraction;

	public float cachedFuelFraction;

	public bool isPushing;

	public float lastEngineOnTime;

	public bool IsStartingUp
	{
		get
		{
			if (engineController != null)
			{
				return engineController.IsStarting;
			}
			return false;
		}
	}

	public VehicleEngineController<PlayerHelicopter>.EngineState CurEngineState
	{
		get
		{
			if (engineController == null)
			{
				return VehicleEngineController<PlayerHelicopter>.EngineState.Off;
			}
			return engineController.CurEngineState;
		}
	}

	public float cachedPitch { get; set; }

	public float cachedYaw { get; set; }

	public float cachedRoll { get; set; }

	public SamSite.SamTargetType SAMTargetType => SamSite.targetTypeVehicle;

	public override bool ForceMovementHandling
	{
		protected get
		{
			if (isPushing)
			{
				return wheels.Length != 0;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerHelicopter.OnRpcMessage"))
		{
			if (rpc == 1851540757 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_OpenFuel ");
				}
				using (TimeWarning.New("RPC_OpenFuel"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1851540757u, "RPC_OpenFuel", this, player, 6f))
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

	public override void InitShared()
	{
		base.InitShared();
		engineController = new VehicleEngineController<PlayerHelicopter>(this, base.isServer, 5f, fuelStoragePrefab, waterSample, Flags.Reserved4);
	}

	public float GetFuelFraction(bool force = false)
	{
		if (base.isServer && ((float)timeSinceCachedFuelFraction > 1f || force))
		{
			cachedFuelFraction = Mathf.Clamp01((float)GetFuelSystem().GetFuelAmount() / fuelGaugeMax);
			timeSinceCachedFuelFraction = 0f;
		}
		return cachedFuelFraction;
	}

	public override bool CanPushNow(BasePlayer pusher)
	{
		if (base.CanPushNow(pusher) && pusher.IsOnGround())
		{
			return !pusher.isMounted;
		}
		return false;
	}

	public override float InheritedVelocityScale()
	{
		return 1f;
	}

	public override bool InheritedVelocityDirection()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.miniCopter != null)
		{
			engineController.FuelSystem.fuelStorageInstance.uid = info.msg.miniCopter.fuelStorageID;
			cachedFuelFraction = info.msg.miniCopter.fuelFraction;
			cachedPitch = info.msg.miniCopter.pitch * maxPitchAnim;
			cachedRoll = info.msg.miniCopter.roll * maxRollAnim;
			cachedYaw = info.msg.miniCopter.yaw * maxYawAnim;
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && CurEngineState == VehicleEngineController<PlayerHelicopter>.EngineState.Off)
		{
			lastEngineOnTime = UnityEngine.Time.time;
		}
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned)
		{
			GetFuelSystem().CheckNewChild(child);
		}
	}

	public override float GetServiceCeiling()
	{
		return HotAirBalloon.serviceCeiling;
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return engineController.FuelSystem;
	}

	public override int StartingFuelUnits()
	{
		return 100;
	}

	public bool IsValidSAMTarget(bool staticRespawn)
	{
		if (staticRespawn)
		{
			return true;
		}
		return !InSafeZone();
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (!IsOn() && !IsStartingUp && inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD))
		{
			engineController.TryStartEngine(player);
		}
		currentInputState.groundControl = inputState.IsDown(BUTTON.DUCK);
		if (currentInputState.groundControl)
		{
			currentInputState.roll = 0f;
			currentInputState.throttle = (inputState.IsDown(BUTTON.FORWARD) ? 1f : 0f);
			currentInputState.throttle -= (inputState.IsDown(BUTTON.BACKWARD) ? 1f : 0f);
		}
		cachedRoll = currentInputState.roll;
		cachedYaw = currentInputState.yaw;
		cachedPitch = currentInputState.pitch;
	}

	public bool Grounded()
	{
		if (wheels.Length == 0)
		{
			return UnityEngine.Physics.Raycast(base.transform.position + Vector3.up * 0.1f, Vector3.down, 0.5f);
		}
		Wheel[] array = wheels;
		foreach (Wheel wheel in array)
		{
			if (wheel.groundedTest && !wheel.wheelCollider.isGrounded)
			{
				return false;
			}
		}
		return true;
	}

	public override void SetDefaultInputState()
	{
		currentInputState.Reset();
		cachedRoll = 0f;
		cachedYaw = 0f;
		cachedPitch = 0f;
		if (Grounded())
		{
			return;
		}
		if (HasDriver())
		{
			float num = Vector3.Dot(Vector3.up, base.transform.right);
			float num2 = Vector3.Dot(Vector3.up, base.transform.forward);
			currentInputState.roll = ((num < 0f) ? 1f : 0f);
			currentInputState.roll -= ((num > 0f) ? 1f : 0f);
			if (num2 < -0f)
			{
				currentInputState.pitch = -1f;
			}
			else if (num2 > 0f)
			{
				currentInputState.pitch = 1f;
			}
		}
		else
		{
			currentInputState.throttle = -1f;
		}
	}

	public void ApplyForceAtWheels()
	{
		if (!(rigidBody == null))
		{
			float brakeScale;
			float num2;
			float num;
			if (currentInputState.groundControl)
			{
				brakeScale = ((currentInputState.throttle == 0f) ? 50f : 0f);
				num = currentInputState.throttle;
				num2 = currentInputState.yaw;
			}
			else
			{
				brakeScale = 20f;
				num2 = 0f;
				num = 0f;
			}
			num *= (IsOn() ? 1f : 0f);
			if (isPushing)
			{
				brakeScale = 0f;
				num = 0.1f;
				num2 = 0f;
			}
			Wheel[] array = wheels;
			foreach (Wheel wheel in array)
			{
				ApplyWheelForce(wheel.wheelCollider, num, brakeScale, wheel.steering ? num2 : 0f);
			}
		}
	}

	public void ApplyForceWithoutWheels()
	{
		if (currentInputState.groundControl)
		{
			if (currentInputState.throttle != 0f)
			{
				rigidBody.AddRelativeForce(Vector3.forward * currentInputState.throttle * motorForceConstant * 15f, ForceMode.Force);
			}
			if (currentInputState.yaw != 0f)
			{
				rigidBody.AddRelativeTorque(new Vector3(0f, currentInputState.yaw * torqueScale.y, 0f), ForceMode.Force);
			}
			float num = rigidBody.mass * (0f - UnityEngine.Physics.gravity.y);
			rigidBody.AddForce(base.transform.up * num * hoverForceScale, ForceMode.Force);
		}
	}

	public void ApplyWheelForce(WheelCollider wheel, float gasScale, float brakeScale, float turning)
	{
		if (wheel.isGrounded)
		{
			float num = gasScale * motorForceConstant;
			float num2 = brakeScale * brakeForceConstant;
			float num3 = 45f * turning;
			if (!Mathf.Approximately(wheel.motorTorque, num))
			{
				wheel.motorTorque = num;
			}
			if (!Mathf.Approximately(wheel.brakeTorque, num2))
			{
				wheel.brakeTorque = num2;
			}
			if (!Mathf.Approximately(wheel.steerAngle, num3))
			{
				wheel.steerAngle = num3;
			}
		}
	}

	public override void MovementUpdate()
	{
		if (Grounded())
		{
			if (wheels.Length != 0)
			{
				ApplyForceAtWheels();
			}
			else
			{
				ApplyForceWithoutWheels();
			}
		}
		if (!currentInputState.groundControl || !Grounded())
		{
			base.MovementUpdate();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		lastEngineOnTime = UnityEngine.Time.realtimeSinceStartup;
		rigidBody.inertiaTensor = rigidBody.inertiaTensor;
		preventBuildingObject.SetActive(value: true);
		InvokeRandomized(UpdateNetwork, 0f, 0.2f, 0.05f);
		InvokeRandomized(DecayTick, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	public void DecayTick()
	{
		if (base.healthFraction != 0f && !IsOn() && !(UnityEngine.Time.time < lastEngineOnTime + 600f))
		{
			float num = 1f / (IsOutside() ? outsidedecayminutes : insidedecayminutes);
			Hurt(MaxHealth() * num, DamageType.Decay, this, useProtection: false);
		}
	}

	public override bool IsEngineOn()
	{
		return IsOn();
	}

	protected override void TryStartEngine(BasePlayer player)
	{
		engineController.TryStartEngine(player);
	}

	public bool MeetsEngineRequirements()
	{
		if (base.autoHover)
		{
			return true;
		}
		if (engineController.IsOff)
		{
			return HasDriver();
		}
		if (!HasDriver())
		{
			return UnityEngine.Time.time <= lastPlayerInputTime + 1f;
		}
		return true;
	}

	public void OnEngineStartFailed()
	{
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		engineController.CheckEngineState();
		engineController.TickFuel(fuelPerSec);
	}

	public void UpdateNetwork()
	{
		Flags flags = base.flags;
		Wheel[] array = wheels;
		foreach (Wheel wheel in array)
		{
			SetFlag(wheel.groundedFlag, wheel.wheelCollider.isGrounded, recursive: false, networkupdate: false);
		}
		if (HasDriver())
		{
			SendNetworkUpdate();
		}
		else if (flags != base.flags)
		{
			SendNetworkUpdate_Flags();
		}
	}

	public override void OnEntityMessage(BaseEntity from, string msg)
	{
		if (msg == "RadarLock")
		{
			SetFlag(Flags.Reserved13, b: true);
			Invoke(ClearRadarLock, 1f);
		}
		else if (msg == "RadarWarning")
		{
			SetFlag(Flags.Reserved12, b: true);
			Invoke(ClearRadarWarning, 1f);
		}
		else
		{
			base.OnEntityMessage(from, msg);
		}
	}

	public void ClearRadarLock()
	{
		SetFlag(Flags.Reserved13, b: false);
	}

	public void ClearRadarWarning()
	{
		SetFlag(Flags.Reserved12, b: false);
	}

	public void UpdateCOM()
	{
		rigidBody.centerOfMass = com.localPosition;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.miniCopter = Facepunch.Pool.Get<ProtoBuf.Minicopter>();
		info.msg.miniCopter.fuelStorageID = engineController.FuelSystem.fuelStorageInstance.uid;
		info.msg.miniCopter.fuelFraction = GetFuelFraction(force: true);
		info.msg.miniCopter.pitch = currentInputState.pitch;
		info.msg.miniCopter.roll = currentInputState.roll;
		info.msg.miniCopter.yaw = currentInputState.yaw;
	}

	public override void OnKilled(HitInfo info)
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((bool)mounted)
				{
					HitInfo hitInfo = new HitInfo(info.Initiator, this, DamageType.Explosion, 1000f, base.transform.position);
					hitInfo.Weapon = info.Weapon;
					hitInfo.WeaponPrefab = info.WeaponPrefab;
					mounted.Hurt(hitInfo);
				}
			}
		}
		base.OnKilled(info);
	}

	public override void DoPushAction(BasePlayer player)
	{
		Vector3 vector = Vector3Ex.Direction2D(player.transform.position, base.transform.position);
		Vector3 vector2 = player.eyes.BodyForward();
		vector2.y = 0.25f;
		Vector3 position = base.transform.position + vector * 2f;
		float num = rigidBody.mass * 2f;
		rigidBody.AddForceAtPosition(vector2 * num, position, ForceMode.Impulse);
		rigidBody.AddForce(Vector3.up * 3f, ForceMode.Impulse);
		isPushing = true;
		Invoke(DisablePushing, 0.5f);
	}

	public void DisablePushing()
	{
		isPushing = false;
		Wheel[] array = wheels;
		foreach (Wheel wheel in array)
		{
			ApplyWheelForce(wheel.wheelCollider, 0f, 20f, 0f);
		}
	}

	public override bool IsValidHomingTarget()
	{
		object obj = Interface.CallHook("CanBeHomingTargeted", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return IsOn();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(6f)]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			BasePlayer driver = GetDriver();
			if ((!(driver != null) || !(driver != player)) && (!IsSafe() || !(player != creatorEntity)))
			{
				engineController.FuelSystem.LootFuel(player);
			}
		}
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
