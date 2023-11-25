using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer;

public static class AppPlayerExtensions
{
	public static AppTeamInfo GetAppTeamInfo(this BasePlayer player, ulong steamId)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		appTeamInfo.members = Pool.GetList<AppTeamInfo.Member>();
		AppTeamInfo.Member member = Pool.Get<AppTeamInfo.Member>();
		if (player != null)
		{
			Vector2 vector = Util.WorldToMap(player.transform.position);
			member.steamId = player.userID;
			member.name = player.displayName ?? "";
			member.x = vector.x;
			member.y = vector.y;
			member.isOnline = player.IsConnected;
			member.spawnTime = player.lifeStory?.timeBorn ?? 0;
			member.isAlive = player.IsAlive();
			member.deathTime = player.previousLifeStory?.timeDied ?? 0;
		}
		else
		{
			member.steamId = steamId;
			member.name = SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(steamId) ?? "";
			member.x = 0f;
			member.y = 0f;
			member.isOnline = false;
			member.spawnTime = 0u;
			member.isAlive = false;
			member.deathTime = 0u;
		}
		appTeamInfo.members.Add(member);
		appTeamInfo.leaderSteamId = 0uL;
		appTeamInfo.mapNotes = GetMapNotes(member.steamId, personalNotes: true);
		appTeamInfo.leaderMapNotes = Pool.GetList<AppTeamInfo.Note>();
		return appTeamInfo;
	}

	public static AppTeamInfo GetAppTeamInfo(this RelationshipManager.PlayerTeam team, ulong requesterSteamId)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		appTeamInfo.members = Pool.GetList<AppTeamInfo.Member>();
		for (int i = 0; i < team.members.Count; i++)
		{
			ulong num = team.members[i];
			BasePlayer basePlayer = RelationshipManager.FindByID(num);
			if (!basePlayer)
			{
				basePlayer = null;
			}
			Vector2 vector = Util.WorldToMap(basePlayer?.transform.position ?? Vector3.zero);
			AppTeamInfo.Member member = Pool.Get<AppTeamInfo.Member>();
			member.steamId = num;
			member.name = basePlayer?.displayName ?? SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num) ?? "";
			member.x = vector.x;
			member.y = vector.y;
			member.isOnline = basePlayer?.IsConnected ?? false;
			member.spawnTime = (basePlayer?.lifeStory?.timeBorn).GetValueOrDefault();
			member.isAlive = basePlayer?.IsAlive() ?? false;
			member.deathTime = (basePlayer?.previousLifeStory?.timeDied).GetValueOrDefault();
			appTeamInfo.members.Add(member);
		}
		appTeamInfo.leaderSteamId = team.teamLeader;
		appTeamInfo.mapNotes = GetMapNotes(requesterSteamId, personalNotes: true);
		if (requesterSteamId != team.teamLeader)
		{
			appTeamInfo.leaderMapNotes = GetMapNotes(team.teamLeader, personalNotes: false);
		}
		else
		{
			appTeamInfo.leaderMapNotes = Pool.GetList<AppTeamInfo.Note>();
		}
		return appTeamInfo;
	}

	private static List<AppTeamInfo.Note> GetMapNotes(ulong playerId, bool personalNotes)
	{
		List<AppTeamInfo.Note> list = Pool.GetList<AppTeamInfo.Note>();
		PlayerState playerState = SingletonComponent<ServerMgr>.Instance.playerStateManager.Get(playerId);
		if (playerState != null)
		{
			if (personalNotes && playerState.deathMarker != null)
			{
				AddMapNote(list, playerState.deathMarker, BasePlayer.MapNoteType.Death);
			}
			if (playerState.pointsOfInterest != null)
			{
				foreach (MapNote item in playerState.pointsOfInterest)
				{
					AddMapNote(list, item, BasePlayer.MapNoteType.PointOfInterest);
				}
			}
		}
		return list;
	}

	private static void AddMapNote(List<AppTeamInfo.Note> result, MapNote note, BasePlayer.MapNoteType type)
	{
		Vector2 vector = Util.WorldToMap(note.worldPosition);
		AppTeamInfo.Note note2 = Pool.Get<AppTeamInfo.Note>();
		note2.type = (int)type;
		note2.x = vector.x;
		note2.y = vector.y;
		note2.icon = note.icon;
		note2.colourIndex = note.colourIndex;
		note2.label = note.label;
		result.Add(note2);
	}
}
