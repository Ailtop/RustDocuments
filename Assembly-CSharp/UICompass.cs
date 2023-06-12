using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICompass : MonoBehaviour
{
	public RawImage compassStrip;

	public CanvasGroup compassGroup;

	public List<CompassMapMarker> CompassMarkers;

	public List<CompassMapMarker> TeamCompassMarkers;

	public List<CompassMissionMarker> MissionMarkers;

	public List<CompassMapMarker> LocalPings;

	public List<CompassMapMarker> TeamPings;

	public Image LeftPingPulse;

	public Image RightPingPulse;
}
