using Rust.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINexusMap : BaseMonoBehaviour, IPointerDownHandler, IEventSystemHandler
{
	private static readonly Memoized<string, int> IntMemoized = new Memoized<string, int>((int i) => i.ToString());

	public RawImage BackdropImage;

	public RawImage BackgroundImage;

	public RectTransform LoadingView;

	public RectTransform MissingView;

	public ScrollRectEx MapScrollRect;

	public ScrollRectZoom MapScrollZoom;

	public Image CameraPositon;

	public CanvasGroup ZoneNameCanvasGroup;

	public RectTransform ZoneNameContainer;

	public GameObjectRef ZoneNamePrefab;

	[Header("Zone Details")]
	public CanvasGroup ZoneDetails;

	public RustText ZoneName;

	public RustText OnlineCount;

	public RustText MaxCount;

	public GameObject InboundFerriesSection;

	public RectTransform InboundFerriesList;

	public GameObject OutboundFerriesSection;

	public RectTransform OutboundFerriesList;

	public GameObject ConnectionsSection;

	public RectTransform ConnectionsList;

	[Header("Behavior")]
	public bool ShowLocalPlayer;

	public float OutOfBoundsScaleFactor = 5f;

	public float ZoneNameAlphaPower = 100f;

	public UnityEvent OnClicked;

	public void OnPointerDown(PointerEventData eventData)
	{
	}
}
