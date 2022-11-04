#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class TrainEngine : TrainCar, IEngineControllerUser, IEntity
{
	private enum LeverStyle
	{
		WorkCart = 0,
		Locomotive = 1
	}

	public enum EngineSpeeds
	{
		Rev_Hi = 0,
		Rev_Med = 1,
		Rev_Lo = 2,
		Zero = 3,
		Fwd_Lo = 4,
		Fwd_Med = 5,
		Fwd_Hi = 6
	}

	public const float HAZARD_CHECK_EVERY = 1f;

	public const float HAZARD_DIST_MAX = 325f;

	public const float HAZARD_DIST_MIN = 20f;

	public const float HAZARD_SPEED_MIN = 4.5f;

	public float buttonHoldTime;

	public static readonly EngineSpeeds MaxThrottle = EngineSpeeds.Fwd_Hi;

	public static readonly EngineSpeeds MinThrottle = EngineSpeeds.Rev_Hi;

	public EngineDamageOverTime engineDamage;

	public Vector3 engineLocalOffset;

	public int lastSentLinedUpToUnload = -1;

	[Header("Train Engine")]
	[SerializeField]
	public Transform leftHandLever;

	[SerializeField]
	public Transform rightHandLever;

	[SerializeField]
	public Transform leftHandGrip;

	[SerializeField]
	public Transform rightHandGrip;

	[SerializeField]
	private LeverStyle leverStyle;

	[SerializeField]
	public Canvas monitorCanvas;

	[SerializeField]
	public RustText monitorText;

	[SerializeField]
	private LocomotiveExtraVisuals gauges;

	[SerializeField]
	public float engineForce = 50000f;

	[SerializeField]
	public float maxSpeed = 12f;

	[SerializeField]
	public float engineStartupTime = 1f;

	[SerializeField]
	public GameObjectRef fuelStoragePrefab;

	[SerializeField]
	public float idleFuelPerSec = 0.05f;

	[SerializeField]
	public float maxFuelPerSec = 0.15f;

	[SerializeField]
	public ProtectionProperties driverProtection;

	[SerializeField]
	public bool lootablesAreOnPlatform;

	[SerializeField]
	private bool mustMountFromPlatform = true;

	[SerializeField]
	private VehicleLight[] onLights;

	[SerializeField]
	public VehicleLight[] headlights;

	[SerializeField]
	private VehicleLight[] notMovingLights;

	[SerializeField]
	private VehicleLight[] movingForwardLights;

	[FormerlySerializedAs("movingBackwardsLights")]
	[SerializeField]
	private VehicleLight[] movingBackwardLights;

	[SerializeField]
	public ParticleSystemContainer fxEngineOn;

	[SerializeField]
	public ParticleSystemContainer fxLightDamage;

	[SerializeField]
	public ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	public ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	public ParticleSystemContainer fxEngineTrouble;

	[SerializeField]
	public BoxCollider engineWorldCol;

	[SerializeField]
	public float engineDamageToSlow = 150f;

	[SerializeField]
	public float engineDamageTimeframe = 10f;

	[SerializeField]
	public float engineSlowedTime = 10f;

	[SerializeField]
	public float engineSlowedMaxVel = 4f;

	[SerializeField]
	private ParticleSystemContainer[] sparks;

	[FormerlySerializedAs("brakeSparkLights")]
	[SerializeField]
	private Light[] sparkLights;

	[SerializeField]
	private TrainEngineAudio trainAudio;

	public const Flags Flag_HazardAhead = Flags.Reserved6;

	public const Flags Flag_Horn = Flags.Reserved8;

	public const Flags Flag_AltColor = Flags.Reserved9;

	public const Flags Flag_EngineSlowed = Flags.Reserved10;

	public VehicleEngineController<TrainEngine> engineController;

	public override bool networkUpdateOnCompleteTrainChange => true;

	public bool LightsAreOn => HasFlag(Flags.Reserved5);

	public bool CloseToHazard => HasFlag(Flags.Reserved6);

	public bool EngineIsSlowed => HasFlag(Flags.Reserved10);

	public EngineSpeeds CurThrottleSetting { get; set; } = EngineSpeeds.Zero;


	public override TrainCarType CarType => TrainCarType.Engine;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TrainEngine.OnRpcMessage"))
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

	public override void ServerInit()
	{
		base.ServerInit();
		engineDamage = new EngineDamageOverTime(engineDamageToSlow, engineDamageTimeframe, OnEngineTookHeavyDamage);
		engineLocalOffset = base.transform.InverseTransformPoint(engineWorldCol.transform.position + engineWorldCol.transform.rotation * engineWorldCol.center);
	}

	public override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned)
		{
			GetFuelSystem().CheckNewChild(child);
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		engineController.CheckEngineState();
		if (engineController.IsOn)
		{
			float fuelPerSecond = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, Mathf.Abs(GetThrottleFraction()));
			if (engineController.TickFuel(fuelPerSecond) > 0)
			{
				ClientRPC(null, "SetFuelAmount", GetFuelAmount());
			}
			if (completeTrain != null && completeTrain.LinedUpToUnload != lastSentLinedUpToUnload)
			{
				SendNetworkUpdate();
			}
		}
		else if (LightsAreOn && !HasDriver())
		{
			SetFlag(Flags.Reserved5, b: false);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.trainEngine = Facepunch.Pool.Get<ProtoBuf.TrainEngine>();
		info.msg.trainEngine.throttleSetting = (int)CurThrottleSetting;
		info.msg.trainEngine.fuelStorageID = GetFuelSystem().fuelStorageInstance.uid;
		info.msg.trainEngine.fuelAmount = GetFuelAmount();
		info.msg.trainEngine.numConnectedCars = completeTrain.NumTrainCars;
		info.msg.trainEngine.linedUpToUnload = completeTrain.LinedUpToUnload;
		lastSentLinedUpToUnload = completeTrain.LinedUpToUnload;
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return engineController.FuelSystem;
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
		if (!IsDriver(player))
		{
			return;
		}
		if (engineController.IsOff)
		{
			if ((inputState.IsDown(BUTTON.FORWARD) && !inputState.WasDown(BUTTON.FORWARD)) || (inputState.IsDown(BUTTON.BACKWARD) && !inputState.WasDown(BUTTON.BACKWARD)))
			{
				engineController.TryStartEngine(player);
			}
			SetFlag(Flags.Reserved8, b: false);
		}
		else
		{
			if (!ProcessThrottleInput(BUTTON.FORWARD, IncreaseThrottle))
			{
				ProcessThrottleInput(BUTTON.BACKWARD, DecreaseThrottle);
			}
			SetFlag(Flags.Reserved8, inputState.IsDown(BUTTON.FIRE_PRIMARY));
		}
		if (inputState.IsDown(BUTTON.LEFT))
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Left);
		}
		else if (inputState.IsDown(BUTTON.RIGHT))
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Right);
		}
		else
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Default);
		}
		bool ProcessThrottleInput(BUTTON button, Action action)
		{
			if (inputState.IsDown(button))
			{
				if (!inputState.WasDown(button))
				{
					action();
					buttonHoldTime = 0f;
				}
				else
				{
					buttonHoldTime += player.clientTickInterval;
					if (buttonHoldTime > 0.55f)
					{
						action();
						buttonHoldTime = 0.4f;
					}
				}
				return true;
			}
			return false;
		}
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		SetFlag(Flags.Reserved8, b: false);
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		driverProtection.Scale(info.damageTypes);
	}

	public bool MeetsEngineRequirements()
	{
		if (!HasDriver() && CurThrottleSetting == EngineSpeeds.Zero)
		{
			return false;
		}
		if (!completeTrain.AnyPlayersOnTrain())
		{
			return vehicle.trainskeeprunning;
		}
		return true;
	}

	public void OnEngineStartFailed()
	{
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (CanMount(player))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	protected override float GetThrottleForce()
	{
		if (IsDead() || base.IsDestroyed)
		{
			return 0f;
		}
		float num = 0f;
		float num2 = (engineController.IsOn ? GetThrottleFraction() : 0f);
		float value = maxSpeed * num2;
		float curTopSpeed = GetCurTopSpeed();
		value = Mathf.Clamp(value, 0f - curTopSpeed, curTopSpeed);
		float trackSpeed = GetTrackSpeed();
		if (num2 > 0f && trackSpeed < value)
		{
			num += GetCurEngineForce();
		}
		else if (num2 < 0f && trackSpeed > value)
		{
			num -= GetCurEngineForce();
		}
		return num;
	}

	public override bool HasThrottleInput()
	{
		if (engineController.IsOn)
		{
			return CurThrottleSetting != EngineSpeeds.Zero;
		}
		return false;
	}

	public override void Hurt(HitInfo info)
	{
		if (engineDamage != null && Vector3.SqrMagnitude(engineLocalOffset - info.HitPositionLocal) < 2f)
		{
			engineDamage.TakeDamage(info.damageTypes.Total());
		}
		base.Hurt(info);
	}

	public void StopEngine()
	{
		engineController.StopEngine();
	}

	public override Vector3 GetExplosionPos()
	{
		return engineWorldCol.transform.position + engineWorldCol.center;
	}

	public void IncreaseThrottle()
	{
		if (CurThrottleSetting != MaxThrottle)
		{
			SetThrottle(CurThrottleSetting + 1);
		}
	}

	public void DecreaseThrottle()
	{
		if (CurThrottleSetting != MinThrottle)
		{
			SetThrottle(CurThrottleSetting - 1);
		}
	}

	public void SetZeroThrottle()
	{
		SetThrottle(EngineSpeeds.Zero);
	}

	public override void ServerFlagsChanged(Flags old, Flags next)
	{
		base.ServerFlagsChanged(old, next);
		if (next.HasFlag(Flags.On) && !old.HasFlag(Flags.On))
		{
			SetFlag(Flags.Reserved5, b: true);
			InvokeRandomized(CheckForHazards, 0f, 1f, 0.1f);
		}
		else if (!next.HasFlag(Flags.On) && old.HasFlag(Flags.On))
		{
			CancelInvoke(CheckForHazards);
			SetFlag(Flags.Reserved6, b: false);
		}
	}

	public void CheckForHazards()
	{
		float trackSpeed = GetTrackSpeed();
		if (trackSpeed > 4.5f || trackSpeed < -4.5f)
		{
			float maxHazardDist = Mathf.Lerp(40f, 325f, Mathf.Abs(trackSpeed) * 0.05f);
			SetFlag(Flags.Reserved6, base.FrontTrackSection.HasValidHazardWithin(this, base.FrontWheelSplineDist, 20f, maxHazardDist, localTrackSelection, trackSpeed, base.RearTrackSection, null));
		}
		else
		{
			SetFlag(Flags.Reserved6, b: false);
		}
	}

	public void OnEngineTookHeavyDamage()
	{
		SetFlag(Flags.Reserved10, b: true);
		Invoke(ResetEngineToNormal, engineSlowedTime);
	}

	public void ResetEngineToNormal()
	{
		SetFlag(Flags.Reserved10, b: false);
	}

	public float GetCurTopSpeed()
	{
		float num = maxSpeed * GetEnginePowerMultiplier(0.5f);
		if (EngineIsSlowed)
		{
			num = Mathf.Clamp(num, 0f - engineSlowedMaxVel, engineSlowedMaxVel);
		}
		return num;
	}

	public float GetCurEngineForce()
	{
		return engineForce * GetEnginePowerMultiplier(0.75f);
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanBeLooted(player))
		{
			GetFuelSystem().LootFuel(player);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		engineController = new VehicleEngineController<TrainEngine>(this, base.isServer, engineStartupTime, fuelStoragePrefab);
		if (base.isServer)
		{
			bool b = SeedRandom.Range(net.ID, 0, 2) == 0;
			SetFlag(Flags.Reserved9, b);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.trainEngine != null)
		{
			engineController.FuelSystem.fuelStorageInstance.uid = info.msg.trainEngine.fuelStorageID;
			SetThrottle((EngineSpeeds)info.msg.trainEngine.throttleSetting);
		}
	}

	public override bool CanBeLooted(BasePlayer player)
	{
		if (!base.CanBeLooted(player))
		{
			return false;
		}
		if (player.isMounted)
		{
			return false;
		}
		if (lootablesAreOnPlatform)
		{
			return PlayerIsOnPlatform(player);
		}
		if (GetLocalVelocity().magnitude < 2f)
		{
			return true;
		}
		return PlayerIsOnPlatform(player);
	}

	public float GetEnginePowerMultiplier(float minPercent)
	{
		if (base.healthFraction > 0.4f)
		{
			return 1f;
		}
		return Mathf.Lerp(minPercent, 1f, base.healthFraction / 0.4f);
	}

	public float GetThrottleFraction()
	{
		return CurThrottleSetting switch
		{
			EngineSpeeds.Rev_Hi => -1f, 
			EngineSpeeds.Rev_Med => -0.5f, 
			EngineSpeeds.Rev_Lo => -0.2f, 
			EngineSpeeds.Zero => 0f, 
			EngineSpeeds.Fwd_Lo => 0.2f, 
			EngineSpeeds.Fwd_Med => 0.5f, 
			EngineSpeeds.Fwd_Hi => 1f, 
			_ => 0f, 
		};
	}

	public bool IsNearDesiredSpeed(float leeway)
	{
		float num = Vector3.Dot(base.transform.forward, GetLocalVelocity());
		float num2 = maxSpeed * GetThrottleFraction();
		if (num2 < 0f)
		{
			return num - leeway <= num2;
		}
		return num + leeway >= num2;
	}

	public override void SetTrackSelection(TrainTrackSpline.TrackSelection trackSelection)
	{
		base.SetTrackSelection(trackSelection);
	}

	public void SetThrottle(EngineSpeeds throttle)
	{
		if (CurThrottleSetting != throttle)
		{
			CurThrottleSetting = throttle;
			if (base.isServer)
			{
				ClientRPC(null, "SetThrottle", (sbyte)throttle);
			}
		}
	}

	public int GetFuelAmount()
	{
		if (base.isServer)
		{
			return engineController.FuelSystem.GetFuelAmount();
		}
		return 0;
	}

	public bool CanMount(BasePlayer player)
	{
		if (mustMountFromPlatform)
		{
			return PlayerIsOnPlatform(player);
		}
		return true;
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
