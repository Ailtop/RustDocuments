using ConVar;
using Facepunch;
using ProtoBuf.Nexus;

namespace Rust.Nexus.Handlers;

public class PingHandler : BaseNexusRequestHandler<PingRequest>
{
	protected override void Handle()
	{
		Response response = BaseNexusRequestHandler<PingRequest>.NewResponse();
		response.ping = Facepunch.Pool.Get<PingResponse>();
		response.ping.players = BasePlayer.activePlayerList.Count;
		response.ping.maxPlayers = ConVar.Server.maxplayers;
		response.ping.queuedPlayers = SingletonComponent<ServerMgr>.Instance.connectionQueue.Queued;
		SendSuccess(response);
	}
}
