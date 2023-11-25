using System;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;

public class AttackHelicopterTurret : StorageContainer
{
	public enum GunStatus
	{
		NoWeapon = 0,
		Ready = 1,
		Reloading = 2,
		NoAmmo = 3
	}

	[SerializeField]
	public Transform turretSocket;

	[SerializeField]
	public Transform turretHorizontal;

	[SerializeField]
	public Transform turretVertical;

	[NonSerialized]
	public AttackHelicopter owner;

	public EntityRef<HeldEntity> attachedHeldEntity;

	[NonSerialized]
	public bool forceAcceptAmmo;

	public const float WEAPON_Z_OFFSET_SCALE = -0.5f;

	public float muzzleYOffset;

	public float lastSentX;

	public float lastSentY;

	public bool HasOwner => owner != null;

	public GunStatus GunState { get; set; }

	public float GunXAngle => turretVertical.localEulerAngles.x;

	public float GunYAngle => turretHorizontal.localEulerAngles.y;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AttackHelicopterTurret.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.attackHeliTurret != null)
		{
			_ = GunState;
			GunState = (GunStatus)info.msg.attackHeliTurret.gunState;
			float xRot = info.msg.attackHeliTurret.xRot;
			float yRot = info.msg.attackHeliTurret.yRot;
			SetGunRotation(xRot, yRot);
			attachedHeldEntity.uid = info.msg.attackHeliTurret.heldEntityID;
		}
	}

	public void SetGunRotation(float xRot, float yRot)
	{
		if (!(owner == null))
		{
			turretHorizontal.localEulerAngles = new Vector3(0f, yRot, 0f);
			turretVertical.localEulerAngles = new Vector3(0f - xRot, 0f, 0f);
		}
	}

	public HeldEntity GetAttachedHeldEntity()
	{
		HeldEntity heldEntity = attachedHeldEntity.Get(base.isServer);
		if (BaseNetworkableEx.IsValid(heldEntity))
		{
			return heldEntity;
		}
		return null;
	}

	public void GetAmmoAmounts(out int clip, out int available)
	{
		clip = 0;
		available = 0;
		if (base.isServer && GetAttachedHeldEntity() is BaseProjectile baseProjectile)
		{
			clip = baseProjectile.primaryMagazine.contents;
			available = base.inventory.GetAmmoAmount(baseProjectile.primaryMagazine.definition.ammoTypes);
		}
	}

	public Vector3 GetProjectedHitPos()
	{
		HeldEntity heldEntity = GetAttachedHeldEntity();
		if (heldEntity == null || heldEntity.MuzzleTransform == null)
		{
			return Ballistics.GetBulletHitPoint(turretSocket.position, turretSocket.forward);
		}
		return Ballistics.GetBulletHitPoint(heldEntity.MuzzleTransform.position, heldEntity.MuzzleTransform.forward);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		InvokeRandomized(RefreshGunState, 0f, 0.25f, 0.05f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (HasOwner)
		{
			info.msg.attackHeliTurret = Pool.Get<AttackHeliTurret>();
			GetAmmoAmounts(out var clip, out var available);
			info.msg.attackHeliTurret.clipAmmo = clip;
			info.msg.attackHeliTurret.totalAmmo = available;
			info.msg.attackHeliTurret.gunState = (int)GunState;
			info.msg.attackHeliTurret.xRot = turretVertical.localEulerAngles.x;
			info.msg.attackHeliTurret.yRot = turretHorizontal.localEulerAngles.y;
			info.msg.attackHeliTurret.heldEntityID = attachedHeldEntity.uid;
		}
	}

	public override BasePlayer ToPlayer()
	{
		if (HasOwner)
		{
			return owner.GetPassenger();
		}
		return null;
	}

	public bool CanAcceptItem(Item item, int targetSlot)
	{
		Item slot = base.inventory.GetSlot(0);
		if (IsValidWeapon(item) && targetSlot == 0)
		{
			return true;
		}
		if (item.info.category == ItemCategory.Ammunition)
		{
			if (forceAcceptAmmo)
			{
				return true;
			}
			if (slot == null || GetAttachedHeldEntity() == null)
			{
				return false;
			}
			if (targetSlot == 0)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsValidWeapon(Item item)
	{
		ItemDefinition info = item.info;
		if (item.isBroken)
		{
			return false;
		}
		ItemModEntity component = info.GetComponent<ItemModEntity>();
		if (component == null)
		{
			return false;
		}
		HeldEntity component2 = component.entityPrefab.Get().GetComponent<HeldEntity>();
		if (component2 == null)
		{
			return false;
		}
		if (!component2.IsUsableByTurret)
		{
			return false;
		}
		return true;
	}

	public bool InputTick(AttackHelicopter.GunnerInputState input)
	{
		if (!owner.GunnerIsInGunnerView)
		{
			return false;
		}
		bool result = false;
		if (input.reload)
		{
			TryReload();
		}
		else if (input.fire1)
		{
			result = TryFireWeapon();
		}
		input.eyeRay.direction = ClampEyeAngle(owner.transform, input.eyeRay.direction, owner.turretPitchClamp, owner.turretYawClamp);
		Vector3 bulletHitPoint = Ballistics.GetBulletHitPoint(input.eyeRay);
		bulletHitPoint.y -= muzzleYOffset;
		Vector3 direction = bulletHitPoint - turretSocket.position;
		direction = base.transform.InverseTransformDirection(direction);
		Vector3 eulerAngles = Quaternion.LookRotation(direction, Vector3.up).eulerAngles;
		float num = 0f - eulerAngles.x;
		float y = eulerAngles.y;
		SetGunRotation(num, y);
		if (Mathf.Abs(num - lastSentX) > 1f || Mathf.Abs(y - lastSentY) > 1f)
		{
			ClientRPC(null, "RPCRotation", GetNetworkTime(), num, y);
			lastSentX = num;
			lastSentY = y;
		}
		return result;
	}

	public Vector3 ClampEyeAngle(Transform heliTransform, Vector3 eyeDir, Vector2 pitchRange, Vector2 yawRange)
	{
		Vector3 vector = heliTransform.InverseTransformDirection(eyeDir);
		float x = Mathf.Clamp(Mathf.Asin(0f - vector.y) * 57.29578f, pitchRange.x, pitchRange.y);
		float value = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
		value = Mathf.Clamp(value, yawRange.x, yawRange.y);
		vector = Quaternion.Euler(x, value, 0f) * Vector3.forward;
		return heliTransform.TransformDirection(vector);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if ((bool)item.info.GetComponent<ItemModEntity>())
		{
			if (IsInvoking(UpdateAttachedWeapon))
			{
				UpdateAttachedWeapon();
			}
			Invoke(UpdateAttachedWeapon, 0.5f);
		}
	}

	public void UpdateAttachedWeapon()
	{
		if (!HasOwner)
		{
			Debug.LogError(GetType().Name + ": Turret socket not yet set.");
			return;
		}
		HeldEntity heldEntity = AutoTurret.TryAddWeaponToTurret(base.inventory.GetSlot(0), turretSocket, this, -0.5f);
		if (heldEntity != null)
		{
			attachedHeldEntity.Set(heldEntity);
			muzzleYOffset = turretSocket.InverseTransformPoint(heldEntity.MuzzleTransform.position).y;
		}
		else
		{
			HeldEntity heldEntity2 = GetAttachedHeldEntity();
			if (heldEntity2 != null)
			{
				heldEntity2.SetGenericVisible(wantsVis: false);
				heldEntity2.SetLightsOn(isOn: false);
			}
			attachedHeldEntity.Set(null);
			muzzleYOffset = 0f;
		}
		SendNetworkUpdate();
	}

	public bool TryReload()
	{
		BaseProjectile baseProjectile = GetAttachedHeldEntity() as BaseProjectile;
		if (baseProjectile == null)
		{
			return false;
		}
		return baseProjectile.ServerTryReload(base.inventory);
	}

	public bool TryFireWeapon()
	{
		HeldEntity heldEntity = GetAttachedHeldEntity();
		if (heldEntity == null)
		{
			return false;
		}
		if (owner.InSafeZone())
		{
			return false;
		}
		if (heldEntity is BaseProjectile baseProjectile)
		{
			if (baseProjectile.primaryMagazine.contents <= 0)
			{
				baseProjectile.ServerTryReload(base.inventory);
				return false;
			}
			if (baseProjectile.NextAttackTime > Time.time)
			{
				return false;
			}
		}
		heldEntity.ServerUse();
		GetAmmoAmounts(out var clip, out var available);
		ClientRPC(null, "RPCAmmo", (short)clip, (short)available);
		return true;
	}

	public void RefreshGunState()
	{
		HeldEntity heldEntity = GetAttachedHeldEntity();
		GunStatus gunStatus;
		if ((bool)heldEntity)
		{
			gunStatus = GunStatus.Ready;
			BaseProjectile baseProjectile = heldEntity as BaseProjectile;
			if (baseProjectile != null)
			{
				if (baseProjectile.ServerIsReloading())
				{
					gunStatus = GunStatus.Reloading;
				}
				else
				{
					GetAmmoAmounts(out var clip, out var available);
					if (clip == 0 && available == 0)
					{
						gunStatus = GunStatus.NoAmmo;
					}
				}
			}
		}
		else
		{
			gunStatus = GunStatus.NoWeapon;
		}
		if (gunStatus != GunState)
		{
			GunState = gunStatus;
			SendNetworkUpdate();
		}
	}
}
