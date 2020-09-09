#define UNITY_ASSERTIONS
using ConVar;
using Network;
using System;
using UnityEngine;
using UnityEngine.Assertions;

public class ElectricalBranch : IOEntity
{
	public int branchAmount = 2;

	public GameObjectRef branchPanelPrefab;

	private float nextChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("ElectricalBranch.OnRpcMessage"))
		{
			if (rpc == 643124146 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player + " - SetBranchOffPower ");
				}
				using (TimeWarning.New("SetBranchOffPower"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(643124146u, "SetBranchOffPower", this, player, 3f))
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
							RPCMessage branchOffPower = rPCMessage;
							SetBranchOffPower(branchOffPower);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetBranchOffPower");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SetBranchOffPower(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && player.CanBuild() && !(UnityEngine.Time.time < nextChangeTime))
		{
			nextChangeTime = UnityEngine.Time.time + 1f;
			int value = msg.read.Int32();
			value = (branchAmount = Mathf.Clamp(value, 2, 10000000));
			MarkDirtyForceUpdateOutputs();
			SendNetworkUpdate();
		}
	}

	public override bool AllowDrainFrom(int outputSlot)
	{
		if (outputSlot == 1)
		{
			return false;
		}
		return true;
	}

	public override int DesiredPower()
	{
		return branchAmount;
	}

	public void SetBranchAmount(int newAmount)
	{
		newAmount = Mathf.Clamp(newAmount, 2, 100000000);
		branchAmount = newAmount;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		switch (outputSlot)
		{
		case 0:
			return Mathf.Clamp(GetCurrentEnergy() - branchAmount, 0, GetCurrentEnergy());
		case 1:
			return Mathf.Min(GetCurrentEnergy(), branchAmount);
		default:
			return 0;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericInt1 = branchAmount;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			branchAmount = info.msg.ioEntity.genericInt1;
		}
	}
}
