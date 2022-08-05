namespace ConVar;

[Factory("workshop")]
public class Workshop : ConsoleSystem
{
	[ServerVar]
	public static void print_approved_skins(Arg arg)
	{
		if (!PlatformService.Instance.IsValid || PlatformService.Instance.ItemDefinitions == null)
		{
			return;
		}
		TextTable textTable = new TextTable();
		textTable.AddColumn("name");
		textTable.AddColumn("itemshortname");
		textTable.AddColumn("workshopid");
		textTable.AddColumn("workshopdownload");
		foreach (IPlayerItemDefinition itemDefinition in PlatformService.Instance.ItemDefinitions)
		{
			string name = itemDefinition.Name;
			string itemShortName = itemDefinition.ItemShortName;
			string text = itemDefinition.WorkshopId.ToString();
			string text2 = itemDefinition.WorkshopDownload.ToString();
			textTable.AddRow(name, itemShortName, text, text2);
		}
		arg.ReplyWith(arg.HasArg("--json") ? textTable.ToJson() : textTable.ToString());
	}
}
