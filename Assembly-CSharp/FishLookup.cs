using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class FishLookup : PrefabAttribute
{
	public ItemModFishable FallbackFish;

	private static ItemModFishable[] AvailableFish;

	public static ItemDefinition[] BaitItems;

	private static TimeSince lastShuffle;

	public const int ALL_FISH_COUNT = 9;

	public const string ALL_FISH_ACHIEVEMENT_NAME = "PRO_ANGLER";

	public static void LoadFish()
	{
		if (AvailableFish != null)
		{
			if ((float)lastShuffle > 5f)
			{
				ArrayEx.Shuffle(AvailableFish, (uint)UnityEngine.Random.Range(0, 10000));
			}
			return;
		}
		List<ItemModFishable> obj = Pool.GetList<ItemModFishable>();
		List<ItemDefinition> obj2 = Pool.GetList<ItemDefinition>();
		foreach (ItemDefinition item in ItemManager.itemList)
		{
			ItemModFishable component;
			if (item.TryGetComponent<ItemModFishable>(out component))
			{
				obj.Add(component);
			}
			ItemModCompostable component2;
			if (item.TryGetComponent<ItemModCompostable>(out component2) && component2.BaitValue > 0f)
			{
				obj2.Add(item);
			}
		}
		AvailableFish = obj.ToArray();
		BaitItems = obj2.ToArray();
		Pool.FreeList(ref obj);
		Pool.FreeList(ref obj2);
	}

	public ItemDefinition GetFish(Vector3 worldPos, WaterBody bodyType, ItemDefinition lure, out ItemModFishable fishable, ItemModFishable ignoreFish)
	{
		LoadFish();
		ItemModCompostable component;
		float num = (lure.TryGetComponent<ItemModCompostable>(out component) ? component.BaitValue : 0f);
		WaterBody.FishingTag fishingTag = ((bodyType != null) ? bodyType.FishingType : WaterBody.FishingTag.Ocean);
		float num2 = WaterLevel.GetOverallWaterDepth(worldPos, true, null, true);
		if (worldPos.y < -10f)
		{
			num2 = 10f;
		}
		int num3 = UnityEngine.Random.Range(0, AvailableFish.Length);
		for (int i = 0; i < AvailableFish.Length; i++)
		{
			num3++;
			if (num3 >= AvailableFish.Length)
			{
				num3 = 0;
			}
			ItemModFishable itemModFishable = AvailableFish[num3];
			if (itemModFishable.CanBeFished && !(itemModFishable.MinimumBaitLevel > num) && (!(itemModFishable.MaximumBaitLevel > 0f) || !(num > itemModFishable.MaximumBaitLevel)) && !(itemModFishable == ignoreFish) && (itemModFishable.RequiredTag == (WaterBody.FishingTag)(-1) || (itemModFishable.RequiredTag & fishingTag) != 0) && ((fishingTag & WaterBody.FishingTag.Ocean) != WaterBody.FishingTag.Ocean || ((!(itemModFishable.MinimumWaterDepth > 0f) || !(num2 < itemModFishable.MinimumWaterDepth)) && (!(itemModFishable.MaximumWaterDepth > 0f) || !(num2 > itemModFishable.MaximumWaterDepth)))) && !(UnityEngine.Random.Range(0f, 1f) - num * 3f * 0.01f > itemModFishable.Chance))
			{
				fishable = itemModFishable;
				return itemModFishable.GetComponent<ItemDefinition>();
			}
		}
		fishable = FallbackFish;
		return FallbackFish.GetComponent<ItemDefinition>();
	}

	public void CheckCatchAllAchievement(BasePlayer player)
	{
		LoadFish();
		int num = 0;
		ItemModFishable[] availableFish = AvailableFish;
		foreach (ItemModFishable itemModFishable in availableFish)
		{
			if (!string.IsNullOrEmpty(itemModFishable.SteamStatName) && player.stats.steam.Get(itemModFishable.SteamStatName) > 0)
			{
				num++;
			}
		}
		if (num == 9)
		{
			player.GiveAchievement("PRO_ANGLER");
		}
	}

	protected override Type GetIndexedType()
	{
		return typeof(FishLookup);
	}
}
