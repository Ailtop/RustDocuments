using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemIcon : BaseMonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler, IDraggable, IInventoryChanged, IItemAmountChanged, IItemIconChanged
{
	private Color backgroundColor;

	public Color selectedBackgroundColor = new Color(0.121568628f, 0.419607848f, 32f / 51f, 40f / 51f);

	public float unoccupiedAlpha = 1f;

	public Color unoccupiedColor;

	public ItemContainerSource containerSource;

	public int slotOffset;

	[Range(0f, 64f)]
	public int slot;

	public bool setSlotFromSiblingIndex = true;

	public GameObject slots;

	public CanvasGroup iconContents;

	public CanvasGroup canvasGroup;

	public Image iconImage;

	public Image underlayImage;

	public Text amountText;

	public Text hoverText;

	public Image hoverOutline;

	public Image cornerIcon;

	public Image lockedImage;

	public Image progressImage;

	public Image backgroundImage;

	public Image backgroundUnderlayImage;

	public Image progressPanel;

	public Sprite emptySlotBackgroundSprite;

	public CanvasGroup conditionObject;

	public Image conditionFill;

	public Image maxConditionFill;

	public bool allowSelection = true;

	public bool allowDropping = true;

	public bool allowMove = true;

	[NonSerialized]
	public Item item;

	[NonSerialized]
	public bool invalidSlot;

	public SoundDefinition hoverSound;

	public virtual void OnPointerClick(PointerEventData eventData)
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}
}
