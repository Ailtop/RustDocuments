#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class WaterWell : LiquidContainer
{
	public Animator animator;

	private const Flags Pumping = Flags.Reserved2;

	private const Flags WaterFlow = Flags.Reserved3;

	public float caloriesPerPump = 5f;

	public float pressurePerPump = 0.2f;

	public float pressureForProduction = 1f;

	public float currentPressure;

	public int waterPerPump = 50;

	public GameObject waterLevelObj;

	public float waterLevelObjFullOffset;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WaterWell.OnRpcMessage"))
		{
			if (rpc == 2538739344u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Pump "));
				}
				using (TimeWarning.New("RPC_Pump"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2538739344u, "RPC_Pump", this, player, 3f))
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
							RPC_Pump(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Pump");
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
		SetFlag(Flags.Reserved2, b: false);
		SetFlag(Flags.Reserved3, b: false);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_Pump(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && !player.IsDead() && !player.IsSleeping() && !(player.metabolism.calories.value < caloriesPerPump) && !HasFlag(Flags.Reserved2))
		{
			SetFlag(Flags.Reserved2, b: true);
			player.metabolism.calories.value -= caloriesPerPump;
			player.metabolism.SendChangesToClient();
			currentPressure = Mathf.Clamp01(currentPressure + pressurePerPump);
			Invoke(StopPump, 1.8f);
			if (currentPressure >= 0f)
			{
				CancelInvoke(Produce);
				Invoke(Produce, 1f);
			}
			SendNetworkUpdateImmediate();
		}
	}

	public void StopPump()
	{
		SetFlag(Flags.Reserved2, b: false);
		SendNetworkUpdateImmediate();
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		SendNetworkUpdate();
	}

	public void Produce()
	{
		base.inventory.AddItem(defaultLiquid, waterPerPump, 0uL);
		SetFlag(Flags.Reserved3, b: true);
		ScheduleTapOff();
		SendNetworkUpdateImmediate();
	}

	public void ScheduleTapOff()
	{
		CancelInvoke(TapOff);
		Invoke(TapOff, 1f);
	}

	private void TapOff()
	{
		SetFlag(Flags.Reserved3, b: false);
	}

	public void ReducePressure()
	{
		float num = UnityEngine.Random.Range(0.1f, 0.2f);
		currentPressure = Mathf.Clamp01(currentPressure - num);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.waterwell = Facepunch.Pool.Get<ProtoBuf.WaterWell>();
		info.msg.waterwell.pressure = currentPressure;
		info.msg.waterwell.waterLevel = GetWaterAmount();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.waterwell != null)
		{
			currentPressure = info.msg.waterwell.pressure;
		}
	}

	public float GetWaterAmount()
	{
		if (base.isServer)
		{
			Item slot = base.inventory.GetSlot(0);
			if (slot == null)
			{
				return 0f;
			}
			return slot.amount;
		}
		return 0f;
	}
}
