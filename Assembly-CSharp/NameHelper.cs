using System.Collections.Generic;

public static class NameHelper
{
	private static Dictionary<string, string> _cache = new Dictionary<string, string>();

	public static string Get(ulong userId, string name)
	{
		return name;
	}

	public static string Get(IPlayerInfo playerInfo)
	{
		return Get(playerInfo.UserId, playerInfo.UserName);
	}
}
