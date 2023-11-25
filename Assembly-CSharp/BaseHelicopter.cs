using System;
using System.Collections.Generic;
using Oxide.Core;
using Rust;
using UnityEngine;

public class BaseHelicopter : BaseVehicle, SeekerTarget.ISeekerTargetOwner
{
	[Serializable]
	protected class GroundEffect
	{
		public ParticleSystem effect;

		public Transform groundPoint;
	}

	public class HelicopterInputState
	{
		public float throttle;

		public float roll;

		public float yaw;

		public float pitch;

		public bool groundControl;

		public void Reset()
		{
			throttle = 0f;
			roll = 0f;
			yaw = 0f;
			pitch = 0f;
			groundControl = false;
		}
	}

	[Header("Helicopter")]
	[SerializeField]
	public float engineThrustMax;

	[SerializeField]
	public Vector3 torqueScale;

	[SerializeField]
	protected Transform com;

	[SerializeField]
	public GameObject[] killTriggers;

	[SerializeField]
	protected GroundEffect[] groundEffects;

	[SerializeField]
	public GameObjectRef serverGibs;

	[SerializeField]
	public GameObjectRef explosionEffect;

	[SerializeField]
	public GameObjectRef fireBall;

	[SerializeField]
	public GameObjectRef crashEffect;

	[Tooltip("Lower values mean more lift is produced at high angles.")]
	[SerializeField]
	[Range(0.1f, 0.95f)]
	public float liftDotMax = 0.75f;

	[SerializeField]
	[Range(0.1f, 0.95f)]
	public float altForceDotMin = 0.85f;

	[SerializeField]
	[Range(0.1f, 0.95f)]
	public float liftFraction = 0.25f;

	[SerializeField]
	public float thrustLerpSpeed = 1f;

	public const Flags Flag_InternalLights = Flags.Reserved6;

	public float currentThrottle;

	public float avgThrust;

	public float avgTerrainHeight;

	public HelicopterInputState currentInputState = new HelicopterInputState();

	public float lastPlayerInputTime;

	public float hoverForceScale = 0.99f;

	public Vector3 damageTorque;

	public float nextDamageTime;

	public float nextEffectTime;

	public float pendingImpactDamage;

	public bool autoHover { get; set; }

	public virtual bool ForceMovementHandling => false;

	public virtual float GetServiceCeiling()
	{
		return 1000f;
	}

