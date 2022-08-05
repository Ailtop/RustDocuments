#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class BaseArcadeMachine : BaseVehicle
{
	public class ScoreEntry
	{
		public ulong playerID;

		public int score;

		public string displayName;
	}

	public BaseArcadeGame arcadeGamePrefab;

	public BaseArcadeGame activeGame;

	public ArcadeNetworkTrigger networkTrigger;

	public float broadcastRadius = 8f;

	public Transform gameScreen;

	public RawImage RTImage;

	public Transform leftJoystick;

	public Transform rightJoystick;

	public SoundPlayer musicPlayer;

	public const Flags Flag_P1 = Flags.Reserved7;

	public const Flags Flag_P2 = Flags.Reserved8;

	public List<ScoreEntry> scores = new List<ScoreEntry>(10);

	private const int inputFrameRate = 60;

	private const int snapshotFrameRate = 15;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseArcadeMachine.OnRpcMessage"))
		{
			if (rpc == 271542211 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - BroadcastEntityMessage "));
				}
				using (TimeWarning.New("BroadcastEntityMessage"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(271542211u, "BroadcastEntityMessage", this, player, 7uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(271542211u, "BroadcastEntityMessage", this, player, 3f))
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
							BroadcastEntityMessage(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in BroadcastEntityMessage");
					}
				}
				return true;
			}
			if (rpc == 1365277306 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - DestroyMessageFromHost "));
				}
				using (TimeWarning.New("DestroyMessageFromHost"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1365277306u, "DestroyMessageFromHost", this, player, 3f))
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
							RPCMessage msg3 = rPCMessage;
							DestroyMessageFromHost(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in DestroyMessageFromHost");
					}
				}
				return true;
			}
			if (rpc == 2467852388u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - GetSnapshotFromClient "));
				}
				using (TimeWarning.New("GetSnapshotFromClient"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2467852388u, "GetSnapshotFromClient", this, player, 30uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2467852388u, "GetSnapshotFromClient", this, player, 3f))
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
							RPCMessage msg4 = rPCMessage;
							GetSnapshotFromClient(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in GetSnapshotFromClient");
					}
				}
				return true;
			}
			if (rpc == 2990871635u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RequestAddScore "));
				}
				using (TimeWarning.New("RequestAddScore"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2990871635u, "RequestAddScore", this, player, 3f))
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
							RPCMessage msg5 = rPCMessage;
							RequestAddScore(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RequestAddScore");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void AddScore(BasePlayer player, int score)
	{
		ScoreEntry scoreEntry = new ScoreEntry();
		scoreEntry.displayName = player.displayName;
		scoreEntry.score = score;
		scoreEntry.playerID = player.userID;
		scores.Add(scoreEntry);
		scores.Sort((ScoreEntry a, ScoreEntry b) => b.score.CompareTo(a.score));
		scores.TrimExcess();
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RequestAddScore(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (!(player == null) && PlayerIsMounted(player))
		{
			int score = msg.read.Int32();
			AddScore(player, score);
		}
	}

	public override void PlayerMounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerMounted(player, seat);
		ClientRPCPlayer(null, player, "BeginHosting");
		SetFlag(Flags.Reserved7, b: true, recursive: true);
	}

	public override void PlayerDismounted(BasePlayer player, BaseMountable seat)
	{
		base.PlayerDismounted(player, seat);
		ClientRPCPlayer(null, player, "EndHosting");
		SetFlag(Flags.Reserved7, b: false, recursive: true);
		if (!AnyMounted())
		{
			NearbyClientMessage("NoHost");
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.arcadeMachine = Facepunch.Pool.Get<ArcadeMachine>();
		info.msg.arcadeMachine.scores = Facepunch.Pool.GetList<ArcadeMachine.ScoreEntry>();
		for (int i = 0; i < scores.Count; i++)
		{
			ArcadeMachine.ScoreEntry scoreEntry = Facepunch.Pool.Get<ArcadeMachine.ScoreEntry>();
			scoreEntry.displayName = scores[i].displayName;
			scoreEntry.playerID = scores[i].playerID;
			scoreEntry.score = scores[i].score;
			info.msg.arcadeMachine.scores.Add(scoreEntry);
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.arcadeMachine != null && info.msg.arcadeMachine.scores != null)
		{
			scores.Clear();
			for (int i = 0; i < info.msg.arcadeMachine.scores.Count; i++)
			{
				ScoreEntry scoreEntry = new ScoreEntry();
				scoreEntry.displayName = info.msg.arcadeMachine.scores[i].displayName;
				scoreEntry.score = info.msg.arcadeMachine.scores[i].score;
				scoreEntry.playerID = info.msg.arcadeMachine.scores[i].playerID;
				scores.Add(scoreEntry);
			}
		}
	}

	protected override bool CanPushNow(BasePlayer pusher)
	{
		return false;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
	}

	public void NearbyClientMessage(string msg)
	{
		if (networkTrigger.entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			BasePlayer component = entityContent.GetComponent<BasePlayer>();
			ClientRPCPlayer(null, component, msg);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void DestroyMessageFromHost(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || GetDriver() != player || networkTrigger.entityContents == null)
		{
			return;
		}
		uint arg = msg.read.UInt32();
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			BasePlayer component = entityContent.GetComponent<BasePlayer>();
			ClientRPCPlayer(null, component, "DestroyEntity", arg);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(7uL)]
	[RPC_Server.IsVisible(3f)]
	public void BroadcastEntityMessage(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || GetDriver() != player || networkTrigger.entityContents == null)
		{
			return;
		}
		uint arg = msg.read.UInt32();
		string arg2 = msg.read.String();
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			BasePlayer component = entityContent.GetComponent<BasePlayer>();
			ClientRPCPlayer(null, component, "GetEntityMessage", arg, arg2);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(30uL)]
	[RPC_Server.IsVisible(3f)]
	public void GetSnapshotFromClient(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || player != GetDriver())
		{
			return;
		}
		ArcadeGame arcadeGame = Facepunch.Pool.Get<ArcadeGame>();
		arcadeGame = ArcadeGame.Deserialize(msg.read);
		Connection sourceConnection = null;
		if (networkTrigger.entityContents == null)
		{
			return;
		}
		foreach (BaseEntity entityContent in networkTrigger.entityContents)
		{
			BasePlayer component = entityContent.GetComponent<BasePlayer>();
			ClientRPCPlayer(sourceConnection, component, "GetSnapshotFromServer", arcadeGame);
		}
	}
}
