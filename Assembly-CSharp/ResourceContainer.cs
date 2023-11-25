#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class ResourceContainer : EntityComponent<BaseEntity>
{
	public bool lootable = true;

	[NonSerialized]
	public ItemContainer container;

	[NonSerialized]
	public float lastAccessTime;

	public int accessedSecondsAgo => (int)(UnityEngine.Time.realtimeSinceStartup - lastAccessTime);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ResourceContainer.OnRpcMessage"))
		{
			if (rpc == 548378753 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - StartLootingContainer ");
				}
				using (TimeWarning.New("StartLootingContainer"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.IsVisible.Test(548378753u, "StartLootingContainer", GetBaseEntity(), player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage rPCMessage = default(BaseEntity.RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							BaseEntity.RPCMessage msg2 = rPCMessage;
							StartLootingContainer(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in StartLootingContainer");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[BaseEntity.RPC_Server.IsVisible(3f)]
	[BaseEntity.RPC_Server]
	private void StartLootingContainer(BaseEntity.RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((bool)player && player.CanInteract() && lootable && Interface.CallHook("CanLootEntity", player, this) == null && player.inventory.loot.StartLootingEntity(base.baseEntity))
		{
			lastAccessTime = UnityEngine.Time.realtimeSinceStartup;
			player.inventory.loot.AddContainer(container);
		}
	}
}
