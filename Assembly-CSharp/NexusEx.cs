using System;
using System.Collections.Generic;
using Facepunch.Nexus.Models;

public class NexusEx : Nexus
{
	public static readonly char[] SplitComma = new char[1] { ',' };

	public string Key { get; }

	public HashSet<string> TagsSet { get; }

	public NexusEx(string endpoint, Nexus nexus)
	{
		base.NexusId = nexus.NexusId;
		base.Name = nexus.Name;
		base.LastReset = nexus.LastReset;
		base.ZoneCount = nexus.ZoneCount;
		base.MaxPlayers = nexus.MaxPlayers;
		base.OnlinePlayers = nexus.OnlinePlayers;
		base.QueuedPlayers = nexus.QueuedPlayers;
		base.Build = nexus.Build;
		base.Protocol = nexus.Protocol;
		base.Tags = nexus.Tags;
		Key = $"{endpoint}#{nexus.NexusId}";
		string[] collection = base.Tags?.Split(SplitComma, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
		TagsSet = new HashSet<string>(collection, StringComparer.OrdinalIgnoreCase);
	}
}
