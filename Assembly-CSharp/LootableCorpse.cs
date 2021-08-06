#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class LootableCorpse : BaseCorpse
{
	public string lootPanelName = "generic";

	[NonSerialized]
	public ulong playerSteamID;

	[NonSerialized]
	public string _playerName;

	[NonSerialized]
	public ItemContainer[] containers;

	public virtual string playerName
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
		using (TimeWarning.New("LootableCorpse.OnRpcMessage"))
		{
			if (rpc == 2278459738u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_LootCorpse "));
				}
				using (TimeWarning.New("RPC_LootCorpse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2278459738u, "RPC_LootCorpse", this, player, 3f))
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
							RPC_LootCorpse(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_LootCorpse");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		DropItems();
		if (containers != null)
		{
			ItemContainer[] array = containers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Kill();
			}
		}
		containers = null;
	}

	public void TakeFrom(params ItemContainer[] source)
	{
		Assert.IsTrue(containers == null, "Initializing Twice");
		using (TimeWarning.New("Corpse.TakeFrom"))
		{
			containers = new ItemContainer[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				containers[i] = new ItemContainer();
				containers[i].ServerInitialize(null, source[i].capacity);
				containers[i].GiveUID();
				containers[i].entityOwner = this;
				Item[] array = source[i].itemList.ToArray();
				foreach (Item item in array)
				{
					if (!item.MoveToContainer(containers[i]))
					{
						item.DropAndTossUpwards(base.transform.position);
					}
				}
			}
			ResetRemovalTime();
		}
	}

	public override bool CanRemove()
	{
		return !IsOpen();
	}

	public virtual bool CanLoot()
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void RPC_LootCorpse(RPCMessage rpc)
	{
		BasePlayer player = rpc.player;
		if ((bool)player && player.CanInteract() && CanLoot() && containers != null && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(this))
		{
			SetFlag(Flags.Open, true);
			ItemContainer[] array = containers;
			foreach (ItemContainer container in array)
			{
				player.inventory.loot.AddContainer(container);
			}
			player.inventory.loot.SendImmediate();
			ClientRPCPlayer(null, player, "RPC_ClientLootCorpse");
			SendNetworkUpdate();
		}
	}

	public void PlayerStoppedLooting(BasePlayer player)
	{
		Interface.CallHook("OnLootEntityEnd", player, this);
		ResetRemovalTime();
		SetFlag(Flags.Open, false);
		SendNetworkUpdate();
	}

	public void DropItems()
	{
		if (containers != null)
		{
			DroppedItemContainer droppedItemContainer = ItemContainer.Drop("assets/prefabs/misc/item drop/item_drop_backpack.prefab", base.transform.position, Quaternion.identity, containers);
			if (droppedItemContainer != null)
			{
				droppedItemContainer.playerName = playerName;
				droppedItemContainer.playerSteamID = playerSteamID;
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lootableCorpse = Facepunch.Pool.Get<ProtoBuf.LootableCorpse>();
		info.msg.lootableCorpse.playerName = playerName;
		info.msg.lootableCorpse.playerID = playerSteamID;
		if (!info.forDisk || containers == null)
		{
			return;
		}
		info.msg.lootableCorpse.privateData = Facepunch.Pool.Get<ProtoBuf.LootableCorpse.Private>();
		info.msg.lootableCorpse.privateData.container = Facepunch.Pool.GetList<ProtoBuf.ItemContainer>();
		ItemContainer[] array = containers;
		foreach (ItemContainer itemContainer in array)
		{
			if (itemContainer != null)
			{
				ProtoBuf.ItemContainer itemContainer2 = itemContainer.Save();
				if (itemContainer2 != null)
				{
					info.msg.lootableCorpse.privateData.container.Add(itemContainer2);
				}
			}
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lootableCorpse == null)
		{
			return;
		}
		playerName = info.msg.lootableCorpse.playerName;
		playerSteamID = info.msg.lootableCorpse.playerID;
		if (info.fromDisk && info.msg.lootableCorpse.privateData != null && info.msg.lootableCorpse.privateData.container != null)
		{
			int count = info.msg.lootableCorpse.privateData.container.Count;
			containers = new ItemContainer[count];
			for (int i = 0; i < count; i++)
			{
				containers[i] = new ItemContainer();
				containers[i].ServerInitialize(null, info.msg.lootableCorpse.privateData.container[i].slots);
				containers[i].GiveUID();
				containers[i].entityOwner = this;
				containers[i].Load(info.msg.lootableCorpse.privateData.container[i]);
			}
		}
	}
}
