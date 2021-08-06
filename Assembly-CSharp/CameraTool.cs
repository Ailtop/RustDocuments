#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class CameraTool : HeldEntity
{
	public GameObjectRef screenshotEffect;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CameraTool.OnRpcMessage"))
		{
			if (rpc == 3167878597u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SVNoteScreenshot "));
				}
				using (TimeWarning.New("SVNoteScreenshot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(3167878597u, "SVNoteScreenshot", this, player))
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
							SVNoteScreenshot(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVNoteScreenshot");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void SVNoteScreenshot(RPCMessage msg)
	{
	}
}
