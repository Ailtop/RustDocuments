using System;
using System.Collections.Generic;
using Rust;
using UnityEngine;

public abstract class GroundVehicle : BaseVehicle, IEngineControllerUser, IEntity, TriggerHurtNotChild.IHurtTriggerUser
{
	[Header("GroundVehicle")]
	[SerializeField]
	protected GroundVehicleAudio gvAudio;

	[SerializeField]
	public GameObjectRef collisionEffect;

	[SerializeField]
	private GameObjectRef fuelStoragePrefab;

	[SerializeField]
	public Transform waterloggedPoint;

	[SerializeField]
	public float engineStartupTime = 0.5f;

	[SerializeField]
	private float minCollisionDamageForce = 20000f;

	[SerializeField]
	private float maxCollisionDamageForce = 2500000f;

	[SerializeField]
	private float collisionDamageMultiplier = 1f;

	public VehicleEngineController<GroundVehicle> engineController;

	private Dictionary<BaseEntity, float> damageSinceLastTick = new Dictionary<BaseEntity, float>();

	private float nextCollisionDamageTime;

	private float nextCollisionFXTime;

	private float dragMod;

	private float dragModDuration;

	private TimeSince timeSinceDragModSet;

	public Vector3 Velocity { get; private set; }

	public abstract float DriveWheelVelocity { get; }

	public bool LightsAreOn => HasFlag(Flags.Reserved5);

	public VehicleEngineController<GroundVehicle>.EngineState CurEngineState => engineController.CurEngineState;

	public override void InitShared()
	{
		base.InitShared();
		engineController = new VehicleEngineController<GroundVehicle>(this, base.isServer, engineStartupTime, fuelStoragePrefab, waterloggedPoint);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (old != next && base.isServer)
		{
			ServerFlagsChanged(old, next);
		}
	}

	public float GetSpeed()
	{
		if (IsStationary())
		{
			return 0f;
		}
		return Vector3.Dot(Velocity, base.transform.forward);
	}

	public abstract float GetMaxForwardSpeed();

	public abstract float GetThrottleInput();

	public abstract float GetBrakeInput();

	protected override bool CanPushNow(BasePlayer pusher)
	{
		if (!base.CanPushNow(pusher))
		{
			return false;
		}
		if (pusher.isMounted || pusher.IsSwimming())
		{
			return false;
		}
		return !pusher.IsStandingOnEntity(this, 8192);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		timeSinceDragModSet = default(TimeSince);
		timeSinceDragModSet = float.MaxValue;
	}

	public abstract void OnEngineStartFailed();

	public abstract bool MeetsEngineRequirements();

	protected virtual void ServerFlagsChanged(Flags old, Flags next)
	{
	}

	protected void OnCollisionEnter(Collision collision)
	{
		if (base.isServer)
		{
			ProcessCollision(collision);
		}
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (base.IsMovingOrOn)
		{
			Velocity = GetLocalVelocity();
		}
		else
		{
			Velocity = Vector3.zero;
		}
		if (LightsAreOn && !AnyMounted())
		{
			SetFlag(Flags.Reserved5, false);
		}
		if (!(Time.time >= nextCollisionDamageTime))
		{
			return;
		}
		nextCollisionDamageTime = Time.time + 0.33f;
		foreach (KeyValuePair<BaseEntity, float> item in damageSinceLastTick)
		{
			DoCollisionDamage(item.Key, item.Value);
		}
		damageSinceLastTick.Clear();
	}

	public override void LightToggle(BasePlayer player)
	{
		if (IsDriver(player))
		{
			SetFlag(Flags.Reserved5, !LightsAreOn);
		}
	}

	public float GetPlayerDamageMultiplier()
	{
		return Mathf.Abs(GetSpeed()) * 1f;
	}

	public void OnHurtTriggerOccupant(BaseEntity hurtEntity, DamageType damageType, float damageTotal)
	{
		if (!base.isClient && !hurtEntity.IsDestroyed)
		{
			Vector3 vector = hurtEntity.GetLocalVelocity() - Velocity;
			Vector3 position = ClosestPoint(hurtEntity.transform.position);
			Vector3 vector2 = hurtEntity.RealisticMass * vector;
			rigidBody.AddForceAtPosition(vector2 * 1.25f, position, ForceMode.Impulse);
			QueueCollisionDamage(this, vector2.magnitude * 0.75f / Time.deltaTime);
			SetTempDrag(2.25f, 1f);
		}
	}

	private float QueueCollisionDamage(BaseEntity hitEntity, float forceMagnitude)
	{
		float num = Mathf.InverseLerp(minCollisionDamageForce, maxCollisionDamageForce, forceMagnitude);
		if (num > 0f)
		{
			float num2 = Mathf.Lerp(1f, 200f, num) * collisionDamageMultiplier;
			float value;
			if (damageSinceLastTick.TryGetValue(hitEntity, out value))
			{
				if (value < num2)
				{
					damageSinceLastTick[hitEntity] = num2;
				}
			}
			else
			{
				damageSinceLastTick[hitEntity] = num2;
			}
		}
		return num;
	}

	protected virtual void DoCollisionDamage(BaseEntity hitEntity, float damage)
	{
		Hurt(damage, DamageType.Collision, this, false);
	}

	private void ProcessCollision(Collision collision)
	{
		if (base.isClient || collision == null || collision.gameObject == null || collision.gameObject == null)
		{
			return;
		}
		ContactPoint contact = collision.GetContact(0);
		BaseEntity baseEntity = null;
		if (contact.otherCollider.attachedRigidbody == rigidBody)
		{
			baseEntity = GameObjectEx.ToBaseEntity(contact.otherCollider);
		}
		else if (contact.thisCollider.attachedRigidbody == rigidBody)
		{
			baseEntity = GameObjectEx.ToBaseEntity(contact.thisCollider);
		}
		if (baseEntity != null)
		{
			float forceMagnitude = collision.impulse.magnitude / Time.fixedDeltaTime;
			if (QueueCollisionDamage(baseEntity, forceMagnitude) > 0f)
			{
				ShowCollisionFX(collision);
			}
		}
	}

	private void ShowCollisionFX(Collision collision)
	{
		if (!(Time.time < nextCollisionFXTime))
		{
			nextCollisionFXTime = Time.time + 0.25f;
			if (collisionEffect.isValid)
			{
				Vector3 point = collision.GetContact(0).point;
				point += (base.transform.position - point) * 0.25f;
				Effect.server.Run(collisionEffect.resourcePath, point, base.transform.up);
			}
		}
	}

	public virtual float GetModifiedDrag()
	{
		return (1f - Mathf.InverseLerp(0f, dragModDuration, timeSinceDragModSet)) * dragMod;
	}

	public override EntityFuelSystem GetFuelSystem()
	{
		return engineController.FuelSystem;
	}

	protected override void OnChildAdded(BaseEntity child)
	{
		base.OnChildAdded(child);
		if (base.isServer && isSpawned)
		{
			GetFuelSystem().CheckNewChild(child);
		}
	}

	private void SetTempDrag(float drag, float duration)
	{
		dragMod = Mathf.Clamp(drag, 0f, 1000f);
		timeSinceDragModSet = 0f;
		dragModDuration = duration;
	}

	void IEngineControllerUser.Invoke(Action action, float time)
	{
		Invoke(action, time);
	}

	void IEngineControllerUser.CancelInvoke(Action action)
	{
		CancelInvoke(action);
	}
}
