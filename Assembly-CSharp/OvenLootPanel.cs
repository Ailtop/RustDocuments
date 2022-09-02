using System.Collections.Generic;
using Facepunch;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class OvenLootPanel : MonoBehaviour
{
	public GameObject controlsOn;

	public GameObject controlsOff;

	public Image TitleBackground;

	public RustText TitleText;

	public Color AlertBackgroundColor;

	public Color AlertTextColor;

	public Color OffBackgroundColor;

	public Color OffTextColor;

	public Color OnBackgroundColor;

	public Color OnTextColor;

	private Translate.Phrase OffPhrase = new Translate.Phrase("off", "off");

	private Translate.Phrase OnPhrase = new Translate.Phrase("on", "on");

	private Translate.Phrase NoFuelPhrase = new Translate.Phrase("no_fuel", "No Fuel");

	public GameObject FuelRowPrefab;

	public GameObject MaterialRowPrefab;

	public GameObject ItemRowPrefab;

	public Sprite IconBackground_Wood;

	public Sprite IconBackGround_Input;

	public LootGrid LootGrid_Wood;

	public LootGrid LootGrid_Input;

	public LootGrid LootGrid_Output;

	public GameObject Contents;

	private ItemContainerSource containerSource;

	private int _slotIndex;

	private List<OvenItemIcon> _icons = new List<OvenItemIcon>();

	private bool _inventoryCreated;

	public void SetStatus(OvenStatus status)
	{
	}

	public void EnsureInventoryCreated(BaseOven oven, ItemContainerSource source)
	{
		if (!_inventoryCreated)
		{
			containerSource = source;
			CreateInventory(oven.fuelSlots, oven.inputSlots, oven.outputSlots, source);
		}
	}

	private void CreateInventory(int fuelSlots, int inputSlots, int outputSlots, ItemContainerSource container)
	{
		int num = 0;
		LootGrid_Wood.CreateInventory(container, fuelSlots, 0);
		num += fuelSlots;
		LootGrid_Input.CreateInventory(container, inputSlots, num);
		num += inputSlots;
		LootGrid_Output.CreateInventory(container, outputSlots, num);
		_inventoryCreated = true;
	}

	private void AddRows(int count, OvenSlotType inputType, OvenSlotType outputType)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject obj = Facepunch.Instantiate.GameObject(ItemRowPrefab, Contents.transform);
			OvenItemIcon component = obj.transform.GetChild(0).GetComponent<OvenItemIcon>();
			OvenItemIcon component2 = obj.transform.GetChild(1).GetComponent<OvenItemIcon>();
			component.ItemIcon.slot = _slotIndex++;
			component.SlotType = inputType;
			component.ItemIcon.containerSource = containerSource;
			component.gameObject.SetActive(value: true);
			_icons.Add(component);
			component2.ItemIcon.slot = _slotIndex++;
			component2.SlotType = outputType;
			component2.ItemIcon.containerSource = containerSource;
			component2.gameObject.SetActive(value: true);
			_icons.Add(component2);
		}
	}

	public IEnumerable<ItemIcon> GetItemIcons()
	{
		foreach (OvenItemIcon icon in _icons)
		{
			yield return icon.ItemIcon;
		}
	}
}
