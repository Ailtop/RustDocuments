using System;
using Oxide.Core;
using UnityEngine;
using UnityEngine.AI;

public class NPCPlayer : BasePlayer
{
	public AIInformationZone VirtualInfoZone;

	public Vector3 finalDestination;

	[NonSerialized]
	private float randomOffset;

	[NonSerialized]
	public Vector3 spawnPos;

	public PlayerInventoryProperties[] loadouts;

	public LayerMask movementMask = 429990145;

	public bool LegacyNavigation = true;

	public NavMeshAgent NavAgent;

	public float damageScale = 1f;

	public float shortRange = 10f;

	public float attackLengthMaxShortRangeScale = 1f;

	private bool _isDormant;

	public float lastGunShotTime;

	public float triggerEndTime;

	public float nextTriggerTime;

	private float lastThinkTime;

	private float lastPositionUpdateTime;

	private float lastMovementTickTime;

	public Vector3 lastPos;

	public override bool IsNpc => true;

	public virtual bool IsDormant
	{
		get
		{
			return _isDormant;
		}
		set
		{
			_isDormant = value;
			bool isDormant = _isDormant;
		}
	}

	public override float PositionTickRate
	{
		protected get
		{
			return 0.1f;
		}
	}

	public virtual bool IsOnNavMeshLink
	{
		get
		{
			if (IsNavRunning())
			{
				return NavAgent.isOnOffMeshLink;
			}
			return false;
		}
	}

	public virtual bool HasPath
	{
		get
		{
			if (IsNavRunning())
			{
				return NavAgent.hasPath;
			}
			return false;
		}
	}

	public virtual bool IsLoadBalanced()
	{
		return false;
	}

	public override void ServerInit()
	{
		if (base.isClient)
		{
			return;
		}
		spawnPos = GetPosition();
		randomOffset = UnityEngine.Random.Range(0f, 1f);
		base.ServerInit();
		UpdateNetworkGroup();
		if (loadouts != null && loadouts.Length != 0)
		{
			loadouts[UnityEngine.Random.Range(0, loadouts.Length)].GiveToPlayer(this);
		}
		if (!IsLoadBalanced())
		{
			InvokeRepeating(ServerThink_Internal, 0f, 0.1f);
			lastThinkTime = Time.time;
		}
		Invoke(EquipTest, 0.25f);
		finalDestination = base.transform.position;
		if (NavAgent == null)
		{
			NavAgent = GetComponent<NavMeshAgent>();
		}
		if ((bool)NavAgent)
		{
			NavAgent.updateRotation = false;
			NavAgent.updatePosition = false;
			if (!LegacyNavigation)
			{
				base.transform.gameObject.GetComponent<BaseNavigator>().Init(this, NavAgent);
			}
		}
		InvokeRandomized(TickMovement, 1f, PositionTickRate, PositionTickRate * 0.1f);
	}

	public override void ApplyInheritedVelocity(Vector3 velocity)
	{
		ServerPosition = BaseNpc.GetNewNavPosWithVelocity(this, velocity);
	}

	public void RandomMove()
	{
		float num = 8f;
		Vector2 vector = UnityEngine.Random.insideUnitCircle * num;
		SetDestination(spawnPos + new Vector3(vector.x, 0f, vector.y));
	}

	public virtual void SetDestination(Vector3 newDestination)
	{
		finalDestination = newDestination;
	}

	public AttackEntity GetAttackEntity()
	{
		return GetHeldEntity() as AttackEntity;
	}

	public BaseProjectile GetGun()
	{
		AttackEntity attackEntity = GetHeldEntity() as AttackEntity;
		if (attackEntity == null)
		{
			return null;
		}
		BaseProjectile baseProjectile = attackEntity as BaseProjectile;
		if ((bool)baseProjectile)
		{
			return baseProjectile;
		}
		return null;
	}

