#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class MapEntity : HeldEntity
{
	[NonSerialized]
	public uint[] fogImages = new uint[1];

	[NonSerialized]
	public uint[] paintImages = new uint[144];

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MapEntity.OnRpcMessage"))
		{
			if (rpc == 1443560440 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ImageUpdate "));
				}
				using (TimeWarning.New("ImageUpdate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1443560440u, "ImageUpdate", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(1443560440u, "ImageUpdate", this, player))
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
							ImageUpdate(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ImageUpdate");
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
		if (info.msg.mapEntity != null)
		{
			if (info.msg.mapEntity.fogImages.Count == fogImages.Length)
			{
				fogImages = info.msg.mapEntity.fogImages.ToArray();
			}
			if (info.msg.mapEntity.paintImages.Count == paintImages.Length)
			{
				paintImages = info.msg.mapEntity.paintImages.ToArray();
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mapEntity = Facepunch.Pool.Get<ProtoBuf.MapEntity>();
		info.msg.mapEntity.fogImages = Facepunch.Pool.Get<List<uint>>();
		info.msg.mapEntity.fogImages.AddRange(fogImages);
		info.msg.mapEntity.paintImages = Facepunch.Pool.Get<List<uint>>();
		info.msg.mapEntity.paintImages.AddRange(paintImages);
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.FromOwner]
	[RPC_Server]
	public void ImageUpdate(RPCMessage msg)
	{
		if (msg.player == null)
		{
			return;
		}
		byte b = msg.read.UInt8();
		byte b2 = msg.read.UInt8();
		uint num = msg.read.UInt32();
		if ((b == 0 && fogImages[b2] == num) || (b == 1 && paintImages[b2] == num))
		{
			return;
		}
		uint num2 = (uint)(b * 1000 + b2);
		byte[] array = msg.read.BytesWithSize();
		if (array != null)
		{
			FileStorage.server.RemoveEntityNum(net.ID, num2);
			uint num3 = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, num2);
			if (b == 0)
			{
				fogImages[b2] = num3;
			}
			if (b == 1)
			{
				paintImages[b2] = num3;
			}
			InvalidateNetworkCache();
			Interface.CallHook("OnMapImageUpdated");
		}
	}
}
