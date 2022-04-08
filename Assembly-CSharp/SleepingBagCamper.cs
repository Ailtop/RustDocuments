#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SleepingBagCamper : SleepingBag
{
	public EntityRef<BaseVehicleSeat> AssociatedSeat;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SleepingBagCamper.OnRpcMessage"))
		{
			if (rpc == 2177887503u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerClearBed "));
				}
				using (TimeWarning.New("ServerClearBed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2177887503u, "ServerClearBed", this, player, 3f))
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
							ServerClearBed(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerClearBed");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetFlag(Flags.Reserved3, b: true);
	}

	protected override void PostPlayerSpawn(BasePlayer p)
	{
		base.PostPlayerSpawn(p);
		BaseVehicleSeat baseVehicleSeat = AssociatedSeat.Get(base.isServer);
		if (baseVehicleSeat != null)
		{
			p.EndSleeping();
			baseVehicleSeat.MountPlayer(p);
		}
	}

	public void SetSeat(BaseVehicleSeat seat, bool sendNetworkUpdate = false)
	{
		AssociatedSeat.Set(seat);
		if (sendNetworkUpdate)
		{
			SendNetworkUpdate();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.sleepingBagCamper = Facepunch.Pool.Get<ProtoBuf.SleepingBagCamper>();
			info.msg.sleepingBagCamper.seatID = AssociatedSeat.uid;
		}
	}

	public override bool IsOccupied()
	{
		if (!AssociatedSeat.IsValid(base.isServer) || !AssociatedSeat.Get(base.isServer).IsMounted())
		{
			return WaterLevel.Test(base.transform.position);
		}
		return true;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void ServerClearBed(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && AssociatedSeat.IsValid(base.isServer) && !(AssociatedSeat.Get(base.isServer).GetMounted() != player))
		{
			deployerUserID = 0uL;
			SendNetworkUpdate();
		}
	}
}
