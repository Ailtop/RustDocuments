using System.Collections.Generic;
using Facepunch;
using ProtoBuf;

namespace CompanionServer.Handlers;

public class TeamChat : BasePlayerHandler<AppEmpty>
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
		appResponse.teamChat.messages = Pool.GetList<AppTeamMessage>();
		IReadOnlyList<ChatLog.Entry> history = Server.TeamChat.GetHistory(playerTeam.teamID);
		if (history != null)
		{
			foreach (ChatLog.Entry item in history)
			{
				AppTeamMessage appTeamMessage = Pool.Get<AppTeamMessage>();
				appTeamMessage.steamId = item.SteamId;
				appTeamMessage.name = item.Name;
				appTeamMessage.message = item.Message;
				appTeamMessage.color = item.Color;
				appTeamMessage.time = item.Time;
				appResponse.teamChat.messages.Add(appTeamMessage);
			}
		}
		Send(appResponse);
	}
}
