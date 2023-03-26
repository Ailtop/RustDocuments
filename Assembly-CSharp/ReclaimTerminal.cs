#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class ReclaimTerminal : StorageContainer
{
	public int itemCount;

	public static readonly Translate.Phrase DespawnToast = new Translate.Phrase("softcore.reclaimdespawn", "Items remaining in the reclaim terminal will despawn in two hours.");

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ReclaimTerminal.OnRpcMessage"))
		{
			if (rpc == 2609933020u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_ReloadLoot "));
				}
				using (TimeWarning.New("RPC_ReloadLoot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2609933020u, "RPC_ReloadLoot", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2609933020u, "RPC_ReloadLoot", this, player, 3f))
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
							RPC_ReloadLoot(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_ReloadLoot");
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
		base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_ReloadLoot(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && !(ReclaimManager.instance == null) && !(player.inventory.loot.entitySource != this))
		{
			LoadReclaimLoot(player);
		}
	}

	public void LoadReclaimLoot(BasePlayer player)
	{
		if (ReclaimManager.instance == null)
		{
			return;
		}
		List<ReclaimManager.PlayerReclaimEntry> list = Facepunch.Pool.GetList<ReclaimManager.PlayerReclaimEntry>();
		ReclaimManager.instance.GetReclaimsForPlayer(player.userID, ref list);
		itemCount = 0;
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			if (base.inventory.GetSlot(i) != null)
			{
				itemCount++;
			}
		}
		foreach (ReclaimManager.PlayerReclaimEntry item2 in list)
		{
			for (int num = item2.inventory.itemList.Count - 1; num >= 0; num--)
			{
				Item item = item2.inventory.itemList[num];
				itemCount++;
				item.MoveToContainer(base.inventory);
			}
		}
		Facepunch.Pool.FreeList(ref list);
		SendNetworkUpdate();
	}

	public override bool PlayerOpenLoot(BasePlayer player, string panelToOpen = "", bool doPositionChecks = true)
	{
		if (ReclaimManager.instance == null)
		{
			return false;
		}
		LoadReclaimLoot(player);
		return base.PlayerOpenLoot(player, panelToOpen, doPositionChecks);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		if (!(ReclaimManager.instance == null))
		{
			ReclaimManager.instance.DoCleanup();
			if (base.inventory.itemList.Count > 0)
			{
				ReclaimManager.instance.AddPlayerReclaim(player.userID, base.inventory.itemList, 0uL);
				player.ShowToast(GameTip.Styles.Blue_Long, DespawnToast);
			}
			base.PlayerStoppedLooting(player);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.reclaimTerminal = Facepunch.Pool.Get<ProtoBuf.ReclaimTerminal>();
			info.msg.reclaimTerminal.itemCount = itemCount;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk && info.msg.reclaimTerminal != null)
		{
			itemCount = info.msg.reclaimTerminal.itemCount;
		}
	}
}
