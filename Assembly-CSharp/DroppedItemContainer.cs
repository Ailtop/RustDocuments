#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class DroppedItemContainer : BaseCombatEntity
{
	public string lootPanelName = "generic";

	public int maxItemCount = 36;

	[NonSerialized]
	public ulong playerSteamID;

	[NonSerialized]
	public string _playerName;

	public bool onlyOwnerLoot;

	public ItemContainer inventory;

	public string playerName
	{
		get
		{
			return _playerName;
		}
		set
		{
			_playerName = value;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DroppedItemContainer.OnRpcMessage"))
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

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (baseEntity.InSafeZone() && baseEntity.userID != playerSteamID)
		{
			return false;
		}
		if (onlyOwnerLoot && baseEntity.userID != playerSteamID)
		{
			return false;
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void ServerInit()
	{
		ResetRemovalTime();
		base.ServerInit();
	}

	public void RemoveMe()
	{
		if (IsOpen())
		{
			ResetRemovalTime();
		}
		else
		{
			Kill();
		}
	}

	public void ResetRemovalTime(float dur)
	{
		using (TimeWarning.New("ResetRemovalTime"))
		{
			Invoke(RemoveMe, dur);
		}
	}

	public void ResetRemovalTime()
	{
		ResetRemovalTime(CalculateRemovalTime());
	}

	public float CalculateRemovalTime()
	{
		int num = 1;
		if (inventory != null)
		{
			foreach (Item item in inventory.itemList)
			{
				num = Mathf.Max(num, item.despawnMultiplier);
			}
		}
		return (float)num * ConVar.Server.itemdespawn;
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

	public void TakeFrom(params ItemContainer[] source)
	{
		Assert.IsTrue(inventory == null, "Initializing Twice");
		using (TimeWarning.New("DroppedItemContainer.TakeFrom"))
		{
			int num = 0;
			ItemContainer[] array = source;
			foreach (ItemContainer itemContainer in array)
			{
				num += itemContainer.itemList.Count;
			}
			inventory = new ItemContainer();
			inventory.ServerInitialize(null, Mathf.Min(num, maxItemCount));
			inventory.GiveUID();
			inventory.entityOwner = this;
			inventory.SetFlag(ItemContainer.Flag.NoItemInput, true);
			array = source;
			for (int i = 0; i < array.Length; i++)
			{
				Item[] array2 = array[i].itemList.ToArray();
				foreach (Item item in array2)
				{
					if (!item.MoveToContainer(inventory))
					{
						item.DropAndTossUpwards(base.transform.position);
					}
				}
			}
			ResetRemovalTime();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_OpenLoot(RPCMessage rpc)
	{
		if (inventory != null)
		{
			BasePlayer player = rpc.player;
			if ((bool)player && player.CanInteract() && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(this))
			{
				SetFlag(Flags.Open, true);
				player.inventory.loot.AddContainer(inventory);
				player.inventory.loot.SendImmediate();
				player.ClientRPCPlayer(null, player, "RPC_OpenLootPanel", lootPanelName);
				SendNetworkUpdate();
			}
		}
	}

	public void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		if (inventory == null || inventory.itemList == null || inventory.itemList.Count == 0)
		{
			Kill();
			return;
		}
		ResetRemovalTime();
		SetFlag(Flags.Open, false);
		SendNetworkUpdate();
	}

	public override void PreServerLoad()
	{
		base.PreServerLoad();
		inventory = new ItemContainer();
		inventory.entityOwner = this;
		inventory.ServerInitialize(null, 0);
		inventory.SetFlag(ItemContainer.Flag.NoItemInput, true);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lootableCorpse = Facepunch.Pool.Get<ProtoBuf.LootableCorpse>();
		info.msg.lootableCorpse.playerName = playerName;
		info.msg.lootableCorpse.playerID = playerSteamID;
		if (info.forDisk)
		{
			if (inventory != null)
			{
				info.msg.storageBox = Facepunch.Pool.Get<StorageBox>();
				info.msg.storageBox.contents = inventory.Save();
			}
			else
			{
				Debug.LogWarning("Dropped item container without inventory: " + ToString());
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lootableCorpse != null)
		{
			playerName = info.msg.lootableCorpse.playerName;
			playerSteamID = info.msg.lootableCorpse.playerID;
		}
		if (info.msg.storageBox != null)
		{
			if (inventory != null)
			{
				inventory.Load(info.msg.storageBox.contents);
			}
			else
			{
				Debug.LogWarning("Dropped item container without inventory: " + ToString());
			}
		}
	}
}
