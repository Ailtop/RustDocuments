#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ContainerIOEntity : IOEntity, IItemContainerEntity, IIdealSlotEntity, ILootableEntity, LootPanel.IHasLootPanel, IContainerSounds
{
	public ItemDefinition onlyAllowedItem;

	public ItemContainer.ContentsType allowedContents = ItemContainer.ContentsType.Generic;

	public int maxStackSize = 1;

	public int numSlots;

	public string lootPanelName = "generic";

	public Translate.Phrase panelTitle = new Translate.Phrase("loot", "Loot");

	public bool needsBuildingPrivilegeToUse;

	public bool isLootable = true;

	public bool dropsLoot;

	public bool dropFloats;

	public bool onlyOneUser;

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	public Translate.Phrase LootPanelTitle => panelTitle;

	public ItemContainer inventory { get; private set; }

	public Transform Transform => base.transform;

	public bool DropsLoot => dropsLoot;

	public bool DropFloats => dropFloats;

	public float DestroyLootPercent => 0f;

	public ulong LastLootedBy { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ContainerIOEntity.OnRpcMessage"))
		{
			if (rpc == 331989034 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_OpenLoot "));
				}
				using (TimeWarning.New("RPC_OpenLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(331989034u, "RPC_OpenLoot", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_OpenLoot(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_OpenLoot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (!pickup.requireEmptyInv || inventory == null || inventory.itemList.Count == 0)
		{
			return base.CanPickup(player);
		}
		return false;
	}

	public override void ServerInit()
	{
		if (inventory == null)
		{
			CreateInventory(giveUID: true);
			OnInventoryFirstCreated(inventory);
		}
		base.ServerInit();
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(giveUID: false);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (inventory != null && !inventory.uid.IsValid)
		{
			inventory.GiveUID();
		}
		SetFlag(Flags.Open, b: false);
	}

	public void CreateInventory(bool giveUID)
	{
		inventory = new ItemContainer();
		inventory.entityOwner = this;
		inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		inventory.SetOnlyAllowedItem(onlyAllowedItem);
		inventory.maxStackSize = maxStackSize;
		inventory.ServerInitialize(null, numSlots);
		if (giveUID)
		{
			inventory.GiveUID();
		}
		inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		inventory.onDirty += OnInventoryDirty;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public virtual void OnInventoryFirstCreated(ItemContainer container)
	{
	}

	public virtual void OnItemAddedOrRemoved(Item item, bool added)
	{
	}

	protected virtual void OnInventoryDirty()
	{
	}

	public override void OnKilled(HitInfo info)
	{
		DropItems();
		base.OnKilled(info);
	}

	public void DropItems(BaseEntity initiator = null)
	{
		StorageContainer.DropItems(this, initiator);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract())
			{
				PlayerOpenLoot(player, lootPanelName);
			}
		}
	}

	public virtual bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (Interface.CallHook("CanLootEntity", player, this) != null)
		{
			return false;
		}
		if (needsBuildingPrivilegeToUse && !player.CanBuild())
		{
			return false;
		}
		if (onlyOneUser && IsOpen())
		{
			player.ChatMessage("Already in use");
			return false;
		}
		if (panelToOpen == "")
		{
			panelToOpen = lootPanelName;
		}
		if (player.inventory.loot.StartLootingEntity(this, doPositionChecks))
		{
			SetFlag(Flags.Open, b: true);
			player.inventory.loot.AddContainer(inventory);
			player.inventory.loot.SendImmediate();
			player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", lootPanelName);
			SendNetworkUpdate();
			return true;
		}
		return false;
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		SetFlag(Flags.Open, b: false);
		SendNetworkUpdate();
	}

	public bool ShouldDropItemsIndividually()
	{
		return false;
	}

	public virtual int GetIdealSlot(BasePlayer player, Item item)
	{
		return -1;
	}

	public virtual ItemContainerId GetIdealContainer(BasePlayer player, Item item, bool altMove)
	{
		return default(ItemContainerId);
	}

	public virtual void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
	}

	public bool OccupiedCheck(BasePlayer player = null)
	{
		if (player != null && player.inventory.loot.entitySource == this)
		{
			return true;
		}
		if (onlyOneUser)
		{
			return !IsOpen();
		}
		return true;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk && info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
				inventory.capacity = numSlots;
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}
}
