using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("No Durability", "Wulf/lukespragg/Arainrr", "2.2.3", ResourceId = 1061)]
    public class NoDurability : RustPlugin
    {
        [PluginReference] private readonly Plugin ZoneManager, DynamicPVP;
        private const string PERMISSION_USE = "nodurability.allowed";

        private void Init() => permission.RegisterPermission(PERMISSION_USE, this);

        private void OnLoseCondition(Item item, ref float amount)
        {
            if (item?.info == null) return;
            var player = item.GetOwnerPlayer() ?? item.GetRootContainer()?.GetOwnerPlayer();
            if (player == null) return;
            if (!configData.itemExcludeList.Contains(item.info.shortname) && permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                if (configData.useZoneManager && ZoneManager != null)
                {
                    var zoneIDs = GetPlayerZoneIDs(player);
                    if (zoneIDs != null && zoneIDs.Length > 0)
                    {
                        if (configData.excludeAllZone)
                        {
                            return;
                        }
                        if (configData.excludeDynPVPZone && DynamicPVP != null)
                        {
                            foreach (var zoneID in zoneIDs)
                            {
                                if (IsPlayerInZone(zoneID, player) && IsDynamicPVPZone(zoneID))
                                {
                                    return;
                                }
                            }
                            return;
                        }
                        foreach (var zoneID in configData.zoneExcludeList)
                        {
                            if (IsPlayerInZone(zoneID, player))
                            {
                                return;
                            }
                        }
                    }
                }
                item.condition = item.maxCondition;
            }
        }

        private bool IsDynamicPVPZone(string zoneID) => (bool)DynamicPVP.Call("IsDynamicPVPZone", zoneID);

        private bool IsPlayerInZone(string zoneID, BasePlayer player) => (bool)ZoneManager.Call("IsPlayerInZone", zoneID, player);

        private string[] GetPlayerZoneIDs(BasePlayer player) => (string[])ZoneManager.Call("GetPlayerZoneIDs", player);

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Use ZoneManager")]
            public bool useZoneManager;

            [JsonProperty(PropertyName = "Exclude all zone")]
            public bool excludeAllZone;

            [JsonProperty(PropertyName = "Exclude dynamic pvp zone")]
            public bool excludeDynPVPZone;

            [JsonProperty(PropertyName = "Zone exclude list (Zone ID)")]
            public HashSet<string> zoneExcludeList = new HashSet<string>();

            [JsonProperty(PropertyName = "Item exclude list (Item shortname)")]
            public HashSet<string> itemExcludeList = new HashSet<string>();
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