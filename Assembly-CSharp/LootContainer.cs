using System;
using Oxide.Core;
using Rust;
using UnityEngine;

public class LootContainer : StorageContainer
{
	public enum spawnType
	{
		GENERIC = 0,
		PLAYER = 1,
		TOWN = 2,
		AIRDROP = 3,
		CRASHSITE = 4,
		ROADSIDE = 5
	}

	[Serializable]
	public struct LootSpawnSlot
	{
		public LootSpawn definition;

		public int numberToSpawn;

		public float probability;
	}

	public bool destroyOnEmpty = true;

	public LootSpawn lootDefinition;

	public int maxDefinitionsToSpawn;

	public float minSecondsBetweenRefresh = 3600f;

	public float maxSecondsBetweenRefresh = 7200f;

	public bool initialLootSpawn = true;

	public float xpLootedScale = 1f;

	public float xpDestroyedScale = 1f;

	public bool BlockPlayerItemInput;

	public int scrapAmount;

	public string deathStat = "";

	public LootSpawnSlot[] LootSpawnSlots;

	public spawnType SpawnType;

	public bool FirstLooted;

	private static ItemDefinition scrapDef;

	public bool shouldRefreshContents
	{
		get
		{
			if (minSecondsBetweenRefresh > 0f)
			{
				return maxSecondsBetweenRefresh > 0f;
			}
			return false;
		}
	}

	public override void ResetState()
	{
		FirstLooted = false;
		base.ResetState();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (initialLootSpawn)
		{
			SpawnLoot();
		}
		if (BlockPlayerItemInput && !Rust.Application.isLoadingSave && base.inventory != null)
		{
			base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
		SetFlag(Flags.Reserved6, PlayerInventory.IsBirthday());
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (BlockPlayerItemInput && base.inventory != null)
		{
			base.inventory.SetFlag(ItemContainer.Flag.NoItemInput, b: true);
		}
	}

	public virtual void SpawnLoot()
	{
		if (base.inventory == null)
		{
			Debug.Log("CONTACT DEVELOPERS! LootContainer::PopulateLoot has null inventory!!!");
			return;
		}
		base.inventory.Clear();
		ItemManager.DoRemoves();
		if (Interface.CallHook("OnLootSpawn", this) == null)
		{
			PopulateLoot();
			if (shouldRefreshContents)
			{
				Invoke(SpawnLoot, UnityEngine.Random.Range(minSecondsBetweenRefresh, maxSecondsBetweenRefresh));
			}
		}
	}

	public int ScoreForRarity(Rarity rarity)
	{
		return rarity switch
		{
			Rarity.Common => 1, 
			Rarity.Uncommon => 2, 
			Rarity.Rare => 3, 
			Rarity.VeryRare => 4, 
			_ => 5000, 
		};
	}

	public virtual void PopulateLoot()
	{
		if (LootSpawnSlots.Length != 0)
		{
			LootSpawnSlot[] lootSpawnSlots = LootSpawnSlots;
			for (int i = 0; i < lootSpawnSlots.Length; i++)
			{
				LootSpawnSlot lootSpawnSlot = lootSpawnSlots[i];
				for (int j = 0; j < lootSpawnSlot.numberToSpawn; j++)
				{
					if (UnityEngine.Random.Range(0f, 1f) <= lootSpawnSlot.probability)
					{
						lootSpawnSlot.definition.SpawnIntoContainer(base.inventory);
					}
				}
			}
		}
		else if (lootDefinition != null)
		{
			for (int k = 0; k < maxDefinitionsToSpawn; k++)
			{
				lootDefinition.SpawnIntoContainer(base.inventory);
			}
		}
		if (SpawnType == spawnType.ROADSIDE || SpawnType == spawnType.TOWN)
		{
			foreach (Item item in base.inventory.itemList)
			{
				if (item.hasCondition)
				{
					item.condition = UnityEngine.Random.Range(item.info.condition.foundCondition.fractionMin, item.info.condition.foundCondition.fractionMax) * item.info.condition.max;
				}
			}
		}
		GenerateScrap();
	}

	public void GenerateScrap()
	{
		if (scrapAmount <= 0)
		{
			return;
		}
		if (scrapDef == null)
		{
			scrapDef = ItemManager.FindItemDefinition("scrap");
		}
		int num = scrapAmount;
		if (num > 0)
		{
			Item item = ItemManager.Create(scrapDef, num, 0uL);
			if (!item.MoveToContainer(base.inventory))
			{
				item.Drop(base.transform.position, GetInheritedDropVelocity());
			}
		}
	}

	public override void DropBonusItems(BaseEntity initiator, ItemContainer container)
	{
		base.DropBonusItems(initiator, container);
		if (initiator == null || container == null)
		{
			return;
		}
		BasePlayer basePlayer = initiator as BasePlayer;
		if (basePlayer == null || scrapAmount <= 0 || !(scrapDef != null))
		{
			return;
		}
		float num = ((basePlayer.modifiers != null) ? (1f + basePlayer.modifiers.GetValue(Modifier.ModifierType.Scrap_Yield)) : 0f);
		if (!(num > 1f))
		{
			return;
		}
		float variableValue = basePlayer.modifiers.GetVariableValue(Modifier.ModifierType.Scrap_Yield, 0f);
		float num2 = Mathf.Max((float)scrapAmount * num - (float)scrapAmount, 0f);
		variableValue += num2;
		int num3 = 0;
		if (variableValue >= 1f)
		{
			num3 = (int)variableValue;
			variableValue -= (float)num3;
		}
		basePlayer.modifiers.SetVariableValue(Modifier.ModifierType.Scrap_Yield, variableValue);
		if (num3 > 0)
		{
			Item item = ItemManager.Create(scrapDef, num3, 0uL);
			if (item != null && Interface.CallHook("OnBonusItemDrop", item, basePlayer) == null)
			{
				item.Drop(GetDropPosition() + new Vector3(0f, 0.5f, 0f), GetInheritedDropVelocity());
				Interface.CallHook("OnBonusItemDropped", item, basePlayer);
			}
		}
	}

	public override bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		if (!FirstLooted)
		{
			FirstLooted = true;
		}
		return base.OnStartBeingLooted(baseEntity);
	}

	public override void PlayerStoppedLooting(BasePlayer player)
	{
		base.PlayerStoppedLooting(player);
		if (destroyOnEmpty && (base.inventory == null || base.inventory.itemList == null || base.inventory.itemList.Count == 0))
		{
			Kill(DestroyMode.Gib);
		}
	}

	public void RemoveMe()
	{
		Kill(DestroyMode.Gib);
	}

	public override bool ShouldDropItemsIndividually()
	{
		return true;
	}

	public override void OnKilled(HitInfo info)
	{
		base.OnKilled(info);
		if (info != null && info.InitiatorPlayer != null && !string.IsNullOrEmpty(deathStat))
		{
			info.InitiatorPlayer.stats.Add(deathStat, 1, Stats.Life);
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		base.OnAttacked(info);
	}

	public override void InitShared()
	{
		base.InitShared();
	}
}
