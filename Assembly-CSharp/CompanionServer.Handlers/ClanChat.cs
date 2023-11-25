using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class ClanChat : BaseClanHandler<AppEmpty>
{
	public override async void Execute()
	{
		IClan clan = await GetClan();
		if (clan == null)
		{
			SendError("no_clan");
			return;
		}
		ClanValueResult<ClanChatScrollback> clanValueResult = await clan.GetChatScrollback();
		if (!clanValueResult.IsSuccess)
		{
			SendError(clanValueResult.Result);
			return;
		}
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.clanChat = Pool.Get<AppClanChat>();
		appResponse.clanChat.messages = Pool.GetList<AppClanMessage>();
		foreach (ClanChatEntry entry in clanValueResult.Value.Entries)
		{
			AppClanMessage appClanMessage = Pool.Get<AppClanMessage>();
			appClanMessage.steamId = entry.SteamId;
			appClanMessage.name = entry.Name;
			appClanMessage.message = entry.Message;
			appClanMessage.time = entry.Time;
			appResponse.clanChat.messages.Add(appClanMessage);
		}
		Send(appResponse);
	}
}
