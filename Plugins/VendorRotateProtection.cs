using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Vendor Rotate Protection", "nuchacho/Arainrr", "1.1.2")]
    [Description("Prevents anyone except the entity owner from rotating vending machine.")]
    public class VendorRotateProtection : RustPlugin
    {
        [PluginReference] private readonly Plugin Friends, Clans;

        private object OnRotateVendingMachine(VendingMachine machine, BasePlayer player)
        {
            if (machine == null || player == null || machine.OwnerID == 0) return null;
            if (AreFriends(machine.OwnerID, player.userID)) return null;
            Print(player, Lang("CantRotateVendor", player.UserIDString));
            return false;
        }

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (playerID == friendID) return true;
            if (configData.useTeam && SameTeam(playerID, friendID)) return true;
            if (configData.useFriends && HasFriend(playerID, friendID)) return true;
            if (configData.useClans && SameClan(playerID, friendID)) return true;
            return false;
        }

        private bool HasFriend(ulong playerID, ulong friendID)
        {
            if (Friends == null) return false;
            return (bool)Friends.Call("HasFriend", playerID, friendID);
        }

        private bool SameTeam(ulong playerID, ulong friendID)
        {
            if (!RelationshipManager.TeamsEnabled()) return false;
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam == null) return false;
            var friendTeam = RelationshipManager.Instance.FindPlayersTeam(friendID);
            if (friendTeam == null) return false;
            return playerTeam == friendTeam;
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

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "[VendorRotateProtection]:";

            [JsonProperty(PropertyName = "Chat Prefix Color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Use Team")]
            public bool useTeam = false;

            [JsonProperty(PropertyName = "Use Friends")]
            public bool useFriends = false;

            [JsonProperty(PropertyName = "Use Clans")]
            public bool useClans = false;
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
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        #endregion ConfigurationFile

        #region LanguageFile

        protected override void LoadDefaultMessages()
        {
            //English
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "You can only rotate vending machines that you placed."
            }, this);

            // French
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "Vous pouvez uniquement faire pivoter les distributeurs automatiques que vous avez placés."
            }, this, "fr");

            // German
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "Sie können nur Verkaufsautomaten drehen, die Sie platziert haben."
            }, this, "de");

            // Russian
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "Вы можете вращать только торговые автоматы, которые вы разместили."
            }, this, "ru");

            // Spanish
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "Solo puede girar las máquinas expendedoras que haya colocado."
            }, this, "es");

            // Chinese
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantRotateVendor"] = "您不能旋转这个售货机。"
            }, this, "zh-CN");
        }

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        #endregion LanguageFile
    }
}