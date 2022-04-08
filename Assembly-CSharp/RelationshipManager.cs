#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using CompanionServer;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class RelationshipManager : BaseEntity
{
	public enum RelationshipType
	{
		NONE = 0,
		Acquaintance = 1,
		Friend = 2,
		Enemy = 3
	}

	public class PlayerRelationshipInfo : Facepunch.Pool.IPooled, IServerFileReceiver
	{
		public string displayName;

		public ulong player;

		public RelationshipType type;

		public int weight;

		public uint mugshotCrc;

		public string notes;

		public float lastSeenTime;

		public float lastMugshotTime;

		public void EnterPool()
		{
			Reset();
		}

		public void LeavePool()
		{
			Reset();
		}

		private void Reset()
		{
			displayName = null;
			player = 0uL;
			type = RelationshipType.NONE;
			weight = 0;
			mugshotCrc = 0u;
			notes = "";
			lastMugshotTime = 0f;
		}
	}

	public class PlayerRelationships : Facepunch.Pool.IPooled
	{
		public bool dirty;

		public ulong ownerPlayer;

		public Dictionary<ulong, PlayerRelationshipInfo> relations;

		public bool Forget(ulong player)
		{
			PlayerRelationshipInfo value;
			if (relations.TryGetValue(player, out value))
			{
				relations.Remove(player);
				if (value.mugshotCrc != 0)
				{
					ServerInstance.DeleteMugshot(ownerPlayer, player, value.mugshotCrc);
				}
				return true;
			}
			return false;
		}

		public PlayerRelationshipInfo GetRelations(ulong player)
		{
			BasePlayer basePlayer = FindByID(player);
			PlayerRelationshipInfo value;
			if (relations.TryGetValue(player, out value))
			{
				if (basePlayer != null)
				{
					value.displayName = basePlayer.displayName;
				}
				return value;
			}
			PlayerRelationshipInfo playerRelationshipInfo = Facepunch.Pool.Get<PlayerRelationshipInfo>();
			if (basePlayer != null)
			{
				playerRelationshipInfo.displayName = basePlayer.displayName;
			}
			playerRelationshipInfo.player = player;
			relations.Add(player, playerRelationshipInfo);
			return playerRelationshipInfo;
		}

		public PlayerRelationships()
		{
			LeavePool();
		}

		public void EnterPool()
		{
			ownerPlayer = 0uL;
			if (relations != null)
			{
				relations.Clear();
				Facepunch.Pool.Free(ref relations);
			}
		}

		public void LeavePool()
		{
			ownerPlayer = 0uL;
			relations = Facepunch.Pool.Get<Dictionary<ulong, PlayerRelationshipInfo>>();
			relations.Clear();
		}
	}

	public class PlayerTeam
	{
		public ulong teamID;

		public string teamName;

		public ulong teamLeader;

		public List<ulong> members = new List<ulong>();

		public List<ulong> invites = new List<ulong>();

		public float teamStartTime;

		public List<Network.Connection> onlineMemberConnections = new List<Network.Connection>();

		public float teamLifetime => UnityEngine.Time.realtimeSinceStartup - teamStartTime;

		public BasePlayer GetLeader()
		{
			return FindByID(teamLeader);
		}

		public void SendInvite(BasePlayer player)
		{
			if (invites.Count > 8)
			{
				invites.RemoveRange(0, 1);
			}
			BasePlayer basePlayer = FindByID(teamLeader);
			if (!(basePlayer == null))
			{
				invites.Add(player.userID);
				player.ClientRPCPlayer(null, player, "CLIENT_PendingInvite", basePlayer.displayName, teamLeader, teamID);
			}
		}

		public void AcceptInvite(BasePlayer player)
		{
			if (invites.Contains(player.userID))
			{
				invites.Remove(player.userID);
				AddPlayer(player);
				player.ClearPendingInvite();
			}
		}

		public void RejectInvite(BasePlayer player)
		{
			player.ClearPendingInvite();
			invites.Remove(player.userID);
		}

		public bool AddPlayer(BasePlayer player)
		{
			ulong userID = player.userID;
			if (members.Contains(userID))
			{
				return false;
			}
			if (player.currentTeam != 0L)
			{
				return false;
			}
			if (members.Count >= maxTeamSize)
			{
				return false;
			}
			player.currentTeam = teamID;
			members.Add(userID);
			ServerInstance.playerToTeam.Add(userID, this);
			MarkDirty();
			player.SendNetworkUpdate();
			return true;
		}

		public bool RemovePlayer(ulong playerID)
		{
			if (members.Contains(playerID))
			{
				members.Remove(playerID);
				ServerInstance.playerToTeam.Remove(playerID);
				BasePlayer basePlayer = FindByID(playerID);
				if (basePlayer != null)
				{
					basePlayer.ClearTeam();
					CompanionServer.Util.BroadcastAppTeamRemoval(basePlayer);
				}
				if (teamLeader == playerID)
				{
					if (members.Count > 0)
					{
						SetTeamLeader(members[0]);
					}
					else
					{
						Disband();
					}
				}
				MarkDirty();
				return true;
			}
			return false;
		}

		public void SetTeamLeader(ulong newTeamLeader)
		{
			teamLeader = newTeamLeader;
			MarkDirty();
		}

		public void Disband()
		{
			ServerInstance.DisbandTeam(this);
			CompanionServer.Server.TeamChat.Remove(teamID);
		}

		public void MarkDirty()
		{
			foreach (ulong member in members)
			{
				BasePlayer basePlayer = FindByID(member);
				if (basePlayer != null)
				{
					basePlayer.UpdateTeam(teamID);
				}
			}
			CompanionServer.Util.BroadcastAppTeamUpdate(this);
		}

		public List<Network.Connection> GetOnlineMemberConnections()
		{
			if (members.Count == 0)
			{
				return null;
			}
			onlineMemberConnections.Clear();
			foreach (ulong member in members)
			{
				BasePlayer basePlayer = FindByID(member);
				if (!(basePlayer == null) && basePlayer.Connection != null)
				{
					onlineMemberConnections.Add(basePlayer.Connection);
				}
			}
			return onlineMemberConnections;
		}
	}

	private const int MugshotResolution = 256;

	private const int MugshotMaxFileSize = 65536;

	private const float MugshotMaxDistance = 50f;

	public Dictionary<ulong, PlayerRelationships> relationships = new Dictionary<ulong, PlayerRelationships>();

	private int lastReputationUpdateIndex;

	private const int seenReputationSeconds = 60;

	private int startingReputation;

	[ServerVar]
	public static bool contacts = true;

	[ServerVar]
	public static int forgetafterminutes = 960;

	[ServerVar]
	public static int maxplayerrelationships = 128;

	[ServerVar]
	public static float seendistance = 10f;

	[ServerVar]
	public static float mugshotUpdateInterval = 300f;

	private static List<BasePlayer> _dirtyRelationshipPlayers = new List<BasePlayer>();

	public static int maxTeamSize_Internal = 8;

	public Dictionary<ulong, BasePlayer> cachedPlayers = new Dictionary<ulong, BasePlayer>();

	public Dictionary<ulong, PlayerTeam> playerToTeam = new Dictionary<ulong, PlayerTeam>();

	public Dictionary<ulong, PlayerTeam> teams = new Dictionary<ulong, PlayerTeam>();

	public ulong lastTeamIndex = 1uL;

	[ServerVar]
	public static int maxTeamSize
	{
		get
		{
			return maxTeamSize_Internal;
		}
		set
		{
			maxTeamSize_Internal = value;
			if ((bool)ServerInstance)
			{
				ServerInstance.SendNetworkUpdate();
			}
		}
	}

	public static RelationshipManager ServerInstance { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RelationshipManager.OnRpcMessage"))
		{
			if (rpc == 1684577101 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_ChangeRelationship "));
				}
				using (TimeWarning.New("SERVER_ChangeRelationship"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1684577101u, "SERVER_ChangeRelationship", this, player, 2uL))
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
							SERVER_ChangeRelationship(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SERVER_ChangeRelationship");
					}
				}
				return true;
			}
			if (rpc == 1239936737 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_ReceiveMugshot "));
				}
				using (TimeWarning.New("SERVER_ReceiveMugshot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1239936737u, "SERVER_ReceiveMugshot", this, player, 10uL))
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
							SERVER_ReceiveMugshot(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SERVER_ReceiveMugshot");
					}
				}
				return true;
			}
			if (rpc == 2178173141u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_SendFreshContacts "));
				}
				using (TimeWarning.New("SERVER_SendFreshContacts"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2178173141u, "SERVER_SendFreshContacts", this, player, 1uL))
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
							SERVER_SendFreshContacts(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in SERVER_SendFreshContacts");
					}
				}
				return true;
			}
			if (rpc == 290196604 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SERVER_UpdatePlayerNote "));
				}
				using (TimeWarning.New("SERVER_UpdatePlayerNote"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(290196604u, "SERVER_UpdatePlayerNote", this, player, 10uL))
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
							SERVER_UpdatePlayerNote(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in SERVER_UpdatePlayerNote");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (contacts)
		{
			InvokeRepeating(UpdateContactsTick, 0f, 1f);
			InvokeRepeating(UpdateReputations, 0f, 0.05f);
			InvokeRepeating(SendRelationships, 0f, 5f);
		}
	}

	public void UpdateReputations()
	{
		if (contacts && BasePlayer.activePlayerList.Count != 0)
		{
			if (lastReputationUpdateIndex >= BasePlayer.activePlayerList.Count)
			{
				lastReputationUpdateIndex = 0;
			}
			BasePlayer basePlayer = BasePlayer.activePlayerList[lastReputationUpdateIndex];
			if (basePlayer.reputation != (basePlayer.reputation = GetReputationFor(basePlayer.userID)))
			{
				basePlayer.SendNetworkUpdate();
			}
			lastReputationUpdateIndex++;
		}
	}

	public void UpdateContactsTick()
	{
		if (!contacts)
		{
			return;
		}
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			UpdateAcquaintancesFor(activePlayer, 1f);
		}
	}

	public int GetReputationFor(ulong playerID)
	{
		int num = startingReputation;
		foreach (PlayerRelationships value in relationships.Values)
		{
			foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in value.relations)
			{
				if (relation.Key != playerID)
				{
					continue;
				}
				if (relation.Value.type == RelationshipType.Friend)
				{
					num++;
				}
				else if (relation.Value.type == RelationshipType.Acquaintance)
				{
					if (relation.Value.weight > 60)
					{
						num++;
					}
				}
				else if (relation.Value.type == RelationshipType.Enemy)
				{
					num--;
				}
			}
		}
		return num;
	}

	[ServerVar]
	public static void wipecontacts(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && !(ServerInstance == null))
		{
			ulong userID = basePlayer.userID;
			if (ServerInstance.relationships.ContainsKey(userID))
			{
				Debug.Log("Wiped contacts for :" + userID);
				ServerInstance.relationships.Remove(userID);
				ServerInstance.MarkRelationshipsDirtyFor(userID);
			}
			else
			{
				Debug.Log("No contacts for :" + userID);
			}
		}
	}

	[ServerVar]
	public static void wipe_all_contacts(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null || ServerInstance == null)
		{
			return;
		}
		if (!arg.HasArgs() || arg.Args[0] != "confirm")
		{
			Debug.Log("Please append the word 'confirm' at the end of the console command to execute");
			return;
		}
		ulong userID = basePlayer.userID;
		ServerInstance.relationships.Clear();
		foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
		{
			ServerInstance.MarkRelationshipsDirtyFor(activePlayer);
		}
		Debug.Log("Wiped all contacts.");
	}

	public float GetAcquaintanceMaxDist()
	{
		return seendistance;
	}

	public void UpdateAcquaintancesFor(BasePlayer player, float deltaSeconds)
	{
		PlayerRelationships playerRelationships = GetRelationships(player.userID);
		List<BasePlayer> obj = Facepunch.Pool.GetList<BasePlayer>();
		BaseNetworkable.GetCloseConnections(player.transform.position, GetAcquaintanceMaxDist(), obj);
		foreach (BasePlayer item in obj)
		{
			if (item == player || item.isClient || !item.IsAlive() || item.IsSleeping() || item.limitNetworking)
			{
				continue;
			}
			PlayerRelationshipInfo relations = playerRelationships.GetRelations(item.userID);
			if (!(Vector3.Distance(player.transform.position, item.transform.position) <= GetAcquaintanceMaxDist()))
			{
				continue;
			}
			relations.lastSeenTime = UnityEngine.Time.realtimeSinceStartup;
			if ((relations.type == RelationshipType.NONE || relations.type == RelationshipType.Acquaintance) && player.IsPlayerVisibleToUs(item, 1218519041))
			{
				int num = Mathf.CeilToInt(deltaSeconds);
				if (player.InSafeZone() || item.InSafeZone())
				{
					num = 0;
				}
				if (relations.type != RelationshipType.Acquaintance || (relations.weight < 60 && num > 0))
				{
					SetRelationship(player, item, RelationshipType.Acquaintance, num);
				}
			}
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public void SetSeen(BasePlayer player, BasePlayer otherPlayer)
	{
		ulong userID = player.userID;
		ulong userID2 = otherPlayer.userID;
		PlayerRelationshipInfo relations = GetRelationships(userID).GetRelations(userID2);
		if (relations.type != 0)
		{
			relations.lastSeenTime = UnityEngine.Time.realtimeSinceStartup;
		}
	}

	public bool CleanupOldContacts(PlayerRelationships ownerRelationships, ulong playerID, RelationshipType relationshipType = RelationshipType.Acquaintance)
	{
		int numberRelationships = GetNumberRelationships(playerID);
		if (numberRelationships < maxplayerrelationships)
		{
			return true;
		}
		List<ulong> obj = Facepunch.Pool.GetList<ulong>();
		foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in ownerRelationships.relations)
		{
			if (relation.Value.type == relationshipType && UnityEngine.Time.realtimeSinceStartup - relation.Value.lastSeenTime > (float)forgetafterminutes * 60f)
			{
				obj.Add(relation.Key);
			}
		}
		int count = obj.Count;
		foreach (ulong item in obj)
		{
			ownerRelationships.Forget(item);
		}
		Facepunch.Pool.FreeList(ref obj);
		return numberRelationships - count < maxplayerrelationships;
	}

	public void ForceRelationshipByID(BasePlayer player, ulong otherPlayerID, RelationshipType newType, int weight, bool sendImmediate = false)
	{
		if (!contacts || player == null || player.userID == otherPlayerID || player.IsNpc)
		{
			return;
		}
		ulong userID = player.userID;
		if (HasRelations(userID, otherPlayerID))
		{
			PlayerRelationshipInfo relations = GetRelationships(userID).GetRelations(otherPlayerID);
			if (relations.type != newType)
			{
				relations.weight = 0;
			}
			relations.type = newType;
			relations.weight += weight;
			if (sendImmediate)
			{
				SendRelationshipsFor(player);
			}
			else
			{
				MarkRelationshipsDirtyFor(player);
			}
		}
	}

	public void SetRelationship(BasePlayer player, BasePlayer otherPlayer, RelationshipType type, int weight = 1, bool sendImmediate = false)
	{
		if (!contacts)
		{
			return;
		}
		ulong userID = player.userID;
		ulong userID2 = otherPlayer.userID;
		if (player == null || player == otherPlayer || player.IsNpc || (otherPlayer != null && otherPlayer.IsNpc) || Interface.CallHook("CanSetRelationship", player, otherPlayer, type, weight) != null)
		{
			return;
		}
		PlayerRelationships playerRelationships = GetRelationships(userID);
		if (!CleanupOldContacts(playerRelationships, userID))
		{
			CleanupOldContacts(playerRelationships, userID, RelationshipType.Enemy);
		}
		PlayerRelationshipInfo relations = playerRelationships.GetRelations(userID2);
		bool flag = false;
		if (relations.type != type)
		{
			flag = true;
			relations.weight = 0;
		}
		relations.type = type;
		relations.weight += weight;
		float num = UnityEngine.Time.realtimeSinceStartup - relations.lastMugshotTime;
		if (flag || relations.mugshotCrc == 0 || num >= mugshotUpdateInterval)
		{
			bool flag2 = otherPlayer.IsAlive();
			bool num2 = player.SecondsSinceAttacked > 10f && !player.IsAiming;
			float num3 = 100f;
			if (num2)
			{
				Vector3 normalized = (otherPlayer.eyes.position - player.eyes.position).normalized;
				bool flag3 = Vector3.Dot(player.eyes.HeadForward(), normalized) >= 0.6f;
				float num4 = Vector3Ex.Distance2D(player.transform.position, otherPlayer.transform.position);
				if (flag2 && num4 < num3 && flag3)
				{
					ClientRPCPlayer(null, player, "CLIENT_DoMugshot", userID2);
					relations.lastMugshotTime = UnityEngine.Time.realtimeSinceStartup;
				}
			}
		}
		if (sendImmediate)
		{
			SendRelationshipsFor(player);
		}
		else
		{
			MarkRelationshipsDirtyFor(player);
		}
	}

	public ProtoBuf.RelationshipManager.PlayerRelationships GetRelationshipSaveByID(ulong playerID)
	{
		ProtoBuf.RelationshipManager.PlayerRelationships playerRelationships = Facepunch.Pool.Get<ProtoBuf.RelationshipManager.PlayerRelationships>();
		PlayerRelationships playerRelationships2 = GetRelationships(playerID);
		if (playerRelationships2 != null)
		{
			playerRelationships.playerID = playerID;
			playerRelationships.relations = Facepunch.Pool.GetList<ProtoBuf.RelationshipManager.PlayerRelationshipInfo>();
			{
				foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in playerRelationships2.relations)
				{
					ProtoBuf.RelationshipManager.PlayerRelationshipInfo playerRelationshipInfo = Facepunch.Pool.Get<ProtoBuf.RelationshipManager.PlayerRelationshipInfo>();
					playerRelationshipInfo.playerID = relation.Value.player;
					playerRelationshipInfo.type = (int)relation.Value.type;
					playerRelationshipInfo.weight = relation.Value.weight;
					playerRelationshipInfo.mugshotCrc = relation.Value.mugshotCrc;
					playerRelationshipInfo.displayName = relation.Value.displayName;
					playerRelationshipInfo.notes = relation.Value.notes;
					playerRelationshipInfo.timeSinceSeen = UnityEngine.Time.realtimeSinceStartup - relation.Value.lastSeenTime;
					playerRelationships.relations.Add(playerRelationshipInfo);
				}
				return playerRelationships;
			}
		}
		return null;
	}

	public void MarkRelationshipsDirtyFor(ulong playerID)
	{
		BasePlayer basePlayer = FindByID(playerID);
		if ((bool)basePlayer)
		{
			MarkRelationshipsDirtyFor(basePlayer);
		}
	}

	public static void ForceSendRelationships(BasePlayer player)
	{
		if ((bool)ServerInstance)
		{
			ServerInstance.MarkRelationshipsDirtyFor(player);
		}
	}

	public void MarkRelationshipsDirtyFor(BasePlayer player)
	{
		if (!(player == null))
		{
			if (!_dirtyRelationshipPlayers.Contains(player))
			{
				_dirtyRelationshipPlayers.Add(player);
			}
			ulong userID = player.userID;
		}
	}

	public void SendRelationshipsFor(BasePlayer player)
	{
		if (contacts)
		{
			ulong userID = player.userID;
			ProtoBuf.RelationshipManager.PlayerRelationships relationshipSaveByID = GetRelationshipSaveByID(userID);
			ClientRPCPlayer(null, player, "CLIENT_RecieveLocalRelationships", relationshipSaveByID);
		}
	}

	public void SendRelationships()
	{
		if (!contacts)
		{
			return;
		}
		foreach (BasePlayer dirtyRelationshipPlayer in _dirtyRelationshipPlayers)
		{
			if (!(dirtyRelationshipPlayer == null) && dirtyRelationshipPlayer.IsConnected && !dirtyRelationshipPlayer.IsSleeping())
			{
				SendRelationshipsFor(dirtyRelationshipPlayer);
			}
		}
		_dirtyRelationshipPlayers.Clear();
	}

	public int GetNumberRelationships(ulong player)
	{
		PlayerRelationships value;
		if (relationships.TryGetValue(player, out value))
		{
			return value.relations.Count;
		}
		return 0;
	}

	public bool HasRelations(ulong player, ulong otherPlayer)
	{
		PlayerRelationships value;
		if (relationships.TryGetValue(player, out value) && value.relations.ContainsKey(otherPlayer))
		{
			return true;
		}
		return false;
	}

	public PlayerRelationships GetRelationships(ulong player)
	{
		PlayerRelationships value;
		if (relationships.TryGetValue(player, out value))
		{
			return value;
		}
		PlayerRelationships playerRelationships = Facepunch.Pool.Get<PlayerRelationships>();
		playerRelationships.ownerPlayer = player;
		relationships.Add(player, playerRelationships);
		return playerRelationships;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void SERVER_SendFreshContacts(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if ((bool)player)
		{
			SendRelationshipsFor(player);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	public void SERVER_ChangeRelationship(RPCMessage msg)
	{
		ulong userID = msg.player.userID;
		ulong num = msg.read.UInt64();
		int num2 = Mathf.Clamp(msg.read.Int32(), 0, 3);
		PlayerRelationships playerRelationships = GetRelationships(userID);
		playerRelationships.GetRelations(num);
		BasePlayer player = msg.player;
		RelationshipType relationshipType = (RelationshipType)num2;
		if (num2 == 0)
		{
			if (playerRelationships.Forget(num))
			{
				SendRelationshipsFor(player);
			}
			return;
		}
		BasePlayer basePlayer = FindByID(num);
		if (basePlayer == null)
		{
			ForceRelationshipByID(player, num, relationshipType, 0, true);
		}
		else
		{
			SetRelationship(player, basePlayer, relationshipType, 1, true);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void SERVER_UpdatePlayerNote(RPCMessage msg)
	{
		ulong userID = msg.player.userID;
		ulong player = msg.read.UInt64();
		string notes = msg.read.String();
		GetRelationships(userID).GetRelations(player).notes = notes;
		MarkRelationshipsDirtyFor(userID);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void SERVER_ReceiveMugshot(RPCMessage msg)
	{
		ulong userID = msg.player.userID;
		ulong num = msg.read.UInt64();
		uint num2 = msg.read.UInt32();
		byte[] array = msg.read.BytesWithSize(65536u);
		PlayerRelationships value;
		PlayerRelationshipInfo value2;
		if (array != null && ImageProcessing.IsValidJPG(array, 256, 512) && relationships.TryGetValue(userID, out value) && value.relations.TryGetValue(num, out value2))
		{
			uint steamIdHash = GetSteamIdHash(userID, num);
			uint num3 = FileStorage.server.Store(array, FileStorage.Type.jpg, net.ID, steamIdHash);
			if (num3 != num2)
			{
				Debug.LogWarning("Client/Server FileStorage CRC differs");
			}
			if (num3 != value2.mugshotCrc)
			{
				FileStorage.server.RemoveExact(value2.mugshotCrc, FileStorage.Type.jpg, net.ID, steamIdHash);
			}
			value2.mugshotCrc = num3;
			MarkRelationshipsDirtyFor(userID);
		}
	}

	private void DeleteMugshot(ulong steamId, ulong targetSteamId, uint crc)
	{
		if (crc != 0)
		{
			uint steamIdHash = GetSteamIdHash(steamId, targetSteamId);
			FileStorage.server.RemoveExact(crc, FileStorage.Type.jpg, net.ID, steamIdHash);
		}
	}

	private static uint GetSteamIdHash(ulong requesterSteamId, ulong targetSteamId)
	{
		return (uint)(((requesterSteamId & 0xFFFF) << 16) | (targetSteamId & 0xFFFF));
	}

	public int GetMaxTeamSize()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(true);
		if ((bool)activeGameMode)
		{
			return activeGameMode.GetMaxRelationshipTeamSize();
		}
		return maxTeamSize;
	}

	public void OnEnable()
	{
		if (base.isServer)
		{
			if (ServerInstance != null)
			{
				Debug.LogError("Major fuckup! RelationshipManager spawned twice, Contact Developers!");
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				ServerInstance = this;
			}
		}
	}

	public void OnDestroy()
	{
		if (base.isServer)
		{
			ServerInstance = null;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.relationshipManager = Facepunch.Pool.Get<ProtoBuf.RelationshipManager>();
		info.msg.relationshipManager.maxTeamSize = maxTeamSize;
		if (!info.forDisk)
		{
			return;
		}
		info.msg.relationshipManager.lastTeamIndex = lastTeamIndex;
		info.msg.relationshipManager.teamList = Facepunch.Pool.GetList<ProtoBuf.PlayerTeam>();
		foreach (KeyValuePair<ulong, PlayerTeam> team in teams)
		{
			PlayerTeam value = team.Value;
			if (value == null)
			{
				continue;
			}
			ProtoBuf.PlayerTeam playerTeam = Facepunch.Pool.Get<ProtoBuf.PlayerTeam>();
			playerTeam.teamLeader = value.teamLeader;
			playerTeam.teamID = value.teamID;
			playerTeam.teamName = value.teamName;
			playerTeam.members = Facepunch.Pool.GetList<ProtoBuf.PlayerTeam.TeamMember>();
			foreach (ulong member in value.members)
			{
				ProtoBuf.PlayerTeam.TeamMember teamMember = Facepunch.Pool.Get<ProtoBuf.PlayerTeam.TeamMember>();
				BasePlayer basePlayer = FindByID(member);
				teamMember.displayName = ((basePlayer != null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
				teamMember.userID = member;
				playerTeam.members.Add(teamMember);
			}
			info.msg.relationshipManager.teamList.Add(playerTeam);
		}
		info.msg.relationshipManager.relationships = Facepunch.Pool.GetList<ProtoBuf.RelationshipManager.PlayerRelationships>();
		foreach (ulong key in relationships.Keys)
		{
			PlayerRelationships playerRelationship = relationships[key];
			ProtoBuf.RelationshipManager.PlayerRelationships relationshipSaveByID = GetRelationshipSaveByID(key);
			info.msg.relationshipManager.relationships.Add(relationshipSaveByID);
		}
	}

	public void DisbandTeam(PlayerTeam teamToDisband)
	{
		if (Interface.CallHook("OnTeamDisband", teamToDisband) == null)
		{
			teams.Remove(teamToDisband.teamID);
			Interface.CallHook("OnTeamDisbanded", teamToDisband);
			Facepunch.Pool.Free(ref teamToDisband);
		}
	}

	public static BasePlayer FindByID(ulong userID)
	{
		BasePlayer value = null;
		if (ServerInstance.cachedPlayers.TryGetValue(userID, out value))
		{
			if (value != null)
			{
				return value;
			}
			ServerInstance.cachedPlayers.Remove(userID);
		}
		BasePlayer basePlayer = BasePlayer.FindByID(userID);
		if (!basePlayer)
		{
			basePlayer = BasePlayer.FindSleeping(userID);
		}
		if (basePlayer != null)
		{
			ServerInstance.cachedPlayers.Add(userID, basePlayer);
		}
		return basePlayer;
	}

	public PlayerTeam FindTeam(ulong TeamID)
	{
		if (teams.ContainsKey(TeamID))
		{
			return teams[TeamID];
		}
		return null;
	}

	public PlayerTeam FindPlayersTeam(ulong userID)
	{
		PlayerTeam value;
		if (playerToTeam.TryGetValue(userID, out value))
		{
			return value;
		}
		return null;
	}

	public PlayerTeam CreateTeam()
	{
		PlayerTeam playerTeam = Facepunch.Pool.Get<PlayerTeam>();
		playerTeam.teamID = lastTeamIndex;
		playerTeam.teamStartTime = UnityEngine.Time.realtimeSinceStartup;
		teams.Add(lastTeamIndex, playerTeam);
		lastTeamIndex++;
		return playerTeam;
	}

	[ServerUserVar]
	public static void trycreateteam(ConsoleSystem.Arg arg)
	{
		if (maxTeamSize == 0)
		{
			arg.ReplyWith("Teams are disabled on this server");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.currentTeam == 0L && Interface.CallHook("OnTeamCreate", basePlayer) == null)
		{
			PlayerTeam playerTeam = ServerInstance.CreateTeam();
			playerTeam.teamLeader = basePlayer.userID;
			playerTeam.AddPlayer(basePlayer);
			Interface.CallHook("OnTeamCreated", basePlayer, playerTeam);
		}
	}

	[ServerUserVar]
	public static void promote(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.currentTeam == 0L)
		{
			return;
		}
		BasePlayer lookingAtPlayer = GetLookingAtPlayer(basePlayer);
		if (!(lookingAtPlayer == null) && !lookingAtPlayer.IsDead() && !(lookingAtPlayer == basePlayer) && lookingAtPlayer.currentTeam == basePlayer.currentTeam)
		{
			PlayerTeam playerTeam = ServerInstance.teams[basePlayer.currentTeam];
			if (playerTeam != null && playerTeam.teamLeader == basePlayer.userID && Interface.CallHook("OnTeamPromote", playerTeam, lookingAtPlayer) == null)
			{
				playerTeam.SetTeamLeader(lookingAtPlayer.userID);
			}
		}
	}

	[ServerUserVar]
	public static void leaveteam(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && basePlayer.currentTeam != 0L)
		{
			PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
			if (playerTeam != null && Interface.CallHook("OnTeamLeave", playerTeam, basePlayer) == null)
			{
				playerTeam.RemovePlayer(basePlayer.userID);
				basePlayer.ClearTeam();
			}
		}
	}

	[ServerUserVar]
	public static void acceptinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && basePlayer.currentTeam == 0L)
		{
			ulong uLong = arg.GetULong(0, 0uL);
			PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
			if (playerTeam == null)
			{
				basePlayer.ClearPendingInvite();
			}
			else if (Interface.CallHook("OnTeamAcceptInvite", playerTeam, basePlayer) == null)
			{
				playerTeam.AcceptInvite(basePlayer);
			}
		}
	}

	[ServerUserVar]
	public static void rejectinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!(basePlayer == null) && basePlayer.currentTeam == 0L)
		{
			ulong uLong = arg.GetULong(0, 0uL);
			PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
			if (playerTeam == null)
			{
				basePlayer.ClearPendingInvite();
			}
			else if (Interface.CallHook("OnTeamRejectInvite", basePlayer, playerTeam) == null)
			{
				playerTeam.RejectInvite(basePlayer);
			}
		}
	}

	public static BasePlayer GetLookingAtPlayer(BasePlayer source)
	{
		RaycastHit hitInfo;
		if (UnityEngine.Physics.Raycast(source.eyes.position, source.eyes.HeadForward(), out hitInfo, 5f, 1218652417, QueryTriggerInteraction.Ignore))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if ((bool)entity)
			{
				return entity.GetComponent<BasePlayer>();
			}
		}
		return null;
	}

	[ServerVar]
	public static void sleeptoggle(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		RaycastHit hitInfo;
		if (basePlayer == null || !UnityEngine.Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), out hitInfo, 5f, 1218652417, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
		if (!entity)
		{
			return;
		}
		BasePlayer component = entity.GetComponent<BasePlayer>();
		if ((bool)component && component != basePlayer && !component.IsNpc)
		{
			if (component.IsSleeping())
			{
				component.EndSleeping();
			}
			else
			{
				component.StartSleeping();
			}
		}
	}

	[ServerUserVar]
	public static void kickmember(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer == null)
		{
			return;
		}
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		if (playerTeam != null && !(playerTeam.GetLeader() != basePlayer))
		{
			ulong uLong = arg.GetULong(0, 0uL);
			if (basePlayer.userID != uLong && Interface.CallHook("OnTeamKick", playerTeam, basePlayer, uLong) == null)
			{
				playerTeam.RemovePlayer(uLong);
			}
		}
	}

	[ServerUserVar]
	public static void sendinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		RaycastHit hitInfo;
		if (playerTeam == null || playerTeam.GetLeader() == null || playerTeam.GetLeader() != basePlayer || !UnityEngine.Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), out hitInfo, 5f, 1218652417, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
		if ((bool)entity)
		{
			BasePlayer component = entity.GetComponent<BasePlayer>();
			if ((bool)component && component != basePlayer && !component.IsNpc && component.currentTeam == 0L && Interface.CallHook("OnTeamInvite", basePlayer, component) == null)
			{
				playerTeam.SendInvite(component);
			}
		}
	}

	[ServerVar]
	public static void fakeinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		ulong uLong = arg.GetULong(0, 0uL);
		PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
		if (playerTeam != null)
		{
			if (basePlayer.currentTeam != 0L)
			{
				Debug.Log("already in team");
			}
			playerTeam.SendInvite(basePlayer);
			Debug.Log("sent bot invite");
		}
	}

	[ServerVar]
	public static void addtoteam(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		RaycastHit hitInfo;
		if (playerTeam == null || playerTeam.GetLeader() == null || playerTeam.GetLeader() != basePlayer || !UnityEngine.Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), out hitInfo, 5f, 1218652417, QueryTriggerInteraction.Ignore))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
		if ((bool)entity)
		{
			BasePlayer component = entity.GetComponent<BasePlayer>();
			if ((bool)component && component != basePlayer && !component.IsNpc)
			{
				playerTeam.AddPlayer(component);
			}
		}
	}

	public static bool TeamsEnabled()
	{
		return maxTeamSize > 0;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.relationshipManager == null)
		{
			return;
		}
		lastTeamIndex = info.msg.relationshipManager.lastTeamIndex;
		foreach (ProtoBuf.PlayerTeam team in info.msg.relationshipManager.teamList)
		{
			PlayerTeam playerTeam = Facepunch.Pool.Get<PlayerTeam>();
			playerTeam.teamLeader = team.teamLeader;
			playerTeam.teamID = team.teamID;
			playerTeam.teamName = team.teamName;
			playerTeam.members = new List<ulong>();
			foreach (ProtoBuf.PlayerTeam.TeamMember member in team.members)
			{
				playerTeam.members.Add(member.userID);
			}
			teams[playerTeam.teamID] = playerTeam;
		}
		foreach (PlayerTeam value in teams.Values)
		{
			foreach (ulong member2 in value.members)
			{
				playerToTeam[member2] = value;
				BasePlayer basePlayer = FindByID(member2);
				if (basePlayer != null && basePlayer.currentTeam != value.teamID)
				{
					Debug.LogWarning($"Player {member2} has the wrong teamID: got {basePlayer.currentTeam}, expected {value.teamID}. Fixing automatically.");
					basePlayer.currentTeam = value.teamID;
				}
			}
		}
		foreach (ProtoBuf.RelationshipManager.PlayerRelationships relationship in info.msg.relationshipManager.relationships)
		{
			ulong playerID = relationship.playerID;
			PlayerRelationships playerRelationships = GetRelationships(playerID);
			playerRelationships.relations.Clear();
			foreach (ProtoBuf.RelationshipManager.PlayerRelationshipInfo relation in relationship.relations)
			{
				PlayerRelationshipInfo playerRelationshipInfo = new PlayerRelationshipInfo();
				playerRelationshipInfo.type = (RelationshipType)relation.type;
				playerRelationshipInfo.weight = relation.weight;
				playerRelationshipInfo.displayName = relation.displayName;
				playerRelationshipInfo.mugshotCrc = relation.mugshotCrc;
				playerRelationshipInfo.notes = relation.notes;
				playerRelationshipInfo.player = relation.playerID;
				playerRelationshipInfo.lastSeenTime = UnityEngine.Time.realtimeSinceStartup - relation.timeSinceSeen;
				playerRelationships.relations.Add(relation.playerID, playerRelationshipInfo);
			}
		}
	}
}
