#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PowerCounter : IOEntity
{
	public int counterNumber;

	public int targetCounterNumber = 10;

	public Canvas canvas;

	public CanvasGroup screenAlpha;

	public Text screenText;

	public const Flags Flag_ShowPassthrough = Flags.Reserved2;

	public GameObjectRef counterConfigPanel;

	public Color passthroughColor;

	public Color counterColor;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PowerCounter.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 3554226761u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_SetTarget "));
				}
				using (TimeWarning.New("SERVER_SetTarget"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3554226761u, "SERVER_SetTarget", this, player, 3f))
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
							RPCMessage msg2 = rPCMessage;
							SERVER_SetTarget(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_SetTarget");
					}
				}
				return true;
			}
			if (rpc == 3222475159u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ToggleDisplayMode "));
				}
				using (TimeWarning.New("ToggleDisplayMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3222475159u, "ToggleDisplayMode", this, player, 3f))
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
							RPCMessage msg3 = rPCMessage;
							ToggleDisplayMode(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ToggleDisplayMode");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool DisplayPassthrough()
	{
		return HasFlag(Flags.Reserved2);
	}

	public bool DisplayCounter()
	{
		return !DisplayPassthrough();
	}

	public bool CanPlayerAdmin(BasePlayer player)
	{
		if (player != null)
		{
			return player.CanBuild();
		}
		return false;
	}

	public int GetTarget()
	{
		return targetCounterNumber;
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SERVER_SetTarget(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (Interface.CallHook("OnCounterTargetChange", this, msg.player, num) == null && CanPlayerAdmin(msg.player))
		{
			targetCounterNumber = num;
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ToggleDisplayMode(RPCMessage msg)
	{
		bool flag = msg.read.Bit();
		if (Interface.CallHook("OnCounterModeToggle", this, msg.player, flag) == null && msg.player.CanBuild())
		{
			SetFlag(Flags.Reserved2, flag, false, false);
			MarkDirty();
			SendNetworkUpdate();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (DisplayPassthrough() || counterNumber >= targetCounterNumber)
		{
			return base.GetPassthroughAmount(outputSlot);
		}
		return 0;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.ioEntity == null)
		{
			info.msg.ioEntity = Facepunch.Pool.Get<ProtoBuf.IOEntity>();
		}
		info.msg.ioEntity.genericInt1 = counterNumber;
		info.msg.ioEntity.genericInt2 = GetPassthroughAmount();
		info.msg.ioEntity.genericInt3 = GetTarget();
	}

	public void SetCounterNumber(int newNumber)
	{
		counterNumber = newNumber;
	}

	public override void SendIONetworkUpdate()
	{
		SendNetworkUpdate();
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
		}
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (DisplayCounter() && inputAmount > 0 && inputSlot != 0)
		{
			int num = counterNumber;
			switch (inputSlot)
			{
			case 1:
				counterNumber++;
				break;
			case 2:
				counterNumber--;
				if (counterNumber < 0)
				{
					counterNumber = 0;
				}
				break;
			case 3:
				counterNumber = 0;
				break;
			}
			counterNumber = Mathf.Clamp(counterNumber, 0, 100);
			if (num != counterNumber)
			{
				MarkDirty();
				SendNetworkUpdate();
			}
		}
		if (inputSlot == 0)
		{
			base.UpdateFromInput(inputAmount, inputSlot);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			if (base.isServer)
			{
				counterNumber = info.msg.ioEntity.genericInt1;
			}
			targetCounterNumber = info.msg.ioEntity.genericInt3;
		}
	}
}
