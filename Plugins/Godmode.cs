using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Game.Rust;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Godmode", "Wulf/lukespragg/Arainrr", "4.2.9", ResourceId = 673)]
    [Description("Allows players with permission to be invulerable and god-like")]
    internal class Godmode : RustPlugin
    {
        #region Fields

        private const string PermAdmin = "godmode.admin";
        private const string PermInvulerable = "godmode.invulnerable";
        private const string PermLootPlayers = "godmode.lootplayers";
        private const string PermLootProtection = "godmode.lootprotection";
        private const string PermNoAttacking = "godmode.noattacking";
        private const string PermToggle = "godmode.toggle";
        private const string PermUntiring = "godmode.untiring";
        private const string PermAutoEnable = "godmode.autoenable";

        private Dictionary<ulong, float> informHistory;

        #endregion Fields

        #region Oxide Hook

        private void Init()
        {
            LoadData();
            permission.RegisterPermission(PermAdmin, this);
            permission.RegisterPermission(PermInvulerable, this);
            permission.RegisterPermission(PermLootPlayers, this);
            permission.RegisterPermission(PermLootProtection, this);
            permission.RegisterPermission(PermNoAttacking, this);
            permission.RegisterPermission(PermToggle, this);
            permission.RegisterPermission(PermUntiring, this);
            permission.RegisterPermission(PermAutoEnable, this);

            AddCovalenceCommand(configData.godCommand, nameof(GodCommand));
            AddCovalenceCommand(configData.godsCommand, nameof(GodsCommand));
            if (configData.informOnAttack) informHistory = new Dictionary<ulong, float>();
            if (!configData.disconnectDisable) Unsubscribe(nameof(OnPlayerDisconnected));
        }

        private void OnServerInitialized()
        {
            foreach (var god in storedData.godPlayers)
            {
                EnableGodmode(god, true);
            }
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void OnPlayerConnected(BasePlayer player)
        {
            if (IsGod(player))
            {
                PlayerRename(player, true);
            }
            else if (permission.UserHasPermission(player.UserIDString, PermAutoEnable))
            {
                EnableGodmode(player.UserIDString);
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (IsGod(player))
            {
                DisableGodmode(player.UserIDString);
            }
        }

        private void Unload()
        {
            foreach (var god in storedData.godPlayers)
            {
                DisableGodmode(god, true);
            }
            SaveData();
        }

        private object CanBeWounded(BasePlayer player) => IsGod(player) ? false : (object)null;

        private object CanLootPlayer(BasePlayer target, BasePlayer looter)
        {
            if (target == null || looter == null || target == looter) return null;
            if (IsGod(target) && permission.UserHasPermission(target.UserIDString, PermLootProtection) && !permission.UserHasPermission(looter.UserIDString, PermLootPlayers))
            {
                Print(looter, Lang("NoLooting", looter.UserIDString));
                return false;
            }
            return null;
        }

        private object OnEntityTakeDamage(BasePlayer player, HitInfo info)
        {
            if (player == null || !player.userID.IsSteamId()) return null;
            var attacker = info?.InitiatorPlayer;
            if (IsGod(player) && permission.UserHasPermission(player.UserIDString, PermInvulerable))
            {
                InformPlayers(player, attacker);
                NullifyDamage(ref info);
                return true;
            }
            if (IsGod(attacker) && permission.UserHasPermission(attacker.UserIDString, PermNoAttacking))
            {
                InformPlayers(player, attacker);
                NullifyDamage(ref info);
                return true;
            }
            return null;
        }

        private object OnRunPlayerMetabolism(PlayerMetabolism metabolism, BasePlayer player, float delta)
        {
            if (!IsGod(player)) return null;
            metabolism.hydration.value = 250;
            if (!permission.UserHasPermission(player.UserIDString, PermUntiring)) return null;
            var currentCraftLevel = player.currentCraftLevel;
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench1, currentCraftLevel == 1f);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench2, currentCraftLevel == 2f);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.Workbench3, currentCraftLevel == 3f);
            player.SetPlayerFlag(BasePlayer.PlayerFlags.SafeZone, player.InSafeZone());
            return false;
        }

        #endregion Oxide Hook

        #region Methods

        private void CheckHooks()
        {
            if (storedData.godPlayers.Count > 0)
            {
                Subscribe(nameof(CanBeWounded));
                Subscribe(nameof(CanLootPlayer));
                Subscribe(nameof(OnEntityTakeDamage));
                Subscribe(nameof(OnRunPlayerMetabolism));
            }
            else
            {
                Unsubscribe(nameof(CanBeWounded));
                Unsubscribe(nameof(CanLootPlayer));
                Unsubscribe(nameof(OnEntityTakeDamage));
                Unsubscribe(nameof(OnRunPlayerMetabolism));
            }
        }

        private void InformPlayers(BasePlayer victim, BasePlayer attacker)
        {
            if (!configData.informOnAttack || victim == null || attacker == null || victim == attacker) return;
            float victimTime;
            if (!informHistory.TryGetValue(victim.userID, out victimTime))
            {
                informHistory.Add(victim.userID, 0);
            }
            float attackerTime;
            if (!informHistory.TryGetValue(attacker.userID, out attackerTime))
            {
                informHistory.Add(attacker.userID, 0);
            }
            var currentTime = Time.realtimeSinceStartup;
            if (IsGod(victim))
            {
                if (currentTime - victimTime > configData.informInterval)
                {
                    informHistory[victim.userID] = currentTime;
                    Print(attacker, Lang("InformAttacker", attacker.UserIDString, victim.displayName));
                }
                if (currentTime - attackerTime > configData.informInterval)
                {
                    informHistory[attacker.userID] = currentTime;
                    Print(victim, Lang("InformVictim", victim.UserIDString, attacker.displayName));
                }
            }
            else if (IsGod(attacker))
            {
                if (currentTime - victimTime > configData.informInterval)
                {
                    informHistory[victim.userID] = currentTime;
                    Print(attacker, Lang("CantAttack", attacker.UserIDString, victim.displayName));
                }
                if (currentTime - attackerTime > configData.informInterval)
                {
                    informHistory[attacker.userID] = currentTime;
                    Print(victim, Lang("InformVictim", victim.UserIDString, attacker.displayName));
                }
            }
        }

        #region Godmode Toggle

        private object ToggleGodmode(BasePlayer target, BasePlayer player)
        {
            bool isGod = IsGod(target);
            if (Interface.CallHook("OnGodmodeToggle", target.UserIDString, !isGod) != null) return null;
            if (isGod)
            {
                DisableGodmode(target.UserIDString);
                if (player != null)
                {
                    if (target == player) Print(player, Lang("GodmodeDisabled", player.UserIDString));
                    else
                    {
                        Print(player, Lang("GodmodeDisabledFor", player.UserIDString, target.displayName));
                        Print(target, Lang("GodmodeDisabledBy", target.UserIDString, player.displayName));
                    }
                }
                else Print(target, Lang("GodmodeDisabledBy", target.UserIDString, "server console"));
                return false;
            }
            else
            {
                EnableGodmode(target.UserIDString);
                if (player != null)
                {
                    if (target == player) Print(player, Lang("GodmodeEnabled", player.UserIDString));
                    else
                    {
                        Print(player, Lang("GodmodeEnabledFor", player.UserIDString, target.displayName));
                        Print(target, Lang("GodmodeEnabledBy", target.UserIDString, player.displayName));
                    }
                }
                else Print(target, Lang("GodmodeEnabledBy", target.UserIDString, "server console"));
                string targetID = target.UserIDString;
                if (configData.timeLimit > 0) timer.Once(configData.timeLimit, () => DisableGodmode(targetID));
                return true;
            }
        }

        private bool EnableGodmode(string playerID, bool isInit = false)
        {
            if (string.IsNullOrEmpty(playerID) || IsGod(playerID)) return false;
            var player = RustCore.FindPlayerByIdString(playerID);
            if (player == null) return false;
            PlayerRename(player, true);
            ModifyMetabolism(player, true);
            if (!isInit)
            {
                storedData.godPlayers.Add(player.UserIDString);
                CheckHooks();
            }
            Interface.CallHook("OnGodmodeToggled", playerID, true);
            return true;
        }

        private bool DisableGodmode(string playerID, bool isUnload = false)
        {
            if (string.IsNullOrEmpty(playerID) || !IsGod(playerID)) return false;
            var player = RustCore.FindPlayerByIdString(playerID);
            if (player == null) return false;
            PlayerRename(player, false);
            ModifyMetabolism(player, false);
            if (!isUnload)
            {
                storedData.godPlayers.Remove(player.UserIDString);
                CheckHooks();
            }
            Interface.CallHook("OnGodmodeToggled", playerID, false);
            return true;
        }

        private void PlayerRename(BasePlayer player, bool isGod)
        {
            if (player == null || !configData.showNamePrefix || string.IsNullOrEmpty(configData.namePrefix)) return;
            var originalName = GetPayerOriginalName(player.userID);
            if (isGod) Rename(player, configData.namePrefix + originalName);
            else Rename(player, originalName);
        }

        private void Rename(BasePlayer player, string newName)
        {
            if (player == null || string.IsNullOrEmpty(newName.Trim())) return;
            player._name = player.displayName = newName;
            if (player.IPlayer != null) player.IPlayer.Name = newName;
            if (player.net?.connection != null) player.net.connection.username = newName;
            permission.UpdateNickname(player.UserIDString, newName);
            Player.Teleport(player, player.transform.position);
            player.SendNetworkUpdateImmediate();
            //SingletonComponent<ServerMgr>.Instance.persistance.SetPlayerName(player.userID, newName);
        }

        #endregion Godmode Toggle

        #endregion Methods

        #region Helpers

        private static void NullifyDamage(ref HitInfo info)
        {
            info.damageTypes = new DamageTypeList();
            info.HitMaterial = 0;
            info.PointStart = Vector3.zero;
        }

        private static void ModifyMetabolism(BasePlayer player, bool isGod)
        {
            if (player == null || player.metabolism == null) return;
            if (isGod)
            {
                player.health = player.MaxHealth();
                player.metabolism.bleeding.max = 0;
                player.metabolism.bleeding.value = 0;
                player.metabolism.calories.min = 500;
                player.metabolism.calories.value = 500;
                player.metabolism.dirtyness.max = 0;
                player.metabolism.dirtyness.value = 0;
                player.metabolism.heartrate.min = 0.5f;
                player.metabolism.heartrate.max = 0.5f;
                player.metabolism.heartrate.value = 0.5f;
                //player.metabolism.hydration.min = 250;
                player.metabolism.hydration.value = 250;
                player.metabolism.oxygen.min = 1;
                player.metabolism.oxygen.value = 1;
                player.metabolism.poison.max = 0;
                player.metabolism.poison.value = 0;
                player.metabolism.radiation_level.max = 0;
                player.metabolism.radiation_level.value = 0;
                player.metabolism.radiation_poison.max = 0;
                player.metabolism.radiation_poison.value = 0;
                player.metabolism.temperature.min = 32;
                player.metabolism.temperature.max = 32;
                player.metabolism.temperature.value = 32;
                player.metabolism.wetness.max = 0;
                player.metabolism.wetness.value = 0;
            }
            else
            {
                player.metabolism.bleeding.min = 0;
                player.metabolism.bleeding.max = 1;
                player.metabolism.calories.min = 0;
                player.metabolism.calories.max = 500;
                player.metabolism.dirtyness.min = 0;
                player.metabolism.dirtyness.max = 100;
                player.metabolism.heartrate.min = 0;
                player.metabolism.heartrate.max = 1;
                //player.metabolism.hydration.min = 0;
                player.metabolism.hydration.max = 250;
                player.metabolism.oxygen.min = 0;
                player.metabolism.oxygen.max = 1;
                player.metabolism.poison.min = 0;
                player.metabolism.poison.max = 100;
                player.metabolism.radiation_level.min = 0;
                player.metabolism.radiation_level.max = 100;
                player.metabolism.radiation_poison.min = 0;
                player.metabolism.radiation_poison.max = 500;
                player.metabolism.temperature.min = -100;
                player.metabolism.temperature.max = 100;
                player.metabolism.wetness.min = 0;
                player.metabolism.wetness.max = 1;
            }
            player.metabolism.SendChangesToClient();
        }

        private static string GetPayerOriginalName(ulong playerID)
        {
            return SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(playerID);
        }

        #endregion Helpers

        #region API

        private bool EnableGodmode(IPlayer iPlayer) => EnableGodmode(iPlayer.Id);

        private bool EnableGodmode(ulong playerID) => EnableGodmode(playerID.ToString());

        private bool DisableGodmode(IPlayer iPlayer) => DisableGodmode(iPlayer.Id);

        private bool DisableGodmode(ulong playerID) => DisableGodmode(playerID.ToString());

        private bool IsGod(ulong playerID) => IsGod(playerID.ToString());

        private bool IsGod(BasePlayer player) => player != null && IsGod(player.UserIDString);

        private bool IsGod(string playerID) => storedData.godPlayers.Contains(playerID);

        private string[] AllGods(string playerID) => storedData.godPlayers.ToArray();

        #endregion API

        #region Commands

        private void GodCommand(IPlayer iPlayer, string command, string[] args)
        {
            if (args.Length > 0 && !iPlayer.HasPermission(PermAdmin) || !iPlayer.HasPermission(PermToggle))
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id, command));
                return;
            }
            if (args.Length == 0 && iPlayer.Id == "server_console")
            {
                Print(iPlayer, $"The server console cannot use {command}");
                return;
            }
            var target = args.Length > 0 ? RustCore.FindPlayer(args[0]) : iPlayer.Object as BasePlayer;
            if (args.Length > 0 && target == null)
            {
                Print(iPlayer, Lang("PlayerNotFound", iPlayer.Id, args[0]));
                return;
            }
            object obj = ToggleGodmode(target, iPlayer.Object as BasePlayer);
            if (obj is bool && iPlayer.Id == "server_console" && args.Length > 0)
            {
                if ((bool)obj) Print(iPlayer, $"'{target?.displayName}' have enabled godmode");
                else Print(iPlayer, $"'{target?.displayName}' have disabled godmode");
            }
        }

        private void GodsCommand(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.HasPermission(PermAdmin))
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id, command));
                return;
            }
            if (storedData.godPlayers.Count == 0)
            {
                Print(iPlayer, Lang("NoGods", iPlayer.Id));
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine();
            foreach (var god in storedData.godPlayers)
            {
                var player = RustCore.FindPlayerByIdString(god);
                stringBuilder.AppendLine(player == null ? god : player.ToString());
            }
            Print(iPlayer, stringBuilder.ToString());
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Inform On Attack (true/false)")]
            public bool informOnAttack = true;

            [JsonProperty(PropertyName = "Inform Interval (Seconds)")]
            public float informInterval = 15;

            [JsonProperty(PropertyName = "Show Name Prefix (true/false)")]
            public bool showNamePrefix = true;

            [JsonProperty(PropertyName = "Name Prefix (Default [God])")]
            public string namePrefix = "[God] ";

            [JsonProperty(PropertyName = "Time Limit (Seconds, 0 to Disable)")]
            public float timeLimit = 0f;

            [JsonProperty(PropertyName = "Disable godmode after disconnect (true/false)")]
            public bool disconnectDisable = false;

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "[Godmode]:";

            [JsonProperty(PropertyName = "Chat Prefix color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "God commands", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public string[] godCommand = { "god", "godmode" };

            [JsonProperty(PropertyName = "Gods commands", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public string[] godsCommand = { "gods", "godlist" };
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
            public readonly HashSet<string> godPlayers = new HashSet<string>();
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
                    ClearData();
                }
            }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(IPlayer iPlayer, string message)
        {
            iPlayer?.Reply(message,
                iPlayer.Id == "server_console"
                    ? $"{configData.prefix}"
                    : $"<color={configData.prefixColor}>{configData.prefix}</color>");
        }

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, string.IsNullOrEmpty(configData.prefix) ? string.Empty : $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["GodmodeDisabled"] = "You have <color=#FF4500>Disabled</color> godmode",
                ["GodmodeDisabledBy"] = "Your godmode has been <color=#FF4500>Disabled</color> by {0}",
                ["GodmodeDisabledFor"] = "You have <color=#FF4500>Disabled</color> godmode for {0}",
                ["GodmodeEnabled"] = "You have <color=#00FF00>Enabled</color> godmode",
                ["GodmodeEnabledBy"] = "Your godmode has been <color=#00FF00>Enabled</color> by {0}",
                ["GodmodeEnabledFor"] = "You have <color=#00FF00>Enabled</color> godmode for {0}",
                ["InformAttacker"] = "{0} is in godmode and can't take any damage",
                ["InformVictim"] = "{0} just tried to deal damage to you",
                ["CantAttack"] = "You are in godmode and can't attack {0}",
                ["NoGods"] = "No players currently have godmode enabled",
                ["NoLooting"] = "You are not allowed to loot a player with godmode",
                ["NotAllowed"] = "You are not allowed to use the '{0}' command",
                ["PlayerNotFound"] = "Player '{0}' was not found",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["GodmodeDisabled"] = "您的上帝模式 <color=#FF4500>已禁用</color>",
                ["GodmodeDisabledBy"] = "{0} <color=#FF4500>禁用了</color> 您的上帝模式",
                ["GodmodeDisabledFor"] = "您 <color=#FF4500>禁用了</color> {0} 的上帝模式",
                ["GodmodeEnabled"] = "您的上帝模式 <color=#00FF00>已启用</color>",
                ["GodmodeEnabledBy"] = "{0} <color=#00FF00>启用了</color> 您的上帝模式",
                ["GodmodeEnabledFor"] = "您 <color=#00FF00>启用了</color> {0} 的上帝模式",
                ["InformAttacker"] = "{0} 处于上帝模式，您不能伤害他",
                ["InformVictim"] = "{0} 想伤害您",
                ["CantAttack"] = "您处于上帝模式，不能伤害 {0}",
                ["NoGods"] = "当前没有玩家启用上帝模式",
                ["NoLooting"] = "您不能掠夺处于上帝模式的玩家",
                ["NotAllowed"] = "您没有权限使用 '{0}' 命令",
                ["PlayerNotFound"] = "玩家 '{0}' 未找到",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}