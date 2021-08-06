#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class PressButton : IOEntity
{
	public float pressDuration = 5f;

	public float pressPowerTime = 0.5f;

	public int pressPowerAmount = 2;

	public const Flags Flag_EmittingPower = Flags.Reserved3;

	public bool smallBurst;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PressButton.OnRpcMessage"))
		{
			if (rpc == 3778543711u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Press "));
				}
				using (TimeWarning.New("Press"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3778543711u, "Press", this, player, 3f))
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
							Press(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Press");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetIOState()
	{
		base.ResetIOState();
		SetFlag(Flags.On, false);
		SetFlag(Flags.Reserved3, false);
		CancelInvoke(Unpress);
		CancelInvoke(UnpowerTime);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (IsOn())
		{
			if (HasFlag(Flags.Reserved3) && (sourceItem != null || smallBurst))
			{
				return pressPowerAmount;
			}
			return base.GetPassthroughAmount();
		}
		return 0;
	}

	public void UnpowerTime()
	{
		SetFlag(Flags.Reserved3, false);
		MarkDirty();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetFlag(Flags.On, false);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void Press(RPCMessage msg)
	{
		if (!IsOn() && Interface.CallHook("OnButtonPress", this, msg.player) == null)
		{
			SetFlag(Flags.On, true);
			Invoke(UnpowerTime, pressPowerTime);
			SetFlag(Flags.Reserved3, true);
			SendNetworkUpdateImmediate();
			MarkDirty();
			Invoke(Unpress, pressDuration);
		}
	}

	public void Unpress()
	{
		SetFlag(Flags.On, false);
		MarkDirty();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity.genericFloat1 = pressDuration;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			pressDuration = info.msg.ioEntity.genericFloat1;
		}
	}
}
