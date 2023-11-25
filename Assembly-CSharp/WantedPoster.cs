#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust.UI;
using UnityEngine;
using UnityEngine.Assertions;

public class WantedPoster : DecayEntity, ISignage, IUGCBrowserEntity, ILOD, IServerFileReceiver
{
	private uint imageCrc;

	private ulong playerId;

	private string playerName;

	public MeshRenderer PhotoImage;

	public RustText WantedName;

	public GameObjectRef AssignDialog;

	public const Flags HasTarget = Flags.Reserved1;

	public uiPlayerPreview.EffectMode EffectMode = uiPlayerPreview.EffectMode.Polaroid;

	public uint[] GetContentCRCs
	{
		get
		{
			if (imageCrc == 0)
			{
				return null;
			}
			return new uint[1] { imageCrc };
		}
	}

	public UGCType ContentType => UGCType.ImageJpg;

	public List<ulong> EditingHistory { get; } = new List<ulong>();


	public BaseNetworkable UgcEntity => this;

	public Vector2i TextureSize => new Vector2i(1024, 1024);

	public int TextureCount => 1;

	public NetworkableId NetworkID => net.ID;

	public FileStorage.Type FileType => FileStorage.Type.jpg;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WantedPoster.OnRpcMessage"))
		{
			if (rpc == 2419123501u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClearPlayer ");
				}
				using (TimeWarning.New("ClearPlayer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2419123501u, "ClearPlayer", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2419123501u, "ClearPlayer", this, player, 3f))
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
							ClearPlayer(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ClearPlayer");
					}
				}
				return true;
			}
			if (rpc == 657465493 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UpdatePoster ");
				}
				using (TimeWarning.New("UpdatePoster"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(657465493u, "UpdatePoster", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(657465493u, "UpdatePoster", this, player, 3f))
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
							UpdatePoster(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in UpdatePoster");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void UpdatePoster(RPCMessage msg)
	{
		if (msg.player == null || !CanUpdateSign(msg.player))
		{
			return;
		}
		ulong num = msg.read.UInt64();
		string text = msg.read.String();
		byte[] array = msg.read.BytesWithSize();
		playerId = num;
		playerName = text;
		SetFlag(Flags.Reserved1, b: true);
		if (array == null)
		{
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
			}
			imageCrc = 0u;
		}
		else
		{
			if (!ImageProcessing.IsValidJPG(array, 1024, 1024))
			{
				return;
			}
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
			}
			imageCrc = FileStorage.server.Store(array, FileType, net.ID);
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void ClearPlayer(RPCMessage msg)
	{
		if (!(msg.player == null) && CanUpdateSign(msg.player))
		{
			playerId = 0uL;
			playerName = string.Empty;
			SetFlag(Flags.Reserved1, b: false);
			if (imageCrc != 0)
			{
				FileStorage.server.RemoveExact(imageCrc, FileType, net.ID, 0u);
				imageCrc = 0u;
			}
			SendNetworkUpdate();
		}
	}

	public void SetTextureCRCs(uint[] crcs)
	{
		imageCrc = crcs[0];
		SendNetworkUpdate();
	}

	public void ClearContent()
	{
		imageCrc = 0u;
		SendNetworkUpdate();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.wantedPoster = Facepunch.Pool.Get<ProtoBuf.WantedPoster>();
		info.msg.wantedPoster.imageCrc = imageCrc;
		info.msg.wantedPoster.playerId = playerId;
		info.msg.wantedPoster.playerName = playerName;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.wantedPoster != null)
		{
			imageCrc = info.msg.wantedPoster.imageCrc;
			playerName = info.msg.wantedPoster.playerName;
			playerId = info.msg.wantedPoster.playerId;
		}
	}

	public bool CanUpdateSign(BasePlayer player)
	{
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

	public uint[] GetTextureCRCs()
	{
		return new uint[1] { imageCrc };
	}
}
