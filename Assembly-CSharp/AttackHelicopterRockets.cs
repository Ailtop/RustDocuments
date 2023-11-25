using System;
using System.Collections.Generic;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;

public class AttackHelicopterRockets : StorageContainer
{
	[SerializeField]
	private Transform[] rocketMuzzlePositions;

	[SerializeField]
	private GameObjectRef rocketFireTubeFX;

	[SerializeField]
	public float timeBetweenRockets = 0.5f;

	[SerializeField]
	public float reloadTime = 8f;

	[SerializeField]
	public int rocketsPerReload = 6;

	[SerializeField]
	private ItemDefinition incendiaryRocketDef;

	[SerializeField]
	private ItemDefinition hvRocketDef;

	[SerializeField]
	private ItemDefinition flareItemDef;

	[NonSerialized]
	public AttackHelicopter owner;

	private const AmmoTypes ammoType = AmmoTypes.ROCKET;

	public TimeSince timeSinceRocketFired;

	private int rocketsSinceReload;

	private bool leftSide;

	public bool CanFireNow
	{
		get
		{
			if (!IsReloading && (float)timeSinceRocketFired >= timeBetweenRockets)
			{
				return GetAmmoAmount() > 0;
			}
			return false;
		}
	}

	public bool IsReloading
	{
		get
		{
			if (rocketsSinceReload >= rocketsPerReload && (float)timeSinceRocketFired < reloadTime)
			{
				return GetAmmoAmount() > 0;
			}
			return false;
		}
	}

	private bool HasOwner => owner != null;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AttackHelicopterRockets.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetAmmoAmount()
	{
		if (base.isServer)
		{
			return base.inventory.GetAmmoAmount(AmmoTypes.ROCKET);
		}
		return 0;
	}

	public int GetAmmoBeforeReload()
	{
		int b = ((rocketsSinceReload >= rocketsPerReload) ? rocketsSinceReload : (rocketsPerReload - rocketsSinceReload));
		return Mathf.Min(GetAmmoAmount(), b);
	}

	public bool TryGetAmmoDef(out ItemDefinition ammoDef)
	{
		ammoDef = null;
		if (base.isServer)
		{
			List<Item> obj = Pool.GetList<Item>();
			base.inventory.FindAmmo(obj, AmmoTypes.ROCKET);
			if (obj.Count > 0)
			{
				ammoDef = obj[obj.Count - 1].info;
			}
			Pool.FreeList(ref obj);
		}
		return ammoDef != null;
	}

	public Vector3 MuzzleMidPoint()
	{
		return (rocketMuzzlePositions[1].position + rocketMuzzlePositions[0].position) * 0.5f;
	}

	public float GetMinRocketSpeed()
	{
		return owner.GetSpeed() + 2f;
	}

	public bool TryGetProjectedHitPos(out Vector3 result)
	{
		result = Vector3.zero;
		if (!TryGetAmmoDef(out var ammoDef))
		{
			return false;
		}
		ItemModProjectile component = ammoDef.GetComponent<ItemModProjectile>();
		ServerProjectile component2 = component.projectileObject.Get().GetComponent<ServerProjectile>();
		if (component != null && component2 != null)
		{
			Vector3 origin = MuzzleMidPoint();
			Vector3 forward = owner.transform.forward;
			float minRocketSpeed = GetMinRocketSpeed();
			float gravity = Physics.gravity.y * component2.gravityModifier;
			Vector3 lhs = component2.initialVelocity + forward * component2.speed;
			if (minRocketSpeed > 0f)
			{
				float num = Vector3.Dot(lhs, forward) - minRocketSpeed;
				if (num < 0f)
				{
					lhs += forward * (0f - num);
				}
			}
			result = Ballistics.GetPhysicsProjectileHitPos(origin, lhs.normalized, lhs.magnitude, gravity, 1.5f, 0.5f, 32f, owner);
			return true;
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.attackHeliRockets = Pool.Get<AttackHeliRockets>();
		info.msg.attackHeliRockets.totalAmmo = GetAmmoAmount();
		info.msg.attackHeliRockets.rocketsSinceReload = rocketsSinceReload;
		if (TryGetAmmoDef(out var ammoDef))
		{
			info.msg.attackHeliRockets.ammoItemID = ammoDef.itemid;
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

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (!base.ItemFilter(item, targetSlot))
		{
			return false;
		}
		if (targetSlot == -1)
		{
			if (IsValidFlare())
			{
				for (int i = 12; i < base.inventory.capacity; i++)
				{
					if (!base.inventory.SlotTaken(item, i))
					{
						targetSlot = i;
						break;
					}
				}
			}
			else
			{
				if (!IsValidRocket())
				{
					return false;
				}
				for (int j = 0; j < 12; j++)
				{
					if (!base.inventory.SlotTaken(item, j))
					{
						targetSlot = j;
						break;
					}
				}
			}
		}
		if (targetSlot < 12)
		{
			return IsValidRocket();
		}
		return IsValidFlare();
		bool IsValidFlare()
		{
			return item.info == flareItemDef;
		}
		bool IsValidRocket()
		{
			if (!(item.info == incendiaryRocketDef))
			{
				return item.info == hvRocketDef;
			}
			return true;
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		if (added)
		{
			rocketsSinceReload = 0;
		}
		SendNetworkUpdate();
	}

	public bool InputTick(AttackHelicopter.GunnerInputState input, BasePlayer gunner)
	{
		if (!owner.GunnerIsInGunnerView)
		{
			return false;
		}
		bool result = false;
		if (input.fire2)
		{
			result = TryFireRocket(gunner);
		}
		return result;
	}

	public bool TryFireRocket(BasePlayer shooter)
	{
		if (!CanFireNow)
		{
			return false;
		}
		if (owner == null)
		{
			return false;
		}
		if (owner.InSafeZone())
		{
			return false;
		}
		int num = ((!leftSide) ? 1 : 0);
		Vector3 position = rocketMuzzlePositions[num].position;
		Vector3 forward = rocketMuzzlePositions[num].forward;
		float minRocketSpeed = GetMinRocketSpeed();
		if (owner.TryFireProjectile(this, AmmoTypes.ROCKET, position, forward, shooter, 1f, minRocketSpeed, out var _))
		{
			Effect.server.Run(rocketFireTubeFX.resourcePath, this, StringPool.Get(rocketMuzzlePositions[num].name), Vector3.zero, Vector3.zero, null, broadcast: true);
			leftSide = !leftSide;
			ItemDefinition ammoDef;
			int arg = (TryGetAmmoDef(out ammoDef) ? ammoDef.itemid : 0);
			timeSinceRocketFired = 0f;
			if (rocketsSinceReload < rocketsPerReload)
			{
				rocketsSinceReload++;
			}
			else
			{
				rocketsSinceReload = 1;
			}
			ClientRPC(null, "RPCUpdateAmmo", (short)GetAmmoAmount(), arg, rocketsSinceReload);
			return true;
		}
		return false;
	}

	public bool TryTakeFlare()
	{
		if (base.inventory.TryTakeOne(flareItemDef.itemid, out var item))
		{
			item.Remove();
			return true;
		}
		return false;
	}
}
