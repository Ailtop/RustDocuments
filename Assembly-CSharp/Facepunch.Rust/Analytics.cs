using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Network;
using Newtonsoft.Json;
using UnityEngine;

namespace Facepunch.Rust;

public static class Analytics
{
	public class AzureWebInterface
	{
		public static readonly AzureWebInterface client = new AzureWebInterface(isClient: true);

		public static readonly AzureWebInterface server = new AzureWebInterface(isClient: false);

		public bool IsClient;

		public int MaxRetries = 1;

		public int FlushSize = 1000;

		public TimeSpan FlushDelay = TimeSpan.FromSeconds(30.0);

		private DateTime nextFlush;

		private ConcurrentQueue<List<EventRecord>> listPool = new ConcurrentQueue<List<EventRecord>>();

		private List<EventRecord> pending = new List<EventRecord>();

		private HttpClient HttpClient = new HttpClient();

		public AzureWebInterface(bool isClient)
		{
			IsClient = isClient;
		}

		public void EnqueueEvent(EventRecord point)
		{
			DateTime utcNow = DateTime.UtcNow;
			pending.Add(point);
			if (pending.Count <= FlushSize && !(utcNow > nextFlush))
			{
				return;
			}
			nextFlush = utcNow.Add(FlushDelay);
			List<EventRecord> toUpload = pending;
			Task.Run(async delegate
			{
				await UploadAsync(toUpload);
			});
			List<EventRecord> result;
			while (listPool.TryDequeue(out result))
			{
				foreach (EventRecord item in result)
				{
					EventRecord obj = item;
					Pool.Free(ref obj);
				}
				Pool.FreeList(ref result);
			}
			pending = Pool.GetList<EventRecord>();
		}

		private async Task UploadAsync(List<EventRecord> records)
		{
			string payload;
			try
			{
				payload = JsonConvert.SerializeObject(records);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				listPool.Enqueue(records);
				return;
			}
			for (int attempt = 0; attempt < MaxRetries; attempt++)
			{
				try
				{
					using StringContent content = new StringContent(payload, Encoding.UTF8, "application/json");
					content.Headers.Add(AnalyticsHeader, AnalyticsSecret);
					if (!IsClient)
					{
						content.Headers.Add("X-SERVER-IP", global::Network.Net.sv.ip);
						content.Headers.Add("X-SERVER-PORT", global::Network.Net.sv.port.ToString());
					}
					(await HttpClient.PostAsync(IsClient ? ClientAnalyticsUrl : ServerAnalyticsUrl, content)).EnsureSuccessStatusCode();
				}
				catch (Exception ex)
				{
					if (!(ex is HttpRequestException))
					{
						Debug.LogException(ex);
					}
					continue;
				}
				break;
			}
			listPool.Enqueue(records);
		}
	}

	public static class Server
	{
		public enum DeathType
		{
			Player = 0,
			NPC = 1,
			AutoTurret = 2
		}

		public static bool Enabled;

		private static Dictionary<string, float> bufferData;

		private static TimeSince lastHeldItemEvent;

		private static TimeSince lastAnalyticsSave;

		private static DateTime backupDate;

		private static bool WriteToFile => ConVar.Server.statBackup;

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

		private static DateTime currentDate => DateTime.Now;

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
			string monumentStringFromPosition = GetMonumentStringFromPosition(worldPosition);
			if (!string.IsNullOrEmpty(monumentStringFromPosition))
			{
				switch (deathType)
				{
				case DeathType.Player:
					DesignEvent("player:" + monumentStringFromPosition + "death:" + v);
					break;
				case DeathType.NPC:
					DesignEvent("player:" + monumentStringFromPosition + "death:npc:" + v);
					break;
				case DeathType.AutoTurret:
					DesignEvent("player:" + monumentStringFromPosition + "death:autoturret:" + v);
					break;
				}
			}
			else
			{
				switch (deathType)
				{
				case DeathType.Player:
					DesignEvent("player:death:" + v);
					break;
				case DeathType.NPC:
					DesignEvent("player:death:npc:" + v);
					break;
				case DeathType.AutoTurret:
					DesignEvent("player:death:autoturret:" + v);
					break;
				}
			}
		}

