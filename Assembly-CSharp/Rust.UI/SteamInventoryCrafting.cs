using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rust.UI;

public class SteamInventoryCrafting : MonoBehaviour
{
	public GameObject Container;

	public ToggleGroup ToggleGroup;

	public Button ConvertToItem;

	public TextMeshProUGUI WoodAmount;

	public TextMeshProUGUI ClothAmount;

	public TextMeshProUGUI MetalAmount;

	public TextMeshProUGUI InfoText;

	public SteamInventoryCrateOpen CraftModal;

	public GameObject CraftingContainer;

	public GameObject CraftingButton;

	public SteamInventoryNewItem NewItemModal;

	public IPlayerItemDefinition ResultItem { get; private set; }

	public Coroutine MarketCoroutine { get; private set; }
}
