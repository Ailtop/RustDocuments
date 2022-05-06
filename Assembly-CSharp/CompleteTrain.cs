using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class CompleteTrain : IDisposable
{
	private enum StaticCollisionState
	{
		Free = 0,
		StaticColliding = 1,
		StayingStill = 2
	}

	private float trackSpeed;

	private List<TrainCar> trainCars;

	private TriggerTrainCollisions frontCollisionTrigger;

	private TriggerTrainCollisions rearCollisionTrigger;

	private bool ranUpdateTick;

	private bool disposed;

	private const float IMPACT_ENERGY_FRACTION = 0.75f;

	private const float MIN_COLLISION_FORCE = 70000f;

	private float lastMovingTime = float.MinValue;

	private const float SLEEP_SPEED = 0.1f;

	private const float SLEEP_DELAY = 10f;

	private TimeSince timeSinceLastChange;

	private StaticCollisionState staticCollidingAtFront;

	private HashSet<GameObject> monitoredStaticContentF = new HashSet<GameObject>();

	private StaticCollisionState staticCollidingAtRear;

	private HashSet<GameObject> monitoredStaticContentR = new HashSet<GameObject>();

	private Dictionary<Rigidbody, float> prevTrackSpeeds = new Dictionary<Rigidbody, float>();

	public TrainCar PrimaryTrainCar { get; private set; }

	public bool TrainIsReversing => PrimaryTrainCar != trainCars[0];

	public float TotalForces { get; private set; }

	public float TotalMass { get; private set; }

	public CompleteTrain(TrainCar trainCar)
		: this(new List<TrainCar>(1) { trainCar })
	{
	}

	public CompleteTrain(List<TrainCar> allTrainCars)
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
					float preChangeTrackSpeed = trainCar.coupling.PreChangeTrackSpeed;
					num = ((!num2) ? (num + preChangeTrackSpeed) : (num - preChangeTrackSpeed));
				}
				trainCar.SetNewCompleteTrain(this);
			}
		}
		num = (trackSpeed = num / (float)trainCars.Count);
		ParamsTick();
	}

	public void RemoveTrainCar(TrainCar trainCar)
	{
		trainCars.Remove(trainCar);
		timeSinceLastChange = 0f;
	}

	public float GetTrackSpeedFor(TrainCar trainCar)
	{
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

	public void UpdateTick(float dt)
	{
		if (ranUpdateTick || disposed)
		{
			return;
		}
		ranUpdateTick = true;
		if (!IsAllAsleep() || HasAnyEnginesOn() || HasAnyCollisions())
		{
			ParamsTick();
			MovementTick(dt);
			if (Mathf.Abs(trackSpeed) > 0.1f)
			{
				lastMovingTime = UnityEngine.Time.time;
			}
			if (!HasAnyEnginesOn() && !HasAnyCollisions() && UnityEngine.Time.time > lastMovingTime + 10f)
			{
				SleepAll();
			}
		}
	}

	public void Dispose()
	{
		disposed = true;
	}

	public bool IncludesAnEngine()
	{
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.IsEngine)
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
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.IsEngine && trainCar.IsOn())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsAllAsleep()
	{
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
		foreach (TrainCar trainCar in trainCars)
		{
			trainCar.rigidBody.Sleep();
		}
	}

	public bool ContainsOnly(TrainCar trainCar)
	{
		if (trainCars.Count == 1)
		{
			return trainCars[0] == trainCar;
		}
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
		for (int j = 0; j < trainCars.Count; j++)
		{
			TrainCar trainCar2 = trainCars[j];
			float forces = trainCar2.GetForces();
			TotalForces += (IsCoupledBackwards(trainCar2) ? (0f - forces) : forces);
			if (j == num)
			{
				TotalMass += trainCar2.rigidBody.mass;
			}
			else
			{
				TotalMass += trainCar2.rigidBody.mass * 0.4f;
			}
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
		trackSpeed += TotalForces * dt / TotalMass;
		float drag = trainCars[0].rigidBody.drag;
		if (trackSpeed > 0f)
		{
			trackSpeed -= drag * 4f * dt;
			if (trackSpeed < 0f)
			{
				trackSpeed = 0f;
			}
		}
		else if (trackSpeed < 0f)
		{
			trackSpeed += drag * 4f * dt;
			if (trackSpeed > 0f)
			{
				trackSpeed = 0f;
			}
		}
		trackSpeed = ApplyCollisionsToTrackSpeed(trackSpeed, TotalMass, dt);
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
		for (int num = trainCars.Count - 2; num >= 0; num--)
		{
			MoveOtherTrainCar(trainCars[num], trainCars[num + 1]);
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
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.AnyPlayersOnTrainCar())
			{
				return true;
			}
		}
		return false;
	}

	private bool IsCoupledBackwards(TrainCar trainCar)
	{
		return IsCoupledBackwards(trainCars.IndexOf(trainCar));
	}

	private bool IsCoupledBackwards(int trainCarIndex)
	{
		if (trainCars.Count == 1 || trainCarIndex < 0 || trainCarIndex > trainCars.Count - 1)
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

	private TrainTrackSpline.TrackSelection GetTrackSelection()
	{
		TrainTrackSpline.TrackSelection result = TrainTrackSpline.TrackSelection.Default;
		foreach (TrainCar trainCar in trainCars)
		{
			if (trainCar.localTrackSelection != 0)
			{
				return trainCar.localTrackSelection;
			}
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
		trackSpeed = ApplyCollisions(trackSpeed, owner, forwardVector, atOurFront: true, frontCollisionTrigger, totalMass, ref staticCollidingAtFront, deltaTime);
		owner = rearCollisionTrigger.owner;
		forwardVector = (IsCoupledBackwards(owner) ? (-owner.transform.forward) : owner.transform.forward);
		trackSpeed = ApplyCollisions(trackSpeed, owner, forwardVector, atOurFront: false, rearCollisionTrigger, totalMass, ref staticCollidingAtRear, deltaTime);
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

	private float ApplyCollisions(float trackSpeed, TrainCar ourTrainCar, Vector3 forwardVector, bool atOurFront, TriggerTrainCollisions trigger, float ourTotalMass, ref StaticCollisionState wasStaticColliding, float deltaTime)
	{
		Vector3 vector = forwardVector * trackSpeed;
		bool hasAnyStaticContents = trigger.HasAnyStaticContents;
		float num = (hasAnyStaticContents ? (vector.magnitude * ourTotalMass) : 0f);
		trackSpeed = HandleStaticCollisions(hasAnyStaticContents, atOurFront, trackSpeed, ref wasStaticColliding, trigger);
		if (!hasAnyStaticContents)
		{
			foreach (TrainCar trainContent in trigger.trainContents)
			{
				Vector3 vector2 = trainContent.transform.forward * trainContent.GetTrackSpeed();
				trackSpeed = HandleTrainCollision(atOurFront, forwardVector, trackSpeed, ourTrainCar.transform, trainContent, deltaTime, ref wasStaticColliding);
				num += Vector3.Magnitude(vector2 - vector) * trainContent.rigidBody.mass;
			}
			foreach (Rigidbody otherRigidbodyContent in trigger.otherRigidbodyContents)
			{
				trackSpeed = HandleRigidbodyCollision(atOurFront, trackSpeed, forwardVector, ourTotalMass, otherRigidbodyContent, otherRigidbodyContent.mass, deltaTime, calcSecondaryForces: true);
				num += Vector3.Magnitude(otherRigidbodyContent.velocity - vector) * otherRigidbodyContent.mass;
			}
		}
		if (num >= 70000f && (float)timeSinceLastChange > 1f && trigger.owner.ApplyCollisionDamage(num) > 5f)
		{
			foreach (Collider colliderContent in trigger.colliderContents)
			{
				trigger.owner.TryShowCollisionFX(colliderContent.ClosestPointOnBounds(trigger.owner.transform.position));
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
			if (flag)
			{
				HashSet<GameObject> hashSet2 = (front ? monitoredStaticContentF : monitoredStaticContentR);
				if (hashSet2.Count > 0)
				{
					bool flag2 = true;
					foreach (GameObject item in hashSet2)
					{
						if (item != null)
						{
							flag2 = false;
							break;
						}
					}
					if (flag2)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				trackSpeed = 0f;
			}
			else
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
			trackSpeed += num2 / TotalMass * deltaTime;
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
			else
			{
				prevTrackSpeeds.Add(theirRB, num);
			}
		}
		float value = num3 / ourTotalMass * deltaTime;
		value = Mathf.Clamp(value, 0f - Mathf.Abs(num) - 6f, Mathf.Abs(num) + 6f);
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
