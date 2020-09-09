using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Don't Target Me", "Quantum/Arainrr", "1.1.1")]
    [Description("Makes turrets, player npcs and normal npcs ignore you.")]
    internal class DontTargetMe : RustPlugin
    {
        private const string PERMISSION_ALL = "donttargetme.all";
        private const string PERMISSION_NPC = "donttargetme.npc";
        private const string PERMISSION_APC = "donttargetme.apc";
        private const string PERMISSION_SAM = "donttargetme.sam";
        private const string PERMISSION_HELI = "donttargetme.heli";
        private const string PERMISSION_TURRETS = "donttargetme.turrets";
        private readonly Dictionary<ulong, TargetFlags> playerFlags = new Dictionary<ulong, TargetFlags>();

        #region Hooks

        private void Init()
        {
            permission.RegisterPermission(PERMISSION_ALL, this);
            permission.RegisterPermission(PERMISSION_NPC, this);
            permission.RegisterPermission(PERMISSION_APC, this);
            permission.RegisterPermission(PERMISSION_SAM, this);
            permission.RegisterPermission(PERMISSION_HELI, this);
            permission.RegisterPermission(PERMISSION_TURRETS, this);

            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdToggle));
        }

        private void OnServerInitialized()
        {
            if (!configData.disableWhenDis) Unsubscribe(nameof(OnPlayerDisconnected));
            if (!configData.enableWhenCon) Unsubscribe(nameof(OnPlayerConnected));
            else
            {
                foreach (var player in BasePlayer.activePlayerList)
                    PlayerFlagsInit(player);
            }
            CheckHooks();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            if (PlayerFlagsInit(player))
                CheckHooks();
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null) return;
            if (playerFlags.ContainsKey(player.userID))
            {
                playerFlags.Remove(player.userID);
                CheckHooks();
            }
        }

        private object CanBeTargeted(BasePlayer player, MonoBehaviour behaviour) => player != null && HasTargetFlags(player, TargetFlags.Turret) ? false : (object)null;

        private object OnNpcTarget(BaseEntity npc, BasePlayer player) => player != null && HasTargetFlags(player, TargetFlags.Npc) ? true : (object)null;

        private object CanBradleyApcTarget(BradleyAPC apc, BasePlayer player) => player != null && HasTargetFlags(player, TargetFlags.Bradley) ? false : (object)null;

        private object CanHelicopterTarget(PatrolHelicopterAI heli, BasePlayer player) => player != null && HasTargetFlags(player, TargetFlags.Helicopter) ? (object)false : null;

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI heli, BasePlayer player) => player != null && HasTargetFlags(player, TargetFlags.Helicopter) ? (object)false : null;

        private object OnSamSiteTarget(SamSite samSite, BaseVehicle vehicle)
        {
            var driver = vehicle?.GetDriver();
            if (driver != null && HasTargetFlags(driver, TargetFlags.Sam)) return false;
            return null;
        }

        #endregion Hooks

        #region Helper

        [Flags]
        private enum TargetFlags
        {
            None = 0,
            Npc = 1,
            Sam = 1 << 1,
            Turret = 1 << 2,
            Bradley = 1 << 3,
            Helicopter = 1 << 4,
        }

        private bool HasTargetFlags(BasePlayer player, TargetFlags flag)
        {
            TargetFlags flags;
            if (playerFlags.TryGetValue(player.userID, out flags))
                return flags.HasFlag(flag);
            return false;
        }

        private bool PlayerFlagsInit(BasePlayer player)
        {
            if (playerFlags.ContainsKey(player.userID)) playerFlags.Remove(player.userID);
            TargetFlags flags = TargetFlags.None;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_ALL))
            {
                foreach (TargetFlags flag in Enum.GetValues(typeof(TargetFlags)))
                    flags |= flag;
            }
            else
            {
                if (permission.UserHasPermission(player.UserIDString, PERMISSION_TURRETS))
                    flags |= TargetFlags.Turret;
                if (permission.UserHasPermission(player.UserIDString, PERMISSION_APC))
                    flags |= TargetFlags.Bradley;
                if (permission.UserHasPermission(player.UserIDString, PERMISSION_NPC))
                    flags |= TargetFlags.Npc;
                if (permission.UserHasPermission(player.UserIDString, PERMISSION_SAM))
                    flags |= TargetFlags.Sam;
                if (permission.UserHasPermission(player.UserIDString, PERMISSION_HELI))
                    flags |= TargetFlags.Helicopter;
            }
            if (flags != TargetFlags.None)
            {
                playerFlags.Add(player.userID, flags);
                return true;
            }
            return false;
        }

        private void CheckHooks()
        {
            if (playerFlags.Count <= 0)
            {
                Unsubscribe(nameof(OnNpcTarget));
                Unsubscribe(nameof(CanBeTargeted));
                Unsubscribe(nameof(OnSamSiteTarget));
                Unsubscribe(nameof(CanBradleyApcTarget));
                Unsubscribe(nameof(CanHelicopterTarget));
                Unsubscribe(nameof(CanHelicopterStrafeTarget));
                return;
            }
            bool turret = false, npc = false, apc = false, heli = false, sam = false;
            foreach (var flags in playerFlags.Values)
            {
                if (flags.HasFlag(TargetFlags.Turret)) turret = true;
                if (flags.HasFlag(TargetFlags.Npc)) npc = true;
                if (flags.HasFlag(TargetFlags.Bradley)) apc = true;
                if (flags.HasFlag(TargetFlags.Helicopter)) heli = true;
                if (flags.HasFlag(TargetFlags.Sam)) sam = true;
            }

            if (!turret) Unsubscribe(nameof(CanBeTargeted));
            else Subscribe(nameof(CanBeTargeted));

            if (!npc) Unsubscribe(nameof(OnNpcTarget));
            else Subscribe(nameof(OnNpcTarget));

            if (!apc) Unsubscribe(nameof(CanBradleyApcTarget));
            else Subscribe(nameof(CanBradleyApcTarget));

            if (!sam) Unsubscribe(nameof(OnSamSiteTarget));
            else Subscribe(nameof(OnSamSiteTarget));

            if (!heli)
            {
                Unsubscribe(nameof(CanHelicopterTarget));
                Unsubscribe(nameof(CanHelicopterStrafeTarget));
            }
            else
            {
                Subscribe(nameof(CanHelicopterTarget));
                Subscribe(nameof(CanHelicopterStrafeTarget));
            }
        }

        #endregion Helper

        private void CmdToggle(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            if (playerFlags.ContainsKey(player.userID))
            {
                playerFlags.Remove(player.userID);
                Print(player, Lang("Toggle", player.UserIDString, Lang("Disabled", player.UserIDString)));
            }
            else
            {
                if (PlayerFlagsInit(player)) Print(player, Lang("Toggle", player.UserIDString, Lang("Enabled", player.UserIDString)));
                else
                {
                    Print(player, Lang("NotAllowed", player.UserIDString));
                    return;
                }
            }
            CheckHooks();
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Disable when player disconneted")]
            public bool disableWhenDis = true;

            [JsonProperty(PropertyName = "Enable when player connected")]
            public bool enableWhenCon = false;

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat Command")]
                public string command = "dtm";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "[DontTargetMe]: ";

                [JsonProperty(PropertyName = "Chat Prefix Color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }
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

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command",
                ["Toggle"] = "Don't Target Me is {0}",
                ["Enabled"] = "<color=#8ee700>Enabled</color>",
                ["Disabled"] = "<color=#ce422b>Disabled</color>",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["Toggle"] = "不要瞄准我 {0}",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}