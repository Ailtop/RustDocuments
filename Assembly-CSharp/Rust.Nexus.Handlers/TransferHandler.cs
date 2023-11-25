using System;
using System.Collections.Generic;
using ConVar;
using Network;
using ProtoBuf;
using ProtoBuf.Nexus;
using UnityEngine;

namespace Rust.Nexus.Handlers;

public class TransferHandler : BaseNexusRequestHandler<TransferRequest>
{
	private static readonly Dictionary<ulong, ulong> UidMapping = new Dictionary<ulong, ulong>();

	private static readonly Dictionary<BaseEntity, ProtoBuf.Entity> EntityToSpawn = new Dictionary<BaseEntity, ProtoBuf.Entity>();

	private static readonly Dictionary<ulong, BasePlayer> SpawnedPlayers = new Dictionary<ulong, BasePlayer>();

	private static readonly List<string> PlayerIds = new List<string>();

	private static readonly List<NetworkableId> EntitiesToProtect = new List<NetworkableId>();

	private static readonly Dictionary<ulong, RelationshipManager.PlayerTeam> TeamMapping = new Dictionary<ulong, RelationshipManager.PlayerTeam>();

	protected override void Handle()
	{
		UidMapping.Clear();
		base.Request.InspectUids(UpdateWithNewUid);
		PlayerIds.Clear();
		EntitiesToProtect.Clear();
		foreach (ProtoBuf.Entity entity in base.Request.entities)
		{
			if (entity.basePlayer != null)
			{
				ulong userid = entity.basePlayer.userid;
				Debug.Log($"Found player {userid} in transfer");
				PlayerIds.Add(userid.ToString("G"));
				BasePlayer basePlayer = BasePlayer.FindByID(userid) ?? BasePlayer.FindSleeping(userid);
				if (basePlayer != null)
				{
					if (basePlayer.IsConnected)
					{
						basePlayer.Kick("Player transfer is overwriting you - contact developers!");
					}
					basePlayer.Kill();
				}
				entity.basePlayer.currentTeam = 0uL;
				RelationshipManager.ServerInstance.FindPlayersTeam(userid)?.RemovePlayer(userid);
				if ((entity.basePlayer.playerFlags & 0x10) == 0)
				{
					entity.basePlayer.playerFlags |= 33554432;
				}
				if (entity.basePlayer.loadingTimeout <= 0f || entity.basePlayer.loadingTimeout > ConVar.Nexus.loadingTimeout)
				{
					entity.basePlayer.loadingTimeout = ConVar.Nexus.loadingTimeout;
				}
			}
			if (entity.baseCombat != null && entity.baseNetworkable != null)
			{
				EntitiesToProtect.Add(entity.baseNetworkable.uid);
			}
		}
		RepositionEntitiesFromTransfer();
		SpawnedPlayers.Clear();
		SpawnEntities(SpawnedPlayers);
		foreach (NetworkableId item in EntitiesToProtect)
		{
			if (BaseNetworkable.serverEntities.Find(item) is BaseEntity baseEntity)
			{
				baseEntity.EnableTransferProtection();
			}
		}
		TeamMapping.Clear();
		foreach (PlayerSecondaryData secondaryDatum in base.Request.secondaryData)
		{
			if (!SpawnedPlayers.TryGetValue(secondaryDatum.userId, out var value))
			{
				Debug.LogError($"Got secondary data for {secondaryDatum.userId} but they were not spawned in the transfer");
				continue;
			}
			value.LoadSecondaryData(secondaryDatum);
			if (secondaryDatum.isTeamLeader && secondaryDatum.teamId != 0L && !TeamMapping.ContainsKey(secondaryDatum.teamId))
			{
				RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.CreateTeam();
				playerTeam.teamLeader = value.userID;
				playerTeam.AddPlayer(value);
				TeamMapping.Add(secondaryDatum.teamId, playerTeam);
			}
		}
		foreach (PlayerSecondaryData secondaryDatum2 in base.Request.secondaryData)
		{
			if (SpawnedPlayers.TryGetValue(secondaryDatum2.userId, out var value2) && secondaryDatum2.teamId != 0L && !secondaryDatum2.isTeamLeader)
			{
				if (TeamMapping.TryGetValue(secondaryDatum2.teamId, out var value3))
				{
					value3.AddPlayer(value2);
					continue;
				}
				RelationshipManager.PlayerTeam playerTeam2 = RelationshipManager.ServerInstance.CreateTeam();
				playerTeam2.teamLeader = value2.userID;
				playerTeam2.AddPlayer(value2);
				TeamMapping.Add(secondaryDatum2.teamId, playerTeam2);
			}
		}
		if (PlayerIds.Count > 0)
		{
			Debug.Log("Completing transfers for players: " + string.Join(", ", PlayerIds));
			CompleteTransfers();
		}
		static void UpdateWithNewUid(UidType type, ref ulong prevUid)
		{
			if (type == UidType.Clear)
			{
				prevUid = 0uL;
			}
			else if (prevUid != 0L)
			{
				if (!UidMapping.TryGetValue(prevUid, out var value4))
				{
					value4 = Network.Net.sv.TakeUID();
					UidMapping.Add(prevUid, value4);
				}
				prevUid = value4;
			}
		}
	}

