using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Network;
using Newtonsoft.Json;
using Rust;
using UnityEngine;

namespace Facepunch.Rust;

public static class Analytics
{
	public static class Azure
	{
		private struct EntitySumItem
		{
			public uint PrefabId;

			public int Count;

			public int Grade;
		}

		private struct EntityKey : IEquatable<EntityKey>
		{
			public uint PrefabId;

			public int Grade;

			public bool Equals(EntityKey other)
			{
				if (PrefabId == other.PrefabId)
				{
					return Grade == other.Grade;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (17 * 23 + PrefabId.GetHashCode()) * 31 + Grade.GetHashCode();
			}
		}

		private class PendingItemsData : Pool.IPooled
		{
			public PendingItemsKey Key;

			public int amount;

			public string category;

			public void EnterPool()
			{
				Key = default(PendingItemsKey);
				amount = 0;
				category = null;
			}

			public void LeavePool()
			{
			}
		}

		private struct PendingItemsKey : IEquatable<PendingItemsKey>
		{
			public string Item;

			public bool Consumed;

			public string Entity;

			public string Category;

			public NetworkableId EntityId;

			public bool Equals(PendingItemsKey other)
			{
				if (Item == other.Item && Entity == other.Entity && EntityId == other.EntityId && Consumed == other.Consumed)
				{
					return Category == other.Category;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return ((((17 * 23 + Item.GetHashCode()) * 31 + Consumed.GetHashCode()) * 37 + Entity.GetHashCode()) * 47 + Category.GetHashCode()) * 53 + EntityId.GetHashCode();
			}
		}

		private struct PlayerPos
		{
			public string UserId;

			public Vector3 Position;

			public Vector3 Direction;
		}

		private class TeamInfo : Pool.IPooled
		{
			public List<string> online = new List<string>();

			public List<string> offline = new List<string>();

			public int member_count;

			public void EnterPool()
			{
				online.Clear();
				offline.Clear();
				member_count = 0;
			}

			public void LeavePool()
			{
			}
		}

		public enum ResourceMode
		{
			Produced = 0,
			Consumed = 1
		}

		private static class EventIds
		{
			public const string EntityBuilt = "entity_built";

			public const string EntityPickup = "entity_pickup";

			public const string EntityDamage = "entity_damage";

			public const string PlayerRespawn = "player_respawn";

			public const string ExplosiveLaunched = "explosive_launch";

			public const string Explosion = "explosion";

			public const string ItemEvent = "item_event";

			public const string EntitySum = "entity_sum";

			public const string ItemSum = "item_sum";

			public const string ItemDespawn = "item_despawn";

			public const string ItemDropped = "item_drop";

			public const string ItemPickup = "item_pickup";

			public const string AntihackViolation = "antihack_violation";

			public const string AntihackViolationDetailed = "antihack_violation_detailed";

			public const string PlayerConnect = "player_connect";

			public const string PlayerDisconnect = "player_disconnect";

			public const string ConsumableUsed = "consumeable_used";

			public const string MedUsed = "med_used";

			public const string ResearchStarted = "research_start";

			public const string BlueprintLearned = "blueprint_learned";

			public const string TeamChanged = "team_change";

			public const string EntityAuthChange = "auth_change";

			public const string VendingOrderChanged = "vending_changed";

			public const string VendingSale = "vending_sale";

			public const string ChatMessage = "chat";

			public const string BlockUpgrade = "block_upgrade";

			public const string BlockDemolish = "block_demolish";

			public const string ItemRepair = "item_repair";

			public const string EntityRepair = "entity_repair";

			public const string ItemSkinned = "item_skinned";

			public const string ItemAggregate = "item_aggregate";

			public const string CodelockChanged = "code_change";

			public const string CodelockEntered = "code_enter";

			public const string SleepingBagAssign = "sleeping_bag_assign";

			public const string FallDamage = "fall_damage";

			public const string PlayerWipeIdSet = "player_wipe_id_set";

			public const string ServerInfo = "server_info";

			public const string UnderwaterCrateUntied = "crate_untied";

			public const string VehiclePurchased = "vehicle_purchase";

			public const string NPCVendor = "npc_vendor";

			public const string BlueprintsOnline = "blueprint_aggregate_online";

			public const string PlayerPositions = "player_positions";

			public const string ProjectileInvalid = "projectile_invalid";

			public const string ItemDefinitions = "item_definitions";

			public const string KeycardSwiped = "keycard_swiped";

			public const string EntitySpawned = "entity_spawned";

			public const string EntityKilled = "entity_killed";

			public const string HackableCrateStarted = "hackable_crate_started";

			public const string HackableCrateEnded = "hackable_crate_ended";

			public const string StashHidden = "stash_hidden";

			public const string StashRevealed = "stash_reveal";

			public const string EntityManifest = "entity_manifest";

			public const string LootEntity = "loot_entity";

			public const string OnlineTeams = "online_teams";

			public const string Gambling = "gambing";

			public const string MissionComplete = "mission_complete";

			public const string PlayerPinged = "player_pinged";

			public const string BagUnclaim = "bag_unclaim";
		}

		private struct SimpleItemAmount
		{
			public string ItemName;

			public int Amount;

			public ulong Skin;

			public float Condition;

			public SimpleItemAmount(Item item)
			{
				ItemName = item.info.shortname;
				Amount = item.amount;
				Skin = item.skin;
				Condition = item.conditionNormalized;
			}
		}

		private struct FiredProjectileKey : IEquatable<FiredProjectileKey>
		{
			public ulong UserId;

			public int ProjectileId;

			public FiredProjectileKey(ulong userId, int projectileId)
			{
				UserId = userId;
				ProjectileId = projectileId;
			}

			public bool Equals(FiredProjectileKey other)
			{
				if (other.UserId == UserId)
				{
					return other.ProjectileId == ProjectileId;
				}
				return false;
			}
		}

		private class PendingFiredProjectile : Pool.IPooled
		{
			public EventRecord Record;

			public BasePlayer.FiredProjectile FiredProjectile;

			public bool Hit;

			public void EnterPool()
			{
				Hit = false;
				Record = null;
				FiredProjectile = null;
			}

			public void LeavePool()
			{
			}
		}

		private static Dictionary<int, string> geneCache = new Dictionary<int, string>();

		public static int MaxMSPerFrame = 5;

		private static Dictionary<PendingItemsKey, PendingItemsData> pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();

