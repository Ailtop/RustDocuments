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
		appTeamInfo.mapNotes = GetMapNotes(player, personalNotes: true);
		appTeamInfo.leaderMapNotes = GetMapNotes(null, personalNotes: false);
		return appTeamInfo;
	}

	public static AppTeamInfo GetAppTeamInfo(this RelationshipManager.PlayerTeam team, ulong requesterSteamId)
	{
		AppTeamInfo appTeamInfo = Pool.Get<AppTeamInfo>();
		appTeamInfo.members = Pool.GetList<AppTeamInfo.Member>();
		BasePlayer player = null;
		BasePlayer basePlayer = null;
		for (int i = 0; i < team.members.Count; i++)
		{
			ulong num = team.members[i];
			BasePlayer basePlayer2 = RelationshipManager.FindByID(num);
			if (!basePlayer2)
			{
				basePlayer2 = null;
			}
			if (num == requesterSteamId)
			{
				player = basePlayer2;
			}
			if (num == team.teamLeader)
			{
				basePlayer = basePlayer2;
			}
			Vector2 vector = Util.WorldToMap(basePlayer2?.transform.position ?? Vector3.zero);
			AppTeamInfo.Member member = Pool.Get<AppTeamInfo.Member>();
			member.steamId = num;
			member.name = basePlayer2?.displayName ?? SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(num) ?? "";
			member.x = vector.x;
			member.y = vector.y;
			member.isOnline = basePlayer2?.IsConnected ?? false;
			member.spawnTime = basePlayer2?.lifeStory?.timeBorn ?? 0;
			member.isAlive = basePlayer2?.IsAlive() ?? false;
			member.deathTime = basePlayer2?.previousLifeStory?.timeDied ?? 0;
			appTeamInfo.members.Add(member);
		}
		appTeamInfo.leaderSteamId = team.teamLeader;
		appTeamInfo.mapNotes = GetMapNotes(player, personalNotes: true);
		appTeamInfo.leaderMapNotes = GetMapNotes((requesterSteamId != team.teamLeader) ? basePlayer : null, personalNotes: false);
		return appTeamInfo;
	}

	private static List<AppTeamInfo.Note> GetMapNotes(BasePlayer player, bool personalNotes)
	{
		List<AppTeamInfo.Note> list = Pool.GetList<AppTeamInfo.Note>();
		if (player != null)
		{
			if (personalNotes && player.ServerCurrentDeathNote != null)
			{
				AddMapNote(list, player.ServerCurrentDeathNote, BasePlayer.MapNoteType.Death);
			}
			if (player.ServerCurrentMapNote != null)
			{
				AddMapNote(list, player.ServerCurrentMapNote, BasePlayer.MapNoteType.PointOfInterest);
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
		result.Add(note2);
	}
}