	private static async void CompleteTransfers()
	{
		try
		{
			await NexusServer.ZoneClient.CompleteTransfers(PlayerIds);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	private void RepositionEntitiesFromTransfer()
	{
		ProtoBuf.Entity entity = base.Request.entities[0];
		Vector3 pos = entity.baseEntity.pos;
		Quaternion rotation = Quaternion.Euler(entity.baseEntity.rot);
		(Vector3 Position, Quaternion Rotation, bool PreserveY) tuple = ZoneController.Instance.ChooseTransferDestination(base.FromZone.Key, base.Request.method, base.Request.from, base.Request.to, pos, rotation);
		var (vector, quaternion, _) = tuple;
		if (tuple.PreserveY)
		{
			vector.y = pos.y;
		}
		Vector3 vector2 = vector - pos;
		Quaternion quaternion2 = quaternion * Quaternion.Inverse(rotation);
		foreach (ProtoBuf.Entity entity2 in base.Request.entities)
		{
			if (entity2.baseEntity != null && (entity2.parent == null || !entity2.parent.uid.IsValid))
			{
				entity2.baseEntity.pos += vector2;
				entity2.baseEntity.rot = (Quaternion.Euler(entity2.baseEntity.rot) * quaternion2).eulerAngles;
			}
		}
	}

	private void SpawnEntities(Dictionary<ulong, BasePlayer> players)
	{
		Application.isLoadingSave = true;
		try
		{
			EntityToSpawn.Clear();
			foreach (ProtoBuf.Entity entity in base.Request.entities)
			{
				BaseEntity baseEntity = GameManager.server.CreateEntity(StringPool.Get(entity.baseNetworkable.prefabID), entity.baseEntity.pos, Quaternion.Euler(entity.baseEntity.rot));
				if (baseEntity != null)
				{
					baseEntity.InitLoad(entity.baseNetworkable.uid);
					EntityToSpawn.Add(baseEntity, entity);
				}
			}
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item in EntityToSpawn)
			{
				BaseEntity key = item.Key;
				if (!(key == null))
				{
					key.Spawn();
					key.Load(new BaseNetworkable.LoadInfo
					{
						fromDisk = true,
						fromTransfer = true,
						msg = item.Value
					});
				}
			}
			foreach (KeyValuePair<BaseEntity, ProtoBuf.Entity> item2 in EntityToSpawn)
			{
				BaseEntity key2 = item2.Key;
				if (!(key2 == null))
				{
					key2.UpdateNetworkGroup();
					key2.PostServerLoad();
					if (key2 is BasePlayer basePlayer)
					{
						players[basePlayer.userID] = basePlayer;
					}
				}
			}
		}
		finally
		{
			Application.isLoadingSave = false;
		}
	}
}
