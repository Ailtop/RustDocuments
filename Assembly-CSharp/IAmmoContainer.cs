using System.Collections.Generic;
using Rust;

public interface IAmmoContainer
{
	void FindAmmo(List<Item> list, AmmoTypes ammoType);

	List<Item> FindItemsByItemID(int id);

	Item FindItemByItemName(string name);

	bool HasAmmo(AmmoTypes ammoType);

	Item FindItemByUID(ItemId iUID);

	bool GiveItem(Item item, ItemContainer container = null);
}
