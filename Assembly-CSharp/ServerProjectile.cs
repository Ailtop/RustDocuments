using System;
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

	protected virtual int mask => 1236478737;

	public Vector3 CurrentVelocity { get; set; }

	public float GetMaxRange(float maxFuseTime)
	{
		if (gravityModifier == 0f)
		{
			return float.PositiveInfinity;
		}
		float a = Mathf.Sin((float)Math.PI / 2f) * speed * speed / (0f - Physics.gravity.y * gravityModifier);
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

	public virtual bool DoMovement()
	{
		if (impacted)
		{
			return false;
		}
		CurrentVelocity += Physics.gravity * gravityModifier * Time.fixedDeltaTime * Time.timeScale;
		Vector3 currentVelocity = CurrentVelocity;
		if (swimScale != Vector3.zero)
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
		float num2 = currentVelocity.magnitude * Time.fixedDeltaTime;
		Vector3 position = base.transform.position;
		RaycastHit hitInfo;
		if (GamePhysics.Trace(new Ray(position, currentVelocity.normalized), radius, out hitInfo, num2 + scanRange, mask, QueryTriggerInteraction.Ignore))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (IsAValidHit(entity))
			{
				base.transform.position += base.transform.forward * Mathf.Max(0f, hitInfo.distance - 0.1f);
				GetComponent<IProjectileImpact>()?.ProjectileImpact(hitInfo, position);
				impacted = true;
				return false;
			}
		}
		base.transform.position += base.transform.forward * num2;
		base.transform.rotation = Quaternion.LookRotation(currentVelocity.normalized);
		return true;
	}

	protected virtual bool IsAValidHit(BaseEntity hitEnt)
	{
		if (BaseEntityEx.IsValid(hitEnt) && BaseEntityEx.IsValid(base.baseEntity.creatorEntity))
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
