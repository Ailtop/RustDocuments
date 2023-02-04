using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class IndustrialFilterDialog : UIDialog
{
	public GameObjectRef ItemPrefab;

	public Transform ItemParent;

	public GameObject ItemSearchParent;

	public ItemSearchEntry ItemSearchEntryPrefab;

	public VirtualItemIcon TargetItemIcon;

	public GameObject TargetCategoryRoot;

	public RustText TargetCategoryText;

	public Image TargetCategoryImage;

	public GameObject NoItemsPrompt;

	public RustButton PasteButton;
}
