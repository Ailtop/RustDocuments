using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace CompanionServer.Handlers;

public class MapMarkers : BaseHandler<AppEmpty>
{
	public override void Execute()
	{
		AppMapMarkers appMapMarkers = Pool.Get<AppMapMarkers>();
		appMapMarkers.markers = Pool.GetList<AppMarker>();
		RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(base.UserId);
		if (playerTeam != null)
		{
			foreach (ulong member in playerTeam.members)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(member);
				if (!(basePlayer == null))
				{
					appMapMarkers.markers.Add(GetPlayerMarker(basePlayer));
				}
			}
		}
		else if (base.Player != null)
		{
			appMapMarkers.markers.Add(GetPlayerMarker(base.Player));
		}
		foreach (MapMarker serverMapMarker in MapMarker.serverMapMarkers)
		{
			if (serverMapMarker.appType != 0)
			{
				appMapMarkers.markers.Add(serverMapMarker.GetAppMarkerData());
			}
		}
		AppResponse appResponse = Pool.Get<AppResponse>();
		appResponse.mapMarkers = appMapMarkers;
		Send(appResponse);
	}

	private static AppMarker GetPlayerMarker(BasePlayer player)
	{
		AppMarker appMarker = Pool.Get<AppMarker>();
		Vector2 vector = Util.WorldToMap(player.transform.position);
		appMarker.id = player.net.ID;
		appMarker.type = AppMarkerType.Player;
		appMarker.x = vector.x;
		appMarker.y = vector.y;
		appMarker.steamId = player.userID;
		return appMarker;
	}
}
