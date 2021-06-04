using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class NPCDwelling : BaseEntity
{
	public NPCSpawner npcSpawner;

	public SpawnGroup[] spawnGroups;

	public AIMovePoint[] movePoints;

	public AICoverPoint[] coverPoints;

	public override void ServerInit()
	{
		base.ServerInit();
		UpdateInformationZone(false);
		npcSpawner.SpawnInitial();
		SpawnGroup[] array = spawnGroups;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SpawnInitial();
		}
	}

	public override void DestroyShared()
	{
		if (base.isServer)
		{
			CleanupSpawned();
		}
		base.DestroyShared();
		if (base.isServer)
		{
			UpdateInformationZone(true);
		}
	}

	public bool ValidateAIPoint(Vector3 pos)
	{
		base.gameObject.SetActive(false);
		bool result = !GamePhysics.CheckSphere(pos + Vector3.up * 0.6f, 0.5f, 65537);
		base.gameObject.SetActive(true);
		return result;
	}

	public void UpdateInformationZone(bool remove)
	{
		AIInformationZone forPoint = AIInformationZone.GetForPoint(base.transform.position);
		if (!forPoint)
		{
			return;
		}
		if (movePoints != null)
		{
			AIMovePoint[] array = movePoints;
			foreach (AIMovePoint aIMovePoint in array)
			{
				if (remove)
				{
					forPoint.RemoveMovePoint(aIMovePoint);
				}
				else if (ValidateAIPoint(aIMovePoint.transform.position))
				{
					forPoint.AddMovePoint(aIMovePoint);
				}
			}
		}
		if (coverPoints == null)
		{
			return;
		}
		AICoverPoint[] array2 = coverPoints;
		foreach (AICoverPoint aICoverPoint in array2)
		{
			if (remove)
			{
				forPoint.RemoveCoverPoint(aICoverPoint);
			}
			else if (ValidateAIPoint(aICoverPoint.transform.position))
			{
				forPoint.AddCoverPoint(aICoverPoint);
			}
		}
	}

	public void CheckDespawn()
	{
		if (!PlayersNearby() && npcSpawner.currentPopulation <= 0)
		{
			CleanupSpawned();
			Kill();
		}
	}

	public void CleanupSpawned()
	{
		if (spawnGroups != null)
		{
			SpawnGroup[] array = spawnGroups;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		if ((bool)npcSpawner)
		{
			npcSpawner.Clear();
		}
	}

	public bool PlayersNearby()
	{
		List<BasePlayer> obj = Pool.GetList<BasePlayer>();
		Vis.Entities(base.transform.position, TimeoutPlayerCheckRadius(), obj, 131072);
		bool result = false;
		foreach (BasePlayer item in obj)
		{
			if (!item.IsSleeping() && item.IsAlive())
			{
				result = true;
				break;
			}
		}
		Pool.FreeList(ref obj);
		return result;
	}

	public virtual float TimeoutPlayerCheckRadius()
	{
		return 10f;
	}
}
