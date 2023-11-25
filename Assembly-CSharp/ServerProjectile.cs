using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class ServerProjectile : EntityComponent<BaseEntity>, IServerComponent
{
	public interface IProjectileImpact
	{
		void ProjectileImpact(RaycastHit hitInfo, Vector3 rayOrigin);
	}

	public Vector3 initialVelocity;

	public float drag;

	public float gravityModifier = 1f;

	public float speed = 15f;

	public float scanRange;

	public Vector3 swimScale;

	public Vector3 swimSpeed;

	public float radius;

	public bool impacted;

	public float swimRandom;

	public virtual bool HasRangeLimit => true;

	protected virtual int mask => 1237003025;

	public Vector3 CurrentVelocity { get; set; }

	public float GetMaxRange(float maxFuseTime)
	{
		if (gravityModifier == 0f)
		{
			return float.PositiveInfinity;
		}
		float a = Mathf.Sin(MathF.PI / 2f) * speed * speed / (0f - Physics.gravity.y * gravityModifier);
		float b = speed * maxFuseTime;
		return Mathf.Min(a, b);
	}

	protected void FixedUpdate()
	{
		if (base.baseEntity != null && base.baseEntity.isServer)
		{
			DoMovement();
		}
	}

	public virtual bool ShouldSwim()
	{
		return swimScale != Vector3.zero;
	}

	public virtual bool DoMovement()
	{
		if (impacted)
		{
			return false;
		}
		CurrentVelocity += Physics.gravity * gravityModifier * Time.fixedDeltaTime * Time.timeScale;
		Vector3 currentVelocity = CurrentVelocity;
		if (ShouldSwim())
		{
			if (swimRandom == 0f)
			{
				swimRandom = UnityEngine.Random.Range(0f, 20f);
			}
			float num = Time.time + swimRandom;
			Vector3 direction = new Vector3(Mathf.Sin(num * swimSpeed.x) * swimScale.x, Mathf.Cos(num * swimSpeed.y) * swimScale.y, Mathf.Sin(num * swimSpeed.z) * swimScale.z);
			direction = base.transform.InverseTransformDirection(direction);
			currentVelocity += direction;
		}
		List<RaycastHit> obj = Pool.GetList<RaycastHit>();
		float num2 = currentVelocity.magnitude * Time.fixedDeltaTime;
		Vector3 position = base.transform.position;
		GamePhysics.TraceAll(new Ray(position, currentVelocity.normalized), radius, obj, num2 + scanRange, mask, QueryTriggerInteraction.Ignore);
		foreach (RaycastHit item in obj)
		{
			BaseEntity entity = RaycastHitEx.GetEntity(item);
			if ((!(entity != null) || !entity.isClient) && IsAValidHit(entity))
			{
				ColliderInfo colliderInfo = ((item.collider != null) ? item.collider.GetComponent<ColliderInfo>() : null);
				if (colliderInfo == null || colliderInfo.HasFlag(ColliderInfo.Flags.Shootable))
				{
					base.transform.position += base.transform.forward * Mathf.Max(0f, item.distance - 0.1f);
					GetComponent<IProjectileImpact>()?.ProjectileImpact(item, position);
					impacted = true;
					Pool.FreeList(ref obj);
					return false;
				}
			}
		}
		base.transform.position += base.transform.forward * num2;
		base.transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);
		Pool.FreeList(ref obj);
		return true;
	}

	protected virtual bool IsAValidHit(BaseEntity hitEnt)
	{
		if (BaseNetworkableEx.IsValid(hitEnt) && BaseNetworkableEx.IsValid(base.baseEntity.creatorEntity))
		{
			return hitEnt.net.ID != base.baseEntity.creatorEntity.net.ID;
		}
		return true;
	}

	public virtual void InitializeVelocity(Vector3 overrideVel)
	{
		base.transform.rotation = Quaternion.LookRotation(overrideVel.normalized);
		initialVelocity = overrideVel;
		CurrentVelocity = overrideVel;
	}
}
