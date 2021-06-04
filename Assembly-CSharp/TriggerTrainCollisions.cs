using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerTrainCollisions : TriggerBase
{
	public enum ColliderLocation
	{
		Front,
		Rear
	}

	public Collider triggerCollider;

	public ColliderLocation location;

	public BaseTrain owner;

	[NonSerialized]
	public HashSet<GameObject> staticContents = new HashSet<GameObject>();

	[NonSerialized]
	public HashSet<BaseTrain> trainContents = new HashSet<BaseTrain>();

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
				BaseTrain componentInParent2 = obj.GetComponentInParent<BaseTrain>();
				if (componentInParent2 != null)
				{
					trainContents.Add(componentInParent2);
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
			BaseTrain componentInParent = obj.GetComponentInParent<BaseTrain>();
			if (componentInParent != null)
			{
				if (!_003COnObjectRemoved_003Eg__HasAnotherColliderFor_007C17_0(componentInParent))
				{
					trainContents.Remove(componentInParent);
				}
			}
			else
			{
				Rigidbody componentInParent2 = obj.GetComponentInParent<Rigidbody>();
				if (!_003COnObjectRemoved_003Eg__HasAnotherColliderFor_007C17_0(componentInParent2))
				{
					otherRigidbodyContents.Remove(componentInParent2);
				}
			}
		}
		base.OnObjectRemoved(obj);
	}
}
