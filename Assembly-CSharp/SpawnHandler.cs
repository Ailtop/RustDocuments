using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ConVar;
using UnityEngine;

public class SpawnHandler : SingletonComponent<SpawnHandler>
{
	public float TickInterval = 60f;

	public int MinSpawnsPerTick = 100;

	public int MaxSpawnsPerTick = 100;

	public LayerMask PlacementMask;

	public LayerMask PlacementCheckMask;

	public float PlacementCheckHeight = 25f;

	public LayerMask RadiusCheckMask;

	public float RadiusCheckDistance = 5f;

	public LayerMask BoundsCheckMask;

	public SpawnFilter CharacterSpawn;

	public float CharacterSpawnCutoff;

	public SpawnPopulationBase[] SpawnPopulations;

	public SpawnDistribution[] SpawnDistributions;

	public SpawnDistribution CharDistribution;

	public ListHashSet<ISpawnGroup> SpawnGroups = new ListHashSet<ISpawnGroup>();

	internal List<SpawnIndividual> SpawnIndividuals = new List<SpawnIndividual>();

	[ReadOnly]
	public SpawnPopulationBase[] ConvarSpawnPopulations;

	public Dictionary<SpawnPopulationBase, SpawnDistribution> population2distribution;

	private bool spawnTick;

	public SpawnPopulationBase[] AllSpawnPopulations;

	protected void OnEnable()
	{
		AllSpawnPopulations = SpawnPopulations.Concat(ConvarSpawnPopulations).ToArray();
		StartCoroutine(SpawnTick());
		StartCoroutine(SpawnGroupTick());
		StartCoroutine(SpawnIndividualTick());
	}

	public static BasePlayer.SpawnPoint GetSpawnPoint()
	{
		if (SingletonComponent<SpawnHandler>.Instance == null || SingletonComponent<SpawnHandler>.Instance.CharDistribution == null)
		{
			return null;
		}
		BasePlayer.SpawnPoint spawnPoint = new BasePlayer.SpawnPoint();
		if (!((WaterSystem.OceanLevel < 0.5f) ? GetSpawnPointStandard(spawnPoint) : FloodedSpawnHandler.GetSpawnPoint(spawnPoint, WaterSystem.OceanLevel + 1f)))
		{
			return null;
		}
		return spawnPoint;
	}

