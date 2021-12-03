#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class DudTimedExplosive : TimedExplosive, IIgniteable, ISplashable
{
	public GameObjectRef fizzleEffect;

	public GameObject wickSpark;

	public AudioSource wickSound;

	public float dudChance = 0.3f;

	[ItemSelector(ItemCategory.All)]
	public ItemDefinition itemToGive;

	[NonSerialized]
	private float explodeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DudTimedExplosive.OnRpcMessage"))
		{
			if (rpc == 2436818324u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Pickup "));
				}
				using (TimeWarning.New("RPC_Pickup"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2436818324u, "RPC_Pickup", this, player, 3f))
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
							RPC_Pickup(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Pickup");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private bool IsWickBurning()
	{
		return HasFlag(Flags.On);
	}

	public override float GetRandomTimerTime()
	{
		float randomTimerTime = base.GetRandomTimerTime();
		float num = 1f;
		if (UnityEngine.Random.Range(0f, 1f) <= 0.15f)
		{
			num = 0.334f;
		}
		else if (UnityEngine.Random.Range(0f, 1f) <= 0.15f)
		{
			num = 3f;
		}
		return randomTimerTime * num;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_Pickup(RPCMessage msg)
	{
		if (!IsWickBurning())
		{
			BasePlayer player = msg.player;
			if (UnityEngine.Random.Range(0f, 1f) >= 0.5f && HasParent())
			{
				SetFuse(UnityEngine.Random.Range(2.5f, 3f));
				return;
			}
			player.GiveItem(ItemManager.Create(itemToGive, 1, skinID));
			Kill();
		}
	}

	public override void SetFuse(float fuseLength)
	{
		base.SetFuse(fuseLength);
		explodeTime = UnityEngine.Time.realtimeSinceStartup + fuseLength;
		SetFlag(Flags.On, true);
		SendNetworkUpdate();
		CancelInvoke(base.KillMessage);
	}

	public override void Explode()
	{
		if (creatorEntity != null && creatorEntity.IsNpc)
		{
			base.Explode();
		}
		else if (UnityEngine.Random.Range(0f, 1f) < dudChance && Interface.CallHook("OnExplosiveDud", this) == null)
		{
			BecomeDud();
		}
		else
		{
			base.Explode();
		}
	}

	public override bool CanStickTo(BaseEntity entity)
	{
		if (base.CanStickTo(entity))
		{
			return IsWickBurning();
		}
		return false;
	}

	public virtual void BecomeDud()
	{
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		bool flag = false;
		EntityRef entityRef = parentEntity;
		while (entityRef.IsValid(base.isServer) && !flag)
		{
			BaseEntity baseEntity = entityRef.Get(base.isServer);
			if (baseEntity.syncPosition)
			{
				flag = true;
			}
			entityRef = baseEntity.parentEntity;
		}
		if (flag)
		{
			SetParent(null);
		}
		base.transform.SetPositionAndRotation(position, rotation);
		SetFlag(Flags.On, false);
		SetCollisionEnabled(true);
		if (flag)
		{
			SetMotionEnabled(true);
		}
		Effect.server.Run("assets/bundled/prefabs/fx/impacts/blunt/concrete/concrete1.prefab", this, 0u, Vector3.zero, Vector3.zero);
		SendNetworkUpdate();
		CancelInvoke(base.KillMessage);
		Invoke(base.KillMessage, 1200f);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.dudExplosive = Facepunch.Pool.Get<DudExplosive>();
		info.msg.dudExplosive.fuseTimeLeft = explodeTime - UnityEngine.Time.realtimeSinceStartup;
	}

	public void Ignite()
	{
		SetFuse(GetRandomTimerTime());
		ReceiveCollisionMessages(true);
		if (waterCausesExplosion)
		{
			InvokeRepeating(base.WaterCheck, 0f, 0.5f);
		}
	}

	public bool CanIgnite()
	{
		return !HasFlag(Flags.On);
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (!base.IsDestroyed)
		{
			return HasFlag(Flags.On);
		}
		return false;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		BecomeDud();
		if (IsInvoking(Explode))
		{
			CancelInvoke(Explode);
		}
		return 1;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.dudExplosive != null)
		{
			explodeTime = UnityEngine.Time.realtimeSinceStartup + info.msg.dudExplosive.fuseTimeLeft;
		}
	}
}
