using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Laptop Crate Hack", "TheSurgeon/Arainrr", "1.1.2")]
    [Description("Require a laptop to hack a crate.")]
    public class LaptopCrateHack : RustPlugin
    {
        [PluginReference] private readonly Plugin Friends, Clans;
        private const int LAPTOP_ITEMID = 1523195708;

        private Dictionary<uint, int> extraHackTimes;

        private void Init()
        {
            if (!configData.ownCrate) Unsubscribe(nameof(CanLootEntity));
            if (!configData.extraHack) Unsubscribe(nameof(OnPlayerInput));
            if (configData.maxExtraHack < 0)
            {
                Unsubscribe(nameof(OnEntityKill));
            }
            else
            {
                extraHackTimes = new Dictionary<uint, int>();
            }
        }

        private object CanHackCrate(BasePlayer player, HackableLockedCrate crate)
        {
            if (configData.requireInHand)
            {
                var activeItem = player.GetActiveItem();
                if (activeItem == null || activeItem.info.itemid != LAPTOP_ITEMID)
                {
                    Print(player, Lang("NotHolding", player.UserIDString));
                    return false;
                }
            }
            var amount = player.inventory.GetAmount(LAPTOP_ITEMID);
            if (amount < configData.numberRequired)
            {
                Print(player, Lang("YouNeed", player.UserIDString, configData.numberRequired, amount));
                return false;
            }
            if (configData.consumeLaptop)
            {
                List<Item> collect = new List<Item>();
                player.inventory.Take(collect, LAPTOP_ITEMID, configData.numberRequired);
                foreach (Item item in collect) item.Remove();
            }
            if (configData.ownCrate) crate.OwnerID = player.userID;
            crate.hackSeconds = HackableLockedCrate.requiredHackSeconds - configData.unlockTime;
            return null;
        }

        private object CanLootEntity(BasePlayer player, HackableLockedCrate crate)
        {
            if (crate.OwnerID.IsSteamId() && !AreFriends(player.userID, crate.OwnerID))
            {
                Print(player, Lang("YouDontOwn", player.UserIDString));
                return false;
            }
            return null;
        }

        private void OnPlayerInput(BasePlayer player, InputState input)
        {
            if (player == null || input == null) return;
            if (input.WasJustPressed(BUTTON.USE))
            {
                var activeItem = player.GetActiveItem();
                if (activeItem != null && activeItem.info.itemid == LAPTOP_ITEMID)
                {
                    var crate = GetLookEntity(player);
                    if (crate == null || crate.net == null || crate.IsFullyHacked() || !crate.IsBeingHacked()) return;
                    if (crate.hackSeconds > HackableLockedCrate.requiredHackSeconds) return;
                    if (configData.maxExtraHack > 0)
                    {
                        int times;
                        if (extraHackTimes.TryGetValue(crate.net.ID, out times) && times >= configData.maxExtraHack)
                        {
                            return;
                        }

                        if (extraHackTimes.ContainsKey(crate.net.ID))
                        {
                            extraHackTimes[crate.net.ID]++;
                        }
                        else extraHackTimes.Add(crate.net.ID, 1);
                    }

                    activeItem.UseItem();
                    crate.hackSeconds += configData.extraUnlockTime;
                }
            }
        }

        private void OnEntityKill(HackableLockedCrate crate)
        {
            if (crate == null || crate.net == null) return;
            extraHackTimes.Remove(crate.net.ID);
        }

        private static HackableLockedCrate GetLookEntity(BasePlayer player)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(player.eyes.HeadRay(), out hitInfo, 20f, Rust.Layers.Solid))
                return hitInfo.GetEntity() as HackableLockedCrate;
            return null;
        }

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (playerID == friendID) return true;
            if (configData.useTeams && SameTeam(friendID, playerID)) return true;
            if (configData.useFriends && HasFriend(friendID, playerID)) return true;
            if (configData.useClans && SameClan(friendID, playerID)) return true;
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
            [JsonProperty(PropertyName = "Require laptop to be in hand (True/False)")]
            public bool requireInHand = false;

            [JsonProperty(PropertyName = "Consume laptop (True/False)")]
            public bool consumeLaptop = true;

            [JsonProperty(PropertyName = "Laptops Required (Must be greater than 0)")]
            public int numberRequired = 1;

            [JsonProperty(PropertyName = "Hack crate unlock time (Seconds)")]
            public float unlockTime = 900f;

            [JsonProperty(PropertyName = "Use additional hack (True/False) (Use laptop to reduce crate unlocking time)")]
            public bool extraHack = false;

            [JsonProperty(PropertyName = "Maximum times of additional hack (0 = Disable)")]
            public int maxExtraHack = 0;

            [JsonProperty(PropertyName = "When a laptop consumed, how much unlock time reduces? (Seconds)")]
            public float extraUnlockTime = 300f;

            [JsonProperty(PropertyName = "Only player that hacked can loot? (True/False)")]
            public bool ownCrate = false;

            [JsonProperty(PropertyName = "Use Teams")]
            public bool useTeams = false;

            [JsonProperty(PropertyName = "Use Friends")]
            public bool useFriends = false;

            [JsonProperty(PropertyName = "Use Clans")]
            public bool useClans = false;

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "[LaptopCrateHack]: ";

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

        private void Print(BasePlayer player, string message)
        {
            if (string.IsNullOrEmpty(configData.chatS.prefix))
                Player.Message(player, message, string.Empty, configData.chatS.steamIDIcon);
            else Player.Message(player, message, $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["YouNeed"] = "Error: You need {0} Targeting Computers and you only have {1}.",
                ["NotHolding"] = "Error: You must be holding a Targeting Computer in your hand to hack this crate.",
                ["YouDontOwn"] = "Error: Only the player that hacked this crate can loot it."
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["YouNeed"] = "破解黑客箱需要 {0} 个计算机，但是您只有 {1} 个.",
                ["NotHolding"] = "您手上必须拿着计算机才可以破解黑客箱",
                ["YouDontOwn"] = "只有破解这个黑客箱的玩家才可以掠夺它"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}