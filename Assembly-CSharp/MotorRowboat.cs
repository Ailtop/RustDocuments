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

public class MotorRowboat : BaseBoat
{
	[Header("Audio")]
	public BlendedSoundLoops engineLoops;

	public BlendedSoundLoops waterLoops;

	public SoundDefinition engineStartSoundDef;

	public SoundDefinition engineStopSoundDef;

	public SoundDefinition movementSplashAccentSoundDef;

	public SoundDefinition engineSteerSoundDef;

	public GameObjectRef pushLandEffect;

	public GameObjectRef pushWaterEffect;

	public float waterSpeedDivisor = 10f;

	public float turnPitchModScale = -0.25f;

	public float tiltPitchModScale = 0.3f;

	public float splashAccentFrequencyMin = 1f;

	public float splashAccentFrequencyMax = 10f;

	protected const Flags Flag_EngineOn = Flags.Reserved1;

	protected const Flags Flag_ThrottleOn = Flags.Reserved2;

	protected const Flags Flag_TurnLeft = Flags.Reserved3;

	protected const Flags Flag_TurnRight = Flags.Reserved4;

	protected const Flags Flag_Submerged = Flags.Reserved5;

	protected const Flags Flag_HasFuel = Flags.Reserved6;

	protected const Flags Flag_RecentlyPushed = Flags.Reserved8;

	public const float submergeFractionMinimum = 0.85f;

	[Header("Fuel")]
	public GameObjectRef fuelStoragePrefab;

	public float fuelPerSec;

	[Header("Storage")]
	public GameObjectRef storageUnitPrefab;

	public EntityRef storageUnitInstance;

	[Header("Effects")]
	public Transform boatRear;

	public ParticleSystemContainer wakeEffect;

	public ParticleSystemContainer engineEffectIdle;

	public ParticleSystemContainer engineEffectThrottle;

	public Projector causticsProjector;

	public Transform causticsDepthTest;

	public Transform engineLeftHandPosition;

	public Transform engineRotate;

	public Transform propellerRotate;

	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population = 1f;

	[ServerVar(Help = "How long before a boat loses all its health while outside. If it's in deep water, deepwaterdecayminutes is used")]
	public static float outsidedecayminutes = 180f;

	[ServerVar(Help = "How long before a boat loses all its health while in deep water")]
	public static float deepwaterdecayminutes = 120f;

	public EntityFuelSystem fuelSystem;

	public TimeSince timeSinceLastUsedFuel;

	public Transform[] stationaryDismounts;

	public Collider mainCollider;

	public float angularDragBase = 0.5f;

	public float angularDragVelocity = 0.5f;

	public float landDrag = 0.2f;

	public float waterDrag = 0.8f;

	public float offAxisDrag = 1f;

	public float offAxisDot = 0.25f;

	private const float DECAY_TICK_TIME = 60f;

	private TimeSince startedFlip;

	public float lastHadDriverTime;

	public bool dying;

