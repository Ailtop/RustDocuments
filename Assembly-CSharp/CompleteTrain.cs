using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class CompleteTrain : IDisposable
{
	private enum ShuntState
	{
		None = 0,
		Forwards = 1,
		Backwards = 2
	}

	public enum StaticCollisionState
	{
		Free = 0,
		StaticColliding = 1,
		StayingStill = 2
	}

	private Vector3 unloaderPos;

	public float trackSpeed;

	private float prevTrackSpeed;

	public List<TrainCar> trainCars;

	public TriggerTrainCollisions frontCollisionTrigger;

	public TriggerTrainCollisions rearCollisionTrigger;

	public bool ranUpdateTick;

	public bool disposed;

	public const float IMPACT_ENERGY_FRACTION = 0.75f;

	public const float MIN_COLLISION_FORCE = 70000f;

	public float lastMovingTime = float.MinValue;

	public const float SLEEP_SPEED = 0.1f;

	public const float SLEEP_DELAY = 10f;

	public TimeSince timeSinceLastChange;

	private bool isShunting;

	private TimeSince timeSinceShuntStart;

	private const float MAX_SHUNT_TIME = 20f;

	private const float SHUNT_SPEED = 4f;

	private const float SHUNT_SPEED_CHANGE_RATE = 10f;

	private Action<CoalingTower.ActionAttemptStatus> shuntEndCallback;

	private float shuntDistance;

	private Vector3 shuntDirection;

	private Vector2 shuntStartPos2D = Vector2.zero;

	private Vector2 shuntTargetPos2D = Vector2.zero;

	private TrainCar shuntTarget;

	public StaticCollisionState staticCollidingAtFront;

	private HashSet<GameObject> monitoredStaticContentF = new HashSet<GameObject>();

	public StaticCollisionState staticCollidingAtRear;

	private HashSet<GameObject> monitoredStaticContentR = new HashSet<GameObject>();

	public Dictionary<Rigidbody, float> prevTrackSpeeds = new Dictionary<Rigidbody, float>();

	public TrainCar PrimaryTrainCar { get; set; }

	public bool TrainIsReversing => PrimaryTrainCar != trainCars[0];

	public float TotalForces { get; set; }

	public float TotalMass { get; set; }

	public int NumTrainCars => trainCars.Count;

	public int LinedUpToUnload { get; private set; } = -1;


	public bool IsLinedUpToUnload => LinedUpToUnload >= 0;

	public CompleteTrain(TrainCar trainCar)
	{
		List<TrainCar> list = Facepunch.Pool.GetList<TrainCar>();
		list.Add(trainCar);
		Init(list);
	}

	public CompleteTrain(List<TrainCar> allTrainCars)
	{
		Init(allTrainCars);
	}

	private void Init(List<TrainCar> allTrainCars)
	{
		trainCars = allTrainCars;
		timeSinceLastChange = 0f;
		lastMovingTime = UnityEngine.Time.time;
		float num = 0f;
		PrimaryTrainCar = trainCars[0];
		for (int i = 0; i < trainCars.Count; i++)
		{
			TrainCar trainCar = trainCars[i];
			if (trainCar.completeTrain != this)
			{
				if (trainCar.completeTrain != null)
				{
					bool num2 = IsCoupledBackwards(i);
					bool preChangeCoupledBackwards = trainCar.coupling.PreChangeCoupledBackwards;
					float preChangeTrackSpeed = trainCar.coupling.PreChangeTrackSpeed;
					num = ((num2 == preChangeCoupledBackwards) ? (num + preChangeTrackSpeed) : (num - preChangeTrackSpeed));
				}
				trainCar.SetNewCompleteTrain(this);
			}
		}
		num = (trackSpeed = num / (float)trainCars.Count);
		prevTrackSpeed = trackSpeed;
		ParamsTick();
	}

	~CompleteTrain()
	{
		Cleanup();
	}

	public void Dispose()
	{
		Cleanup();
		System.GC.SuppressFinalize(this);
	}

	private void Cleanup()
	{
		if (!disposed)
		{
			EndShunting(CoalingTower.ActionAttemptStatus.GenericError);
			disposed = true;
			Facepunch.Pool.FreeList(ref trainCars);
		}
	}

	public void RemoveTrainCar(TrainCar trainCar)
	{
		if (disposed)
		{
			return;
		}
		if (trainCars.Count <= 1)
		{
			Debug.LogWarning(GetType().Name + ": Can't remove car from CompleteTrain of length one.");
			return;
		}
		int num = IndexOf(trainCar);
		bool flag = ((num != 0) ? IsCoupledBackwards(0) : IsCoupledBackwards(1));
		trainCars.RemoveAt(num);
		timeSinceLastChange = 0f;
		LinedUpToUnload = -1;
		if (IsCoupledBackwards(0) != flag)
		{
			trackSpeed *= -1f;
		}
	}

	public float GetTrackSpeedFor(TrainCar trainCar)
	{
		if (disposed)
		{
			return 0f;
		}
		if (trainCars.IndexOf(trainCar) < 0)
		{
			Debug.LogError(GetType().Name + ": Train car not found in the trainCars list.");
			return 0f;
		}
		if (IsCoupledBackwards(trainCar))
		{
			return 0f - trackSpeed;
		}
		return trackSpeed;
	}

	public float GetPrevTrackSpeedFor(TrainCar trainCar)
	{
		if (trainCars.IndexOf(trainCar) < 0)
		{
			Debug.LogError(GetType().Name + ": Train car not found in the trainCars list.");
			return 0f;
		}
		if (IsCoupledBackwards(trainCar))
		{
			return 0f - prevTrackSpeed;
		}
		return prevTrackSpeed;
	}

	public void UpdateTick(float dt)
	{
		if (ranUpdateTick || disposed)
		{
			return;
		}
		ranUpdateTick = true;
		if (IsAllAsleep() && !HasAnyEnginesOn() && !HasAnyCollisions() && !isShunting)
		{
			trackSpeed = 0f;
			return;
		}
		ParamsTick();
		MovementTick(dt);
		LinedUpToUnload = CheckLinedUpToUnload(out unloaderPos);
		if (!disposed)
		{
			if (Mathf.Abs(trackSpeed) > 0.1f)
			{
				lastMovingTime = UnityEngine.Time.time;
			}
			if (!HasAnyEnginesOn() && !HasAnyCollisions() && UnityEngine.Time.time > lastMovingTime + 10f)
			{
				trackSpeed = 0f;
				SleepAll();
			}
		}
	}

	public bool IncludesAnEngine()
	{
		if (disposed)
		{
			return false;
		}
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.CarType == TrainCar.TrainCarType.Engine)
			{
				return true;
			}
		}
		return false;
	}

	protected bool HasAnyCollisions()
	{
		if (!frontCollisionTrigger.HasAnyContents)
		{
			return rearCollisionTrigger.HasAnyContents;
		}
		return true;
	}

	private bool HasAnyEnginesOn()
	{
		if (disposed)
		{
			return false;
		}
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.CarType == TrainCar.TrainCarType.Engine && trainCar.IsOn())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAllAsleep()
	{
		if (disposed)
		{
			return true;
		}
		foreach (TrainCar trainCar in trainCars)
		{
			if (!trainCar.rigidBody.IsSleeping())
			{
				return false;
			}
		}
		return true;
	}

	private void SleepAll()
	{
		if (disposed)
		{
			return;
		}
		foreach (TrainCar trainCar in trainCars)
		{
			trainCar.rigidBody.Sleep();
		}
	}

	public bool TryShuntCarTo(Vector3 shuntDirection, float shuntDistance, TrainCar shuntTarget, Action<CoalingTower.ActionAttemptStatus> shuntEndCallback, out CoalingTower.ActionAttemptStatus status)
	{
		if (disposed)
		{
			status = CoalingTower.ActionAttemptStatus.NoTrainCar;
			return false;
		}
		if (isShunting)
		{
			status = CoalingTower.ActionAttemptStatus.AlreadyShunting;
			return false;
		}
		if (Mathf.Abs(trackSpeed) > 0.1f)
		{
			status = CoalingTower.ActionAttemptStatus.TrainIsMoving;
			return false;
		}
		if (HasThrottleInput())
		{
			status = CoalingTower.ActionAttemptStatus.TrainHasThrottle;
			return false;
		}
		this.shuntDirection = shuntDirection;
		this.shuntDistance = shuntDistance;
		this.shuntTarget = shuntTarget;
		timeSinceShuntStart = 0f;
		shuntStartPos2D.x = shuntTarget.transform.position.x;
		shuntStartPos2D.y = shuntTarget.transform.position.z;
		isShunting = true;
		this.shuntEndCallback = shuntEndCallback;
		status = CoalingTower.ActionAttemptStatus.NoError;
		return true;
	}

	private void EndShunting(CoalingTower.ActionAttemptStatus status)
	{
		isShunting = false;
		if (shuntEndCallback != null)
		{
			shuntEndCallback(status);
			shuntEndCallback = null;
		}
		shuntTarget = null;
	}

	public bool ContainsOnly(TrainCar trainCar)
	{
		if (disposed)
		{
			return false;
		}
		if (trainCars.Count == 1)
		{
			return trainCars[0] == trainCar;
		}
		return false;
	}

	public int IndexOf(TrainCar trainCar)
	{
		if (disposed)
		{
			return -1;
		}
		return trainCars.IndexOf(trainCar);
	}

	public bool TryGetAdjacentTrainCar(TrainCar trainCar, bool next, Vector3 forwardDir, out TrainCar result)
	{
		int num = trainCars.IndexOf(trainCar);
		Vector3 lhs = ((!IsCoupledBackwards(num)) ? trainCar.transform.forward : (-trainCar.transform.forward));
		if (Vector3.Dot(lhs, forwardDir) < 0f)
		{
			next = !next;
		}
		if (num >= 0)
		{
			num = ((!next) ? (num - 1) : (num + 1));
			if (num >= 0 && num < trainCars.Count)
			{
				result = trainCars[num];
				return true;
			}
		}
		result = null;
		return false;
	}

	private void ParamsTick()
	{
		TotalForces = 0f;
		TotalMass = 0f;
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < trainCars.Count; i++)
		{
			TrainCar trainCar = trainCars[i];
			if (trainCar.rigidBody.mass > num2)
			{
				num2 = trainCar.rigidBody.mass;
				num = i;
			}
		}
		bool flag = false;
		for (int j = 0; j < trainCars.Count; j++)
		{
			TrainCar trainCar2 = trainCars[j];
			float forces = trainCar2.GetForces();
			TotalForces += (IsCoupledBackwards(trainCar2) ? (0f - forces) : forces);
			flag |= trainCar2.HasThrottleInput();
			if (j == num)
			{
				TotalMass += trainCar2.rigidBody.mass;
			}
			else
			{
				TotalMass += trainCar2.rigidBody.mass * 0.4f;
			}
		}
		if (isShunting && flag)
		{
			EndShunting(CoalingTower.ActionAttemptStatus.TrainHasThrottle);
		}
		if (trainCars.Count == 1)
		{
			frontCollisionTrigger = trainCars[0].FrontCollisionTrigger;
			rearCollisionTrigger = trainCars[0].RearCollisionTrigger;
		}
		else
		{
			frontCollisionTrigger = (trainCars[0].coupling.IsRearCoupled ? trainCars[0].FrontCollisionTrigger : trainCars[0].RearCollisionTrigger);
			rearCollisionTrigger = (trainCars[trainCars.Count - 1].coupling.IsRearCoupled ? trainCars[trainCars.Count - 1].FrontCollisionTrigger : trainCars[trainCars.Count - 1].RearCollisionTrigger);
		}
	}

	private void MovementTick(float dt)
	{
		prevTrackSpeed = trackSpeed;
		if (!isShunting)
		{
			trackSpeed += TotalForces * dt / TotalMass;
		}
		else
		{
			bool flag = Vector3.Dot(shuntDirection, PrimaryTrainCar.transform.forward) >= 0f;
			if (IsCoupledBackwards(PrimaryTrainCar))
			{
				flag = !flag;
			}
			if (shuntTarget == null || shuntTarget.IsDead() || shuntTarget.IsDestroyed)
			{
				EndShunting(CoalingTower.ActionAttemptStatus.NoTrainCar);
			}
			else
			{
				float num = 4f;
				shuntTargetPos2D.x = shuntTarget.transform.position.x;
				shuntTargetPos2D.y = shuntTarget.transform.position.z;
				float num2 = shuntDistance - Vector3.Distance(shuntStartPos2D, shuntTargetPos2D);
				if (num2 < 2f)
				{
					float t = Mathf.InverseLerp(0f, 2f, num2);
					num *= Mathf.Lerp(0.1f, 1f, t);
				}
				trackSpeed = Mathf.MoveTowards(trackSpeed, flag ? num : (0f - num), dt * 10f);
				if ((float)timeSinceShuntStart > 20f || num2 <= 0f)
				{
					EndShunting(CoalingTower.ActionAttemptStatus.NoError);
					trackSpeed = 0f;
				}
			}
		}
		float num3 = trainCars[0].rigidBody.drag;
		if (IsLinedUpToUnload)
		{
			float num4 = Mathf.Abs(trackSpeed);
			if (num4 > 1f)
			{
				TrainCarUnloadable trainCarUnloadable = trainCars[LinedUpToUnload] as TrainCarUnloadable;
				if (trainCarUnloadable != null)
				{
					float value = trainCarUnloadable.MinDistToUnloadingArea(unloaderPos);
					float num5 = Mathf.InverseLerp(2f, 0f, value);
					if (num4 < 2f)
					{
						float num6 = (num4 - 1f) / 1f;
						num5 *= num6;
					}
					num3 = Mathf.Lerp(num3, 3.5f, num5);
				}
			}
		}
		if (trackSpeed > 0f)
		{
			trackSpeed -= num3 * 4f * dt;
			if (trackSpeed < 0f)
			{
				trackSpeed = 0f;
			}
		}
		else if (trackSpeed < 0f)
		{
			trackSpeed += num3 * 4f * dt;
			if (trackSpeed > 0f)
			{
				trackSpeed = 0f;
			}
		}
		float num7 = trackSpeed;
		trackSpeed = ApplyCollisionsToTrackSpeed(trackSpeed, TotalMass, dt);
		if (isShunting && trackSpeed != num7)
		{
			EndShunting(CoalingTower.ActionAttemptStatus.GenericError);
		}
		if (disposed)
		{
			return;
		}
		trackSpeed = Mathf.Clamp(trackSpeed, 0f - (TrainCar.TRAINCAR_MAX_SPEED - 1f), TrainCar.TRAINCAR_MAX_SPEED - 1f);
		if (trackSpeed > 0f)
		{
			PrimaryTrainCar = trainCars[0];
		}
		else if (trackSpeed < 0f)
		{
			PrimaryTrainCar = trainCars[trainCars.Count - 1];
		}
		else if (TotalForces > 0f)
		{
			PrimaryTrainCar = trainCars[0];
		}
		else if (TotalForces < 0f)
		{
			PrimaryTrainCar = trainCars[trainCars.Count - 1];
		}
		else
		{
			PrimaryTrainCar = trainCars[0];
		}
		if (trackSpeed == 0f && TotalForces == 0f)
		{
			return;
		}
		PrimaryTrainCar.FrontTrainCarTick(GetTrackSelection(), dt);
		if (trainCars.Count <= 1)
		{
			return;
		}
		if (PrimaryTrainCar == trainCars[0])
		{
			for (int i = 1; i < trainCars.Count; i++)
			{
				MoveOtherTrainCar(trainCars[i], trainCars[i - 1]);
			}
			return;
		}
		for (int num8 = trainCars.Count - 2; num8 >= 0; num8--)
		{
			MoveOtherTrainCar(trainCars[num8], trainCars[num8 + 1]);
		}
	}

	private void MoveOtherTrainCar(TrainCar trainCar, TrainCar prevTrainCar)
	{
		TrainTrackSpline frontTrackSection = prevTrainCar.FrontTrackSection;
		float frontWheelSplineDist = prevTrainCar.FrontWheelSplineDist;
		float num = 0f;
		TrainCoupling coupledTo = trainCar.coupling.frontCoupling.CoupledTo;
		TrainCoupling coupledTo2 = trainCar.coupling.rearCoupling.CoupledTo;
		if (coupledTo == prevTrainCar.coupling.frontCoupling)
		{
			num += trainCar.DistFrontWheelToFrontCoupling;
			num += prevTrainCar.DistFrontWheelToFrontCoupling;
		}
		else if (coupledTo2 == prevTrainCar.coupling.rearCoupling)
		{
			num -= trainCar.DistFrontWheelToBackCoupling;
			num -= prevTrainCar.DistFrontWheelToBackCoupling;
		}
		else if (coupledTo == prevTrainCar.coupling.rearCoupling)
		{
			num += trainCar.DistFrontWheelToFrontCoupling;
			num += prevTrainCar.DistFrontWheelToBackCoupling;
		}
		else if (coupledTo2 == prevTrainCar.coupling.frontCoupling)
		{
			num -= trainCar.DistFrontWheelToBackCoupling;
			num -= prevTrainCar.DistFrontWheelToFrontCoupling;
		}
		else
		{
			Debug.LogError(GetType().Name + ": Uncoupled!");
		}
		trainCar.OtherTrainCarTick(frontTrackSection, frontWheelSplineDist, 0f - num);
	}

	public void ResetUpdateTick()
	{
		ranUpdateTick = false;
	}

	public bool Matches(List<TrainCar> listToCompare)
	{
		if (disposed)
		{
			return false;
		}
		if (listToCompare.Count != trainCars.Count)
		{
			return false;
		}
		for (int i = 0; i < listToCompare.Count; i++)
		{
			if (trainCars[i] != listToCompare[i])
			{
				return false;
			}
		}
		return true;
	}

	public void ReduceSpeedBy(float velChange)
	{
		prevTrackSpeed = trackSpeed;
		if (trackSpeed > 0f)
		{
			trackSpeed = Mathf.Max(0f, trackSpeed - velChange);
		}
		else if (trackSpeed < 0f)
		{
			trackSpeed = Mathf.Min(0f, trackSpeed + velChange);
		}
	}

	public bool AnyPlayersOnTrain()
	{
		if (disposed)
		{
			return false;
		}
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.AnyPlayersOnTrainCar())
			{
				return true;
			}
		}
		return false;
	}

	private int CheckLinedUpToUnload(out Vector3 unloaderPos)
	{
		if (disposed)
		{
			unloaderPos = Vector3.zero;
			return -1;
		}
		for (int i = 0; i < trainCars.Count; i++)
		{
			TrainCar trainCar = trainCars[i];
			if (CoalingTower.IsUnderAnUnloader(trainCar, out var isLinedUp, out unloaderPos))
			{
				trainCar.SetFlag(BaseEntity.Flags.Reserved4, isLinedUp);
				if (isLinedUp)
				{
					return i;
				}
			}
		}
		unloaderPos = Vector3.zero;
		return -1;
	}

	public bool IsCoupledBackwards(TrainCar trainCar)
	{
		if (disposed)
		{
			return false;
		}
		return IsCoupledBackwards(trainCars.IndexOf(trainCar));
	}

	private bool IsCoupledBackwards(int trainCarIndex)
	{
		if (disposed || trainCars.Count == 1 || trainCarIndex < 0 || trainCarIndex > trainCars.Count - 1)
		{
			return false;
		}
		TrainCar trainCar = trainCars[trainCarIndex];
		if (trainCarIndex == 0)
		{
			return trainCar.coupling.IsFrontCoupled;
		}
		TrainCoupling coupledTo = trainCar.coupling.frontCoupling.CoupledTo;
		if (coupledTo != null)
		{
			return coupledTo.owner != trainCars[trainCarIndex - 1];
		}
		return true;
	}

	private bool HasThrottleInput()
	{
		for (int i = 0; i < trainCars.Count; i++)
		{
			if (trainCars[i].HasThrottleInput())
			{
				return true;
			}
		}
		return false;
	}

	private TrainTrackSpline.TrackSelection GetTrackSelection()
	{
		TrainTrackSpline.TrackSelection result = TrainTrackSpline.TrackSelection.Default;
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.localTrackSelection == TrainTrackSpline.TrackSelection.Default)
			{
				continue;
			}
			if (IsCoupledBackwards(trainCar) != IsCoupledBackwards(PrimaryTrainCar))
			{
				if (trainCar.localTrackSelection == TrainTrackSpline.TrackSelection.Left)
				{
					return TrainTrackSpline.TrackSelection.Right;
				}
				if (trainCar.localTrackSelection == TrainTrackSpline.TrackSelection.Right)
				{
					return TrainTrackSpline.TrackSelection.Left;
				}
			}
			return trainCar.localTrackSelection;
		}
		return result;
	}

	public void FreeStaticCollision()
	{
		staticCollidingAtFront = StaticCollisionState.Free;
		staticCollidingAtRear = StaticCollisionState.Free;
	}

	private float ApplyCollisionsToTrackSpeed(float trackSpeed, float totalMass, float deltaTime)
	{
		TrainCar owner = frontCollisionTrigger.owner;
		Vector3 forwardVector = (IsCoupledBackwards(owner) ? (-owner.transform.forward) : owner.transform.forward);
		trackSpeed = ApplyCollisions(trackSpeed, owner, forwardVector, atOurFront: true, frontCollisionTrigger, totalMass, ref staticCollidingAtFront, staticCollidingAtRear, deltaTime);
		if (disposed)
		{
			return trackSpeed;
		}
		owner = rearCollisionTrigger.owner;
		forwardVector = (IsCoupledBackwards(owner) ? (-owner.transform.forward) : owner.transform.forward);
		trackSpeed = ApplyCollisions(trackSpeed, owner, forwardVector, atOurFront: false, rearCollisionTrigger, totalMass, ref staticCollidingAtRear, staticCollidingAtFront, deltaTime);
		if (disposed)
		{
			return trackSpeed;
		}
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

	private float ApplyCollisions(float trackSpeed, TrainCar ourTrainCar, Vector3 forwardVector, bool atOurFront, TriggerTrainCollisions trigger, float ourTotalMass, ref StaticCollisionState wasStaticColliding, StaticCollisionState otherEndStaticColliding, float deltaTime)
	{
		Vector3 vector = forwardVector * trackSpeed;
		bool flag = trigger.HasAnyStaticContents;
		if (atOurFront && ourTrainCar.FrontAtEndOfLine)
		{
			flag = true;
		}
		else if (!atOurFront && ourTrainCar.RearAtEndOfLine)
		{
			flag = true;
		}
		float num = (flag ? (vector.magnitude * Mathf.Clamp(ourTotalMass, 1f, 13000f)) : 0f);
		trackSpeed = HandleStaticCollisions(flag, atOurFront, trackSpeed, ref wasStaticColliding, trigger);
		if (!flag && otherEndStaticColliding == StaticCollisionState.Free)
		{
			foreach (TrainCar trainContent in trigger.trainContents)
			{
				Vector3 vector2 = trainContent.transform.forward * trainContent.GetPrevTrackSpeed();
				trackSpeed = HandleTrainCollision(atOurFront, forwardVector, trackSpeed, ourTrainCar.transform, trainContent, deltaTime, ref wasStaticColliding);
				num += Vector3.Magnitude(vector2 - vector) * Mathf.Clamp(trainContent.rigidBody.mass, 1f, 13000f);
			}
			foreach (Rigidbody otherRigidbodyContent in trigger.otherRigidbodyContents)
			{
				trackSpeed = HandleRigidbodyCollision(atOurFront, trackSpeed, forwardVector, ourTotalMass, otherRigidbodyContent, otherRigidbodyContent.mass, deltaTime, calcSecondaryForces: true);
				num += Vector3.Magnitude(otherRigidbodyContent.velocity - vector) * Mathf.Clamp(otherRigidbodyContent.mass, 1f, 13000f);
			}
		}
		if (num >= 70000f && (float)timeSinceLastChange > 1f && trigger.owner.ApplyCollisionDamage(num) > 5f)
		{
			foreach (Collider colliderContent in trigger.colliderContents)
			{
				Vector3 contactPoint = colliderContent.ClosestPointOnBounds(trigger.owner.transform.position);
				trigger.owner.TryShowCollisionFX(contactPoint, trigger.owner.collisionEffect);
			}
			return trackSpeed;
		}
		return trackSpeed;
	}

	private float HandleStaticCollisions(bool staticColliding, bool front, float trackSpeed, ref StaticCollisionState wasStaticColliding, TriggerTrainCollisions trigger = null)
	{
		float num = (front ? (-5f) : 5f);
		if (staticColliding && (front ? (trackSpeed > num) : (trackSpeed < num)))
		{
			trackSpeed = num;
			wasStaticColliding = StaticCollisionState.StaticColliding;
			HashSet<GameObject> hashSet = (front ? monitoredStaticContentF : monitoredStaticContentR);
			hashSet.Clear();
			if (trigger != null)
			{
				foreach (GameObject staticContent in trigger.staticContents)
				{
					hashSet.Add(staticContent);
				}
				return trackSpeed;
			}
		}
		else if (wasStaticColliding == StaticCollisionState.StaticColliding)
		{
			trackSpeed = 0f;
			wasStaticColliding = StaticCollisionState.StayingStill;
		}
		else if (wasStaticColliding == StaticCollisionState.StayingStill)
		{
			bool flag = (front ? (trackSpeed > 0.01f) : (trackSpeed < -0.01f));
			bool flag2 = false;
			if (!flag)
			{
				flag2 = (front ? (trackSpeed < -0.01f) : (trackSpeed > 0.01f));
			}
			if (flag)
			{
				HashSet<GameObject> hashSet2 = (front ? monitoredStaticContentF : monitoredStaticContentR);
				if (hashSet2.Count > 0)
				{
					bool flag3 = true;
					foreach (GameObject item in hashSet2)
					{
						if (item != null)
						{
							flag3 = false;
							break;
						}
					}
					if (flag3)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				trackSpeed = 0f;
			}
			else if (flag2)
			{
				wasStaticColliding = StaticCollisionState.Free;
			}
		}
		else if (front)
		{
			monitoredStaticContentF.Clear();
		}
		else
		{
			monitoredStaticContentR.Clear();
		}
		return trackSpeed;
	}

	private float HandleTrainCollision(bool front, Vector3 forwardVector, float trackSpeed, Transform ourTransform, TrainCar theirTrain, float deltaTime, ref StaticCollisionState wasStaticColliding)
	{
		Vector3 vector = (front ? forwardVector : (-forwardVector));
		float num = Vector3.Angle(vector, theirTrain.transform.forward);
		float f = Vector3.Dot(vector, (theirTrain.transform.position - ourTransform.position).normalized);
		if ((num > 30f && num < 150f) || Mathf.Abs(f) < 0.975f)
		{
			trackSpeed = (front ? (-0.5f) : 0.5f);
		}
		else
		{
			List<CompleteTrain> prevTrains = Facepunch.Pool.GetList<CompleteTrain>();
			float totalPushingMass = GetTotalPushingMass(vector, forwardVector, ref prevTrains);
			trackSpeed = ((!(totalPushingMass < 0f)) ? HandleRigidbodyCollision(front, trackSpeed, forwardVector, TotalMass, theirTrain.rigidBody, totalPushingMass, deltaTime, calcSecondaryForces: false) : HandleStaticCollisions(staticColliding: true, front, trackSpeed, ref wasStaticColliding));
			prevTrains.Clear();
			float num2 = GetTotalPushingForces(vector, forwardVector, ref prevTrains);
			if (!front)
			{
				num2 *= -1f;
			}
			if ((front && num2 <= 0f) || (!front && num2 >= 0f))
			{
				trackSpeed += num2 / TotalMass * deltaTime;
			}
			Facepunch.Pool.FreeList(ref prevTrains);
		}
		return trackSpeed;
	}

	private float HandleRigidbodyCollision(bool atOurFront, float trackSpeed, Vector3 forwardVector, float ourTotalMass, Rigidbody theirRB, float theirTotalMass, float deltaTime, bool calcSecondaryForces)
	{
		float num = Vector3.Dot(forwardVector, theirRB.velocity);
		float num2 = trackSpeed - num;
		if ((atOurFront && num2 <= 0f) || (!atOurFront && num2 >= 0f))
		{
			return trackSpeed;
		}
		float num3 = num2 / deltaTime * theirTotalMass * 0.75f;
		if (calcSecondaryForces)
		{
			if (prevTrackSpeeds.ContainsKey(theirRB))
			{
				float num4 = num2 / deltaTime * ourTotalMass * 0.75f / theirTotalMass * deltaTime;
				float num5 = prevTrackSpeeds[theirRB] - num;
				num3 -= Mathf.Clamp((num5 - num4) * ourTotalMass, 0f, 1000000f);
				prevTrackSpeeds[theirRB] = num;
			}
			else if (num != 0f)
			{
				prevTrackSpeeds.Add(theirRB, num);
			}
		}
		float value = num3 / ourTotalMass * deltaTime;
		value = Mathf.Clamp(value, 0f - Mathf.Abs(num - trackSpeed) - 0.5f, Mathf.Abs(num - trackSpeed) + 0.5f);
		trackSpeed -= value;
		return trackSpeed;
	}

	private float GetTotalPushingMass(Vector3 pushDirection, Vector3 ourForward, ref List<CompleteTrain> prevTrains)
	{
		float num = 0f;
		if (prevTrains.Count > 0)
		{
			if (prevTrains.Contains(this))
			{
				if (Global.developer > 1 || UnityEngine.Application.isEditor)
				{
					Debug.LogWarning("GetTotalPushingMass: Recursive loop detected. Bailing out.");
				}
				return 0f;
			}
			num += TotalMass;
		}
		prevTrains.Add(this);
		bool flag = Vector3.Dot(ourForward, pushDirection) >= 0f;
		if ((flag ? staticCollidingAtFront : staticCollidingAtRear) != 0)
		{
			return -1f;
		}
		TriggerTrainCollisions triggerTrainCollisions = (flag ? frontCollisionTrigger : rearCollisionTrigger);
		foreach (TrainCar trainContent in triggerTrainCollisions.trainContents)
		{
			if (trainContent.completeTrain != this)
			{
				Vector3 ourForward2 = (trainContent.completeTrain.IsCoupledBackwards(trainContent) ? (-trainContent.transform.forward) : trainContent.transform.forward);
				float totalPushingMass = trainContent.completeTrain.GetTotalPushingMass(pushDirection, ourForward2, ref prevTrains);
				if (totalPushingMass < 0f)
				{
					return -1f;
				}
				num += totalPushingMass;
			}
		}
		foreach (Rigidbody otherRigidbodyContent in triggerTrainCollisions.otherRigidbodyContents)
		{
			num += otherRigidbodyContent.mass;
		}
		return num;
	}

	private float GetTotalPushingForces(Vector3 pushDirection, Vector3 ourForward, ref List<CompleteTrain> prevTrains)
	{
		float num = 0f;
		if (prevTrains.Count > 0)
		{
			if (prevTrains.Contains(this))
			{
				if (Global.developer > 1 || UnityEngine.Application.isEditor)
				{
					Debug.LogWarning("GetTotalPushingForces: Recursive loop detected. Bailing out.");
				}
				return 0f;
			}
			num += TotalForces;
		}
		prevTrains.Add(this);
		bool num2 = Vector3.Dot(ourForward, pushDirection) >= 0f;
		TriggerTrainCollisions triggerTrainCollisions = (num2 ? frontCollisionTrigger : rearCollisionTrigger);
		if (!num2)
		{
			num *= -1f;
		}
		foreach (TrainCar trainContent in triggerTrainCollisions.trainContents)
		{
			if (trainContent.completeTrain != this)
			{
				Vector3 ourForward2 = (trainContent.completeTrain.IsCoupledBackwards(trainContent) ? (-trainContent.transform.forward) : trainContent.transform.forward);
				num += trainContent.completeTrain.GetTotalPushingForces(pushDirection, ourForward2, ref prevTrains);
			}
		}
		return num;
	}
}
