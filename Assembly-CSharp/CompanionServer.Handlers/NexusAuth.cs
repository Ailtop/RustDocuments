using System.Globalization;
using ConVar;
using Facepunch;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class NexusAuth : BaseHandler<AppGetNexusAuth>
{
	public override ValidationResult Validate()
	{
		if (!NexusServer.Started)
		{
			return ValidationResult.NotFound;
		}
		return base.Validate();
	}

	public override async void Execute()
	{
		if (base.Request.playerId == 0L)
		{
			SendError("invalid_playerid");
			return;
		}
		string playerId = base.Request.playerId.ToString("G", CultureInfo.InvariantCulture);
		NexusPlayer nexusPlayer = await NexusServer.ZoneClient.GetPlayer(playerId);
		if (nexusPlayer == null || !nexusPlayer.TryGetVariable("appKey", out var variable) || variable.Type != VariableType.String || base.Proto.appKey != variable.GetAsString())
		{
			SendError("access_denied");
			return;
		}
		AppResponse appResponse = Facepunch.Pool.Get<AppResponse>();
		appResponse.nexusAuth = Facepunch.Pool.Get<AppNexusAuth>();
		appResponse.nexusAuth.serverId = App.serverid;
		appResponse.nexusAuth.playerToken = SingletonComponent<ServerMgr>.Instance.persistance.GetOrGenerateAppToken(base.Request.playerId, out var _);
		Send(appResponse);
	}
}
