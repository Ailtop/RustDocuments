public class ItemModRFListener : ItemModAssociatedEntity<PagerEntity>
{
	public GameObjectRef frequencyPanelPrefab;

	public override void ServerCommand(Item item, string command, BasePlayer player)
	{
		base.ServerCommand(item, command, player);
		PagerEntity associatedEntity = ItemModAssociatedEntity<PagerEntity>.GetAssociatedEntity(item);
		if (command == "stop")
		{
			associatedEntity.SetOff();
		}
		else if (command == "silenton")
		{
			associatedEntity.SetSilentMode(true);
		}
		else if (command == "silentoff")
		{
			associatedEntity.SetSilentMode(false);
		}
	}
}