		private static string GetMonumentStringFromPosition(Vector3 worldPosition)
		{
			MonumentInfo monumentInfo = TerrainMeta.Path.FindMonumentWithBoundsOverlap(worldPosition);
			if (monumentInfo != null && !string.IsNullOrEmpty(monumentInfo.displayPhrase.token))
			{
				return monumentInfo.displayPhrase.token;
			}
			if (SingletonComponent<EnvironmentManager>.Instance != null && (EnvironmentManager.Get(worldPosition) & EnvironmentType.TrainTunnels) == EnvironmentType.TrainTunnels)
			{
				return "train_tunnel_display_name";
			}
			return string.Empty;
		}

		public static void Crafting(string targetItemShortname, int skinId)
		{
			if (CanSendAnalytics)
			{
				DesignEvent("player:craft:" + targetItemShortname);
				SkinUsed(targetItemShortname, skinId);
			}
		}

		public static void SkinUsed(string itemShortName, int skinId)
		{
			if (CanSendAnalytics && skinId != 0)
			{
				DesignEvent($"skinUsed:{itemShortName}:{skinId}");
			}
		}

		public static void ExcavatorStarted()
		{
			if (CanSendAnalytics)
			{
				DesignEvent("monuments:excavatorstarted");
			}
		}

		public static void ExcavatorStopped(float activeDuration)
		{
			if (CanSendAnalytics)
			{
				DesignEvent("monuments:excavatorstopped", activeDuration);
			}
		}

		public static void SlotMachineTransaction(int scrapSpent, int scrapReceived)
		{
			if (CanSendAnalytics)
			{
				DesignEvent("slots:scrapSpent", scrapSpent);
				DesignEvent("slots:scrapReceived", scrapReceived);
			}
		}

		public static void VehiclePurchased(string vehicleType)
		{
			if (CanSendAnalytics)
			{
				DesignEvent("vehiclePurchased:" + vehicleType);
			}
		}

		public static void FishCaught(ItemDefinition fish)
		{
			if (CanSendAnalytics && !(fish == null))
			{
				DesignEvent("fishCaught:" + fish.shortname);
			}
		}

		public static void VendingMachineTransaction(NPCVendingOrder npcVendingOrder, ItemDefinition purchased, int amount)
		{
			if (CanSendAnalytics && !(purchased == null))
			{
				if (npcVendingOrder == null)
				{
					DesignEvent("vendingPurchase:player:" + purchased.shortname, amount);
				}
				else
				{
					DesignEvent("vendingPurchase:static:" + purchased.shortname, amount);
				}
			}
		}

