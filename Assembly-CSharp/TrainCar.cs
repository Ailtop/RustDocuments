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
using UnityEngine.Serialization;

public class TrainCar : BaseVehicle, TriggerHurtNotChild.IHurtTriggerUser, TrainTrackSpline.ITrainTrackUser, ITrainCollidable, IPrefabPreProcess
{
	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population = 0.6f;

	[ServerVar(Help = "Ratio of wagons to train engines that spawn")]
	public static int wagons_per_engine = 2;

	[ServerVar(Help = "Ratio of Work Carts with additional cover to standard Work Carts. 1.0 = All covered, 0.0 = all standard.")]
	public static float variant_ratio = 0.5f;

	protected bool trainDebug;

	public CompleteTrain completeTrain;

	public float frontBogieYRot;

	public float rearBogieYRot;

	public Vector3 spawnOrigin;

	public static float TRAINCAR_MAX_SPEED = 25f;

	public TrainTrackSpline _frontTrackSection;

	public float distFrontToBackWheel;

	public float initialSpawnTime;

	public float decayDuration = 1200f;

	public float decayTickSpacing = 60f;

	public float lastDecayTick;

	public float decayingFor;

	[Header("Train Car")]
	[SerializeField]
	public float corpseSeconds = 60f;

	[SerializeField]
	public TriggerTrainCollisions frontCollisionTrigger;

	[SerializeField]
	public TriggerTrainCollisions rearCollisionTrigger;

	[SerializeField]
	public float collisionDamageDivide = 100000f;

	[SerializeField]
	public float derailCollisionForce = 130000f;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerFront;

	[SerializeField]
	public TriggerHurtNotChild hurtTriggerRear;

	[SerializeField]
	public float hurtTriggerMinSpeed = 1f;

	[SerializeField]
	public Transform centreOfMassTransform;

	[SerializeField]
	public Transform frontBogiePivot;

	[SerializeField]
	public bool frontBogieCanRotate = true;

	[SerializeField]
	public Transform rearBogiePivot;

	[SerializeField]
	public bool rearBogieCanRotate = true;

	[SerializeField]
	private Transform[] wheelVisuals;

	[SerializeField]
	public float wheelRadius = 0.615f;

	[FormerlySerializedAs("fxFinalExplosion")]
	[SerializeField]
	public GameObjectRef fxDestroyed;

	[SerializeField]
	public TriggerParent platformParentTrigger;

	public GameObjectRef collisionEffect;

	public Transform frontCoupling;

	public Transform frontCouplingPivot;

	public Transform rearCoupling;

	public Transform rearCouplingPivot;

	[SerializeField]
	public SoundDefinition coupleSound;

	[SerializeField]
	private SoundDefinition uncoupleSound;

	[SerializeField]
	private TrainCarAudio trainCarAudio;

	[FormerlySerializedAs("frontCoupleFx")]
	[SerializeField]
	private ParticleSystem frontCouplingChangedFx;

	[FormerlySerializedAs("rearCoupleFx")]
	[SerializeField]
	private ParticleSystem rearCouplingChangedFx;

	[FormerlySerializedAs("fxCoupling")]
	[SerializeField]
	private ParticleSystem newCouplingFX;

	[SerializeField]
	[ReadOnly]
	public Vector3 frontBogieLocalOffset;

	[SerializeField]
	[ReadOnly]
	public Vector3 rearBogieLocalOffset;

	[ReadOnly]
	public float DistFrontWheelToFrontCoupling;

	[ReadOnly]
	public float DistFrontWheelToBackCoupling;

	public TrainCouplingController coupling;

	[NonSerialized]
	public TrainTrackSpline.TrackSelection localTrackSelection;

	public Vector3 Position => base.transform.position;

	public float FrontWheelSplineDist { get; set; }

	public virtual bool IsEngine => false;

	protected virtual bool networkUpdateOnCompleteTrainChange => false;

