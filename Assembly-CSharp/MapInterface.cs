using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class MapInterface : SingletonComponent<MapInterface>
{
	[Serializable]
	public struct PointOfInterestSpriteConfig
	{
		public Sprite inner;

		public Sprite outer;
	}

	public static bool IsOpen;

	public Image cameraPositon;

	public ScrollRectEx scrollRect;

	public RustButton showGridToggle;

	public RustButton FocusButton;

	public CanvasGroup CanvasGroup;

	public SoundDefinition PlaceMarkerSound;

	public SoundDefinition ClearMarkerSound;

	public MapView View;

	public UINexusMap NexusMap;

	public GameObject NexusButtonGroup;

	public RustButton NexusToggle;

	public Color[] PointOfInterestColours;

	public PointOfInterestSpriteConfig[] PointOfInterestSprites;

	public Sprite PingBackground;

	public bool DebugStayOpen;

	public GameObject MarkerListSection;

	public GameObjectRef MarkerListPrefab;

	public GameObject MarkerHeader;

	public Transform LocalPlayerMarkerListParent;

	public Transform TeamMarkerListParent;

	public GameObject TeamLeaderHeader;

	public RustButton HideTeamLeaderMarkersToggle;

	public CanvasGroup TeamMarkersCanvas;

	public RustImageButton ShowSleepingBagsButton;

	public RustImageButton ShowVendingMachinesButton;
}
