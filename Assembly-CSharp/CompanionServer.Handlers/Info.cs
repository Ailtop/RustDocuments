using CompanionServer.Cameras;
using ConVar;
using Facepunch;
using Facepunch.Math;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class Info : BasePlayerHandler<AppEmpty>
{
	public override void Execute()
	{
		AppInfo appInfo = Facepunch.Pool.Get<AppInfo>();
		appInfo.name = ConVar.Server.hostname;
		appInfo.headerImage = ConVar.Server.headerimage;
		appInfo.logoImage = ConVar.Server.logoimage;
		appInfo.url = ConVar.Server.url;
		appInfo.map = World.Name;
		appInfo.mapSize = World.Size;
		appInfo.wipeTime = (uint)Epoch.FromDateTime(SaveRestore.SaveCreatedTime.ToUniversalTime());
		appInfo.players = (uint)BasePlayer.activePlayerList.Count;
		appInfo.maxPlayers = (uint)ConVar.Server.maxplayers;
		appInfo.queuedPlayers = (uint)SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued;
		appInfo.seed = World.Seed;
		appInfo.camerasEnabled = CameraRenderer.enabled;
		if (NexusServer.Started)
		{
			int? nexusId = NexusServer.NexusId;
			string zoneKey = NexusServer.ZoneKey;
			if (nexusId.HasValue && zoneKey != null)
			{
				appInfo.nexus = Nexus.endpoint;
				appInfo.nexusId = nexusId.Value;
				appInfo.nexusZone = zoneKey;
			}
		}
		AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
		appResponse.info = appInfo;
		Send(appResponse);
	}
}
