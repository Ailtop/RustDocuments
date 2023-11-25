using System;
using System.Collections.Generic;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using UnityEngine;

public static class NexusClanUtil
{
	public const string MotdVariable = "motd";

	public const string MotdAuthorVariable = "motd_author";

	public const string LogoVariable = "logo";

	public const string ColorVariable = "color";

	public const string CanSetLogoVariable = "can_set_logo";

	public const string CanSetMotdVariable = "can_set_motd";

	public const string CanSetPlayerNotesVariable = "can_set_player_notes";

	public const string PlayerNoteVariable = "notes";

	public static readonly List<VariableUpdate> DefaultLeaderVariables = new List<VariableUpdate>
	{
		new VariableUpdate("can_set_logo", bool.TrueString),
		new VariableUpdate("can_set_motd", bool.TrueString),
		new VariableUpdate("can_set_player_notes", bool.TrueString)
	};

	private static readonly Memoized<string, ulong> SteamIdToPlayerId = new Memoized<string, ulong>((ulong steamId) => steamId.ToString("G"));

	public static string GetPlayerId(ulong steamId)
	{
		return SteamIdToPlayerId.Get(steamId);
	}

	public static ulong GetSteamId(string playerId)
	{
		return ulong.Parse(playerId);
	}

	public static void GetMotd(this NexusClan clan, out string motd, out long motdTimestamp, out ulong motdAuthor)
	{
		if (!clan.TryGetVariable("motd", out var variable) || !clan.TryGetVariable("motd_author", out var variable2) || variable.Type != VariableType.String || variable2.Type != VariableType.String)
		{
			motd = null;
			motdTimestamp = 0L;
			motdAuthor = 0uL;
		}
		else
		{
			motd = variable.GetAsString();
			motdTimestamp = variable.LastUpdated * 1000;
			motdAuthor = GetSteamId(variable2.GetAsString());
		}
	}

	public static void GetBanner(this NexusClan clan, out byte[] logo, out Color32 color)
	{
		logo = ((clan.TryGetVariable("logo", out var variable) && variable.Type == VariableType.Binary) ? variable.GetAsBinary() : null);
		color = ((clan.TryGetVariable("color", out var variable2) && variable2.Type == VariableType.String) ? ColorEx.FromInt32(int.Parse(variable2.GetAsString())) : ((Color32)Color.white));
	}

	public static ClanRole ToClanRole(this NexusClanRole role)
	{
		bool flag = role.Rank == 1;
		ClanRole result = default(ClanRole);
		result.RoleId = role.RoleId;
		result.Rank = role.Rank;
		result.Name = role.Name;
		result.CanInvite = flag || role.CanInvite;
		result.CanKick = flag || role.CanKick;
		result.CanPromote = flag || role.CanPromote;
		result.CanDemote = flag || role.CanDemote;
		result.CanSetLogo = flag || (role.TryGetVariable("can_set_logo", out var variable) && ParseFlag(variable));
		result.CanSetMotd = flag || (role.TryGetVariable("can_set_motd", out var variable2) && ParseFlag(variable2));
		result.CanSetPlayerNotes = flag || (role.TryGetVariable("can_set_player_notes", out var variable3) && ParseFlag(variable3));
		result.CanAccessLogs = flag || role.CanAccessLogs;
		return result;
	}

	public static ClanMember ToClanMember(this NexusClanMember member)
	{
		member.TryGetVariable("notes", out var variable);
		ClanMember result = default(ClanMember);
		result.SteamId = GetSteamId(member.PlayerId);
		result.RoleId = member.RoleId;
		result.Joined = member.Joined * 1000;
		result.LastSeen = member.LastSeen * 1000;
		result.Notes = variable?.GetAsString();
		result.NotesTimestamp = variable?.LastUpdated ?? 0;
		return result;
	}

	public static ClanInvite ToClanInvite(this Facepunch.Nexus.Models.ClanInvite invite)
	{
		ClanInvite result = default(ClanInvite);
		result.SteamId = GetSteamId(invite.PlayerId);
		result.Recruiter = GetSteamId(invite.RecruiterPlayerId);
		result.Timestamp = invite.Created * 1000;
		return result;
	}

	public static ClanResult ToClanResult(this NexusClanResultCode result)
	{
		return result switch
		{
			NexusClanResultCode.Fail => ClanResult.Fail, 
			NexusClanResultCode.Success => ClanResult.Success, 
			NexusClanResultCode.NoClan => ClanResult.NoClan, 
			NexusClanResultCode.NotFound => ClanResult.NotFound, 
			NexusClanResultCode.NoPermission => ClanResult.NoPermission, 
			NexusClanResultCode.DuplicateName => ClanResult.DuplicateName, 
			NexusClanResultCode.RoleNotEmpty => ClanResult.RoleNotEmpty, 
			NexusClanResultCode.CannotSwapLeader => ClanResult.CannotSwapLeader, 
			NexusClanResultCode.CannotDeleteLeader => ClanResult.CannotDeleteLeader, 
			NexusClanResultCode.CannotKickLeader => ClanResult.CannotKickLeader, 
			NexusClanResultCode.CannotDemoteLeader => ClanResult.CannotDemoteLeader, 
			NexusClanResultCode.AlreadyInAClan => ClanResult.AlreadyInAClan, 
			_ => throw new NotSupportedException($"Cannot map NexusClanResultCode {result} to ClanResult"), 
		};
	}

	public static ClanRoleParameters ToRoleParameters(this ClanRole role)
	{
		ClanRoleParameters result = default(ClanRoleParameters);
		result.Name = role.Name;
		result.CanInvite = role.CanInvite;
		result.CanKick = role.CanKick;
		result.CanPromote = role.CanPromote;
		result.CanDemote = role.CanDemote;
		result.CanAccessLogs = role.CanAccessLogs;
		result.Variables = new List<VariableUpdate>(3)
		{
			FlagVariable("can_set_logo", role.CanSetLogo),
			FlagVariable("can_set_motd", role.CanSetMotd),
			FlagVariable("can_set_player_notes", role.CanSetPlayerNotes)
		};
		return result;
	}

	public static VariableUpdate FlagVariable(string key, bool value)
	{
		return new VariableUpdate(key, value ? bool.TrueString : bool.FalseString);
	}

	private static bool ParseFlag(Variable variable, bool defaultValue = false)
	{
		if ((object)variable == null || variable.Type != VariableType.String || !bool.TryParse(variable.GetAsString(), out var result))
		{
			return false;
		}
		return result;
	}
}
