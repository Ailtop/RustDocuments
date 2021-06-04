using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapView : FacepunchBehaviour
{
	public RawImage mapImage;

	public Image cameraPositon;

	public ScrollRectEx scrollRect;

	public GameObject monumentMarkerContainer;

	public Transform clusterMarkerContainer;

	public GameObjectRef monumentMarkerPrefab;

	public TeamMemberMapMarker[] teamPositions;

	public PointOfInterestMapMarker PointOfInterestMarker;

	public PointOfInterestMapMarker LeaderPointOfInterestMarker;

	public GameObject PlayerDeathMarker;

	public List<SleepingBagMapMarker> SleepingBagMarkers = new List<SleepingBagMapMarker>();

	public List<SleepingBagClusterMapMarker> SleepingBagClusters = new List<SleepingBagClusterMapMarker>();

	public RawImage TrainLayer;

	public bool ShowGrid;

	public bool ShowPointOfInterestMarkers;

	public bool ShowDeathMarker = true;

	public bool ShowSleepingBags = true;

	public bool ShowLocalPlayer = true;

	public bool ShowTeamMembers = true;

	public bool ShowTrainLayer;
}
