#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Landmine : BaseTrap
{
	public GameObjectRef explosionEffect;

	public GameObjectRef triggeredEffect;

	public float minExplosionRadius;

	public float explosionRadius;

	public bool blocked;

	private ulong triggerPlayerID;

	public List<DamageTypeEntry> damageTypes = new List<DamageTypeEntry>();

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Landmine.OnRpcMessage"))
		{
			if (rpc == 1552281787 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Disarm "));
				}
				using (TimeWarning.New("RPC_Disarm"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1552281787u, "RPC_Disarm", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_Disarm(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Disarm");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool Triggered()
	{
		return HasFlag(Flags.Open);
	}

	public bool Armed()
	{
		return HasFlag(Flags.On);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.landmine = Facepunch.Pool.Get<ProtoBuf.Landmine>();
			info.msg.landmine.triggeredID = triggerPlayerID;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk && info.msg.landmine != null)
		{
			triggerPlayerID = info.msg.landmine.triggeredID;
		}
	}

	public override void ServerInit()
	{
		SetFlag(Flags.On, false);
		Invoke(Arm, 1.5f);
		base.ServerInit();
	}

	public override void ObjectEntered(GameObject obj)
	{
		if (!base.isClient)
		{
			if (!Armed())
			{
				CancelInvoke(Arm);
				blocked = true;
			}
			else if (Interface.CallHook("OnTrapTrigger", this, obj) == null)
			{
				BasePlayer ply = GameObjectEx.ToBaseEntity(obj) as BasePlayer;
				Trigger(ply);
			}
		}
	}

	public void Trigger(BasePlayer ply = null)
	{
		if ((bool)ply)
		{
			triggerPlayerID = ply.userID;
		}
		SetFlag(Flags.Open, true);
		SendNetworkUpdate();
	}

	public override void OnEmpty()
	{
		if (blocked)
		{
			Arm();
			blocked = false;
		}
		else if (Triggered())
		{
			Invoke(TryExplode, 0.05f);
		}
	}

	public virtual void Explode()
	{
		base.health = float.PositiveInfinity;
		Effect.server.Run(explosionEffect.resourcePath, PivotPoint(), base.transform.up, null, true);
		DamageUtil.RadiusDamage(this, LookupPrefab(), CenterPoint(), minExplosionRadius, explosionRadius, damageTypes, 2263296, true);
		if (!base.IsDestroyed)
		{
			Kill();
		}
	}

	public override void OnKilled(HitInfo info)
	{
		Invoke(Explode, UnityEngine.Random.Range(0.1f, 0.3f));
	}

	private void OnGroundMissing()
	{
		Explode();
	}

	private void TryExplode()
	{
		if (Armed())
		{
			Explode();
		}
	}

	public override void Arm()
	{
		SetFlag(Flags.On, true);
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Disarm(RPCMessage rpc)
	{
		if (rpc.player.net.ID != triggerPlayerID && Armed() && Interface.CallHook("OnTrapDisarm", this, rpc.player) == null)
		{
			SetFlag(Flags.On, false);
			if (UnityEngine.Random.Range(0, 100) < 15)
			{
				Invoke(TryExplode, 0.05f);
				return;
			}
			rpc.player.GiveItem(ItemManager.CreateByName("trap.landmine", 1, 0uL), GiveItemReason.PickedUp);
			Kill();
		}
	}
}
