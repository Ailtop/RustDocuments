#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SpinnerWheel : Signage
{
	public Transform wheel;

	public float velocity;

	public Quaternion targetRotation = Quaternion.identity;

	[Header("Sound")]
	public SoundDefinition spinLoopSoundDef;

	public SoundDefinition spinStartSoundDef;

	public SoundDefinition spinAccentSoundDef;

	public SoundDefinition spinStopSoundDef;

	public float minTimeBetweenSpinAccentSounds = 0.3f;

	public float spinAccentAngleDelta = 180f;

	private Sound spinSound;

	private SoundModulation.Modulator spinSoundGain;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SpinnerWheel.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 3019675107u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_AnyoneSpin "));
				}
				using (TimeWarning.New("RPC_AnyoneSpin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3019675107u, "RPC_AnyoneSpin", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_AnyoneSpin(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_AnyoneSpin");
					}
				}
				return true;
			}
			if (rpc == 1455840454 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Spin "));
				}
				using (TimeWarning.New("RPC_Spin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1455840454u, "RPC_Spin", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							RPC_Spin(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Spin");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual bool AllowPlayerSpins()
	{
		return true;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.spinnerWheel = Facepunch.Pool.Get<ProtoBuf.SpinnerWheel>();
		info.msg.spinnerWheel.spin = wheel.rotation.eulerAngles;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.spinnerWheel != null)
		{
			Quaternion rotation = Quaternion.Euler(info.msg.spinnerWheel.spin);
			if (base.isServer)
			{
				wheel.transform.rotation = rotation;
			}
		}
	}

	public virtual float GetMaxSpinSpeed()
	{
		return 720f;
	}

	public virtual void Update_Server()
	{
		if (velocity > 0f)
		{
			float num = Mathf.Clamp(GetMaxSpinSpeed() * velocity, 0f, GetMaxSpinSpeed());
			velocity -= UnityEngine.Time.deltaTime * Mathf.Clamp(velocity / 2f, 0.1f, 1f);
			if (velocity < 0f)
			{
				velocity = 0f;
			}
			wheel.Rotate(Vector3.up, num * UnityEngine.Time.deltaTime, Space.Self);
			SendNetworkUpdate();
		}
	}

	public void Update_Client()
	{
	}

	public void Update()
	{
		if (base.isClient)
		{
			Update_Client();
		}
		if (base.isServer)
		{
			Update_Server();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Spin(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && AllowPlayerSpins() && (AnyoneSpin() || rpc.player.CanBuild()) && Interface.CallHook("OnSpinWheel", rpc.player, this) == null && !(velocity > 15f))
		{
			velocity += UnityEngine.Random.Range(4f, 7f);
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_AnyoneSpin(RPCMessage rpc)
	{
		if (rpc.player.CanInteract())
		{
			SetFlag(Flags.Reserved3, rpc.read.Bit());
		}
	}

	public bool AnyoneSpin()
	{
		return HasFlag(Flags.Reserved3);
	}
}
