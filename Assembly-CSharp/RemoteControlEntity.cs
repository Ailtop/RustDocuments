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

public class RemoteControlEntity : BaseCombatEntity, IRemoteControllable
{
	public static List<IRemoteControllable> allControllables = new List<IRemoteControllable>();

	[Header("RC Entity")]
	public string rcIdentifier = "NONE";

	public Transform viewEyes;

	public GameObjectRef IDPanelPrefab;

	public bool IsBeingControlled { get; set; }

	public virtual bool RequiresMouse => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RemoteControlEntity.OnRpcMessage"))
		{
			if (rpc == 1053317251 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_SetID "));
				}
				using (TimeWarning.New("Server_SetID"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1053317251u, "Server_SetID", this, player, 3f))
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
							Server_SetID(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_SetID");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public Transform GetEyes()
	{
		return viewEyes;
	}

	public BaseEntity GetEnt()
	{
		return this;
	}

	public bool Occupied()
	{
		return false;
	}

	public string GetIdentifier()
	{
		return rcIdentifier;
	}

	public virtual void UserInput(InputState inputState, BasePlayer player)
	{
	}

	public virtual void InitializeControl(BasePlayer controller)
	{
		IsBeingControlled = true;
	}

	public virtual void StopControl()
	{
		IsBeingControlled = false;
	}

	public void UpdateIdentifier(string newID, bool clientSend = false)
	{
		string rcIdentifier2 = rcIdentifier;
		if (base.isServer)
		{
			if (!IDInUse(newID))
			{
				rcIdentifier = newID;
				Debug.Log("Updated Identifier to : " + rcIdentifier);
			}
			else
			{
				Debug.Log("ID In use!" + newID);
			}
			SendNetworkUpdate();
		}
	}

	public virtual void RCSetup()
	{
		if (base.isServer)
		{
			allControllables.Add(this);
		}
	}

	public virtual void RCShutdown()
	{
		if (base.isServer)
		{
			allControllables.Remove(this);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		RCSetup();
	}

	public override void DestroyShared()
	{
		RCShutdown();
		base.DestroyShared();
	}

	public virtual bool CanControl()
	{
		object obj = Interface.CallHook("OnEntityControl", this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void Server_SetID(RPCMessage msg)
	{
		if (!CanControl())
		{
			return;
		}
		string text = msg.read.String();
		if (ComputerStation.IsValidIdentifier(text))
		{
			string text2 = msg.read.String();
			if (ComputerStation.IsValidIdentifier(text2) && text == GetIdentifier())
			{
				Debug.Log("SetID success!");
				UpdateIdentifier(text2);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.rcEntity = Facepunch.Pool.Get<RCEntity>();
		info.msg.rcEntity.identifier = GetIdentifier();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null && ComputerStation.IsValidIdentifier(info.msg.rcEntity.identifier))
		{
			UpdateIdentifier(info.msg.rcEntity.identifier);
		}
	}

	public static bool IDInUse(string id)
	{
		return FindByID(id) != null;
	}

	public static IRemoteControllable FindByID(string id)
	{
		foreach (IRemoteControllable allControllable in allControllables)
		{
			if (allControllable != null && allControllable.GetIdentifier() == id)
			{
				return allControllable;
			}
		}
		return null;
	}

	public static bool InstallControllable(IRemoteControllable newControllable)
	{
		if (allControllables.Contains(newControllable))
		{
			return false;
		}
		allControllables.Add(newControllable);
		return true;
	}

	public static bool RemoveControllable(IRemoteControllable newControllable)
	{
		if (!allControllables.Contains(newControllable))
		{
			return false;
		}
		allControllables.Remove(newControllable);
		return true;
	}
}
