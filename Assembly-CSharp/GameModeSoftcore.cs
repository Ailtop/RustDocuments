using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

public class GameModeSoftcore : GameModeVanilla
{
	public GameObjectRef reclaimManagerPrefab;

	public GameObjectRef reclaimBackpackPrefab;

	public static readonly Translate.Phrase ReclaimToast = new Translate.Phrase("softcore.reclaim", "You can reclaim some of your lost items by visiting the Outpost or Bandit Town.");

	public ItemAmount[] startingGear;

	[ServerVar]
	public static float reclaim_fraction_belt = 0.5f;

	[ServerVar]
	public static float reclaim_fraction_wear = 0f;

	[ServerVar]
	public static float reclaim_fraction_main = 0.5f;

	protected override void OnCreated()
	{
		base.OnCreated();
		SingletonComponent<ServerMgr>.Instance.CreateImportantEntity<ReclaimManager>(reclaimManagerPrefab.resourcePath);
	}

	public void AddFractionOfContainer(ItemContainer from, ref List<Item> to, float fraction = 1f, bool takeLastItem = false)
	{
		if (from.itemList.Count == 0)
		{
			return;
		}
		fraction = Mathf.Clamp01(fraction);
		int count = from.itemList.Count;
		float num = Mathf.Ceil((float)count * fraction);
		if (count == 1 && num == 1f && !takeLastItem)
		{
			return;
		}
		List<int> obj = Facepunch.Pool.GetList<int>();
		for (int i = 0; i < from.capacity; i++)
		{
			if (from.GetSlot(i) != null)
			{
				obj.Add(i);
			}
		}
		if (obj.Count == 0)
		{
			Facepunch.Pool.FreeList(ref obj);
			return;
		}
		for (int j = 0; (float)j < num; j++)
		{
			int index = Random.Range(0, obj.Count);
			Item item = from.GetSlot(obj[index]);
			if (item.info.stackable > 1)
			{
				foreach (Item item2 in from.itemList)
				{
					if (!(item2.info == item.info) || item2.amount >= item.amount || to.Contains(item2))
					{
						continue;
					}
					item = item2;
					for (int k = 0; k < obj.Count; k++)
					{
						if (obj[k] == item2.position)
						{
							index = k;
						}
					}
				}
			}
			to.Add(item);
			obj.RemoveAt(index);
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public List<Item> RemoveItemsFrom(ItemContainer itemContainer, ItemAmount[] types)
	{
		List<Item> list = Facepunch.Pool.GetList<Item>();
		foreach (ItemAmount itemAmount in types)
		{
			for (int j = 0; (float)j < itemAmount.amount; j++)
			{
				Item item = itemContainer.FindItemByItemID(itemAmount.itemDef.itemid);
				if (item != null)
				{
					item.RemoveFromContainer();
					list.Add(item);
				}
			}
		}
		return list;
	}

	public void ReturnItemsTo(ref List<Item> source, ItemContainer itemContainer)
	{
		foreach (Item item in source)
		{
			item.MoveToContainer(itemContainer);
		}
		Facepunch.Pool.FreeList(ref source);
	}

	public override void OnPlayerDeath(BasePlayer instigator, BasePlayer victim, HitInfo deathInfo = null)
	{
		if (victim != null && !victim.IsNpc)
		{
			SetInventoryLocked(victim, wantsLocked: false);
			int newID = 0;
			if (ReclaimManager.instance == null)
			{
				Debug.LogError("No reclaim manage for softcore");
				return;
			}
			List<Item> to = Facepunch.Pool.GetList<Item>();
			List<Item> source = RemoveItemsFrom(victim.inventory.containerBelt, startingGear);
			AddFractionOfContainer(victim.inventory.containerBelt, ref to, reclaim_fraction_belt);
			AddFractionOfContainer(victim.inventory.containerWear, ref to, reclaim_fraction_wear);
			AddFractionOfContainer(victim.inventory.containerMain, ref to, reclaim_fraction_main);
			if (to.Count > 0)
			{
				newID = ReclaimManager.instance.AddPlayerReclaim(victim.userID, to, (instigator == null) ? 0 : instigator.userID, (instigator == null) ? "" : instigator.displayName);
			}
			ReturnItemsTo(ref source, victim.inventory.containerBelt);
			if (to.Count > 0)
			{
				Vector3 pos = victim.transform.position + Vector3.up * 0.25f;
				Quaternion rot = Quaternion.Euler(0f, victim.transform.eulerAngles.y, 0f);
				ReclaimBackpack component = GameManager.server.CreateEntity(reclaimBackpackPrefab.resourcePath, pos, rot).GetComponent<ReclaimBackpack>();
				component.InitForPlayer(victim.userID, newID);
				component.Spawn();
			}
			Facepunch.Pool.FreeList(ref to);
		}
		base.OnPlayerDeath(instigator, victim, deathInfo);
	}

	public override void OnPlayerRespawn(BasePlayer player)
	{
		base.OnPlayerRespawn(player);
		player.ShowToast(2, ReclaimToast);
	}

	public override SleepingBag[] FindSleepingBagsForPlayer(ulong playerID, bool ignoreTimers)
	{
		return SleepingBag.FindForPlayer(playerID, ignoreTimers);
	}

	public override float CorpseRemovalTime(BaseCorpse corpse)
	{
		foreach (MonumentInfo monument in TerrainMeta.Path.Monuments)
		{
			if (monument != null && monument.IsSafeZone && monument.Bounds.Contains(corpse.transform.position))
			{
				return 30f;
			}
		}
		return Server.corpsedespawn;
	}

	public void SetInventoryLocked(BasePlayer player, bool wantsLocked)
	{
		player.inventory.containerMain.SetLocked(wantsLocked);
		player.inventory.containerBelt.SetLocked(wantsLocked);
		player.inventory.containerWear.SetLocked(wantsLocked);
	}

	public override void OnPlayerWounded(BasePlayer instigator, BasePlayer victim, HitInfo info)
	{
		base.OnPlayerWounded(instigator, victim, info);
		SetInventoryLocked(victim, wantsLocked: true);
	}

	public override void OnPlayerRevived(BasePlayer instigator, BasePlayer victim)
	{
		SetInventoryLocked(victim, wantsLocked: false);
		base.OnPlayerRevived(instigator, victim);
	}

	public override bool CanMoveItemsFrom(PlayerInventory inv, BaseEntity source, Item item)
	{
		if (item.parent != null && item.parent.HasFlag(ItemContainer.Flag.IsPlayer))
		{
			return !item.parent.IsLocked();
		}
		return base.CanMoveItemsFrom(inv, source, item);
	}
}