	public TrainTrackSpline FrontTrackSection
	{
		get
		{
			return _frontTrackSection;
		}
		set
		{
			if (_frontTrackSection != value)
			{
				if (_frontTrackSection != null)
				{
					_frontTrackSection.DeregisterTrackUser(this);
				}
				_frontTrackSection = value;
				if (_frontTrackSection != null)
				{
					_frontTrackSection.RegisterTrackUser(this);
				}
			}
		}
	}

	public TrainTrackSpline RearTrackSection { get; set; }

	public bool IsAtAStation
	{
		get
		{
			if (FrontTrackSection != null)
			{
				return FrontTrackSection.isStation;
			}
			return false;
		}
	}

	protected bool IsOnAboveGroundSpawnRail
	{
		get
		{
			if (FrontTrackSection != null)
			{
				return FrontTrackSection.aboveGroundSpawn;
			}
			return false;
		}
	}

	public bool RecentlySpawned => UnityEngine.Time.time < initialSpawnTime + 2f;

	public TriggerTrainCollisions FrontCollisionTrigger => frontCollisionTrigger;

	public TriggerTrainCollisions RearCollisionTrigger => rearCollisionTrigger;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TrainCar.OnRpcMessage"))
		{
			if (rpc == 3930273067u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_WantsUncouple "));
				}
				using (TimeWarning.New("RPC_WantsUncouple"))
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
							RPC_WantsUncouple(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_WantsUncouple");
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
		spawnOrigin = base.transform.position;
		distFrontToBackWheel = Vector3.Distance(GetFrontWheelPos(), GetRearWheelPos());
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		UpdateCompleteTrain();
		InvokeRandomized(UpdateClients, 0f, 0.15f, 0.02f);
		InvokeRandomized(DecayTick, UnityEngine.Random.Range(20f, 40f), decayTickSpacing, decayTickSpacing * 0.1f);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.health <= 0f)
		{
			ActualDeath();
			return;
		}
		SetFlag(Flags.Reserved2, b: false);
		SetFlag(Flags.Reserved3, b: false);
	}

