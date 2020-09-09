using System.Collections.Generic;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Sleeper Guard", "Wulf/lukespragg/Arainrr", "1.1.0")]
    [Description("Protects sleeping players from being hurt, killed, or looted")]
    public class SleeperGuard : RustPlugin
    {
        private const string permCanLoot = "sleeperguard.canloot";
        private const string permCanDamage = "sleeperguard.candamage";
        private const string permNPCIgnore = "sleeperguard.npcignore";
        private const string permNoTimeLimit = "sleeperguard.notimelimit";
        private const string permNoLootDelay = "sleeperguard.nolootdelay";
        private const string permNoDamageDelay = "sleeperguard.nodamagedelay";
        private const string permLootProtection = "sleeperguard.lootprotection";
        private const string permDamageProtection = "sleeperguard.damageprotection";

        private readonly Dictionary<ulong, float> noticeTimes = new Dictionary<ulong, float>();

        private void Init()
        {
            LoadData();
            permission.RegisterPermission(permCanLoot, this);
            permission.RegisterPermission(permCanDamage, this);
            permission.RegisterPermission(permNPCIgnore, this);
            permission.RegisterPermission(permNoTimeLimit, this);
            permission.RegisterPermission(permNoLootDelay, this);
            permission.RegisterPermission(permNoDamageDelay, this);
            permission.RegisterPermission(permLootProtection, this);
            permission.RegisterPermission(permDamageProtection, this);
            if (!configData.ignoreSleeper)
            {
                Unsubscribe(nameof(OnNpcTarget));
                Unsubscribe(nameof(CanBradleyApcTarget));
            }
        }

        private void OnServerInitialized()
        {
            if (storedData.lastSavedTimestamp > 0)
            {
                double unloadedPluginTime = TimeEx.currentTimestamp - storedData.lastSavedTimestamp;
                foreach (var entry in storedData.sleeperDatas)
                    entry.Value.ignoredTime += unloadedPluginTime;
            }
            foreach (var sleeper in BasePlayer.sleepingPlayerList)
                OnPlayerSleep(sleeper);
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void Unload() => SaveData();

        private bool CanGuardPlayer(BasePlayer sleeper, bool isLoot, out double timeleft)
        {
            StoredData.TimeData timeData;
            if (!storedData.sleeperDatas.TryGetValue(sleeper.userID, out timeData))
            {
                timeleft = 0;
                return false;
            }
            if (permission.UserHasPermission(sleeper.UserIDString, permNoTimeLimit))
            {
                timeleft = -1;
                return true;
            }
            double secondsSleeping = timeData.SecondsSleeping;
            if (isLoot)
            {
                if (configData.lootDelay <= 0 ? true : (permission.UserHasPermission(sleeper.UserIDString, permNoLootDelay) || secondsSleeping >= configData.lootDelay))
                {
                    if (configData.lootGuardTime <= 0)
                    {
                        timeleft = -1;
                        return true;
                    }
                    timeleft = configData.lootGuardTime - secondsSleeping;
                    if (timeleft > 0)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (configData.damageDelay <= 0 ? true : (permission.UserHasPermission(sleeper.UserIDString, permNoDamageDelay) || secondsSleeping >= configData.damageDelay))
                {
                    if (configData.damageGuardTime <= 0)
                    {
                        timeleft = -1;
                        return true;
                    }
                    timeleft = configData.damageGuardTime - secondsSleeping;
                    if (timeleft > 0)
                    {
                        return true;
                    }
                }
            }
            timeleft = 0;
            return false;
        }

        #region IgnoreSleeper

        private object OnNpcTarget(BaseEntity npc, BasePlayer player) => CanIgnoreSleeper(player) ? true : (object)null;

        private object CanBradleyApcTarget(BradleyAPC apc, BasePlayer player) => CanIgnoreSleeper(player) ? false : (object)null;

        private bool CanIgnoreSleeper(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId() || !player.IsSleeping()) return false;
            if (!permission.UserHasPermission(player.UserIDString, permNPCIgnore)) return false;
            double timeleft;
            if (permission.UserHasPermission(player.UserIDString, permDamageProtection) && CanGuardPlayer(player, false, out timeleft)) return true;
            return false;
        }

        #endregion IgnoreSleeper

        #region DamageProtection

        private object OnEntityTakeDamage(BasePlayer target, HitInfo info)
        {
            if (target == null || !target.userID.IsSteamId()) return null;
            if (target.IsSleeping())
            {
                BasePlayer attacker = info?.InitiatorPlayer;
                if (attacker != null && permission.UserHasPermission(attacker.UserIDString, permCanDamage)) return null;
                if (permission.UserHasPermission(target.UserIDString, permDamageProtection))
                {
                    double timeleft;
                    if (CanGuardPlayer(target, false, out timeleft))
                    {
                        if (configData.notifyPlayer && attacker != null && CanNotice(attacker))
                            Print(attacker, Lang("NoDamage", attacker.UserIDString, timeleft.ToString("0.0")));
                        NullifyDamage(ref info);
                        return true;
                    }
                }
            }
            return null;
        }

        private static void NullifyDamage(ref HitInfo info)
        {
            info.damageTypes = new DamageTypeList();
            info.HitMaterial = 0;
            info.PointStart = Vector3.zero;
        }

        #endregion DamageProtection

        #region LootProtection

        private object CanLootPlayer(BasePlayer target, BasePlayer looter)
        {
            if (target == null || looter == null) return null;
            if (target.IsSleeping())
            {
                if (permission.UserHasPermission(looter.UserIDString, permCanLoot)) return null;
                if (permission.UserHasPermission(target.UserIDString, permLootProtection))
                {
                    double timeleft;
                    if (CanGuardPlayer(target, true, out timeleft))
                    {
                        if (configData.notifyPlayer && CanNotice(looter))
                            Print(looter, Lang("NoLoot", looter.UserIDString, timeleft.ToString("0.0")));
                        return false;
                    }
                }
            }
            return null;
        }

        #endregion LootProtection

        #region SleeperHandling

        private void OnPlayerSleep(BasePlayer player)
        {
            if (!storedData.sleeperDatas.ContainsKey(player.userID))
                storedData.sleeperDatas.Add(player.userID, new StoredData.TimeData());
        }

        private void OnPlayerSleepEnded(BasePlayer player) => storedData.sleeperDatas.Remove(player.userID);

        private bool CanNotice(BasePlayer player)
        {
            float lastTime;
            if (!noticeTimes.TryGetValue(player.userID, out lastTime))
            {
                noticeTimes.Add(player.userID, Time.realtimeSinceStartup);
                return true;
            }
            if (Time.realtimeSinceStartup - lastTime >= configData.notifyInterval)
            {
                noticeTimes[player.userID] = Time.realtimeSinceStartup;
                return true;
            }
            return false;
        }

        #endregion SleeperHandling

        #region ConfigurationFile

        private ConfigData configData;

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Damage delay (seconds) (0 to disable)")]
            public double damageDelay = 0;

            [JsonProperty(PropertyName = "Damage guard times (seconds) (0 to unlimit)")]
            public double damageGuardTime = 0;

            [JsonProperty(PropertyName = "Loot delay (seconds) (0 to disable)")]
            public double lootDelay = 0;

            [JsonProperty(PropertyName = "Loot guard times (seconds) (0 to unlimit)")]
            public double lootGuardTime = 0;

            [JsonProperty(PropertyName = "Notify player (true/false)")]
            public bool notifyPlayer = true;

            [JsonProperty(PropertyName = "Notification interval (seconds)")]
            public float notifyInterval = 5;

            [JsonProperty(PropertyName = "NPC ignores sleepers (Enabling it can cause server lag)")]
            public bool ignoreSleeper = false;

            [JsonProperty(PropertyName = "Chat prefix")]
            public string prefix = "[SleeperGuard]:";

            [JsonProperty(PropertyName = "Chat prefix color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;
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
            public double lastSavedTimestamp;
            public Dictionary<ulong, TimeData> sleeperDatas = new Dictionary<ulong, TimeData>();

            public class TimeData
            {
                [JsonIgnore] public double SecondsSleeping => TimeEx.currentTimestamp - sleepStartTime - ignoredTime;
                public double sleepStartTime;
                public double ignoredTime;

                public TimeData()
                {
                    sleepStartTime = TimeEx.currentTimestamp;
                }
            }
        }

        private void LoadData()
        {
            try
            {
                storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
            }
            catch
            {
                ClearData();
            }
        }

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        private void SaveData()
        {
            storedData.lastSavedTimestamp = TimeEx.currentTimestamp;
            Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);
        }

        private void OnNewSave(string filename) => ClearData();

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoDamage"] = "You can't hurt this sleeping player. Protection time left: {0} seconds.",
                ["NoLoot"] = "You can't loot this sleeping player. Protection time left: {0} seconds.",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoDamage"] = "您不能伤害这个睡眠玩家，剩余保护时间: {0} 秒",
                ["NoLoot"] = "您不能掠夺这个睡眠玩家，剩余保护时间: {0} 秒",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}