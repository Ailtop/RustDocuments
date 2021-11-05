using System;
using System.Collections.Generic;
using Oxide.Core;
using Rust;
using Rust.Ai;
using UnityEngine;

public class TimedExplosive : BaseEntity, ServerProjectile.IProjectileImpact
{
	public interface IPreventSticking
	{
		bool CanStickTo(Collider collider);
	}

	public float timerAmountMin = 10f;

	public float timerAmountMax = 20f;

	public float minExplosionRadius;

	public float explosionRadius = 10f;

	public bool canStick;

	public bool onlyDamageParent;

	public GameObjectRef explosionEffect;

	[Tooltip("Optional: Will fall back to explosionEffect if not assigned.")]
	public GameObjectRef underwaterExplosionEffect;

	public GameObjectRef stickEffect;

	public GameObjectRef bounceEffect;

	public bool explosionUsesForward;

	public bool waterCausesExplosion;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	[NonSerialized]
	private float lastBounceTime;

	private CollisionDetectionMode? initialCollisionDetectionMode;

	public void SetDamageScale(float scale)
	{
		foreach (DamageTypeEntry damageType in damageTypes)
		{
			damageType.amount *= scale;
		}
	}

	public override float GetNetworkTime()
	{
		return Time.fixedTime;
	}

	public override void ServerInit()
	{
		lastBounceTime = Time.time;
		base.ServerInit();
		SetFuse(GetRandomTimerTime());
		ReceiveCollisionMessages(true);
		if (waterCausesExplosion)
		{
			InvokeRepeating(WaterCheck, 0f, 0.5f);
		}
	}

	public void WaterCheck()
	{
		if (waterCausesExplosion && WaterFactor() >= 0.5f)
		{
			Explode();
		}
	}

	public virtual void SetFuse(float fuseLength)
	{
		if (base.isServer)
		{
			object obj = Interface.CallHook("OnExplosiveFuseSet", this, fuseLength);
			if (obj is float)
			{
				fuseLength = (float)obj;
			}
			Invoke(Explode, fuseLength);
		}
	}

	public virtual float GetRandomTimerTime()
	{
		return UnityEngine.Random.Range(timerAmountMin, timerAmountMax);
	}

	public virtual void ProjectileImpact(RaycastHit info, Vector3 rayOrigin)
	{
		Explode();
	}

	public virtual void Explode()
	{
		Explode(PivotPoint());
	}

