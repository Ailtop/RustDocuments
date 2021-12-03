using System.Collections.Generic;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class SamSite : ContainerIOEntity
{
	public interface ISamSiteTarget
	{
		SamTargetType SAMTargetType { get; }

		bool isClient { get; }

		bool IsValidSAMTarget(bool staticRespawn);

		Vector3 CenterPoint();

		Vector3 GetWorldVelocity();

		bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity);
	}

	public class SamTargetType
	{
		public readonly float scanRadius;

		public readonly float speedMultiplier;

		public readonly float timeBetweenBursts;

		public SamTargetType(float scanRadius, float speedMultiplier, float timeBetweenBursts)
		{
			this.scanRadius = scanRadius;
			this.speedMultiplier = speedMultiplier;
			this.timeBetweenBursts = timeBetweenBursts;
		}
	}

	public Animator pitchAnimator;

	public GameObject yaw;

	public GameObject pitch;

	public GameObject gear;

	public Transform eyePoint;

	public float gearEpislonDegrees = 20f;

	public float turnSpeed = 1f;

	public float clientLerpSpeed = 1f;

	public Vector3 currentAimDir = Vector3.forward;

	public Vector3 targetAimDir = Vector3.forward;

	public float vehicleScanRadius = 350f;

	public float missileScanRadius = 500f;

	public GameObjectRef projectileTest;

	public GameObjectRef muzzleFlashTest;

	public bool staticRespawn;

	public ItemDefinition ammoType;

	public Transform[] tubes;

	[ServerVar(Help = "how long until static sam sites auto repair")]
	public static float staticrepairseconds = 1200f;

	public SoundDefinition yawMovementLoopDef;

	public float yawGainLerp = 8f;

	public float yawGainMovementSpeedMult = 0.1f;

	public SoundDefinition pitchMovementLoopDef;

	public float pitchGainLerp = 10f;

	public float pitchGainMovementSpeedMult = 0.5f;

	public static SamTargetType targetTypeUnknown;

	public static SamTargetType targetTypeVehicle;

	public static SamTargetType targetTypeMissile;

	public ISamSiteTarget currentTarget;

	public SamTargetType mostRecentTargetType;

	public Item ammoItem;

	public float lockOnTime;

	public float lastTargetVisibleTime;

	public int currentTubeIndex;

	private int firedCount;

	public float nextBurstTime;

	public override bool IsPowered()
	{
		if (!staticRespawn)
		{
			return HasFlag(Flags.Reserved8);
		}
		return true;
	}

	public override int ConsumptionAmount()
	{
		return 25;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}

	public void SetTarget(ISamSiteTarget target)
	{
		currentTarget = target;
		if (!ObjectEx.IsUnityNull(target))
		{
			mostRecentTargetType = target.SAMTargetType;
		}
	}

	public void ClearTarget()
	{
		SetTarget(null);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		targetTypeUnknown = new SamTargetType(vehicleScanRadius, 1f, 5f);
		targetTypeVehicle = new SamTargetType(vehicleScanRadius, 1f, 5f);
		targetTypeMissile = new SamTargetType(missileScanRadius, 2.25f, 3.5f);
		mostRecentTargetType = targetTypeUnknown;
		ClearTarget();
		InvokeRandomized(TargetScan, 1f, 3f, 1f);
		currentAimDir = base.transform.forward;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.samSite = Pool.Get<SAMSite>();
		info.msg.samSite.aimDir = GetAimDir();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (staticRespawn && HasFlag(Flags.Reserved1))
		{
			Invoke(SelfHeal, staticrepairseconds);
		}
	}

	public void SelfHeal()
	{
		lifestate = LifeState.Alive;
		base.health = startHealth;
		SetFlag(Flags.Reserved1, false);
	}

	public override void Die(HitInfo info = null)
	{
		if (staticRespawn)
		{
			ClearTarget();
			Quaternion quaternion = Quaternion.Euler(0f, Quaternion.LookRotation(currentAimDir, Vector3.up).eulerAngles.y, 0f);
			currentAimDir = quaternion * Vector3.forward;
			Invoke(SelfHeal, staticrepairseconds);
			lifestate = LifeState.Dead;
			base.health = 0f;
			SetFlag(Flags.Reserved1, true);
		}
		else
		{
			base.Die(info);
		}
	}

	public void FixedUpdate()
	{
		Vector3 vector = currentAimDir;
		if (!ObjectEx.IsUnityNull(currentTarget) && IsPowered())
		{
			float num = projectileTest.Get().GetComponent<ServerProjectile>().speed * currentTarget.SAMTargetType.speedMultiplier;
			Vector3 vector2 = currentTarget.CenterPoint();
			float num2 = Vector3.Distance(vector2, eyePoint.transform.position);
			float num3 = num2 / num;
			Vector3 a = vector2 + currentTarget.GetWorldVelocity() * num3;
			num3 = Vector3.Distance(a, eyePoint.transform.position) / num;
			a = vector2 + currentTarget.GetWorldVelocity() * num3;
			if (currentTarget.GetWorldVelocity().magnitude > 0.1f)
			{
				float num4 = Mathf.Sin(Time.time * 3f) * (1f + num3 * 0.5f);
				a += currentTarget.GetWorldVelocity().normalized * num4;
			}
			currentAimDir = (a - eyePoint.transform.position).normalized;
			if (num2 > currentTarget.SAMTargetType.scanRadius)
			{
				ClearTarget();
			}
		}
		Vector3 eulerAngles = Quaternion.LookRotation(currentAimDir, base.transform.up).eulerAngles;
		eulerAngles = BaseMountable.ConvertVector(eulerAngles);
		float t = Mathf.InverseLerp(0f, 90f, 0f - eulerAngles.x);
		float z = Mathf.Lerp(15f, -75f, t);
		Quaternion localRotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
		yaw.transform.localRotation = localRotation;
		Quaternion localRotation2 = Quaternion.Euler(pitch.transform.localRotation.eulerAngles.x, pitch.transform.localRotation.eulerAngles.y, z);
		pitch.transform.localRotation = localRotation2;
		if (currentAimDir != vector)
		{
			SendNetworkUpdate();
		}
	}

	public Vector3 GetAimDir()
	{
		return currentAimDir;
	}

	public bool HasValidTarget()
	{
		return !ObjectEx.IsUnityNull(currentTarget);
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.isServer && pickup.requireEmptyInv && base.inventory != null && base.inventory.itemList.Count > 0)
		{
			return false;
		}
		return !HasAmmo();
	}

	public void TargetScan()
	{
		if (!IsPowered())
		{
			lastTargetVisibleTime = 0f;
			return;
		}
		if (Time.time > lastTargetVisibleTime + 3f)
		{
			ClearTarget();
		}
		if (HasValidTarget() || IsDead())
		{
			return;
		}
		List<ISamSiteTarget> obj = Pool.GetList<ISamSiteTarget>();
		if (Interface.CallHook("OnSamSiteTargetScan", this, obj) == null)
		{
			_003CTargetScan_003Eg__AddTargetSet_007C48_0(obj, 32768, targetTypeVehicle.scanRadius);
			_003CTargetScan_003Eg__AddTargetSet_007C48_0(obj, 1048576, targetTypeMissile.scanRadius);
		}
		ISamSiteTarget samSiteTarget = null;
		foreach (ISamSiteTarget item in obj)
		{
			if (!item.isClient && !(item.CenterPoint().y < eyePoint.transform.position.y) && item.IsVisible(eyePoint.transform.position, item.SAMTargetType.scanRadius * 2f) && item.IsValidSAMTarget(staticRespawn) && Interface.CallHook("OnSamSiteTarget", this, item) == null)
			{
				samSiteTarget = item;
				break;
			}
		}
		if (!ObjectEx.IsUnityNull(samSiteTarget) && currentTarget != samSiteTarget)
		{
			lockOnTime = Time.time + 0.5f;
		}
		SetTarget(samSiteTarget);
		if (!ObjectEx.IsUnityNull(currentTarget))
		{
			lastTargetVisibleTime = Time.time;
		}
		Pool.FreeList(ref obj);
		if (ObjectEx.IsUnityNull(currentTarget))
		{
			CancelInvoke(WeaponTick);
		}
		else
		{
			InvokeRandomized(WeaponTick, 0f, 0.5f, 0.2f);
		}
	}

	public virtual bool HasAmmo()
	{
		if (!staticRespawn)
		{
			if (ammoItem != null && ammoItem.amount > 0)
			{
				return ammoItem.parent == base.inventory;
			}
			return false;
		}
		return true;
	}

	public void Reload()
	{
		if (staticRespawn)
		{
			return;
		}
		for (int i = 0; i < base.inventory.itemList.Count; i++)
		{
			Item item = base.inventory.itemList[i];
			if (item != null && item.info.itemid == ammoType.itemid && item.amount > 0)
			{
				ammoItem = item;
				return;
			}
		}
		ammoItem = null;
	}

	public void EnsureReloaded()
	{
		if (!HasAmmo())
		{
			Reload();
		}
	}

	public bool IsReloading()
	{
		return IsInvoking(Reload);
	}

	public void WeaponTick()
	{
		if (IsDead() || Time.time < lockOnTime || Time.time < nextBurstTime)
		{
			return;
		}
		if (!IsPowered())
		{
			firedCount = 0;
			return;
		}
		if (firedCount >= 6)
		{
			float timeBetweenBursts = mostRecentTargetType.timeBetweenBursts;
			nextBurstTime = Time.time + timeBetweenBursts;
			firedCount = 0;
			return;
		}
		EnsureReloaded();
		if (Interface.CallHook("CanSamSiteShoot", this) == null && HasAmmo())
		{
			if (!staticRespawn && ammoItem != null)
			{
				ammoItem.UseItem();
			}
			firedCount++;
			float speedMultiplier = 1f;
			if (!ObjectEx.IsUnityNull(currentTarget))
			{
				speedMultiplier = currentTarget.SAMTargetType.speedMultiplier;
			}
			FireProjectile(tubes[currentTubeIndex].position, currentAimDir, speedMultiplier);
			Effect.server.Run(muzzleFlashTest.resourcePath, this, StringPool.Get("Tube " + (currentTubeIndex + 1)), Vector3.zero, Vector3.up);
			currentTubeIndex++;
			if (currentTubeIndex >= tubes.Length)
			{
				currentTubeIndex = 0;
			}
		}
	}

	public void FireProjectile(Vector3 origin, Vector3 direction, float speedMultiplier)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity(projectileTest.resourcePath, origin, Quaternion.LookRotation(direction, Vector3.up));
		if (!(baseEntity == null))
		{
			baseEntity.creatorEntity = this;
			ServerProjectile component = baseEntity.GetComponent<ServerProjectile>();
			if ((bool)component)
			{
				component.InitializeVelocity(GetInheritedProjectileVelocity() + direction * component.speed * speedMultiplier);
			}
			baseEntity.Spawn();
		}
	}
}
