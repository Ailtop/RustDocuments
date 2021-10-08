#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseOven : StorageContainer, ISplashable
{
	public enum TemperatureType
	{
		Normal,
		Warming,
		Cooking,
		Smelting,
		Fractioning
	}

	public TemperatureType temperature;

	public Menu.Option switchOnMenu;

	public Menu.Option switchOffMenu;

	public ItemAmount[] startupContents;

	public bool allowByproductCreation = true;

	public ItemDefinition fuelType;

	public bool canModFire;

	public bool disabledBySplash = true;

	private const float UpdateRate = 0.5f;

	public float cookingTemperature
	{
		get
		{
			switch (temperature)
			{
			case TemperatureType.Fractioning:
				return 1500f;
			case TemperatureType.Cooking:
				return 200f;
			case TemperatureType.Smelting:
				return 1000f;
			case TemperatureType.Warming:
				return 50f;
			default:
				return 15f;
			}
		}
	}

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
			item.SetFlag(Item.Flag.OnFire, false);
			item.MarkDirty();
		}
	}

	public void OvenFull()
	{
		StopCooking();
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
			if ((bool)item.info.GetComponent<ItemModBurnable>() && (fuelType == null || item.info == fuelType))
			{
				return item;
			}
		}
		return null;
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
			item.SetFlag(Item.Flag.OnFire, true);
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
			Item item = ItemManager.Create(burnable.byproductItem, burnable.byproductAmount, 0uL);
			if (!item.MoveToContainer(base.inventory))
			{
				OvenFull();
				item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
			}
		}
		if (fuel.amount <= 1)
		{
			fuel.Remove();
			return;
		}
		fuel.amount--;
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
		if (FindBurnable() != null)
		{
			base.inventory.temperature = cookingTemperature;
			UpdateAttachmentTemperature();
			InvokeRepeating(Cook, 0.5f, 0.5f);
			SetFlag(Flags.On, true);
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
					item.SetFlag(Item.Flag.OnFire, false);
					item.MarkDirty();
				}
			}
		}
		CancelInvoke(Cook);
		SetFlag(Flags.On, false);
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

	public override bool HasSlot(Slot slot)
	{
		if (canModFire && slot == Slot.FireMod)
		{
			return true;
		}
		return base.HasSlot(slot);
	}
}
