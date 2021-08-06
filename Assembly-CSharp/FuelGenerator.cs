#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class FuelGenerator : ContainerIOEntity
{
	public int outputEnergy = 35;

	public float fuelPerSec = 1f;

	protected float fuelTickRate = 3f;

	private float pendingFuel;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("FuelGenerator.OnRpcMessage"))
		{
			if (rpc == 1401355317 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_EngineSwitch "));
				}
				using (TimeWarning.New("RPC_EngineSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1401355317u, "RPC_EngineSwitch", this, player, 3f))
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
							RPC_EngineSwitch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_EngineSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return outputEnergy;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void Init()
	{
		if (IsOn())
		{
			UpdateCurrentEnergy();
			InvokeRepeating(FuelConsumption, fuelTickRate, fuelTickRate);
		}
		base.Init();
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0 && inputAmount > 0)
		{
			TurnOn();
		}
		if (inputSlot == 1 && inputAmount > 0)
		{
			TurnOff();
		}
		base.UpdateFromInput(inputAmount, inputSlot);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		if (!IsOn())
		{
			return 0;
		}
		return outputEnergy;
	}

	public void UpdateCurrentEnergy()
	{
		currentEnergy = CalculateCurrentEnergy(0, 0);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_EngineSwitch(RPCMessage msg)
	{
		if (Interface.CallHook("OnSwitchToggle", this, msg.player) == null)
		{
			bool generatorState = msg.read.Bit();
			SetGeneratorState(generatorState);
			Interface.CallHook("OnSwitchToggled", this, msg.player);
		}
	}

	public void SetGeneratorState(bool wantsOn)
	{
		if (wantsOn)
		{
			TurnOn();
		}
		else
		{
			TurnOff();
		}
	}

	public int GetFuelAmount()
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return 0;
		}
		return slot.amount;
	}

	public bool HasFuel()
	{
		return GetFuelAmount() >= 1;
	}

	public bool UseFuel(float seconds)
	{
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || slot.amount < 1)
		{
			return false;
		}
		pendingFuel += seconds * fuelPerSec;
		if (pendingFuel >= 1f)
		{
			int num = Mathf.FloorToInt(pendingFuel);
			slot.UseItem(num);
			pendingFuel -= num;
		}
		return true;
	}

	public void TurnOn()
	{
		if (!IsOn() && UseFuel(1f))
		{
			SetFlag(Flags.On, true);
			UpdateCurrentEnergy();
			MarkDirty();
			InvokeRepeating(FuelConsumption, fuelTickRate, fuelTickRate);
		}
	}

	public void FuelConsumption()
	{
		if (!UseFuel(fuelTickRate))
		{
			TurnOff();
		}
	}

	public void TurnOff()
	{
		if (IsOn())
		{
			SetFlag(Flags.On, false);
			UpdateCurrentEnergy();
			MarkDirty();
			CancelInvoke(FuelConsumption);
		}
	}
}
