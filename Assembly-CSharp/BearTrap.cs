#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class BearTrap : BaseTrap
{
	protected Animator animator;

	private GameObject hurtTarget;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BearTrap.OnRpcMessage"))
		{
			if (rpc == 547827602 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Arm "));
				}
				using (TimeWarning.New("RPC_Arm"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(547827602u, "RPC_Arm", this, player, 3f))
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
							RPC_Arm(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Arm");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool Armed()
	{
		return HasFlag(Flags.On);
	}

	public override void InitShared()
	{
		animator = GetComponent<Animator>();
		base.InitShared();
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player) && !Armed())
		{
			return player.CanBuild();
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		Arm();
	}

	public override void Arm()
	{
		base.Arm();
		RadialResetCorpses(120f);
	}

	public void Fire()
	{
		SetFlag(Flags.On, false);
		SendNetworkUpdate();
	}

	public override void ObjectEntered(GameObject obj)
	{
		if (Armed() && Interface.CallHook("OnTrapTrigger", this, obj) == null)
		{
			hurtTarget = obj;
			Invoke(DelayedFire, 0.05f);
		}
	}

	public void DelayedFire()
	{
		if ((bool)hurtTarget)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(hurtTarget);
			if (baseEntity != null)
			{
				HitInfo hitInfo = new HitInfo(this, baseEntity, DamageType.Bite, 50f, base.transform.position);
				hitInfo.damageTypes.Add(DamageType.Stab, 30f);
				baseEntity.OnAttacked(hitInfo);
			}
			hurtTarget = null;
		}
		RadialResetCorpses(1800f);
		Fire();
		Hurt(25f);
	}

	public void RadialResetCorpses(float duration)
	{
		List<BaseCorpse> obj = Facepunch.Pool.GetList<BaseCorpse>();
		Vis.Entities(base.transform.position, 5f, obj, 512);
		foreach (BaseCorpse item in obj)
		{
			item.ResetRemovalTime(duration);
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public override void OnAttacked(HitInfo info)
	{
		float num = info.damageTypes.Total();
		if ((info.damageTypes.IsMeleeType() && num > 20f) || num > 30f)
		{
			Fire();
		}
		base.OnAttacked(info);
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Arm(RPCMessage rpc)
	{
		if (!Armed() && Interface.CallHook("OnTrapArm", this, rpc.player) == null)
		{
			Arm();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!base.isServer && animator.isInitialized)
		{
			animator.SetBool("armed", Armed());
		}
	}
}
