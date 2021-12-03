#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class AudioVisualisationEntity : IOEntity
{
	public enum LightColour
	{
		Red,
		Green,
		Blue,
		Yellow,
		Pink
	}

	public enum VolumeSensitivity
	{
		Small,
		Medium,
		Large
	}

	public enum Speed
	{
		Low,
		Medium,
		High
	}

	private EntityRef<BaseEntity> connectedTo;

	public GameObjectRef SettingsDialog;

	public LightColour currentColour { get; private set; }

	public VolumeSensitivity currentVolumeSensitivity { get; private set; } = VolumeSensitivity.Medium;


	public Speed currentSpeed { get; private set; } = Speed.Medium;


	public int currentGradient { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AudioVisualisationEntity.OnRpcMessage"))
		{
			if (rpc == 4002266471u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ServerUpdateSettings "));
				}
				using (TimeWarning.New("ServerUpdateSettings"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4002266471u, "ServerUpdateSettings", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4002266471u, "ServerUpdateSettings", this, player, 3f))
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
							ServerUpdateSettings(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ServerUpdateSettings");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && old.HasFlag(Flags.Reserved8) != next.HasFlag(Flags.Reserved8) && next.HasFlag(Flags.Reserved8))
		{
			IOEntity audioSource = GetAudioSource(this, BoomBox.BacktrackLength);
			if (audioSource != null)
			{
				ClientRPC(null, "Client_PlayAudioFrom", audioSource.net.ID);
			}
			connectedTo.Set(audioSource);
		}
	}

	private IOEntity GetAudioSource(IOEntity entity, int depth)
	{
		if (depth <= 0)
		{
			return null;
		}
		IOSlot[] array = entity.inputs;
		for (int i = 0; i < array.Length; i++)
		{
			IOEntity iOEntity = array[i].connectedTo.Get(base.isServer);
			if (iOEntity == this)
			{
				return null;
			}
			IAudioConnectionSource component;
			if (iOEntity != null && iOEntity.TryGetComponent<IAudioConnectionSource>(out component))
			{
				return iOEntity;
			}
			AudioVisualisationEntity component2;
			if (iOEntity != null && iOEntity.TryGetComponent<AudioVisualisationEntity>(out component2) && component2.connectedTo.IsSet)
			{
				return component2.connectedTo.Get(base.isServer) as IOEntity;
			}
			if (iOEntity != null)
			{
				iOEntity = GetAudioSource(iOEntity, depth - 1);
				IAudioConnectionSource component3;
				if (iOEntity != null && iOEntity.TryGetComponent<IAudioConnectionSource>(out component3))
				{
					return iOEntity;
				}
			}
		}
		return null;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.connectedSpeaker == null)
		{
			info.msg.connectedSpeaker = Facepunch.Pool.Get<ProtoBuf.ConnectedSpeaker>();
		}
		info.msg.connectedSpeaker.connectedTo = connectedTo.uid;
		if (info.msg.audioEntity == null)
		{
			info.msg.audioEntity = Facepunch.Pool.Get<AudioEntity>();
		}
		info.msg.audioEntity.colourMode = (int)currentColour;
		info.msg.audioEntity.volumeRange = (int)currentVolumeSensitivity;
		info.msg.audioEntity.speed = (int)currentSpeed;
		info.msg.audioEntity.gradient = currentGradient;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	public void ServerUpdateSettings(RPCMessage msg)
	{
		int num = msg.read.Int32();
		int num2 = msg.read.Int32();
		int num3 = msg.read.Int32();
		int num4 = msg.read.Int32();
		if (currentColour != (LightColour)num || currentVolumeSensitivity != (VolumeSensitivity)num2 || currentSpeed != (Speed)num3 || currentGradient != num4)
		{
			currentColour = (LightColour)num;
			currentVolumeSensitivity = (VolumeSensitivity)num2;
			currentSpeed = (Speed)num3;
			currentGradient = num4;
			MarkDirty();
			SendNetworkUpdate();
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.audioEntity != null)
		{
			currentColour = (LightColour)info.msg.audioEntity.colourMode;
			currentVolumeSensitivity = (VolumeSensitivity)info.msg.audioEntity.volumeRange;
			currentSpeed = (Speed)info.msg.audioEntity.speed;
			currentGradient = info.msg.audioEntity.gradient;
		}
		if (info.msg.connectedSpeaker != null)
		{
			connectedTo.uid = info.msg.connectedSpeaker.connectedTo;
		}
	}
}
