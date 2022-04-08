using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ReclaimBackpack : StorageContainer
{
	public int reclaimID;

	public ulong playerSteamID;

	public bool onlyOwnerLoot = true;

	public Collider myCollider;

	public GameObject art;

	private bool isBeingLooted;

	public void InitForPlayer(ulong playerID, int newID)
	{
		playerSteamID = playerID;
		reclaimID = newID;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		Invoke(RemoveMe, ReclaimManager.reclaim_expire_minutes * 60f);
		InvokeRandomized(CheckEmpty, 1f, 30f, 3f);
	}

	public void RemoveMe()
	{
		Kill();
	}

	public void CheckEmpty()
	{
		if (ReclaimManager.instance.GetReclaimForPlayer(playerSteamID, reclaimID) == null && !isBeingLooted)
		{
			Kill();
		}
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (baseEntity.InSafeZone() && baseEntity.userID != playerSteamID)
		{
			return false;
		}
		if (onlyOwnerLoot && baseEntity.userID != playerSteamID)
		{
			return false;
		}
		ReclaimManager.PlayerReclaimEntry reclaimForPlayer = ReclaimManager.instance.GetReclaimForPlayer(baseEntity.userID, reclaimID);
		if (reclaimForPlayer != null)
		{
			for (int num = reclaimForPlayer.inventory.itemList.Count - 1; num >= 0; num--)
			{
				reclaimForPlayer.inventory.itemList[num].MoveToContainer(base.inventory);
			}
			ReclaimManager.instance.RemoveEntry(reclaimForPlayer);
		}
		isBeingLooted = true;
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		isBeingLooted = false;
		if (base.inventory.itemList.Count > 0)
		{
			ReclaimManager.instance.AddPlayerReclaim(playerSteamID, base.inventory.itemList, 0uL, "", reclaimID);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.lootableCorpse = Pool.Get<ProtoBuf.LootableCorpse>();
		info.msg.lootableCorpse.playerID = playerSteamID;
		info.msg.lootableCorpse.underwearSkin = (uint)reclaimID;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.lootableCorpse != null)
		{
			playerSteamID = info.msg.lootableCorpse.playerID;
			reclaimID = (int)info.msg.lootableCorpse.underwearSkin;
		}
	}
}
