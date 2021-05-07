#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Signage : IOEntity, ILOD, ISignage
{
	private const float TextureRequestTimeout = 15f;

	public GameObjectRef changeTextDialog;

	public MeshPaintableSource[] paintableSources;

	[NonSerialized]
	public uint[] textureIDs;

	public Vector2i TextureSize
	{
		get
		{
			if (paintableSources == null || paintableSources.Length == 0)
			{
				return Vector2i.zero;
			}
			MeshPaintableSource meshPaintableSource = paintableSources[0];
			return new Vector2i(meshPaintableSource.texWidth, meshPaintableSource.texHeight);
		}
	}

	public int TextureCount
	{
		get
		{
			MeshPaintableSource[] array = paintableSources;
			if (array == null)
			{
				return 0;
			}
			return array.Length;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Signage.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1455609404 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log(string.Concat("SV_RPCMessage: ", player, " - LockSign "));
				}
				using (TimeWarning.New("LockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455609404u, "LockSign", this, player, 3f))
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
							LockSign(msg2);
						}
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
						player.Kick("RPC Error in LockSign");
					}
				}
				return true;
			}
			if (rpc == 4149904254u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log(string.Concat("SV_RPCMessage: ", player, " - UnLockSign "));
				}
				using (TimeWarning.New("UnLockSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4149904254u, "UnLockSign", this, player, 3f))
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
							UnLockSign(msg3);
						}
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
						player.Kick("RPC Error in UnLockSign");
					}
				}
				return true;
			}
			if (rpc == 1255380462 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					UnityEngine.Debug.Log(string.Concat("SV_RPCMessage: ", player, " - UpdateSign "));
				}
				using (TimeWarning.New("UpdateSign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1255380462u, "UpdateSign", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(1255380462u, "UpdateSign", this, player, 5f))
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
							RPCMessage msg4 = rPCMessage;
							UpdateSign(msg4);
						}
					}
					catch (Exception exception3)
					{
						UnityEngine.Debug.LogException(exception3);
						player.Kick("RPC Error in UpdateSign");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (paintableSources != null && paintableSources.Length > 1)
		{
			MeshPaintableSource meshPaintableSource = paintableSources[0];
			for (int i = 1; i < paintableSources.Length; i++)
			{
				MeshPaintableSource obj = paintableSources[i];
				obj.texWidth = meshPaintableSource.texWidth;
				obj.texHeight = meshPaintableSource.texHeight;
			}
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.MaxDistance(5f)]
	public void UpdateSign(RPCMessage msg)
	{
		if (msg.player == null || !CanUpdateSign(msg.player))
		{
			return;
		}
		int num = msg.read.Int32();
		if (num < 0 || num >= paintableSources.Length)
		{
			return;
		}
		byte[] array = msg.read.BytesWithSize();
		if (msg.read.Unread > 0 && msg.read.Bit() && !msg.player.IsAdmin)
		{
			UnityEngine.Debug.LogWarning($"{msg.player} tried to upload a sign from a file but they aren't admin, ignoring");
			return;
		}
		EnsureInitialized();
		if (array == null)
		{
			if (textureIDs[num] != 0)
			{
				FileStorage.server.RemoveExact(textureIDs[num], FileStorage.Type.png, net.ID, (uint)num);
			}
			textureIDs[num] = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidPNG(array, 1024, 1024))
			{
				return;
			}
			if (textureIDs[num] != 0)
			{
				FileStorage.server.RemoveExact(textureIDs[num], FileStorage.Type.png, net.ID, (uint)num);
			}
			textureIDs[num] = FileStorage.server.Store(array, FileStorage.Type.png, net.ID, (uint)num);
		}
		SendNetworkUpdate();
		Interface.CallHook("OnSignUpdated", this, msg.player);
	}

	public void EnsureInitialized()
	{
		int num = Mathf.Max(paintableSources.Length, 1);
		if (textureIDs == null || textureIDs.Length != num)
		{
			Array.Resize(ref textureIDs, num);
		}
	}

	[Conditional("SIGN_DEBUG")]
	private static void SignDebugLog(string str)
	{
		UnityEngine.Debug.Log(str);
	}

	public virtual bool CanUpdateSign(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUpdateSign", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (player.IsAdmin || player.IsDeveloper)
		{
			return true;
		}
		if (!player.CanBuild())
		{
			return false;
		}
		if (IsLocked())
		{
			return player.userID == base.OwnerID;
		}
		return true;
	}

	public bool CanUnlockSign(BasePlayer player)
	{
		if (!IsLocked())
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	public bool CanLockSign(BasePlayer player)
	{
		if (IsLocked())
		{
			return false;
		}
		return CanUpdateSign(player);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		EnsureInitialized();
		bool flag = false;
		if (info.msg.sign != null)
		{
			uint num = textureIDs[0];
			if (info.msg.sign.imageIds != null && info.msg.sign.imageIds.Count > 0)
			{
				int num2 = Mathf.Min(info.msg.sign.imageIds.Count, textureIDs.Length);
				for (int i = 0; i < num2; i++)
				{
					uint num3 = info.msg.sign.imageIds[i];
					bool flag2 = num3 != textureIDs[i];
					flag = flag || flag2;
					textureIDs[i] = num3;
				}
			}
			else
			{
				flag = num != info.msg.sign.imageid;
				textureIDs[0] = info.msg.sign.imageid;
			}
		}
		if (!base.isServer)
		{
			return;
		}
		bool flag3 = false;
		for (int j = 0; j < paintableSources.Length; j++)
		{
			uint num4 = textureIDs[j];
			if (num4 != 0)
			{
				byte[] array = FileStorage.server.Get(num4, FileStorage.Type.png, net.ID, (uint)j);
				if (array == null)
				{
					Log($"Frame {j} (id={num4}) doesn't exist, clearing");
					textureIDs[j] = 0u;
				}
				flag3 = flag3 || array != null;
			}
		}
		if (!flag3)
		{
			SetFlag(Flags.Locked, false);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void LockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUpdateSign(msg.player))
		{
			SetFlag(Flags.Locked, true);
			SendNetworkUpdate();
			base.OwnerID = msg.player.userID;
			Interface.CallHook("OnSignLocked", this, msg.player);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void UnLockSign(RPCMessage msg)
	{
		if (msg.player.CanInteract() && CanUnlockSign(msg.player))
		{
			SetFlag(Flags.Locked, false);
			SendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		EnsureInitialized();
		List<uint> list = Facepunch.Pool.GetList<uint>();
		uint[] array = textureIDs;
		foreach (uint item in array)
		{
			list.Add(item);
		}
		info.msg.sign = Facepunch.Pool.Get<Sign>();
		info.msg.sign.imageid = 0u;
		info.msg.sign.imageIds = list;
	}

	public override void OnKilled(HitInfo info)
	{
		if (net != null)
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
		}
		if (textureIDs != null)
		{
			Array.Clear(textureIDs, 0, textureIDs.Length);
		}
		base.OnKilled(info);
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override string Categorize()
	{
		return "sign";
	}
}