	public virtual float AmmoFractionRemaining()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if ((bool)attackEntity)
		{
			return attackEntity.AmmoFraction();
		}
		return 0f;
	}

	public virtual bool IsReloading()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if (!attackEntity)
		{
			return false;
		}
		return attackEntity.ServerIsReloading();
	}

	public virtual void AttemptReload()
	{
		AttackEntity attackEntity = GetAttackEntity();
		if (!(attackEntity == null) && attackEntity.CanReload())
		{
			attackEntity.ServerReload();
		}
	}

	public virtual bool ShotTest(float targetDist)
	{
		AttackEntity attackEntity = GetHeldEntity() as AttackEntity;
		if (attackEntity == null)
		{
			return false;
		}
		BaseProjectile baseProjectile = attackEntity as BaseProjectile;
		if ((bool)baseProjectile)
		{
			if (baseProjectile.primaryMagazine.contents <= 0)
			{
				baseProjectile.ServerReload();
				return false;
			}
			if (baseProjectile.NextAttackTime > Time.time)
			{
				return false;
			}
		}
		if (!Mathf.Approximately(attackEntity.attackLengthMin, -1f))
		{
			if (IsInvoking(TriggerDown))
			{
				return true;
			}
			if (Time.time < nextTriggerTime)
			{
				return true;
			}
			InvokeRepeating(TriggerDown, 0f, 0.01f);
			if (targetDist <= shortRange)
			{
				triggerEndTime = Time.time + UnityEngine.Random.Range(attackEntity.attackLengthMin, attackEntity.attackLengthMax * attackLengthMaxShortRangeScale);
			}
			else
			{
				triggerEndTime = Time.time + UnityEngine.Random.Range(attackEntity.attackLengthMin, attackEntity.attackLengthMax);
			}
			TriggerDown();
			return true;
		}
		attackEntity.ServerUse(damageScale);
		lastGunShotTime = Time.time;
		return true;
	}

	public virtual float GetAimConeScale()
	{
		return 1f;
	}

	public void CancelBurst(float delay = 0.2f)
	{
		if (triggerEndTime > Time.time + delay)
		{
			triggerEndTime = Time.time + delay;
		}
	}

	public bool MeleeAttack()
	{
		AttackEntity attackEntity = GetHeldEntity() as AttackEntity;
		if (attackEntity == null)
		{
			return false;
		}
		BaseMelee baseMelee = attackEntity as BaseMelee;
		if (baseMelee == null)
		{
			return false;
		}
		baseMelee.ServerUse(damageScale);
		return true;
	}

	public virtual void TriggerDown()
	{
		AttackEntity attackEntity = GetHeldEntity() as AttackEntity;
		if (attackEntity != null)
		{
			attackEntity.ServerUse(damageScale);
		}
		lastGunShotTime = Time.time;
		if (Time.time > triggerEndTime)
		{
			CancelInvoke(TriggerDown);
			nextTriggerTime = Time.time + ((attackEntity != null) ? attackEntity.attackSpacing : 1f);
		}
	}

	public virtual void EquipWeapon()
	{
		if (inventory == null || inventory.containerBelt == null)
		{
			return;
		}
		Item slot = inventory.containerBelt.GetSlot(0);
		if (Interface.CallHook("OnNpcEquipWeapon", this, slot) != null || slot == null)
		{
			return;
		}
		UpdateActiveItem(inventory.containerBelt.GetSlot(0).uid);
		BaseEntity heldEntity = slot.GetHeldEntity();
		if (heldEntity != null)
		{
			AttackEntity component = heldEntity.GetComponent<AttackEntity>();
			if (component != null)
			{
				component.TopUpAmmo();
			}
		}
	}

	public void EquipTest()
	{
		EquipWeapon();
	}

	internal void ServerThink_Internal()
	{
		float delta = Time.time - lastThinkTime;
		ServerThink(delta);
		lastThinkTime = Time.time;
	}

	public virtual void ServerThink(float delta)
	{
		TickAi(delta);
	}

	public virtual void Resume()
	{
	}

	public virtual bool IsNavRunning()
	{
		return false;
	}

	public virtual void TickAi(float delta)
	{
	}

	public void TickMovement()
	{
		float delta = Time.realtimeSinceStartup - lastMovementTickTime;
		lastMovementTickTime = Time.realtimeSinceStartup;
		MovementUpdate(delta);
	}

	public override float GetNetworkTime()
	{
		if (Time.realtimeSinceStartup - lastPositionUpdateTime > PositionTickRate * 2f)
		{
			return Time.time;
		}
		return lastPositionUpdateTime;
	}

	public virtual void MovementUpdate(float delta)
	{
		if (!LegacyNavigation || base.isClient || !IsAlive() || IsWounded() || (!base.isMounted && !IsNavRunning()))
		{
			return;
		}
		if (IsDormant || !syncPosition)
		{
			if (IsNavRunning())
			{
				NavAgent.destination = ServerPosition;
			}
			return;
		}
		Vector3 moveToPosition = base.transform.position;
		if (HasPath)
		{
			moveToPosition = NavAgent.nextPosition;
		}
		if (ValidateNextPosition(ref moveToPosition))
		{
			UpdateSpeed(delta);
			UpdatePositionAndRotation(moveToPosition);
		}
	}

	private bool ValidateNextPosition(ref Vector3 moveToPosition)
	{
		if (!ValidBounds.Test(moveToPosition) && base.transform != null && !base.IsDestroyed)
		{
			Debug.Log(string.Concat("Invalid NavAgent Position: ", this, " ", moveToPosition.ToString(), " (destroying)"));
			Kill();
			return false;
		}
		return true;
	}

	private void UpdateSpeed(float delta)
	{
		float b = DesiredMoveSpeed();
		NavAgent.speed = Mathf.Lerp(NavAgent.speed, b, delta * 8f);
	}

	protected virtual void UpdatePositionAndRotation(Vector3 moveToPosition)
	{
		lastPositionUpdateTime = Time.time;
		ServerPosition = moveToPosition;
		SetAimDirection(GetAimDirection());
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public virtual float DesiredMoveSpeed()
	{
		float running = Mathf.Sin(Time.time + randomOffset);
		return GetSpeed(running, 0f, 0f);
	}

	public override bool EligibleForWounding(HitInfo info)
	{
		return false;
	}

	public virtual Vector3 GetAimDirection()
	{
		if (Vector3Ex.Distance2D(finalDestination, GetPosition()) >= 1f)
		{
			Vector3 normalized = (finalDestination - GetPosition()).normalized;
			return new Vector3(normalized.x, 0f, normalized.z);
		}
		return eyes.BodyForward();
	}

	public virtual void SetAimDirection(Vector3 newAim)
	{
		if (!(newAim == Vector3.zero))
		{
			AttackEntity attackEntity = GetAttackEntity();
			if ((bool)attackEntity)
			{
				newAim = attackEntity.ModifyAIAim(newAim);
			}
			eyes.rotation = Quaternion.LookRotation(newAim, Vector3.up);
			viewAngles = eyes.rotation.eulerAngles;
			ServerRotation = eyes.rotation;
			lastPositionUpdateTime = Time.time;
		}
	}
}
