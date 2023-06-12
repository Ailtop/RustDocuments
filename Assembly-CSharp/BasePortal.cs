#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class BasePortal : BaseCombatEntity
{
	public bool isUsablePortal = true;

	private Vector3 destination_pos;

	private Quaternion destination_rot;

	public BasePortal targetPortal;

	public NetworkableId targetID;

	public Transform localEntryExitPos;

	public Transform relativeAnchor;

	public bool isMirrored = true;

	public GameObjectRef appearEffect;

	public GameObjectRef disappearEffect;

	public GameObjectRef transitionSoundEffect;

	public string useTagString = "";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BasePortal.OnRpcMessage"))
		{
			if (rpc == 561762999 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_UsePortal "));
				}
				using (TimeWarning.New("RPC_UsePortal"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(561762999u, "RPC_UsePortal", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(561762999u, "RPC_UsePortal", this, player, 3f))
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
							RPC_UsePortal(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_UsePortal");
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
		info.msg.ioEntity = Facepunch.Pool.Get<ProtoBuf.IOEntity>();
		info.msg.ioEntity.genericEntRef1 = targetID;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			targetID = info.msg.ioEntity.genericEntRef1;
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
	}

	public void LinkPortal()
	{
		if (targetPortal != null)
		{
			targetID = targetPortal.net.ID;
		}
		if (targetPortal == null && targetID.IsValid)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(targetID);
			if (baseNetworkable != null)
			{
				targetPortal = baseNetworkable.GetComponent<BasePortal>();
			}
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Debug.Log("Post server load");
		LinkPortal();
	}

	public void SetDestination(Vector3 destPos, Quaternion destRot)
	{
		destination_pos = destPos;
		destination_rot = destRot;
	}

	public Vector3 GetLocalEntryExitPosition()
	{
		return localEntryExitPos.transform.position;
	}

	public Quaternion GetLocalEntryExitRotation()
	{
		return localEntryExitPos.transform.rotation;
	}

	public BasePortal GetPortal()
	{
		LinkPortal();
		return targetPortal;
	}

	public virtual void UsePortal(BasePlayer player)
	{
		if (Interface.CallHook("OnPortalUse", player, this) != null)
		{
			return;
		}
		LinkPortal();
		if (targetPortal != null)
		{
			player.PauseFlyHackDetection();
			player.PauseSpeedHackDetection();
			Vector3 position = player.transform.position;
			Vector3 vector = targetPortal.GetLocalEntryExitPosition();
			Vector3 vector2 = base.transform.InverseTransformDirection(player.eyes.BodyForward());
			Vector3 vector3 = vector2;
			if (isMirrored)
			{
				Vector3 position2 = base.transform.InverseTransformPoint(player.transform.position);
				vector = targetPortal.relativeAnchor.transform.TransformPoint(position2);
				vector3 = targetPortal.relativeAnchor.transform.TransformDirection(vector2);
			}
			else
			{
				vector3 = targetPortal.GetLocalEntryExitRotation() * Vector3.forward;
			}
			if (disappearEffect.isValid)
			{
				Effect.server.Run(disappearEffect.resourcePath, position, Vector3.up);
			}
			if (appearEffect.isValid)
			{
				Effect.server.Run(appearEffect.resourcePath, vector, Vector3.up);
			}
			player.SetParent(null, worldPositionStays: true);
			player.Teleport(vector);
			player.ForceUpdateTriggers();
			player.ClientRPCPlayer(null, player, "ForceViewAnglesTo", vector3);
			if (transitionSoundEffect.isValid)
			{
				Effect.server.Run(transitionSoundEffect.resourcePath, targetPortal.relativeAnchor.transform.position, Vector3.up);
			}
			player.UpdateNetworkGroup();
			player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, b: true);
			SendNetworkUpdateImmediate();
			player.ClientRPCPlayer(null, player, "StartLoading_Quick", arg1: true);
			Interface.CallHook("OnPortalUsed", player, this);
		}
		else
		{
			Debug.Log("No portal...");
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void RPC_UsePortal(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (IsActive())
		{
			UsePortal(player);
		}
	}

	public bool IsActive()
	{
		return true;
	}
}
