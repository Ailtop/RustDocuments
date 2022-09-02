using System.Collections.Generic;
using ConVar;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Rust/Spawn Population")]
public class SpawnPopulation : BaseScriptableObject
{
	[Header("Spawnables")]
	public string ResourceFolder = string.Empty;

	public GameObjectRef[] ResourceList;

	[Tooltip("Usually per square km")]
	[Header("Spawn Info")]
	[FormerlySerializedAs("TargetDensity")]
	[SerializeField]
	public float _targetDensity = 1f;

	public float SpawnRate = 1f;

	public int ClusterSizeMin = 1;

	public int ClusterSizeMax = 1;

	public int ClusterDithering;

	public int SpawnAttemptsInitial = 20;

	public int SpawnAttemptsRepeating = 10;

	public bool EnforcePopulationLimits = true;

	public bool ScaleWithLargeMaps = true;

	public bool ScaleWithSpawnFilter = true;

	public bool ScaleWithServerPopulation;

	public bool AlignToNormal;

	public SpawnFilter Filter = new SpawnFilter();

	public float FilterCutoff;

	public Prefab<Spawnable>[] Prefabs;

	public int[] numToSpawn;

	private int sumToSpawn;

	public virtual float TargetDensity => _targetDensity;

	public bool Initialize()
	{
		if (Prefabs == null || Prefabs.Length == 0)
		{
			if (!string.IsNullOrEmpty(ResourceFolder))
			{
				Prefabs = Prefab.Load<Spawnable>("assets/bundled/prefabs/autospawn/" + ResourceFolder, GameManager.server, PrefabAttribute.server, useProbabilities: false);
			}
			if (ResourceList != null && ResourceList.Length != 0)
			{
				List<string> list = new List<string>();
				GameObjectRef[] resourceList = ResourceList;
				foreach (GameObjectRef gameObjectRef in resourceList)
				{
					string resourcePath = gameObjectRef.resourcePath;
					if (string.IsNullOrEmpty(resourcePath))
					{
						Debug.LogWarning(base.name + " resource list contains invalid resource path for GUID " + gameObjectRef.guid, this);
					}
					else
					{
						list.Add(resourcePath);
					}
				}
				Prefabs = Prefab.Load<Spawnable>(list.ToArray(), GameManager.server, PrefabAttribute.server);
			}
			if (Prefabs == null || Prefabs.Length == 0)
			{
				return false;
			}
			numToSpawn = new int[Prefabs.Length];
		}
		return true;
	}

	public void UpdateWeights(SpawnDistribution distribution, int targetCount)
	{
		int num = 0;
		for (int i = 0; i < Prefabs.Length; i++)
		{
			Prefab<Spawnable> prefab = Prefabs[i];
			int prefabWeight = GetPrefabWeight(prefab);
			num += prefabWeight;
		}
		int num2 = Mathf.CeilToInt((float)targetCount / (float)num);
		sumToSpawn = 0;
		for (int j = 0; j < Prefabs.Length; j++)
		{
			Prefab<Spawnable> prefab2 = Prefabs[j];
			int prefabWeight2 = GetPrefabWeight(prefab2);
			int count = distribution.GetCount(prefab2.ID);
			int num3 = Mathf.Max(prefabWeight2 * num2 - count, 0);
			numToSpawn[j] = num3;
			sumToSpawn += num3;
		}
	}

	protected virtual int GetPrefabWeight(Prefab<Spawnable> prefab)
	{
		if (!prefab.Parameters)
		{
			return 1;
		}
		return prefab.Parameters.Count;
	}

	public bool TryTakeRandomPrefab(out Prefab<Spawnable> result)
	{
		int num = Random.Range(0, sumToSpawn);
		for (int i = 0; i < Prefabs.Length; i++)
		{
			if ((num -= numToSpawn[i]) < 0)
			{
				numToSpawn[i]--;
				sumToSpawn--;
				result = Prefabs[i];
				return true;
			}
		}
		result = null;
		return false;
	}

	public void ReturnPrefab(Prefab<Spawnable> prefab)
	{
		if (prefab == null)
		{
			return;
		}
		for (int i = 0; i < Prefabs.Length; i++)
		{
			if (Prefabs[i] == prefab)
			{
				numToSpawn[i]++;
				sumToSpawn++;
			}
		}
	}

	public float GetCurrentSpawnRate()
	{
		if (ScaleWithServerPopulation)
		{
			return SpawnRate * SpawnHandler.PlayerLerp(Spawn.min_rate, Spawn.max_rate);
		}
		return SpawnRate * Spawn.max_rate;
	}

	public float GetCurrentSpawnDensity()
	{
		if (ScaleWithServerPopulation)
		{
			return TargetDensity * SpawnHandler.PlayerLerp(Spawn.min_density, Spawn.max_density) * 1E-06f;
		}
		return TargetDensity * Spawn.max_density * 1E-06f;
	}

	public float GetMaximumSpawnDensity()
	{
		if (ScaleWithServerPopulation)
		{
			return 2f * TargetDensity * SpawnHandler.PlayerLerp(Spawn.min_density, Spawn.max_density) * 1E-06f;
		}
		return 2f * TargetDensity * Spawn.max_density * 1E-06f;
	}

	public virtual bool GetSpawnPosOverride(Prefab<Spawnable> prefab, ref Vector3 newPos, ref Quaternion newRot)
	{
		return true;
	}

	public virtual void OnPostFill(SpawnHandler spawnHandler)
	{
	}
}
