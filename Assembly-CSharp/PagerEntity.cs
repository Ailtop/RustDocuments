#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PagerEntity : BaseEntity, IRFObject
{
	public static Flags Flag_Silent = Flags.Reserved1;

	private int frequency = 55;

	public float beepRepeat = 2f;

	public GameObjectRef pagerEffect;

	public GameObjectRef silentEffect;

	private float nextChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PagerEntity.OnRpcMessage"))
		{
			if (rpc == 2778616053u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerSetFrequency "));
				}
				using (TimeWarning.New("ServerSetFrequency"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2778616053u, "ServerSetFrequency", this, player, 3f))
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
							ServerSetFrequency(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerSetFrequency");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetFrequency()
	{
		return frequency;
	}

	public override void SwitchParent(BaseEntity ent)
	{
		SetParent(ent, false, true);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		RFManager.AddListener(frequency, this);
	}

	internal override void DoServerDestroy()
	{
		RFManager.RemoveListener(frequency, this);
		base.DoServerDestroy();
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	public float GetMaxRange()
	{
		return float.PositiveInfinity;
	}

	public void RFSignalUpdate(bool on)
	{
		if (!base.IsDestroyed)
		{
			bool flag = IsOn();
			if (on != flag)
			{
				SetFlag(Flags.On, on);
				SendNetworkUpdate();
			}
		}
	}

	public void SetSilentMode(bool wantsSilent)
	{
		SetFlag(Flag_Silent, wantsSilent);
	}

	public void SetOff()
	{
		SetFlag(Flags.On, false);
	}

	public void ChangeFrequency(int newFreq)
	{
		RFManager.ChangeFrequency(frequency, newFreq, this, true);
		frequency = newFreq;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ServerSetFrequency(RPCMessage msg)
	{
		if (!(msg.player == null) && msg.player.CanBuild() && !(UnityEngine.Time.time < nextChangeTime))
		{
			nextChangeTime = UnityEngine.Time.time + 2f;
			int num = msg.read.Int32();
			if (Interface.CallHook("OnRfFrequencyChange", this, num, msg.player) == null)
			{
				RFManager.ChangeFrequency(frequency, num, this, true);
				frequency = num;
				SendNetworkUpdateImmediate();
				Interface.CallHook("OnRfFrequencyChanged", this, num, msg.player);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.ioEntity = Facepunch.Pool.Get<ProtoBuf.IOEntity>();
		info.msg.ioEntity.genericInt1 = frequency;
	}

	internal override void OnParentRemoved()
	{
		SetParent(null, false, true);
	}

	public void OnParentDestroying()
	{
		if (base.isServer)
		{
			base.transform.parent = null;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.ioEntity != null)
		{
			frequency = info.msg.ioEntity.genericInt1;
		}
		if (base.isServer && info.fromDisk)
		{
			ChangeFrequency(frequency);
		}
	}
}
