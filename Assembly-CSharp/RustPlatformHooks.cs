using System;
using System.Net;
using ConVar;
using Facepunch;
using Network;
using Rust;
using Rust.Platform.Common;

public class RustPlatformHooks : IPlatformHooks
{
	public static readonly RustPlatformHooks Instance = new RustPlatformHooks();

	public uint SteamAppId => Rust.Defines.appID;

	public ServerParameters? ServerParameters
	{
		get
		{
			if (Network.Net.sv == null)
			{
				return null;
			}
			IPAddress address = null;
			if (!string.IsNullOrEmpty(ConVar.Server.ip))
			{
				address = IPAddress.Parse(ConVar.Server.ip);
			}
			if (ConVar.Server.queryport <= 0 || ConVar.Server.queryport == ConVar.Server.port)
			{
				ConVar.Server.queryport = Math.Max(ConVar.Server.port, RCon.Port) + 1;
			}
			return new ServerParameters("rust", "Rust", 2392.ToString(), ConVar.Server.secure, CommandLine.HasSwitch("-sdrnet"), address, (ushort)Network.Net.sv.port, (ushort)ConVar.Server.queryport);
		}
	}

	public void Abort()
	{
		Rust.Application.Quit();
	}

	public void OnItemDefinitionsChanged()
	{
		ItemManager.InvalidateWorkshopSkinCache();
	}

	public void AuthSessionValidated(ulong userId, ulong ownerUserId, AuthResponse response)
	{
		SingletonComponent<ServerMgr>.Instance.OnValidateAuthTicketResponse(userId, ownerUserId, response);
	}
}
