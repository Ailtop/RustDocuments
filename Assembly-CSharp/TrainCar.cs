using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Serialization;

public class TrainCar : BaseVehicle, TriggerHurtNotChild.IHurtTriggerUser, TrainTrackSpline.ITrainTrackUser, ITrainCollidable, IPrefabPreProcess
{
	public enum StaticCollisionState
	{
		Free = 0,
		StaticColliding = 1,
		StayingStill = 2
	}

	public StaticCollisionState staticCollidingAtFront;

	public StaticCollisionState staticCollidingAtRear;

	private const float MIN_COLLISION_FORCE = 70000f;

	public float nextCollisionFXTime;

	private const float MIN_TIME_BETWEEN_COLLISION_FX = 0.5f;

	public Dictionary<Rigidbody, float> prevTrackSpeeds = new Dictionary<Rigidbody, float>();

	protected bool trainDebug;

	public float frontBogieYRot;

	public float rearBogieYRot;

	private TrainTrackSpline _frontTrackSection;

	public float lastMovingTime = float.MinValue;

	private const float SLEEP_SPEED = 0.25f;

	private const float SLEEP_DELAY = 10f;

	public float distFrontToBackWheel;

	public float initialSpawnTime;

	[Header("Base Train")]
	[SerializeField]
	public float corpseSeconds = 60f;

	[SerializeField]
	public TriggerTrainCollisions frontCollisionTrigger;

	[SerializeField]
	public TriggerTrainCollisions rearCollisionTrigger;

	[Tooltip("How much impact energy is retained on collisions. 1.0 = 100% retained, 0.0 = 100% loss of energy")]
	[SerializeField]
	public float impactEnergyFraction = 0.75f;

	[SerializeField]
	public float collisionDamageDivide = 100000f;

	[SerializeField]
	public float derailCollisionForce = 130000f;

	[SerializeField]
	public GameObjectRef collisionEffect;

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
	private float wheelRadius = 0.615f;

	[SerializeField]
	private ParticleSystemContainer[] sparks;

	[FormerlySerializedAs("brakeSparkLights")]
	[SerializeField]
	private Light[] sparkLights;

	[SerializeField]
	[ReadOnly]
	private Vector3 frontBogieLocalOffset;

	[SerializeField]
	[ReadOnly]
	public Vector3 rearBogieLocalOffset;

	public TrainTrackSpline.TrackSelection curTrackSelection;

	public float TrackSpeed { get; set; }

	public Vector3 Position => base.transform.position;

	public float FrontWheelSplineDist { get; private set; }

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

