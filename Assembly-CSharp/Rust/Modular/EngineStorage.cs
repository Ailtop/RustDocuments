using System;
using System.Linq;
using Facepunch;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

namespace Rust.Modular
{
	public class EngineStorage : StorageContainer
	{
		public enum EngineItemTypes
		{
			Crankshaft = 0,
			Carburetor = 1,
			SparkPlug = 2,
			Piston = 3,
			Valve = 4
		}

		[Header("Engine Storage")]
		public Sprite engineIcon;

		public float internalDamageMultiplier = 0.5f;

		public EngineItemTypes[] slotTypes;

		[SerializeField]
		public VehicleModuleEngineItems allEngineItems;

		[SerializeField]
		[ReadOnly]
		public int accelerationBoostSlots;

		[SerializeField]
		[ReadOnly]
		public int topSpeedBoostSlots;

		[SerializeField]
		[ReadOnly]
		public int fuelEconomyBoostSlots;

		public bool isUsable { get; set; }

		public float accelerationBoostPercent { get; set; }

		public float topSpeedBoostPercent { get; set; }

		public float fuelEconomyBoostPercent { get; set; }

		public VehicleModuleEngine GetEngineModule()
		{
			BaseEntity baseEntity = GetParentEntity();
			if (baseEntity != null)
			{
				return baseEntity.GetComponent<VehicleModuleEngine>();
			}
			return null;
		}

		public float GetAveragedLoadoutPercent()
		{
			return (accelerationBoostPercent + topSpeedBoostPercent + fuelEconomyBoostPercent) / 3f;
		}

		public override void Load(LoadInfo info)
		{
			base.Load(info);
			if (info.msg.engineStorage != null)
			{
				isUsable = info.msg.engineStorage.isUsable;
				accelerationBoostPercent = info.msg.engineStorage.accelerationBoost;
				topSpeedBoostPercent = info.msg.engineStorage.topSpeedBoost;
				fuelEconomyBoostPercent = info.msg.engineStorage.fuelEconomyBoost;
			}
			GetEngineModule()?.RefreshPerformanceStats(this);
		}

		public override bool CanBeLooted(BasePlayer player)
		{
			VehicleModuleEngine engineModule = GetEngineModule();
			if (engineModule != null)
			{
				return engineModule.CanBeLooted(player);
			}
			return false;
		}

		public int GetValidSlot(Item item)
		{
			ItemModEngineItem component = item.info.GetComponent<ItemModEngineItem>();
			if (component == null)
			{
				return -1;
			}
			ItemContainer rootContainer = item.GetRootContainer();
			EngineItemTypes engineItemType = component.engineItemType;
			for (int i = 0; i < inventorySlots; i++)
			{
				if (component.engineItemType == slotTypes[i] && !rootContainer.SlotTaken(item, i))
				{
					return i;
				}
			}
			return -1;
		}

		public override void OnInventoryFirstCreated(ItemContainer container)
		{
			RefreshLoadoutData();
		}

		public void NonUserSpawn()
		{
		}

		public override void OnItemAddedOrRemoved(Item item, bool added)
		{
			RefreshLoadoutData();
		}

		public override bool ItemFilter(Item item, int targetSlot)
		{
			if (!base.ItemFilter(item, targetSlot))
			{
				return false;
			}
			if (targetSlot < 0 || targetSlot >= slotTypes.Length)
			{
				return false;
			}
			ItemModEngineItem component = item.info.GetComponent<ItemModEngineItem>();
			if (component != null && component.engineItemType == slotTypes[targetSlot])
			{
				return true;
			}
			return false;
		}

		public void RefreshLoadoutData()
		{
			if (Interface.CallHook("OnEngineLoadoutRefresh", this) == null)
			{
				isUsable = base.inventory.IsFull() && base.inventory.itemList.All((Item item) => !item.isBroken);
				accelerationBoostPercent = GetContainerItemsValueFor(EngineItemTypeEx.BoostsAcceleration) / (float)accelerationBoostSlots;
				topSpeedBoostPercent = GetContainerItemsValueFor(EngineItemTypeEx.BoostsTopSpeed) / (float)topSpeedBoostSlots;
				fuelEconomyBoostPercent = GetContainerItemsValueFor(EngineItemTypeEx.BoostsFuelEconomy) / (float)fuelEconomyBoostSlots;
				SendNetworkUpdate();
				GetEngineModule()?.RefreshPerformanceStats(this);
			}
		}

		public override void Save(SaveInfo info)
		{
			base.Save(info);
			info.msg.engineStorage = Pool.Get<ProtoBuf.EngineStorage>();
			info.msg.engineStorage.isUsable = isUsable;
			info.msg.engineStorage.accelerationBoost = accelerationBoostPercent;
			info.msg.engineStorage.topSpeedBoost = topSpeedBoostPercent;
			info.msg.engineStorage.fuelEconomyBoost = fuelEconomyBoostPercent;
		}

		public void OnModuleDamaged(float damageTaken)
		{
			if (damageTaken <= 0f)
			{
				return;
			}
			damageTaken *= internalDamageMultiplier;
			float[] array = new float[base.inventory.capacity];
			float num = 0f;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = UnityEngine.Random.value;
				num += array[i];
			}
			float num2 = damageTaken / num;
			for (int j = 0; j < array.Length; j++)
			{
				Item slot = base.inventory.GetSlot(j);
				if (slot != null)
				{
					slot.condition -= array[j] * num2;
				}
			}
			RefreshLoadoutData();
		}

		public void AdminAddParts(int tier)
		{
			if (base.inventory == null)
			{
				Debug.LogWarning(GetType().Name + ": Null inventory on " + base.name);
				return;
			}
			for (int i = 0; i < base.inventory.capacity; i++)
			{
				Item slot = base.inventory.GetSlot(i);
				if (slot != null)
				{
					slot.RemoveFromContainer();
					slot.Remove();
				}
			}
			for (int j = 0; j < base.inventory.capacity; j++)
			{
				ItemModEngineItem output;
				if (base.inventory.GetSlot(j) == null && allEngineItems.TryGetItem(tier, slotTypes[j], out output))
				{
					ItemDefinition component = output.GetComponent<ItemDefinition>();
					Item item = ItemManager.Create(component, 1, 0uL);
					if (item != null)
					{
						item.condition = component.condition.max;
						item.MoveToContainer(base.inventory, j, false);
					}
					else
					{
						Debug.LogError(GetType().Name + ": Failed to create engine storage item.");
					}
				}
			}
		}

		public float GetContainerItemsValueFor(Func<EngineItemTypes, bool> boostConditional)
		{
			float num = 0f;
			foreach (Item item in base.inventory.itemList)
			{
				ItemModEngineItem component = item.info.GetComponent<ItemModEngineItem>();
				if (component != null && boostConditional(component.engineItemType) && !item.isBroken)
				{
					num += (float)item.amount * GetTierValue(component.tier);
				}
			}
			return num;
		}

		public float GetTierValue(int tier)
		{
			switch (tier)
			{
			case 1:
				return 0.6f;
			case 2:
				return 0.8f;
			case 3:
				return 1f;
			default:
				Debug.LogError(GetType().Name + ": Unrecognised item tier: " + tier);
				return 0f;
			}
		}
	}
}
