using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Chinook Drop Randomizer", "shinnova/Arainrr", "1.5.1")]
    [Description("Make the chinook drop location more random")]
    public class ChinookDropRandomizer : RustPlugin
    {
        private bool initialized = false;
        private readonly static int GROUND_LAYER = LayerMask.GetMask("Terrain", "World", "Construction", "Deployed");
        private Dictionary<string, List<MonumentInfo>> monumentList = new Dictionary<string, List<MonumentInfo>>();

        private readonly Dictionary<string, float> defaultMonumentSizes = new Dictionary<string, float>()
        {
            ["Harbor"] = 125f,
            ["Giant Excavator Pit"] = 180f,
            ["Launch Site"] = 265f,
            ["Train Yard"] = 130f,
            ["Power Plant"] = 150f,
            ["Junkyard"] = 150f,
            ["Airfield"] = 200f,
            ["Water Treatment Plant"] = 190f,
            ["Bandit Camp"] = 80f,
            ["Sewer Branch"] = 80f,
            ["Oxum's Gas Station"] = 40f,
            ["Satellite Dish"] = 95f,
            ["Abandoned Supermarket"] = 30f,
            ["The Dome"] = 65f,
            ["Abandoned Cabins"] = 50f,
            ["Large Oil Rig"] = 100f,
            ["Oil Rig"] = 50f,
            ["Lighthouse"] = 40f,
            ["Outpost"] = 115f,
            ["HQM Quarry"] = 30f,
            ["Stone Quarry"] = 30f,
            ["Sulfur Quarry"] = 30f,
            ["Mining Outpost"] = 40f,
            ["Military Tunnel"] = 120f,
        };

        private void OnServerInitialized()
        {
            monumentList = TerrainMeta.Path?.Monuments?.Where(x => x.shouldDisplayOnMap)?.GroupBy(x => x.displayPhrase.english.Replace("\n", ""))?.ToDictionary(x => x.Key, y => y.ToList());
            UpdateConfig();
            initialized = true;
            foreach (var chinook in BaseNetworkable.serverEntities.OfType<CH47HelicopterAIController>())
                OnEntitySpawned(chinook);
        }

        private void UpdateConfig()
        {
            foreach (var monumentName in monumentList.Keys)
            {
                float monumentSize = 0f;
                defaultMonumentSizes.TryGetValue(monumentName, out monumentSize);
                if (!configData.monumentsSettings.ContainsKey(monumentName))
                    configData.monumentsSettings.Add(monumentName, new ConfigData.Monument { enabled = true, monumentSize = monumentSize });
            }
            SaveConfig();
        }

        private void OnEntitySpawned(CH47HelicopterAIController chinook)
        {
            if (chinook == null || !initialized) return;
            if (chinook.landingTarget != Vector3.zero) return;
            timer.Once(configData.dropDelay, () => TryDropCrate(chinook));
        }

        private object CanHelicopterDropCrate(CH47HelicopterAIController chinook)
        {
            if (configData.blockDefaultDrop) return false;
            return null;
        }

        private Vector3 GetGroundPosition(Vector3 position)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(position, Vector3.down, out hitInfo, 300f, GROUND_LAYER)) position.y = hitInfo.point.y;
            else position.y = TerrainMeta.HeightMap.GetHeight(position);
            return position;
        }

        private bool AboveWater(Vector3 location)
        {
            if (GetGroundPosition(location).y <= 0) return true;
            return false;
        }

        private bool AboveMonument(Vector3 location)
        {
            foreach (var entry in monumentList)
            {
                var monumentName = entry.Key;
                if (configData.monumentsSettings.ContainsKey(monumentName) && configData.monumentsSettings[monumentName].enabled)
                    foreach (var monumentInfo in entry.Value)
                        if (Vector3Ex.Distance2D(monumentInfo.transform.position, location) < configData.monumentsSettings[monumentName].monumentSize)
                            return true;
            }
            return false;
        }

        private void TryDropCrate(CH47HelicopterAIController chinook)
        {
            timer.Once(Random.Range(configData.minTime, configData.maxTime), () =>
            {
                if (chinook == null || chinook.IsDestroyed) return;
                if (chinook.numCrates > 0)
                {
                    if (configData.checkWater ? !AboveWater(chinook.transform.position) : true)
                    {
                        if (configData.checkMonument ? !AboveMonument(chinook.transform.position) : true)
                        {
                            if (BasePlayer.activePlayerList.Count >= configData.minPlayers)
                            {
                                chinook.DropCrate();
                                if (chinook.numCrates == 0) return;
                            }
                        }
                    }
                    TryDropCrate(chinook);
                }
            });
        }

        #region ConfigurationFile

        private ConfigData configData;

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Prevent the game from handling chinook drops")]
            public bool blockDefaultDrop = false;

            [JsonProperty(PropertyName = "Time before chinook starts trying to drop (seconds)")]
            public float dropDelay = 200f;

            [JsonProperty(PropertyName = "Minimum time until drop (seconds)")]
            public float minTime = 40f;

            [JsonProperty(PropertyName = "Maximum time until drop (seconds)")]
            public float maxTime = 60f;

            [JsonProperty(PropertyName = "Minimum number of online players to drop")]
            public int minPlayers = 0;

            [JsonProperty(PropertyName = "Don't drop above water")]
            public bool checkWater = true;

            [JsonProperty(PropertyName = "Don't drop above monuments")]
            public bool checkMonument = false;

            [JsonProperty(PropertyName = "What monuments to check (only works if monument checking is enabled)")]
            public Dictionary<string, Monument> monumentsSettings = new Dictionary<string, Monument>();

            public class Monument
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled;

                [JsonProperty(PropertyName = "Monument size")]
                public float monumentSize;
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
                PrintError("Config has corrupted or incorrectly formatted");
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