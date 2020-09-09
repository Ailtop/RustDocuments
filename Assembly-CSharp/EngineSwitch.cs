#define UNITY_ASSERTIONS
using ConVar;
using Network;
using Oxide.Core;
using System;
using UnityEngine;
using UnityEngine.Assertions;

public class EngineSwitch : BaseEntity
{
	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("EngineSwitch.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1249530220 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - StartEngine ");
				}
				using (TimeWarning.New("StartEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1249530220u, "StartEngine", this, player, 3f))
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
							StartEngine(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in StartEngine");
					}
				}
				return true;
			}
			if (rpc == 1739656243 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - StopEngine ");
				}
				using (TimeWarning.New("StopEngine"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1739656243u, "StopEngine", this, player, 3f))
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
							StopEngine(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in StopEngine");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void StopEngine(RPCMessage msg)
	{
		MiningQuarry miningQuarry = GetParentEntity() as MiningQuarry;
		if ((bool)miningQuarry)
		{
			miningQuarry.EngineSwitch(false);
			Interface.CallHook("OnQuarryToggled", miningQuarry, msg.player);
		}
	}

	[RPC_Server.MaxDistance(3f)]
	[RPC_Server]
	public void StartEngine(RPCMessage msg)
	{
		MiningQuarry miningQuarry = GetParentEntity() as MiningQuarry;
		if ((bool)miningQuarry)
		{
			miningQuarry.EngineSwitch(true);
			Interface.CallHook("OnQuarryToggled", miningQuarry, msg.player);
		}
	}
}
