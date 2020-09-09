using Rust.Workshop;
using UnityEngine;

public class ItemSkin : SteamInventoryItem
{
	public Skinnable Skinnable;

	public Material[] Materials;

	[Tooltip("If set, whenever we make an item with this skin, we'll spawn this item without a skin instead")]
	public ItemDefinition Redirect;

	public void ApplySkin(GameObject obj)
	{
		if (!(Skinnable == null))
		{
			Skin.Apply(obj, Skinnable, Materials);
		}
	}
}
