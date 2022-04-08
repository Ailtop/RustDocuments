#define UNITY_ASSERTIONS
using System;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PaintedItemStorageEntity : BaseEntity, IServerFileReceiver
{
	public uint _currentImageCrc;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PaintedItemStorageEntity.OnRpcMessage"))
		{
			if (rpc == 2439017595u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					UnityEngine.Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_UpdateImage "));
				}
				using (TimeWarning.New("Server_UpdateImage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2439017595u, "Server_UpdateImage", this, player, 3uL))
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
							Server_UpdateImage(msg2);
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
						player.Kick("RPC Error in Server_UpdateImage");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.paintedItem != null)
		{
			_currentImageCrc = info.msg.paintedItem.imageCrc;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.paintedItem = Facepunch.Pool.Get<PaintedItem>();
		info.msg.paintedItem.imageCrc = _currentImageCrc;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(3uL)]
	private void Server_UpdateImage(RPCMessage msg)
	{
		if (msg.player == null || msg.player.userID != base.OwnerID)
		{
			return;
		}
		foreach (Item item2 in msg.player.inventory.containerWear.itemList)
		{
			if (item2.instanceData != null && item2.instanceData.subEntity == net.ID)
			{
				return;
			}
		}
		Item item = msg.player.inventory.FindBySubEntityID(net.ID);
		if (item == null || item.isBroken)
		{
			return;
		}
		byte[] array = msg.read.BytesWithSize();
		if (array == null)
		{
			if (_currentImageCrc != 0)
			{
				FileStorage.server.RemoveExact(_currentImageCrc, FileStorage.Type.png, net.ID, 0u);
			}
			_currentImageCrc = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidPNG(array, 512, 512))
			{
				return;
			}
			uint currentImageCrc = _currentImageCrc;
			if (_currentImageCrc != 0)
			{
				FileStorage.server.RemoveExact(_currentImageCrc, FileStorage.Type.png, net.ID, 0u);
			}
			_currentImageCrc = FileStorage.server.Store(array, FileStorage.Type.png, net.ID);
			if (_currentImageCrc != currentImageCrc)
			{
				item.LoseCondition(0.25f);
			}
		}
		Interface.CallHook("OnItemPainted", this, item, msg.player, array);
		SendNetworkUpdate();
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (!Rust.Application.isQuitting && net != null)
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
		}
	}

	[Conditional("PAINTED_ITEM_DEBUG")]
	private void DebugOnlyLog(string str)
	{
		UnityEngine.Debug.Log(str, this);
	}
}
