using Oxide.Core;
using Rust;
using UnityEngine;

public class EntityFuelSystem
{
	public readonly bool isServer;

	public readonly BaseEntity owner;

	public EntityRef fuelStorageInstance;

	public float nextFuelCheckTime;

	public bool cachedHasFuel;

	public float pendingFuel;

	public EntityFuelSystem(BaseEntity owner, bool isServer)
	{
		this.isServer = isServer;
		this.owner = owner;
	}

	public bool IsInFuelInteractionRange(BasePlayer player)
	{
		StorageContainer fuelContainer = GetFuelContainer();
		object obj = Interface.CallHook("CanCheckFuel", this, fuelContainer, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (fuelContainer != null)
		{
			float num = 0f;
			if (isServer)
			{
				num = 3f;
			}
			return fuelContainer.Distance(player.eyes.position) <= num;
		}
		return false;
	}

	public StorageContainer GetFuelContainer()
	{
		BaseEntity baseEntity = fuelStorageInstance.Get(isServer);
		if (baseEntity != null && BaseEntityEx.IsValid(baseEntity))
		{
			return baseEntity.GetComponent<StorageContainer>();
		}
		return null;
	}

	public void SpawnFuelStorage(GameObjectRef fuelStoragePrefab, Transform fuelStoragePoint, Collider colliderToIgnore = null)
	{
		if (fuelStoragePrefab != null && !(fuelStoragePoint == null) && !Rust.Application.isLoadingSave)
		{
			Vector3 pos = owner.transform.InverseTransformPoint(fuelStoragePoint.position);
			Quaternion rot = Quaternion.Inverse(owner.transform.rotation) * fuelStoragePoint.rotation;
			BaseEntity baseEntity = GameManager.server.CreateEntity(fuelStoragePrefab.resourcePath, pos, rot);
			baseEntity.SetParent(owner);
			baseEntity.Spawn();
			fuelStorageInstance.Set(baseEntity);
			Collider component = baseEntity.GetComponent<Collider>();
			if (colliderToIgnore != null && component != null)
			{
				Physics.IgnoreCollision(component, colliderToIgnore, true);
			}
		}
	}

	public Item GetFuelItem()
	{
		StorageContainer fuelContainer = GetFuelContainer();
		object obj = Interface.CallHook("OnFuelItemCheck", this, fuelContainer);
		if (obj is Item)
		{
			return (Item)obj;
		}
		if (fuelContainer == null)
		{
			return null;
		}
		return fuelContainer.inventory.GetSlot(0);
	}

	public int GetFuelAmount()
	{
		Item fuelItem = GetFuelItem();
		object obj = Interface.CallHook("OnFuelAmountCheck", this, fuelItem);
		if (obj is int)
		{
			return (int)obj;
		}
		if (fuelItem == null || fuelItem.amount < 1)
		{
			return 0;
		}
		return fuelItem.amount;
	}

	public bool HasFuel(bool forceCheck = false)
	{
		if (Time.time > nextFuelCheckTime || forceCheck)
		{
			object obj = Interface.CallHook("OnFuelCheck", this);
			if (obj is bool)
			{
				return (bool)obj;
			}
			cachedHasFuel = (float)GetFuelAmount() > 0f;
			nextFuelCheckTime = Time.time + UnityEngine.Random.Range(1f, 2f);
		}
		return cachedHasFuel;
	}

	public bool TryUseFuel(float seconds, float fuelUsedPerSecond)
	{
		StorageContainer fuelContainer = GetFuelContainer();
		object obj = Interface.CallHook("CanUseFuel", this, fuelContainer, seconds, fuelUsedPerSecond);
		if (obj != null)
		{
			if (!(obj is bool))
			{
				return false;
			}
			return (bool)obj;
		}
		if (fuelContainer == null)
		{
			return false;
		}
		Item slot = fuelContainer.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return false;
		}
		pendingFuel += seconds * fuelUsedPerSecond;
		if (pendingFuel >= 1f)
		{
			int num = Mathf.FloorToInt(pendingFuel);
			slot.UseItem(num);
			pendingFuel -= num;
		}
		return true;
	}

	public void LootFuel(BasePlayer player)
	{
		if (IsInFuelInteractionRange(player))
		{
			GetFuelContainer().PlayerOpenLoot(player);
		}
	}

	public void AddStartingFuel(float amount = -1f)
	{
		amount = ((amount == -1f) ? ((float)GetFuelContainer().allowedItem.stackable * 0.2f) : amount);
		GetFuelContainer().inventory.AddItem(GetFuelContainer().allowedItem, Mathf.FloorToInt(amount), 0uL);
	}

	public void AdminFillFuel()
	{
		GetFuelContainer().inventory.AddItem(GetFuelContainer().allowedItem, GetFuelContainer().allowedItem.stackable, 0uL);
	}
}
