namespace ConVar;

public class Steam
{
	[ReplicatedVar(Saved = true, ShowInAdminUI = true)]
	public static bool server_allow_steam_nicknames { get; set; } = true;

}
