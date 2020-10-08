using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Loot Bouncer", "Sorrow/Arainrr", "1.0.6")]
    [Description("Empty the containers when players do not pick up all the items")]
    public class LootBouncer : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Slap, Trade;
        private readonly Dictionary<uint, int> lootEntities = new Dictionary<uint, int>();
        private readonly Dictionary<uint, HashSet<ulong>> entityPlayers = new Dictionary<uint, HashSet<ulong>>();
        private readonly Dictionary<uint, Timer> lootDestroyTimer = new Dictionary<uint, Timer>();

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            Unsubscribe(nameof(OnEntityDeath));
            Unsubscribe(nameof(OnPlayerAttack));
        }

        private void OnServerInitialized()
        {
            UpdateConfig();
            if (configData.slapPlayer && Slap == null)
                PrintError("Slap is not loaded, get it at https://umod.org/plugins/slap");
            if (configData.lootContainerS.Any(x => IsBarrel(x.Key) && x.Value))
            {
                Subscribe(nameof(OnEntityDeath));
                Subscribe(nameof(OnPlayerAttack));
            }
        }

        private void Unload()
        {
            foreach (var value in lootDestroyTimer.Values)
                value?.Destroy();
        }

        private void OnLootEntity(BasePlayer player, LootContainer lootContainer)
        {
            if (lootContainer == null || lootContainer.net == null || player == null) return;
            var obj = Trade?.Call("IsTradeBox", lootContainer);
            if (obj is bool && (bool)obj) return;
            bool enabled;
            if (configData.lootContainerS.TryGetValue(lootContainer.ShortPrefabName, out enabled) && !enabled)
            {
                return;
            }

            var entityID = lootContainer.net.ID;
            if (!lootEntities.ContainsKey(entityID))
            {
                lootEntities.Add(entityID, lootContainer.inventory.itemList.Count);
            }

            HashSet<ulong> looters;
            if (entityPlayers.TryGetValue(entityID, out looters)) looters.Add(player.userID);
            else entityPlayers.Add(entityID, new HashSet<ulong> { player.userID });
        }

        private void OnLootEntityEnd(BasePlayer player, LootContainer lootContainer)
        {
            if (lootContainer == null || lootContainer.net == null || player == null) return;
            var entityID = lootContainer.net.ID;
            HashSet<ulong> looters;
            if (!(lootContainer.inventory?.itemList?.Count > 0))
            {
                lootEntities.Remove(entityID);
                if (entityPlayers.TryGetValue(entityID, out looters))
                {
                    looters.Remove(player.userID);
                }
                return;
            }

            int tempItemsCount;
            if (lootEntities.TryGetValue(entityID, out tempItemsCount))
            {
                lootEntities.Remove(entityID);
                if (lootContainer.inventory.itemList.Count < tempItemsCount)
                {
                    if (!lootDestroyTimer.ContainsKey(entityID))
                    {
                        lootDestroyTimer.Add(entityID, timer.Once(configData.timeBeforeLootEmpty, () => DropItems(lootContainer)));
                    }
                }
                else if (entityPlayers.TryGetValue(entityID, out looters))
                {
                    looters.Remove(player.userID);
                }
                EmptyJunkPile(lootContainer);
            }
        }

        private void OnPlayerAttack(BasePlayer attacker, HitInfo info)
        {
            if (attacker == null || !attacker.userID.IsSteamId()) return;
            var barrel = info?.HitEntity as LootContainer;
            if (barrel == null || barrel.net == null) return;
            if (!IsBarrel(barrel.ShortPrefabName)) return;
            bool enabled;
            if (configData.lootContainerS.TryGetValue(barrel.ShortPrefabName, out enabled) && !enabled)
            {
                return;
            }

            var barrelID = barrel.net.ID;
            HashSet<ulong> attackers;
            if (entityPlayers.TryGetValue(barrelID, out attackers)) attackers.Add(attacker.userID);
            else entityPlayers.Add(barrelID, new HashSet<ulong> { attacker.userID });

            if (!lootDestroyTimer.ContainsKey(barrelID))
            {
                lootDestroyTimer.Add(barrelID, timer.Once(configData.timeBeforeLootEmpty, () => DropItems(barrel)));
            }
            EmptyJunkPile(barrel);
        }

        private void OnEntityDeath(LootContainer barrel, HitInfo info)
        {
            if (barrel == null || barrel.net == null) return;
            if (!IsBarrel(barrel.ShortPrefabName)) return;
            var attacker = info?.InitiatorPlayer;
            if (attacker == null || !attacker.userID.IsSteamId()) return;

            HashSet<ulong> attackers;
            if (!entityPlayers.TryGetValue(barrel.net.ID, out attackers)) return;
            attackers.Remove(attacker.userID);
        }

        private void OnEntityKill(LootContainer lootContainer)
        {
            if (lootContainer == null || lootContainer.net == null) return;
            var entityID = lootContainer.net.ID;
            lootEntities.Remove(entityID);

            Timer value;
            if (lootDestroyTimer.TryGetValue(entityID, out value))
            {
                value?.Destroy();
                lootDestroyTimer.Remove(entityID);
            }

            HashSet<ulong> playerIDs;
            if (!entityPlayers.TryGetValue(entityID, out playerIDs))
            {
                return;
            }
            entityPlayers.Remove(entityID);
            if (configData.slapPlayer && Slap != null)
            {
                foreach (var playerID in playerIDs)
                {
                    var player = BasePlayer.FindByID(playerID);
                    if (player == null || player.IPlayer == null) continue;
                    Slap.Call("SlapPlayer", player.IPlayer);
                    Print(player, Lang("SlapMessage", player.UserIDString));
                }
            }
        }

        #endregion Oxide Hooks

        #region Methods

        private static bool IsBarrel(string shortPrefabName) => shortPrefabName.Contains("barrel");

        private void DropItems(LootContainer lootContainer)
        {
            if (lootContainer == null || lootContainer.IsDestroyed) return;
            if (configData.removeItems)
            {
                lootContainer.inventory?.Clear();
            }
            else
            {
                DropUtil.DropItems(lootContainer.inventory, lootContainer.GetDropPosition());
            }
            lootContainer.RemoveMe();
        }

        private void EmptyJunkPile(LootContainer lootContainer)
        {
            if (!configData.emptyJunkpile) return;
            var spawnGroup = lootContainer.GetComponent<SpawnPointInstance>()?.parentSpawnGroup;
            if (spawnGroup == null) return;

            var junkPiles = Facepunch.Pool.GetList<JunkPile>();
            Vis.Entities(lootContainer.transform.position, 10f, junkPiles, Rust.Layers.Mask.Default);
            var junkPile = junkPiles.FirstOrDefault(x => x.spawngroups.Contains(spawnGroup));
            var flag = junkPile == null || junkPile.net == null;
            Facepunch.Pool.FreeList(ref junkPiles);
            if (flag) return;

            if (lootDestroyTimer.ContainsKey(junkPile.net.ID)) return;
            lootDestroyTimer.Add(junkPile.net.ID, timer.Once(configData.timeBeforeJunkpileEmpty, () =>
            {
                if (junkPile != null && !junkPile.IsDestroyed)
                {
                    if (configData.dropNearbyLoot)
                    {
                        var lootContainers = Facepunch.Pool.GetList<LootContainer>();
                        Vis.Entities(junkPile.transform.position, 10f, lootContainers, Rust.Layers.Mask.Default);
                        foreach (var loot in lootContainers)
                        {
                            var lootSpawnGroup = loot.GetComponent<SpawnPointInstance>()?.parentSpawnGroup;
                            if (lootSpawnGroup != null && junkPile.spawngroups.Contains(lootSpawnGroup))
                            {
                                DropItems(loot);
                            }
                        }
                        Facepunch.Pool.FreeList(ref lootContainers);
                    }
                    junkPile.SinkAndDestroy();
                }
            }));
        }

        private void UpdateConfig()
        {
            foreach (var prefab in GameManifest.Current.entities)
            {
                var lootContainer = GameManager.server.FindPrefab(prefab.ToLower())?.GetComponent<LootContainer>();
                if (lootContainer == null || string.IsNullOrEmpty(lootContainer.ShortPrefabName)) continue;
                if (!configData.lootContainerS.ContainsKey(lootContainer.ShortPrefabName))
                    configData.lootContainerS.Add(lootContainer.ShortPrefabName, !lootContainer.ShortPrefabName.Contains("stocking"));
            }
            SaveConfig();
        }

        #endregion Methods

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Time before the loot containers are empties (seconds)")]
            public float timeBeforeLootEmpty = 30f;

            [JsonProperty(PropertyName = "Empty the entire junkpile when automatically empty loot")]
            public bool emptyJunkpile = false;

            [JsonProperty(PropertyName = "Empty the nearby loot when emptying junkpile")]
            public bool dropNearbyLoot = false;

            [JsonProperty(PropertyName = "Time before the junkpile are empties (seconds)")]
            public float timeBeforeJunkpileEmpty = 150f;

            [JsonProperty(PropertyName = "Slaps players who don't empty containers")]
            public bool slapPlayer = false;

            [JsonProperty(PropertyName = "Remove instead bouncing")]
            public bool removeItems = false;

            [JsonProperty(PropertyName = "Chat prefix")]
            public string prefix = "[LootBouncer]:";

            [JsonProperty(PropertyName = "Chat prefix color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Loot container settings")]
            public Dictionary<string, bool> lootContainerS = new Dictionary<string, bool>();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                    LoadDefaultConfig();
            }
            catch
            {
                PrintError("The configuration file is corrupted");
                LoadDefaultConfig();
            }
            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            configData = new ConfigData();
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        #endregion ConfigurationFile

        #region LanguageFile

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["SlapMessage"] = "You didn't empty the container. You got slapped by the container!!!",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["SlapMessage"] = "WDNMD，不清空容器，给你个大耳刮子",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}