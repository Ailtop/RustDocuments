using ConVar;
using UnityEngine;

public class NPCSpawner : SpawnGroup
{
	public MonumentNavMesh monumentNavMesh;

	public bool shouldFillOnSpawn;

	[Header("InfoZone Config")]
	public AIInformationZone VirtualInfoZone;

	[Header("Navigator Config")]
	public AIMovePointPath Path;

	public BasePath AStarGraph;

	public override void SpawnInitial()
	{
		fillOnSpawn = shouldFillOnSpawn;
		if (WaitingForNavMesh())
		{
			Invoke(LateSpawn, 10f);
		}
		else
		{
			base.SpawnInitial();
		}
	}

	public bool WaitingForNavMesh()
	{
		if (monumentNavMesh != null)
		{
			return monumentNavMesh.IsBuilding;
		}
		if (!DungeonNavmesh.NavReady())
		{
			return true;
		}
		return !AI.move;
	}

	public void LateSpawn()
	{
		if (!WaitingForNavMesh())
		{
			SpawnInitial();
			Debug.Log("Navmesh complete, spawning");
		}
		else
		{
			Invoke(LateSpawn, 5f);
		}
	}

	protected override void PostSpawnProcess(BaseEntity entity, BaseSpawnPoint spawnPoint)
	{
		base.PostSpawnProcess(entity, spawnPoint);
		BaseNavigator component = entity.GetComponent<BaseNavigator>();
		if (VirtualInfoZone != null)
		{
			if (VirtualInfoZone.Virtual)
			{
				NPCPlayer nPCPlayer = entity as NPCPlayer;
				if (nPCPlayer != null)
				{
					nPCPlayer.VirtualInfoZone = VirtualInfoZone;
					HumanNPCNew humanNPCNew = nPCPlayer as HumanNPCNew;
					if (humanNPCNew != null)
					{
						humanNPCNew.VirtualInfoZone.RegisterSleepableEntity(humanNPCNew.Brain);
					}
				}
			}
			else
			{
				Debug.LogError("NPCSpawner trying to set a virtual info zone without the Virtual property!");
			}
		}
		if (component != null)
		{
			component.Path = Path;
			component.AStarGraph = AStarGraph;
		}
	}
}
