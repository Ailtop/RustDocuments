#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SprayCanSpray : DecayEntity, ISplashable
{
	public DateTime sprayTimestamp;

	public ulong sprayedByPlayer;

	public static ListHashSet<SprayCanSpray> AllSprays = new ListHashSet<SprayCanSpray>();

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SprayCanSpray.OnRpcMessage"))
		{
			if (rpc == 2774110739u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_RequestWaterClear "));
				}
				using (TimeWarning.New("Server_RequestWaterClear"))
				{
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							Server_RequestWaterClear(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_RequestWaterClear");
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
		if (info.msg.spray == null)
		{
			info.msg.spray = Facepunch.Pool.Get<Spray>();
		}
		info.msg.spray.sprayedBy = sprayedByPlayer;
		info.msg.spray.timestamp = sprayTimestamp.ToBinary();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.spray != null)
		{
			sprayedByPlayer = info.msg.spray.sprayedBy;
			sprayTimestamp = DateTime.FromBinary(info.msg.spray.timestamp);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		sprayTimestamp = DateTime.Now;
		sprayedByPlayer = deployedBy.userID;
		if (ConVar.Global.MaxSpraysPerPlayer > 0 && sprayedByPlayer != 0L)
		{
			int num = -1;
			DateTime now = DateTime.Now;
			int num2 = 0;
			for (int i = 0; i < AllSprays.Count; i++)
			{
				if (AllSprays[i].sprayedByPlayer == sprayedByPlayer)
				{
					num2++;
					if (num == -1 || AllSprays[i].sprayTimestamp < now)
					{
						num = i;
						now = AllSprays[i].sprayTimestamp;
					}
				}
			}
			if (num2 >= ConVar.Global.MaxSpraysPerPlayer && num != -1)
			{
				AllSprays[num].Kill();
			}
		}
		if (deployedBy == null || !deployedBy.IsBuildingAuthed())
		{
			Invoke(ApplyOutOfAuthConditionPenalty, 1f);
		}
	}

	private void ApplyOutOfAuthConditionPenalty()
	{
		if (!IsFullySpawned())
		{
			Invoke(ApplyOutOfAuthConditionPenalty, 1f);
			return;
		}
		float amount = MaxHealth() * (1f - ConVar.Global.SprayOutOfAuthMultiplier);
		Hurt(amount, DamageType.Decay);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InvokeRandomized(RainCheck, 60f, 180f, 30f);
		if (!AllSprays.Contains(this))
		{
			AllSprays.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (AllSprays.Contains(this))
		{
			AllSprays.Remove(this);
		}
	}

	private void RainCheck()
	{
		if (Climate.GetRain(base.transform.position) > 0f && IsOutside())
		{
			Kill();
		}
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		return amount > 0;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed)
		{
			Kill();
		}
		return 1;
	}

	[RPC_Server]
	private void Server_RequestWaterClear(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && Menu_WaterClear_ShowIf(player))
		{
			Kill();
		}
	}

	private bool Menu_WaterClear_ShowIf(BasePlayer player)
	{
		if (player.GetHeldEntity() != null && player.GetHeldEntity() is BaseLiquidVessel baseLiquidVessel)
		{
			return baseLiquidVessel.AmountHeld() > 0;
		}
		return false;
	}
}
