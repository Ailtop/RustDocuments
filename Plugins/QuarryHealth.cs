using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Quarry Health", "Waizujin/Arainrr", "1.1.2")]
    [Description("Changes the health value of quarries.")]
    public class QuarryHealth : RustPlugin
    {
        private void Init() => Unsubscribe(nameof(OnEntitySpawned));

        private void OnServerInitialized() => Subscribe(nameof(OnEntitySpawned));

        private void OnEntitySpawned(MiningQuarry miningQuarry) => UpdateQuarry(miningQuarry);

        private void UpdateQuarry(MiningQuarry miningQuarry)
        {
            if (miningQuarry == null) return;
            if (miningQuarry.ShortPrefabName == "mining_quarry")
            {
                miningQuarry.health = miningQuarry._maxHealth = configData.quarryHealth;
                miningQuarry.SendNetworkUpdateImmediate();
            }
            else if (miningQuarry.ShortPrefabName == "mining.pumpjack")
            {
                miningQuarry._health = miningQuarry._maxHealth = configData.pumpHealth;
                miningQuarry.SendNetworkUpdateImmediate();
            }
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Quarry Max Health")]
            public float quarryHealth = 3000f;

            [JsonProperty(PropertyName = "Pumpjack Max Health")]
            public float pumpHealth = 3000f;
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