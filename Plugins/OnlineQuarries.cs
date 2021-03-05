using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Online Quarries", "mvrb/Arainrr", "1.2.5", ResourceId = 2216)]
    [Description("Automatically disable players' quarries when offline")]
    public class OnlineQuarries : RustPlugin
    {
        [PluginReference] private Plugin Friends, Clans;
        private Dictionary<ulong, Timer> stopEngineTimer = new Dictionary<ulong, Timer>();
        private HashSet<MiningQuarry> miningQuarries = new HashSet<MiningQuarry>();

        private void OnServerInitialized()
        {
            foreach (var miningQuarry in BaseNetworkable.serverEntities.OfType<MiningQuarry>())
                OnEntitySpawned(miningQuarry);
            CheckQuarries();
        }

        private void OnEntitySpawned(MiningQuarry miningQuarry)
        {
            if (miningQuarry == null || miningQuarry.OwnerID == 0) return;
            miningQuarries.Add(miningQuarry);
        }

        private void Unload()
        {
            foreach (var entry in stopEngineTimer)
                entry.Value?.Destroy();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null) return;
            if (stopEngineTimer.ContainsKey(player.userID))
            {
                stopEngineTimer[player.userID]?.Destroy();
                stopEngineTimer.Remove(player.userID);
            }
            if (configData.autoStart)
                CheckQuarries(player, true);
        }

        private void OnPlayerDisconnected(BasePlayer player)
        {
            if (player == null) return;
            ulong playerID = player.userID;
            if (stopEngineTimer.ContainsKey(playerID))
            {
                stopEngineTimer[player.userID]?.Destroy();
                stopEngineTimer.Remove(player.userID);
            }
            stopEngineTimer.Add(playerID, timer.Once(configData.offlineTime, () =>
            {
                CheckQuarries();
                stopEngineTimer.Remove(playerID);
            }));
        }

        private void OnQuarryToggled(MiningQuarry miningQuarry, BasePlayer player)
        {
            if (miningQuarry == null || miningQuarry.OwnerID == 0) return;
            bool isOn = miningQuarry.HasFlag(BaseEntity.Flags.On);
            if (configData.preventOther)
            {
                if (!CheckPlayer(miningQuarry.OwnerID, player.userID))
                {
                    if (isOn)
                    {
                        miningQuarry.SetOn(false);
                        Item item = ItemManager.CreateByName("lowgradefuel");
                        if (item == null) return;
                        item.MoveToContainer(miningQuarry.fuelStoragePrefab?.instance?.GetComponent<StorageContainer>()?.inventory);
                        isOn = false;
                    }
                    else miningQuarry.SetOn(true);
                }
                return;
            }
            if (isOn)
            {
                if (BasePlayer.FindByID(miningQuarry.OwnerID) != null) return;
                if (!CheckPlayer(miningQuarry.OwnerID))
                {
                    miningQuarry.SetOn(false);
                    Item item = ItemManager.CreateByName("lowgradefuel");
                    if (item == null) return;
                    item.MoveToContainer(miningQuarry.fuelStoragePrefab?.instance?.GetComponent<StorageContainer>()?.inventory);
                }
            }
        }

        private void CheckQuarries(BasePlayer player = null, bool isOn = false)
        {
            foreach (var miningQuarry in miningQuarries)
            {
                if (miningQuarry == null) continue;
                if (player != null)
                {
                    if (CheckPlayer(miningQuarry.OwnerID, player.userID) || miningQuarry.OwnerID == player.userID)
                        miningQuarry.SetOn(isOn);
                    continue;
                }
                if (!CheckPlayer(miningQuarry.OwnerID))
                    miningQuarry.SetOn(isOn);
            }
        }

        private bool CheckPlayer(ulong playerID, ulong friendID = 0)
        {
            if (friendID == 0)
            {
                foreach (var friend in BasePlayer.activePlayerList)
                    if (AreFriends(playerID, friend.userID)) return true;
            }
            else if (AreFriends(playerID, friendID))
                return true;
            return false;
        }

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (playerID == friendID) return true;
            if (configData.useTeam && SameTeam(friendID, playerID)) return true;
            if (configData.useFriends && HasFriend(friendID, playerID)) return true;
            if (configData.useClans && SameClan(friendID, playerID)) return true;
            return false;
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

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Use team")]
            public bool useTeam = false;

            [JsonProperty(PropertyName = "Use clans")]
            public bool useClans = false;

            [JsonProperty(PropertyName = "Use friends")]
            public bool useFriends = false;

            [JsonProperty(PropertyName = "Prevent other players from turning the quarry on or off")]
            public bool preventOther = false;

            [JsonProperty(PropertyName = "Automatically disable the delay of quarry (seconds)")]
            public float offlineTime = 120f;

            [JsonProperty(PropertyName = "Quarry automatically starts after players are online")]
            public bool autoStart = true;
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
    }
}