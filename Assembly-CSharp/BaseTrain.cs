using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Rust;
using UnityEngine;

public class BaseTrain : BaseVehicle, TriggerHurtNotChild.IHurtTriggerUser, TrainTrackSpline.ITrainTrackUser, ITrainCollidable
{
	public enum StaticCollisionState
	{
		Free,
		StaticColliding,
		StayingStill
	}

	public StaticCollisionState staticCollidingAtFront;

	public StaticCollisionState staticCollidingAtRear;

	private const float MIN_COLLISION_FORCE = 50000f;

	public float nextCollisionFXTime;

	private const float MIN_TIME_BETWEEN_COLLISION_FX = 0.5f;

	public Dictionary<Rigidbody, float> prevTrackSpeeds = new Dictionary<Rigidbody, float>();

	protected bool trainDebug;

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

	[SerializeField]
	[Tooltip("How much impact energy is retained on collisions. 1.0 = 100% retained, 0.0 = 100% loss of energy")]
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
	public CapsuleCollider frontWheelWorldCol;

	[SerializeField]
	public CapsuleCollider rearWheelWorldCol;

	[SerializeField]
	public Transform centreOfMassTransform;

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
		using (TimeWarning.New("BaseTrain.OnRpcMessage"))
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

	public float GetTotalPushingForces(Vector3 pushDirection, List<BaseTrain> prevTrains = null)
	{
		if (prevTrains == null)
		{
			prevTrains = Facepunch.Pool.GetList<BaseTrain>();
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
		foreach (BaseTrain trainContent in triggerTrainCollisions.trainContents)
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
		trackSpeed = ApplyCollisions(trackSpeed, true, frontCollisionTrigger, ref staticCollidingAtFront, deltaTime);
		trackSpeed = ApplyCollisions(trackSpeed, false, rearCollisionTrigger, ref staticCollidingAtRear, deltaTime);
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
			foreach (BaseTrain trainContent in trigger.trainContents)
			{
				trackSpeed = HandleTrainCollision(atOurFront, trackSpeed, trainContent, deltaTime, ref wasStaticColliding);
				num += Vector3.Magnitude(trainContent.rigidBody.velocity - rigidBody.velocity) * trainContent.rigidBody.mass;
			}
			foreach (Rigidbody otherRigidbodyContent in trigger.otherRigidbodyContents)
			{
				trackSpeed = HandleRigidbodyCollision(atOurFront, trackSpeed, otherRigidbodyContent, otherRigidbodyContent.mass, deltaTime, true);
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

	public float HandleTrainCollision(bool front, float trackSpeed, BaseTrain theirTrain, float deltaTime, ref StaticCollisionState wasStaticColliding)
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
			trackSpeed = ((!(totalPushingMass < 0f)) ? HandleRigidbodyCollision(front, trackSpeed, theirTrain.rigidBody, totalPushingMass, deltaTime, false) : HandleStaticCollisions(true, front, trackSpeed, ref wasStaticColliding));
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
		if (forceMagnitude < 50000f)
		{
			return 0f;
		}
		float num = ((!(forceMagnitude > derailCollisionForce)) ? (Mathf.Pow(forceMagnitude, 1.4f) / collisionDamageDivide) : float.MaxValue);
		Hurt(num, DamageType.Collision, this, false);
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

	public float GetTotalPushingMass(Vector3 pushDirection, List<BaseTrain> prevTrains = null)
	{
		if (prevTrains == null)
		{
			prevTrains = Facepunch.Pool.GetList<BaseTrain>();
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
		foreach (BaseTrain trainContent in triggerTrainCollisions.trainContents)
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
		TrainTrackSpline splineResult;
		float distResult;
		if (TrainTrackSpline.TryFindTrackNearby(GetFrontWheelPos(), 2f, out splineResult, out distResult) && splineResult.HasClearTrackSpaceNear(this))
		{
			FrontTrackSection = splineResult;
			FrontWheelSplineDist = distResult;
			SetTheRestFromFrontWheelData(FrontTrackSection, FrontTrackSection.GetPosition(FrontWheelSplineDist));
		}
		else
		{
			Kill();
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

	public override Quaternion GetAngularVelocityServer()
	{
		if (rigidBody.angularVelocity.sqrMagnitude < 0.1f)
		{
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(rigidBody.angularVelocity, base.transform.up);
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (IsFullySpawned())
		{
			float num = 0f;
			if (!rigidBody.IsSleeping() || IsOn() || HasAnyCollisions())
			{
				num = FixedUpdateMoveTrain(UnityEngine.Time.fixedDeltaTime);
			}
			hurtTriggerFront.gameObject.SetActive(num > hurtTriggerMinSpeed);
			hurtTriggerRear.gameObject.SetActive(num < 0f - hurtTriggerMinSpeed);
		}
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
		return frontWheelWorldCol.transform.position + frontWheelWorldCol.transform.rotation * frontWheelWorldCol.center;
	}

	public Vector3 GetRearWheelPos()
	{
		return rearWheelWorldCol.transform.position + rearWheelWorldCol.transform.rotation * rearWheelWorldCol.center;
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
		TrackSpeed += num2 * deltaTime;
		TrackSpeed = ApplyCollisionsToTrackSpeed(TrackSpeed, deltaTime);
		float distMoved = TrackSpeed * deltaTime;
		TrainTrackSpline onSpline;
		bool atEndOfLine;
		FrontWheelSplineDist = FrontTrackSection.GetSplineDistAfterMove(FrontWheelSplineDist, base.transform.forward, distMoved, curTrackSelection, out onSpline, out atEndOfLine);
		Vector3 position = onSpline.GetPosition(FrontWheelSplineDist);
		FrontTrackSection = onSpline;
		SetTheRestFromFrontWheelData(onSpline, position);
		return TrackSpeed;
	}

	public void SetTheRestFromFrontWheelData(TrainTrackSpline frontTS, Vector3 targetFrontWheelPos)
	{
		TrainTrackSpline onSpline;
		bool atEndOfLine;
		float splineDistAfterMove = frontTS.GetSplineDistAfterMove(FrontWheelSplineDist, base.transform.forward, 0f - distFrontToBackWheel, curTrackSelection, out onSpline, out atEndOfLine, RearTrackSection);
		Vector3 position = onSpline.GetPosition(splineDistAfterMove);
		if (atEndOfLine)
		{
			bool atEndOfLine2;
			FrontWheelSplineDist = onSpline.GetSplineDistAfterMove(splineDistAfterMove, base.transform.forward, distFrontToBackWheel, curTrackSelection, out frontTS, out atEndOfLine2);
		}
		RearTrackSection = onSpline;
		Vector3 vector = targetFrontWheelPos - position;
		Vector3 vector2 = targetFrontWheelPos - vector * 0.5f;
		rigidBody.MovePosition(vector2);
		if (vector.magnitude == 0f)
		{
			rigidBody.MoveRotation(Quaternion.identity);
		}
		else
		{
			rigidBody.MoveRotation(Quaternion.LookRotation(vector));
		}
		if (UnityEngine.Application.isEditor)
		{
			Debug.DrawLine(targetFrontWheelPos, position, Color.magenta, 0.2f);
			Debug.DrawLine(rigidBody.position, vector2, Color.yellow, 0.2f);
			Debug.DrawRay(vector2, Vector3.up, Color.yellow, 0.2f);
		}
	}

	public virtual float GetEngineForces()
	{
		return 0f;
	}

	public bool CustomCollision(BaseTrain train, TriggerTrainCollisions trainTrigger)
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
