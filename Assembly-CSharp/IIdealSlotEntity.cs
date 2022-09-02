public interface IIdealSlotEntity
{
	int GetIdealSlot(BasePlayer player, ItemContainer container, Item item);

	uint GetIdealContainer(BasePlayer player, Item item);
}
