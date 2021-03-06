#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class StorageContainer : DecayEntity, IItemContainerEntity, PlayerInventory.ICanMoveFrom
{
	public static readonly Translate.Phrase LockedMessage = new Translate.Phrase("storage.locked", "Can't loot right now");

	public static readonly Translate.Phrase InUseMessage = new Translate.Phrase("storage.in_use", "Already in use");

	public int inventorySlots = 6;

	public float dropChance = 0.75f;

	public bool isLootable = true;

	public bool isLockable = true;

	public bool isMonitorable;

	public string panelName = "generic";

	public ItemContainer.ContentsType allowedContents;

	public ItemDefinition allowedItem;

	public int maxStackSize;

	public bool needsBuildingPrivilegeToUse;

	public SoundDefinition openSound;

	public SoundDefinition closeSound;

	[Header("Item Dropping")]
	public Vector3 dropPosition;

	public Vector3 dropVelocity = Vector3.forward;

	public ItemCategory onlyAcceptCategory = ItemCategory.All;

	public bool onlyOneUser;

	public ItemContainer inventory { get; set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StorageContainer.OnRpcMessage"))
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

	public override void ResetState()
	{
		base.ResetState();
		if (base.isServer && inventory != null)
		{
			inventory.Clear();
			inventory = null;
		}
	}

	public virtual void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(dropPosition, Vector3.one * 0.1f);
		Gizmos.color = Color.white;
		Gizmos.DrawRay(dropPosition, dropVelocity);
	}

	public void MoveAllInventoryItems(ItemContainer source, ItemContainer dest)
	{
		for (int i = 0; i < Mathf.Min(source.capacity, dest.capacity); i++)
		{
			source.GetSlot(i)?.MoveToContainer(dest);
		}
	}

	public virtual void ReceiveInventoryFromItem(Item item)
	{
		if (item.contents != null)
		{
			MoveAllInventoryItems(item.contents, inventory);
		}
	}

	public override bool CanPickup(BasePlayer player)
	{
		bool flag = GetSlot(Slot.Lock) != null;
		if (base.isClient)
		{
			if (base.CanPickup(player))
			{
				return !flag;
			}
			return false;
		}
		if ((!pickup.requireEmptyInv || inventory == null || inventory.itemList.Count == 0) && base.CanPickup(player))
		{
			return !flag;
		}
		return false;
	}

	public override void OnPickedUp(Item createdItem, BasePlayer player)
	{
		base.OnPickedUp(createdItem, player);
		if (createdItem != null && createdItem.contents != null)
		{
			MoveAllInventoryItems(inventory, createdItem.contents);
			return;
		}
		for (int i = 0; i < inventory.capacity; i++)
		{
			Item slot = inventory.GetSlot(i);
			if (slot != null)
			{
				slot.RemoveFromContainer();
				player.GiveItem(slot, GiveItemReason.PickedUp);
			}
		}
	}

	public override void ServerInit()
	{
		if (inventory == null)
		{
			CreateInventory(true);
			OnInventoryFirstCreated(inventory);
		}
		base.ServerInit();
	}

	public virtual void OnInventoryFirstCreated(ItemContainer container)
	{
	}

	public virtual void OnItemAddedOrRemoved(Item item, bool added)
	{
	}

	public virtual bool ItemFilter(Item item, int targetSlot)
	{
		if (onlyAcceptCategory == ItemCategory.All)
		{
			return true;
		}
		return item.info.category == onlyAcceptCategory;
	}

	public void CreateInventory(bool giveUID)
	{
		inventory = new ItemContainer();
		inventory.entityOwner = this;
		inventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
		inventory.onlyAllowedItem = allowedItem;
		inventory.maxStackSize = maxStackSize;
		inventory.ServerInitialize(null, inventorySlots);
		if (giveUID)
		{
			inventory.GiveUID();
		}
		inventory.onDirty += OnInventoryDirty;
		inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		inventory.canAcceptItem = ItemFilter;
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		CreateInventory(false);
	}

	protected virtual void OnInventoryDirty()
	{
		InvalidateNetworkCache();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (inventory != null && inventory.uid == 0)
		{
			inventory.GiveUID();
		}
		SetFlag(Flags.Open, false);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (inventory != null)
		{
			inventory.Kill();
			inventory = null;
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (isLootable)
		{
			BasePlayer player = rpc.player;
			PlayerOpenLoot(player);
		}
	}

	public virtual string GetPanelName()
	{
		return panelName;
	}

	public virtual bool CanMoveFrom(BasePlayer player, Item item)
	{
		return !inventory.IsLocked();
	}

	public virtual bool CanOpenLootPanel(BasePlayer player, string panelName = "")
	{
		if (needsBuildingPrivilegeToUse && !player.CanBuild())
		{
			return false;
		}
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock != null && !baseLock.OnTryToOpen(player))
		{
			player.ChatMessage("It is locked...");
			return false;
		}
		return true;
	}

	public virtual void AddContainers(PlayerLoot loot)
	{
		loot.AddContainer(inventory);
	}

	public virtual bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (Interface.CallHook("CanLootEntity", player, this) != null)
		{
			return false;
		}
		if (IsLocked())
		{
			player.ShowToast(1, LockedMessage);
			return false;
		}
		if (onlyOneUser && IsOpen())
		{
			player.ShowToast(1, InUseMessage);
			return false;
		}
		if (panelToOpen == "")
		{
			panelToOpen = panelName;
		}
		if (!CanOpenLootPanel(player, panelToOpen))
		{
			return false;
		}
		if (player.inventory.loot.StartLootingEntity(this, doPositionChecks))
		{
			SetFlag(Flags.Open, true);
			AddContainers(player.inventory.loot);
			player.inventory.loot.SendImmediate();
			player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", panelToOpen);
			SendNetworkUpdate();
			return true;
		}
		return false;
	}

	public virtual void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		SetFlag(Flags.Open, false);
		SendNetworkUpdate();
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

	public override void OnKilled(HitInfo info)
	{
		DropItems(info?.Initiator);
		base.OnKilled(info);
	}

	public void DropItems(BaseEntity initiator = null)
	{
		if (inventory == null || inventory.itemList == null || inventory.itemList.Count == 0 || dropChance == 0f)
		{
			return;
		}
		if (ShouldDropItemsIndividually() || inventory.itemList.Count == 1)
		{
			if (initiator != null)
			{
				DropBonusItems(initiator, inventory);
			}
			DropUtil.DropItems(inventory, GetDropPosition());
		}
		else
		{
			bool flag = inventory.Drop("assets/prefabs/misc/item drop/item_drop.prefab", GetDropPosition(), base.transform.rotation) != null;
		}
	}

	public virtual void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
	}

	public override Vector3 GetDropPosition()
	{
		return base.transform.localToWorldMatrix.MultiplyPoint(dropPosition);
	}

	public override Vector3 GetDropVelocity()
	{
		return GetInheritedDropVelocity() + base.transform.localToWorldMatrix.MultiplyVector(dropPosition);
	}

	public virtual bool ShouldDropItemsIndividually()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
				inventory.capacity = inventorySlots;
			}
			else
			{
				Debug.LogWarning("Storage container without inventory: " + ToString());
			}
		}
	}

	public override bool HasSlot(Slot slot)
	{
		if (isLockable && slot == Slot.Lock)
		{
			return true;
		}
		if (isMonitorable && slot == Slot.StorageMonitor)
		{
			return true;
		}
		return base.HasSlot(slot);
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
}
