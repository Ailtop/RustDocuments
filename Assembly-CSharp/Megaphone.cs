#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class Megaphone : HeldEntity
{
	[Header("Megaphone")]
	public VoiceProcessor voiceProcessor;

	public float VoiceDamageMinFrequency = 2f;

	public float VoiceDamageAmount = 1f;

	public AudioSource VoiceSource;

	public SoundDefinition StartBroadcastingSfx;

	public SoundDefinition StopBroadcastingSfx;

	[ReplicatedVar(Default = "100")]
	public static float MegaphoneVoiceRange { get; set; } = 100f;


	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Megaphone.OnRpcMessage"))
		{
			if (rpc == 4196056309u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_ToggleBroadcasting "));
				}
				using (TimeWarning.New("Server_ToggleBroadcasting"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwner.Test(4196056309u, "Server_ToggleBroadcasting", this, player))
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
							Server_ToggleBroadcasting(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_ToggleBroadcasting");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void UpdateItemCondition()
	{
		Item ownerItem = GetOwnerItem();
		if (ownerItem != null && ownerItem.hasCondition)
		{
			ownerItem.LoseCondition(VoiceDamageAmount);
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	private void Server_ToggleBroadcasting(RPCMessage msg)
	{
		bool flag = msg.read.Int8() == 1;
		SetFlag(Flags.On, flag);
		if (flag)
		{
			if (!IsInvoking(UpdateItemCondition))
			{
				InvokeRepeating(UpdateItemCondition, 0f, VoiceDamageMinFrequency);
			}
		}
		else if (IsInvoking(UpdateItemCondition))
		{
			CancelInvoke(UpdateItemCondition);
		}
	}
}
