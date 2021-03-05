using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Don't Target Me", "Quantum/Arainrr", "1.1.2")]
    [Description("Makes turrets, player npcs and normal npcs ignore you.")]
    public class DontTargetMe : RustPlugin
    {
        #region Fileds

        private const string PERMISSION_ALL = "donttargetme.all";
        private const string PERMISSION_NPC = "donttargetme.npc";
        private const string PERMISSION_APC = "donttargetme.apc";
        private const string PERMISSION_SAM = "donttargetme.sam";
        private const string PERMISSION_HELI = "donttargetme.heli";
        private const string PERMISSION_TURRETS = "donttargetme.turrets";

        //Reduce boxing
        private static readonly object True = true, False = false, Null = null;

        private readonly Dictionary<ulong, TargetFlags> playerFlags = new Dictionary<ulong, TargetFlags>();

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

        #endregion Fileds

        #region Oxide Hooks

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
            if (player == null || !player.userID.IsSteamId()) return;
            if (PlayerFlagsInit(player))
            {
                CheckHooks();
            }
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            if (playerFlags.Remove(player.userID))
            {
                CheckHooks();
            }
        }

        private object CanBeTargeted(BasePlayer player, MonoBehaviour behaviour) => HasTargetFlags(player, TargetFlags.Turret) ? False : Null;

        private object OnNpcTarget(BaseEntity npc, BasePlayer player) => HasTargetFlags(player, TargetFlags.Npc) ? True : Null;

        private object CanBradleyApcTarget(BradleyAPC apc, BasePlayer player) => HasTargetFlags(player, TargetFlags.Bradley) ? False : Null;

        private object CanHelicopterTarget(PatrolHelicopterAI heli, BasePlayer player) => HasTargetFlags(player, TargetFlags.Helicopter) ? False : Null;

        private object CanHelicopterStrafeTarget(PatrolHelicopterAI heli, BasePlayer player) => HasTargetFlags(player, TargetFlags.Helicopter) ? False : Null;

        private object OnSamSiteTarget(SamSite samSite, BaseCombatEntity baseCombatEntity) => AnyHasTargetFlags(baseCombatEntity, TargetFlags.Sam) ? False : Null;

        #endregion Oxide Hooks

        #region Methods

        private bool AnyHasTargetFlags(BaseCombatEntity baseCombatEntity, TargetFlags flag)
        {
            var baseVehicle = baseCombatEntity as BaseVehicle;
            if (baseVehicle != null)
            {
                var mountedPlayers = GetMountedPlayers(baseVehicle);
                foreach (var mountedPlayer in mountedPlayers)
                {
                    if (HasTargetFlags(mountedPlayer, flag))
                    {
                        return true;
                    }
                }
                return false;
            }
            var children = baseCombatEntity.GetComponentsInChildren<BasePlayer>();
            if (children != null && children.Length > 0)
            {
                foreach (var child in children)
                {
                    if (HasTargetFlags(child, flag))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static IEnumerable<BasePlayer> GetMountedPlayers(BaseVehicle baseVehicle)
        {
            if (!baseVehicle.HasMountPoints())
            {
                var mountedPlayer = baseVehicle.GetMounted();
                if (mountedPlayer != null)
                {
                    yield return mountedPlayer;
                }
            }

            foreach (var mountPointInfo in baseVehicle.mountPoints)
            {
                if (mountPointInfo.mountable != null)
                {
                    var mountedPlayer = mountPointInfo.mountable.GetMounted();
                    if (mountedPlayer != null)
                    {
                        yield return mountedPlayer;
                    }
                }
            }
        }

        private bool HasTargetFlags(BasePlayer player, TargetFlags flag)
        {
            if (player == null || !player.userID.IsSteamId()) return false;
            TargetFlags flags;
            if (playerFlags.TryGetValue(player.userID, out flags))
            {
                return flags.HasFlag(flag);
            }
            return false;
        }

        private bool PlayerFlagsInit(BasePlayer player)
        {
            playerFlags.Remove(player.userID);
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

        #endregion Methods

        #region Commands

        private void CmdToggle(BasePlayer player, string command, string[] args)
        {
            if (player == null) return;
            if (playerFlags.Remove(player.userID))
            {
                Print(player, Lang("Toggle", player.UserIDString, Lang("Disabled", player.UserIDString)));
            }
            else
            {
                if (PlayerFlagsInit(player))
                {
                    Print(player, Lang("Toggle", player.UserIDString, Lang("Enabled", player.UserIDString)));
                }
                else
                {
                    Print(player, Lang("NotAllowed", player.UserIDString));
                    return;
                }
            }
            CheckHooks();
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Disable when player disconnected")]
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
                public string prefix = "<color=#00FFFF>[DontTargetMe]</color>: ";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
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
                    if (GetConfigValue(out prefix, "Chat Settings", "Chat Prefix") && GetConfigValue(out prefixColor, "Chat Settings", "Chat Prefix Color"))
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

        #region LanguageFile

        private void Print(BasePlayer player, string message) => Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);

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
                ["NotAllowed"] = "您没有使用该命令的权限",
                ["Toggle"] = "不要瞄准我 {0}",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}