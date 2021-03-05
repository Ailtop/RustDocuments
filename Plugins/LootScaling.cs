using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Loot Scaling", "Kyrah Abattoir/Arainrr", "1.0.0", ResourceId = 1874)]
    [Description("Scale loot spawn rate/density by player count.")]
    internal class LootScaling : RustPlugin
    {
        //NOTE the minimum spawn rate/density are two arbitrary values in the engine from 0.1 to 1.0 (10%/100%)
        //you can override them by passing new values to the server with:
        //
        //spawn.min_rate
        //spawn.max_rate
        //spawn.min_density
        //spawn.max_density
        //
        //I have NOT tested changing these, but setting min_rate/density to 0 is probably NOT a good idea so don't do it!

        private void OnServerInitialized()
        {
            Dictionary<string, bool> newConfig = new Dictionary<string, bool>();
            foreach (SpawnPopulation spawnPopulation in SingletonComponent<SpawnHandler>.Instance.SpawnPopulations)
            {
                if (configData.populationScaling.ContainsKey(spawnPopulation.name))
                {
                    //Well since FacePunch already implemented it all for us, we should probably use it.
                    spawnPopulation.ScaleWithServerPopulation = configData.populationScaling[spawnPopulation.name];
                    newConfig.Add(spawnPopulation.name, configData.populationScaling[spawnPopulation.name]);
                }
                else
                    newConfig.Add(spawnPopulation.name, false);
            }
            configData.populationScaling = newConfig.OrderBy(x => x.Key).ToDictionary(p => p.Key, o => o.Value);
            SaveConfig();
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            //you can put here which of the spawn categories you wish to enable player count scaling on.
            //Rust will then adjust item density based on the percentage of players online from 10% rate/density to 100% rate/density
            [JsonProperty(PropertyName = "Population Scaling")]
            public Dictionary<string, bool> populationScaling;

            public static ConfigData DefaultConfig()
            {
                return new ConfigData()
                {
                    populationScaling = new Dictionary<string, bool>(),
                };
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
            configData = ConfigData.DefaultConfig();
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        #endregion ConfigurationFile
    }
}