		public static void Consume(string consumedItem)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(consumedItem))
			{
				DesignEvent("player:consume:" + consumedItem);
			}
		}

		public static void TreeKilled(BaseEntity withWeapon)
		{
			if (CanSendAnalytics)
			{
				if (withWeapon != null)
				{
					DesignEvent("treekilled:" + withWeapon.ShortPrefabName);
				}
				else
				{
					DesignEvent("treekilled");
				}
			}
		}

		public static void OreKilled(OreResourceEntity entity, HitInfo info)
		{
			if (CanSendAnalytics && entity.TryGetComponent<ResourceDispenser>(out var component) && component.containedItems.Count > 0 && component.containedItems[0].itemDef != null)
			{
				if (info.WeaponPrefab != null)
				{
					DesignEvent("orekilled:" + component.containedItems[0].itemDef.shortname + ":" + info.WeaponPrefab.ShortPrefabName);
				}
				else
				{
					DesignEvent($"orekilled:{component.containedItems[0]}");
				}
			}
		}

		public static void MissionComplete(BaseMission mission)
		{
			if (CanSendAnalytics)
			{
				DesignEvent("missionComplete:" + mission.shortname, canBackup: true);
			}
		}

		public static void MissionFailed(BaseMission mission, BaseMission.MissionFailReason reason)
		{
			if (CanSendAnalytics)
			{
				DesignEvent($"missionFailed:{mission.shortname}:{reason}", canBackup: true);
			}
		}

		public static void FreeUnderwaterCrate()
		{
			if (CanSendAnalytics)
			{
				DesignEvent("loot:freeUnderWaterCrate");
			}
		}

		public static void HeldItemDeployed(ItemDefinition def)
		{
			if (CanSendAnalytics && !((float)lastHeldItemEvent < 0.1f))
			{
				lastHeldItemEvent = 0f;
				DesignEvent("heldItemDeployed:" + def.shortname);
			}
		}

		public static void UsedZipline()
		{
			if (CanSendAnalytics)
			{
				DesignEvent("usedZipline");
			}
		}

		public static void ReportCandiesCollectedByPlayer(int count)
		{
			if (Enabled)
			{
				DesignEvent("halloween:candiesCollected", count);
			}
		}

		public static void ReportPlayersParticipatedInHalloweenEvent(int count)
		{
			if (Enabled)
			{
				DesignEvent("halloween:playersParticipated", count);
			}
		}

		public static void Trigger(string message)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(message))
			{
				DesignEvent(message);
			}
		}

		private static void DesignEvent(string message, bool canBackup = false)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(message))
			{
				GA.DesignEvent(message);
				if (canBackup)
				{
					LocalBackup(message, 1f);
				}
			}
		}

		private static void DesignEvent(string message, float value, bool canBackup = false)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(message))
			{
				GA.DesignEvent(message, value);
				if (canBackup)
				{
					LocalBackup(message, value);
				}
			}
		}

		private static void DesignEvent(string message, int value, bool canBackup = false)
		{
			if (CanSendAnalytics && !string.IsNullOrEmpty(message))
			{
				GA.DesignEvent(message, value);
				if (canBackup)
				{
					LocalBackup(message, value);
				}
			}
		}

		private static string GetBackupPath(DateTime date)
		{
			return string.Format("{0}/{1}_{2}_{3}_analytics_backup.txt", ConVar.Server.GetServerFolder("analytics"), date.Day, date.Month, date.Year);
		}

		private static void LocalBackup(string message, float value)
		{
			if (!WriteToFile)
			{
				return;
			}
			if (bufferData != null && backupDate.Date != currentDate.Date)
			{
				SaveBufferIntoDateFile(backupDate);
				bufferData.Clear();
				backupDate = currentDate;
			}
			if (bufferData == null)
			{
				if (bufferData == null)
				{
					bufferData = new Dictionary<string, float>();
				}
				lastAnalyticsSave = 0f;
				backupDate = currentDate;
			}
			if (bufferData.ContainsKey(message))
			{
				bufferData[message] += value;
			}
			else
			{
				bufferData.Add(message, value);
			}
			if ((float)lastAnalyticsSave > 120f)
			{
				lastAnalyticsSave = 0f;
				SaveBufferIntoDateFile(currentDate);
				bufferData.Clear();
			}
			static void MergeBuffers(Dictionary<string, float> target, Dictionary<string, float> destination)
			{
				foreach (KeyValuePair<string, float> item in target)
				{
					if (destination.ContainsKey(item.Key))
					{
						destination[item.Key] += item.Value;
					}
					else
					{
						destination.Add(item.Key, item.Value);
					}
				}
			}
			static void SaveBufferIntoDateFile(DateTime date)
			{
				string backupPath = GetBackupPath(date);
				if (File.Exists(backupPath))
				{
					Dictionary<string, float> dictionary = (Dictionary<string, float>)JsonConvert.DeserializeObject(File.ReadAllText(backupPath), typeof(Dictionary<string, float>));
					if (dictionary != null)
					{
						MergeBuffers(dictionary, bufferData);
					}
				}
				string contents = JsonConvert.SerializeObject(bufferData);
				File.WriteAllText(GetBackupPath(date), contents);
			}
		}
	}

	[ServerVar(Name = "client_analytics_url")]
	public static string ClientAnalyticsUrl { get; set; } = "https://functions-rust-api.azurewebsites.net/api/public/analytics/rust/client";


	[ServerVar(Name = "server_analytics_url")]
	public static string ServerAnalyticsUrl { get; set; } = "https://functions-rust-api.azurewebsites.net/api/public/analytics/rust/server";


	[ServerVar(Name = "analytics_header")]
	public static string AnalyticsHeader { get; set; } = "X-API-KEY";


	[ServerVar(Name = "analytics_secret")]
	public static string AnalyticsSecret { get; set; } = "";

}