	public bool RecentlySpawned => UnityEngine.Time.time < initialSpawnTime + 2f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("TrainCar.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void ReduceSpeedBy(float velChange)
	{
		if (TrackSpeed > 0f)
		{
			TrackSpeed = Mathf.Max(0f, TrackSpeed - velChange);
		}
		else if (TrackSpeed < 0f)
		{
			TrackSpeed = Mathf.Min(0f, TrackSpeed + velChange);
		}
	}

	public float GetTotalPushingForces(Vector3 pushDirection, List<TrainCar> prevTrains = null)
	{
		if (prevTrains == null)
		{
			prevTrains = Facepunch.Pool.GetList<TrainCar>();
		}
		else if (prevTrains.Contains(this))
		{
			Debug.LogWarning("GetTotalPushingForces: Recursive loop detected. Bailing out.");
			Facepunch.Pool.FreeList(ref prevTrains);
			return 0f;
		}
		prevTrains.Add(this);
		bool num = Vector3.Dot(base.transform.forward, pushDirection) >= 0f;
		TriggerTrainCollisions triggerTrainCollisions = (num ? frontCollisionTrigger : rearCollisionTrigger);
		float num2 = GetEngineForces();
		if (!num)
		{
			num2 *= -1f;
		}
		foreach (TrainCar trainContent in triggerTrainCollisions.trainContents)
		{
			if (!(trainContent == this))
			{
				num2 += trainContent.GetTotalPushingForces(pushDirection, prevTrains);
			}
		}
		Facepunch.Pool.FreeList(ref prevTrains);
		return num2;
	}

	public void FreeStaticCollision()
	{
		staticCollidingAtFront = StaticCollisionState.Free;
		staticCollidingAtRear = StaticCollisionState.Free;
	}

	public float ApplyCollisionsToTrackSpeed(float trackSpeed, float deltaTime)
	{
		trackSpeed = ApplyCollisions(trackSpeed, atOurFront: true, frontCollisionTrigger, ref staticCollidingAtFront, deltaTime);
		trackSpeed = ApplyCollisions(trackSpeed, atOurFront: false, rearCollisionTrigger, ref staticCollidingAtRear, deltaTime);
		Rigidbody rigidbody = null;
		foreach (KeyValuePair<Rigidbody, float> prevTrackSpeed in prevTrackSpeeds)
		{
			if (prevTrackSpeed.Key == null || (!frontCollisionTrigger.otherRigidbodyContents.Contains(prevTrackSpeed.Key) && !rearCollisionTrigger.otherRigidbodyContents.Contains(prevTrackSpeed.Key)))
			{
				rigidbody = prevTrackSpeed.Key;
				break;
			}
		}
		if (rigidbody != null)
		{
			prevTrackSpeeds.Remove(rigidbody);
		}
		return trackSpeed;
	}

	public float ApplyCollisions(float trackSpeed, bool atOurFront, TriggerTrainCollisions trigger, ref StaticCollisionState wasStaticColliding, float deltaTime)
	{
		bool hasAnyStaticContents = trigger.HasAnyStaticContents;
		float num = (hasAnyStaticContents ? (rigidBody.velocity.magnitude * rigidBody.mass) : 0f);
		trackSpeed = HandleStaticCollisions(hasAnyStaticContents, atOurFront, trackSpeed, ref wasStaticColliding);
		if (!hasAnyStaticContents)
		{
			foreach (TrainCar trainContent in trigger.trainContents)
			{
				trackSpeed = HandleTrainCollision(atOurFront, trackSpeed, trainContent, deltaTime, ref wasStaticColliding);
				num += Vector3.Magnitude(trainContent.rigidBody.velocity - rigidBody.velocity) * trainContent.rigidBody.mass;
			}
			foreach (Rigidbody otherRigidbodyContent in trigger.otherRigidbodyContents)
			{
				trackSpeed = HandleRigidbodyCollision(atOurFront, trackSpeed, otherRigidbodyContent, otherRigidbodyContent.mass, deltaTime, calcSecondaryForces: true);
				num += Vector3.Magnitude(otherRigidbodyContent.velocity - rigidBody.velocity) * otherRigidbodyContent.mass;
			}
		}
		if (ApplyCollisionDamage(num) > 5f && collisionEffect.isValid && UnityEngine.Time.time > nextCollisionFXTime)
		{
			foreach (Collider colliderContent in trigger.colliderContents)
			{
				Effect.server.Run(collisionEffect.resourcePath, colliderContent.ClosestPointOnBounds(base.transform.position), base.transform.up);
			}
			nextCollisionFXTime = UnityEngine.Time.time + 0.5f;
		}
		return trackSpeed;
	}

	public float HandleStaticCollisions(bool staticColliding, bool front, float trackSpeed, ref StaticCollisionState wasStaticColliding)
	{
		float num = (front ? (-5f) : 5f);
		if (staticColliding && (front ? (trackSpeed > num) : (trackSpeed < num)))
		{
			trackSpeed = num;
			wasStaticColliding = StaticCollisionState.StaticColliding;
		}
		else if (wasStaticColliding == StaticCollisionState.StaticColliding)
		{
			trackSpeed = 0f;
			wasStaticColliding = StaticCollisionState.StayingStill;
		}
		else if (wasStaticColliding == StaticCollisionState.StayingStill)
		{
			if (front ? (trackSpeed > 0.01f) : (trackSpeed < -0.01f))
			{
				trackSpeed = 0f;
			}
			else
			{
				wasStaticColliding = StaticCollisionState.Free;
			}
		}
		return trackSpeed;
	}

	public float HandleTrainCollision(bool front, float trackSpeed, TrainCar theirTrain, float deltaTime, ref StaticCollisionState wasStaticColliding)
	{
		Vector3 pushDirection = (front ? base.transform.forward : (-base.transform.forward));
		float num = Vector3.Angle(base.transform.forward, theirTrain.transform.forward);
		float f = Vector3.Dot(rhs: (theirTrain.transform.position - base.transform.position).normalized, lhs: base.transform.forward);
		if ((num > 30f && num < 150f) || Mathf.Abs(f) < 0.975f)
		{
			trackSpeed = (front ? (-0.5f) : 0.5f);
		}
		else
		{
			float totalPushingMass = theirTrain.GetTotalPushingMass(pushDirection);
			trackSpeed = ((!(totalPushingMass < 0f)) ? HandleRigidbodyCollision(front, trackSpeed, theirTrain.rigidBody, totalPushingMass, deltaTime, calcSecondaryForces: false) : HandleStaticCollisions(staticColliding: true, front, trackSpeed, ref wasStaticColliding));
			float num2 = theirTrain.GetTotalPushingForces(pushDirection);
			if (!front)
			{
				num2 *= -1f;
			}
			trackSpeed += num2 / rigidBody.mass * deltaTime;
		}
		return trackSpeed;
	}

	public float HandleRigidbodyCollision(bool atOurFront, float trackSpeed, Rigidbody theirRB, float theirTotalMass, float deltaTime, bool calcSecondaryForces)
	{
		float num = Vector3.Dot(base.transform.forward, theirRB.velocity);
		float num2 = trackSpeed - num;
		if ((atOurFront && num2 <= 0f) || (!atOurFront && num2 >= 0f))
		{
			return trackSpeed;
		}
		float num3 = num2 / deltaTime * theirTotalMass * impactEnergyFraction;
		if (calcSecondaryForces)
		{
			if (prevTrackSpeeds.ContainsKey(theirRB))
			{
				float num4 = num2 / deltaTime * rigidBody.mass * impactEnergyFraction / theirTotalMass * deltaTime;
				float num5 = prevTrackSpeeds[theirRB] - num;
				num3 -= Mathf.Clamp((num5 - num4) * rigidBody.mass, 0f, 1000000f);
				prevTrackSpeeds[theirRB] = num;
			}
			else
			{
				prevTrackSpeeds.Add(theirRB, num);
			}
		}
		float num6 = num3 / rigidBody.mass * deltaTime;
		trackSpeed -= num6;
		return trackSpeed;
	}

	public float ApplyCollisionDamage(float forceMagnitude)
	{
		if (forceMagnitude < 70000f)
		{
			return 0f;
		}
		float num = ((!(forceMagnitude > derailCollisionForce)) ? (Mathf.Pow(forceMagnitude, 1.4f) / collisionDamageDivide) : float.MaxValue);
		Hurt(num, DamageType.Collision, this, useProtection: false);
		return num;
	}

	public bool HasAnyCollisions()
	{
		if (!frontCollisionTrigger.HasAnyContents)
		{
			return rearCollisionTrigger.HasAnyContents;
		}
		return true;
	}

	public float GetTotalPushingMass(Vector3 pushDirection, List<TrainCar> prevTrains = null)
	{
		if (prevTrains == null)
		{
			prevTrains = Facepunch.Pool.GetList<TrainCar>();
		}
		else if (prevTrains.Contains(this))
		{
			Debug.LogWarning("GetTotalPushingMass: Recursive loop detected. Bailing out.");
			Facepunch.Pool.FreeList(ref prevTrains);
			return 0f;
		}
		prevTrains.Add(this);
		bool flag = Vector3.Dot(base.transform.forward, pushDirection) >= 0f;
		if ((flag ? staticCollidingAtFront : staticCollidingAtRear) != 0)
		{
			Facepunch.Pool.FreeList(ref prevTrains);
			return -1f;
		}
		TriggerTrainCollisions triggerTrainCollisions = (flag ? frontCollisionTrigger : rearCollisionTrigger);
		float num = rigidBody.mass;
		foreach (TrainCar trainContent in triggerTrainCollisions.trainContents)
		{
			if (!(trainContent == this))
			{
				float totalPushingMass = trainContent.GetTotalPushingMass(pushDirection, prevTrains);
				if (totalPushingMass < 0f)
				{
					Facepunch.Pool.FreeList(ref prevTrains);
					return -1f;
				}
				num += totalPushingMass;
			}
		}
		foreach (Rigidbody otherRigidbodyContent in triggerTrainCollisions.otherRigidbodyContents)
		{
			num += otherRigidbodyContent.mass;
		}
		Facepunch.Pool.FreeList(ref prevTrains);
		return num;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		distFrontToBackWheel = Vector3.Distance(GetFrontWheelPos(), GetRearWheelPos());
		lastMovingTime = UnityEngine.Time.time;
		rigidBody.centerOfMass = centreOfMassTransform.localPosition;
		InvokeRandomized(UpdateClients, 0f, 0.15f, 0.02f);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.health <= 0f)
		{
			ActualDeath();
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		initialSpawnTime = UnityEngine.Time.time;
		if (TrainTrackSpline.TryFindTrackNearby(GetFrontWheelPos(), 2f, out var splineResult, out var distResult) && splineResult.HasClearTrackSpaceNear(this))
		{
			FrontWheelSplineDist = distResult;
			Vector3 tangent;
			Vector3 positionAndTangent = splineResult.GetPositionAndTangent(FrontWheelSplineDist, base.transform.forward, out tangent);
			SetTheRestFromFrontWheelData(ref splineResult, positionAndTangent, tangent);
			FrontTrackSection = splineResult;
		}
		else
		{
			Kill();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.baseTrain = Facepunch.Pool.Get<BaseTrain>();
		info.msg.baseTrain.time = GetNetworkTime();
		info.msg.baseTrain.frontBogieYRot = frontBogieYRot;
		info.msg.baseTrain.rearBogieYRot = rearBogieYRot;
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
		if ((num + num2) / num3 > 0.5f || vehicle.cinematictrains)
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
		return Mathf.Abs(TrackSpeed) * 1f;
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
		base.DoServerDestroy();
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
		return 25f;
	}

	public override Vector3 GetLocalVelocityServer()
	{
		return base.transform.forward * TrackSpeed;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		float num = 0f;
		if (!rigidBody.IsSleeping() || IsOn() || HasAnyCollisions())
		{
			num = FixedUpdateMoveTrain(UnityEngine.Time.fixedDeltaTime);
		}
		hurtTriggerFront.gameObject.SetActive(num > hurtTriggerMinSpeed);
		hurtTriggerRear.gameObject.SetActive(num < 0f - hurtTriggerMinSpeed);
	}

	public float FixedUpdateMoveTrain(float deltaTime)
	{
		if (!IsFullySpawned())
		{
			return 0f;
		}
		if (rigidBody.IsSleeping())
		{
			rigidBody.WakeUp();
			lastMovingTime = UnityEngine.Time.time;
		}
		float num = FixedUpdateTrainOnTrack(deltaTime);
		if (Mathf.Abs(num) > 0.25f || Mathf.Abs(rigidBody.angularVelocity.magnitude) > 0.25f)
		{
			lastMovingTime = UnityEngine.Time.time;
		}
		if (!HasDriver() && !HasAnyCollisions() && UnityEngine.Time.time > lastMovingTime + 10f)
		{
			rigidBody.Sleep();
		}
		return num;
	}

	public Vector3 GetFrontOfTrainPos()
	{
		return base.transform.position + base.transform.rotation * (bounds.center + Vector3.forward * bounds.extents.z);
	}

	public Vector3 GetRearOfTrainPos()
	{
		return base.transform.position + base.transform.rotation * (bounds.center - Vector3.forward * bounds.extents.z);
	}

	public Vector3 GetFrontWheelPos()
	{
		return base.transform.position + base.transform.rotation * frontBogieLocalOffset;
	}

	public Vector3 GetRearWheelPos()
	{
		return base.transform.position + base.transform.rotation * rearBogieLocalOffset;
	}

	public float FixedUpdateTrainOnTrack(float deltaTime)
	{
		float engineForces = GetEngineForces();
		TrackSpeed += engineForces / rigidBody.mass * deltaTime;
		if (TrackSpeed > 0f)
		{
			TrackSpeed -= rigidBody.drag * 5f * deltaTime;
			if (TrackSpeed < 0f)
			{
				TrackSpeed = 0f;
			}
		}
		else if (TrackSpeed < 0f)
		{
			TrackSpeed += rigidBody.drag * 5f * deltaTime;
			if (TrackSpeed > 0f)
			{
				TrackSpeed = 0f;
			}
		}
		float num = base.transform.localEulerAngles.x;
		if (num > 180f)
		{
			num -= 360f;
		}
		float num2 = num / 90f * UnityEngine.Physics.gravity.y;
		TrackSpeed -= num2 * deltaTime;
		TrackSpeed = ApplyCollisionsToTrackSpeed(TrackSpeed, deltaTime);
		float distMoved = TrackSpeed * deltaTime;
		TrainTrackSpline preferredAltTrack = ((RearTrackSection != FrontTrackSection) ? RearTrackSection : null);
		FrontWheelSplineDist = FrontTrackSection.GetSplineDistAfterMove(FrontWheelSplineDist, base.transform.forward, distMoved, curTrackSelection, out var onSpline, out var _, preferredAltTrack);
		Vector3 tangent;
		Vector3 positionAndTangent = onSpline.GetPositionAndTangent(FrontWheelSplineDist, base.transform.forward, out tangent);
		SetTheRestFromFrontWheelData(ref onSpline, positionAndTangent, tangent);
		FrontTrackSection = onSpline;
		return TrackSpeed;
	}

	public void SetTheRestFromFrontWheelData(ref TrainTrackSpline frontTS, Vector3 targetFrontWheelPos, Vector3 targetFrontWheelTangent)
	{
		TrainTrackSpline onSpline;
		bool atEndOfLine;
		float splineDistAfterMove = frontTS.GetSplineDistAfterMove(FrontWheelSplineDist, base.transform.forward, 0f - distFrontToBackWheel, curTrackSelection, out onSpline, out atEndOfLine, RearTrackSection);
		Vector3 tangent;
		Vector3 positionAndTangent = onSpline.GetPositionAndTangent(splineDistAfterMove, base.transform.forward, out tangent);
		if (atEndOfLine)
		{
			FrontWheelSplineDist = onSpline.GetSplineDistAfterMove(splineDistAfterMove, base.transform.forward, distFrontToBackWheel, curTrackSelection, out frontTS, out var _, onSpline);
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

	public virtual float GetEngineForces()
	{
		return 0f;
	}

	private void UpdateClients()
	{
		if (IsMoving())
		{
			ClientRPC(null, "BaseTrainUpdate", GetNetworkTime(), frontBogieYRot, rearBogieYRot);
		}
	}

	public override void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(process, rootObj, name, serverside, clientside, bundling);
		if (frontBogiePivot != null)
		{
			frontBogieLocalOffset = frontBogiePivot.position - base.transform.position;
		}
		if (rearBogiePivot != null)
		{
			rearBogieLocalOffset = rearBogiePivot.position - base.transform.position;
		}
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
		if (curTrackSelection != trackSelection)
		{
			curTrackSelection = trackSelection;
			if (base.isServer)
			{
				ClientRPC(null, "SetTrackSelection", (sbyte)curTrackSelection);
			}
		}
	}
}
