using System;
using System.Collections.Generic;
using System.Linq;
using Facepunch;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
public class PieMenu : UIBehaviour
{
	[Serializable]
	public class MenuOption
	{
		public struct ColorMode
		{
			public enum PieMenuSpriteColorOption
			{
				CustomColor = 0,
				SpriteColor = 1
			}

			public PieMenuSpriteColorOption Mode;

			public Color CustomColor;
		}

		public string name;

		public string desc;

		public string requirements;

		public Sprite sprite;

		public bool disabled;

		public int order;

		public ColorMode? overrideColorMode;

		public bool showOverlay;

		[NonSerialized]
		public Action<BasePlayer> action;

		[NonSerialized]
		public Action<BasePlayer> actionPrev;

		[NonSerialized]
		public Action<BasePlayer> actionNext;

		[NonSerialized]
		public PieOption option;

		[NonSerialized]
		public bool selected;

		[NonSerialized]
		public bool allowMerge;

		[NonSerialized]
		public bool wantsMerge;
	}

	public static PieMenu Instance;

	public Image middleBox;

	public PieShape pieBackgroundBlur;

	public PieShape pieBackground;

	public PieShape pieSelection;

	public GameObject pieOptionPrefab;

	public GameObject optionsCanvas;

	public MenuOption[] options;

	public GameObject scaleTarget;

	public GameObject arrowLeft;

	public GameObject arrowRight;

	public float sliceGaps = 10f;

	[Range(0f, 1f)]
	public float outerSize = 1f;

	[Range(0f, 1f)]
	public float innerSize = 0.5f;

	[Range(0f, 1f)]
	public float iconSize = 0.8f;

	[Range(0f, 360f)]
	public float startRadius;

	[Range(0f, 360f)]
	public float radiusSize = 360f;

	public Image middleImage;

	public TextMeshProUGUI middleTitle;

	public TextMeshProUGUI middleDesc;

	public TextMeshProUGUI middleRequired;

	public Color colorIconActive;

	public Color colorIconHovered;

	public Color colorIconDisabled;

	public Color colorBackgroundDisabled;

	public SoundDefinition clipOpen;

	public SoundDefinition clipCancel;

	public SoundDefinition clipChanged;

	public SoundDefinition clipSelected;

	public MenuOption defaultOption;

	private bool isClosing;

	private CanvasGroup canvasGroup;

	public Material IconMaterial;

	internal MenuOption selectedOption;

	private static Color pieSelectionColor = new Color(0.804f, 0.255f, 0.169f, 1f);

	private static Color middleImageColor = new Color(0.804f, 0.255f, 0.169f, 0.784f);