	public virtual void Explode(Vector3 explosionFxPos)
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		bool flag = false;
		if (underwaterExplosionEffect.isValid)
		{
			flag = WaterLevel.GetWaterDepth(base.transform.position) > 1f;
		}
		if (flag)
		{
			Effect.server.Run(underwaterExplosionEffect.resourcePath, explosionFxPos, explosionUsesForward ? base.transform.forward : Vector3.up, null, true);
		}
		else if (explosionEffect.isValid)
		{
			Effect.server.Run(explosionEffect.resourcePath, explosionFxPos, explosionUsesForward ? base.transform.forward : Vector3.up, null, true);
		}
		if (damageTypes.Count > 0)
		{
			Sensation sensation;
			if (onlyDamageParent)
			{
				DamageUtil.RadiusDamage(creatorEntity, LookupPrefab(), CenterPoint(), minExplosionRadius, explosionRadius, damageTypes, 166144, true);
				BaseEntity baseEntity = GetParentEntity();
				BaseCombatEntity baseCombatEntity = baseEntity as BaseCombatEntity;
				while (baseCombatEntity == null && baseEntity != null && baseEntity.HasParent())
				{
					baseEntity = baseEntity.GetParentEntity();
					baseCombatEntity = baseEntity as BaseCombatEntity;
				}
				if ((bool)baseCombatEntity)
				{
					HitInfo hitInfo = new HitInfo();
					hitInfo.Initiator = creatorEntity;
					hitInfo.WeaponPrefab = LookupPrefab();
					hitInfo.damageTypes.Add(damageTypes);
					baseCombatEntity.Hurt(hitInfo);
				}
				if (creatorEntity != null && damageTypes != null)
				{
					float num = 0f;
					foreach (DamageTypeEntry damageType in damageTypes)
					{
						num += damageType.amount;
					}
					sensation = default(Sensation);
					sensation.Type = SensationType.Explosion;
					sensation.Position = creatorEntity.transform.position;
					sensation.Radius = explosionRadius * 17f;
					sensation.DamagePotential = num;
					sensation.InitiatorPlayer = creatorEntity as BasePlayer;
					sensation.Initiator = creatorEntity;
					Sense.Stimulate(sensation);
				}
			}
			else
			{
				DamageUtil.RadiusDamage(creatorEntity, LookupPrefab(), CenterPoint(), minExplosionRadius, explosionRadius, damageTypes, 1076005121, true);
				if (creatorEntity != null && damageTypes != null)
				{
					float num2 = 0f;
					foreach (DamageTypeEntry damageType2 in damageTypes)
					{
						num2 += damageType2.amount;
					}
					sensation = default(Sensation);
					sensation.Type = SensationType.Explosion;
					sensation.Position = creatorEntity.transform.position;
					sensation.Radius = explosionRadius * 17f;
					sensation.DamagePotential = num2;
					sensation.InitiatorPlayer = creatorEntity as BasePlayer;
					sensation.Initiator = creatorEntity;
					Sense.Stimulate(sensation);
				}
			}
		}
		if (!base.IsDestroyed)
		{
			Kill(DestroyMode.Gib);
		}
	}

	public override void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		if (canStick && !IsStuck())
		{
			bool flag = true;
			if ((bool)hitEntity)
			{
				flag = CanStickTo(hitEntity, collision.collider);
				if (!flag)
				{
					Collider component = GetComponent<Collider>();
					if (collision.collider != null && component != null)
					{
						Physics.IgnoreCollision(collision.collider, component);
					}
				}
			}
			if (flag)
			{
				DoCollisionStick(collision, hitEntity);
			}
		}
		DoBounceEffect();
	}

	public virtual bool CanStickTo(BaseEntity entity, Collider collider)
	{
		return entity.GetComponent<IPreventSticking>()?.CanStickTo(collider) ?? true;
	}

	private void DoBounceEffect()
	{
		if (!bounceEffect.isValid || Time.time - lastBounceTime < 0.2f)
		{
			return;
		}
		Rigidbody component = GetComponent<Rigidbody>();
		if (!component || !(component.velocity.magnitude < 1f))
		{
			if (bounceEffect.isValid)
			{
				Effect.server.Run(bounceEffect.resourcePath, base.transform.position, Vector3.up, null, true);
			}
			lastBounceTime = Time.time;
		}
	}

	private void DoCollisionStick(Collision collision, BaseEntity ent)
	{
		ContactPoint contact = collision.GetContact(0);
		DoStick(contact.point, contact.normal, ent);
	}

	public virtual void SetMotionEnabled(bool wantsMotion)
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component)
		{
			if (!initialCollisionDetectionMode.HasValue)
			{
				initialCollisionDetectionMode = component.collisionDetectionMode;
			}
			component.useGravity = wantsMotion;
			if (!wantsMotion)
			{
				component.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
			component.isKinematic = !wantsMotion;
			if (wantsMotion)
			{
				component.collisionDetectionMode = initialCollisionDetectionMode.Value;
			}
		}
	}

	public bool IsStuck()
	{
		Rigidbody component = GetComponent<Rigidbody>();
		if ((bool)component && !component.isKinematic)
		{
			return false;
		}
		Collider component2 = GetComponent<Collider>();
		if ((bool)component2 && component2.enabled)
		{
			return false;
		}
		return parentEntity.IsValid(true);
	}

	public void DoStick(Vector3 position, Vector3 normal, BaseEntity ent)
	{
		if (ent == null)
		{
			return;
		}
		if (ent is TimedExplosive)
		{
			if (!ent.HasParent())
			{
				return;
			}
			position = ent.transform.position;
			ent = ent.parentEntity.Get(true);
		}
		SetMotionEnabled(false);
		SetCollisionEnabled(false);
		if (!HasChild(ent))
		{
			base.transform.position = position;
			base.transform.rotation = Quaternion.LookRotation(normal, base.transform.up);
			SetParent(ent, StringPool.closest, true);
			if (stickEffect.isValid)
			{
				Effect.server.Run(stickEffect.resourcePath, base.transform.position, Vector3.up, null, true);
			}
			ReceiveCollisionMessages(false);
		}
	}

	private void UnStick()
	{
		if ((bool)GetParentEntity())
		{
			SetParent(null, true, true);
			SetMotionEnabled(true);
			SetCollisionEnabled(true);
			ReceiveCollisionMessages(true);
		}
	}

	internal override void OnParentRemoved()
	{
		UnStick();
	}

	public virtual void SetCollisionEnabled(bool wantsCollision)
	{
		Collider component = GetComponent<Collider>();
		if ((bool)component && component.enabled != wantsCollision)
		{
			component.enabled = wantsCollision;
		}
	}
}
