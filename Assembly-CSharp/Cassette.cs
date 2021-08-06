#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Cassette : BaseEntity
{
	public float MaxCassetteLength = 15f;

	[ReplicatedVar]
	public static float MaxCassetteFileSizeMB = 5f;

	public ulong CreatorSteamId;

	public PreloadedCassetteContent.PreloadType PreloadType;

	public PreloadedCassetteContent PreloadContent;

	public SoundDefinition InsertCassetteSfx;

	public int ViewmodelIndex;

	public Sprite HudSprite;

	public int MaximumVoicemailSlots = 1;

	public int preloadedAudioId;

	public ICassettePlayer currentCassettePlayer;

	public uint AudioId { get; private set; }

	public SoundDefinition PreloadedAudio => PreloadContent.GetSoundContent(preloadedAudioId, PreloadType);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Cassette.OnRpcMessage"))
		{
			if (rpc == 4031457637u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Server_MakeNewFile "));
				}
				using (TimeWarning.New("Server_MakeNewFile"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4031457637u, "Server_MakeNewFile", this, player, 1uL))
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
							Server_MakeNewFile(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in Server_MakeNewFile");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	[ServerVar]
	public static void ClearCassettes(ConsoleSystem.Arg arg)
	{
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			Cassette cassette;
			if ((object)(cassette = serverEntity as Cassette) != null && cassette.ClearSavedAudio())
			{
				num++;
			}
		}
		arg.ReplyWith($"Deleted the contents of {num} cassettes");
	}

	[ServerVar]
	public static void ClearCassettesByUser(ConsoleSystem.Arg arg)
	{
		ulong uInt = arg.GetUInt64(0, 0uL);
		int num = 0;
		foreach (BaseNetworkable serverEntity in BaseNetworkable.serverEntities)
		{
			Cassette cassette;
			if ((object)(cassette = serverEntity as Cassette) != null && cassette.CreatorSteamId == uInt)
			{
				cassette.ClearSavedAudio();
				num++;
			}
		}
		arg.ReplyWith($"Deleted {num} cassettes recorded by {uInt}");
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.cassette == null)
		{
			return;
		}
		uint audioId = AudioId;
		AudioId = info.msg.cassette.audioId;
		CreatorSteamId = info.msg.cassette.creatorSteamId;
		preloadedAudioId = info.msg.cassette.preloadAudioId;
		if (base.isServer && info.msg.cassette.holder != 0)
		{
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(info.msg.cassette.holder);
			ICassettePlayer cassettePlayer;
			if (baseNetworkable != null && (cassettePlayer = baseNetworkable as ICassettePlayer) != null)
			{
				currentCassettePlayer = cassettePlayer;
			}
		}
	}

	public void AssignPreloadContent()
	{
		switch (PreloadType)
		{
		case PreloadedCassetteContent.PreloadType.Short:
			preloadedAudioId = UnityEngine.Random.Range(0, PreloadContent.ShortTapeContent.Length);
			break;
		case PreloadedCassetteContent.PreloadType.Medium:
			preloadedAudioId = UnityEngine.Random.Range(0, PreloadContent.MediumTapeContent.Length);
			break;
		case PreloadedCassetteContent.PreloadType.Long:
			preloadedAudioId = UnityEngine.Random.Range(0, PreloadContent.LongTapeContent.Length);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.cassette = Facepunch.Pool.Get<ProtoBuf.Cassette>();
		info.msg.cassette.audioId = AudioId;
		info.msg.cassette.creatorSteamId = CreatorSteamId;
		info.msg.cassette.preloadAudioId = preloadedAudioId;
		if (!ObjectEx.IsUnityNull(currentCassettePlayer) && BaseEntityEx.IsValid(currentCassettePlayer.ToBaseEntity))
		{
			info.msg.cassette.holder = currentCassettePlayer.ToBaseEntity.net.ID;
		}
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		currentCassettePlayer?.OnCassetteRemoved(this);
		currentCassettePlayer = null;
		ICassettePlayer cassettePlayer;
		if (newParent != null && (cassettePlayer = newParent as ICassettePlayer) != null)
		{
			Invoke(DelayedCassetteInserted, 0.1f);
			currentCassettePlayer = cassettePlayer;
		}
	}

	public void DelayedCassetteInserted()
	{
		if (currentCassettePlayer != null)
		{
			currentCassettePlayer.OnCassetteInserted(this);
		}
	}

	public void SetAudioId(uint id, ulong userId)
	{
		AudioId = id;
		CreatorSteamId = userId;
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void Server_MakeNewFile(RPCMessage msg)
	{
		if (msg.player == null)
		{
			return;
		}
		HeldEntity heldEntity;
		if (GetParentEntity() != null && (object)(heldEntity = GetParentEntity() as HeldEntity) != null && heldEntity.GetOwnerPlayer() != msg.player)
		{
			Debug.Log("Player mismatch!");
			return;
		}
		byte[] data = msg.read.BytesWithSize();
		ulong userId = msg.read.UInt64();
		if (IsOggValid(data, this))
		{
			FileStorage.server.RemoveAllByEntity(net.ID);
			uint id = FileStorage.server.Store(data, FileStorage.Type.ogg, net.ID);
			SetAudioId(id, userId);
		}
	}

	public bool ClearSavedAudio()
	{
		if (AudioId == 0)
		{
			return false;
		}
		FileStorage.server.RemoveAllByEntity(net.ID);
		AudioId = 0u;
		CreatorSteamId = 0uL;
		SendNetworkUpdate();
		return true;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		ClearSavedAudio();
	}

	public static bool IsOggValid(byte[] data, Cassette c)
	{
		return IsOggValid(data, c.MaxCassetteLength);
	}

	public static bool IsOggValid(byte[] data, float maxLength)
	{
		if (data == null)
		{
			return false;
		}
		if (ByteToMegabyte(data.Length) >= MaxCassetteFileSizeMB)
		{
			Debug.Log("Audio file is too large! Aborting");
			return false;
		}
		double oggLength = GetOggLength(data);
		if (oggLength > (double)(maxLength * 1.2f))
		{
			Debug.Log($"Audio duration is longer than cassette limit! {oggLength} > {maxLength * 1.2f}");
			return false;
		}
		return true;
	}

	public static float ByteToMegabyte(int byteSize)
	{
		return (float)byteSize / 1024f / 1024f;
	}

	public static double GetOggLength(byte[] t)
	{
		int num = t.Length;
		long num2 = -1L;
		int num3 = -1;
		for (int num4 = num - 1 - 8 - 2 - 4; num4 >= 0; num4--)
		{
			if (t[num4] == 79 && t[num4 + 1] == 103 && t[num4 + 2] == 103 && t[num4 + 3] == 83)
			{
				num2 = BitConverter.ToInt64(new byte[8]
				{
					t[num4 + 6],
					t[num4 + 7],
					t[num4 + 8],
					t[num4 + 9],
					t[num4 + 10],
					t[num4 + 11],
					t[num4 + 12],
					t[num4 + 13]
				}, 0);
				break;
			}
		}
		for (int i = 0; i < num - 8 - 2 - 4; i++)
		{
			if (t[i] == 118 && t[i + 1] == 111 && t[i + 2] == 114 && t[i + 3] == 98 && t[i + 4] == 105 && t[i + 5] == 115)
			{
				num3 = BitConverter.ToInt32(new byte[4]
				{
					t[i + 11],
					t[i + 12],
					t[i + 13],
					t[i + 14]
				}, 0);
				break;
			}
		}
		if (RecorderTool.debugRecording)
		{
			Debug.Log($"{num2} / {num3}");
		}
		return (double)num2 / (double)num3;
	}
}
