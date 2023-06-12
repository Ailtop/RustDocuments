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

	public Toggle showGridToggle;

	public Button FocusButton;

	public CanvasGroup CanvasGroup;

	public SoundDefinition PlaceMarkerSound;

	public SoundDefinition ClearMarkerSound;

	public MapView View;

	public Color[] PointOfInterestColours;

	public PointOfInterestSpriteConfig[] PointOfInterestSprites;

	public Sprite PingBackground;

	public bool DebugStayOpen;

	public GameObjectRef MarkerListPrefab;

	public GameObject MarkerHeader;

	public Transform LocalPlayerMarkerListParent;

	public Transform TeamMarkerListParent;

	public GameObject TeamLeaderHeader;

	public RustButton HideTeamLeaderMarkersToggle;

	public CanvasGroup TeamMarkersCanvas;

	public RustImageButton ShowSleepingBagsButton;
}
