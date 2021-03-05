using System;
using System.Collections.Generic;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Sleeper Guard", "Wulf/lukespragg/Arainrr", "1.1.1")]
    [Description("Protects sleeping players from being hurt, killed, or looted")]
    public class SleeperGuard : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Friends, Clans;

        private const string permCanLoot = "sleeperguard.canloot";
        private const string permCanDamage = "sleeperguard.candamage";
        private const string permNPCIgnore = "sleeperguard.npcignore";
        private const string permNoTimeLimit = "sleeperguard.notimelimit";
        private const string permNoLootDelay = "sleeperguard.nolootdelay";
        private const string permNoDamageDelay = "sleeperguard.nodamagedelay";
        private const string permLootProtection = "sleeperguard.lootprotection";
        private const string permDamageProtection = "sleeperguard.damageprotection";

        private readonly Dictionary<ulong, float> noticeTimes = new Dictionary<ulong, float>();

        //Reduce boxing
        private static readonly object True = true, False = false, Null = null;

        #endregion Fields

        #region Oxide Hooks

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

        private object OnNpcTarget(BaseEntity npc, BasePlayer player) => CanIgnoreSleeper(player) ? True : Null;

        private object CanBradleyApcTarget(BradleyAPC apc, BasePlayer player) => CanIgnoreSleeper(player) ? False : Null;

        private object OnEntityTakeDamage(BasePlayer target, HitInfo info)
        {
            if (target == null || !target.userID.IsSteamId()) return Null;
            if (target.IsSleeping())
            {
                BasePlayer attacker = info?.InitiatorPlayer;
                if (attacker != null && permission.UserHasPermission(attacker.UserIDString, permCanDamage)) return Null;
                if (permission.UserHasPermission(target.UserIDString, permDamageProtection))
                {
                    double timeleft;
                    if (CanGuardPlayer(target, false, out timeleft))
                    {
                        if (configData.notifyPlayer && attacker != null && CanNotice(attacker))
                        {
                            Print(attacker, Lang("NoDamage", attacker.UserIDString, timeleft.ToString("0.0")));
                        }
                        NullifyDamage(ref info);
                        return True;
                    }
                }
            }
            return Null;
        }

        private object CanLootPlayer(BasePlayer target, BasePlayer looter)
        {
            if (target == null || looter == null || !target.IsSleeping()) return Null;
            if (permission.UserHasPermission(looter.UserIDString, permCanLoot)) return Null;
            if (permission.UserHasPermission(target.UserIDString, permLootProtection))
            {
                double timeleft;
                if (CanGuardPlayer(target, true, out timeleft) && !AreFriends(target.userID, looter.userID))
                {
                    if (configData.notifyPlayer && CanNotice(looter))
                    {
                        Print(looter, Lang("NoLoot", looter.UserIDString, timeleft.ToString("0.0")));
                    }
                    return False;
                }
            }
            return Null;
        }

        private void OnPlayerSleep(BasePlayer player)
        {
            if (!storedData.sleeperDatas.ContainsKey(player.userID))
            {
                storedData.sleeperDatas.Add(player.userID, new StoredData.TimeData());
            }
        }

        private void OnPlayerSleepEnded(BasePlayer player)
        {
            storedData.sleeperDatas.Remove(player.userID);
        }

        #endregion Oxide Hooks

        #region Methods

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
                if (configData.lootDelay <= 0 || secondsSleeping >= configData.lootDelay || permission.UserHasPermission(sleeper.UserIDString, permNoLootDelay))
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
                if (configData.damageDelay <= 0 || secondsSleeping >= configData.damageDelay || permission.UserHasPermission(sleeper.UserIDString, permNoDamageDelay))
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

        private bool CanIgnoreSleeper(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId() || !player.IsSleeping()) return false;
            if (!permission.UserHasPermission(player.UserIDString, permNPCIgnore)) return false;
            double timeleft;
            return permission.UserHasPermission(player.UserIDString, permDamageProtection) && CanGuardPlayer(player, false, out timeleft);
        }

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

        #region AreFriends

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (!playerID.IsSteamId()) return false;
            if (playerID == friendID) return true;
            if (configData.useTeams && SameTeam(playerID, friendID)) return true;
            if (configData.useFriends && HasFriend(playerID, friendID)) return true;
            if (configData.useClans && SameClan(playerID, friendID)) return true;
            return false;
        }

        private static bool SameTeam(ulong playerID, ulong friendID)
        {
            if (!RelationshipManager.TeamsEnabled()) return false;
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam == null) return false;
            var friendTeam = RelationshipManager.Instance.FindPlayersTeam(friendID);
            if (friendTeam == null) return false;
            return playerTeam == friendTeam;
        }

        private bool HasFriend(ulong playerID, ulong friendID)
        {
            if (Friends == null) return false;
            return (bool)Friends.Call("HasFriend", playerID, friendID);
        }

        private bool SameClan(ulong playerID, ulong friendID)
        {
            if (Clans == null) return false;
            //Clans
            var isMember = Clans.Call("IsClanMember", playerID.ToString(), friendID.ToString());
            if (isMember != null) return (bool)isMember;
            //Rust:IO Clans
            var playerClan = Clans.Call("GetClanOf", playerID);
            if (playerClan == null) return false;
            var friendClan = Clans.Call("GetClanOf", friendID);
            if (friendClan == null) return false;
            return (string)playerClan == (string)friendClan;
        }

        #endregion AreFriends

        #endregion Methods

        #region Helpers

        private static void NullifyDamage(ref HitInfo info)
        {
            info.damageTypes = new DamageTypeList();
            info.HitMaterial = 0;
            info.PointStart = Vector3.zero;
        }

        #endregion Helpers

        #region ConfigurationFile

        private ConfigData configData;

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Use Teams (Used For Loot)")]
            public bool useTeams = false;

            [JsonProperty(PropertyName = "Use Clans (Used For Loot)")]
            public bool useClans = true;

            [JsonProperty(PropertyName = "Use Friends (Used For Loot)")]
            public bool useFriends = true;

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

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat prefix")]
                public string prefix = "<color=#00FFFF>[SleeperGuard]</color>: ";

                [JsonProperty(PropertyName = "Chat steamID icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version;
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
            catch (Exception ex)
            {
                PrintError($"The configuration file is corrupted. \n{ex}");
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
                if (configData.version <= default(VersionNumber))
                {
                    string prefix, prefixColor;
                    if (GetConfigValue(out prefix, "Chat prefix") && GetConfigValue(out prefixColor, "Chat prefix color"))
                    {
                        configData.chatS.prefix = $"<color={prefixColor}>{prefix}</color>: ";
                    }
                }
                configData.version = Version;
            }
        }

        private bool GetConfigValue<T>(out T value, params string[] path)
        {
            var configValue = Config.Get(path);
            if (configValue == null)
            {
                value = default(T);
                return false;
            }
            value = Config.ConvertValue<T>(configValue);
            return true;
        }

        #endregion ConfigurationFile

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public double lastSavedTimestamp;
            public readonly Dictionary<ulong, TimeData> sleeperDatas = new Dictionary<ulong, TimeData>();

            public class TimeData
            {
                public double sleepStartTime;
                public double ignoredTime;
                [JsonIgnore] public double SecondsSleeping => TimeEx.currentTimestamp - sleepStartTime - ignoredTime;

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

        private void Print(BasePlayer player, string message) => Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);

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