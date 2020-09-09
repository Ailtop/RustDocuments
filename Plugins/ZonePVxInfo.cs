//Requires: ZoneManager
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Zone PVx Info", "BuzZ[PHOQUE]/Arainrr", "1.0.1")]
    [Description("HUD on PVx name defined Zones")]
    public class ZonePVxInfo : RustPlugin
    {
        [PluginReference] private readonly Plugin ZoneManager;
        private const string UINAME_MAIN = "ZonePVxInfoUI";
        private bool pvpAll;

        private enum PVxType
        {
            PVE,
            PVP
        }

        #region Oxide Hooks

        private void Init()
        {
            AddCovalenceCommand("pvpall", nameof(CmdServerPVx));
        }

        private void OnServerInitialized()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                OnPlayerConnected(player);
            }
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyUI(player);
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            if (player.IsReceivingSnapshot)
            {
                timer.Once(1f, () => OnPlayerConnected(player));
                return;
            }

            if (pvpAll)
            {
                CratePVxUI(player, PVxType.PVP);
            }
            else
            {
                CheckPlayerZone(player);
            }
        }

        #endregion Oxide Hooks

        #region ZoneManager

        private string GetZoneName(string zoneID) => (string)ZoneManager.Call("GetZoneName", zoneID);

        private string[] GetPlayerZoneIDs(BasePlayer player) => (string[])ZoneManager.Call("GetPlayerZoneIDs", player);

        private void OnEnterZone(string zoneID, BasePlayer player) => CheckPlayerZone(player);

        private void OnExitZone(string zoneID, BasePlayer player) => CheckPlayerZone(player);

        private void CheckPlayerZone(BasePlayer player)
        {
            if (pvpAll || player == null || !player.IsConnected || !player.userID.IsSteamId()) return;
            var zoneIDs = GetPlayerZoneIDs(player);
            foreach (var zoneID in zoneIDs)
            {
                string zoneName = GetZoneName(zoneID);
                if (string.IsNullOrEmpty(zoneName)) continue;
                switch (configData.defaultType)
                {
                    case PVxType.PVE:
                        if (zoneName.Contains("pvp", CompareOptions.IgnoreCase))
                        {
                            CratePVxUI(player, PVxType.PVP);
                            return;
                        }
                        continue;
                    case PVxType.PVP:
                        if (zoneName.Contains("pve", CompareOptions.IgnoreCase))
                        {
                            CratePVxUI(player, PVxType.PVE);
                            return;
                        }
                        continue;
                    default: continue;
                }
            }
            CratePVxUI(player, configData.defaultType);
        }

        #endregion ZoneManager

        #region RaidableBases

        private void OnPlayerEnteredRaidableBase(BasePlayer player, Vector3 location, bool allowPVP)
        {
            if (pvpAll) return;
            CratePVxUI(player, allowPVP ? PVxType.PVP : PVxType.PVE);
        }

        private void OnPlayerExitedRaidableBase(BasePlayer player, Vector3 location, bool allowPVP)
        {
            CheckPlayerZone(player);
        }

        #endregion RaidableBases

        #region Commands

        private void CmdServerPVx(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin) return;
            if (args == null || args.Length < 1) return;
            switch (args[0].ToLower())
            {
                case "0":
                case "off":
                case "false":
                    pvpAll = false;
                    foreach (var player in BasePlayer.activePlayerList)
                    {
                        CheckPlayerZone(player);
                    }
                    return;

                case "1":
                case "on":
                case "true":
                    pvpAll = true;
                    foreach (var player in BasePlayer.activePlayerList)
                    {
                        CratePVxUI(player, PVxType.PVP);
                    }
                    return;
            }
        }

        #endregion Commands

        #region UI

        private void CratePVxUI(BasePlayer player, PVxType type)
        {
            string zoneType, zoneColor, textColor;
            switch (type)
            {
                case PVxType.PVE:
                    zoneType = "PVE";
                    zoneColor = configData.pveColor;
                    textColor = configData.pveTextColor;
                    break;

                case PVxType.PVP:
                    zoneType = "PVP";
                    zoneColor = configData.pvpColor;
                    textColor = configData.pvpTextColor;
                    break;

                default: return;
            }
            DestroyUI(player);
            var container = new CuiElementContainer
            {
                {
                    new CuiPanel
                    {
                        Image = { Color = zoneColor },
                        RectTransform =
                        {
                            AnchorMin = configData.minAnchor,
                            AnchorMax = configData.maxAnchor
                        }
                    },
                    new CuiElement().Parent = "Overlay",
                    UINAME_MAIN
                },
                {
                    new CuiLabel
                    {
                        Text =
                        {
                            Text = zoneType, FontSize = configData.textSize, Align = TextAnchor.MiddleCenter,
                            Color = textColor
                        },
                        RectTransform = { AnchorMin = "0.10 0.10", AnchorMax = "0.90 0.90" }
                    },
                    UINAME_MAIN,
                    CuiHelper.GetGuid()
                }
            };
            CuiHelper.AddUi(player, container);
        }

        private static void DestroyUI(BasePlayer player) => CuiHelper.DestroyUi(player, UINAME_MAIN);

        #endregion UI

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty(PropertyName = "Server Default PVx (pvp or pve)")]
            public PVxType defaultType = PVxType.PVE;

            [JsonProperty(PropertyName = "UI Color - For PVP")]
            public string pvpColor = "0.6 0.2 0.2 0.5";

            [JsonProperty(PropertyName = "UI Color - For PVE")]
            public string pveColor = "0.5 1.0 0.0 0.4";

            [JsonProperty(PropertyName = "UI Text - Size")]
            public int textSize = 14;

            [JsonProperty(PropertyName = "UI Text - Color For PVP")]
            public string pvpTextColor = "1 1 1 1";

            [JsonProperty(PropertyName = "UI Text - Color For PVE")]
            public string pveTextColor = "1 1 1 1";

            [JsonProperty(PropertyName = "UI Anchor - Min")]
            public string minAnchor = "0.65 0.04";

            [JsonProperty(PropertyName = "UI Anchor - Max")]
            public string maxAnchor = "0.69 0.08";
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