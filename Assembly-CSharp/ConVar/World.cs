namespace ConVar
{
	[Factory("world")]
	public class World : ConsoleSystem
	{
		[ClientVar]
		[ServerVar]
		public static bool cache = true;

		[ClientVar]
		public static bool streaming = true;

		[ClientVar]
		public static void monuments(Arg arg)
		{
			if ((bool)TerrainMeta.Path)
			{
				TextTable textTable = new TextTable();
				textTable.AddColumn("type");
				textTable.AddColumn("name");
				textTable.AddColumn("pos");
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					textTable.AddRow(monument.Type.ToString(), monument.name, monument.transform.position.ToString());
				}
				arg.ReplyWith(textTable.ToString());
			}
		}
	}
}
