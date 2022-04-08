using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class TeamChat : BaseHandler<AppEmpty>
{
	public override void Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam == null)
		{
			SendError("no_team");
			return;
		}
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.teamChat = Pool.Get<AppTeamChat>();
		appResponse.teamChat.messages = Pool.GetList<AppChatMessage>();
		IReadOnlyList<ChatLog.Entry> history = Server.TeamChat.GetHistory(playerTeam.teamID);
		if (history != null)
		{
			foreach (ChatLog.Entry item in history)
			{
				AppChatMessage appChatMessage = Pool.Get<AppChatMessage>();
				appChatMessage.steamId = item.SteamId;
				appChatMessage.name = item.Name;
				appChatMessage.message = item.Message;
				appChatMessage.color = item.Color;
				appChatMessage.time = item.Time;
				appResponse.teamChat.messages.Add(appChatMessage);
			}
		}
		Send(appResponse);
	}
}
