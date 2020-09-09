using ConVar;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.HTN.NPCTurret
{
	public class NPCTurretSpawner : MonoBehaviour, IServerComponent
	{
		public GameObjectRef NPCTurretPrefab;

		[NonSerialized]
		public List<NPCTurretDomain> Spawned = new List<NPCTurretDomain>();

		[NonSerialized]
		public BaseSpawnPoint[] SpawnPoints;

		public int MaxPopulation = 1;

		public bool InitialSpawn;

		public float MinRespawnTimeMinutes = 20f;

		public float MaxRespawnTimeMinutes = 20f;

		public bool OnlyRotateAroundYAxis;

		public bool ReducedLongRangeAccuracy;

		public bool BurstAtLongRange;

		private bool pendingRespawn;

		private bool _lastInvokeWasNoSpawn;

		private void Awake()
		{
			SpawnPoints = GetComponentsInChildren<BaseSpawnPoint>();
		}

		public void Start()
		{
			Invoke("DelayedStart", 3f);
		}

		public void DelayedStart()
		{
			if (InitialSpawn && ConVar.AI.npc_spawn_on_cargo_ship)
			{
				DoRespawn();
			}
			InvokeRepeating("CheckIfRespawnNeeded", 0f, 5f);
		}

		public void CheckIfRespawnNeeded()
		{
			if (!ConVar.AI.npc_spawn_on_cargo_ship)
			{
				_lastInvokeWasNoSpawn = true;
			}
			else if (_lastInvokeWasNoSpawn)
			{
				DoRespawn();
				_lastInvokeWasNoSpawn = false;
			}
			else if (!pendingRespawn && (Spawned == null || Spawned.Count == 0 || IsAllSpawnedDead()))
			{
				ScheduleRespawn();
				_lastInvokeWasNoSpawn = false;
			}
		}

		private bool IsAllSpawnedDead()
		{
			int num = 0;
			while (num < Spawned.Count)
			{
				NPCTurretDomain nPCTurretDomain = Spawned[num];
				if (nPCTurretDomain == null || nPCTurretDomain.transform == null || nPCTurretDomain.NPCTurretContext == null || nPCTurretDomain.NPCTurretContext.Body == null || nPCTurretDomain.NPCTurretContext.Body.IsDestroyed || nPCTurretDomain.NPCTurretContext.Body.IsDead())
				{
					Spawned.RemoveAt(num);
					num--;
					num++;
					continue;
				}
				return false;
			}
			return true;
		}

		public void ScheduleRespawn()
		{
			CancelInvoke("DoRespawn");
			Invoke("DoRespawn", UnityEngine.Random.Range(MinRespawnTimeMinutes, MaxRespawnTimeMinutes) * 60f);
			pendingRespawn = true;
		}

		public void DoRespawn()
		{
			if (!Application.isLoading && !Application.isLoadingSave)
			{
				SpawnScientist();
			}
			pendingRespawn = false;
		}

		public void SpawnScientist()
		{
			if (Spawned == null || Spawned.Count >= MaxPopulation)
			{
				Debug.LogWarning("Attempted to spawn an AStar Scientist, but the spawner was full!");
			}
			else
			{
				if (!ConVar.AI.npc_enable)
				{
					return;
				}
				int num = MaxPopulation - Spawned.Count;
				int num2 = 0;
				BaseEntity baseEntity;
				while (true)
				{
					if (num2 >= num)
					{
						return;
					}
					Vector3 pos;
					Quaternion rot;
					if (!(GetSpawnPoint(out pos, out rot) == null))
					{
						baseEntity = GameManager.server.CreateEntity(NPCTurretPrefab.resourcePath, pos, rot, false);
						NPCTurretDomain component = baseEntity.GetComponent<NPCTurretDomain>();
						if (!component)
						{
							break;
						}
						baseEntity.enableSaving = false;
						PoolableEx.AwakeFromInstantiate(baseEntity.gameObject);
						baseEntity.Spawn();
						if (OnlyRotateAroundYAxis && component.NPCTurretContext != null && component.NPCTurretContext.Body != null)
						{
							component.NPCTurretContext.Body.OnlyRotateAroundYAxis = true;
						}
						component.ReducedLongRangeAccuracy = ReducedLongRangeAccuracy;
						component.BurstAtLongRange = BurstAtLongRange;
						Spawned.Add(component);
					}
					num2++;
				}
				baseEntity.Kill();
			}
		}

		private BaseSpawnPoint GetSpawnPoint(out Vector3 pos, out Quaternion rot)
		{
			BaseSpawnPoint baseSpawnPoint = null;
			pos = Vector3.zero;
			rot = Quaternion.identity;
			int num = UnityEngine.Random.Range(0, SpawnPoints.Length);
			for (int i = 0; i < SpawnPoints.Length; i++)
			{
				baseSpawnPoint = SpawnPoints[(num + i) % SpawnPoints.Length];
				if ((bool)baseSpawnPoint && baseSpawnPoint.gameObject.activeSelf)
				{
					break;
				}
			}
			if ((bool)baseSpawnPoint)
			{
				baseSpawnPoint.GetLocation(out pos, out rot);
			}
			return baseSpawnPoint;
		}
	}
}
