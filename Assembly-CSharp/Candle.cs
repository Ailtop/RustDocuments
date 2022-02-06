#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Candle : BaseCombatEntity, ISplashable, IIgniteable
{
	public float lifeTimeSeconds = 7200f;

	public float burnRate = 10f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Candle.OnRpcMessage"))
		{
			if (rpc == 2523893445u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SetWantsOn "));
				}
				using (TimeWarning.New("SetWantsOn"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2523893445u, "SetWantsOn", this, player, 3f))
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
							RPCMessage wantsOn = rPCMessage;
							SetWantsOn(wantsOn);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SetWantsOn");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetWantsOn(RPCMessage msg)
	{
		bool b = msg.read.Bit();
		SetFlag(Flags.On, b);
		UpdateInvokes();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateInvokes();
	}

	public void UpdateInvokes()
	{
		if (IsOn())
		{
			InvokeRandomized(Burn, burnRate, burnRate, 1f);
		}
		else
		{
			CancelInvoke(Burn);
		}
	}

	public void Burn()
	{
		float num = burnRate / lifeTimeSeconds;
		Hurt(num * MaxHealth(), DamageType.Decay, this, false);
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer && info.damageTypes.Get(DamageType.Heat) > 0f && !IsOn())
		{
			SetFlag(Flags.On, true);
			UpdateInvokes();
		}
		base.OnAttacked(info);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed && amount > 1)
		{
			return IsOn();
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if (amount > 1)
		{
			SetFlag(Flags.On, false);
			UpdateInvokes();
			amount--;
		}
		return amount;
	}

	public void Ignite(Vector3 fromPos)
	{
		SetFlag(Flags.On, true);
		UpdateInvokes();
	}

	public bool CanIgnite()
	{
		return !IsOn();
	}
}
