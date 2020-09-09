using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Sam Site Map", "Arainrr", "1.3.5")]
    [Description("Mark all samsites in the map")]
    internal class SamSiteMap : RustPlugin
    {
        private const string PERMISSION_USE = "samsitemap.use";
        private const string PREFAB_MARKER = "assets/prefabs/tools/map/genericradiusmarker.prefab";
        private const string PREFAB_TEXT = "assets/prefabs/deployable/vendingmachine/vending_mapmarker.prefab";
        private readonly HashSet<MapMarker> mapMarkers = new HashSet<MapMarker>();
        private readonly Dictionary<SamSite, MapMarkerGenericRadius> samSiteMarkers = new Dictionary<SamSite, MapMarkerGenericRadius>();

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_USE, this);
            if (!configData.usePermission) Unsubscribe(nameof(CanNetworkTo));
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                if (baseNetworkable is SamSite)
                    OnEntitySpawned(baseNetworkable as SamSite);
            }
        }

        private object CanNetworkTo(MapMarker marker, BasePlayer player)
        {
            if (marker == null || player == null) return null;
            if (!mapMarkers.Contains(marker)) return null;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return null;
            else return false;
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (configData.usePermission && !permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return;
            if (player.IsReceivingSnapshot)
            {
                timer.Once(1f, () => OnPlayerConnected(player));
                return;
            }
            foreach (var entry in samSiteMarkers)
            {
                if (entry.Value == null) continue;
                foreach (var mapMarker in entry.Value.GetComponentsInChildren<MapMarker>())
                {
                    if (mapMarker is MapMarkerGenericRadius)
                        (mapMarker as MapMarkerGenericRadius).SendUpdate();
                    else mapMarker.SendNetworkUpdate();
                }
                entry.Value.SendUpdate();
            }
        }

        private void OnEntitySpawned(SamSite samSite)
        {
            NextTick(() =>
            {
                if (samSite == null) return;
                CreateMapMarker(samSite, samSite.OwnerID == 0);
            });
        }

        private void OnEntityKill(SamSite samSite)
        {
            if (samSite == null) return;
            MapMarkerGenericRadius mapMarker;
            if (!samSiteMarkers.TryGetValue(samSite, out mapMarker)) return;
            KillMapMarker(mapMarker);
            samSiteMarkers.Remove(samSite);
        }

        private void OnEntityKill(MapMarker mapMarker) => mapMarkers.Remove(mapMarker);

        private void Unload()
        {
            foreach (var entry in samSiteMarkers)
                KillMapMarker(entry.Value);
            samSiteMarkers.Clear();
        }

        private void KillMapMarker(MapMarkerGenericRadius mapMarker)
        {
            if (mapMarker != null && !mapMarker.IsDestroyed)
                mapMarker.KillMessage();
        }

        private void CreateMapMarker(SamSite samSite, bool isStatic = false)
        {
            if (isStatic ? !configData.staticSamS.enabled : !configData.playerSamS.enabled) return;
            if (samSiteMarkers.ContainsKey(samSite)) return;
            //Text map marker
            VendingMachineMapMarker machineMapMarker = null;
            if (!string.IsNullOrEmpty(isStatic ? configData.staticSamS.samSiteMarker.text : configData.playerSamS.samSiteMarker.text))
            {
                machineMapMarker = GameManager.server.CreateEntity(PREFAB_TEXT, samSite.transform.position) as VendingMachineMapMarker;
                if (machineMapMarker != null)
                {
                    machineMapMarker.markerShopName = isStatic ? configData.staticSamS.samSiteMarker.text : configData.playerSamS.samSiteMarker.text;
                    machineMapMarker.OwnerID = samSite.OwnerID;
                    mapMarkers.Add(machineMapMarker);
                    machineMapMarker.Spawn();
                    machineMapMarker.SendNetworkUpdate();
                }
            }
            //Attack range map marker
            MapMarkerGenericRadius radiusMapMarker = null;
            if ((isStatic && configData.staticSamS.samSiteMarker.samSiteRadiusMarker.enabled) || (!isStatic && configData.playerSamS.samSiteMarker.samSiteRadiusMarker.enabled))
            {
                radiusMapMarker = GameManager.server.CreateEntity(PREFAB_MARKER, samSite.transform.position) as MapMarkerGenericRadius;
                if (radiusMapMarker != null)
                {
                    radiusMapMarker.alpha = isStatic ? configData.staticSamS.samSiteMarker.samSiteRadiusMarker.alpha : configData.playerSamS.samSiteMarker.samSiteRadiusMarker.alpha;
                    var color1 = isStatic ? configData.staticSamS.samSiteMarker.samSiteRadiusMarker.colorl : configData.playerSamS.samSiteMarker.samSiteRadiusMarker.colorl;
                    if (!ColorUtility.TryParseHtmlString(color1, out radiusMapMarker.color1))
                    {
                        radiusMapMarker.color1 = Color.black;
                        PrintError($"Invalid range map marker color1: {color1}");
                    }
                    var color2 = isStatic ? configData.staticSamS.samSiteMarker.samSiteRadiusMarker.color2 : configData.playerSamS.samSiteMarker.samSiteRadiusMarker.color2;
                    if (!ColorUtility.TryParseHtmlString(color2, out radiusMapMarker.color2))
                    {
                        radiusMapMarker.color2 = Color.white;
                        PrintError($"Invalid range map marker color2: {color2}");
                    }
                    radiusMapMarker.radius = samSite.scanRadius / 145f;
                    radiusMapMarker.OwnerID = samSite.OwnerID;
                    mapMarkers.Add(radiusMapMarker);
                    radiusMapMarker.Spawn();
                    radiusMapMarker.SendUpdate();
                }
            }
            //Sam map marker
            MapMarkerGenericRadius mapMarker = GameManager.server.CreateEntity(PREFAB_MARKER, samSite.transform.position) as MapMarkerGenericRadius;
            if (mapMarker != null)
            {
                mapMarker.alpha = isStatic ? configData.staticSamS.samSiteMarker.alpha : configData.playerSamS.samSiteMarker.alpha;
                var color1 = isStatic ? configData.staticSamS.samSiteMarker.colorl : configData.playerSamS.samSiteMarker.colorl;
                if (!ColorUtility.TryParseHtmlString(color1, out mapMarker.color1))
                {
                    mapMarker.color1 = Color.black;
                    PrintError($"Invalid map marker color1: {color1}");
                }
                var color2 = isStatic ? configData.staticSamS.samSiteMarker.color2 : configData.playerSamS.samSiteMarker.color2;
                if (!ColorUtility.TryParseHtmlString(color2, out mapMarker.color2))
                {
                    mapMarker.color2 = Color.white;
                    PrintError($"Invalid map marker color2: {color2}");
                }
                mapMarker.radius = isStatic ? configData.staticSamS.samSiteMarker.radius : configData.playerSamS.samSiteMarker.radius;
                mapMarker.OwnerID = samSite.OwnerID;
                mapMarkers.Add(mapMarker);
                mapMarker.Spawn();
                mapMarker.SendUpdate();
            }

            if (radiusMapMarker != null) radiusMapMarker.SetParent(mapMarker, true);
            if (machineMapMarker != null) machineMapMarker.SetParent(mapMarker, true);

            samSiteMarkers.Add(samSite, mapMarker);
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Use permission")]
            public bool usePermission = false;

            [JsonProperty(PropertyName = "Static SAM settings")]
            public SamSiteSettings staticSamS = new SamSiteSettings
            {
                enabled = true,
                samSiteMarker = new SamSiteMarker
                {
                    radius = 0.08f,
                    colorl = "#FF4500",
                    color2 = "#0000FF",
                    alpha = 1f,
                    text = "static sam",
                    samSiteRadiusMarker = new SamSiteRadiusMarker
                    {
                        colorl = "#FFFF00",
                        color2 = "#FFFFF0",
                        alpha = 0.5f,
                    }
                },
            };

            [JsonProperty(PropertyName = "Player's SAM settings")]
            public SamSiteSettings playerSamS = new SamSiteSettings
            {
                enabled = true,
                samSiteMarker = new SamSiteMarker
                {
                    radius = 0.08f,
                    colorl = "#00FF00",
                    color2 = "#0000FF",
                    alpha = 1f,
                    text = "player's sam",
                    samSiteRadiusMarker = new SamSiteRadiusMarker
                    {
                        colorl = "#FFFF00",
                        color2 = "#FFFFF0",
                        alpha = 0.5f,
                    }
                },
            };
        }

        private class SamSiteSettings
        {
            [JsonProperty(PropertyName = "Enabled map marker")]
            public bool enabled = true;

            [JsonProperty(PropertyName = "SAM map marker")]
            public SamSiteMarker samSiteMarker = new SamSiteMarker();
        }

        private class SamSiteMarker
        {
            [JsonProperty(PropertyName = "Map marker radius")]
            public float radius = 0.08f;

            [JsonProperty(PropertyName = "Map marker color1")]
            public string colorl = "#00FF00";

            [JsonProperty(PropertyName = "Map marker color2")]
            public string color2 = "#0000FF";

            [JsonProperty(PropertyName = "Map marker alpha")]
            public float alpha = 1f;

            [JsonProperty(PropertyName = "Map marker text")]
            public string text = "sam";

            [JsonProperty(PropertyName = "SAM attack range map marker")]
            public SamSiteRadiusMarker samSiteRadiusMarker = new SamSiteRadiusMarker();
        }

        private class SamSiteRadiusMarker
        {
            [JsonProperty(PropertyName = "Enabled Sam attack range map marker")]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Range map marker color1")]
            public string colorl = "#FFFF00";

            [JsonProperty(PropertyName = "Range map marker color2")]
            public string color2 = "#FFFFF0";

            [JsonProperty(PropertyName = "Range map marker alpha")]
            public float alpha = 0.5f;
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