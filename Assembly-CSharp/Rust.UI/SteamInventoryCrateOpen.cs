using TMPro;
using UnityEngine;

namespace Rust.UI;

public class SteamInventoryCrateOpen : MonoBehaviour
{
	public TextMeshProUGUI Name;

	public TextMeshProUGUI Requirements;

	public TextMeshProUGUI Label;

	public HttpImage IconImage;

	public GameObject ErrorPanel;

	public TextMeshProUGUI ErrorText;

	public GameObject CraftButton;

	public GameObject ProgressPanel;

	public SteamInventoryNewItem NewItemModal;
}
