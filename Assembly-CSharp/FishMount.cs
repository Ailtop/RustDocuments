#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class FishMount : StorageContainer
{
	public Animator[] FishRoots = new Animator[0];

	public GameObjectRef FishInteractSound = new GameObjectRef();

	public float UseCooldown = 3f;

	public const Flags HasFish = Flags.Reserved1;

	private int GetCurrentFishItemIndex
	{
		get
		{
			if (base.inventory.GetSlot(0) == null || !base.inventory.GetSlot(0).info.TryGetComponent<ItemModFishable>(out var component))
			{
				return -1;
			}
			return component.FishMountIndex;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FishMount.OnRpcMessage"))
		{
			if (rpc == 3280542489u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - UseFish ");
				}
				using (TimeWarning.New("UseFish"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3280542489u, "UseFish", this, player, 3f))
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
							UseFish(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in UseFish");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.simpleInt == null)
		{
			info.msg.simpleInt = Facepunch.Pool.Get<SimpleInt>();
		}
		info.msg.simpleInt.value = GetCurrentFishItemIndex;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (item.info.TryGetComponent<ItemModFishable>(out var component) && component.CanBeMounted)
		{
			return true;
		}
		return false;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetFlag(Flags.Busy, b: false);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		SetFlag(Flags.Reserved1, GetCurrentFishItemIndex >= 0);
		SendNetworkUpdate();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	private void UseFish(RPCMessage msg)
	{
		if (HasFlag(Flags.Reserved1) && !IsBusy())
		{
			Effect.server.Run(FishInteractSound.resourcePath, base.transform.position);
			SetFlag(Flags.Busy, b: true);
			Invoke(ClearBusy, UseCooldown);
			ClientRPC(null, "PlayAnimation");
		}
	}

	private void ClearBusy()
	{
		SetFlag(Flags.Busy, b: false);
	}
}
