using System.Linq;
using UnityEngine;

namespace ConVar;

[Factory("stability")]
public class Stability : ConsoleSystem
{
	[ServerVar]
	public static int verbose = 0;

	[ServerVar]
	public static int strikes = 10;

	[ServerVar]
	public static float collapse = 0.05f;

	[ServerVar]
	public static float accuracy = 0.001f;

	[ServerVar]
	public static float stabilityqueue = 9f;

	[ServerVar]
	public static float surroundingsqueue = 3f;

	[ServerVar]
	public static void refresh_stability(Arg args)
	{
		StabilityEntity[] array = BaseNetworkable.serverEntities.OfType<StabilityEntity>().ToArray();
		Debug.Log("Refreshing stability on " + array.Length + " entities...");
		for (int i = 0; i < array.Length; i++)
		{
			array[i].UpdateStability();
		}
	}
}
