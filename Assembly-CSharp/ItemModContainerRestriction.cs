using System;

public class ItemModContainerRestriction : ItemMod
{
	[Flags]
	public enum SlotFlags
	{
		Map = 0x1
	}

	[InspectorFlags]
	public SlotFlags slotFlags;

	public bool CanExistWith(ItemModContainerRestriction other)
	{
		if (other == null)
		{
			return true;
		}
		if ((slotFlags & other.slotFlags) != 0)
		{
			return false;
		}
		return true;
	}
}