	public const float maxVelForStationaryDismount = 4f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MotorRowboat.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1873751172 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_EngineToggle "));
				}
				using (TimeWarning.New("RPC_EngineToggle"))
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
							RPC_EngineToggle(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_EngineToggle");
					}
				}
				return true;
			}
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
							RPCMessage msg3 = rPCMessage;
							RPC_OpenFuel(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
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
		fuelSystem = new EntityFuelSystem(base.isServer, fuelStoragePrefab, children);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		timeSinceLastUsedFuel = 0f;
		InvokeRandomized(BoatDecay, UnityEngine.Random.Range(30f, 60f), 60f, 6f);
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer)
		{
			if (isSpawned)
			{
				fuelSystem.CheckNewChild(child);
			}
			if (child.prefabID == storageUnitPrefab.GetEntity().prefabID)
			{
				storageUnitInstance.Set(child);
			}
		}
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return fuelSystem;
	}

	public override int StartingFuelUnits()
	{
		return 50;
	}

	public void BoatDecay()
	{
		if (!dying)
		{
			BaseBoat.WaterVehicleDecay(this, 60f, timeSinceLastUsedFuel, outsidedecayminutes, deepwaterdecayminutes);
		}
	}

	public override void DoPushAction(BasePlayer player)
	{
		if (IsFlipped())
		{
			Vector3 vector = base.transform.InverseTransformPoint(player.transform.position);
			float num = 4f;
			if (vector.x > 0f)
			{
				num = 0f - num;
			}
			rigidBody.AddRelativeTorque(Vector3.forward * num, ForceMode.VelocityChange);
			rigidBody.AddForce(Vector3.up * 4f, ForceMode.VelocityChange);
			startedFlip = 0f;
			InvokeRepeatingFixedTime(FlipMonitor);
		}
		else
		{
			Vector3 vector2 = Vector3Ex.Direction2D(player.transform.position, base.transform.position);
			Vector3 vector3 = Vector3Ex.Direction2D(player.transform.position + player.eyes.BodyForward() * 3f, player.transform.position);
			vector3 = (Vector3.up * 0.1f + vector3).normalized;
			Vector3 position = base.transform.position + vector2 * 2f;
			float num2 = 3f;
			float value = Vector3.Dot(base.transform.forward, vector3);
			num2 += Mathf.InverseLerp(0.8f, 1f, value) * 3f;
			rigidBody.AddForceAtPosition(vector3 * num2, position, ForceMode.VelocityChange);
		}
		if (HasFlag(Flags.Reserved5))
		{
			if (pushWaterEffect.isValid)
			{
				Effect.server.Run(pushWaterEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
		else if (pushLandEffect.isValid)
		{
			Effect.server.Run(pushLandEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
	}

	private void FlipMonitor()
	{
		float num = Vector3.Dot(Vector3.up, base.transform.up);
		rigidBody.angularVelocity = Vector3.Lerp(rigidBody.angularVelocity, Vector3.zero, UnityEngine.Time.fixedDeltaTime * 8f * num);
		if ((float)startedFlip > 3f)
		{
			CancelInvokeFixedTime(FlipMonitor);
		}
	}

	[RPC_Server]
	public void RPC_OpenFuel(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && IsDriver(player))
		{
			fuelSystem.LootFuel(player);
		}
	}

	[RPC_Server]
	public void RPC_EngineToggle(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null))
		{
			bool flag = msg.read.Bit();
			if (InDryDock())
			{
				flag = false;
			}
			if (IsDriver(player) && flag != EngineOn())
			{
				EngineToggle(flag);
			}
		}
	}

	public void EngineToggle(bool wantsOn)
	{
		if (!fuelSystem.HasFuel(true))
		{
			return;
		}
		BasePlayer driver = GetDriver();
		if (!wantsOn || Interface.CallHook("OnEngineStart", this, driver) == null)
		{
			SetFlag(Flags.Reserved1, wantsOn);
			if (wantsOn)
			{
				Interface.CallHook("OnEngineStarted", this, driver);
			}
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(CheckInvalidBoat, 1f);
		if (base.health <= 0f)
		{
			Invoke(ActualDeath, vehicle.boat_corpse_seconds);
			buoyancy.buoyancyScale = 0f;
			dying = true;
		}
	}

	public void CheckInvalidBoat()
	{
		if (!fuelSystem.fuelStorageInstance.IsValid(base.isServer) || !storageUnitInstance.IsValid(base.isServer))
		{
			Debug.Log("Destroying invalid boat ");
			Invoke(ActualDeath, 1f);
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
	}

	public override bool EngineOn()
	{
		return HasFlag(Flags.Reserved1);
	}

	public float TimeSinceDriver()
	{
		return UnityEngine.Time.time - lastHadDriverTime;
	}

	public override void DriverInput(InputState inputState, BasePlayer player)
	{
		base.DriverInput(inputState, player);
		lastHadDriverTime = UnityEngine.Time.time;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float num = TimeSinceDriver();
		if (num > 15f)
		{
			steering += Mathf.InverseLerp(15f, 30f, num);
			steering = Mathf.Clamp(-1f, 1f, steering);
			if (num > 75f)
			{
				gasPedal = 0f;
			}
		}
		SetFlags();
		UpdateDrag();
		if (dying)
		{
			buoyancy.buoyancyScale = Mathf.Lerp(buoyancy.buoyancyScale, 0f, UnityEngine.Time.fixedDeltaTime * 0.1f);
		}
		else
		{
			float num2 = 1f;
			float value = rigidBody.velocity.Magnitude2D();
			float num3 = Mathf.InverseLerp(1f, 10f, value) * 0.5f * base.healthFraction;
			if (!EngineOn())
			{
				num3 = 0f;
			}
			float num4 = 1f - 0.3f * (1f - base.healthFraction);
			buoyancy.buoyancyScale = (num2 + num3) * num4;
		}
		if (EngineOn())
		{
			float num5 = (HasFlag(Flags.Reserved2) ? 1f : 0.0333f);
			fuelSystem.TryUseFuel(UnityEngine.Time.fixedDeltaTime * num5, fuelPerSec);
			timeSinceLastUsedFuel = 0f;
		}
	}

	private void SetFlags()
	{
		using (TimeWarning.New("SetFlag"))
		{
			bool b = EngineOn() && !IsFlipped() && base.healthFraction > 0f && fuelSystem.HasFuel() && TimeSinceDriver() < 75f;
			Flags num = flags;
			SetFlag(Flags.Reserved3, steering > 0f, false, false);
			SetFlag(Flags.Reserved4, steering < 0f, false, false);
			SetFlag(Flags.Reserved1, b, false, false);
			SetFlag(Flags.Reserved2, EngineOn() && gasPedal != 0f, false, false);
			SetFlag(Flags.Reserved5, buoyancy.submergedFraction > 0.85f, false, false);
			SetFlag(Flags.Reserved6, fuelSystem.HasFuel(), false, false);
			SetFlag(Flags.Reserved7, GetLocalVelocity().sqrMagnitude < 0.5f && !AnyMounted(), false, false);
			SetFlag(Flags.Reserved8, base.RecentlyPushed, false, false);
			if (num != flags)
			{
				Invoke(base.SendNetworkUpdate_Flags, 0f);
			}
		}
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return rigidBody.velocity;
	}

	public override void SeatClippedWorld(BaseMountable mountable)
	{
		BasePlayer mounted = mountable.GetMounted();
		if (!(mounted == null))
		{
			if (IsDriver(mounted))
			{
				steering = 0f;
				gasPedal = 0f;
			}
			float num = Mathf.InverseLerp(4f, 20f, rigidBody.velocity.magnitude);
			if (num > 0f)
			{
				mounted.Hurt(num * 100f, DamageType.Blunt, this, false);
			}
			if (mounted != null && mounted.isMounted)
			{
				base.SeatClippedWorld(mountable);
			}
		}
	}

	public void UpdateDrag()
	{
		float value = rigidBody.velocity.SqrMagnitude2D();
		float num = Mathf.InverseLerp(0f, 2f, value);
		rigidBody.angularDrag = angularDragBase + angularDragVelocity * num;
		rigidBody.drag = landDrag + waterDrag * Mathf.InverseLerp(0f, 1f, buoyancy.submergedFraction);
		if (offAxisDrag > 0f)
		{
			float value2 = Vector3.Dot(base.transform.forward, rigidBody.velocity.normalized);
			float num2 = Mathf.InverseLerp(0.98f, 0.92f, value2);
			rigidBody.drag += num2 * offAxisDrag * buoyancy.submergedFraction;
		}
	}

	public override void OnKilled(HitInfo info)
	{
		if (!dying)
		{
			dying = true;
			repair.enabled = false;
			Invoke(DismountAllPlayers, 10f);
			Invoke(ActualDeath, vehicle.boat_corpse_seconds);
		}
	}

	public void ActualDeath()
	{
		Kill(DestroyMode.Gib);
	}

	public override bool MountEligable(BasePlayer player)
	{
		if (dying)
		{
			return false;
		}
		if (rigidBody.velocity.magnitude >= 5f && HasDriver())
		{
			return false;
		}
		return base.MountEligable(player);
	}

	public override bool HasValidDismountPosition(BasePlayer player)
	{
		if (rigidBody.velocity.magnitude <= 4f)
		{
			Vector3 visualCheckOrigin = player.TriggerPoint();
			Transform[] array = stationaryDismounts;
			foreach (Transform transform in array)
			{
				if (ValidDismountPosition(transform.transform.position, visualCheckOrigin))
				{
					return true;
				}
			}
		}
		return base.HasValidDismountPosition(player);
	}

	public override bool GetDismountPosition(BasePlayer player, out Vector3 res)
	{
		if (rigidBody.velocity.magnitude <= 4f)
		{
			List<Vector3> obj = Facepunch.Pool.GetList<Vector3>();
			Vector3 visualCheckOrigin = player.TriggerPoint();
			Transform[] array = stationaryDismounts;
			foreach (Transform transform in array)
			{
				if (ValidDismountPosition(transform.transform.position, visualCheckOrigin))
				{
					obj.Add(transform.transform.position);
				}
			}
			if (obj.Count > 0)
			{
				Vector3 pos = player.transform.position;
				obj.Sort((Vector3 a, Vector3 b) => Vector3.Distance(a, pos).CompareTo(Vector3.Distance(b, pos)));
				res = obj[0];
				Facepunch.Pool.FreeList(ref obj);
				return true;
			}
			Facepunch.Pool.FreeList(ref obj);
		}
		return base.GetDismountPosition(player, out res);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.motorBoat = Facepunch.Pool.Get<Motorboat>();
		info.msg.motorBoat.storageid = storageUnitInstance.uid;
		info.msg.motorBoat.fuelStorageID = fuelSystem.fuelStorageInstance.uid;
	}

	public override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (!IsStationary() || (!(pusher.WaterFactor() <= 0.6f) && !IsFlipped()))
		{
			return false;
		}
		if (!IsFlipped() && pusher.IsStandingOnEntity(this, 8192))
		{
			return false;
		}
		if (Vector3.Distance(pusher.transform.position, base.transform.position) > 5f)
		{
			return false;
		}
		if (dying)
		{
			return false;
		}
		if (!pusher.isMounted && pusher.IsOnGround() && base.healthFraction > 0f)
		{
			return ShowPushMenu(pusher);
		}
		return false;
	}

	private bool ShowPushMenu(BasePlayer player)
	{
		if (!IsFlipped() && player.IsStandingOnEntity(this, 8192))
		{
			return false;
		}
		if (IsStationary())
		{
			if (!(player.WaterFactor() <= 0.6f))
			{
				return IsFlipped();
			}
			return true;
		}
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.motorBoat != null)
		{
			fuelSystem.fuelStorageInstance.uid = info.msg.motorBoat.fuelStorageID;
			storageUnitInstance.uid = info.msg.motorBoat.storageid;
		}
	}
}
