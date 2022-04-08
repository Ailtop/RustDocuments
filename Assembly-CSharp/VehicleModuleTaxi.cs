#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class VehicleModuleTaxi : VehicleModuleStorage
{
	[Header("Taxi")]
	[SerializeField]
	private SoundDefinition kickButtonSound;

	[SerializeField]
	private SphereCollider kickButtonCollider;

	[SerializeField]
	private float maxKickVelocity = 4f;

	private Vector3 KickButtonPos => kickButtonCollider.transform.position + kickButtonCollider.transform.rotation * kickButtonCollider.center;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("VehicleModuleTaxi.OnRpcMessage"))
		{
			if (rpc == 2714639811u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_KickPassengers "));
				}
				using (TimeWarning.New("RPC_KickPassengers"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2714639811u, "RPC_KickPassengers", this, player, 3f))
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
							RPC_KickPassengers(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_KickPassengers");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool CanKickPassengers(BasePlayer player)
	{
		if (!base.IsOnAVehicle)
		{
			return false;
		}
		if (base.Vehicle.GetSpeed() > maxKickVelocity)
		{
			return false;
		}
		if (player == null)
		{
			return false;
		}
		if (!base.Vehicle.PlayerIsMounted(player))
		{
			return false;
		}
		Vector3 lhs = KickButtonPos - player.transform.position;
		if (Vector3.Dot(lhs, player.transform.forward) < 0f)
		{
			return lhs.sqrMagnitude < 4f;
		}
		return false;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_KickPassengers(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && CanKickPassengers(player))
		{
			KickPassengers();
		}
	}

	private void KickPassengers()
	{
		if (!base.IsOnAVehicle)
		{
			return;
		}
		foreach (MountPointInfo mountPoint in mountPoints)
		{
			BaseMountable mountable = mountPoint.mountable;
			BasePlayer mounted = mountable.GetMounted();
			if (mounted != null && mountable.HasValidDismountPosition(mounted))
			{
				mountable.AttemptDismount(mounted);
			}
		}
	}
}
