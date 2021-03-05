#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class MiniCopter : BaseHelicopterVehicle, IEngineControllerUser, IEntity, SamSite.ISamSiteTarget
{
	[Header("Fuel")]
	public GameObjectRef fuelStoragePrefab;

	public Transform fuelStoragePoint;

	public float fuelPerSec = 0.25f;

	public float fuelGaugeMax = 100f;

	public EntityFuelSystem fuelSystem;

	public float cachedFuelFraction;

	public Transform waterSample;

	public WheelCollider leftWheel;

	public WheelCollider rightWheel;

	public WheelCollider frontWheel;

	public Transform leftWheelTrans;

	public Transform rightWheelTrans;

	public Transform frontWheelTrans;

	public float cachedrotation_left;

	public float cachedrotation_right;

	public float cachedrotation_front;

	[Header("IK")]
	public Transform joystickPositionLeft;

	public Transform joystickPositionRight;

	public Transform leftFootPosition;

	public Transform rightFootPosition;

	public AnimationCurve bladeEngineCurve;

	public Animator animator;

	public float maxRotorSpeed = 10f;

	public float timeUntilMaxRotorSpeed = 7f;

	public float rotorBlurThreshold = 8f;

	public Transform mainRotorBlur;

	public Transform mainRotorBlades;

	public Transform rearRotorBlades;

	public Transform rearRotorBlur;

	public float motorForceConstant = 150f;

	public float brakeForceConstant = 500f;

	public GameObject preventBuildingObject;

	[ServerVar(Help = "Population active on the server")]
	public static float population = 0f;

	[ServerVar(Help = "How long before a minicopter is killed while outside")]
	public static float outsidedecayminutes = 480f;

	[ServerVar(Help = "How long before a minicopter is killed while indoors")]
	public static float insidedecayminutes = 2880f;

	public VehicleEngineController engineController;

	public bool isPushing;

	public float lastEngineOnTime;

	public float cachedPitch;

	public float cachedYaw;

	public float cachedRoll;

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

	public VehicleEngineController.EngineState CurEngineState
	{
		get
		{
			if (engineController == null)
			{
				return VehicleEngineController.EngineState.Off;
			}
			return engineController.CurEngineState;
		}
	}

	public float GetFuelFraction()
	{
		if (base.isServer)
		{
			cachedFuelFraction = Mathf.Clamp01((float)fuelSystem.GetFuelAmount() / fuelGaugeMax);
		}
		return cachedFuelFraction;
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return fuelSystem;
	}

	[RPC_Server.IsVisible(6f)]
	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			BasePlayer driver = GetDriver();
			if ((!(driver != null) || !(driver != player)) && (!IsSafe() || !(player != creatorEntity)))
			{
				fuelSystem.LootFuel(player);
			}
		}
	}

	public override void InitShared()
	{
		engineController = new VehicleEngineController(this, base.isServer, 5f, Flags.Reserved4);
		fuelSystem = new EntityFuelSystem(this, base.isServer);
	}

	public bool IsValidSAMTarget()
	{
		return true;
	}

	public override float GetServiceCeiling()
	{
		return HotAirBalloon.serviceCeiling;
	}

	public override void PilotInput(InputState inputState, BasePlayer player)
	{
		base.PilotInput(inputState, player);
		if (!IsOn() && !IsStartingUp && inputState.IsDown(BUTTON.FORWARD))
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
		if (leftWheel.isGrounded)
		{
			return rightWheel.isGrounded;
		}
		return false;
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
		if (IsMounted())
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
			float num = 50f;
			float num2 = 0f;
			float num3 = 0f;
			if (currentInputState.groundControl)
			{
				num = ((currentInputState.throttle == 0f) ? 50f : 0f);
				num2 = currentInputState.throttle;
				num3 = currentInputState.yaw;
			}
			else
			{
				num = 20f;
				num3 = 0f;
				num2 = 0f;
			}
			num2 *= (IsOn() ? 1f : 0f);
			if (isPushing)
			{
				num = 0f;
				num2 = 0.1f;
				num3 = 0f;
			}
			ApplyWheelForce(frontWheel, num2, num, num3);
			ApplyWheelForce(leftWheel, num2, num, 0f);
			ApplyWheelForce(rightWheel, num2, num, 0f);
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
			ApplyForceAtWheels();
		}
		if (IsOn() && (!currentInputState.groundControl || !Grounded()))
		{
			base.MovementUpdate();
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		lastEngineOnTime = UnityEngine.Time.realtimeSinceStartup;
		rigidBody.inertiaTensor = rigidBody.inertiaTensor;
		preventBuildingObject.SetActive(true);
		InvokeRandomized(UpdateNetwork, 0f, 0.2f, 0.05f);
		InvokeRandomized(DecayTick, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	public void DecayTick()
	{
		if (base.healthFraction != 0f && !IsOn() && !(UnityEngine.Time.time < lastEngineOnTime + 600f))
		{
			float num = 1f / (IsOutside() ? outsidedecayminutes : insidedecayminutes);
			Hurt(MaxHealth() * num, DamageType.Decay, this, false);
		}
	}

	public override void SpawnSubEntities()
	{
		base.SpawnSubEntities();
		fuelSystem.SpawnFuelStorage(fuelStoragePrefab, fuelStoragePoint);
	}

	public bool Waterlogged()
	{
		return WaterLevel.Test(waterSample.transform.position, true, this);
	}

	public override bool ShouldApplyHoverForce()
	{
		return IsOn();
	}

	public override bool IsEngineOn()
	{
		return IsOn();
	}

	public bool CanRunEngines()
	{
		if (HasDriver() && fuelSystem.HasFuel() && !Waterlogged())
		{
			return !IsDead();
		}
		return false;
	}

	public void OnEngineStartFailed()
	{
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && CurEngineState == VehicleEngineController.EngineState.Off)
		{
			lastEngineOnTime = UnityEngine.Time.time;
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (isSpawned)
		{
			if ((IsOn() || IsStartingUp) && ((UnityEngine.Time.time > lastPlayerInputTime + 1f && !HasDriver()) || !fuelSystem.HasFuel() || Waterlogged()))
			{
				engineController.StopEngine();
			}
			if (IsOn())
			{
				fuelSystem.TryUseFuel(UnityEngine.Time.fixedDeltaTime, fuelPerSec);
			}
		}
	}

	public void UpdateNetwork()
	{
		Flags flags = base.flags;
		SetFlag(Flags.Reserved1, leftWheel.isGrounded, false, false);
		SetFlag(Flags.Reserved2, rightWheel.isGrounded, false, false);
		SetFlag(Flags.Reserved3, frontWheel.isGrounded, false, false);
		if (HasDriver())
		{
			SendNetworkUpdate();
		}
		else if (flags != base.flags)
		{
			SendNetworkUpdate_Flags();
		}
	}

	public void UpdateCOM()
	{
		rigidBody.centerOfMass = com.localPosition;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.miniCopter = Facepunch.Pool.Get<Minicopter>();
		info.msg.miniCopter.fuelStorageID = fuelSystem.fuelStorageInstance.uid;
		info.msg.miniCopter.fuelFraction = GetFuelFraction();
		info.msg.miniCopter.pitch = currentInputState.pitch;
		info.msg.miniCopter.roll = currentInputState.roll;
		info.msg.miniCopter.yaw = currentInputState.yaw;
	}

	public override void DismountAllPlayers()
	{
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			if (mountPoint.mountable != null)
			{
				BasePlayer mounted = mountPoint.mountable.GetMounted();
				if ((bool)mounted)
				{
					mounted.Hurt(10000f, DamageType.Explosion, this, false);
				}
			}
		}
	}

	public override void DoPushAction(BasePlayer player)
	{
		player.metabolism.calories.Subtract(3f);
		player.metabolism.SendChangesToClient();
		Vector3 a = Vector3Ex.Direction2D(player.transform.position, base.transform.position);
		Vector3 a2 = player.eyes.BodyForward();
		a2.y = 0.25f;
		Vector3 position = base.transform.position + a * 2f;
		float d = rigidBody.mass * 2f;
		rigidBody.AddForceAtPosition(a2 * d, position, ForceMode.Impulse);
		rigidBody.AddForce(Vector3.up * 3f, ForceMode.Impulse);
		if (rigidBody.IsSleeping())
		{
			rigidBody.WakeUp();
		}
		isPushing = true;
		Invoke(DisablePushing, 0.5f);
	}

	private void DisablePushing()
	{
		isPushing = false;
	}

	public float RemapValue(float toUse, float maxRemap)
	{
		return toUse * maxRemap;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.miniCopter != null)
		{
			fuelSystem.fuelStorageInstance.uid = info.msg.miniCopter.fuelStorageID;
			cachedFuelFraction = info.msg.miniCopter.fuelFraction;
			cachedPitch = RemapValue(info.msg.miniCopter.pitch, 0.5f);
			cachedRoll = RemapValue(info.msg.miniCopter.roll, 0.2f);
			cachedYaw = RemapValue(info.msg.miniCopter.yaw, 0.35f);
		}
	}

	public override bool CanPushNow(BasePlayer pusher)
	{
		if (base.CanPushNow(pusher))
		{
			return pusher.IsOnGround();
		}
		return false;
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MiniCopter.OnRpcMessage"))
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

	void IEngineControllerUser.Invoke(Action action, float time)
	{
		Invoke(action, time);
	}

	void IEngineControllerUser.CancelInvoke(Action action)
	{
		CancelInvoke(action);
	}
}
