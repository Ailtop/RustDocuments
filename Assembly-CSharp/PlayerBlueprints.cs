using Facepunch;
using Oxide.Core;
using ProtoBuf;

public class PlayerBlueprints : EntityComponent<BasePlayer>
{
	public SteamInventory steamInventory;

	public void Reset()
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			persistantPlayerInfo.unlockedItems.Clear();
		}
		else
		{
			persistantPlayerInfo.unlockedItems = Pool.GetList<int>();
		}
		base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		base.baseEntity.SendNetworkUpdate();
	}

	public void UnlockAll()
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		foreach (ItemBlueprint bp in ItemManager.bpList)
		{
			if (bp.userCraftable && !bp.defaultBlueprint && !persistantPlayerInfo.unlockedItems.Contains(bp.targetItem.itemid))
			{
				persistantPlayerInfo.unlockedItems.Add(bp.targetItem.itemid);
			}
		}
		base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
		base.baseEntity.SendNetworkUpdateImmediate();
		base.baseEntity.ClientRPCPlayer(null, base.baseEntity, "UnlockedBlueprint", 0);
	}

	public bool IsUnlocked(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (persistantPlayerInfo.unlockedItems != null)
		{
			return persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid);
		}
		return false;
	}

	public void Unlock(ItemDefinition itemDef)
	{
		PersistantPlayer persistantPlayerInfo = base.baseEntity.PersistantPlayerInfo;
		if (!persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
		{
			persistantPlayerInfo.unlockedItems.Add(itemDef.itemid);
			base.baseEntity.PersistantPlayerInfo = persistantPlayerInfo;
			base.baseEntity.SendNetworkUpdateImmediate();
			base.baseEntity.ClientRPCPlayer(null, base.baseEntity, "UnlockedBlueprint", itemDef.itemid);
			base.baseEntity.stats.Add("blueprint_studied", 1, (Stats)5);
		}
	}

	public bool HasUnlocked(ItemDefinition targetItem)
	{
		if ((bool)targetItem.Blueprint)
		{
			if (targetItem.Blueprint.NeedsSteamItem)
			{
				if (targetItem.steamItem != null && !steamInventory.HasItem(targetItem.steamItem.id))
				{
					return false;
				}
				if (base.baseEntity.UnlockAllSkins)
				{
					return true;
				}
				if (targetItem.steamItem == null)
				{
					bool flag = false;
					ItemSkinDirectory.Skin[] skins = targetItem.skins;
					for (int i = 0; i < skins.Length; i++)
					{
						ItemSkinDirectory.Skin skin = skins[i];
						if (steamInventory.HasItem(skin.id))
						{
							flag = true;
							break;
						}
					}
					if (!flag && targetItem.skins2 != null)
					{
						IPlayerItemDefinition[] skins2 = targetItem.skins2;
						foreach (IPlayerItemDefinition playerItemDefinition in skins2)
						{
							if (steamInventory.HasItem(playerItemDefinition.DefinitionId))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
				return true;
			}
			if (targetItem.Blueprint.NeedsSteamDLC)
			{
				if (base.baseEntity.UnlockAllSkins)
				{
					return true;
				}
				if (targetItem.steamDlc != null && targetItem.steamDlc.HasLicense(base.baseEntity.userID))
				{
					return true;
				}
			}
		}
		int[] defaultBlueprints = ItemManager.defaultBlueprints;
		for (int i = 0; i < defaultBlueprints.Length; i++)
		{
			if (defaultBlueprints[i] == targetItem.itemid)
			{
				return true;
			}
		}
		if (base.baseEntity.isServer)
		{
			return IsUnlocked(targetItem);
		}
		return false;
	}

	public bool CanCraft(int itemid, int skinItemId, ulong playerId)
	{
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(itemid);
		if (itemDefinition == null)
		{
			return false;
		}
		object obj = Interface.CallHook("CanCraft", this, itemDefinition, skinItemId);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (skinItemId != 0 && !base.baseEntity.UnlockAllSkins && !CheckSkinOwnership(skinItemId, playerId))
		{
			return false;
		}
		if (base.baseEntity.currentCraftLevel < (float)itemDefinition.Blueprint.workbenchLevelRequired)
		{
			return false;
		}
		if (HasUnlocked(itemDefinition))
		{
			return true;
		}
		return false;
	}

	public bool CheckSkinOwnership(int skinItemId, ulong playerId)
	{
		ItemSkinDirectory.Skin skin = ItemSkinDirectory.FindByInventoryDefinitionId(skinItemId);
		if (skin.invItem != null && skin.invItem.HasUnlocked(playerId))
		{
			return true;
		}
		return steamInventory.HasItem(skinItemId);
	}
}
