using UnityEngine;

namespace ConVar;

[Factory("craft")]
public class Craft : ConsoleSystem
{
	[ServerVar]
	public static bool instant;

	[ServerUserVar]
	public static void add(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if (!basePlayer || basePlayer.IsDead())
		{
			return;
		}
		int @int = args.GetInt(0);
		int int2 = args.GetInt(1, 1);
		int num = (int)args.GetUInt64(2, 0uL);
		if (int2 < 1)
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(@int);
		if (itemDefinition == null)
		{
			args.ReplyWith("Item not found");
			return;
		}
		ItemBlueprint itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
		if (!itemBlueprint)
		{
			args.ReplyWith("Blueprint not found");
			return;
		}
		if (!itemBlueprint.userCraftable)
		{
			args.ReplyWith("Item is not craftable");
			return;
		}
		if (!basePlayer.blueprints.CanCraft(@int, num, basePlayer.userID))
		{
			num = 0;
			if (!basePlayer.blueprints.CanCraft(@int, num, basePlayer.userID))
			{
				args.ReplyWith("You can't craft this item");
				return;
			}
			args.ReplyWith("You don't have permission to use this skin, so crafting unskinned");
		}
		if (!basePlayer.inventory.crafting.CraftItem(itemBlueprint, basePlayer, null, int2, num))
		{
			args.ReplyWith("Couldn't craft!");
		}
	}

	[ServerUserVar]
	public static void canceltask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			if (!basePlayer.inventory.crafting.CancelTask(@int, ReturnItems: true))
			{
				args.ReplyWith("Couldn't cancel task!");
			}
		}
	}

	[ServerUserVar]
	public static void cancel(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			basePlayer.inventory.crafting.CancelBlueprint(@int);
		}
	}

	[ServerUserVar]
	public static void fasttracktask(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((bool)basePlayer && !basePlayer.IsDead())
		{
			int @int = args.GetInt(0);
			if (!basePlayer.inventory.crafting.FastTrackTask(@int))
			{
				args.ReplyWith("Couldn't fast track task!");
			}
		}
	}
}
