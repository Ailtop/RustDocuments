#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerInventory : EntityComponent<BasePlayer>
{
	public enum Type
	{
		Main,
		Belt,
		Wear
	}

	public interface ICanMoveFrom
	{
		bool CanMoveFrom(BasePlayer player, Item item);
	}

	public ItemContainer containerMain;

	public ItemContainer containerBelt;

	public ItemContainer containerWear;

	public ItemCrafter crafting;

	public PlayerLoot loot;

	[ServerVar]
	public static bool forceBirthday;

	private static float nextCheckTime;

	private static bool wasBirthday;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerInventory.OnRpcMessage"))
		{
			BaseEntity.RPCMessage rPCMessage;
			if (rpc == 3482449460u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ItemCmd "));
				}
				using (TimeWarning.New("ItemCmd"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.FromOwner.Test(3482449460u, "ItemCmd", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg2 = rPCMessage;
							ItemCmd(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ItemCmd");
					}
				}
				return true;
			}
			if (rpc == 3041092525u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - MoveItem "));
				}
				using (TimeWarning.New("MoveItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.FromOwner.Test(3041092525u, "MoveItem", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg3 = rPCMessage;
							MoveItem(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in MoveItem");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected void Initialize()
	{
		containerMain = new ItemContainer();
		containerMain.SetFlag(ItemContainer.Flag.IsPlayer, true);
		containerBelt = new ItemContainer();
		containerBelt.SetFlag(ItemContainer.Flag.IsPlayer, true);
		containerBelt.SetFlag(ItemContainer.Flag.Belt, true);
		containerWear = new ItemContainer();
		containerWear.SetFlag(ItemContainer.Flag.IsPlayer, true);
		containerWear.SetFlag(ItemContainer.Flag.Clothing, true);
		crafting = GetComponent<ItemCrafter>();
		crafting.AddContainer(containerMain);
		crafting.AddContainer(containerBelt);
		loot = GetComponent<PlayerLoot>();
		if (!loot)
		{
			loot = base.gameObject.AddComponent<PlayerLoot>();
		}
	}

	public void DoDestroy()
	{
		if (containerMain != null)
		{
			containerMain.Kill();
			containerMain = null;
		}
		if (containerBelt != null)
		{
			containerBelt.Kill();
			containerBelt = null;
		}
		if (containerWear != null)
		{
			containerWear.Kill();
			containerWear = null;
		}
	}

	public void ServerInit(BasePlayer owner)
	{
		Initialize();
		containerMain.ServerInitialize(null, 24);
		if (containerMain.uid == 0)
		{
			containerMain.GiveUID();
		}
		containerBelt.ServerInitialize(null, 6);
		if (containerBelt.uid == 0)
		{
			containerBelt.GiveUID();
		}
		containerWear.ServerInitialize(null, 7);
		if (containerWear.uid == 0)
		{
			containerWear.GiveUID();
		}
		containerMain.playerOwner = owner;
		containerBelt.playerOwner = owner;
		containerWear.playerOwner = owner;
		containerWear.onItemAddedRemoved = OnClothingChanged;
		containerWear.canAcceptItem = CanWearItem;
		containerBelt.canAcceptItem = CanEquipItem;
		containerMain.onPreItemRemove = OnItemRemoved;
		containerWear.onPreItemRemove = OnItemRemoved;
		containerBelt.onPreItemRemove = OnItemRemoved;
		containerMain.onDirty += OnContentsDirty;
		containerBelt.onDirty += OnContentsDirty;
		containerWear.onDirty += OnContentsDirty;
		containerBelt.onItemAddedRemoved = OnItemAddedOrRemoved;
		containerMain.onItemAddedRemoved = OnItemAddedOrRemoved;
	}

	public void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		if (item.info.isHoldable)
		{
			Invoke(UpdatedVisibleHolsteredItems, 0.1f);
		}
		if (bAdded)
		{
			BasePlayer basePlayer = base.baseEntity;
			if (!basePlayer.HasPlayerFlag(BasePlayer.PlayerFlags.DisplaySash) && basePlayer.IsHostileItem(item))
			{
				base.baseEntity.SetPlayerFlag(BasePlayer.PlayerFlags.DisplaySash, true);
			}
		}
	}

	public void UpdatedVisibleHolsteredItems()
	{
		List<HeldEntity> obj = Facepunch.Pool.GetList<HeldEntity>();
		List<Item> items = Facepunch.Pool.GetList<Item>();
		AllItemsNoAlloc(ref items);
		foreach (Item item in items)
		{
			if (item.info.isHoldable && !(item.GetHeldEntity() == null))
			{
				HeldEntity component = item.GetHeldEntity().GetComponent<HeldEntity>();
				if (!(component == null))
				{
					obj.Add(component);
				}
			}
		}
		Facepunch.Pool.FreeList(ref items);
		IOrderedEnumerable<HeldEntity> orderedEnumerable = obj.OrderByDescending((HeldEntity x) => x.hostileScore);
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		foreach (HeldEntity item2 in orderedEnumerable)
		{
			if (!(item2 == null) && item2.holsterInfo.displayWhenHolstered)
			{
				if (flag3 && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.BACK)
				{
					item2.SetVisibleWhileHolstered(true);
					flag3 = false;
				}
				else if (flag2 && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.RIGHT_THIGH)
				{
					item2.SetVisibleWhileHolstered(true);
					flag2 = false;
				}
				else if (flag && !item2.IsDeployed() && item2.holsterInfo.slot == HeldEntity.HolsterInfo.HolsterSlot.LEFT_THIGH)
				{
					item2.SetVisibleWhileHolstered(true);
					flag = false;
				}
				else
				{
					item2.SetVisibleWhileHolstered(false);
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	private void OnContentsDirty()
	{
		if (base.baseEntity != null)
		{
			base.baseEntity.InvalidateNetworkCache();
		}
	}

	private bool CanMoveItemsFrom(BaseEntity entity, Item item)
	{
		ICanMoveFrom canMoveFrom;
		if ((canMoveFrom = entity as ICanMoveFrom) != null && !canMoveFrom.CanMoveFrom(base.baseEntity, item))
		{
			return false;
		}
		if ((bool)BaseGameMode.GetActiveGameMode(true))
		{
			return BaseGameMode.GetActiveGameMode(true).CanMoveItemsFrom(this, entity, item);
		}
		return true;
	}

	[BaseEntity.RPC_Server]
	[BaseEntity.RPC_Server.FromOwner]
	private void ItemCmd(BaseEntity.RPCMessage msg)
	{
		if (msg.player != null && msg.player.IsWounded())
		{
			return;
		}
		uint id = msg.read.UInt32();
		string text = msg.read.String();
		Item item = FindItemUID(id);
		if (item == null || Interface.CallHook("OnItemAction", item, text, msg.player) != null || item.IsLocked() || !CanMoveItemsFrom(item.parent.entityOwner, item))
		{
			return;
		}
		if (text == "drop")
		{
			int num = item.amount;
			if (msg.read.Unread >= 4)
			{
				num = msg.read.Int32();
			}
			base.baseEntity.stats.Add("item_drop", 1, (Stats)5);
			if (num < item.amount)
			{
				item.SplitItem(num)?.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
			}
			else
			{
				item.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
			}
			base.baseEntity.SignalBroadcast(BaseEntity.Signal.Gesture, "drop_item");
		}
		else
		{
			item.ServerCommand(text, base.baseEntity);
			ItemManager.DoRemoves();
			ServerUpdate(0f);
		}
	}

	[BaseEntity.RPC_Server]
	[BaseEntity.RPC_Server.FromOwner]
	private void MoveItem(BaseEntity.RPCMessage msg)
	{
		uint num = msg.read.UInt32();
		uint num2 = msg.read.UInt32();
		int num3 = msg.read.Int8();
		int num4 = (int)msg.read.UInt32();
		Item item = FindItemUID(num);
		if (item == null)
		{
			msg.player.ChatMessage("Invalid item (" + num + ")");
		}
		else
		{
			if (Interface.CallHook("CanMoveItem", item, this, num2, num3, num4) != null)
			{
				return;
			}
			if (!CanMoveItemsFrom(item.parent.entityOwner, item))
			{
				msg.player.ChatMessage("Cannot move item!");
				return;
			}
			if (num4 <= 0)
			{
				num4 = item.amount;
			}
			num4 = Mathf.Clamp(num4, 1, item.MaxStackable());
			if (msg.player.GetActiveItem() == item)
			{
				msg.player.UpdateActiveItem(0u);
			}
			if (num2 == 0)
			{
				if (!GiveItem(item))
				{
					msg.player.ChatMessage("GiveItem failed!");
				}
				return;
			}
			ItemContainer itemContainer = FindContainer(num2);
			if (itemContainer == null)
			{
				msg.player.ChatMessage("Invalid container (" + num2 + ")");
				return;
			}
			ItemContainer parent = item.parent;
			if ((parent != null && parent.IsLocked()) || itemContainer.IsLocked())
			{
				msg.player.ChatMessage("Container is locked!");
				return;
			}
			if (itemContainer.PlayerItemInputBlocked())
			{
				msg.player.ChatMessage("Container does not accept player items!");
				return;
			}
			using (TimeWarning.New("Split"))
			{
				if (item.amount > num4)
				{
					int split_Amount = num4;
					if (itemContainer.maxStackSize > 0)
					{
						split_Amount = Mathf.Min(num4, itemContainer.maxStackSize);
					}
					Item item2 = item.SplitItem(split_Amount);
					if (!item2.MoveToContainer(itemContainer, num3))
					{
						item.amount += item2.amount;
						item2.Remove();
					}
					ItemManager.DoRemoves();
					ServerUpdate(0f);
					return;
				}
			}
			if (item.MoveToContainer(itemContainer, num3))
			{
				ItemManager.DoRemoves();
				ServerUpdate(0f);
			}
		}
	}

	private void OnClothingChanged(Item item, bool bAdded)
	{
		base.baseEntity.SV_ClothingChanged();
		ItemManager.DoRemoves();
		ServerUpdate(0f);
		Interface.CallHook("OnClothingItemChanged", this, item, bAdded);
	}

	private void OnItemRemoved(Item item)
	{
		base.baseEntity.InvalidateNetworkCache();
	}

	private bool CanEquipItem(Item item, int targetSlot)
	{
		object obj = Interface.CallHook("CanEquipItem", this, item, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		ItemModContainerRestriction component = item.info.GetComponent<ItemModContainerRestriction>();
		if (component == null)
		{
			return true;
		}
		Item[] array = containerBelt.itemList.ToArray();
		foreach (Item item2 in array)
		{
			if (item2 != item)
			{
				ItemModContainerRestriction component2 = item2.info.GetComponent<ItemModContainerRestriction>();
				if (!(component2 == null) && !component.CanExistWith(component2) && !item2.MoveToContainer(containerMain))
				{
					item2.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
				}
			}
		}
		return true;
	}

	private bool CanWearItem(Item item, int targetSlot)
	{
		ItemModWearable component = item.info.GetComponent<ItemModWearable>();
		if (component == null)
		{
			return false;
		}
		object obj = Interface.CallHook("CanWearItem", this, item, targetSlot);
		if (obj is bool)
		{
			return (bool)obj;
		}
		Item[] array = containerWear.itemList.ToArray();
		foreach (Item item2 in array)
		{
			if (item2 == item)
			{
				continue;
			}
			ItemModWearable component2 = item2.info.GetComponent<ItemModWearable>();
			if (!(component2 == null) && !component.CanExistWith(component2))
			{
				bool flag = false;
				if (item.parent == containerBelt)
				{
					flag = item2.MoveToContainer(containerBelt);
				}
				if (!flag && !item2.MoveToContainer(containerMain))
				{
					item2.Drop(base.baseEntity.GetDropPosition(), base.baseEntity.GetDropVelocity());
				}
			}
		}
		return true;
	}

	public void ServerUpdate(float delta)
	{
		loot.Check();
		if (delta > 0f)
		{
			crafting.ServerUpdate(delta);
		}
		float currentTemperature = base.baseEntity.currentTemperature;
		UpdateContainer(delta, Type.Main, containerMain, false, currentTemperature);
		UpdateContainer(delta, Type.Belt, containerBelt, true, currentTemperature);
		UpdateContainer(delta, Type.Wear, containerWear, true, currentTemperature);
	}

	public void UpdateContainer(float delta, Type type, ItemContainer container, bool bSendInventoryToEveryone, float temperature)
	{
		if (container != null)
		{
			container.temperature = temperature;
			if (delta > 0f)
			{
				container.OnCycle(delta);
			}
			if (container.dirty)
			{
				SendUpdatedInventory(type, container, bSendInventoryToEveryone);
				base.baseEntity.InvalidateNetworkCache();
			}
		}
	}

	public void SendSnapshot()
	{
		using (TimeWarning.New("PlayerInventory.SendSnapshot"))
		{
			SendUpdatedInventory(Type.Main, containerMain);
			SendUpdatedInventory(Type.Belt, containerBelt, true);
			SendUpdatedInventory(Type.Wear, containerWear, true);
		}
	}

	public void SendUpdatedInventory(Type type, ItemContainer container, bool bSendInventoryToEveryone = false)
	{
		using (UpdateItemContainer updateItemContainer = Facepunch.Pool.Get<UpdateItemContainer>())
		{
			updateItemContainer.type = (int)type;
			if (container != null)
			{
				container.dirty = false;
				updateItemContainer.container = Facepunch.Pool.Get<List<ProtoBuf.ItemContainer>>();
				updateItemContainer.container.Add(container.Save());
			}
			if (Interface.CallHook("OnInventoryNetworkUpdate", this, container, updateItemContainer, type, bSendInventoryToEveryone) == null)
			{
				if (bSendInventoryToEveryone)
				{
					base.baseEntity.ClientRPC(null, "UpdatedItemContainer", updateItemContainer);
				}
				else
				{
					base.baseEntity.ClientRPCPlayer(null, base.baseEntity, "UpdatedItemContainer", updateItemContainer);
				}
			}
		}
	}

	public Item FindItemUID(uint id)
	{
		if (id == 0)
		{
			return null;
		}
		if (containerMain != null)
		{
			Item item = containerMain.FindItemByUID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindItemByUID(id);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindItemByUID(id);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return loot.FindItem(id);
	}

	public Item FindItemID(string itemName)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemName);
		if (itemDefinition == null)
		{
			return null;
		}
		return FindItemID(itemDefinition.itemid);
	}

	public Item FindItemID(int id)
	{
		if (containerMain != null)
		{
			Item item = containerMain.FindItemByItemID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		if (containerBelt != null)
		{
			Item item2 = containerBelt.FindItemByItemID(id);
			if (item2 != null && item2.IsValid())
			{
				return item2;
			}
		}
		if (containerWear != null)
		{
			Item item3 = containerWear.FindItemByItemID(id);
			if (item3 != null && item3.IsValid())
			{
				return item3;
			}
		}
		return null;
	}

	public List<Item> FindItemIDs(int id)
	{
		List<Item> list = new List<Item>();
		if (containerMain != null)
		{
			list.AddRange(containerMain.FindItemsByItemID(id));
		}
		if (containerBelt != null)
		{
			list.AddRange(containerBelt.FindItemsByItemID(id));
		}
		if (containerWear != null)
		{
			list.AddRange(containerWear.FindItemsByItemID(id));
		}
		return list;
	}

	public ItemContainer FindContainer(uint id)
	{
		using (TimeWarning.New("FindContainer"))
		{
			ItemContainer itemContainer = containerMain.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			itemContainer = containerBelt.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			itemContainer = containerWear.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
			return loot.FindContainer(id);
		}
	}

	public ItemContainer GetContainer(Type id)
	{
		if (id == Type.Main)
		{
			return containerMain;
		}
		if (Type.Belt == id)
		{
			return containerBelt;
		}
		if (Type.Wear == id)
		{
			return containerWear;
		}
		return null;
	}

	public bool GiveItem(Item item, ItemContainer container = null)
	{
		if (item == null)
		{
			return false;
		}
		int position = -1;
		GetIdealPickupContainer(item, ref container, ref position);
		if (container != null && item.MoveToContainer(container, position))
		{
			return true;
		}
		if (item.MoveToContainer(containerMain))
		{
			return true;
		}
		if (item.MoveToContainer(containerBelt))
		{
			return true;
		}
		return false;
	}

	protected void GetIdealPickupContainer(Item item, ref ItemContainer container, ref int position)
	{
		if (item.info.stackable > 1)
		{
			if (containerBelt != null && containerBelt.FindItemByItemID(item.info.itemid) != null)
			{
				container = containerBelt;
				return;
			}
			if (containerMain != null && containerMain.FindItemByItemID(item.info.itemid) != null)
			{
				container = containerMain;
				return;
			}
		}
		if (item.info.isUsable && !item.info.HasFlag(ItemDefinition.Flag.NotStraightToBelt))
		{
			container = containerBelt;
		}
	}

	public void Strip()
	{
		containerMain.Clear();
		containerBelt.Clear();
		containerWear.Clear();
		ItemManager.DoRemoves();
	}

	public static bool IsBirthday()
	{
		if (forceBirthday)
		{
			return true;
		}
		if (UnityEngine.Time.time < nextCheckTime)
		{
			return wasBirthday;
		}
		nextCheckTime = UnityEngine.Time.time + 60f;
		DateTime now = DateTime.Now;
		wasBirthday = now.Day == 11 && now.Month == 12;
		return wasBirthday;
	}

	public static bool IsChristmas()
	{
		return XMas.enabled;
	}

	public void GiveDefaultItems()
	{
		if (Interface.CallHook("OnDefaultItemsReceive", this) != null)
		{
			return;
		}
		Strip();
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(true);
		if (activeGameMode != null && activeGameMode.HasLoadouts())
		{
			BaseGameMode.GetActiveGameMode(true).LoadoutPlayer(base.baseEntity);
			return;
		}
		ulong skin = 0uL;
		int infoInt = base.baseEntity.GetInfoInt("client.rockskin", 0);
		if (infoInt > 0 && base.baseEntity.blueprints.steamInventory.HasItem(infoInt))
		{
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition("rock");
			if (itemDefinition != null && ItemDefinition.FindSkin(itemDefinition.itemid, infoInt) != 0L)
			{
				IPlayerItemDefinition itemDefinition2 = PlatformService.Instance.GetItemDefinition(infoInt);
				if (itemDefinition2 != null)
				{
					skin = itemDefinition2.WorkshopDownload;
				}
			}
		}
		GiveItem(ItemManager.CreateByName("rock", 1, skin), containerBelt);
		GiveItem(ItemManager.CreateByName("torch", 1, 0uL), containerBelt);
		if (IsBirthday())
		{
			GiveItem(ItemManager.CreateByName("cakefiveyear", 1, 0uL), containerBelt);
			GiveItem(ItemManager.CreateByName("partyhat", 1, 0uL), containerWear);
		}
		if (IsChristmas())
		{
			GiveItem(ItemManager.CreateByName("snowball", 1, 0uL), containerBelt);
			GiveItem(ItemManager.CreateByName("snowball", 1, 0uL), containerBelt);
			GiveItem(ItemManager.CreateByName("snowball", 1, 0uL), containerBelt);
		}
		Interface.CallHook("OnDefaultItemsReceived", this);
	}

	public ProtoBuf.PlayerInventory Save(bool bForDisk)
	{
		ProtoBuf.PlayerInventory playerInventory = Facepunch.Pool.Get<ProtoBuf.PlayerInventory>();
		if (bForDisk)
		{
			playerInventory.invMain = containerMain.Save();
		}
		playerInventory.invBelt = containerBelt.Save();
		playerInventory.invWear = containerWear.Save();
		return playerInventory;
	}

	public void Load(ProtoBuf.PlayerInventory msg)
	{
		if (msg.invMain != null)
		{
			containerMain.Load(msg.invMain);
		}
		if (msg.invBelt != null)
		{
			containerBelt.Load(msg.invBelt);
		}
		if (msg.invWear != null)
		{
			containerWear.Load(msg.invWear);
		}
	}

	public int Take(List<Item> collect, int itemid, int amount)
	{
		int num = 0;
		if (containerMain != null)
		{
			int num2 = containerMain.Take(collect, itemid, amount);
			num += num2;
			amount -= num2;
		}
		if (amount <= 0)
		{
			return num;
		}
		if (containerBelt != null)
		{
			int num3 = containerBelt.Take(collect, itemid, amount);
			num += num3;
			amount -= num3;
		}
		if (amount <= 0)
		{
			return num;
		}
		if (containerWear != null)
		{
			int num4 = containerWear.Take(collect, itemid, amount);
			num += num4;
			amount -= num4;
		}
		return num;
	}

	public int GetAmount(int itemid)
	{
		if (itemid == 0)
		{
			return 0;
		}
		int num = 0;
		if (containerMain != null)
		{
			num += containerMain.GetAmount(itemid, true);
		}
		if (containerBelt != null)
		{
			num += containerBelt.GetAmount(itemid, true);
		}
		if (containerWear != null)
		{
			num += containerWear.GetAmount(itemid, true);
		}
		return num;
	}

	public Item[] AllItems()
	{
		List<Item> list = new List<Item>();
		if (containerMain != null)
		{
			list.AddRange(containerMain.itemList);
		}
		if (containerBelt != null)
		{
			list.AddRange(containerBelt.itemList);
		}
		if (containerWear != null)
		{
			list.AddRange(containerWear.itemList);
		}
		return list.ToArray();
	}

	public int AllItemsNoAlloc(ref List<Item> items)
	{
		items.Clear();
		if (containerMain != null)
		{
			items.AddRange(containerMain.itemList);
		}
		if (containerBelt != null)
		{
			items.AddRange(containerBelt.itemList);
		}
		if (containerWear != null)
		{
			items.AddRange(containerWear.itemList);
		}
		return items.Count;
	}

	public void FindAmmo(List<Item> list, AmmoTypes ammoType)
	{
		if (containerMain != null)
		{
			containerMain.FindAmmo(list, ammoType);
		}
		if (containerBelt != null)
		{
			containerBelt.FindAmmo(list, ammoType);
		}
	}

	public bool HasAmmo(AmmoTypes ammoType)
	{
		if (!containerMain.HasAmmo(ammoType))
		{
			return containerBelt.HasAmmo(ammoType);
		}
		return true;
	}
}
