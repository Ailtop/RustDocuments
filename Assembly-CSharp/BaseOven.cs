#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseOven : StorageContainer, ISplashable
{
	public enum TemperatureType
	{
		Normal = 0,
		Warming = 1,
		Cooking = 2,
		Smelting = 3,
		Fractioning = 4
	}

	public struct MinMax
	{
		public int Min;

		public int Max;

		public MinMax(int min, int max)
		{
			Min = min;
			Max = max;
		}
	}

	private enum OvenItemType
	{
		Burnable = 0,
		Byproduct = 1,
		MaterialInput = 2,
		MaterialOutput = 3
	}

	private static Dictionary<float, HashSet<ItemDefinition>> _materialOutputCache;

	public TemperatureType temperature;

	public Menu.Option switchOnMenu;

	public Menu.Option switchOffMenu;

	public ItemAmount[] startupContents;

	public bool allowByproductCreation = true;

	public ItemDefinition fuelType;

	public bool canModFire;

	public bool disabledBySplash = true;

	public int smeltSpeed = 1;

	public int fuelSlots = 1;

	public int inputSlots = 1;

	public int outputSlots = 1;

	public int _activeCookingSlot;

	public int _inputSlotIndex;

	public int _outputSlotIndex;

	public const float UpdateRate = 0.5f;

	public float cookingTemperature => temperature switch
	{
		TemperatureType.Fractioning => 1500f, 
		TemperatureType.Cooking => 200f, 
		TemperatureType.Smelting => 1000f, 
		TemperatureType.Warming => 50f, 
		_ => 15f, 
	};

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseOven.OnRpcMessage"))
		{
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SVSwitch "));
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							SVSwitch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PreInitShared()
	{
		base.PreInitShared();
		_inputSlotIndex = fuelSlots;
		_outputSlotIndex = _inputSlotIndex + inputSlots;
	}

	public override void ServerInit()
	{
		inventorySlots = fuelSlots + inputSlots + outputSlots;
		base.ServerInit();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (IsOn())
		{
			StartCooking();
		}
	}

	public override void OnInventoryFirstCreated(ItemContainer container)
	{
		base.OnInventoryFirstCreated(container);
		if (startupContents != null)
		{
			ItemAmount[] array = startupContents;
			foreach (ItemAmount itemAmount in array)
			{
				ItemManager.Create(itemAmount.itemDef, (int)itemAmount.amount, 0uL).MoveToContainer(container);
			}
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		if (item != null && item.HasFlag(Item.Flag.OnFire))
		{
			item.SetFlag(Item.Flag.OnFire, b: false);
			item.MarkDirty();
		}
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (!base.ItemFilter(item, targetSlot))
		{
			return false;
		}
		if (targetSlot == -1)
		{
			return false;
		}
		if (IsOutputItem(item) && item.GetEntityOwner() != this)
		{
			BaseEntity entityOwner = item.GetEntityOwner();
			if (entityOwner != this && entityOwner != null)
			{
				return false;
			}
		}
		MinMax? allowedSlots = GetAllowedSlots(item);
		if (!allowedSlots.HasValue)
		{
			return false;
		}
		if (targetSlot >= allowedSlots.Value.Min)
		{
			return targetSlot <= allowedSlots.Value.Max;
		}
		return false;
	}

	public MinMax? GetAllowedSlots(Item item)
	{
		int num = 0;
		int num2 = 0;
		if (IsBurnableItem(item))
		{
			num2 = fuelSlots;
		}
		else if (IsOutputItem(item))
		{
			num = _outputSlotIndex;
			num2 = num + outputSlots;
		}
		else
		{
			if (!IsMaterialInput(item))
			{
				return null;
			}
			num = _inputSlotIndex;
			num2 = num + inputSlots;
		}
		return new MinMax(num, num2 - 1);
	}

	public MinMax GetOutputSlotRange()
	{
		return new MinMax(_outputSlotIndex, _outputSlotIndex + outputSlots - 1);
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		MinMax? allowedSlots = GetAllowedSlots(item);
		if (!allowedSlots.HasValue)
		{
			return -1;
		}
		for (int i = allowedSlots.Value.Min; i <= allowedSlots.Value.Max; i++)
		{
			Item slot = container.GetSlot(i);
			if (slot == null || (slot.CanStack(item) && slot.amount < slot.MaxStackable()))
			{
				return i;
			}
		}
		return base.GetIdealSlot(player, container, item);
	}

	public void OvenFull()
	{
		StopCooking();
	}

	private int GetFuelRate()
	{
		return 1;
	}

	private int GetCharcoalRate()
	{
		return 1;
	}

	public int GetSmeltingSpeed()
	{
		return smeltSpeed;
	}

	public void Cook()
	{
		Item item = FindBurnable();
		if (Interface.CallHook("OnOvenCook", this, item) != null)
		{
			return;
		}
		if (item == null)
		{
			StopCooking();
			return;
		}
		_activeCookingSlot = FindActiveCookingSlot();
		base.inventory.OnCycle(0.5f);
		BaseEntity slot = GetSlot(Slot.FireMod);
		if ((bool)slot)
		{
			slot.SendMessage("Cook", 0.5f, SendMessageOptions.DontRequireReceiver);
		}
		ItemModBurnable component = item.info.GetComponent<ItemModBurnable>();
		item.fuel -= 0.5f * (cookingTemperature / 200f);
		if (!item.HasFlag(Item.Flag.OnFire))
		{
			item.SetFlag(Item.Flag.OnFire, b: true);
			item.MarkDirty();
		}
		if (item.fuel <= 0f)
		{
			ConsumeFuel(item, component);
		}
		OnCooked();
		Interface.CallHook("OnOvenCooked", this, item, slot);
	}

	protected virtual void OnCooked()
	{
	}

	public void ConsumeFuel(Item fuel, ItemModBurnable burnable)
	{
		if (Interface.CallHook("OnFuelConsume", this, fuel, burnable) != null)
		{
			return;
		}
		if (allowByproductCreation && burnable.byproductItem != null && UnityEngine.Random.Range(0f, 1f) > burnable.byproductChance)
		{
			Item item = ItemManager.Create(burnable.byproductItem, burnable.byproductAmount * GetCharcoalRate(), 0uL);
			if (!item.MoveToContainer(base.inventory))
			{
				OvenFull();
				item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
			}
		}
		if (fuel.amount <= GetFuelRate())
		{
			fuel.Remove();
			return;
		}
		fuel.UseItem(GetFuelRate());
		fuel.fuel = burnable.fuelAmount;
		fuel.MarkDirty();
		Interface.CallHook("OnFuelConsumed", this, fuel, burnable);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	protected virtual void SVSwitch(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (Interface.CallHook("OnOvenToggle", this, msg.player) == null && flag != IsOn() && (!needsBuildingPrivilegeToUse || msg.player.CanBuild()))
		{
			if (flag)
			{
				StartCooking();
			}
			else
			{
				StopCooking();
			}
		}
	}

	public int FindActiveCookingSlot()
	{
		int num = _inputSlotIndex + inputSlots;
		for (int i = _inputSlotIndex; i <= num; i++)
		{
			if (base.inventory.GetSlot(i) != null)
			{
				return i;
			}
		}
		return -1;
	}

	public float GetTemperature(int slot)
	{
		if (slot != _activeCookingSlot)
		{
			return 15f;
		}
		if (!HasFlag(Flags.On))
		{
			return 15f;
		}
		return cookingTemperature;
	}

	public void UpdateAttachmentTemperature()
	{
		BaseEntity slot = GetSlot(Slot.FireMod);
		if ((bool)slot)
		{
			slot.SendMessage("ParentTemperatureUpdate", base.inventory.temperature, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void StartCooking()
	{
		if (Interface.CallHook("OnOvenStart", this) == null && FindBurnable() != null)
		{
			base.inventory.temperature = cookingTemperature;
			UpdateAttachmentTemperature();
			InvokeRepeating(Cook, 0.5f, 0.5f);
			SetFlag(Flags.On, b: true);
			Interface.CallHook("OnOvenStarted", this);
		}
	}

	public virtual void StopCooking()
	{
		UpdateAttachmentTemperature();
		if (base.inventory != null)
		{
			base.inventory.temperature = 15f;
			foreach (Item item in base.inventory.itemList)
			{
				if (item.HasFlag(Item.Flag.OnFire))
				{
					item.SetFlag(Item.Flag.OnFire, b: false);
					item.MarkDirty();
				}
			}
		}
		CancelInvoke(Cook);
		SetFlag(Flags.On, b: false);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed && IsOn())
		{
			return disabledBySplash;
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		StopCooking();
		return Mathf.Min(200, amount);
	}

	public Item FindBurnable()
	{
		object obj = Interface.CallHook("OnFindBurnable", this);
		if (obj is Item)
		{
			return (Item)obj;
		}
		if (base.inventory == null)
		{
			return null;
		}
		foreach (Item item in base.inventory.itemList)
		{
			if (IsBurnableItem(item))
			{
				return item;
			}
		}
		return null;
	}

	public bool IsBurnableItem(Item item)
	{
		if ((bool)item.info.GetComponent<ItemModBurnable>() && (fuelType == null || item.info == fuelType))
		{
			return true;
		}
		return false;
	}

	public bool IsBurnableByproduct(Item item)
	{
		ItemModBurnable itemModBurnable = fuelType?.GetComponent<ItemModBurnable>();
		if (itemModBurnable == null)
		{
			return false;
		}
		return item.info == itemModBurnable.byproductItem;
	}

	public bool IsMaterialInput(Item item)
	{
		ItemModCookable component = item.info.GetComponent<ItemModCookable>();
		if (component == null || (float)component.lowTemp > cookingTemperature || (float)component.highTemp < cookingTemperature)
		{
			return false;
		}
		return true;
	}

	public bool IsMaterialOutput(Item item)
	{
		if (_materialOutputCache == null)
		{
			BuildMaterialOutputCache();
		}
		if (!_materialOutputCache.TryGetValue(cookingTemperature, out var value))
		{
			Debug.LogError("Can't find smeltable item list for oven");
			return true;
		}
		return value.Contains(item.info);
	}

	public bool IsOutputItem(Item item)
	{
		if (!IsMaterialOutput(item))
		{
			return IsBurnableByproduct(item);
		}
		return true;
	}

	private void BuildMaterialOutputCache()
	{
		_materialOutputCache = new Dictionary<float, HashSet<ItemDefinition>>();
		float[] array = (from x in GameManager.server.preProcessed.prefabList.Values
			select x.GetComponent<BaseOven>() into x
			where x != null
			select x.cookingTemperature).Distinct().ToArray();
		foreach (float key in array)
		{
			HashSet<ItemDefinition> hashSet = new HashSet<ItemDefinition>();
			_materialOutputCache[key] = hashSet;
			foreach (ItemDefinition item in ItemManager.itemList)
			{
				ItemModCookable component = item.GetComponent<ItemModCookable>();
				if (!(component == null) && component.CanBeCookedByAtTemperature(key))
				{
					hashSet.Add(component.becomeOnCooked);
				}
			}
		}
	}

	public override bool HasSlot(Slot slot)
	{
		if (canModFire && slot == Slot.FireMod)
		{
			return true;
		}
		return base.HasSlot(slot);
	}
}
