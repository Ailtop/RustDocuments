using UnityEngine;

namespace ConVar;

[Factory("spawn")]
public class Spawn : ConsoleSystem
{
	[ServerVar]
	public static float min_rate = 0.5f;

	[ServerVar]
	public static float max_rate = 1f;

	[ServerVar]
	public static float min_density = 0.5f;

	[ServerVar]
	public static float max_density = 1f;

	[ServerVar]
	public static float player_base = 100f;

	[ServerVar]
	public static float player_scale = 2f;

	[ServerVar]
	public static bool respawn_populations = true;

	[ServerVar]
	public static bool respawn_groups = true;

	[ServerVar]
	public static bool respawn_individuals = true;

	[ServerVar]
	public static float tick_populations = 60f;

	[ServerVar]
	public static float tick_individuals = 300f;

	[ServerVar]
	public static void fill_populations(Arg args)
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			SingletonComponent<SpawnHandler>.Instance.FillPopulations();
		}
	}

	[ServerVar]
	public static void fill_groups(Arg args)
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			SingletonComponent<SpawnHandler>.Instance.FillGroups();
		}
	}

	[ServerVar]
	public static void fill_individuals(Arg args)
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			SingletonComponent<SpawnHandler>.Instance.FillIndividuals();
		}
	}

	[ServerVar]
	public static void report(Arg args)
	{
		if ((bool)SingletonComponent<SpawnHandler>.Instance)
		{
			args.ReplyWith(SingletonComponent<SpawnHandler>.Instance.GetReport(detailed: false));
		}
		else
		{
			args.ReplyWith("No spawn handler found.");
		}
	}

	[ServerVar]
	public static void scalars(Arg args)
	{
		TextTable textTable = new TextTable();
		textTable.AddColumn("Type");
		textTable.AddColumn("Value");
		textTable.AddRow("Player Fraction", SpawnHandler.PlayerFraction().ToString());
		textTable.AddRow("Player Excess", SpawnHandler.PlayerExcess().ToString());
		textTable.AddRow("Population Rate", SpawnHandler.PlayerLerp(min_rate, max_rate).ToString());
		textTable.AddRow("Population Density", SpawnHandler.PlayerLerp(min_density, max_density).ToString());
		textTable.AddRow("Group Rate", SpawnHandler.PlayerScale(player_scale).ToString());
		args.ReplyWith(textTable.ToString());
	}

	[ServerVar]
	public static void cargoshipevent(Arg args)
	{
		BaseEntity baseEntity = GameManager.server.CreateEntity("assets/content/vehicles/boats/cargoship/cargoshiptest.prefab");
		if (baseEntity != null)
		{
			baseEntity.SendMessage("TriggeredEventSpawn", SendMessageOptions.DontRequireReceiver);
			baseEntity.Spawn();
			args.ReplyWith("Cargo ship event has been started");
		}
		else
		{
			args.ReplyWith("Couldn't find cargo ship prefab - maybe it has been renamed?");
		}
	}
}
