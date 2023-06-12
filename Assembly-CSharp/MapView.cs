using System.Collections.Generic;
using Rust.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MapView : FacepunchBehaviour
{
	public RawImage mapImage;

	public Image cameraPositon;

	public ScrollRectEx scrollRect;

	public GameObject monumentMarkerContainer;

	public Transform clusterMarkerContainer;

	public GameObjectRef monumentMarkerPrefab;

	public GameObject missionMarkerContainer;

	public GameObjectRef missionMarkerPrefab;

	public Transform activeInteractionParent;

	public Transform localPlayerInterestPointRoot;

	public TeamMemberMapMarker[] teamPositions;

	public List<PointOfInterestMapMarker> PointOfInterestMarkers;

	public List<PointOfInterestMapMarker> TeamPointOfInterestMarkers;

	public List<PointOfInterestMapMarker> LocalPings;

	public List<PointOfInterestMapMarker> TeamPings;

	public GameObject PlayerDeathMarker;

	public List<SleepingBagMapMarker> SleepingBagMarkers = new List<SleepingBagMapMarker>();

	public List<SleepingBagClusterMapMarker> SleepingBagClusters = new List<SleepingBagClusterMapMarker>();

	[FormerlySerializedAs("TrainLayer")]
	public RawImage UndergroundLayer;

	public bool ShowGrid;

	public bool ShowPointOfInterestMarkers;

	public bool ShowDeathMarker = true;

	public bool ShowSleepingBags = true;

	public bool AllowSleepingBagDeletion;

	public bool ShowLocalPlayer = true;

	public bool ShowTeamMembers = true;

	public bool ShowTrainLayer;

	public bool ShowMissions;

	[FormerlySerializedAs("ShowTrainLayer")]
	public bool ShowUndergroundLayers;

	public bool MLRSMarkerMode;

	public RustImageButton LockButton;

	public RustImageButton OverworldButton;

	public RustImageButton TrainButton;

	public RustImageButton[] UnderwaterButtons;

	public RustImageButton DungeonButton;
}