	public override void Spawn()
	{
		base.Spawn();
		initialSpawnTime = UnityEngine.Time.time;
		if (TrainTrackSpline.TryFindTrackNearby(GetFrontWheelPos(), 2f, out var splineResult, out var distResult) && (Rust.Application.isLoadingSave || splineResult.HasClearTrackSpaceNear(this)))
		{
			FrontWheelSplineDist = distResult;
			Vector3 tangent;
			Vector3 positionAndTangent = splineResult.GetPositionAndTangent(FrontWheelSplineDist, base.transform.forward, out tangent);
			SetTheRestFromFrontWheelData(ref splineResult, positionAndTangent, tangent, localTrackSelection, null);
			FrontTrackSection = splineResult;
		}
		else
		{
			Invoke(base.KillMessage, 0f);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseTrain = Facepunch.Pool.Get<BaseTrain>();
		info.msg.baseTrain.time = GetNetworkTime();
		info.msg.baseTrain.frontBogieYRot = frontBogieYRot;
		info.msg.baseTrain.rearBogieYRot = rearBogieYRot;
		if (coupling.frontCoupling.TryGetCoupledToID(out var id))
		{
			info.msg.baseTrain.frontCouplingID = id;
			info.msg.baseTrain.frontCouplingToFront = coupling.frontCoupling.CoupledTo.isFrontCoupling;
		}
		if (coupling.rearCoupling.TryGetCoupledToID(out id))
		{
			info.msg.baseTrain.rearCouplingID = id;
			info.msg.baseTrain.rearCouplingToFront = coupling.rearCoupling.CoupledTo.isFrontCoupling;
		}
	}

	public virtual void ServerFlagsChanged(Flags old, Flags next)
	{
		if (isSpawned && (next.HasFlag(Flags.Reserved2) != old.HasFlag(Flags.Reserved2) || next.HasFlag(Flags.Reserved3) != old.HasFlag(Flags.Reserved3)))
		{
			UpdateCompleteTrain();
		}
	}

	private void UpdateCompleteTrain()
	{
		List<TrainCar> result = Facepunch.Pool.GetList<TrainCar>();
		coupling.GetAll(ref result);
		if (completeTrain == null || !completeTrain.Matches(result))
		{
			SetNewCompleteTrain(new CompleteTrain(result));
		}
		else
		{
			Facepunch.Pool.FreeList(ref result);
		}
	}

	public void SetNewCompleteTrain(CompleteTrain ct)
	{
		if (completeTrain != ct)
		{
			RemoveFromCompleteTrain();
			completeTrain = ct;
			if (networkUpdateOnCompleteTrainChange)
			{
				SendNetworkUpdate();
			}
		}
	}

	public override void Hurt(HitInfo info)
	{
		if (!RecentlySpawned)
		{
			base.Hurt(info);
		}
	}

	public override void OnKilled(HitInfo info)
	{
		float num = info.damageTypes.Get(DamageType.AntiVehicle);
		float num2 = info.damageTypes.Get(DamageType.Explosion);
		float num3 = info.damageTypes.Total();
		if ((num + num2) / num3 > 0.5f || vehicle.cinematictrains || corpseSeconds == 0f)
		{
			if (HasDriver())
			{
				GetDriver().Hurt(float.MaxValue);
			}
			base.OnKilled(info);
		}
		else
		{
			Invoke(ActualDeath, corpseSeconds);
		}
		if (base.IsDestroyed && fxDestroyed.isValid)
		{
			Effect.server.Run(fxDestroyed.resourcePath, GetExplosionPos(), Vector3.up, null, broadcast: true);
		}
	}

	public virtual Vector3 GetExplosionPos()
	{
		return base.transform.position + base.transform.rotation * bounds.center;
	}

	public void ActualDeath()
	{
		Kill(DestroyMode.Gib);
	}

	public override void DoRepair(BasePlayer player)
	{
		base.DoRepair(player);
		if (IsDead() && Health() > 0f)
		{
			CancelInvoke(ActualDeath);
			lifestate = LifeState.Alive;
		}
	}

	public float GetPlayerDamageMultiplier()
	{
		return Mathf.Abs(GetTrackSpeed()) * 1f;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
	}

	public override void DoServerDestroy()
	{
		if (FrontTrackSection != null)
		{
			FrontTrackSection.DeregisterTrackUser(this);
		}
		coupling.Uncouple(front: true);
		coupling.Uncouple(front: false);
		RemoveFromCompleteTrain();
		base.DoServerDestroy();
	}

	private void RemoveFromCompleteTrain()
	{
		if (completeTrain != null)
		{
			if (completeTrain.ContainsOnly(this))
			{
				completeTrain.Dispose();
				completeTrain = null;
			}
			else
			{
				completeTrain.RemoveTrainCar(this);
			}
		}
	}

	public override bool MountEligable(BasePlayer player)
	{
		if (IsDead())
		{
			return false;
		}
		return base.MountEligable(player);
	}

	public override float MaxVelocity()
	{
		return TRAINCAR_MAX_SPEED;
	}

	public float GetTrackSpeed()
	{
		if (completeTrain == null)
		{
			return 0f;
		}
		return completeTrain.GetTrackSpeedFor(this);
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return base.transform.forward * GetTrackSpeed();
	}

	public bool AnyPlayersOnTrainCar()
	{
		if (AnyMounted())
		{
			return true;
		}
		if (platformParentTrigger != null && platformParentTrigger.HasAnyEntityContents)
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

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (completeTrain != null)
		{
			completeTrain.UpdateTick(UnityEngine.Time.fixedDeltaTime);
			float trackSpeed = GetTrackSpeed();
			hurtTriggerFront.gameObject.SetActive(!coupling.IsFrontCoupled && trackSpeed > hurtTriggerMinSpeed);
			hurtTriggerRear.gameObject.SetActive(!coupling.IsRearCoupled && trackSpeed < 0f - hurtTriggerMinSpeed);
		}
	}

	public override void PostVehicleFixedUpdate()
	{
		base.PostVehicleFixedUpdate();
		if (completeTrain != null)
		{
			completeTrain.ResetUpdateTick();
		}
	}

	public Vector3 GetFrontOfTrainPos()
	{
		return base.transform.position + base.transform.rotation * (bounds.center + Vector3.forward * bounds.extents.z);
	}

	public Vector3 GetRearOfTrainPos()
	{
		return base.transform.position + base.transform.rotation * (bounds.center - Vector3.forward * bounds.extents.z);
	}

	public void FrontTrainCarTick(TrainTrackSpline.TrackSelection trackSelection, float dt)
	{
		float distToMove = GetTrackSpeed() * dt;
		TrainTrackSpline preferredAltTrack = ((RearTrackSection != FrontTrackSection) ? RearTrackSection : null);
		MoveFrontWheelsAlongTrackSpline(FrontTrackSection, FrontWheelSplineDist, distToMove, preferredAltTrack, trackSelection);
	}

	public void OtherTrainCarTick(TrainTrackSpline theirTrackSpline, float prevSplineDist, float distanceOffset)
	{
		MoveFrontWheelsAlongTrackSpline(theirTrackSpline, prevSplineDist, distanceOffset, FrontTrackSection, TrainTrackSpline.TrackSelection.Default);
	}

	public void MoveFrontWheelsAlongTrackSpline(TrainTrackSpline trackSpline, float prevSplineDist, float distToMove, TrainTrackSpline preferredAltTrack, TrainTrackSpline.TrackSelection trackSelection)
	{
		FrontWheelSplineDist = trackSpline.GetSplineDistAfterMove(prevSplineDist, base.transform.forward, distToMove, trackSelection, out var onSpline, out var _, preferredAltTrack, null);
		Vector3 tangent;
		Vector3 positionAndTangent = onSpline.GetPositionAndTangent(FrontWheelSplineDist, base.transform.forward, out tangent);
		SetTheRestFromFrontWheelData(ref onSpline, positionAndTangent, tangent, trackSelection, trackSpline);
		FrontTrackSection = onSpline;
	}

	public Vector3 GetFrontWheelPos()
	{
		return base.transform.position + base.transform.rotation * frontBogieLocalOffset;
	}

	public Vector3 GetRearWheelPos()
	{
		return base.transform.position + base.transform.rotation * rearBogieLocalOffset;
	}

	public void SetTheRestFromFrontWheelData(ref TrainTrackSpline frontTS, Vector3 targetFrontWheelPos, Vector3 targetFrontWheelTangent, TrainTrackSpline.TrackSelection trackSelection, TrainTrackSpline additionalAlt)
	{
		TrainTrackSpline onSpline;
		bool atEndOfLine;
		float splineDistAfterMove = frontTS.GetSplineDistAfterMove(FrontWheelSplineDist, base.transform.forward, 0f - distFrontToBackWheel, trackSelection, out onSpline, out atEndOfLine, RearTrackSection, additionalAlt);
		Vector3 tangent;
		Vector3 positionAndTangent = onSpline.GetPositionAndTangent(splineDistAfterMove, base.transform.forward, out tangent);
		if (atEndOfLine)
		{
			FrontWheelSplineDist = onSpline.GetSplineDistAfterMove(splineDistAfterMove, base.transform.forward, distFrontToBackWheel, trackSelection, out frontTS, out var _, onSpline, additionalAlt);
			targetFrontWheelPos = frontTS.GetPositionAndTangent(FrontWheelSplineDist, base.transform.forward, out targetFrontWheelTangent);
		}
		RearTrackSection = onSpline;
		Vector3 normalized = (targetFrontWheelPos - positionAndTangent).normalized;
		Vector3 vector = targetFrontWheelPos - Quaternion.LookRotation(normalized) * frontBogieLocalOffset;
		rigidBody.MovePosition(vector);
		if (normalized.magnitude == 0f)
		{
			rigidBody.MoveRotation(Quaternion.identity);
		}
		else
		{
			rigidBody.MoveRotation(Quaternion.LookRotation(normalized));
		}
		frontBogieYRot = Vector3.SignedAngle(base.transform.forward, targetFrontWheelTangent, base.transform.up);
		rearBogieYRot = Vector3.SignedAngle(base.transform.forward, tangent, base.transform.up);
		if (UnityEngine.Application.isEditor)
		{
			Debug.DrawLine(targetFrontWheelPos, positionAndTangent, Color.magenta, 0.2f);
			Debug.DrawLine(rigidBody.position, vector, Color.yellow, 0.2f);
			Debug.DrawRay(vector, Vector3.up, Color.yellow, 0.2f);
		}
	}

	public virtual float GetForces()
	{
		float num = base.transform.localEulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		return 0f + num / 90f * (0f - UnityEngine.Physics.gravity.y) * RealisticMass;
	}

	public float ApplyCollisionDamage(float forceMagnitude)
	{
		float num = ((!(forceMagnitude > derailCollisionForce)) ? (Mathf.Pow(forceMagnitude, 1.4f) / collisionDamageDivide) : float.MaxValue);
		Hurt(num, DamageType.Collision, this, useProtection: false);
		return num;
	}

	public void UpdateClients()
	{
		if (IsMoving())
		{
			ClientRPC(null, "BaseTrainUpdate", GetNetworkTime(), frontBogieYRot, rearBogieYRot);
		}
	}

	public void DecayTick()
	{
		if (completeTrain == null)
		{
			return;
		}
		bool flag = HasDriver() || completeTrain.AnyPlayersOnTrain();
		if (flag)
		{
			decayingFor = 0f;
		}
		bool num = (!IsAtAStation || !(Vector3.Distance(spawnOrigin, base.transform.position) < 50f)) && !IsOnAboveGroundSpawnRail && !flag && !AnyPlayersNearby(30f);
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num2 = realtimeSinceStartup - lastDecayTick;
		lastDecayTick = realtimeSinceStartup;
		if (num)
		{
			decayingFor += num2;
			if (decayingFor >= decayDuration && (IsEngine || !completeTrain.IncludesAnEngine()))
			{
				ActualDeath();
			}
		}
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

	[RPC_Server]
	public void RPC_WantsUncouple(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && !(Vector3.SqrMagnitude(base.transform.position - player.transform.position) > 200f) && Interface.CallHook("OnTrainCarUncouple", this, msg.player) == null)
		{
			bool front = msg.read.Bit();
			coupling.Uncouple(front);
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(process, rootObj, name, serverside, clientside, bundling);
		frontBogieLocalOffset = base.transform.InverseTransformPoint(frontBogiePivot.position);
		float num = ((!(frontCoupling != null)) ? (bounds.extents.z + bounds.center.z) : base.transform.InverseTransformPoint(frontCoupling.position).z);
		float num2 = ((!(rearCoupling != null)) ? (0f - bounds.extents.z + bounds.center.z) : base.transform.InverseTransformPoint(rearCoupling.position).z);
		DistFrontWheelToFrontCoupling = num - frontBogieLocalOffset.z;
		DistFrontWheelToBackCoupling = 0f - num2 + frontBogieLocalOffset.z;
		rearBogieLocalOffset = base.transform.InverseTransformPoint(rearBogiePivot.position);
	}

	public override void InitShared()
	{
		base.InitShared();
		coupling = new TrainCouplingController(this);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.baseTrain != null && base.isServer)
		{
			frontBogieYRot = info.msg.baseTrain.frontBogieYRot;
			rearBogieYRot = info.msg.baseTrain.rearBogieYRot;
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

	public bool CustomCollision(TrainCar train, TriggerTrainCollisions trainTrigger)
	{
		return false;
	}

	public override float InheritedVelocityScale()
	{
		return 0.5f;
	}

	public virtual void SetTrackSelection(TrainTrackSpline.TrackSelection trackSelection)
	{
		if (localTrackSelection != trackSelection)
		{
			localTrackSelection = trackSelection;
			if (base.isServer)
			{
				ClientRPC(null, "SetTrackSelection", (sbyte)localTrackSelection);
			}
		}
	}
}
