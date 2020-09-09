using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;

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

        private void CheckQuarries(BasePlayer otherPlayer = null, bool isOn = false)
        {
            foreach (var miningQuarry in miningQuarries)
            {
                if (miningQuarry == null) continue;
                if (otherPlayer != null)
                {
                    if (CheckPlayer(miningQuarry.OwnerID, otherPlayer.userID) || miningQuarry.OwnerID == otherPlayer.userID)
                        miningQuarry.SetOn(isOn);
                    continue;
                }
                if (!CheckPlayer(miningQuarry.OwnerID))
                    miningQuarry.SetOn(isOn);
            }
        }

        private bool CheckPlayer(ulong playerID, ulong otherPlayerID = 0)
        {
            var player = RustCore.FindPlayerById(playerID);
            if (otherPlayerID == 0)
            {
                foreach (var otherPlayer in BasePlayer.activePlayerList)
                {
                    if (otherPlayer == null) continue;
                    if (playerID == otherPlayer.userID) return true;
                    if (player != null && SameTeam(player, otherPlayer)) return true;
                    if (HasFriend(playerID, otherPlayer.userID)) return true;
                    if (SameClan(playerID, otherPlayer.userID)) return true;
                }
            }
            else
            {
                if (playerID == otherPlayerID) return true;
                var otherPlayer = RustCore.FindPlayerById(otherPlayerID);
                if (player != null && otherPlayer != null && SameTeam(player, otherPlayer)) return true;
                if (HasFriend(playerID, otherPlayerID)) return true;
                if (SameClan(playerID, otherPlayerID)) return true;
            }
            return false;
        }

        private bool HasFriend(ulong playerID, ulong otherPlayerID)
        {
            if (Friends == null || !configData.useFriends) return false;
            return (bool)Friends.Call("HasFriend", playerID, otherPlayerID);
        }

        private bool SameTeam(BasePlayer player, BasePlayer otherPlayer)
        {
            if (player.currentTeam == 0 || otherPlayer.currentTeam == 0 || !configData.useTeam) return false;
            return player.currentTeam == otherPlayer.currentTeam;
        }

        private bool SameClan(ulong playerID, ulong otherPlayerID)
        {
            if (Clans == null || !configData.useClans) return false;
            var playerClan = (string)Clans.Call("GetClanOf", playerID);
            var otherPlayerClan = (string)Clans.Call("GetClanOf", otherPlayerID);
            if (playerClan == null || otherPlayerClan == null) return false;
            return playerClan == otherPlayerClan;
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
    }
}