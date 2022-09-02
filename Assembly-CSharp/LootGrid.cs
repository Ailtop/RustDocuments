using UnityEngine;

public class LootGrid : MonoBehaviour
{
	public int Container;

	public int Offset;

	public int Count = 1;

	public GameObject ItemIconPrefab;

	public Sprite BackgroundImage;

	public ItemContainerSource Inventory;

	public void CreateInventory(ItemContainerSource inventory, int? slots = null, int? offset = null)
	{
		Inventory = inventory;
		Count = slots ?? Count;
		Offset = offset ?? Offset;
		for (int i = 0; i < Count; i++)
		{
			ItemIcon component = Object.Instantiate(ItemIconPrefab, base.transform).GetComponent<ItemIcon>();
			component.slot = Offset + i;
			component.emptySlotBackgroundSprite = BackgroundImage ?? component.emptySlotBackgroundSprite;
			component.containerSource = inventory;
		}
	}
}
