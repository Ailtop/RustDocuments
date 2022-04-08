#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class CollectableEasterEgg : BaseEntity
{
	public Transform artwork;

	public float bounceRange = 0.2f;

	public float bounceSpeed = 1f;

	public GameObjectRef pickupEffect;

	public ItemDefinition itemToGive;

	private float lastPickupStartTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CollectableEasterEgg.OnRpcMessage"))
		{
			if (rpc == 2436818324u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_PickUp "));
				}
				using (TimeWarning.New("RPC_PickUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2436818324u, "RPC_PickUp", this, player, 3f))
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
							RPC_PickUp(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_PickUp");
					}
				}
				return true;
			}
			if (rpc == 2243088389u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_StartPickUp "));
				}
				using (TimeWarning.New("RPC_StartPickUp"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2243088389u, "RPC_StartPickUp", this, player, 3f))
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
							RPC_StartPickUp(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_StartPickUp");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		int num = UnityEngine.Random.Range(0, 3);
		SetFlag(Flags.Reserved1, num == 0, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved2, num == 1, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved3, num == 2, recursive: false, networkupdate: false);
		base.ServerInit();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_StartPickUp(RPCMessage msg)
	{
		if (!(msg.player == null))
		{
			lastPickupStartTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_PickUp(RPCMessage msg)
	{
		if (msg.player == null)
		{
			return;
		}
		float num = UnityEngine.Time.realtimeSinceStartup - lastPickupStartTime;
		if (!(msg.player.GetHeldEntity() as EasterBasket) && (num > 2f || num < 0.8f))
		{
			return;
		}
		if ((bool)EggHuntEvent.serverEvent)
		{
			if (!EggHuntEvent.serverEvent.IsEventActive())
			{
				return;
			}
			EggHuntEvent.serverEvent.EggCollected(msg.player);
			int iAmount = 1;
			msg.player.GiveItem(ItemManager.Create(itemToGive, iAmount, 0uL));
		}
		Effect.server.Run(pickupEffect.resourcePath, base.transform.position + Vector3.up * 0.3f, Vector3.up);
		Kill();
	}
}
