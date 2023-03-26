using UnityEngine;
using UnityEngine.UI;

public class RCMenu : ComputerMenu
{
	public Image backgroundOpaque;

	public InputField newBookmarkEntryField;

	public NeedsCursor needsCursor;

	public float hiddenOffset = -256f;

	public RectTransform devicesPanel;

	private Vector3 initialDevicesPosition;

	public static bool isControllingCamera;

	public CanvasGroup overExposure;

	public CanvasGroup interference;

	public float interferenceFadeDuration = 0.2f;

	public float rangeInterferenceScale = 10000f;

	public Text timeText;

	public Text watchedDurationText;

	public Text deviceNameText;

	public Text noSignalText;

	public SoundDefinition bookmarkPressedSoundDef;

	public GameObject[] hideIfStatic;

	public GameObject readOnlyIndicator;

	public GameObject crosshair;

	public float fogOverrideDensity = 0.1f;

	public float autoTurretFogDistance = 30f;

	public float autoTurretDotBaseScale = 2f;

	public float autoTurretDotGrowScale = 4f;
}
