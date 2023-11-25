using ProtoBuf;

namespace CompanionServer.Handlers;

public class PromoteToLeader : BasePlayerHandler<AppPromoteToLeader>
{
	public override void Execute()
	{
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam == null)
		{
			SendError("no_team");
			return;
		}
		if (playerTeam.teamLeader != base.UserId)
		{
			SendError("access_denied");
			return;
		}
		if (playerTeam.teamLeader == base.Proto.steamId)
		{
			SendSuccess();
			return;
		}
		if (!playerTeam.members.Contains(base.Proto.steamId))
		{
			SendError("not_found");
			return;
		}
		playerTeam.SetTeamLeader(base.Proto.steamId);
		SendSuccess();
	}
}
