#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class VehiclePrivilege : BaseEntity
{
	public List<PlayerNameID> authorizedPlayers = new List<PlayerNameID>();

	public const Flags Flag_MaxAuths = Flags.Reserved5;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehiclePrivilege.OnRpcMessage"))
		{
			if (rpc == 1092560690 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - AddSelfAuthorize ");
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
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
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 253307592 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ClearList ");
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(253307592u, "ClearList", this, player, 3f))
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
							ClearList(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RemoveSelfAuthorize ");
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							RemoveSelfAuthorize(rpc4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		base.ResetState();
		authorizedPlayers.Clear();
	}

	public bool IsAuthed(BasePlayer player)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == player.userID);
	}

	public bool IsAuthed(ulong userID)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingPrivilege = Facepunch.Pool.Get<BuildingPrivilege>();
		info.msg.buildingPrivilege.users = authorizedPlayers;
	}

	public override void PostSave(SaveInfo info)
	{
		info.msg.buildingPrivilege.users = null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		authorizedPlayers.Clear();
		if (info.msg.buildingPrivilege != null && info.msg.buildingPrivilege.users != null)
		{
			authorizedPlayers = info.msg.buildingPrivilege.users;
			info.msg.buildingPrivilege.users = null;
		}
	}

	public bool IsDriver(BasePlayer player)
	{
		BaseEntity baseEntity = GetParentEntity();
		if (baseEntity == null)
		{
			return false;
		}
		BaseVehicle baseVehicle = baseEntity as BaseVehicle;
		if (baseVehicle == null)
		{
			return false;
		}
		return baseVehicle.IsDriver(player);
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void UpdateMaxAuthCapacity()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((bool)activeGameMode && activeGameMode.limitTeamAuths)
		{
			SetFlag(Flags.Reserved5, authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize());
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void AddSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && IsDriver(rpc.player) && Interface.CallHook("OnCupboardAuthorize", this, rpc.player) == null)
		{
			AddPlayer(rpc.player);
			SendNetworkUpdate();
		}
	}

	public void AddPlayer(BasePlayer player)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == player.userID);
			PlayerNameID playerNameID = new PlayerNameID();
			playerNameID.userid = player.userID;
			playerNameID.username = player.displayName;
			authorizedPlayers.Add(playerNameID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, player, authorizedPlayers.Select((PlayerNameID x) => x.userid), "added", player.userID);
			UpdateMaxAuthCapacity();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void RemoveSelfAuthorize(RPCMessage rpc)
	{
		RPCMessage rpc2 = rpc;
		if (rpc2.player.CanInteract() && IsDriver(rpc2.player) && Interface.CallHook("OnCupboardDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == rpc2.player.userID);
			Facepunch.Rust.Analytics.Azure.OnEntityAuthChanged(this, rpc2.player, authorizedPlayers.Select((PlayerNameID x) => x.userid), "removed", rpc2.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	private void ClearList(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && IsDriver(rpc.player) && Interface.CallHook("OnCupboardClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}
}
