using ConVar;
using UnityEngine;

namespace Facepunch.Rust;

public static class Analytics
{
	public static class Server
	{
		public enum DeathType
		{
			Player = 0,
			NPC = 1,
			AutoTurret = 2
		}

		public static bool Enabled;

		private static TimeSince lastHeldItemEvent;

		private static bool CanSendAnalytics
		{
			get
			{
				if (ConVar.Server.official && ConVar.Server.stats)
				{
					return Enabled;
				}
				return false;
			}
		}

		internal static void Death(BaseEntity initiator, BaseEntity weaponPrefab, Vector3 worldPosition)
		{
			if (!CanSendAnalytics || !(initiator != null))
			{
				return;
			}
			if (initiator is BasePlayer)
			{
				if (weaponPrefab != null)
				{
					Death(weaponPrefab.ShortPrefabName, worldPosition, initiator.IsNpc ? DeathType.NPC : DeathType.Player);
				}
				else
				{
					Death("player", worldPosition);
				}
			}
			else if (initiator is AutoTurret)
			{
				if (weaponPrefab != null)
				{
					Death(weaponPrefab.ShortPrefabName, worldPosition, DeathType.AutoTurret);
				}
			}
			else
			{
				Death(initiator.Categorize(), worldPosition, initiator.IsNpc ? DeathType.NPC : DeathType.Player);
			}
		}

		internal static void Death(string v, Vector3 worldPosition, DeathType deathType = DeathType.Player)
		{
			if (!CanSendAnalytics)
			{
				return;
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.FindMonumentWithBoundsOverlap(worldPosition);
			if (monumentInfo != null && !string.IsNullOrEmpty(monumentInfo.displayPhrase.token))
			{
				switch (deathType)
				{
				case DeathType.Player:
					GA.DesignEvent("player:" + monumentInfo.displayPhrase.token + "death:" + v);
					break;
				case DeathType.NPC:
					GA.DesignEvent("player:" + monumentInfo.displayPhrase.token + "death:npc:" + v);
					break;
				case DeathType.AutoTurret:
					GA.DesignEvent("player:" + monumentInfo.displayPhrase.token + "death:autoturret:" + v);
					break;
				}
			}
			else
			{
				switch (deathType)
				{
				case DeathType.Player:
					GA.DesignEvent("player:death:" + v);
					break;
				case DeathType.NPC:
					GA.DesignEvent("player:death:npc:" + v);
					break;
				case DeathType.AutoTurret:
					GA.DesignEvent("player:death:autoturret:" + v);
					break;
				}
			}
		}

		public static void Crafting(string targetItemShortname, int skinId)
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("player:craft:" + targetItemShortname);
				SkinUsed(targetItemShortname, skinId);
			}
		}

		public static void SkinUsed(string itemShortName, int skinId)
		{
			if (CanSendAnalytics && skinId != 0)
			{
				GA.DesignEvent($"skinUsed:{itemShortName}:{skinId}");
			}
		}

		public static void ExcavatorStarted()
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("monuments:excavatorstarted");
			}
		}

		public static void ExcavatorStopped(float activeDuration)
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("monuments:excavatorstopped", activeDuration);
			}
		}

		public static void SlotMachineTransaction(int scrapSpent, int scrapReceived)
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("slots:scrapSpent", scrapSpent);
				GA.DesignEvent("slots:scrapReceived", scrapReceived);
			}
		}

		public static void VehiclePurchased(string vehicleType)
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("vehiclePurchased:" + vehicleType);
			}
		}

		public static void FishCaught(ItemDefinition fish)
		{
			if (CanSendAnalytics && !(fish == null))
			{
				GA.DesignEvent("fishCaught:" + fish.shortname);
			}
		}

		public static void VendingMachineTransaction(NPCVendingOrder npcVendingOrder, ItemDefinition purchased, int amount)
		{
			if (CanSendAnalytics && !(purchased == null))
			{
				if (npcVendingOrder == null)
				{
					GA.DesignEvent("vendingPurchase:player:" + purchased.shortname, amount);
				}
				else
				{
					GA.DesignEvent("vendingPurchase:static:" + purchased.shortname, amount);
				}
			}
		}

		public static void Consume(string consumedItem)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(consumedItem))
			{
				GA.DesignEvent("player:consume:" + consumedItem);
			}
		}

		public static void TreeKilled(BaseEntity withWeapon)
		{
			if (CanSendAnalytics)
			{
				if (withWeapon != null)
				{
					GA.DesignEvent("treekilled:" + withWeapon.ShortPrefabName);
				}
				else
				{
					GA.DesignEvent("treekilled");
				}
			}
		}

		public static void OreKilled(OreResourceEntity entity, HitInfo info)
		{
			if (CanSendAnalytics && entity.TryGetComponent<ResourceDispenser>(out var component) && component.containedItems.Count > 0 && component.containedItems[0].itemDef != null)
			{
				if (info.WeaponPrefab != null)
				{
					GA.DesignEvent("orekilled:" + component.containedItems[0].itemDef.shortname + ":" + info.WeaponPrefab.ShortPrefabName);
				}
				else
				{
					GA.DesignEvent($"orekilled:{component.containedItems[0]}");
				}
			}
		}

		public static void MissionComplete(BaseMission mission)
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("missionComplete:" + mission.shortname);
			}
		}

		public static void FreeUnderwaterCrate()
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("loot:freeUnderWaterCrate");
			}
		}

		public static void HeldItemDeployed(ItemDefinition def)
		{
			if (CanSendAnalytics && !((float)lastHeldItemEvent < 0.1f))
			{
				lastHeldItemEvent = 0f;
				GA.DesignEvent("heldItemDeployed:" + def.shortname);
			}
		}

		public static void UsedZipline()
		{
			if (CanSendAnalytics)
			{
				GA.DesignEvent("usedZipline");
			}
		}
	}
}
