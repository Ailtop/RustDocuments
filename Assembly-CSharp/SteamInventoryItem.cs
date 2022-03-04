using UnityEngine;

public class SteamInventoryItem : ScriptableObject
{
	public enum Category
	{
		None = 0,
		Clothing = 1,
		Weapon = 2,
		Decoration = 3,
		Crate = 4,
		Resource = 5
	}

	public enum SubCategory
	{
		None = 0,
		Shirt = 1,
		Pants = 2,
		Jacket = 3,
		Hat = 4,
		Mask = 5,
		Footwear = 6,
		Weapon = 7,
		Misc = 8,
		Crate = 9,
		Resource = 10,
		CrateUncraftable = 11
	}

	public int id;

	public Sprite icon;

	public Translate.Phrase displayName;

	public Translate.Phrase displayDescription;

	[Header("Steam Inventory")]
	public Category category;

	public SubCategory subcategory;

	public SteamInventoryCategory steamCategory;

	public bool isLimitedTimeOffer = true;

	[Tooltip("Stop this item being broken down into cloth etc")]
	public bool PreventBreakingDown;

	[Header("Meta")]
	public string itemname;

	public ulong workshopID;

	public SteamDLCItem DlcItem;

	[Tooltip("Does nothing currently")]
	public bool forceCraftableItemDesc;

	public ItemDefinition itemDefinition => ItemManager.FindItemDefinition(itemname);

	public virtual bool HasUnlocked(ulong playerId)
	{
		if (DlcItem != null && DlcItem.HasLicense(playerId))
		{
			return true;
		}
		return false;
	}
}
