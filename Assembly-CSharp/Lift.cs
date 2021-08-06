#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Lift : AnimatedBuildingBlock
{
	public GameObjectRef triggerPrefab;

	public string triggerBone;

	public float resetDelay = 5f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Lift.OnRpcMessage"))
		{
			if (rpc == 2657791441u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_UseLift "));
				}
				using (TimeWarning.New("RPC_UseLift"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2657791441u, "RPC_UseLift", this, player, 3f))
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
							RPC_UseLift(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_UseLift");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_UseLift(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && Interface.CallHook("OnLiftUse", this, rpc.player) == null)
		{
			MoveUp();
		}
	}

	private void MoveUp()
	{
		if (!IsOpen() && !IsBusy())
		{
			SetFlag(Flags.Open, true);
			SendNetworkUpdateImmediate();
		}
	}

	private void MoveDown()
	{
		if (IsOpen() && !IsBusy())
		{
			SetFlag(Flags.Open, false);
			SendNetworkUpdateImmediate();
		}
	}

	protected override void OnAnimatorDisabled()
	{
		if (base.isServer && IsOpen())
		{
			Invoke(MoveDown, resetDelay);
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		if (!Rust.Application.isLoadingSave)
		{
			BaseEntity baseEntity = GameManager.server.CreateEntity(triggerPrefab.resourcePath, Vector3.zero, Quaternion.identity);
			baseEntity.Spawn();
			baseEntity.SetParent(this, triggerBone);
		}
	}
}
