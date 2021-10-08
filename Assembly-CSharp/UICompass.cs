using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
	public RawImage compassStrip;

	public CanvasGroup compassGroup;

	public CompassMapMarker CompassMarker;

	public CompassMapMarker TeamLeaderCompassMarker;

	public List<CompassMissionMarker> MissionMarkers;
}
