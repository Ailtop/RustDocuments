#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class DeployableBoomBox : ContainerIOEntity, ICassettePlayer, IAudioConnectionSource
{
	public BoomBox BoxController;

	public int PowerUsageWhilePlaying = 10;

	public const int MaxBacktrackHopsClient = 30;

	public bool IsStatic;

	public BaseEntity ToBaseEntity => this;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DeployableBoomBox.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1918716764 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_UpdateRadioIP "));
				}
				using (TimeWarning.New("Server_UpdateRadioIP"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1918716764u, "Server_UpdateRadioIP", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1918716764u, "Server_UpdateRadioIP", this, player, 3f))
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
							Server_UpdateRadioIP(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_UpdateRadioIP");
					}
				}
				return true;
			}
			if (rpc == 1785864031 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerTogglePlay "));
				}
				using (TimeWarning.New("ServerTogglePlay"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1785864031u, "ServerTogglePlay", this, player, 3f))
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
							ServerTogglePlay(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ServerTogglePlay");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public IOEntity ToEntity()
	{
		return this;
	}

	public override int ConsumptionAmount()
	{
		if (!IsOn())
		{
			return 0;
		}
		return PowerUsageWhilePlaying;
	}

	public override int DesiredPower()
	{
		if (!IsOn())
		{
			return 0;
		}
		return PowerUsageWhilePlaying;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		if (inputSlot == 0)
		{
			base.UpdateHasPower(inputAmount, inputSlot);
			if (!IsPowered() && IsOn())
			{
				BoxController.ServerTogglePlay(false);
			}
		}
		else if (IsPowered() && !IsConnectedToAnySlot(this, inputSlot, IOEntity.backtracking))
		{
			BoxController.ServerTogglePlay(inputAmount > 0);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = ItemFilter;
		BoxController.HurtCallback = HurtCallback;
		if (IsStatic)
		{
			SetFlag(Flags.Reserved8, true);
		}
	}

	public bool ItemFilter(Item item, int count)
	{
		ItemDefinition[] validCassettes = BoxController.ValidCassettes;
		for (int i = 0; i < validCassettes.Length; i++)
		{
			if (validCassettes[i] == item.info)
			{
				return true;
			}
		}
		return false;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (!IsOn())
		{
			return 0;
		}
		return base.GetPassthroughAmount(outputSlot);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		if (inputSlot != 0)
		{
			return currentEnergy;
		}
		return base.CalculateCurrentEnergy(inputAmount, inputSlot);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ServerTogglePlay(RPCMessage msg)
	{
		BoxController.ServerTogglePlay(msg);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.IsVisible(3f)]
	public void Server_UpdateRadioIP(RPCMessage msg)
	{
		BoxController.Server_UpdateRadioIP(msg);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		BoxController.Save(info);
	}

	public bool ClearRadioByUserId(ulong id)
	{
		return BoxController.ClearRadioByUserId(id);
	}

	public void OnCassetteInserted(Cassette c)
	{
		BoxController.OnCassetteInserted(c);
	}

	public void OnCassetteRemoved(Cassette c)
	{
		BoxController.OnCassetteRemoved(c);
	}

	public void HurtCallback(float amount)
	{
		Hurt(amount, DamageType.Decay);
	}

	public override void Load(LoadInfo info)
	{
		BoxController.Load(info);
		base.Load(info);
		if (base.isServer && IsStatic)
		{
			SetFlag(Flags.Reserved8, true);
		}
	}
}