	private static bool GetSpawnPointStandard(BasePlayer.SpawnPoint spawnPoint)
	{
		for (int i = 0; i < 60; i++)
		{
			if (!SingletonComponent<SpawnHandler>.Instance.CharDistribution.Sample(out spawnPoint.pos, out spawnPoint.rot))
			{
				continue;
			}
			bool flag = true;
			if (TerrainMeta.Path != null)
			{
				foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
				{
					if (monument.Distance(spawnPoint.pos) < 50f)
					{
						flag = false;
						break;
					}
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateDistributions()
	{
		if (World.Size == 0)
		{
			return;
		}
		SpawnDistributions = new SpawnDistribution[AllSpawnPopulations.Length];
		population2distribution = new Dictionary<SpawnPopulationBase, SpawnDistribution>();
		Vector3 size = TerrainMeta.Size;
		Vector3 position = TerrainMeta.Position;
		int populationRes = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.25f));
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
			if (spawnPopulationBase == null)
			{
				Debug.LogError("Spawn handler contains null spawn population.");
				continue;
			}
			byte[] baseMapValues = spawnPopulationBase.GetBaseMapValues(populationRes);
			SpawnDistribution value = (SpawnDistributions[i] = new SpawnDistribution(this, baseMapValues, position, size));
			population2distribution.Add(spawnPopulationBase, value);
		}
		int char_res = Mathf.NextPowerOfTwo((int)((float)World.Size * 0.5f));
		byte[] map = new byte[char_res * char_res];
		SpawnFilter filter = CharacterSpawn;
		float cutoff = CharacterSpawnCutoff;
		Parallel.For(0, char_res, delegate(int z)
		{
			for (int j = 0; j < char_res; j++)
			{
				float normX = ((float)j + 0.5f) / (float)char_res;
				float normZ = ((float)z + 0.5f) / (float)char_res;
				float factor = filter.GetFactor(normX, normZ);
				map[z * char_res + j] = (byte)((factor >= cutoff) ? (255f * factor) : 0f);
			}
		});
		CharDistribution = new SpawnDistribution(this, map, position, size);
	}

	public void FillPopulations()
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
			}
		}
	}

	public void FillGroups()
	{
		for (int i = 0; i < SpawnGroups.Count; i++)
		{
			SpawnGroups[i].Fill();
		}
	}

	public void FillIndividuals()
	{
		for (int i = 0; i < SpawnIndividuals.Count; i++)
		{
			SpawnIndividual spawnIndividual = SpawnIndividuals[i];
			Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID), spawnIndividual.Position, spawnIndividual.Rotation);
		}
	}

	public void InitialSpawn()
	{
		if (ConVar.Spawn.respawn_populations && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (!(AllSpawnPopulations[i] == null))
				{
					SpawnInitial(AllSpawnPopulations[i], SpawnDistributions[i]);
				}
			}
		}
		if (ConVar.Spawn.respawn_groups)
		{
			for (int j = 0; j < SpawnGroups.Count; j++)
			{
				SpawnGroups[j].SpawnInitial();
			}
		}
	}

	public void StartSpawnTick()
	{
		spawnTick = true;
	}

	private IEnumerator SpawnTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_populations)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_populations);
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				if (spawnPopulationBase == null)
				{
					continue;
				}
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (spawnDistribution == null)
				{
					continue;
				}
				try
				{
					if (SpawnDistributions != null)
					{
						SpawnRepeating(spawnPopulationBase, spawnDistribution);
					}
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	private IEnumerator SpawnGroupTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_groups)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(1f);
			for (int i = 0; i < SpawnGroups.Count; i++)
			{
				ISpawnGroup spawnGroup = SpawnGroups[i];
				if (spawnGroup != null)
				{
					try
					{
						spawnGroup.SpawnRepeating();
					}
					catch (Exception message)
					{
						Debug.LogError(message);
					}
					yield return CoroutineEx.waitForEndOfFrame;
				}
			}
		}
	}

	private IEnumerator SpawnIndividualTick()
	{
		while (true)
		{
			yield return CoroutineEx.waitForEndOfFrame;
			if (!spawnTick || !ConVar.Spawn.respawn_individuals)
			{
				continue;
			}
			yield return CoroutineEx.waitForSeconds(ConVar.Spawn.tick_individuals);
			for (int i = 0; i < SpawnIndividuals.Count; i++)
			{
				SpawnIndividual spawnIndividual = SpawnIndividuals[i];
				try
				{
					Spawn(Prefab.Load<Spawnable>(spawnIndividual.PrefabID), spawnIndividual.Position, spawnIndividual.Rotation);
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
				yield return CoroutineEx.waitForEndOfFrame;
			}
		}
	}

	public void SpawnInitial(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int numToFill = targetCount - count;
		population.Fill(this, distribution, numToFill, initialSpawn: true);
	}

	public void SpawnRepeating(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		int count = distribution.Count;
		int num = targetCount - count;
		num = Mathf.RoundToInt((float)num * population.GetCurrentSpawnRate());
		num = UnityEngine.Random.Range(Mathf.Min(num, MinSpawnsPerTick), Mathf.Min(num, MaxSpawnsPerTick));
		population.Fill(this, distribution, num, initialSpawn: false);
	}

	public GameObject Spawn(SpawnPopulationBase population, Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		if (prefab == null)
		{
			return null;
		}
		if (prefab.Component == null)
		{
			Debug.LogError("[Spawn] Missing component 'Spawnable' on " + prefab.Name);
			return null;
		}
		Vector3 scale = Vector3.one;
		DecorComponent[] components = PrefabAttribute.server.FindAll<DecorComponent>(prefab.ID);
		DecorComponentEx.ApplyDecorComponents(prefab.Object.transform, components, ref pos, ref rot, ref scale);
		if (!prefab.ApplyTerrainAnchors(ref pos, rot, scale, TerrainAnchorMode.MinimizeMovement, population.GetSpawnFilter()))
		{
			return null;
		}
		if (!prefab.ApplyTerrainChecks(pos, rot, scale, population.GetSpawnFilter()))
		{
			return null;
		}
		if (!prefab.ApplyTerrainFilters(pos, rot, scale))
		{
			return null;
		}
		if (!prefab.ApplyWaterChecks(pos, rot, scale))
		{
			return null;
		}
		if (!prefab.ApplyBoundsChecks(pos, rot, scale, BoundsCheckMask))
		{
			return null;
		}
		if (Global.developer > 1)
		{
			Debug.Log("[Spawn] Spawning " + prefab.Name);
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot, active: false);
		if (baseEntity == null)
		{
			Debug.LogWarning("[Spawn] Couldn't create prefab as entity - " + prefab.Name);
			return null;
		}
		Spawnable component = baseEntity.GetComponent<Spawnable>();
		if (component.Population != population)
		{
			component.Population = population;
		}
		PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
		baseEntity.Spawn();
		return baseEntity.gameObject;
	}

	private GameObject Spawn(Prefab<Spawnable> prefab, Vector3 pos, Quaternion rot)
	{
		if (!CheckBounds(prefab.Object, pos, rot, Vector3.one))
		{
			return null;
		}
		BaseEntity baseEntity = prefab.SpawnEntity(pos, rot);
		if (baseEntity == null)
		{
			Debug.LogWarning("[Spawn] Couldn't create prefab as entity - " + prefab.Name);
			return null;
		}
		baseEntity.Spawn();
		return baseEntity.gameObject;
	}

	public bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		return CheckBounds(gameObject, pos, rot, scale, BoundsCheckMask);
	}

	public static bool CheckBounds(GameObject gameObject, Vector3 pos, Quaternion rot, Vector3 scale, LayerMask mask)
	{
		if (gameObject == null)
		{
			return true;
		}
		if ((int)mask != 0)
		{
			BaseEntity component = gameObject.GetComponent<BaseEntity>();
			if (component != null && UnityEngine.Physics.CheckBox(pos + rot * Vector3.Scale(component.bounds.center, scale), Vector3.Scale(component.bounds.extents, scale), rot, mask))
			{
				return false;
			}
		}
		return true;
	}

	public void EnforceLimits(bool forceAll = false)
	{
		if (SpawnDistributions == null)
		{
			return;
		}
		for (int i = 0; i < AllSpawnPopulations.Length; i++)
		{
			if (!(AllSpawnPopulations[i] == null))
			{
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution distribution = SpawnDistributions[i];
				if (forceAll || spawnPopulationBase.EnforcePopulationLimits)
				{
					EnforceLimits(spawnPopulationBase, distribution);
				}
			}
		}
	}

	public void EnforceLimits(SpawnPopulationBase population, SpawnDistribution distribution)
	{
		int targetCount = population.GetTargetCount(distribution);
		Spawnable[] array = FindAll(population);
		if (array.Length <= targetCount)
		{
			return;
		}
		Debug.Log(population?.ToString() + " has " + array.Length + " objects, but max allowed is " + targetCount);
		int count = array.Length - targetCount;
		Debug.Log(" - deleting " + count + " objects");
		foreach (Spawnable item in array.Take(count))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
			if (BaseNetworkableEx.IsValid(baseEntity))
			{
				baseEntity.Kill();
			}
			else
			{
				GameManager.Destroy(item.gameObject);
			}
		}
	}

	public Spawnable[] FindAll(SpawnPopulationBase population)
	{
		return (from x in UnityEngine.Object.FindObjectsOfType<Spawnable>()
			where x.gameObject.activeInHierarchy && x.Population == population
			select x).ToArray();
	}

	public void AddRespawn(SpawnIndividual individual)
	{
		SpawnIndividuals.Add(individual);
	}

	public void AddInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning("[SpawnHandler] trying to add instance to invalid population: " + spawnable.Population);
			}
			else
			{
				value.AddInstance(spawnable);
			}
		}
	}

	public void RemoveInstance(Spawnable spawnable)
	{
		if (spawnable.Population != null)
		{
			if (!population2distribution.TryGetValue(spawnable.Population, out var value))
			{
				Debug.LogWarning("[SpawnHandler] trying to remove instance from invalid population: " + spawnable.Population);
			}
			else
			{
				value.RemoveInstance(spawnable);
			}
		}
	}

	public static float PlayerFraction()
	{
		float num = Mathf.Max(Server.maxplayers, 1);
		return Mathf.Clamp01((float)BasePlayer.activePlayerList.Count / num);
	}

	public static float PlayerLerp(float min, float max)
	{
		return Mathf.Lerp(min, max, PlayerFraction());
	}

	public static float PlayerExcess()
	{
		float num = Mathf.Max(ConVar.Spawn.player_base, 1f);
		float num2 = BasePlayer.activePlayerList.Count;
		if (num2 <= num)
		{
			return 0f;
		}
		return (num2 - num) / num;
	}

	public static float PlayerScale(float scalar)
	{
		return Mathf.Max(1f, PlayerExcess() * scalar);
	}

	public void DumpReport(string filename)
	{
		File.AppendAllText(filename, "\r\n\r\nSpawnHandler Report:\r\n\r\n" + GetReport());
	}

	public string GetReport(bool detailed = true)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (AllSpawnPopulations == null)
		{
			stringBuilder.AppendLine("Spawn population array is null.");
		}
		if (SpawnDistributions == null)
		{
			stringBuilder.AppendLine("Spawn distribution array is null.");
		}
		if (AllSpawnPopulations != null && SpawnDistributions != null)
		{
			for (int i = 0; i < AllSpawnPopulations.Length; i++)
			{
				if (AllSpawnPopulations[i] == null)
				{
					continue;
				}
				SpawnPopulationBase spawnPopulationBase = AllSpawnPopulations[i];
				SpawnDistribution spawnDistribution = SpawnDistributions[i];
				if (spawnPopulationBase != null)
				{
					spawnPopulationBase.GetReportString(stringBuilder, detailed);
					if (spawnDistribution != null)
					{
						int count = spawnDistribution.Count;
						int targetCount = spawnPopulationBase.GetTargetCount(spawnDistribution);
						stringBuilder.AppendLine("\tPopulation: " + count + "/" + targetCount);
					}
					else
					{
						stringBuilder.AppendLine("\tDistribution #" + i + " is not set.");
					}
				}
				else
				{
					stringBuilder.AppendLine("Population #" + i + " is not set.");
				}
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}
}