		private static Dictionary<FiredProjectileKey, PendingFiredProjectile> firedProjectiles = new Dictionary<FiredProjectileKey, PendingFiredProjectile>();

		public static bool Stats
		{
			get
			{
				if (!string.IsNullOrEmpty(AnalyticsSecret) || ConVar.Server.official)
				{
					return ConVar.Server.stats;
				}
				return false;
			}
		}

		private static string GetGenesAsString(GrowableEntity plant)
		{
			int key = GrowableGeneEncoding.EncodeGenesToInt(plant.Genes);
			if (!geneCache.TryGetValue(key, out var value))
			{
				return string.Join("", from x in plant.Genes.Genes
					group x by x.GetDisplayCharacter() into x
					orderby x.Key
					select x.Count() + x.Key);
			}
			return value;
		}

		private static string GetMonument(BaseEntity entity)
		{
			if (entity == null)
			{
				return null;
			}
			SpawnGroup spawnGroup = null;
			if (entity is BaseCorpse baseCorpse)
			{
				spawnGroup = baseCorpse.spawnGroup;
			}
			if (spawnGroup == null)
			{
				SpawnPointInstance component = entity.GetComponent<SpawnPointInstance>();
				if (component != null)
				{
					spawnGroup = component.parentSpawnPointUser as SpawnGroup;
				}
			}
			if (spawnGroup != null)
			{
				if (!string.IsNullOrEmpty(spawnGroup.category))
				{
					return spawnGroup.category;
				}
				if (spawnGroup.Monument != null)
				{
					return spawnGroup.Monument.name;
				}
			}
			MonumentInfo monumentInfo = TerrainMeta.Path.FindMonumentWithBoundsOverlap(entity.transform.position);
			if (monumentInfo != null)
			{
				return monumentInfo.name;
			}
			return null;
		}

		private static string GetBiome(Vector3 position)
		{
			string result = null;
			switch ((TerrainBiome.Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position))
			{
			case TerrainBiome.Enum.Arid:
				result = "arid";
				break;
			case TerrainBiome.Enum.Temperate:
				result = "grass";
				break;
			case TerrainBiome.Enum.Tundra:
				result = "tundra";
				break;
			case TerrainBiome.Enum.Arctic:
				result = "arctic";
				break;
			}
			return result;
		}

		private static bool IsOcean(Vector3 position)
		{
			return TerrainMeta.TopologyMap.GetTopology(position) == 128;
		}

		private static IEnumerator AggregateLoop()
		{
			int loop = 0;
			while (!global::Rust.Application.isQuitting)
			{
				yield return CoroutineEx.waitForSecondsRealtime(60f);
				if (Stats)
				{
					yield return TryCatch(AggregatePlayers(blueprints: false, positions: true));
					if (loop % 60 == 0)
					{
						PushServerInfo();
						yield return TryCatch(AggregateEntitiesAndItems());
						yield return TryCatch(AggregatePlayers(blueprints: true));
						yield return TryCatch(AggregateTeams());
						Dictionary<PendingItemsKey, PendingItemsData> dict = pendingItems;
						pendingItems = new Dictionary<PendingItemsKey, PendingItemsData>();
						yield return PushPendingItemsLoopAsync(dict);
					}
					loop++;
				}
			}
		}

		private static IEnumerator TryCatch(IEnumerator coroutine)
		{
			while (true)
			{
				try
				{
					if (!coroutine.MoveNext())
					{
						break;
					}
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
					break;
				}
				yield return coroutine.Current;
			}
		}

		private static IEnumerator AggregateEntitiesAndItems()
		{
			List<BaseNetworkable> entityQueue = new List<BaseNetworkable>();
			entityQueue.Clear();
			int totalCount = BaseNetworkable.serverEntities.Count;
			entityQueue.AddRange(BaseNetworkable.serverEntities);
			Dictionary<string, int> itemDict = new Dictionary<string, int>();
			Dictionary<EntityKey, int> entityDict = new Dictionary<EntityKey, int>();
			yield return null;
			UnityEngine.Debug.Log("Starting to aggregate entities & items...");
			DateTime startTime = DateTime.UtcNow;
			Stopwatch watch = Stopwatch.StartNew();
			foreach (BaseNetworkable entity in entityQueue)
			{
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
				if (entity == null || entity.IsDestroyed)
				{
					continue;
				}
				EntityKey entityKey = default(EntityKey);
				entityKey.PrefabId = entity.prefabID;
				EntityKey key = entityKey;
				if (entity is BuildingBlock buildingBlock)
				{
					key.Grade = (int)(buildingBlock.grade + 1);
				}
				entityDict.TryGetValue(key, out var value);
				entityDict[key] = value + 1;
				if (!(entity is LootContainer) && (!(entity is BasePlayer basePlayer) || !basePlayer.IsNpc) && !(entity is NPCPlayer))
				{
					if (entity is BasePlayer basePlayer2)
					{
						AddItemsToDict(basePlayer2.inventory.containerMain, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerBelt, itemDict);
						AddItemsToDict(basePlayer2.inventory.containerWear, itemDict);
					}
					else if (entity is IItemContainerEntity itemContainerEntity)
					{
						AddItemsToDict(itemContainerEntity.inventory, itemDict);
					}
					else if (entity is DroppedItemContainer droppedItemContainer && droppedItemContainer.inventory != null)
					{
						AddItemsToDict(droppedItemContainer.inventory, itemDict);
					}
				}
			}
			UnityEngine.Debug.Log($"Took {System.Math.Round(DateTime.UtcNow.Subtract(startTime).TotalSeconds, 1)}s to aggregate {totalCount} entities & items...");
			_ = DateTime.UtcNow;
			EventRecord.New("entity_sum").AddObject("counts", entityDict.Select(delegate(KeyValuePair<EntityKey, int> x)
			{
				EntitySumItem result = default(EntitySumItem);
				result.PrefabId = x.Key.PrefabId;
				result.Grade = x.Key.Grade;
				result.Count = x.Value;
				return result;
			})).Submit();
			yield return null;
			EventRecord.New("item_sum").AddObject("counts", itemDict).Submit();
			yield return null;
		}

		private static void AddItemsToDict(ItemContainer container, Dictionary<string, int> dict)
		{
			if (container == null || container.itemList == null)
			{
				return;
			}
			foreach (Item item in container.itemList)
			{
				string shortname = item.info.shortname;
				dict.TryGetValue(shortname, out var value);
				dict[shortname] = value + item.amount;
				if (item.contents != null)
				{
					AddItemsToDict(item.contents, dict);
				}
			}
		}

