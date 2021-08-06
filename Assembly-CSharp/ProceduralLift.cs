#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class ProceduralLift : BaseEntity
{
	public float movementSpeed = 1f;

	public float resetDelay = 5f;

	public ProceduralLiftCabin cabin;

	public ProceduralLiftStop[] stops;

	public GameObjectRef triggerPrefab;

	public string triggerBone;

	private int floorIndex = -1;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ProceduralLift.OnRpcMessage"))
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

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_UseLift(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && Interface.CallHook("OnLiftUse", this, rpc.player) == null && !IsBusy())
		{
			MoveToFloor((floorIndex + 1) % stops.Length);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SnapToFloor(0);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lift = Facepunch.Pool.Get<ProtoBuf.Lift>();
		info.msg.lift.floor = floorIndex;
	}

	public override void Load(LoadInfo info)
	{
		if (info.msg.lift != null)
		{
			if (floorIndex == -1)
			{
				SnapToFloor(info.msg.lift.floor);
			}
			else
			{
				MoveToFloor(info.msg.lift.floor);
			}
		}
		base.Load(info);
	}

	private void ResetLift()
	{
		MoveToFloor(0);
	}

	private void MoveToFloor(int floor)
	{
		floorIndex = Mathf.Clamp(floor, 0, stops.Length - 1);
		if (base.isServer)
		{
			SetFlag(Flags.Busy, true);
			SendNetworkUpdateImmediate();
			CancelInvoke(ResetLift);
		}
	}

	private void SnapToFloor(int floor)
	{
		floorIndex = Mathf.Clamp(floor, 0, stops.Length - 1);
		ProceduralLiftStop proceduralLiftStop = stops[floorIndex];
		cabin.transform.position = proceduralLiftStop.transform.position;
		if (base.isServer)
		{
			SetFlag(Flags.Busy, false);
			SendNetworkUpdateImmediate();
			CancelInvoke(ResetLift);
		}
	}

	private void OnFinishedMoving()
	{
		if (base.isServer)
		{
			SetFlag(Flags.Busy, false);
			SendNetworkUpdateImmediate();
			if (floorIndex != 0)
			{
				Invoke(ResetLift, resetDelay);
			}
		}
	}

	protected void Update()
	{
		if (floorIndex < 0 || floorIndex > stops.Length - 1)
		{
			return;
		}
		ProceduralLiftStop proceduralLiftStop = stops[floorIndex];
		if (!(cabin.transform.position == proceduralLiftStop.transform.position))
		{
			cabin.transform.position = Vector3.MoveTowards(cabin.transform.position, proceduralLiftStop.transform.position, movementSpeed * UnityEngine.Time.deltaTime);
			if (cabin.transform.position == proceduralLiftStop.transform.position)
			{
				OnFinishedMoving();
			}
		}
	}
}
