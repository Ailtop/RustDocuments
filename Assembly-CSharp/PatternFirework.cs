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

public class PatternFirework : MortarFirework, IUGCBrowserEntity
{
	public enum FuseLength
	{
		Short = 0,
		Medium = 1,
		Long = 2,
		Max = 2
	}

	public const int CurrentVersion = 1;

	[Header("PatternFirework")]
	public GameObjectRef FireworkDesignerDialog;

	public int MaxStars = 25;

	public float ShellFuseLengthShort = 3f;

	public float ShellFuseLengthMed = 5.5f;

	public float ShellFuseLengthLong = 8f;

	[NonSerialized]
	public ProtoBuf.PatternFirework.Design Design;

	[NonSerialized]
	public FuseLength ShellFuseLength;

	public uint[] GetContentCRCs
	{
		get
		{
			if (Design == null || Design.stars.Count <= 0)
			{
				return Array.Empty<uint>();
			}
			return new uint[1] { 1u };
		}
	}

	public UGCType ContentType => UGCType.PatternBoomer;

	public List<ulong> EditingHistory
	{
		get
		{
			if (Design == null)
			{
				return new List<ulong>();
			}
			return new List<ulong> { Design.editedBy };
		}
	}

	public BaseNetworkable UgcEntity => this;

	public override void DestroyShared()
	{
		base.DestroyShared();
		Design?.Dispose();
		Design = null;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ShellFuseLength = FuseLength.Medium;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void StartOpenDesigner(RPCMessage rpc)
	{
		if (PlayerCanModify(rpc.player))
		{
			ClientRPCPlayer(null, rpc.player, "OpenDesigner");
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	private void ServerSetFireworkDesign(RPCMessage rpc)
	{
		if (!PlayerCanModify(rpc.player))
		{
			return;
		}
		ProtoBuf.PatternFirework.Design design = ProtoBuf.PatternFirework.Design.Deserialize(rpc.read);
		if (Interface.CallHook("OnFireworkDesignChange", this, design, rpc.player) != null)
		{
			return;
		}
		if (design?.stars != null)
		{
			while (design.stars.Count > MaxStars)
			{
				int index = design.stars.Count - 1;
				design.stars[index].Dispose();
				design.stars.RemoveAt(index);
			}
			foreach (ProtoBuf.PatternFirework.Star star in design.stars)
			{
				star.position = new Vector2(Mathf.Clamp(star.position.x, -1f, 1f), Mathf.Clamp(star.position.y, -1f, 1f));
				star.color = new Color(Mathf.Clamp01(star.color.r), Mathf.Clamp01(star.color.g), Mathf.Clamp01(star.color.b), 1f);
			}
			design.editedBy = rpc.player.userID;
		}
		Design?.Dispose();
		Design = design;
		Interface.CallHook("OnFireworkDesignChanged", this, design, rpc.player);
		SendNetworkUpdateImmediate();
	}

	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	private void SetShellFuseLength(RPCMessage rpc)
	{
		if (PlayerCanModify(rpc.player))
		{
			ShellFuseLength = (FuseLength)Mathf.Clamp(rpc.read.Int32(), 0, 2);
			SendNetworkUpdateImmediate();
		}
	}

	private bool PlayerCanModify(BasePlayer player)
	{
		if (player == null || !player.CanInteract())
		{
			return false;
		}
		object obj = Interface.CallHook("CanDesignFirework", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		BuildingPrivlidge buildingPrivilege = GetBuildingPrivilege();
		if (buildingPrivilege != null && !buildingPrivilege.CanAdministrate(player))
		{
			return false;
		}
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.patternFirework = Facepunch.Pool.Get<ProtoBuf.PatternFirework>();
		info.msg.patternFirework.design = Design?.Copy();
		info.msg.patternFirework.shellFuseLength = (int)ShellFuseLength;
	}

	public void ClearContent()
	{
		Design?.Dispose();
		Design = null;
		SendNetworkUpdateImmediate();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.patternFirework != null)
		{
			Design?.Dispose();
			Design = info.msg.patternFirework.design?.Copy();
			ShellFuseLength = (FuseLength)info.msg.patternFirework.shellFuseLength;
		}
	}

	public float GetShellFuseLength()
	{
		return ShellFuseLength switch
		{
			FuseLength.Short => ShellFuseLengthShort, 
			FuseLength.Medium => ShellFuseLengthMed, 
			FuseLength.Long => ShellFuseLengthLong, 
			_ => ShellFuseLengthMed, 
		};
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PatternFirework.OnRpcMessage"))
		{
			if (rpc == 3850129568u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerSetFireworkDesign "));
				}
				using (TimeWarning.New("ServerSetFireworkDesign"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3850129568u, "ServerSetFireworkDesign", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3850129568u, "ServerSetFireworkDesign", this, player, 3f))
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
							ServerSetFireworkDesign(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerSetFireworkDesign");
					}
				}
				return true;
			}
			if (rpc == 2132764204 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetShellFuseLength "));
				}
				using (TimeWarning.New("SetShellFuseLength"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2132764204u, "SetShellFuseLength", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2132764204u, "SetShellFuseLength", this, player, 3f))
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
							RPCMessage shellFuseLength = rPCMessage;
							SetShellFuseLength(shellFuseLength);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SetShellFuseLength");
					}
				}
				return true;
			}
			if (rpc == 2760408151u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - StartOpenDesigner "));
				}
				using (TimeWarning.New("StartOpenDesigner"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2760408151u, "StartOpenDesigner", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2760408151u, "StartOpenDesigner", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							StartOpenDesigner(rpc3);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in StartOpenDesigner");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}
}
