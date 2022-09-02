#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ShopFront : StorageContainer
{
	public static class ShopFrontFlags
	{
		public const Flags VendorAccepted = Flags.Reserved1;

		public const Flags CustomerAccepted = Flags.Reserved2;

		public const Flags Exchanging = Flags.Reserved3;
	}

	public float maxUseAngle = 27f;

	public BasePlayer vendorPlayer;

	public BasePlayer customerPlayer;

	public GameObjectRef transactionCompleteEffect;

	public ItemContainer customerInventory;

	private float AngleDotProduct => 1f - maxUseAngle / 90f;

	public ItemContainer vendorInventory => base.inventory;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ShopFront.OnRpcMessage"))
		{
			if (rpc == 1159607245 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AcceptClicked "));
				}
				using (TimeWarning.New("AcceptClicked"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1159607245u, "AcceptClicked", this, player, 3f))
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
							AcceptClicked(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AcceptClicked");
					}
				}
				return true;
			}
			if (rpc == 3168107540u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - CancelClicked "));
				}
				using (TimeWarning.New("CancelClicked"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3168107540u, "CancelClicked", this, player, 3f))
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
							RPCMessage msg3 = rPCMessage;
							CancelClicked(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in CancelClicked");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool TradeLocked()
	{
		return false;
	}

	public bool IsTradingPlayer(BasePlayer player)
	{
		if (player != null)
		{
			if (!IsPlayerCustomer(player))
			{
				return IsPlayerVendor(player);
			}
			return true;
		}
		return false;
	}

	public bool IsPlayerCustomer(BasePlayer player)
	{
		return player == customerPlayer;
	}

	public bool IsPlayerVendor(BasePlayer player)
	{
		return player == vendorPlayer;
	}

	public bool PlayerInVendorPos(BasePlayer player)
	{
		return Vector3.Dot(base.transform.right, (player.transform.position - base.transform.position).normalized) <= 0f - AngleDotProduct;
	}

	public bool PlayerInCustomerPos(BasePlayer player)
	{
		return Vector3.Dot(base.transform.right, (player.transform.position - base.transform.position).normalized) >= AngleDotProduct;
	}

	public bool LootEligable(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (PlayerInVendorPos(player) && vendorPlayer == null)
		{
			return true;
		}
		if (PlayerInCustomerPos(player) && customerPlayer == null)
		{
			return true;
		}
		return false;
	}

	public void ResetTrade()
	{
		SetFlag(Flags.Reserved1, b: false);
		SetFlag(Flags.Reserved2, b: false);
		SetFlag(Flags.Reserved3, b: false);
		vendorInventory.SetLocked(isLocked: false);
		customerInventory.SetLocked(isLocked: false);
		CancelInvoke(CompleteTrade);
	}

	public void CompleteTrade()
	{
		if (vendorPlayer != null && customerPlayer != null && HasFlag(Flags.Reserved1) && HasFlag(Flags.Reserved2))
		{
			if (Interface.CallHook("OnShopCompleteTrade", this) != null)
			{
				return;
			}
			for (int num = vendorInventory.capacity - 1; num >= 0; num--)
			{
				Item slot = vendorInventory.GetSlot(num);
				Item slot2 = customerInventory.GetSlot(num);
				if ((bool)customerPlayer && slot != null)
				{
					customerPlayer.GiveItem(slot);
				}
				if ((bool)vendorPlayer && slot2 != null)
				{
					vendorPlayer.GiveItem(slot2);
				}
			}
			Effect.server.Run(transactionCompleteEffect.resourcePath, this, 0u, new Vector3(0f, 1f, 0f), Vector3.zero);
		}
		ResetTrade();
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AcceptClicked(RPCMessage msg)
	{
		if (IsTradingPlayer(msg.player) && !(vendorPlayer == null) && !(customerPlayer == null) && Interface.CallHook("OnShopAcceptClick", this, msg.player) == null)
		{
			if (IsPlayerVendor(msg.player))
			{
				SetFlag(Flags.Reserved1, b: true);
				vendorInventory.SetLocked(isLocked: true);
			}
			else if (IsPlayerCustomer(msg.player))
			{
				SetFlag(Flags.Reserved2, b: true);
				customerInventory.SetLocked(isLocked: true);
			}
			if (HasFlag(Flags.Reserved1) && HasFlag(Flags.Reserved2))
			{
				SetFlag(Flags.Reserved3, b: true);
				Invoke(CompleteTrade, 2f);
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void CancelClicked(RPCMessage msg)
	{
		if (IsTradingPlayer(msg.player) && Interface.CallHook("OnShopCancelClick", this, msg.player) == null)
		{
			_ = (bool)vendorPlayer;
			_ = (bool)customerPlayer;
			ResetTrade();
		}
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = vendorInventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptVendorItem));
		if (customerInventory == null)
		{
			customerInventory = new ItemContainer();
			customerInventory.allowedContents = ((allowedContents == (ItemContainer.ContentsType)0) ? ItemContainer.ContentsType.Generic : allowedContents);
			customerInventory.SetOnlyAllowedItem(allowedItem);
			customerInventory.entityOwner = this;
			customerInventory.maxStackSize = maxStackSize;
			customerInventory.ServerInitialize(null, inventorySlots);
			customerInventory.GiveUID();
			customerInventory.onDirty += OnInventoryDirty;
			customerInventory.onItemAddedRemoved = OnItemAddedOrRemoved;
			ItemContainer itemContainer2 = customerInventory;
			itemContainer2.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer2.canAcceptItem, new Func<Item, int, bool>(CanAcceptCustomerItem));
			OnInventoryFirstCreated(customerInventory);
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		ResetTrade();
	}

	private bool CanAcceptVendorItem(Item item, int targetSlot)
	{
		if ((vendorPlayer != null && item.GetOwnerPlayer() == vendorPlayer) || vendorInventory.itemList.Contains(item) || item.parent == null)
		{
			return true;
		}
		return false;
	}

	private bool CanAcceptCustomerItem(Item item, int targetSlot)
	{
		if ((customerPlayer != null && item.GetOwnerPlayer() == customerPlayer) || customerInventory.itemList.Contains(item) || item.parent == null)
		{
			return true;
		}
		return false;
	}

	public override bool CanMoveFrom(BasePlayer player, Item item)
	{
		if (TradeLocked())
		{
			return false;
		}
		if (IsTradingPlayer(player))
		{
			if (IsPlayerCustomer(player) && customerInventory.itemList.Contains(item) && !customerInventory.IsLocked())
			{
				return true;
			}
			if (IsPlayerVendor(player) && vendorInventory.itemList.Contains(item) && !vendorInventory.IsLocked())
			{
				return true;
			}
		}
		return false;
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (base.CanOpenLootPanel(player, panelName))
		{
			return LootEligable(player);
		}
		return false;
	}

	public void ReturnPlayerItems(BasePlayer player)
	{
		if (!IsTradingPlayer(player))
		{
			return;
		}
		ItemContainer itemContainer = null;
		if (IsPlayerVendor(player))
		{
			itemContainer = vendorInventory;
		}
		else if (IsPlayerCustomer(player))
		{
			itemContainer = customerInventory;
		}
		if (itemContainer != null)
		{
			for (int num = itemContainer.itemList.Count - 1; num >= 0; num--)
			{
				Item item = itemContainer.itemList[num];
				player.GiveItem(item);
			}
		}
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		if (IsTradingPlayer(player))
		{
			ReturnPlayerItems(player);
			if (player == vendorPlayer)
			{
				vendorPlayer = null;
			}
			if (player == customerPlayer)
			{
				customerPlayer = null;
			}
			UpdatePlayers();
			ResetTrade();
			base.PlayerStoppedLooting(player);
		}
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		bool flag = base.PlayerOpenLoot(player, panelToOpen);
		if (flag)
		{
			player.inventory.loot.AddContainer(customerInventory);
			player.inventory.loot.SendImmediate();
		}
		if (PlayerInVendorPos(player) && vendorPlayer == null)
		{
			vendorPlayer = player;
		}
		else
		{
			if (!PlayerInCustomerPos(player) || !(customerPlayer == null))
			{
				return false;
			}
			customerPlayer = player;
		}
		ResetTrade();
		UpdatePlayers();
		return flag;
	}

	public void UpdatePlayers()
	{
		ClientRPC(null, "CLIENT_ReceivePlayers", (!(vendorPlayer == null)) ? vendorPlayer.net.ID : 0u, (!(customerPlayer == null)) ? customerPlayer.net.ID : 0u);
	}

	public override int GetIdealSlot(BasePlayer player, ItemContainer container, Item item)
	{
		if (player == customerPlayer)
		{
			return 1;
		}
		return base.GetIdealSlot(player, container, item);
	}
}
