using System;
using ConVar;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class PlanterBox : StorageContainer, ISplashable
{
	public int soilSaturation;

	public int soilSaturationMax = 8000;

	public MeshRenderer soilRenderer;

	private static readonly float MinimumSaturationTriggerLevel = Server.optimalPlanterQualitySaturation - 0.2f;

	private static readonly float MaximumSaturationTriggerLevel = Server.optimalPlanterQualitySaturation + 0.1f;

	private TimeCachedValue<float> sunExposure;

	private TimeCachedValue<float> artificialLightExposure;

	private TimeCachedValue<float> plantTemperature;

	private TimeCachedValue<float> plantArtificalTemperature;

	private TimeSince lastRainCheck;

	public float soilSaturationFraction => (float)soilSaturation / (float)soilSaturationMax;

	public int availableIdealWaterCapacity => Mathf.Max(availableIdealWaterCapacity, Mathf.Max(idealSaturation - soilSaturation, 0));

	public int availableWaterCapacity => soilSaturationMax - soilSaturation;

	public int idealSaturation => Mathf.FloorToInt((float)soilSaturationMax * Server.optimalPlanterQualitySaturation);

	public bool BelowMinimumSaturationTriggerLevel => soilSaturationFraction < MinimumSaturationTriggerLevel;

	public bool AboveMaximumSaturationTriggerLevel => soilSaturationFraction > MaximumSaturationTriggerLevel;

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		base.inventory.SetOnlyAllowedItem(allowedItem);
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(InventoryItemFilter));
		sunExposure = new TimeCachedValue<float>
		{
			refreshCooldown = 30f,
			refreshRandomRange = 5f,
			updateValue = CalculateSunExposure
		};
		artificialLightExposure = new TimeCachedValue<float>
		{
			refreshCooldown = 60f,
			refreshRandomRange = 5f,
			updateValue = CalculateArtificialLightExposure
		};
		plantTemperature = new TimeCachedValue<float>
		{
			refreshCooldown = 20f,
			refreshRandomRange = 5f,
			updateValue = CalculatePlantTemperature
		};
		plantArtificalTemperature = new TimeCachedValue<float>
		{
			refreshCooldown = 60f,
			refreshRandomRange = 5f,
			updateValue = CalculateArtificialTemperature
		};
		lastRainCheck = 0f;
		InvokeRandomized(CalculateRainFactor, 20f, 30f, 15f);
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		if (added && ItemIsFertilizer(item))
		{
			FertilizeGrowables();
		}
	}

	public bool InventoryItemFilter(Item item, int targetSlot)
	{
		if (item == null)
		{
			return false;
		}
		if (ItemIsFertilizer(item))
		{
			return true;
		}
		return false;
	}

	private bool ItemIsFertilizer(Item item)
	{
		return item.info.shortname == "fertilizer";
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.resource = Facepunch.Pool.Get<BaseResource>();
		info.msg.resource.stage = soilSaturation;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.resource != null)
		{
			soilSaturation = info.msg.resource.stage;
		}
	}

	public void FertilizeGrowables()
	{
		int num = GetFertilizerCount();
		if (num <= 0)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			if (child == null)
			{
				continue;
			}
			GrowableEntity growableEntity = child as GrowableEntity;
			if (!(growableEntity == null) && !growableEntity.Fertilized && ConsumeFertilizer())
			{
				growableEntity.Fertilize();
				num--;
				if (num == 0)
				{
					break;
				}
			}
		}
	}

	public int GetFertilizerCount()
	{
		int num = 0;
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null && ItemIsFertilizer(slot))
			{
				num += slot.amount;
			}
		}
		return num;
	}

	public bool ConsumeFertilizer()
	{
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null && ItemIsFertilizer(slot))
			{
				int num = Mathf.Min(1, slot.amount);
				if (num > 0)
				{
					slot.UseItem(num);
					return true;
				}
			}
		}
		return false;
	}

	public int ConsumeWater(int amount, GrowableEntity ignoreEntity = null)
	{
		int num = Mathf.Min(amount, soilSaturation);
		soilSaturation -= num;
		RefreshGrowables(ignoreEntity);
		SendNetworkUpdate();
		return num;
	}

	public bool WantsSplash(ItemDefinition splashType, int amount)
	{
		if (base.IsDestroyed)
		{
			return false;
		}
		if (splashType == null || splashType.shortname == null)
		{
			return false;
		}
		if (!(splashType.shortname == "water.salt"))
		{
			return soilSaturation < soilSaturationMax;
		}
		return true;
	}

	public int DoSplash(ItemDefinition splashType, int amount)
	{
		if (splashType.shortname == "water.salt")
		{
			soilSaturation = 0;
			RefreshGrowables();
			SendNetworkUpdate();
			return amount;
		}
		int num = Mathf.Min(availableWaterCapacity, amount);
		soilSaturation += num;
		RefreshGrowables();
		SendNetworkUpdate();
		return num;
	}

	private void RefreshGrowables(GrowableEntity ignoreEntity = null)
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			GrowableEntity growableEntity;
			if (!(child == null) && !(child == ignoreEntity) && (object)(growableEntity = child as GrowableEntity) != null)
			{
				growableEntity.QueueForQualityUpdate();
			}
		}
	}

	public void ForceLightUpdate()
	{
		sunExposure?.ForceNextRun();
		artificialLightExposure?.ForceNextRun();
	}

	public void ForceTemperatureUpdate()
	{
		plantArtificalTemperature?.ForceNextRun();
	}

	public float GetSunExposure()
	{
		return sunExposure?.Get(false) ?? 0f;
	}

	private float CalculateSunExposure()
	{
		return GrowableEntity.SunRaycast(base.transform.position + new Vector3(0f, 1f, 0f));
	}

	public float GetArtificialLightExposure()
	{
		return artificialLightExposure?.Get(false) ?? 0f;
	}

	private float CalculateArtificialLightExposure()
	{
		return GrowableEntity.CalculateArtificialLightExposure(base.transform);
	}

	public float GetPlantTemperature()
	{
		return (plantTemperature?.Get(false) ?? 0f) + (plantArtificalTemperature?.Get(false) ?? 0f);
	}

	private float CalculatePlantTemperature()
	{
		return Mathf.Max(Climate.GetTemperature(base.transform.position), 15f);
	}

	private void CalculateRainFactor()
	{
		if (sunExposure.Get(false) > 0f)
		{
			float rain = Climate.GetRain(base.transform.position);
			if (rain > 0f)
			{
				soilSaturation = Mathf.Clamp(soilSaturation + Mathf.RoundToInt(4f * rain * (float)lastRainCheck), 0, soilSaturationMax);
				RefreshGrowables();
				SendNetworkUpdate();
			}
		}
		lastRainCheck = 0f;
	}

	private float CalculateArtificialTemperature()
	{
		return GrowableEntity.CalculateArtificialTemperature(base.transform);
	}
}
