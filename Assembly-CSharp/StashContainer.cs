#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class StashContainer : StorageContainer
{
	public static class StashContainerFlags
	{
		public const Flags Hidden = Flags.Reserved5;
	}

	public Transform visuals;

	public float burriedOffset;

	public float raisedOffset;

	public GameObjectRef buryEffect;

	public float uncoverRange = 3f;

	private float lastToggleTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("StashContainer.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 4130263076u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_HideStash "));
				}
				using (TimeWarning.New("RPC_HideStash"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4130263076u, "RPC_HideStash", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_HideStash(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_HideStash");
					}
				}
				return true;
			}
			if (rpc == 298671803 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_WantsUnhide "));
				}
				using (TimeWarning.New("RPC_WantsUnhide"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(298671803u, "RPC_WantsUnhide", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							RPC_WantsUnhide(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_WantsUnhide");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsHidden()
	{
		return HasFlag(Flags.Reserved5);
	}

	public bool PlayerInRange(BasePlayer ply)
	{
		if (Vector3.Distance(base.transform.position, ply.transform.position) <= uncoverRange)
		{
			Vector3 normalized = (base.transform.position - ply.eyes.position).normalized;
			if (Vector3.Dot(ply.eyes.BodyForward(), normalized) > 0.95f)
			{
				return true;
			}
		}
		return false;
	}

	public void DoOccludedCheck()
	{
		if (UnityEngine.Physics.SphereCast(new Ray(base.transform.position + Vector3.up * 5f, Vector3.down), 0.25f, 5f, 2097152))
		{
			DropItems();
			Kill();
		}
	}

	public void OnPhysicsNeighbourChanged()
	{
		if (!IsInvoking(DoOccludedCheck))
		{
			Invoke(DoOccludedCheck, UnityEngine.Random.Range(5f, 10f));
		}
	}

	public void SetHidden(bool isHidden)
	{
		if (!(UnityEngine.Time.realtimeSinceStartup - lastToggleTime < 3f) && isHidden != HasFlag(Flags.Reserved5))
		{
			lastToggleTime = UnityEngine.Time.realtimeSinceStartup;
			Invoke(Decay, 259200f);
			if (base.isServer)
			{
				SetFlag(Flags.Reserved5, isHidden);
			}
		}
	}

	public void DisableNetworking()
	{
		base.limitNetworking = true;
		SetFlag(Flags.Disabled, true);
	}

	public void Decay()
	{
		Kill();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetHidden(false);
	}

	public void ToggleHidden()
	{
		SetHidden(!IsHidden());
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_HideStash(RPCMessage rpc)
	{
		if (Interface.CallHook("CanHideStash", rpc.player, this) == null)
		{
			SetHidden(true);
			Interface.CallHook("OnStashHidden", this, rpc.player);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_WantsUnhide(RPCMessage rpc)
	{
		if (IsHidden())
		{
			BasePlayer player = rpc.player;
			if (PlayerInRange(player) && Interface.CallHook("CanSeeStash", rpc.player, this) == null)
			{
				SetHidden(false);
				Interface.CallHook("OnStashExposed", this, rpc.player);
			}
		}
	}
}