	public override float MaxVelocity()
	{
		return 50f;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		rigidBody.centerOfMass = com.localPosition;
		SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.MEDIUM);
	}

	internal override void DoServerDestroy()
	{
		SeekerTarget.SetSeekerTarget(this, SeekerTarget.SeekerStrength.OFF);
		base.DoServerDestroy();
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		if (IsDriver(player))
		{
			if (!autoHover)
			{
				PilotInput(inputState, player);
			}
		}
		else
		{
			PassengerInput(inputState, player);
		}
	}

	public bool ToggleAutoHover(BasePlayer player)
	{
		autoHover = !autoHover;
		if (autoHover && !IsEngineOn())
		{
			TryStartEngine(player);
		}
		return autoHover;
	}

	public virtual void PilotInput(InputState inputState, BasePlayer player)
	{
		currentInputState.Reset();
		currentInputState.throttle = (inputState.IsDown(BUTTON.FORWARD) ? 1f : 0f);
		currentInputState.throttle -= ((inputState.IsDown(BUTTON.BACKWARD) || inputState.IsDown(BUTTON.DUCK)) ? 1f : 0f);
		currentInputState.pitch = inputState.current.mouseDelta.y;
		currentInputState.roll = 0f - inputState.current.mouseDelta.x;
		currentInputState.yaw = (inputState.IsDown(BUTTON.RIGHT) ? 1f : 0f);
		currentInputState.yaw -= (inputState.IsDown(BUTTON.LEFT) ? 1f : 0f);
		currentInputState.pitch = MouseToBinary(currentInputState.pitch);
		currentInputState.roll = MouseToBinary(currentInputState.roll);
		lastPlayerInputTime = Time.time;
		static float MouseToBinary(float amount)
		{
			return Mathf.Clamp(amount, -1f, 1f);
		}
	}

	public virtual void PassengerInput(InputState inputState, BasePlayer player)
	{
	}

	public virtual void SetDefaultInputState()
	{
		currentInputState.Reset();
		if (HasDriver())
		{
			float num = Vector3.Dot(Vector3.up, base.transform.right);
			float num2 = Vector3.Dot(Vector3.up, base.transform.forward);
			currentInputState.roll = ((num < 0f) ? 1f : 0f);
			currentInputState.roll -= ((num > 0f) ? 1f : 0f);
			if (num2 < -0f)
			{
				currentInputState.pitch = -1f;
			}
			else if (num2 > 0f)
			{
				currentInputState.pitch = 1f;
			}
		}
		else
		{
			currentInputState.throttle = -1f;
		}
	}

	public virtual bool IsEnginePowered()
	{
		return true;
	}

	public override void VehicleFixedUpdate()
	{
		base.VehicleFixedUpdate();
		if (Time.time > lastPlayerInputTime + 0.5f)
		{
			SetDefaultInputState();
		}
		EnableGlobalBroadcast(IsEngineOn());
		if (IsEngineOn() || ForceMovementHandling)
		{
			MovementUpdate();
		}
		SetFlag(Flags.Reserved6, TOD_Sky.Instance.IsNight);
		GameObject[] array = killTriggers;
		foreach (GameObject obj in array)
		{
			bool active = rigidBody.velocity.y < 0f;
			obj.SetActive(active);
		}
	}

	public override void LightToggle(BasePlayer player)
	{
		if (IsDriver(player))
		{
			SetFlag(Flags.Reserved5, !HasFlag(Flags.Reserved5));
		}
	}

	public virtual bool IsEngineOn()
	{
		return true;
	}

	protected virtual void TryStartEngine(BasePlayer player)
	{
	}

	public void ClearDamageTorque()
	{
		SetDamageTorque(Vector3.zero);
	}

	public void SetDamageTorque(Vector3 newTorque)
	{
		damageTorque = newTorque;
	}

	public void AddDamageTorque(Vector3 torqueToAdd)
	{
		damageTorque += torqueToAdd;
	}

	public virtual void MovementUpdate()
	{
		HelicopterInputState helicopterInputState = currentInputState;
		if (autoHover)
		{
			float num = 50f - base.transform.position.y;
			helicopterInputState.throttle = Mathf.Clamp(num * 0.01f, -1f, 1f);
			helicopterInputState.pitch = 0f;
			helicopterInputState.roll = 0f;
			helicopterInputState.yaw = 0f;
		}
		if (helicopterInputState.groundControl)
		{
			currentThrottle = -0.75f;
		}
		else
		{
			currentThrottle = Mathf.Lerp(currentThrottle, helicopterInputState.throttle, 2f * Time.fixedDeltaTime);
			currentThrottle = Mathf.Clamp(currentThrottle, -0.8f, 1f);
			if (helicopterInputState.pitch != 0f || helicopterInputState.roll != 0f || helicopterInputState.yaw != 0f)
			{
				rigidBody.AddRelativeTorque(new Vector3(helicopterInputState.pitch * torqueScale.x, helicopterInputState.yaw * torqueScale.y, helicopterInputState.roll * torqueScale.z), ForceMode.Force);
			}
		}
		if (damageTorque != Vector3.zero)
		{
			rigidBody.AddRelativeTorque(new Vector3(damageTorque.x, damageTorque.y, damageTorque.z), ForceMode.Force);
		}
		avgThrust = Mathf.Lerp(avgThrust, engineThrustMax * currentThrottle, Time.fixedDeltaTime * thrustLerpSpeed);
		float value = Mathf.Clamp01(Vector3.Dot(base.transform.up, Vector3.up));
		float num2 = Mathf.InverseLerp(liftDotMax, 1f, value);
		float serviceCeiling = GetServiceCeiling();
		avgTerrainHeight = Mathf.Lerp(avgTerrainHeight, TerrainMeta.HeightMap.GetHeight(base.transform.position), Time.deltaTime);
		float num3 = 1f - Mathf.InverseLerp(avgTerrainHeight + serviceCeiling - 20f, avgTerrainHeight + serviceCeiling, base.transform.position.y);
		num2 *= num3;
		float num4 = 1f - Mathf.InverseLerp(altForceDotMin, 1f, value);
		Vector3 force = Vector3.up * engineThrustMax * liftFraction * currentThrottle * num2;
		Vector3 force2 = (base.transform.up - Vector3.up).normalized * engineThrustMax * currentThrottle * num4;
		float num5 = rigidBody.mass * (0f - Physics.gravity.y);
		rigidBody.AddForce(base.transform.up * num5 * num2 * hoverForceScale, ForceMode.Force);
		rigidBody.AddForce(force, ForceMode.Force);
		rigidBody.AddForce(force2, ForceMode.Force);
	}

	public void DelayedImpactDamage()
	{
		float num = explosionForceMultiplier;
		explosionForceMultiplier = 0f;
		Hurt(pendingImpactDamage * MaxHealth(), DamageType.Explosion, this, useProtection: false);
		pendingImpactDamage = 0f;
		explosionForceMultiplier = num;
	}

	public virtual bool CollisionDamageEnabled()
	{
		return true;
	}

	public void ProcessCollision(Collision collision)
	{
		if (base.isClient || !CollisionDamageEnabled() || Time.time < nextDamageTime)
		{
			return;
		}
		float magnitude = collision.relativeVelocity.magnitude;
		if ((bool)collision.gameObject)
		{
			if (((1 << collision.collider.gameObject.layer) & 0x48A18101) <= 0)
			{
				return;
			}
			BaseEntity entity = CollisionEx.GetEntity(collision);
			if (entity != null && entity is Parachute)
			{
				return;
			}
		}
		float num = Mathf.InverseLerp(5f, 30f, magnitude);
		if (!(num > 0f))
		{
			return;
		}
		pendingImpactDamage += Mathf.Max(num, 0.15f);
		if (Vector3.Dot(base.transform.up, Vector3.up) < 0.5f)
		{
			pendingImpactDamage *= 5f;
		}
		if (Time.time > nextEffectTime)
		{
			nextEffectTime = Time.time + 0.25f;
			if (crashEffect.isValid)
			{
				Vector3 point = collision.GetContact(0).point;
				point += (base.transform.position - point) * 0.25f;
				Effect.server.Run(crashEffect.resourcePath, point, base.transform.up);
			}
		}
		rigidBody.AddForceAtPosition(collision.GetContact(0).normal * (1f + 3f * num), collision.GetContact(0).point, ForceMode.VelocityChange);
		nextDamageTime = Time.time + 0.333f;
		Invoke(DelayedImpactDamage, 0.015f);
	}

	public void OnCollisionEnter(Collision collision)
	{
		ProcessCollision(collision);
	}

	public override void OnKilled(HitInfo info)
	{
		if (base.isClient)
		{
			base.OnKilled(info);
			return;
		}
		if (explosionEffect.isValid)
		{
			Effect.server.Run(explosionEffect.resourcePath, base.transform.position, Vector3.up, null, broadcast: true);
		}
		Vector3 vector = rigidBody.velocity * 0.25f;
		List<ServerGib> list = null;
		if (serverGibs.isValid)
		{
			GameObject gibSource = serverGibs.Get().GetComponent<ServerGib>()._gibSource;
			list = ServerGib.CreateGibs(serverGibs.resourcePath, base.gameObject, gibSource, vector, 3f);
		}
		Vector3 vector2 = CenterPoint();
		if (fireBall.isValid && !InSafeZone())
		{
			for (int i = 0; i < 12; i++)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(fireBall.resourcePath, vector2, base.transform.rotation);
				if (!baseEntity)
				{
					continue;
				}
				float minInclusive = 3f;
				float maxInclusive = 10f;
				Vector3 onUnitSphere = UnityEngine.Random.onUnitSphere;
				onUnitSphere.Normalize();
				float num = UnityEngine.Random.Range(0.5f, 4f);
				RaycastHit hitInfo;
				bool num2 = Physics.Raycast(vector2, onUnitSphere, out hitInfo, num, 1218652417);
				Vector3 position = hitInfo.point;
				if (!num2)
				{
					position = vector2 + onUnitSphere * num;
				}
				position -= onUnitSphere * 0.5f;
				baseEntity.transform.position = position;
				Collider component = baseEntity.GetComponent<Collider>();
				baseEntity.Spawn();
				baseEntity.SetVelocity(vector + onUnitSphere * UnityEngine.Random.Range(minInclusive, maxInclusive));
				if (list == null)
				{
					continue;
				}
				foreach (ServerGib item in list)
				{
					Physics.IgnoreCollision(component, item.GetCollider(), ignore: true);
				}
			}
		}
		base.OnKilled(info);
	}

	public virtual bool IsValidHomingTarget()
	{
		object obj = Interface.CallHook("CanBeHomingTargeted", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}
}
