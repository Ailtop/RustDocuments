public static class NameHelper
{
	public static string Get(ulong userId, string name, bool isClient = true)
	{
		return name;
	}

	public static string Get(IPlayerInfo playerInfo, bool isClient = true)
	{
		return Get(playerInfo.UserId, playerInfo.UserName, isClient);
	}
}