		private static IEnumerator PushPendingItemsLoopAsync(Dictionary<PendingItemsKey, PendingItemsData> dict)
		{
			Stopwatch watch = Stopwatch.StartNew();
			foreach (PendingItemsData value in dict.Values)
			{
				try
				{
					LogResource(value.Key.Consumed ? ResourceMode.Consumed : ResourceMode.Produced, value.category, value.Key.Item, value.amount, null, null, safezone: false, null, 0uL, value.Key.Entity);
				}
				catch (Exception exception)
				{
					UnityEngine.Debug.LogException(exception);
				}
				PendingItemsData obj = value;
				Pool.Free(ref obj);
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			dict.Clear();
		}

		public static void AddPendingItems(BaseEntity entity, string itemName, int amount, string category, bool consumed = true, bool perEntity = false)
		{
			PendingItemsKey pendingItemsKey = default(PendingItemsKey);
			pendingItemsKey.Entity = entity.ShortPrefabName;
			pendingItemsKey.Category = category;
			pendingItemsKey.Item = itemName;
			pendingItemsKey.Consumed = consumed;
			pendingItemsKey.EntityId = (perEntity ? entity.net.ID : default(NetworkableId));
			PendingItemsKey key = pendingItemsKey;
			if (!pendingItems.TryGetValue(key, out var value))
			{
				value = Pool.Get<PendingItemsData>();
				value.Key = key;
				value.category = category;
				pendingItems[key] = value;
			}
			value.amount += amount;
		}

		private static IEnumerator AggregatePlayers(bool blueprints = false, bool positions = false)
		{
			Stopwatch watch = Stopwatch.StartNew();
			List<BasePlayer> list = Pool.GetList<BasePlayer>();
			list.AddRange(BasePlayer.activePlayerList);
			Dictionary<int, int> playerBps = (blueprints ? new Dictionary<int, int>() : null);
			List<PlayerPos> playerPositions = (positions ? new List<PlayerPos>() : null);
			foreach (BasePlayer item in list)
			{
				if (item == null || item.IsDestroyed)
				{
					continue;
				}
				if (blueprints)
				{
					foreach (int unlockedItem in item.PersistantPlayerInfo.unlockedItems)
					{
						playerBps.TryGetValue(unlockedItem, out var value);
						playerBps[unlockedItem] = value + 1;
					}
				}
				if (positions)
				{
					playerPositions.Add(new PlayerPos
					{
						UserId = item.WipeId,
						Position = item.transform.position,
						Direction = item.eyes.bodyRotation.eulerAngles
					});
				}
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			if (blueprints)
			{
				EventRecord.New("blueprint_aggregate_online").AddObject("blueprints", playerBps.Select((KeyValuePair<int, int> x) => new
				{
					Key = ItemManager.FindItemDefinition(x.Key).shortname,
					value = x.Value
				})).Submit();
			}
			if (positions)
			{
				EventRecord.New("player_positions").AddObject("positions", playerPositions).AddObject("player_count", playerPositions.Count)
					.Submit();
			}
		}

		private static IEnumerator AggregateTeams()
		{
			yield return null;
			HashSet<ulong> teamIds = new HashSet<ulong>();
			int inTeam = 0;
			int notInTeam = 0;
			foreach (BasePlayer activePlayer in BasePlayer.activePlayerList)
			{
				if (activePlayer != null && !activePlayer.IsDestroyed && activePlayer.currentTeam != 0L)
				{
					teamIds.Add(activePlayer.currentTeam);
					inTeam++;
				}
				else
				{
					notInTeam++;
				}
			}
			yield return null;
			Stopwatch watch = Stopwatch.StartNew();
			List<TeamInfo> teams = Pool.GetList<TeamInfo>();
			foreach (ulong item in teamIds)
			{
				RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindTeam(item);
				if (playerTeam == null || !((playerTeam.members != null) & (playerTeam.members.Count > 0)))
				{
					continue;
				}
				TeamInfo teamInfo = Pool.Get<TeamInfo>();
				teams.Add(teamInfo);
				foreach (ulong member in playerTeam.members)
				{
					BasePlayer basePlayer = RelationshipManager.FindByID(member);
					if (basePlayer != null && !basePlayer.IsDestroyed && basePlayer.IsConnected && !basePlayer.IsSleeping())
					{
						teamInfo.online.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
					else
					{
						teamInfo.offline.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
				}
				teamInfo.member_count = teamInfo.online.Count + teamInfo.offline.Count;
				if (watch.ElapsedMilliseconds > MaxMSPerFrame)
				{
					yield return null;
					watch.Restart();
				}
			}
			EventRecord.New("online_teams").AddObject("teams", teams).AddField("users_in_team", inTeam)
				.AddField("users_not_in_team", notInTeam)
				.Submit();
			foreach (TeamInfo item2 in teams)
			{
				TeamInfo obj = item2;
				Pool.Free(ref obj);
			}
			Pool.FreeList(ref teams);
		}

		public static void Initialize()
		{
			PushItemDefinitions();
			PushEntityManifest();
			SingletonComponent<ServerMgr>.Instance.StartCoroutine(AggregateLoop());
		}

		private static void PushServerInfo()
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("server_info").AddField("seed", World.Seed).AddField("size", World.Size)
					.AddField("url", World.Url)
					.AddField("wipe_id", SaveRestore.WipeId)
					.AddField("ip_convar", global::Network.Net.sv.ip)
					.AddField("port_convar", global::Network.Net.sv.port)
					.AddField("net_protocol", global::Network.Net.sv.ProtocolId)
					.AddField("protocol_network", 2392)
					.AddField("protocol_save", 238)
					.AddField("changeset", BuildInfo.Current?.Scm.ChangeId ?? "0")
					.AddField("unity_version", UnityEngine.Application.unityVersion)
					.AddField("branch", BuildInfo.Current?.Scm.Branch ?? "empty")
					.AddField("server_tags", ConVar.Server.tags)
					.AddField("device_id", SystemInfo.deviceUniqueIdentifier)
					.AddField("network_id", global::Network.Net.sv.GetLastUIDGiven())
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void PushItemDefinitions()
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (GameManifest.Current == null || BuildInfo.Current?.Scm?.ChangeId == null)
				{
					return;
				}
				EventRecord.New("item_definitions").AddObject("items", from x in ItemManager.itemDictionary
					select x.Value into x
					select new
					{
						item_id = x.itemid,
						shortname = x.shortname,
						craft_time = (x.Blueprint?.time ?? 0f),
						workbench = (x.Blueprint?.workbenchLevelRequired ?? 0),
						category = x.category.ToString(),
						display_name = x.displayName.english,
						despawn_rarity = x.despawnRarity,
						ingredients = x.Blueprint?.ingredients.Select((ItemAmount y) => new
						{
							shortname = y.itemDef.shortname,
							amount = (int)y.amount
						})
					}).AddField("changeset", BuildInfo.Current.Scm.ChangeId)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void PushEntityManifest()
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (!(GameManifest.Current == null) && BuildInfo.Current?.Scm?.ChangeId != null)
				{
					EventRecord.New("entity_manifest").AddObject("entities", GameManifest.Current.entities.Select((string x) => new
					{
						shortname = Path.GetFileNameWithoutExtension(x),
						prefab_id = StringPool.Get(x.ToLower())
					})).AddField("changeset", BuildInfo.Current?.Scm.ChangeId ?? "editor")
						.Submit();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFiredProjectile(BasePlayer player, BasePlayer.FiredProjectile projectile, Guid projectileGroupId)
		{
			if (!Stats || !HighFrequencyStats)
			{
				return;
			}
			try
			{
				EventRecord record = EventRecord.New("entity_damage").AddField("start_pos", projectile.position).AddField("start_vel", projectile.initialVelocity)
					.AddField("velocity_inherit", projectile.inheritedVelocity)
					.AddField("ammo_item", projectile.itemDef?.shortname)
					.AddField("weapon", projectile.weaponSource)
					.AddField("projectile_group", projectileGroupId)
					.AddField("projectile_id", projectile.id)
					.AddField("attacker", player)
					.AddField("look_dir", player.tickViewAngles)
					.AddField("model_state", (player.modelStateTick ?? player.modelState).flags);
				PendingFiredProjectile pendingFiredProjectile = Pool.Get<PendingFiredProjectile>();
				pendingFiredProjectile.Record = record;
				pendingFiredProjectile.FiredProjectile = projectile;
				firedProjectiles[new FiredProjectileKey(player.userID, projectile.id)] = pendingFiredProjectile;
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFiredProjectileRemoved(BasePlayer player, BasePlayer.FiredProjectile projectile)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				FiredProjectileKey key = new FiredProjectileKey(player.userID, projectile.id);
				if (!firedProjectiles.TryGetValue(key, out var value))
				{
					UnityEngine.Debug.LogWarning($"Can't find projectile for player '{player}' with id {projectile.id}");
					return;
				}
				if (!value.Hit)
				{
					EventRecord record = value.Record;
					if (projectile.updates.Count > 0)
					{
						record.AddObject("projectile_updates", projectile.updates);
					}
					record.Submit();
				}
				Pool.Free(ref value);
				firedProjectiles.Remove(key);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnQuarryItem(ResourceMode mode, string item, int amount, MiningQuarry sourceEntity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item, amount, "quarry", mode == ResourceMode.Consumed);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExcavatorProduceItem(Item item, BaseEntity sourceEntity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				AddPendingItems(sourceEntity, item.info.shortname, item.amount, "excavator", consumed: false);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExcavatorConsumeFuel(Item item, int amount, BaseEntity dieselEngine)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "excavator", item.info.shortname, amount, dieselEngine, null, safezone: false, null, 0uL);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCraftItem(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "craft", item, amount, null, null, inSafezone, workbench, player?.userID ?? 0);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCraftMaterialConsumed(string item, int amount, BasePlayer player, BaseEntity workbench, bool inSafezone, string targetItem)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(safezone: inSafezone, workbench: workbench, targetItem: targetItem, mode: ResourceMode.Consumed, category: "craft", itemName: item, amount: amount, sourceEntity: null, tool: null, steamId: player?.userID ?? 0);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnConsumableUsed(BasePlayer player, Item item)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("consumeable_used").AddField("player", player).AddField("item", item)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntitySpawned(BaseEntity entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				trackedSpawnedIds.Add(entity.net.ID);
				EventRecord.New("entity_spawned").AddField("entity", entity).Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void TryLogEntityKilled(BaseNetworkable entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (trackedSpawnedIds.Contains(entity.net.ID))
				{
					EventRecord.New("entity_killed").AddField("entity", entity).Submit();
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnMedUsed(string itemName, BasePlayer player, BasePlayer target)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("med_used").AddField("player", player).AddField("target", target)
					.AddField("item_name", itemName)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCodelockChanged(BasePlayer player, CodeLock codeLock, string oldCode, string newCode, bool isGuest)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("code_change").AddField("player", player).AddField("codelock", codeLock)
					.AddField("old_code", oldCode)
					.AddField("new_code", newCode)
					.AddField("is_guest", isGuest)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnCodeLockEntered(BasePlayer player, CodeLock codeLock, bool isGuest)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("code_enter").AddField("player", player).AddField("codelock", codeLock)
					.AddField("is_guest", isGuest)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTeamChanged(string change, ulong teamId, ulong teamLeader, ulong user, List<ulong> members)
		{
			if (!Stats)
			{
				return;
			}
			List<string> obj = Pool.GetList<string>();
			try
			{
				if (members != null)
				{
					foreach (ulong member in members)
					{
						obj.Add(SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(member));
					}
				}
				EventRecord.New("team_change").AddField("team_leader", SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(teamLeader)).AddField("team", teamId)
					.AddField("target_user", SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(user))
					.AddField("change", change)
					.AddObject("users", obj)
					.AddField("member_count", members.Count)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
			Pool.FreeList(ref obj);
		}

		public static void OnEntityAuthChanged(BaseEntity entity, BasePlayer player, IEnumerable<ulong> authedList, string change, ulong targetUser)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(targetUser);
				EventRecord.New("auth_change").AddField("entity", entity).AddField("player", player)
					.AddField("target", userWipeId)
					.AddObject("auth_list", authedList.Select((ulong x) => SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(x)))
					.AddField("change", change)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSleepingBagAssigned(BasePlayer player, SleepingBag bag, ulong targetUser)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				string value = ((targetUser != 0L) ? SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(targetUser) : "");
				EventRecord.New("sleeping_bag_assign").AddField("entity", bag).AddField("player", player)
					.AddField("target", value)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFallDamage(BasePlayer player, float velocity, float damage)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("fall_damage").AddField("player", player).AddField("velocity", velocity)
					.AddField("damage", damage)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnResearchStarted(BasePlayer player, BaseEntity entity, Item item, int scrapCost)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("research_start").AddField("player", player).AddField("item", item.info.shortname)
					.AddField("scrap", scrapCost)
					.AddField("entity", entity)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBlueprintLearned(BasePlayer player, ItemDefinition item, string reason, BaseEntity entity = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("blueprint_learned").AddField("player", player).AddField("item", item.shortname)
					.AddField("reason", reason)
					.AddField("entity", entity)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemRecycled(string item, int amount, Recycler recycler)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Consumed, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnRecyclerItemProduced(string item, int amount, Recycler recycler, Item sourceItem)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "recycler", item, amount, recycler, null, safezone: false, null, recycler.LastLootedBy, null, sourceItem);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnGatherItem(string item, int amount, BaseEntity sourceEntity, BasePlayer player, AttackEntity weapon = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				LogResource(ResourceMode.Produced, "gather", item, amount, sourceEntity, weapon, safezone: false, null, player.userID);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFirstLooted(BaseEntity entity, BasePlayer player)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (entity is LootContainer lootContainer)
				{
					LogItemsLooted(player, entity, lootContainer.inventory);
					EventRecord.New("loot_entity").AddField("entity", entity).AddField("player", player)
						.AddField("monument", GetMonument(entity))
						.AddField("biome", GetBiome(entity.transform.position))
						.Submit();
				}
				else if (entity is LootableCorpse lootableCorpse)
				{
					ItemContainer[] containers = lootableCorpse.containers;
					foreach (ItemContainer container in containers)
					{
						LogItemsLooted(player, entity, container);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLootContainerDestroyed(LootContainer entity, BasePlayer player, AttackEntity weapon)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (entity.DropsLoot && player != null && Vector3.Distance(entity.transform.position, player.transform.position) < 50f && entity.inventory?.itemList != null && entity.inventory.itemList.Count > 0)
				{
					LogItemsLooted(player, entity, entity.inventory, weapon);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityDestroyed(BaseNetworkable entity)
		{
			TryLogEntityKilled(entity);
			if (!Stats)
			{
				return;
			}
			try
			{
				if (!(entity is LootContainer lootContainer) || !lootContainer.FirstLooted)
				{
					return;
				}
				foreach (Item item in lootContainer.inventory.itemList)
				{
					OnItemDespawn(lootContainer, item, 3, lootContainer.LastLootedBy);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityBuilt(BaseEntity entity, BasePlayer player)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("entity_built").AddField("player", player).AddField("entity", entity);
				if (entity is SleepingBag)
				{
					int sleepingBagCount = SleepingBag.GetSleepingBagCount(player.userID);
					eventRecord.AddField("bags_active", sleepingBagCount);
					eventRecord.AddField("max_sleeping_bags", ConVar.Server.max_sleeping_bags);
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnKeycardSwiped(BasePlayer player, CardReader cardReader)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("keycard_swiped").AddField("player", player).AddField("card_level", cardReader.accessLevel)
					.AddField("entity", cardReader)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLockedCrateStarted(BasePlayer player, HackableLockedCrate crate)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("hackable_crate_started").AddField("player", player).AddField("entity", crate)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnLockedCrateFinished(ulong player, HackableLockedCrate crate)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(player);
				EventRecord.New("hackable_crate_ended").AddField("player_userid", userWipeId).AddField("entity", crate)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnStashHidden(BasePlayer player, StashContainer entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("stash_hidden").AddField("player", player).AddField("entity", entity)
					.AddField("owner", SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(entity.OwnerID))
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnStashRevealed(BasePlayer player, StashContainer entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("stash_reveal").AddField("player", player).AddField("entity", entity)
					.AddField("owner", SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(entity.OwnerID))
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnAntihackViolation(BasePlayer player, int type, string message)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation").AddField("player", player).AddField("violation_type", type)
					.AddField("message", message)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEyehackViolation(BasePlayer player, Vector3 eyePos)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 6)
					.AddField("eye_pos", eyePos)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnNoclipViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount, Collider collider)
		{
			if (!Stats || !HighFrequencyStats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 1)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("collider_name", collider.name)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFlyhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 3)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnProjectileHackViolation(BasePlayer.FiredProjectile projectile)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				FiredProjectileKey key = new FiredProjectileKey(projectile.attacker.userID, projectile.id);
				if (!firedProjectiles.TryGetValue(key, out var value))
				{
					UnityEngine.Debug.LogWarning($"Can't find projectile for player '{projectile.attacker}' with id {projectile.id}");
				}
				else
				{
					value.Record.AddField("projectile_invalid", value: true).AddObject("updates", projectile.updates);
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSpeedhackViolation(BasePlayer player, Vector3 startPos, Vector3 endPos, int tickCount)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 2)
					.AddField("start_pos", startPos)
					.AddField("end_pos", endPos)
					.AddField("tick_count", tickCount)
					.AddField("distance", Vector3.Distance(startPos, endPos))
					.AddField("distance_2d", Vector3Ex.Distance2D(startPos, endPos))
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnTerrainHackViolation(BasePlayer player)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("antihack_violation_detailed").AddField("player", player).AddField("violation_type", 10)
					.AddField("seed", World.Seed)
					.AddField("size", World.Size)
					.AddField("map_url", World.Url)
					.AddField("map_checksum", World.Checksum)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityTakeDamage(HitInfo info, bool isDeath)
		{
			if (!Stats || !HighFrequencyStats)
			{
				return;
			}
			try
			{
				BasePlayer initiatorPlayer = info.InitiatorPlayer;
				BasePlayer basePlayer = info.HitEntity as BasePlayer;
				if ((info.Initiator == null && !isDeath) || ((initiatorPlayer == null || initiatorPlayer.IsNpc || initiatorPlayer.IsBot) && (basePlayer == null || basePlayer.IsNpc || basePlayer.IsBot)))
				{
					return;
				}
				EventRecord eventRecord = null;
				float value = -1f;
				float value2 = -1f;
				if (initiatorPlayer != null)
				{
					if (info.IsProjectile())
					{
						FiredProjectileKey key = new FiredProjectileKey(initiatorPlayer.userID, info.ProjectileID);
						if (firedProjectiles.TryGetValue(key, out var value3))
						{
							eventRecord = value3.Record;
							value = Vector3.Distance(info.HitNormalWorld, value3.FiredProjectile.initialPosition);
							value = Vector3Ex.Distance2D(info.HitNormalWorld, value3.FiredProjectile.initialPosition);
							value3.Hit = info.DidHit;
						}
					}
					else
					{
						value = Vector3.Distance(info.HitNormalWorld, initiatorPlayer.eyes.position);
						value = Vector3Ex.Distance2D(info.HitNormalWorld, initiatorPlayer.eyes.position);
					}
				}
				if (eventRecord == null)
				{
					eventRecord = EventRecord.New("entity_damage");
				}
				eventRecord.AddField("is_hit", value: true).AddField("is_headshot", info.isHeadshot).AddField("victim", info.HitEntity)
					.AddField("damage", info.damageTypes.Total())
					.AddField("damage_type", info.damageTypes.GetMajorityDamageType().ToString())
					.AddField("pos_world", info.HitPositionWorld)
					.AddField("pos_local", info.HitPositionLocal)
					.AddField("point_start", info.PointStart)
					.AddField("point_end", info.PointEnd)
					.AddField("normal_world", info.HitNormalWorld)
					.AddField("normal_local", info.HitNormalLocal)
					.AddField("distance_cl", info.ProjectileDistance)
					.AddField("distance", value)
					.AddField("distance_2d", value2);
				if (!info.IsProjectile())
				{
					eventRecord.AddField("weapon", info.Weapon);
					eventRecord.AddField("attacker", info.Initiator);
				}
				if (info.HitBone != 0)
				{
					eventRecord.AddField("bone", info.HitBone).AddField("bone_name", info.boneName).AddField("hit_area", (int)info.boneArea);
				}
				if (info.ProjectileID != 0)
				{
					eventRecord.AddField("projectile_id", info.ProjectileID).AddField("projectile_integrity", info.ProjectileIntegrity).AddField("projectile_hits", info.ProjectileHits)
						.AddField("trajectory_mismatch", info.ProjectileTrajectoryMismatch)
						.AddField("travel_time", info.ProjectileTravelTime)
						.AddField("projectile_velocity", info.ProjectileVelocity)
						.AddField("projectile_prefab", info.ProjectilePrefab.name);
				}
				if (initiatorPlayer != null && !info.IsProjectile())
				{
					eventRecord.AddField("attacker_eye_pos", initiatorPlayer.eyes.position);
					eventRecord.AddField("attacker_eye_dir", initiatorPlayer.eyes.BodyForward());
					if (initiatorPlayer.GetType() == typeof(BasePlayer))
					{
						eventRecord.AddField("attacker_life", initiatorPlayer.respawnId);
					}
					if (isDeath)
					{
						eventRecord.AddObject("attacker_worn", initiatorPlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
						eventRecord.AddObject("attacker_hotbar", initiatorPlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
					}
				}
				if (basePlayer != null)
				{
					eventRecord.AddField("victim_life", basePlayer.respawnId);
					eventRecord.AddObject("victim_worn", basePlayer.inventory.containerWear.itemList.Select((Item x) => new SimpleItemAmount(x)));
					eventRecord.AddObject("victim_hotbar", basePlayer.inventory.containerBelt.itemList.Select((Item x) => new SimpleItemAmount(x)));
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerRespawned(BasePlayer player, BaseEntity targetEntity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("player_respawn").AddField("player", player).AddField("bag", targetEntity)
					.AddField("life_id", player.respawnId)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExplosiveLaunched(BasePlayer player, BaseEntity explosive, BaseEntity launcher = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("explosive_launch").AddField("player", player).AddField("explosive", explosive)
					.AddField("explosive_velocity", explosive.GetWorldVelocity())
					.AddField("explosive_direction", explosive.GetWorldVelocity().normalized);
				if (launcher != null)
				{
					eventRecord.AddField("launcher", launcher);
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnExplosion(TimedExplosive explosive)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("explosion").AddField("entity", explosive).Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemDespawn(BaseEntity itemContainer, Item item, int dropReason, ulong userId)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("item_despawn").AddField("entity", itemContainer).AddField("item", item)
					.AddField("drop_reason", dropReason);
				if (userId != 0L)
				{
					eventRecord.AddField("player_userid", userId);
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemDropped(BasePlayer player, WorldItem entity, DroppedItem.DropReasonEnum dropReason)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("item_drop").AddField("player", player).AddField("entity", entity)
					.AddField("item", entity.GetItem())
					.AddField("drop_reason", (int)dropReason)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemPickup(BasePlayer player, WorldItem entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("item_pickup").AddField("player", player).AddField("entity", entity)
					.AddField("item", entity.GetItem())
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerConnected(Connection connection)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				EventRecord.New("player_connect").AddField("player_userid", userWipeId).AddField("username", connection.username)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerDisconnected(Connection connection, string reason)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(connection.userid);
				EventRecord.New("player_disconnect").AddField("player_userid", userWipeId).AddField("username", connection.username)
					.AddField("reason", reason)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityPickedUp(BasePlayer player, BaseEntity entity)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("entity_pickup").AddField("player", player).AddField("entity", entity)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnChatMessage(BasePlayer player, string message, int channel)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("chat").AddField("player", player).AddField("message", message)
					.AddField("channel", channel)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnVendingMachineOrderChanged(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, bool added)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				EventRecord.New("vending_changed").AddField("player", player).AddField("entity", vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("change", added ? "added" : "removed")
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuyFromVendingMachine(BasePlayer player, VendingMachine vendingMachine, int sellItemId, int sellAmount, bool sellingBp, int buyItemId, int buyAmount, bool buyingBp, int numberOfTransactions, BaseEntity drone = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				ItemDefinition itemDefinition = ItemManager.FindItemDefinition(sellItemId);
				ItemDefinition itemDefinition2 = ItemManager.FindItemDefinition(buyItemId);
				EventRecord.New("vending_sale").AddField("player", player).AddField("entity", vendingMachine)
					.AddField("sell_item", itemDefinition.shortname)
					.AddField("sell_amount", sellAmount)
					.AddField("buy_item", itemDefinition2.shortname)
					.AddField("buy_amount", buyAmount)
					.AddField("transactions", numberOfTransactions)
					.AddField("is_selling_bp", sellingBp)
					.AddField("is_buying_bp", buyingBp)
					.AddField("drone_terminal", drone)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnNPCVendor(BasePlayer player, NPCTalking vendor, int scrapCost, string action)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("npc_vendor").AddField("player", player).AddField("vendor", vendor)
					.AddField("scrap_amount", scrapCost)
					.AddField("action", action)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		private static void LogItemsLooted(BasePlayer looter, BaseEntity entity, ItemContainer container, AttackEntity tool = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				if (entity == null || container == null)
				{
					return;
				}
				foreach (Item item in container.itemList)
				{
					if (item != null)
					{
						string shortname = item.info.shortname;
						int amount = item.amount;
						ulong steamId = looter?.userID ?? 0;
						LogResource(ResourceMode.Produced, "loot", shortname, amount, entity, tool, safezone: false, null, steamId);
					}
				}
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void LogResource(ResourceMode mode, string category, string itemName, int amount, BaseEntity sourceEntity = null, AttackEntity tool = null, bool safezone = false, BaseEntity workbench = null, ulong steamId = 0uL, string sourceEntityPrefab = null, Item sourceItem = null, string targetItem = null)
		{
			if (!Stats || !HighFrequencyStats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("item_event").AddField("item_mode", mode.ToString()).AddField("category", category)
					.AddField("item_name", itemName)
					.AddField("amount", amount);
				if (sourceEntity != null)
				{
					eventRecord.AddField("entity", sourceEntity);
					string biome = GetBiome(sourceEntity.transform.position);
					if (biome != null)
					{
						eventRecord.AddField("biome", biome);
					}
					if (IsOcean(sourceEntity.transform.position))
					{
						eventRecord.AddField("ocean", value: true);
					}
					string monument = GetMonument(sourceEntity);
					if (monument != null)
					{
						eventRecord.AddField("monument", monument);
					}
				}
				if (sourceEntityPrefab != null)
				{
					eventRecord.AddField("entity_prefab", sourceEntityPrefab);
				}
				if (tool != null)
				{
					eventRecord.AddField("tool", tool);
				}
				if (safezone)
				{
					eventRecord.AddField("safezone", value: true);
				}
				if (workbench != null)
				{
					eventRecord.AddField("workbench", workbench);
				}
				if (sourceEntity is GrowableEntity plant)
				{
					eventRecord.AddField("genes", GetGenesAsString(plant));
				}
				if (sourceItem != null)
				{
					eventRecord.AddField("source_item", sourceItem.info.shortname);
				}
				if (targetItem != null)
				{
					eventRecord.AddField("target_item", targetItem);
				}
				if (steamId != 0L)
				{
					string userWipeId = SingletonComponent<ServerMgr>.Instance.persistance.GetUserWipeId(steamId);
					eventRecord.AddField("player_userid", userWipeId);
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnSkinChanged(BasePlayer player, RepairBench repairBench, Item item, ulong workshopId)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("item_skinned").AddField("player", player).AddField("entity", repairBench)
					.AddField("item", item)
					.AddField("new_skin", workshopId)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnItemRepaired(BasePlayer player, BaseEntity repairBench, Item itemToRepair, float conditionBefore, float maxConditionBefore)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("item_repair").AddField("player", player).AddField("entity", repairBench)
					.AddField("item", itemToRepair)
					.AddField("old_condition", conditionBefore)
					.AddField("old_max_condition", maxConditionBefore)
					.AddField("max_condition", itemToRepair.maxConditionNormalized)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnEntityRepaired(BasePlayer player, BaseEntity entity, float healthBefore, float healthAfter)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("entity_repair").AddField("player", player).AddField("entity", entity)
					.AddField("healing", healthAfter - healthBefore)
					.AddField("health_before", healthBefore)
					.AddField("health_after", healthAfter)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuildingBlockUpgraded(BasePlayer player, BuildingBlock buildingBlock, BuildingGrade.Enum targetGrade)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("block_upgrade").AddField("player", player).AddField("entity", buildingBlock)
					.AddField("old_grade", (int)buildingBlock.grade)
					.AddField("new_grade", (int)targetGrade)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBuildingBlockDemolished(BasePlayer player, BuildingBlock buildingBlock)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("block_demolish").AddField("player", player).AddField("entity", buildingBlock)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerInitializedWipeId(ulong userId, string wipeId)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("player_wipe_id_set").AddField("user_id", userId).AddField("player_wipe_id", wipeId)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnFreeUnderwaterCrate(BasePlayer player, FreeableLootContainer freeableLootContainer)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("crate_untied").AddField("player", player).AddField("entity", freeableLootContainer)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnVehiclePurchased(BasePlayer player, BaseEntity vehicle)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("vehicle_purchase").AddField("player", player).AddField("entity", vehicle)
					.AddField("price", vehicle)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnMissionComplete(BasePlayer player, BaseMission mission, BaseMission.MissionFailReason? failReason = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("mission_complete").AddField("player", player).AddField("mission", mission.shortname)
					.AddField("mission_succeed", value: true);
				if (failReason.HasValue)
				{
					eventRecord.AddField("mission_succeed", value: false).AddField("fail_reason", failReason.Value.ToString());
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnGamblingResult(BasePlayer player, BaseEntity entity, int scrapPaid, int scrapRecieved, Guid? gambleGroupId = null)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord eventRecord = EventRecord.New("gambing").AddField("player", player).AddField("entity", entity)
					.AddField("scrap_input", scrapPaid)
					.AddField("scrap_output", scrapRecieved);
				if (gambleGroupId.HasValue)
				{
					eventRecord.AddField("gamble_grouping", gambleGroupId.Value);
				}
				eventRecord.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnPlayerPinged(BasePlayer player, BasePlayer.PingType type, bool wasViaWheel)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("player_pinged").AddField("player", player).AddField("pingType", (int)type)
					.AddField("viaWheel", wasViaWheel)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}

		public static void OnBagUnclaimed(BasePlayer player, SleepingBag bag)
		{
			if (!Stats)
			{
				return;
			}
			try
			{
				EventRecord.New("bag_unclaim").AddField("player", player).AddField("entity", bag)
					.Submit();
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
	}

	public class AzureWebInterface
	{
		public static readonly AzureWebInterface client = new AzureWebInterface(isClient: true);

		public static readonly AzureWebInterface server = new AzureWebInterface(isClient: false);

		public bool IsClient;

		public int MaxRetries = 1;

		public int FlushSize = 1000;

		public TimeSpan FlushDelay = TimeSpan.FromSeconds(30.0);

		private DateTime nextFlush;

		private List<EventRecord> pending = new List<EventRecord>();

		private HttpClient HttpClient = new HttpClient();

		private static readonly MediaTypeHeaderValue JsonContentType = new MediaTypeHeaderValue("application/json")
		{
			CharSet = Encoding.UTF8.WebName
		};

		public int PendingCount => pending.Count;

		public AzureWebInterface(bool isClient)
		{
			IsClient = isClient;
		}

		public void EnqueueEvent(EventRecord point)
		{
			DateTime utcNow = DateTime.UtcNow;
			pending.Add(point);
			if (pending.Count > FlushSize || utcNow > nextFlush)
			{
				nextFlush = utcNow.Add(FlushDelay);
				List<EventRecord> toUpload = pending;
				Task.Run(async delegate
				{
					await UploadAsync(toUpload);
				});
				pending = Pool.GetList<EventRecord>();
			}
		}

		private void SerializeEvents(List<EventRecord> records, MemoryStream stream)
		{
			int num = 0;
			using StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
			streamWriter.Write("[");
			foreach (EventRecord record in records)
			{
				SerializeEvent(record, streamWriter, num);
				num++;
			}
			streamWriter.Write("]");
			streamWriter.Flush();
		}

		private void SerializeEvent(EventRecord record, StreamWriter writer, int index)
		{
			if (index > 0)
			{
				writer.Write(',');
			}
			writer.Write("{\"Timestamp\":\"");
			writer.Write(record.Timestamp.ToString("o"));
			writer.Write("\",\"Data\":{");
			bool flag = true;
			foreach (EventRecordField datum in record.Data)
			{
				if (flag)
				{
					flag = false;
				}
				else
				{
					writer.Write(',');
				}
				writer.Write("\"");
				writer.Write(datum.Key1);
				if (datum.Key2 != null)
				{
					writer.Write(datum.Key2);
				}
				writer.Write("\":");
				if (!datum.IsObject)
				{
					writer.Write('"');
				}
				if (datum.String != null)
				{
					if (datum.IsObject)
					{
						writer.Write(datum.String);
					}
					else
					{
						string @string = datum.String;
						int length = datum.String.Length;
						for (int i = 0; i < length; i++)
						{
							char c = @string[i];
							switch (c)
							{
							case '"':
							case '\\':
								writer.Write('\\');
								writer.Write(c);
								break;
							case '\n':
								writer.Write("\\n");
								break;
							case '\r':
								writer.Write("\\r");
								break;
							case '\t':
								writer.Write("\\t");
								break;
							default:
								writer.Write(c);
								break;
							}
						}
					}
				}
				else if (datum.Float.HasValue)
				{
					writer.Write(datum.Float.Value);
				}
				else if (datum.Number.HasValue)
				{
					writer.Write(datum.Number.Value);
				}
				else if (datum.Guid.HasValue)
				{
					writer.Write(datum.Guid.Value.ToString("N"));
				}
				else if (datum.Vector.HasValue)
				{
					writer.Write('(');
					Vector3 value = datum.Vector.Value;
					writer.Write(value.x);
					writer.Write(',');
					writer.Write(value.y);
					writer.Write(',');
					writer.Write(value.z);
					writer.Write(')');
				}
				if (!datum.IsObject)
				{
					writer.Write("\"");
				}
			}
			writer.Write('}');
			writer.Write('}');
		}

		private async Task UploadAsync(List<EventRecord> records)
		{
			MemoryStream stream = Pool.Get<MemoryStream>();
			stream.Position = 0L;
			stream.SetLength(0L);
			try
			{
				SerializeEvents(records, stream);
				for (int attempt = 0; attempt < MaxRetries; attempt++)
				{
					try
					{
						using ByteArrayContent content = new ByteArrayContent(stream.GetBuffer(), 0, (int)stream.Length);
						content.Headers.ContentType = JsonContentType;
						if (!string.IsNullOrEmpty(AnalyticsSecret))
						{
							content.Headers.Add(AnalyticsHeader, AnalyticsSecret);
						}
						else
						{
							content.Headers.Add(AnalyticsHeader, AnalyticsPublicKey);
						}
						if (!IsClient)
						{
							content.Headers.Add("X-SERVER-IP", global::Network.Net.sv.ip);
							content.Headers.Add("X-SERVER-PORT", global::Network.Net.sv.port.ToString());
						}
						(await HttpClient.PostAsync(IsClient ? ClientAnalyticsUrl : ServerAnalyticsUrl, content)).EnsureSuccessStatusCode();
						break;
					}
					catch (Exception ex)
					{
						if (!(ex is HttpRequestException))
						{
							UnityEngine.Debug.LogException(ex);
						}
					}
				}
			}
			catch (Exception ex2)
			{
				if (IsClient)
				{
					UnityEngine.Debug.LogWarning(ex2.ToString());
				}
				else
				{
					UnityEngine.Debug.LogException(ex2);
				}
			}
			finally
			{
				foreach (EventRecord record in records)
				{
					EventRecord obj = record;
					Pool.Free(ref obj);
				}
				Pool.FreeList(ref records);
				Pool.FreeMemoryStream(ref stream);
			}
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

	public static HashSet<string> StatsBlacklist;

	private static HashSet<NetworkableId> trackedSpawnedIds = new HashSet<NetworkableId>();

	public static string ClientAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/client";


	[ServerVar(Name = "server_analytics_url")]
	public static string ServerAnalyticsUrl { get; set; } = "https://rust-api.facepunch.com/api/public/analytics/rust/server";


	[ServerVar(Name = "analytics_header", Saved = true)]
	public static string AnalyticsHeader { get; set; } = "X-API-KEY";


	[ServerVar(Name = "analytics_secret", Saved = true)]
	public static string AnalyticsSecret { get; set; } = "";


	public static string AnalyticsPublicKey { get; set; } = "pub878ABLezSB6onshSwBCRGYDCpEI";


	[ServerVar(Name = "high_freq_stats", Saved = true)]
	public static bool HighFrequencyStats { get; set; } = true;


	[ServerVar(Name = "stats_blacklist", Saved = true)]
	public static string stats_blacklist
	{
		get
		{
			if (StatsBlacklist != null)
			{
				return string.Join(",", StatsBlacklist);
			}
			return "";
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				StatsBlacklist = null;
				return;
			}
			StatsBlacklist = new HashSet<string>(value.Split(','));
		}
	}

	[ServerVar(Name = "pending_analytics")]
	[ClientVar(Name = "pending_analytics")]
	public static void GetPendingAnalytics(ConsoleSystem.Arg arg)
	{
		int pendingCount = AzureWebInterface.server.PendingCount;
		arg.ReplyWith($"Pending: {pendingCount}");
	}
}
