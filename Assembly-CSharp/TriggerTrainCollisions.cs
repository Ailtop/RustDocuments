using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTrainCollisions : TriggerBase
{
	public enum Location
	{
		Front = 0,
		Rear = 1
	}

	public Collider triggerCollider;

	public Location location;

	public TrainCar owner;

	[NonSerialized]
	public HashSet<GameObject> staticContents = new HashSet<GameObject>();

	[NonSerialized]
	public HashSet<TrainCar> trainContents = new HashSet<TrainCar>();

	[NonSerialized]
	public HashSet<Rigidbody> otherRigidbodyContents = new HashSet<Rigidbody>();

	[NonSerialized]
	public HashSet<Collider> colliderContents = new HashSet<Collider>();

	private const float TICK_RATE = 0.2f;

	public bool HasAnyStaticContents => staticContents.Count > 0;

	public bool HasAnyTrainContents => trainContents.Count > 0;

	public bool HasAnyOtherRigidbodyContents => otherRigidbodyContents.Count > 0;

	public bool HasAnyNonStaticContents
	{
		get
		{
			if (!HasAnyTrainContents)
			{
				return HasAnyOtherRigidbodyContents;
			}
			return true;
		}
	}

	internal override void OnObjectAdded(GameObject obj, Collider col)
	{
		if (!owner.isServer)
		{
			return;
		}
		base.OnObjectAdded(obj, col);
		if (obj != null)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			if (baseEntity != null)
			{
				Vector3 vector = baseEntity.transform.position + baseEntity.transform.rotation * Vector3.Scale(obj.transform.lossyScale, baseEntity.bounds.center);
				Vector3 center = triggerCollider.bounds.center;
				Vector3 rhs = vector - center;
				bool flag = Vector3.Dot(owner.transform.forward, rhs) > 0f;
				if ((location == Location.Front && !flag) || (location == Location.Rear && flag))
				{
					return;
				}
			}
		}
		if (obj != null)
		{
			Rigidbody componentInParent = obj.GetComponentInParent<Rigidbody>();
			if (componentInParent != null)
			{
				TrainCar componentInParent2 = obj.GetComponentInParent<TrainCar>();
				if (componentInParent2 != null)
				{
					trainContents.Add(componentInParent2);
					if (owner.coupling != null)
					{
						owner.coupling.TryCouple(componentInParent2, location);
					}
					InvokeRepeating(TrainContentsTick, 0.2f, 0.2f);
				}
				else
				{
					otherRigidbodyContents.Add(componentInParent);
				}
			}
			else
			{
				ITrainCollidable componentInParent3 = obj.GetComponentInParent<ITrainCollidable>();
				if (componentInParent3 == null)
				{
					if (!obj.CompareTag("Railway"))
					{
						staticContents.Add(obj);
					}
				}
				else if (!componentInParent3.EqualNetID(owner) && !componentInParent3.CustomCollision(owner, this))
				{
					staticContents.Add(obj);
				}
			}
		}
		if (col != null)
		{
			colliderContents.Add(col);
		}
	}

	internal override void OnObjectRemoved(GameObject obj)
	{
		if (!owner.isServer || obj == null)
		{
			return;
		}
		Collider[] components = obj.GetComponents<Collider>();
		foreach (Collider item in components)
		{
			colliderContents.Remove(item);
		}
		if (!staticContents.Remove(obj))
		{
			TrainCar componentInParent = obj.GetComponentInParent<TrainCar>();
			if (componentInParent != null)
			{
				if (!HasAnotherColliderFor<TrainCar>(componentInParent))
				{
					trainContents.Remove(componentInParent);
					if (trainContents == null || trainContents.Count == 0)
					{
						CancelInvoke(TrainContentsTick);
					}
				}
			}
			else
			{
				Rigidbody componentInParent2 = obj.GetComponentInParent<Rigidbody>();
				if (!HasAnotherColliderFor<Rigidbody>(componentInParent2))
				{
					otherRigidbodyContents.Remove(componentInParent2);
				}
			}
		}
		base.OnObjectRemoved(obj);
		bool HasAnotherColliderFor<T>(T component) where T : Component
		{
			foreach (Collider colliderContent in colliderContents)
			{
				if (colliderContent != null && (UnityEngine.Object)colliderContent.GetComponentInParent<T>() == (UnityEngine.Object)component)
				{
					return true;
				}
			}
			return false;
		}
	}

	private void TrainContentsTick()
	{
		if (trainContents == null)
		{
			return;
		}
		foreach (TrainCar trainContent in trainContents)
		{
			if (BaseNetworkableEx.IsValid(trainContent) && !trainContent.IsDestroyed && owner.coupling != null)
			{
				owner.coupling.TryCouple(trainContent, location);
			}
		}
	}
}
