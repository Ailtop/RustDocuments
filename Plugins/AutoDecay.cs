using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Decay", "Hougan/Arainrr", "1.2.6")]
    [Description("Auto damage to objects, that are not in building zone")]
    public class AutoDecay : RustPlugin
    {
        private static AutoDecay instance;
        private const string PERMISSION_IGNORE = "autodecay.ignore";
        private readonly Hash<uint, DestroyControl> destroyControlEntities = new Hash<uint, DestroyControl>();
        private readonly Hash<ulong, float> notifyPlayer = new Hash<ulong, float>();

        private void Init()
        {
            LoadData();
            instance = this;
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_IGNORE, this);
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            if (configData.decayEntitySettings.Count <= 0) CreateConfig();
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                var entity = baseNetworkable as BaseCombatEntity;
                if (entity != null)
                {
                    ApplyDestroyControl(entity);
                }
            }

            foreach (var entry in configData.decayEntitySettings)
            {
                if (!storedData.entityShortPrefabNames.Contains(entry.Key))
                {
                    PrintError($"\"{entry.Key}\" is an invalid combat entity short prefab name, Please get them in the data file");
                }
            }
        }

        private readonly List<string> defaultDisabled = new List<string>
        {
            "small_stash_deployed",
            "sleepingbag_leather_deployed",
        };

        private void CreateConfig()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                var itemModDeployable = itemDefinition.GetComponent<ItemModDeployable>();
                if (itemModDeployable == null) continue;
                var baseCombatEntity = GameManager.server.FindPrefab(itemModDeployable.entityPrefab.resourcePath)?.GetComponent<BaseCombatEntity>();
                if (baseCombatEntity == null || string.IsNullOrEmpty(baseCombatEntity.ShortPrefabName)) continue;
                if (configData.decayEntitySettings.ContainsKey(baseCombatEntity.ShortPrefabName)) continue;

                configData.decayEntitySettings.Add(baseCombatEntity.ShortPrefabName, new DecayEntityS
                {
                    enabled = !(itemDefinition.category == ItemCategory.Food || defaultDisabled.Contains(baseCombatEntity.ShortPrefabName)),
                    checkOwner = true,
                    delayTime = 600f,
                    destroyTime = 3600f,
                    tickRate = 10f,
                });
            }
            UpdateData(true);
        }

        private void OnNewSave(string filename) => UpdateData();

        private void UpdateData(bool saveConfig = false)
        {
            storedData.entityShortPrefabNames.Clear();
            foreach (var prefab in GameManifest.Current.entities)
            {
                var baseCombatEntity = GameManager.server.FindPrefab(prefab.ToLower())?.GetComponent<BaseCombatEntity>();
                if (baseCombatEntity == null || string.IsNullOrEmpty(baseCombatEntity.ShortPrefabName)) continue;
                storedData.entityShortPrefabNames.Add(baseCombatEntity.ShortPrefabName);
                if (saveConfig)
                {
                    if (baseCombatEntity is BuildingBlock || baseCombatEntity is BaseVehicle)
                    {
                        if (configData.decayEntitySettings.ContainsKey(baseCombatEntity.ShortPrefabName)) continue;
                        configData.decayEntitySettings.Add(baseCombatEntity.ShortPrefabName, new DecayEntityS
                        {
                            enabled = false,
                            checkOwner = true,
                            delayTime = 600f,
                            destroyTime = 3600f,
                            tickRate = 10f,
                        });
                    }
                }
            }
            if (saveConfig)
            {
                configData.decayEntitySettings = configData.decayEntitySettings.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
                SaveConfig();
            }
            SaveData();
        }

        private void Unload()
        {
            foreach (var destroyControl in destroyControlEntities.Values)
                destroyControl?.Destroy();
            instance = null;
        }

        private void OnEntitySpawned(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            var player = baseCombatEntity.OwnerID != 0 ? BasePlayer.FindByID(baseCombatEntity.OwnerID) : null;
            ApplyDestroyControl(baseCombatEntity, player);
        }

        private void OnEntityDeath(BaseCombatEntity baseCombatEntity, HitInfo info) => OnEntityKill(baseCombatEntity);

        private void OnEntityKill(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            DestroyControl destroyControl;
            if (destroyControlEntities.TryGetValue(baseCombatEntity.net.ID, out destroyControl))
            {
                destroyControl.Destroy();
                destroyControlEntities.Remove(baseCombatEntity.net.ID);
            }
        }

        private void ApplyDestroyControl(BaseCombatEntity baseCombatEntity, BasePlayer player = null)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            if (baseCombatEntity.OwnerID.IsSteamId() && permission.UserHasPermission(baseCombatEntity.OwnerID.ToString(), PERMISSION_IGNORE)) return;
            DecayEntityS decayEntityS;
            if (configData.decayEntitySettings.TryGetValue(baseCombatEntity.ShortPrefabName, out decayEntityS))
            {
                if (!decayEntityS.enabled) return;
                if (decayEntityS.checkOwner && !baseCombatEntity.OwnerID.IsSteamId()) return;
                if (!destroyControlEntities.ContainsKey(baseCombatEntity.net.ID))
                {
                    destroyControlEntities.Add(baseCombatEntity.net.ID, new DestroyControl(baseCombatEntity, decayEntityS.destroyTime, decayEntityS.delayTime, decayEntityS.tickRate));
                    if (configData.notifyPlayer && player != null && baseCombatEntity.GetBuildingPrivilege() == null)
                        SendMessage(player, decayEntityS.delayTime + decayEntityS.destroyTime);
                }
            }
        }

        private void SendMessage(BasePlayer player, float time)
        {
            float value;
            if (notifyPlayer.TryGetValue(player.userID, out value) && Time.realtimeSinceStartup - value <= configData.notifyInterval) return;
            notifyPlayer[player.userID] = Time.realtimeSinceStartup;
            Print(player, Lang("DESTROY", player.UserIDString, TimeSpan.FromSeconds(time).ToShortString()));
        }

        private class DestroyControl
        {
            private readonly BaseCombatEntity baseCombatEntity;
            private readonly float destroyTime;
            private readonly float delayTime;
            private readonly float tickRate;
            private readonly bool isCupboard;

            private float tickDamage;
            private bool startedDamage;
            private bool delayDamage;

            public DestroyControl(BaseCombatEntity baseCombatEntity, float destroyTime, float delayTime, float tickRate)
            {
                this.baseCombatEntity = baseCombatEntity;
                this.destroyTime = destroyTime;
                this.delayTime = delayTime;
                this.tickRate = tickRate;
                if (baseCombatEntity is BuildingPrivlidge) isCupboard = true;
                baseCombatEntity.InvokeRepeating(CheckBuildingPrivilege, UnityEngine.Random.Range(0f, 60f), instance.configData.checkTime);
            }

            private void CheckBuildingPrivilege()
            {
                if (baseCombatEntity == null || baseCombatEntity.IsDestroyed)
                {
                    Destroy();
                    return;
                }
                if (isCupboard ? OnFoundation() : baseCombatEntity.GetBuildingPrivilege() != null)
                {
                    if (startedDamage) StopDamage();
                    return;
                }
                if (!startedDamage && !delayDamage)
                {
                    delayDamage = true;
                    baseCombatEntity.Invoke(StartDamage, delayTime);
                }
            }

            private bool OnFoundation()
            {
                RaycastHit raycastHit;
                return Physics.Raycast(baseCombatEntity.transform.position + new Vector3(0f, 0.1f, 0f), Vector3.down, out raycastHit, 0.11f, Rust.Layers.Mask.Construction) && raycastHit.GetEntity() is BuildingBlock;
            }

            private void StartDamage()
            {
                if (baseCombatEntity == null || baseCombatEntity.IsDestroyed)
                {
                    Destroy();
                    return;
                }
                delayDamage = false;
                startedDamage = true;
                baseCombatEntity.InvokeRepeating(DoDamage, UnityEngine.Random.Range(0f, destroyTime / tickRate), destroyTime / tickRate);
            }

            private void StopDamage()
            {
                startedDamage = false;
                baseCombatEntity.CancelInvoke(DoDamage);
            }

            private void DoDamage()
            {
                if (baseCombatEntity == null || baseCombatEntity.IsDestroyed)
                {
                    Destroy();
                    return;
                }
                var currentTickDamage = baseCombatEntity.MaxHealth() / tickRate;
                if (tickDamage != currentTickDamage) tickDamage = currentTickDamage;
                baseCombatEntity.Hurt(tickDamage, Rust.DamageType.Decay);
            }

            public void Destroy()
            {
                baseCombatEntity?.CancelInvoke(DoDamage);
                baseCombatEntity?.CancelInvoke(CheckBuildingPrivilege);
                baseCombatEntity?.CancelInvoke(StartDamage);
            }
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Check cupboard time (seconds)")]
            public float checkTime = 600f;

            [JsonProperty(PropertyName = "Notify player, that his object will be removed")]
            public bool notifyPlayer = true;

            [JsonProperty(PropertyName = "Notify player interval")]
            public float notifyInterval = 10f;

            [JsonProperty(PropertyName = "Chat prefix")]
            public string prefix = "[AutoDecay]:";

            [JsonProperty(PropertyName = "Chat prefix color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Decay entity list")]
            public Dictionary<string, DecayEntityS> decayEntitySettings = new Dictionary<string, DecayEntityS>();
        }

        private class DecayEntityS
        {
            [JsonProperty("Enabled destroy")]
            public bool enabled;

            [JsonProperty("Check if it is a player's entity")]
            public bool checkOwner;

            [JsonProperty("Delay destroy time (seconds)")]
            public float delayTime;

            [JsonProperty("Tick rate (Damage per tick = max health / this)")]
            public float tickRate;

            [JsonProperty("Destroy time (seconds)")]
            public float destroyTime;
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

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            [JsonProperty("List of short prefab names for all combat entities")]
            public HashSet<string> entityShortPrefabNames = new HashSet<string>();
        }

        private void LoadData()
        {
            try
            {
                storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
            }
            catch
            {
                storedData = null;
            }
            finally
            {
                if (storedData == null)
                {
                    storedData = new StoredData();
                    UpdateData();
                }
            }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, string.IsNullOrEmpty(configData.prefix) ? string.Empty : $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["DESTROY"] = "If you do not install the cupboard, the object will <color=#F4D142>be deleted</color> after {0}."
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["DESTROY"] = "如果您一直不放置领地柜，该实体将在 {0} 后<color=#F4D142>被删除</color>"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}