	private static AnimationCurve easePunch = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.112586f, 0.9976035f), new Keyframe(0.3120486f, 0.01720615f), new Keyframe(0.4316337f, 0.17030682f), new Keyframe(0.5524869f, 0.03141804f), new Keyframe(0.6549395f, 0.002909959f), new Keyframe(0.770987f, 0.009817753f), new Keyframe(0.8838775f, 0.001939224f), new Keyframe(1f, 0f));

	public bool IsOpen { get; private set; }

	protected override void Start()
	{
		base.Start();
		Instance = this;
		canvasGroup = GetComponentInChildren<CanvasGroup>();
		canvasGroup.alpha = 0f;
		canvasGroup.interactable = false;
		canvasGroup.blocksRaycasts = false;
		IsOpen = false;
		isClosing = true;
		GameObjectEx.SetChildComponentsEnabled<TMP_Text>(base.gameObject, enabled: false);
	}

	public void Clear()
	{
		options = new MenuOption[0];
	}

	public void AddOption(MenuOption option)
	{
		List<MenuOption> list = options.ToList();
		list.Add(option);
		options = list.ToArray();
	}

	public void FinishAndOpen()
	{
		IsOpen = true;
		isClosing = false;
		SetDefaultOption();
		Rebuild();
		UpdateInteraction(allowLerp: false);
		PlayOpenSound();
		LeanTween.cancel(base.gameObject);
		LeanTween.cancel(scaleTarget);
		GetComponent<CanvasGroup>().alpha = 0f;
		LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 1f, 0.1f).setEase(LeanTweenType.easeOutCirc);
		scaleTarget.transform.localScale = Vector3.one * 1.5f;
		LeanTween.scale(scaleTarget, Vector3.one, 0.1f).setEase(LeanTweenType.easeOutBounce);
		GameObjectEx.SetChildComponentsEnabled<TMP_Text>(Instance.gameObject, enabled: true);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Rebuild();
	}

	public void SetDefaultOption()
	{
		defaultOption = null;
		for (int i = 0; i < options.Length; i++)
		{
			if (!options[i].disabled)
			{
				if (defaultOption == null)
				{
					defaultOption = options[i];
				}
				if (options[i].selected)
				{
					defaultOption = options[i];
					break;
				}
			}
		}
	}

	public void PlayOpenSound()
	{
	}

	public void PlayCancelSound()
	{
	}

	public void Close(bool success = false)
	{
		if (!isClosing)
		{
			isClosing = true;
			NeedsCursor component = GetComponent<NeedsCursor>();
			if (component != null)
			{
				component.enabled = false;
			}
			LeanTween.cancel(base.gameObject);
			LeanTween.cancel(scaleTarget);
			LeanTween.alphaCanvas(GetComponent<CanvasGroup>(), 0f, 0.2f).setEase(LeanTweenType.easeOutCirc);
			LeanTween.scale(scaleTarget, Vector3.one * (success ? 1.5f : 0.5f), 0.2f).setEase(LeanTweenType.easeOutCirc);
			GameObjectEx.SetChildComponentsEnabled<TMP_Text>(Instance.gameObject, enabled: false);
			IsOpen = false;
			selectedOption = null;
		}
	}

	private void Update()
	{
		if (pieBackground.innerSize != innerSize || pieBackground.outerSize != outerSize || pieBackground.startRadius != startRadius || pieBackground.endRadius != startRadius + radiusSize)
		{
			pieBackground.startRadius = startRadius;
			pieBackground.endRadius = startRadius + radiusSize;
			pieBackground.innerSize = innerSize;
			pieBackground.outerSize = outerSize;
			pieBackground.SetVerticesDirty();
		}
		UpdateInteraction();
		if (IsOpen)
		{
			CursorManager.HoldOpen();
			IngameMenuBackground.Enabled = true;
		}
	}

	public void Rebuild()
	{
		options = options.OrderBy((MenuOption x) => x.order).ToArray();
		while (optionsCanvas.transform.childCount > 0)
		{
			if (UnityEngine.Application.isPlaying)
			{
				GameManager.DestroyImmediate(optionsCanvas.transform.GetChild(0).gameObject, allowDestroyingAssets: true);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(optionsCanvas.transform.GetChild(0).gameObject);
			}
		}
		if (options.Length != 0)
		{
			for (int i = 0; i < options.Length; i++)
			{
				bool flag = false;
				if (options[i].allowMerge)
				{
					if (i > 0)
					{
						flag |= options[i].order == options[i - 1].order;
					}
					if (i < options.Length - 1)
					{
						flag |= options[i].order == options[i + 1].order;
					}
				}
				options[i].wantsMerge = flag;
			}
			int num = options.Length;
			int num2 = options.Where((MenuOption x) => x.wantsMerge).Count();
			int num3 = num - num2;
			int num4 = num3 + num2 / 2;
			float num5 = radiusSize / (float)num * 0.75f;
			float num6 = (radiusSize - num5 * (float)num2) / (float)num3;
			float num7 = startRadius - radiusSize / (float)num4 * 0.25f;
			for (int j = 0; j < options.Length; j++)
			{
				float num8 = (options[j].wantsMerge ? 0.8f : 1f);
				float num9 = (options[j].wantsMerge ? num5 : num6);
				GameObject gameObject = Facepunch.Instantiate.GameObject(pieOptionPrefab);
				gameObject.transform.SetParent(optionsCanvas.transform, worldPositionStays: false);
				options[j].option = gameObject.GetComponent<PieOption>();
				options[j].option.UpdateOption(num7, num9, sliceGaps, options[j].name, outerSize, innerSize, num8 * iconSize, options[j].sprite, options[j].showOverlay);
				options[j].option.imageIcon.material = ((options[j].overrideColorMode.HasValue && options[j].overrideColorMode.Value.Mode == MenuOption.ColorMode.PieMenuSpriteColorOption.SpriteColor) ? null : IconMaterial);
				num7 += num9;
			}
		}
		selectedOption = null;
	}

	public void UpdateInteraction(bool allowLerp = true)
	{
		if (isClosing)
		{
			return;
		}
		Vector3 vector = UnityEngine.Input.mousePosition - new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
		float num = Mathf.Atan2(vector.x, vector.y) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		for (int i = 0; i < options.Length; i++)
		{
			float midRadius = options[i].option.midRadius;
			float sliceSize = options[i].option.sliceSize;
			if ((vector.magnitude < 32f && options[i] == defaultOption) || (vector.magnitude >= 32f && Mathf.Abs(Mathf.DeltaAngle(num, midRadius)) < sliceSize * 0.5f))
			{
				if (allowLerp)
				{
					pieSelection.startRadius = Mathf.MoveTowardsAngle(pieSelection.startRadius, options[i].option.background.startRadius, Time.deltaTime * Mathf.Abs(Mathf.DeltaAngle(pieSelection.startRadius, options[i].option.background.startRadius) * 30f + 10f));
					pieSelection.endRadius = Mathf.MoveTowardsAngle(pieSelection.endRadius, options[i].option.background.endRadius, Time.deltaTime * Mathf.Abs(Mathf.DeltaAngle(pieSelection.endRadius, options[i].option.background.endRadius) * 30f + 10f));
				}
				else
				{
					pieSelection.startRadius = options[i].option.background.startRadius;
					pieSelection.endRadius = options[i].option.background.endRadius;
				}
				middleImage.material = IconMaterial;
				if (options[i].overrideColorMode.HasValue)
				{
					if (options[i].overrideColorMode.Value.Mode == MenuOption.ColorMode.PieMenuSpriteColorOption.CustomColor)
					{
						Color customColor = options[i].overrideColorMode.Value.CustomColor;
						pieSelection.color = customColor;
						customColor.a = middleImageColor.a;
						middleImage.color = customColor;
					}
					else if (options[i].overrideColorMode.Value.Mode == MenuOption.ColorMode.PieMenuSpriteColorOption.SpriteColor)
					{
						pieSelection.color = pieSelectionColor;
						middleImage.color = Color.white;
						middleImage.material = null;
					}
				}
				else
				{
					pieSelection.color = pieSelectionColor;
					middleImage.color = middleImageColor;
				}
				pieSelection.SetVerticesDirty();
				middleImage.sprite = options[i].sprite;
				middleTitle.text = options[i].name;
				middleDesc.text = options[i].desc;
				middleRequired.text = "";
				Facepunch.Input.Button buttonObjectWithBind = Facepunch.Input.GetButtonObjectWithBind("+prevskin");
				if (options[i].actionPrev != null && buttonObjectWithBind != null && buttonObjectWithBind.Code != 0)
				{
					arrowLeft.SetActive(value: true);
					arrowLeft.GetComponentInChildren<TextMeshProUGUI>().text = buttonObjectWithBind.Code.ToShortname();
				}
				else
				{
					arrowLeft.SetActive(value: false);
				}
				Facepunch.Input.Button buttonObjectWithBind2 = Facepunch.Input.GetButtonObjectWithBind("+nextskin");
				if (options[i].actionNext != null && buttonObjectWithBind2 != null && buttonObjectWithBind2.Code != 0)
				{
					arrowRight.SetActive(value: true);
					arrowRight.GetComponentInChildren<TextMeshProUGUI>().text = buttonObjectWithBind2.Code.ToShortname();
				}
				else
				{
					arrowRight.SetActive(value: false);
				}
				string requirements = options[i].requirements;
				if (requirements != null)
				{
					requirements = requirements.Replace("[e]", "<color=#CD412B>");
					requirements = requirements.Replace("[/e]", "</color>");
					middleRequired.text = requirements;
				}
				if (!options[i].showOverlay)
				{
					options[i].option.imageIcon.color = colorIconHovered;
				}
				if (selectedOption != options[i])
				{
					if (selectedOption != null && !options[i].disabled)
					{
						scaleTarget.transform.localScale = Vector3.one;
						LeanTween.scale(scaleTarget, Vector3.one * 1.03f, 0.2f).setEase(easePunch);
					}
					if (selectedOption != null)
					{
						UIEx.RebuildHackUnity2019(selectedOption.option.imageIcon);
					}
					selectedOption = options[i];
					if (selectedOption != null)
					{
						UIEx.RebuildHackUnity2019(selectedOption.option.imageIcon);
					}
				}
			}
			else
			{
				options[i].option.imageIcon.material = IconMaterial;
				if (options[i].overrideColorMode.HasValue)
				{
					if (options[i].overrideColorMode.Value.Mode == MenuOption.ColorMode.PieMenuSpriteColorOption.CustomColor)
					{
						options[i].option.imageIcon.color = options[i].overrideColorMode.Value.CustomColor;
					}
					else if (options[i].overrideColorMode.Value.Mode == MenuOption.ColorMode.PieMenuSpriteColorOption.SpriteColor)
					{
						options[i].option.imageIcon.color = Color.white;
						options[i].option.imageIcon.material = null;
					}
				}
				else
				{
					options[i].option.imageIcon.color = colorIconActive;
				}
			}
			if (options[i].disabled)
			{
				options[i].option.imageIcon.color = colorIconDisabled;
				options[i].option.background.color = colorBackgroundDisabled;
			}
		}
	}

	public bool DoSelect()
	{
		return true;
	}

	public void DoPrev()
	{
	}

	public void DoNext()
	{
	}
}
