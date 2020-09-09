using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Quarry Near No Build", "Arainrr", "1.2.0")]
    [Description("Prevent building near automatically spawned quarries.")]
    internal class QuarryNearNoBuild : RustPlugin
    {
        private const string PERMISSION_IGNORE = "quarrynearnobuild.ignore";
        private readonly List<QuarryInfo> quarryInfos = new List<QuarryInfo>();

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_IGNORE, this);
        }
         

        private void OnServerInitialized()
        {
            FindMiningQuarry();
            Subscribe(nameof(OnEntitySpawned));
            foreach (var miningQuarry in BaseNetworkable.serverEntities.OfType<MiningQuarry>())
            {
                PrintError($"{miningQuarry.name} ");
                OnEntitySpawned(miningQuarry);
            }
        }

        private void OnEntitySpawned(MiningQuarry miningQuarry)
        {
            if (miningQuarry == null) return; 
        }

        private class QuarryInfo
        {
            public Vector3 position;
            public float radius;
        }

        private enum QuarryType
        {
            SulfurQuarry,
            StoneQuarry,
            HQMQuarry,
        }

        private void FindMiningQuarry()
        {
            foreach (var monumentInfo in TerrainMeta.Path.Monuments)
            {
                switch (monumentInfo.name)
                {
                    case "assets/bundled/prefabs/autospawn/monument/small/mining_quarry_a.prefab":
                        if (configData.blockRadius[QuarryType.SulfurQuarry] > 0)
                            quarryInfos.Add(new QuarryInfo { position = monumentInfo.transform.position, radius = configData.blockRadius[QuarryType.SulfurQuarry] });
                        continue;

                    case "assets/bundled/prefabs/autospawn/monument/small/mining_quarry_b.prefab":
                        if (configData.blockRadius[QuarryType.StoneQuarry] > 0)
                            quarryInfos.Add(new QuarryInfo { position = monumentInfo.transform.position, radius = configData.blockRadius[QuarryType.StoneQuarry] });
                        continue;

                    case "assets/bundled/prefabs/autospawn/monument/small/mining_quarry_c.prefab":
                        if (configData.blockRadius[QuarryType.HQMQuarry] > 0)
                            quarryInfos.Add(new QuarryInfo { position = monumentInfo.transform.position, radius = configData.blockRadius[QuarryType.HQMQuarry] });
                        continue;

                    default: continue;
                }
            }
        }

        private object CanBuild(Planner planner, Construction prefab, Construction.Target target)
        {
            var player = planner?.GetOwnerPlayer();
            if (player == null) return null;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_IGNORE)) return null;
            Vector3 position = target.entity?.CenterPoint() ?? target.position;
            foreach (var quarryInfo in quarryInfos)
            {
                if (quarryInfo.radius >= Vector3.Distance(quarryInfo.position, position))
                {
                    Print(player, Lang("CantBuildOnStatic", player.UserIDString));
                    return false;
                }
            }
            return null;
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Block Radius")]
            public Dictionary<QuarryType, float> blockRadius = new Dictionary<QuarryType, float>()
            {
                [QuarryType.SulfurQuarry] = 60f,
                [QuarryType.StoneQuarry] = 60f,
                [QuarryType.HQMQuarry] = 60f,
            };

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "[QuarryNotBuild]: ";

            [JsonProperty(PropertyName = "Chat Prefix Color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;
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

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantBuildOnStatic"] = "Prevent building near this quarry"
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantBuildOnStatic"] = "您不能在系统矿机附近建造"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}