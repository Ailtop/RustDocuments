#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;

public class TrainEngine : BaseTrain, IEngineControllerUser, IEntity
{
	public enum EngineSpeeds
	{
		Rev_Hi,
		Rev_Med,
		Rev_Lo,
		Zero,
		Fwd_Lo,
		Fwd_Med,
		Fwd_Hi
	}

	public float buttonHoldTime;

	public const float HAZARD_CHECK_EVERY = 1f;

	public const float HAZARD_DIST_MAX = 325f;

	public const float HAZARD_DIST_MIN = 20f;

	public const float HAZARD_SPEED_MIN = 4.5f;

	public static readonly EngineSpeeds MaxThrottle = EngineSpeeds.Fwd_Hi;

	public static readonly EngineSpeeds MinThrottle = EngineSpeeds.Rev_Hi;

	public float decayDuration = 1200f;

	public float decayTickSpacing = 60f;

	public float lastDecayTick;

	public float decayingFor;

	public EngineDamageOverTime engineDamage;

	public Vector3 spawnOrigin;

	public Vector3 engineLocalOffset;

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
	public Canvas monitorConvas;

	[SerializeField]
	public RustText monitorText;

	[SerializeField]
	public float engineForce = 50000f;

	[SerializeField]
	public float maxSpeed = 12f;

	[SerializeField]
	public float engineStartupTime = 1f;

	[SerializeField]
	public GameObjectRef fuelStoragePrefab;

	[SerializeField]
	public Transform fuelStoragePoint;

	[SerializeField]
	public float idleFuelPerSec = 0.05f;

	[SerializeField]
	public float maxFuelPerSec = 0.15f;

	[SerializeField]
	public GameObject detailedCabinColliderObj;

	[SerializeField]
	public TriggerParent platformParentTrigger;

	[SerializeField]
	public BoxCollider platformParentTriggerCollider;

	[SerializeField]
	public ProtectionProperties driverProtection;

	[SerializeField]
	public float wheelRadius = 0.615f;

	[SerializeField]
	public Transform[] wheelVisuals;

	[SerializeField]
	public VehicleLight[] lights;

	[SerializeField]
	public ParticleSystemContainer fxLightDamage;

	[SerializeField]
	public ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	public ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	private ParticleSystemContainer fxEngineTrouble;

	[SerializeField]
	public BoxCollider engineWorldCol;

	[SerializeField]
	public GameObjectRef fxFinalExplosion;

	[SerializeField]
	public float engineDamageToSlow = 150f;

	[SerializeField]
	public float engineDamageTimeframe = 10f;

	[SerializeField]
	public float engineSlowedTime = 10f;

	[SerializeField]
	public float engineSlowedMaxVel = 4f;

	[SerializeField]
	private TrainEngineAudio trainAudio;

	public const Flags Flag_HazardAhead = Flags.Reserved6;

	public const Flags Flag_AltColor = Flags.Reserved9;

	public const Flags Flag_EngineSlowed = Flags.Reserved10;

	public VehicleEngineController engineController;

	public EntityFuelSystem fuelSystem;

	public bool LightsAreOn => HasFlag(Flags.Reserved5);

	public bool CloseToHazard => HasFlag(Flags.Reserved6);

	public bool EngineIsSlowed => HasFlag(Flags.Reserved10);

	public EngineSpeeds CurThrottleSetting { get; set; } = EngineSpeeds.Zero;


	public bool IsMovingOrOn
	{
		get
		{
			if (!IsMoving())
			{
				return IsOn();
			}
			return true;
		}
	}

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
		InvokeRandomized(DecayTick, UnityEngine.Random.Range(20f, 40f), decayTickSpacing, decayTickSpacing * 0.1f);
		spawnOrigin = base.transform.position;
		engineDamage = new EngineDamageOverTime(engineDamageToSlow, engineDamageTimeframe, OnEngineTookHeavyDamage);
		engineLocalOffset = base.transform.InverseTransformPoint(engineWorldCol.transform.position + engineWorldCol.transform.rotation * engineWorldCol.center);
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (!IsFullySpawned())
		{
			return;
		}
		if (!engineController.IsOff && (!CanRunEngines() || !AnyPlayersOnTrain()))
		{
			engineController.StopEngine();
		}
		if (engineController.IsOn)
		{
			float fuelUsedPerSecond = Mathf.Lerp(idleFuelPerSec, maxFuelPerSec, Mathf.Abs(GetThrottleFraction()));
			if (fuelSystem.TryUseFuel(UnityEngine.Time.fixedDeltaTime, fuelUsedPerSecond) > 0)
			{
				ClientRPC(null, "SetFuelAmount", GetFuelAmount());
			}
		}
		else if (LightsAreOn && !HasDriver())
		{
			SetFlag(Flags.Reserved5, false);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.trainEngine = Facepunch.Pool.Get<ProtoBuf.TrainEngine>();
		info.msg.trainEngine.throttleSetting = (int)CurThrottleSetting;
		info.msg.trainEngine.fuelStorageID = fuelSystem.fuelStorageInstance.uid;
		info.msg.trainEngine.fuelAmount = GetFuelAmount();
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return fuelSystem;
	}

