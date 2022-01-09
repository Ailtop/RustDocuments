#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Extend;
using Network;
using ProtoBuf;
using Rust;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;

public class MarketTerminal : StorageContainer
{
	private Action<BasePlayer, Item> _onCurrencyRemovedCached;

	private Action<BasePlayer, Item> _onItemPurchasedCached;

	private Action _checkForExpiredOrdersCached;

	private bool _transactionActive;

	private static readonly List<uint> _deliveryEligible = new List<uint>(128);

	private static RealTimeSince _deliveryEligibleLastCalculated;

	public const Flags Flag_HasItems = Flags.Reserved1;

	public const Flags Flag_InventoryFull = Flags.Reserved2;

	[Header("Market Terminal")]
	public GameObjectRef menuPrefab;

	public ulong lockToCustomerDuration = 300uL;

	public ulong orderTimeout = 180uL;

	public ItemDefinition deliveryFeeCurrency;

	public int deliveryFeeAmount;

	public DeliveryDroneConfig config;

	public RustText userLabel;

	private ulong _customerSteamId;

	private string _customerName;

	private TimeUntil _timeUntilCustomerExpiry;

	private EntityRef<Marketplace> _marketplace;

	public List<ProtoBuf.MarketTerminal.PendingOrder> pendingOrders;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MarketTerminal.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 3793918752u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_Purchase "));
				}
				using (TimeWarning.New("Server_Purchase"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3793918752u, "Server_Purchase", this, player, 10uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3793918752u, "Server_Purchase", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							Server_Purchase(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_Purchase");
					}
				}
				return true;
			}
			if (rpc == 1382511247 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_TryOpenMarket "));
				}
				using (TimeWarning.New("Server_TryOpenMarket"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1382511247u, "Server_TryOpenMarket", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1382511247u, "Server_TryOpenMarket", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							Server_TryOpenMarket(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Server_TryOpenMarket");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void Setup(Marketplace marketplace)
	{
		_marketplace = new EntityRef<Marketplace>(marketplace.net.ID);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		_onCurrencyRemovedCached = OnCurrencyRemoved;
		_onItemPurchasedCached = OnItemPurchased;
		_checkForExpiredOrdersCached = CheckForExpiredOrders;
	}

	private void RegisterOrder(BasePlayer player, VendingMachine vendingMachine)
	{
		if (pendingOrders == null)
		{
			pendingOrders = Facepunch.Pool.GetList<ProtoBuf.MarketTerminal.PendingOrder>();
		}
		if (HasPendingOrderFor(vendingMachine.net.ID))
		{
			return;
		}
		Marketplace entity;
		if (!_marketplace.TryGet(true, out entity))
		{
			Debug.LogError("Marketplace is not set", this);
			return;
		}
		uint num = entity.SendDrone(player, this, vendingMachine);
		if (num == 0)
		{
			Debug.LogError("Failed to spawn delivery drone");
			return;
		}
		ProtoBuf.MarketTerminal.PendingOrder pendingOrder = Facepunch.Pool.Get<ProtoBuf.MarketTerminal.PendingOrder>();
		pendingOrder.vendingMachineId = vendingMachine.net.ID;
		pendingOrder.timeUntilExpiry = orderTimeout;
		pendingOrder.droneId = num;
		pendingOrders.Add(pendingOrder);
		CheckForExpiredOrders();
		UpdateHasItems(false);
		SendNetworkUpdateImmediate();
	}

	public void CompleteOrder(uint vendingMachineId)
	{
		if (pendingOrders != null)
		{
			int num = pendingOrders.FindIndexWith((ProtoBuf.MarketTerminal.PendingOrder o) => o.vendingMachineId, vendingMachineId);
			if (num < 0)
			{
				Debug.LogError("Completed market order that doesn't exist?");
				return;
			}
			pendingOrders[num].Dispose();
			pendingOrders.RemoveAt(num);
			CheckForExpiredOrders();
			UpdateHasItems(false);
			SendNetworkUpdateImmediate();
		}
	}

	private void CheckForExpiredOrders()
	{
		if (pendingOrders != null && pendingOrders.Count > 0)
		{
			bool flag = false;
			float? num = null;
			for (int i = 0; i < pendingOrders.Count; i++)
			{
				ProtoBuf.MarketTerminal.PendingOrder pendingOrder = pendingOrders[i];
				if ((float)pendingOrder.timeUntilExpiry <= 0f)
				{
					DeliveryDrone entity;
					if (new EntityRef<DeliveryDrone>(pendingOrder.droneId).TryGet(true, out entity))
					{
						Debug.LogError("Delivery timed out waiting for drone (too slow speed?)", this);
						entity.Kill();
					}
					else
					{
						Debug.LogError("Delivery timed out waiting for drone, and the drone seems to have been destroyed?", this);
					}
					pendingOrders.RemoveAt(i);
					i--;
					flag = true;
				}
				else if (!num.HasValue || (float)pendingOrder.timeUntilExpiry < num.Value)
				{
					num = pendingOrder.timeUntilExpiry;
				}
			}
			if (flag)
			{
				UpdateHasItems(false);
				SendNetworkUpdate();
			}
			if (num.HasValue)
			{
				Invoke(_checkForExpiredOrdersCached, num.Value);
			}
		}
		else
		{
			CancelInvoke(_checkForExpiredOrdersCached);
		}
	}

	private void RestrictToPlayer(BasePlayer player)
	{
		if (_customerSteamId == player.userID)
		{
			_timeUntilCustomerExpiry = lockToCustomerDuration;
			SendNetworkUpdate();
			return;
		}
		if (_customerSteamId != 0L)
		{
			Debug.LogError("Overwriting player restriction! It should be cleared first.", this);
		}
		_customerSteamId = player.userID;
		_customerName = player.displayName;
		_timeUntilCustomerExpiry = lockToCustomerDuration;
		SendNetworkUpdateImmediate();
		ClientRPC(null, "Client_CloseMarketUI", _customerSteamId);
		RemoveAnyLooters();
		if (IsOpen())
		{
			Debug.LogError("Market terminal's inventory is still open after removing looters!", this);
		}
	}

	private void ClearRestriction()
	{
		if (_customerSteamId != 0L)
		{
			_customerSteamId = 0uL;
			_customerName = null;
			_timeUntilCustomerExpiry = 0f;
			SendNetworkUpdateImmediate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void Server_TryOpenMarket(RPCMessage msg)
	{
		if (!CanPlayerInteract(msg.player))
		{
			return;
		}
		if (!_marketplace.IsValid(true))
		{
			Debug.LogError("Marketplace is not set", this);
			return;
		}
		using (EntityIdList entityIdList = Facepunch.Pool.Get<EntityIdList>())
		{
			entityIdList.entityIds = Facepunch.Pool.GetList<uint>();
			GetDeliveryEligibleVendingMachines(entityIdList.entityIds);
			ClientRPCPlayer(null, msg.player, "Client_OpenMarket", entityIdList);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(10uL)]
	public void Server_Purchase(RPCMessage msg)
	{
		if (!CanPlayerInteract(msg.player))
		{
			return;
		}
		if (!_marketplace.IsValid(true))
		{
			Debug.LogError("Marketplace is not set", this);
			return;
		}
		uint num = msg.read.UInt32();
		int num2 = msg.read.Int32();
		int num3 = msg.read.Int32();
		VendingMachine vendingMachine = BaseNetworkable.serverEntities.Find(num) as VendingMachine;
		if (vendingMachine == null || !BaseEntityEx.IsValid(vendingMachine) || num2 < 0 || num2 >= vendingMachine.sellOrders.sellOrders.Count || num3 <= 0 || base.inventory.IsFull())
		{
			return;
		}
		GetDeliveryEligibleVendingMachines(null);
		if (_deliveryEligible == null || !_deliveryEligible.Contains(num))
		{
			return;
		}
		try
		{
			_transactionActive = true;
			int num4 = deliveryFeeAmount;
			ProtoBuf.VendingMachine.SellOrder sellOrder = vendingMachine.sellOrders.sellOrders[num2];
			if (!CanPlayerAffordOrderAndDeliveryFee(msg.player, sellOrder, num3))
			{
				return;
			}
			int num5 = msg.player.inventory.Take(null, deliveryFeeCurrency.itemid, num4);
			if (num5 != num4)
			{
				Debug.LogError($"Took an incorrect number of items for the delivery fee (took {num5}, should have taken {num4})");
			}
			ClientRPCPlayer(null, msg.player, "Client_ShowItemNotice", deliveryFeeCurrency.itemid, -num4, false);
			if (!vendingMachine.DoTransaction(msg.player, num2, num3, base.inventory, _onCurrencyRemovedCached, _onItemPurchasedCached))
			{
				Item item = ItemManager.CreateByItemID(deliveryFeeCurrency.itemid, num4, 0uL);
				if (!msg.player.inventory.GiveItem(item))
				{
					item.Drop(msg.player.inventory.containerMain.dropPosition, msg.player.inventory.containerMain.dropVelocity);
				}
			}
			else
			{
				RestrictToPlayer(msg.player);
				RegisterOrder(msg.player, vendingMachine);
			}
		}
		finally
		{
			_transactionActive = false;
		}
	}

	private void UpdateHasItems(bool sendNetworkUpdate = true)
	{
		if (!Rust.Application.isLoadingSave)
		{
			bool flag = base.inventory.itemList.Count > 0;
			bool flag2 = pendingOrders != null && pendingOrders.Count != 0;
			SetFlag(Flags.Reserved1, flag && !flag2, false, sendNetworkUpdate);
			SetFlag(Flags.Reserved2, base.inventory.IsFull(), false, sendNetworkUpdate);
			if (!flag && !flag2)
			{
				ClearRestriction();
			}
		}
	}

	private void OnCurrencyRemoved(BasePlayer player, Item currencyItem)
	{
		if (player != null && currencyItem != null)
		{
			ClientRPCPlayer(null, player, "Client_ShowItemNotice", currencyItem.info.itemid, -currencyItem.amount, false);
		}
	}

	private void OnItemPurchased(BasePlayer player, Item purchasedItem)
	{
		if (player != null && purchasedItem != null)
		{
			ClientRPCPlayer(null, player, "Client_ShowItemNotice", purchasedItem.info.itemid, purchasedItem.amount, true);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.marketTerminal = Facepunch.Pool.Get<ProtoBuf.MarketTerminal>();
		info.msg.marketTerminal.customerSteamId = _customerSteamId;
		info.msg.marketTerminal.customerName = _customerName;
		info.msg.marketTerminal.timeUntilExpiry = _timeUntilCustomerExpiry;
		info.msg.marketTerminal.marketplaceId = _marketplace.uid;
		info.msg.marketTerminal.orders = Facepunch.Pool.GetList<ProtoBuf.MarketTerminal.PendingOrder>();
		if (pendingOrders == null)
		{
			return;
		}
		foreach (ProtoBuf.MarketTerminal.PendingOrder pendingOrder in pendingOrders)
		{
			ProtoBuf.MarketTerminal.PendingOrder item = pendingOrder.Copy();
			info.msg.marketTerminal.orders.Add(item);
		}
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (_transactionActive)
		{
			return true;
		}
		if (item.parent == null)
		{
			return true;
		}
		if (item.parent == base.inventory)
		{
			return true;
		}
		return false;
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		UpdateHasItems();
	}

	public override bool CanOpenLootPanel(BasePlayer player, string panelName)
	{
		if (CanPlayerInteract(player) && HasFlag(Flags.Reserved1))
		{
			return base.CanOpenLootPanel(player, panelName);
		}
		return false;
	}

	private void RemoveAnyLooters()
	{
		ItemContainer item = base.inventory;
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			if (!(activePlayer == null) && !(activePlayer.inventory == null) && !(activePlayer.inventory.loot == null) && activePlayer.inventory.loot.containers.Contains(item))
			{
				activePlayer.inventory.loot.Clear();
			}
		}
	}

	public void GetDeliveryEligibleVendingMachines(List<uint> vendingMachineIds)
	{
		if ((float)_deliveryEligibleLastCalculated < 5f)
		{
			if (vendingMachineIds == null)
			{
				return;
			}
			foreach (uint item in _deliveryEligible)
			{
				vendingMachineIds.Add(item);
			}
			return;
		}
		_deliveryEligibleLastCalculated = 0f;
		_deliveryEligible.Clear();
		foreach (MapMarker serverMapMarker in MapMarker.serverMapMarkers)
		{
			VendingMachineMapMarker vendingMachineMapMarker;
			if ((object)(vendingMachineMapMarker = serverMapMarker as VendingMachineMapMarker) != null && !(vendingMachineMapMarker.server_vendingMachine == null))
			{
				VendingMachine server_vendingMachine = vendingMachineMapMarker.server_vendingMachine;
				if (!(server_vendingMachine == null) && (_003CGetDeliveryEligibleVendingMachines_003Eg__IsEligible_007C24_0(server_vendingMachine, config.vendingMachineOffset, 1) || _003CGetDeliveryEligibleVendingMachines_003Eg__IsEligible_007C24_0(server_vendingMachine, config.vendingMachineOffset + Vector3.forward * config.maxDistanceFromVendingMachine, 2)))
				{
					_deliveryEligible.Add(server_vendingMachine.net.ID);
				}
			}
		}
		if (vendingMachineIds == null)
		{
			return;
		}
		foreach (uint item2 in _deliveryEligible)
		{
			vendingMachineIds.Add(item2);
		}
	}

	public bool CanPlayerAffordOrderAndDeliveryFee(BasePlayer player, ProtoBuf.VendingMachine.SellOrder sellOrder, int numberOfTransactions)
	{
		int num = player.inventory.FindItemIDs(deliveryFeeCurrency.itemid).Sum((Item i) => i.amount);
		int num2 = deliveryFeeAmount;
		if (num < num2)
		{
			return false;
		}
		if (sellOrder != null)
		{
			int num3 = sellOrder.currencyAmountPerItem * numberOfTransactions;
			if (sellOrder.currencyID == deliveryFeeCurrency.itemid && !sellOrder.currencyIsBP && num < num2 + num3)
			{
				return false;
			}
		}
		return true;
	}

	public bool HasPendingOrderFor(uint vendingMachineId)
	{
		return pendingOrders?.FindWith((ProtoBuf.MarketTerminal.PendingOrder o) => o.vendingMachineId, vendingMachineId) != null;
	}

	public bool CanPlayerInteract(BasePlayer player)
	{
		if (player == null)
		{
			return false;
		}
		if (_customerSteamId == 0L || (float)_timeUntilCustomerExpiry <= 0f)
		{
			return true;
		}
		return player.userID == _customerSteamId;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.marketTerminal == null)
		{
			return;
		}
		_customerSteamId = info.msg.marketTerminal.customerSteamId;
		_customerName = info.msg.marketTerminal.customerName;
		_timeUntilCustomerExpiry = info.msg.marketTerminal.timeUntilExpiry;
		_marketplace = new EntityRef<Marketplace>(info.msg.marketTerminal.marketplaceId);
		if (pendingOrders == null)
		{
			pendingOrders = Facepunch.Pool.GetList<ProtoBuf.MarketTerminal.PendingOrder>();
		}
		if (pendingOrders.Count > 0)
		{
			foreach (ProtoBuf.MarketTerminal.PendingOrder pendingOrder in pendingOrders)
			{
				ProtoBuf.MarketTerminal.PendingOrder obj = pendingOrder;
				Facepunch.Pool.Free(ref obj);
			}
			pendingOrders.Clear();
		}
		foreach (ProtoBuf.MarketTerminal.PendingOrder order in info.msg.marketTerminal.orders)
		{
			ProtoBuf.MarketTerminal.PendingOrder item = order.Copy();
			pendingOrders.Add(item);
		}
	}
}
