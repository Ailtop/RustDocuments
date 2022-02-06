public class ItemModSprayContainer : ItemModContainer
{
	public ItemDefinition[] SprayItems;

	protected override bool ForceAcceptItemCheck => true;

	protected override void SetAllowedItems(ItemContainer container)
	{
		container.SetOnlyAllowedItems(SprayItems);
	}
}
