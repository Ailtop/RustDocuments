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
			Rigidbody componentInParent = obj.GetComponentInParent<Rigidbody>();
			if (componentInParent != null)
			{
				TrainCar componentInParent2 = obj.GetComponentInParent<TrainCar>();
				if (componentInParent2 != null)
				{
					trainContents.Add(componentInParent2);
					if (owner.coupling != null)
					{
						owner.coupling.Touched(componentInParent2, location);
					}
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
					staticContents.Add(obj);
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
}
