using System.Text;
using ConVar;
using UnityEngine;

public abstract class SpawnPopulationBase : BaseScriptableObject
{
	[Header("Spawnables")]
	public bool EnforcePopulationLimits = true;

	public float SpawnRate = 1f;

	public bool ScaleWithServerPopulation;

	protected bool haveInitialized;

	public abstract bool Initialize();

	public float GetCurrentSpawnRate()
	{
		if (ScaleWithServerPopulation)
		{
			return SpawnRate * SpawnHandler.PlayerLerp(Spawn.min_rate, Spawn.max_rate);
		}
		return SpawnRate * Spawn.max_rate;
	}

	public void Fill(SpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn)
	{
		if (GetTargetCount(distribution) == 0)
		{
			return;
		}
		if (!Initialize())
		{
			Debug.LogError("[Spawn] No prefabs to spawn: " + base.name, this);
			return;
		}
		if (Global.developer > 1)
		{
			Debug.Log("[Spawn] Population " + base.name + " needs to spawn " + numToFill);
		}
		SubFill(spawnHandler, distribution, numToFill, initialSpawn);
	}

	public abstract void SubFill(SpawnHandler spawnHandler, SpawnDistribution distribution, int numToFill, bool initialSpawn);

	public abstract byte[] GetBaseMapValues(int populationRes);

	public abstract int GetTargetCount(SpawnDistribution distribution);

	public abstract SpawnFilter GetSpawnFilter();

	public abstract void GetReportString(StringBuilder sb, bool detailed);
}