	public override void OnKilled(HitInfo info)
	{
		base.OnKilled(info);
		if (base.IsDestroyed)
		{
			Effect.server.Run(fxFinalExplosion.resourcePath, engineWorldCol.transform.position + engineWorldCol.center, Vector3.up, null, true);
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
		_003C_003Ec__DisplayClass21_0 _003C_003Ec__DisplayClass21_ = default(_003C_003Ec__DisplayClass21_0);
		_003C_003Ec__DisplayClass21_.inputState = inputState;
		_003C_003Ec__DisplayClass21_._003C_003E4__this = this;
		_003C_003Ec__DisplayClass21_.player = player;
		if (!IsDriver(_003C_003Ec__DisplayClass21_.player))
		{
			return;
		}
		if (engineController.IsOff)
		{
			if ((_003C_003Ec__DisplayClass21_.inputState.IsDown(BUTTON.FORWARD) && !_003C_003Ec__DisplayClass21_.inputState.WasDown(BUTTON.FORWARD)) || (_003C_003Ec__DisplayClass21_.inputState.IsDown(BUTTON.BACKWARD) && !_003C_003Ec__DisplayClass21_.inputState.WasDown(BUTTON.BACKWARD)))
			{
				engineController.TryStartEngine(_003C_003Ec__DisplayClass21_.player);
			}
		}
		else if (!_003CPlayerServerInput_003Eg__ProcessThrottleInput_007C21_0(BUTTON.FORWARD, IncreaseThrottle, ref _003C_003Ec__DisplayClass21_))
		{
			_003CPlayerServerInput_003Eg__ProcessThrottleInput_007C21_0(BUTTON.BACKWARD, DecreaseThrottle, ref _003C_003Ec__DisplayClass21_);
		}
		if (_003C_003Ec__DisplayClass21_.inputState.IsDown(BUTTON.LEFT))
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Left);
		}
		else if (_003C_003Ec__DisplayClass21_.inputState.IsDown(BUTTON.RIGHT))
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Right);
		}
		else
		{
			SetTrackSelection(TrainTrackSpline.TrackSelection.Default);
		}
	}

	public override void ScaleDamageForPlayer(BasePlayer player, HitInfo info)
	{
		base.ScaleDamageForPlayer(player, info);
		driverProtection.Scale(info.damageTypes);
	}

	public override void SpawnSubEntities()
	{
		base.SpawnSubEntities();
		if (!Rust.Application.isLoadingSave)
		{
			fuelSystem.SpawnFuelStorage(fuelStoragePrefab, fuelStoragePoint);
		}
	}

	public bool CanRunEngines()
	{
		if (!HasDriver() && CurThrottleSetting == EngineSpeeds.Zero)
		{
			return false;
		}
		if (fuelSystem.HasFuel())
		{
			return !IsDead();
		}
		return false;
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

	public override float GetEngineForces()
	{
		if (IsDead() || base.IsDestroyed)
		{
			return 0f;
		}
		float num = (engineController.IsOn ? GetThrottleFraction() : 0f);
		float value = maxSpeed * num;
		float curTopSpeed = GetCurTopSpeed();
		value = Mathf.Clamp(value, 0f - curTopSpeed, curTopSpeed);
		if (num > 0f && base.TrackSpeed < value)
		{
			return GetCurEngineForce();
		}
		if (num < 0f && base.TrackSpeed > value)
		{
			return 0f - GetCurEngineForce();
		}
		return 0f;
	}

	public override void Hurt(HitInfo info)
	{
		if (engineDamage != null && Vector3.SqrMagnitude(engineLocalOffset - info.HitPositionLocal) < 2f)
		{
			engineDamage.TakeDamage(info.damageTypes.Total());
		}
		base.Hurt(info);
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

	public void ServerFlagsChanged(Flags old, Flags next)
	{
		if (next.HasFlag(Flags.On) && !old.HasFlag(Flags.On))
		{
			SetFlag(Flags.Reserved5, true);
			InvokeRandomized(CheckForHazards, 0f, 1f, 0.1f);
		}
		else if (!next.HasFlag(Flags.On) && old.HasFlag(Flags.On))
		{
			CancelInvoke(CheckForHazards);
			SetFlag(Flags.Reserved6, false);
		}
	}

	public bool AnyPlayersOnTrain()
	{
		if (AnyMounted())
		{
			return true;
		}
		if (platformParentTrigger.HasAnyEntityContents)
		{
			foreach (BaseEntity entityContent in platformParentTrigger.entityContents)
			{
				if (entityContent.ToPlayer() != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AnyPlayersNearby(float maxDist)
	{
		List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
		Vis.Entities(base.transform.position, maxDist, obj, 131072);
		bool result = false;
		foreach (BasePlayer item in obj)
		{
			if (!item.IsSleeping() && item.IsAlive())
			{
				result = true;
				break;
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		return result;
	}

	public void CheckForHazards()
	{
		if (base.TrackSpeed > 4.5f || base.TrackSpeed < -4.5f)
		{
			float maxHazardDist = Mathf.Lerp(40f, 325f, Mathf.Abs(base.TrackSpeed) * 0.05f);
			SetFlag(Flags.Reserved6, base.FrontTrackSection.HasValidHazardWithin(this, base.FrontWheelSplineDist, 20f, maxHazardDist, curTrackSelection, base.RearTrackSection));
		}
		else
		{
			SetFlag(Flags.Reserved6, false);
		}
	}

	public void OnEngineTookHeavyDamage()
	{
		SetFlag(Flags.Reserved10, true);
		Invoke(ResetEngineToNormal, engineSlowedTime);
	}

	public void ResetEngineToNormal()
	{
		SetFlag(Flags.Reserved10, false);
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

	public void DecayTick()
	{
		bool flag = HasDriver() || AnyPlayersOnTrain();
		bool num = base.IsAtAStation && Vector3.Distance(spawnOrigin, base.transform.position) < 50f;
		if (flag)
		{
			decayingFor = 0f;
		}
		bool num2 = !num && !flag && !AnyPlayersNearby(30f);
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num3 = realtimeSinceStartup - lastDecayTick;
		lastDecayTick = realtimeSinceStartup;
		if (num2)
		{
			decayingFor += num3;
			if (decayingFor >= decayDuration)
			{
				ActualDeath();
			}
		}
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

	public override void InitShared()
	{
		base.InitShared();
		engineController = new VehicleEngineController(this, base.isServer, engineStartupTime);
		fuelSystem = new EntityFuelSystem(this, base.isServer);
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
			fuelSystem.fuelStorageInstance.uid = info.msg.trainEngine.fuelStorageID;
			SetThrottle((EngineSpeeds)info.msg.trainEngine.throttleSetting);
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

	public override bool CanBeLooted(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (player.isMounted)
		{
			return false;
		}
		if (!PlayerIsInParentTrigger(player))
		{
			return false;
		}
		return true;
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
		switch (CurThrottleSetting)
		{
		case EngineSpeeds.Rev_Hi:
			return -1f;
		case EngineSpeeds.Rev_Med:
			return -0.5f;
		case EngineSpeeds.Rev_Lo:
			return -0.2f;
		case EngineSpeeds.Zero:
			return 0f;
		case EngineSpeeds.Fwd_Lo:
			return 0.2f;
		case EngineSpeeds.Fwd_Med:
			return 0.5f;
		case EngineSpeeds.Fwd_Hi:
			return 1f;
		default:
			return 0f;
		}
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
			int result = 0;
			Item fuelItem = fuelSystem.GetFuelItem();
			if (fuelItem != null)
			{
				result = fuelItem.amount;
			}
			return result;
		}
		return 0;
	}

	public bool PlayerIsInParentTrigger(BasePlayer player)
	{
		if (base.isServer)
		{
			if (platformParentTrigger.HasAnyEntityContents)
			{
				return platformParentTrigger.entityContents.Contains(player);
			}
			return false;
		}
		Vector3 position = player.transform.position + Vector3.up * player.GetHeight() * 0.5f;
		Vector3 vector = platformParentTriggerCollider.transform.InverseTransformPoint(position) - platformParentTriggerCollider.center;
		float num = platformParentTriggerCollider.size.x * 0.5f;
		float num2 = platformParentTriggerCollider.size.y * 0.5f;
		float num3 = platformParentTriggerCollider.size.z * 0.5f;
		if (vector.x < num && vector.x > 0f - num && vector.y < num2 && vector.y > 0f - num2 && vector.z < num3)
		{
			return vector.z > 0f - num3;
		}
		return false;
	}

	public bool CanMount(BasePlayer player)
	{
		return PlayerIsInParentTrigger(player);
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
