#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class SpookySpeaker : BaseCombatEntity
{
	public SoundPlayer soundPlayer;

	public float soundSpacing = 12f;

	public float soundSpacingRand = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SpookySpeaker.OnRpcMessage"))
		{
			if (rpc == 2523893445u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetWantsOn "));
				}
				using (TimeWarning.New("SetWantsOn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2523893445u, "SetWantsOn", this, player, 3f))
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
							RPCMessage wantsOn = rPCMessage;
							SetWantsOn(wantsOn);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetWantsOn");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateInvokes();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetWantsOn(RPCMessage msg)
	{
		bool b = msg.read.Bit();
		SetFlag(Flags.On, b);
		UpdateInvokes();
	}

	public void UpdateInvokes()
	{
		if (IsOn())
		{
			InvokeRandomized(SendPlaySound, soundSpacing, soundSpacing, soundSpacingRand);
			Invoke(DelayedOff, 7200f);
		}
		else
		{
			CancelInvoke(SendPlaySound);
			CancelInvoke(DelayedOff);
		}
	}

	public void SendPlaySound()
	{
		ClientRPC(null, "PlaySpookySound");
	}

	public void DelayedOff()
	{
		SetFlag(Flags.On, b: false);
	}
}
