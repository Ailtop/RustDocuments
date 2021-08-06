using UnityEngine;

public class ServerProjectile : EntityComponent<BaseEntity>, IServerComponent
{
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

	public Vector3 currentVelocity = Vector3.zero;

	protected virtual int mask => 1236478737;

	protected void FixedUpdate()
	{
		if (base.baseEntity.isServer)
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
		currentVelocity += Physics.gravity * gravityModifier * Time.fixedDeltaTime * Time.timeScale;
		Vector3 vector = currentVelocity;
		if (swimScale != Vector3.zero)
		{
			if (swimRandom == 0f)
			{
				swimRandom = Random.Range(0f, 20f);
			}
			float num = Time.time + swimRandom;
			Vector3 direction = new Vector3(Mathf.Sin(num * swimSpeed.x) * swimScale.x, Mathf.Cos(num * swimSpeed.y) * swimScale.y, Mathf.Sin(num * swimSpeed.z) * swimScale.z);
			direction = base.transform.InverseTransformDirection(direction);
			vector += direction;
		}
		float num2 = vector.magnitude * Time.fixedDeltaTime;
		RaycastHit hitInfo;
		if (GamePhysics.Trace(new Ray(base.transform.position, vector.normalized), radius, out hitInfo, num2 + scanRange, mask))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if (!BaseEntityEx.IsValid(entity) || !BaseEntityEx.IsValid(base.baseEntity.creatorEntity) || entity.net.ID != base.baseEntity.creatorEntity.net.ID)
			{
				base.transform.position += base.transform.forward * Mathf.Max(0f, hitInfo.distance - 0.1f);
				SendMessage("ProjectileImpact", hitInfo, SendMessageOptions.DontRequireReceiver);
				impacted = true;
				return false;
			}
		}
		base.transform.position += base.transform.forward * num2;
		base.transform.rotation = Quaternion.LookRotation(vector.normalized);
		return true;
	}

	public void InitializeVelocity(Vector3 overrideVel)
	{
		base.transform.rotation = Quaternion.LookRotation(overrideVel.normalized);
		initialVelocity = overrideVel;
		currentVelocity = overrideVel;
	}
}
