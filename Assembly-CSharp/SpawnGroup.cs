using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using UnityEngine;

public class SpawnGroup : BaseMonoBehaviour, IServerComponent, ISpawnPointUser, ISpawnGroup
{
	[Serializable]
	public class SpawnEntry
	{
		public GameObjectRef prefab;

		public int weight = 1;

		public bool mobile;
	}

	[InspectorFlags]
	public MonumentTier Tier = (MonumentTier)(-1);

	public List<SpawnEntry> prefabs;

	public int maxPopulation = 5;

	public int numToSpawnPerTickMin = 1;

	public int numToSpawnPerTickMax = 2;

	public float respawnDelayMin = 10f;

	public float respawnDelayMax = 20f;

	public bool wantsInitialSpawn = true;

	public bool temporary;

	public bool forceInitialSpawn;

	public bool preventDuplicates;

	public BoxCollider setFreeIfMovedBeyond;

	public bool fillOnSpawn;

	public BaseSpawnPoint[] spawnPoints;

	public List<SpawnPointInstance> spawnInstances = new List<SpawnPointInstance>();

	public LocalClock spawnClock = new LocalClock();

	public int currentPopulation => spawnInstances.Count;

	public virtual bool WantsInitialSpawn()
	{
		return wantsInitialSpawn;
	}

	public virtual bool WantsTimedSpawn()
	{
		return respawnDelayMax != float.PositiveInfinity;
	}

	public float GetSpawnDelta()
	{
		return (respawnDelayMax + respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	public float GetSpawnVariance()
	{
		return (respawnDelayMax - respawnDelayMin) * 0.5f / SpawnHandler.PlayerScale(ConVar.Spawn.player_scale);
	}

	protected void Awake()
	{
		if (TerrainMeta.TopologyMap == null)
		{
			return;
		}
		int topology = TerrainMeta.TopologyMap.GetTopology(base.transform.position);
		int num = 469762048;
		int num2 = MonumentInfo.TierToMask(Tier);
		if (num2 == num || (num2 & topology) != 0)
		{
			spawnPoints = GetComponentsInChildren<BaseSpawnPoint>();
			if (WantsTimedSpawn())
			{
				spawnClock.Add(GetSpawnDelta(), GetSpawnVariance(), Spawn);
			}
			if (!temporary && (bool)SingletonComponent<SpawnHandler>.Instance)
			{
				SingletonComponent<SpawnHandler>.Instance.SpawnGroups.Add(this);
			}
			if (forceInitialSpawn)
			{
				Invoke(SpawnInitial, 1f);
			}
		}
	}

	public void Fill()
	{
		Spawn(maxPopulation);
	}

	public void Clear()
	{
		for (int num = spawnInstances.Count - 1; num >= 0; num--)
		{
			SpawnPointInstance spawnPointInstance = spawnInstances[num];
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnPointInstance.gameObject);
			if (setFreeIfMovedBeyond != null && !setFreeIfMovedBeyond.bounds.Contains(baseEntity.transform.position))
			{
				spawnPointInstance.Retire();
			}
			else if ((bool)baseEntity)
			{
				baseEntity.Kill();
			}
		}
		spawnInstances.Clear();
	}

	public bool HasSpawned(uint prefabID)
	{
		foreach (SpawnPointInstance spawnInstance in spawnInstances)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(spawnInstance.gameObject);
			if ((bool)baseEntity && baseEntity.prefabID == prefabID)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void SpawnInitial()
	{
		if (wantsInitialSpawn)
		{
			if (fillOnSpawn)
			{
				Spawn(maxPopulation);
			}
			else
			{
				Spawn();
			}
		}
	}

	public void SpawnRepeating()
	{
		for (int i = 0; i < spawnClock.events.Count; i++)
		{
			LocalClock.TimedEvent value = spawnClock.events[i];
			if (UnityEngine.Time.time > value.time)
			{
				value.delta = GetSpawnDelta();
				value.variance = GetSpawnVariance();
				spawnClock.events[i] = value;
			}
		}
		spawnClock.Tick();
	}

	public void ObjectSpawned(SpawnPointInstance instance)
	{
		spawnInstances.Add(instance);
	}

	public void ObjectRetired(SpawnPointInstance instance)
	{
		spawnInstances.Remove(instance);
	}

	public void DelayedSpawn()
	{
		Invoke(Spawn, 1f);
	}

	public void Spawn()
	{
		Spawn(UnityEngine.Random.Range(numToSpawnPerTickMin, numToSpawnPerTickMax + 1));
	}

	protected virtual void Spawn(int numToSpawn)
	{
		numToSpawn = Mathf.Min(numToSpawn, maxPopulation - currentPopulation);
		for (int i = 0; i < numToSpawn; i++)
		{
			GameObjectRef prefab = GetPrefab();
			if (prefab == null || string.IsNullOrEmpty(prefab.guid))
			{
				continue;
			}
			Vector3 pos;
			Quaternion rot;
			BaseSpawnPoint spawnPoint = GetSpawnPoint(prefab, out pos, out rot);
			if (!spawnPoint)
			{
				continue;
			}
			BaseEntity baseEntity = GameManager.server.CreateEntity(prefab.resourcePath, pos, rot, startActive: false);
			if ((bool)baseEntity)
			{
				if (baseEntity.enableSaving && !(spawnPoint is SpaceCheckingSpawnPoint))
				{
					baseEntity.enableSaving = false;
				}
				PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
				baseEntity.Spawn();
				PostSpawnProcess(baseEntity, spawnPoint);
				SpawnPointInstance spawnPointInstance = baseEntity.gameObject.AddComponent<SpawnPointInstance>();
				spawnPointInstance.parentSpawnPointUser = this;
				spawnPointInstance.parentSpawnPoint = spawnPoint;
				spawnPointInstance.Notify();
			}
		}
	}

	protected virtual void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
	}

	protected GameObjectRef GetPrefab()
	{
		float num = prefabs.Sum((SpawnEntry x) => (!preventDuplicates || !HasSpawned(x.prefab.resourceID)) ? x.weight : 0);
		if (num == 0f)
		{
			return null;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		foreach (SpawnEntry prefab in prefabs)
		{
			int num3 = ((!preventDuplicates || !HasSpawned(prefab.prefab.resourceID)) ? prefab.weight : 0);
			if ((num2 -= (float)num3) <= 0f)
			{
				return prefab.prefab;
			}
		}
		return prefabs[prefabs.Count - 1].prefab;
	}

	protected virtual BaseSpawnPoint GetSpawnPoint(GameObjectRef prefabRef, out Vector3 pos, out Quaternion rot)
	{
		BaseSpawnPoint baseSpawnPoint = null;
		pos = Vector3.zero;
		rot = Quaternion.identity;
		int num = UnityEngine.Random.Range(0, spawnPoints.Length);
		for (int i = 0; i < spawnPoints.Length; i++)
		{
			BaseSpawnPoint baseSpawnPoint2 = spawnPoints[(num + i) % spawnPoints.Length];
			if (!(baseSpawnPoint2 == null) && baseSpawnPoint2.IsAvailableTo(prefabRef) && !baseSpawnPoint2.HasPlayersIntersecting())
			{
				baseSpawnPoint = baseSpawnPoint2;
				break;
			}
		}
		if ((bool)baseSpawnPoint)
		{
			baseSpawnPoint.GetLocation(out pos, out rot);
		}
		return baseSpawnPoint;
	}

	protected virtual void OnDrawGizmos()
	{
		Gizmos.color = new Color(1f, 1f, 0f, 1f);
		Gizmos.DrawSphere(base.transform.position, 0.25f);
	}
}
