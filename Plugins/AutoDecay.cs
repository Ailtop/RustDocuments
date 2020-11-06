using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Decay", "Hougan/Arainrr", "1.2.8")]
    [Description("Auto damage to objects, that are not in building zone")]
    public class AutoDecay : RustPlugin
    {
        #region Fields

        private static AutoDecay instance;
        private const string PERMISSION_IGNORE = "autodecay.ignore";
        private readonly Hash<uint, DestroyControl> destroyControlEntities = new Hash<uint, DestroyControl>();
        private readonly Hash<ulong, float> notifyPlayer = new Hash<ulong, float>();

        private readonly List<string> defaultDisabled = new List<string>
        {
            "small_stash_deployed",
            "sleepingbag_leather_deployed",
        };

        #endregion Fields

        #region Oxide Hooks

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
            UpdateConfig(configData.decayEntitySettings.Count <= 0);
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                var baseCombatEntity = baseNetworkable as BaseCombatEntity;
                if (baseCombatEntity != null)
                {
                    ApplyDestroyControl(baseCombatEntity);
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

        private void OnNewSave(string filename) => UpdateData();

        private void Unload()
        {
            foreach (var destroyControl in destroyControlEntities.Values)
                destroyControl?.Destroy();
            instance = null;
        }

        private void OnEntitySpawned(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            var buildingPrivlidge = baseCombatEntity as BuildingPrivlidge;
            if (buildingPrivlidge != null) HandleCupboard(buildingPrivlidge, true);
            var player = baseCombatEntity.OwnerID.IsSteamId() ? BasePlayer.FindByID(baseCombatEntity.OwnerID) : null;
            ApplyDestroyControl(baseCombatEntity, player, false);
        }

        private void OnEntityDeath(BaseCombatEntity baseCombatEntity, HitInfo info) => OnEntityKill(baseCombatEntity);

        private void OnEntityKill(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            var buildingPrivlidge = baseCombatEntity as BuildingPrivlidge;
            if (buildingPrivlidge != null) HandleCupboard(buildingPrivlidge, false);
            DestroyControl destroyControl;
            if (destroyControlEntities.TryGetValue(baseCombatEntity.net.ID, out destroyControl))
            {
                destroyControl?.Destroy();
                destroyControlEntities.Remove(baseCombatEntity.net.ID);
            }
        }

        #endregion Oxide Hooks

        #region Methods

        private void HandleCupboard(BuildingPrivlidge buildingPrivlidge, bool spawned)
        {
            var decayEntities = buildingPrivlidge?.GetBuilding()?.decayEntities;
            if (decayEntities != null)
            {
                DestroyControl destroyControl;
                foreach (var decayEntity in decayEntities)
                {
                    if (decayEntity == null || decayEntity.net == null) continue;
                    if (destroyControlEntities.TryGetValue(decayEntity.net.ID, out destroyControl))
                    {
                        if (spawned) destroyControl?.OnCupboardPlaced();
                        else destroyControl?.OnCupboardDestroyed();
                    }
                }
            }
        }

        private void UpdateConfig(bool crate)
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                var prefabName = itemDefinition.GetComponent<ItemModDeployable>()?.entityPrefab?.resourcePath;
                if (string.IsNullOrEmpty(prefabName)) continue;
                var baseCombatEntity = GameManager.server.FindPrefab(prefabName)?.GetComponent<BaseCombatEntity>();
                if (baseCombatEntity == null || string.IsNullOrEmpty(baseCombatEntity.ShortPrefabName)) continue;
                if (configData.decayEntitySettings.ContainsKey(baseCombatEntity.ShortPrefabName)) continue;

                configData.decayEntitySettings.Add(baseCombatEntity.ShortPrefabName, new DecayEntityS
                {
                    enabled = crate && !(itemDefinition.category == ItemCategory.Food || defaultDisabled.Contains(baseCombatEntity.ShortPrefabName)),
                    checkOwner = true,
                    delayTime = 600f,
                    destroyTime = 3600f,
                    tickRate = 10f,
                });
            }
            if (crate) UpdateData(true);
            else SaveConfig();
        }

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
            if (saveConfig) SaveConfig();
            SaveData();
        }

        private void ApplyDestroyControl(BaseCombatEntity baseCombatEntity, BasePlayer player = null, bool init = true)
        {
            if (baseCombatEntity == null || baseCombatEntity.net == null) return;
            if (baseCombatEntity.OwnerID.IsSteamId() && permission.UserHasPermission(baseCombatEntity.OwnerID.ToString(), PERMISSION_IGNORE)) return;
            DecayEntityS decayEntityS;
            if (configData.decayEntitySettings.TryGetValue(baseCombatEntity.ShortPrefabName, out decayEntityS) && decayEntityS.enabled)
            {
                if (decayEntityS.checkOwner && !baseCombatEntity.OwnerID.IsSteamId()) return;
                if (!destroyControlEntities.ContainsKey(baseCombatEntity.net.ID))
                {
                    destroyControlEntities.Add(baseCombatEntity.net.ID, new DestroyControl(baseCombatEntity, decayEntityS.destroyTime, decayEntityS.delayTime, decayEntityS.tickRate, init));
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

        #endregion Methods

        #region DestroyControl

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

            public DestroyControl(BaseCombatEntity baseCombatEntity, float destroyTime, float delayTime, float tickRate, bool init)
            {
                this.baseCombatEntity = baseCombatEntity;
                this.destroyTime = destroyTime;
                this.delayTime = delayTime;
                this.tickRate = tickRate;
                isCupboard = baseCombatEntity is BuildingPrivlidge;
                baseCombatEntity.InvokeRepeating(CheckBuildingPrivilege, init ? UnityEngine.Random.Range(0f, 60f) : 1f, instance.configData.checkTime);
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
                    OnCupboardPlaced();
                    return;
                }
                OnCupboardDestroyed();
            }

            private bool OnFoundation()
            {
                RaycastHit raycastHit;
                return Physics.Raycast(baseCombatEntity.transform.position + Vector3.up * 0.1f, Vector3.down, out raycastHit, 0.11f, Rust.Layers.Mask.Construction) && raycastHit.GetEntity() is BuildingBlock;
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
                baseCombatEntity.InvokeRepeating(DoDamage, 0f, destroyTime / tickRate);
            }

            private void StopDamage()
            {
                if (delayDamage)
                {
                    delayDamage = false;
                    baseCombatEntity.CancelInvoke(StartDamage);
                }
                if (startedDamage)
                {
                    startedDamage = false;
                    baseCombatEntity.CancelInvoke(DoDamage);
                }
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

            public void OnCupboardPlaced()
            {
                StopDamage();
            }

            public void OnCupboardDestroyed()
            {
                if (!startedDamage && !delayDamage)
                {
                    delayDamage = true;
                    baseCombatEntity.Invoke(StartDamage, delayTime);
                }
            }

            public void Destroy()
            {
                baseCombatEntity?.CancelInvoke(DoDamage);
                baseCombatEntity?.CancelInvoke(StartDamage);
                baseCombatEntity?.CancelInvoke(CheckBuildingPrivilege);
            }
        }

        #endregion DestroyControl

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Check cupboard time (seconds)")]
            public float checkTime = 300f;

            [JsonProperty(PropertyName = "Notify player, that his object will be removed")]
            public bool notifyPlayer = true;

            [JsonProperty(PropertyName = "Notify player interval")]
            public float notifyInterval = 10f;

            [JsonProperty(PropertyName = "Chat prefix")]
            public string prefix = "<color=#00FFFF>[AutoDecay]</color>: ";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Decay entity list")]
            public Dictionary<string, DecayEntityS> decayEntitySettings = new Dictionary<string, DecayEntityS>();

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version = new VersionNumber(1, 2, 6);
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
                {
                    LoadDefaultConfig();
                }
                else
                {
                    UpdateConfigValues();
                }
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
            configData.version = Version;
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= new VersionNumber(1, 2, 7))
                {
                    if (configData.prefix == "[AutoDecay]:")
                    {
                        configData.prefix = "<color=#00FFFF>[AutoDecay]</color>: ";
                    }
                }
                configData.version = Version;
            }
        }

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
            Player.Message(player, message, configData.prefix, configData.steamIDIcon);
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