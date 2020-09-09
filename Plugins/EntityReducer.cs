using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Entity Reducer", "Arainrr", "2.0.1")]
    [Description("Control all spawn populations on your server")]
    public class EntityReducer : RustPlugin
    {
        private static SpawnHandler spawnHandler;  
        private float densityToMaxPopulation;

        private void OnServerInitialized()
        {
            spawnHandler = SingletonComponent<SpawnHandler>.Instance;
            densityToMaxPopulation = TerrainMeta.Size.x * TerrainMeta.Size.z * 1E-06f;
            UpdateConfig();
            SpawnHandlerHelper();
        }

        private void UpdateConfig()
        {
            Dictionary<string, int> newPopulationSettings = new Dictionary<string, int>();
            for (int i = 0; i < spawnHandler.AllSpawnPopulations.Length; i++)
            {
                if (spawnHandler.AllSpawnPopulations[i] == null) continue;
                var spawnPopulation = spawnHandler.AllSpawnPopulations[i];
                var spawnDistribution = spawnHandler.SpawnDistributions[i];
                if (spawnPopulation != null && spawnDistribution != null)
                {
                    int targetCount = spawnHandler.GetTargetCount(spawnPopulation, spawnDistribution);
                    if (configData.populationSettings.ContainsKey(spawnPopulation.name))
                        newPopulationSettings.Add(spawnPopulation.name, configData.populationSettings[spawnPopulation.name]);
                    else newPopulationSettings.Add(spawnPopulation.name, targetCount);
                }
            }
            configData.populationSettings = newPopulationSettings;
            SaveConfig();
        }

        private void SpawnHandlerHelper()
        {
            if (configData.pluginEnabled)
                ApplySpawnHandler();
        }

        private void ApplySpawnHandler()
        {
            foreach (var spawnPopulation in spawnHandler.AllSpawnPopulations)
            {
                if (spawnPopulation == null) continue;
                if (configData.populationSettings.ContainsKey(spawnPopulation.name))
                {
                    spawnPopulation.ScaleWithSpawnFilter = false;
                    spawnPopulation.ScaleWithServerPopulation = false;
                    spawnPopulation.EnforcePopulationLimits = true;
                    if (spawnPopulation is ConvarControlledSpawnPopulation)
                    {
                        var populationConvar = (spawnPopulation as ConvarControlledSpawnPopulation).PopulationConvar;
                        ConsoleSystem.Command command = ConsoleSystem.Index.Server.Find(populationConvar);
                        command?.Set(configData.populationSettings[spawnPopulation.name] / densityToMaxPopulation);
                    }
                    else spawnPopulation._targetDensity = configData.populationSettings[spawnPopulation.name] / densityToMaxPopulation;
                }
            }
            EnforceLimits();
        }

        #region Enforce Limits

        [ConsoleCommand("er.enforcelimits")]
        private void CmdEnforceLimits(ConsoleSystem.Arg arg)
        {
            if (EnforceLimits()) Print(arg, "Successfully enforced all population limits");
            else Print(arg, "Unsuccessful enforced all population limits");
        }

        private bool EnforceLimits()
        {
            if (spawnHandler.SpawnDistributions == null) return false;
            var spawnables = UnityEngine.Object.FindObjectsOfType<Spawnable>()?.Where(x => x.gameObject.activeInHierarchy && x.Population != null)?.GroupBy(x => x.Population)?.ToDictionary(x => x.Key, y => y.ToArray());
            if (spawnables == null) return false;
            for (int i = 0; i < spawnHandler.AllSpawnPopulations.Length; i++)
            {
                var spawnPopulation = spawnHandler.AllSpawnPopulations[i];
                var spawnDistribution = spawnHandler.SpawnDistributions[i];
                if (spawnPopulation != null && spawnDistribution != null && spawnables.ContainsKey(spawnPopulation))
                    EnforceLimits(spawnPopulation, spawnDistribution, spawnables[spawnPopulation]);
            }
            return true;
        }

        private void EnforceLimits(SpawnPopulation population, SpawnDistribution distribution, Spawnable[] array)
        {
            int targetCount = spawnHandler.GetTargetCount(population, distribution);
            if (array.Length > targetCount)
            {
                Debug.Log(population + " has " + array.Length + " objects, but max allowed is " + targetCount);
                int num = array.Length - targetCount;
                Debug.Log(" - deleting " + num + " objects");
                foreach (Spawnable item in array.Take(num))
                {
                    BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item.gameObject);
                    if (BaseEntityEx.IsValid(baseEntity)) baseEntity.Kill();
                    else GameManager.Destroy(item.gameObject);
                }
            }
        }

        #endregion Enforce Limits

        [ConsoleCommand("er.fillpopulations")]
        private void CmdFillPopulations(ConsoleSystem.Arg arg)
        {
            spawnHandler.FillPopulations();
            Print(arg, "Successfully filled all populations");
        }

        [ConsoleCommand("er.getreport")]
        private void CmdGetReport(ConsoleSystem.Arg arg) => Print(arg, GetReport());

        public string GetReport()
        {
            SpawnPopulation[] AllSpawnPopulations = spawnHandler.AllSpawnPopulations;
            SpawnDistribution[] SpawnDistributions = spawnHandler.SpawnDistributions;
            StringBuilder stringBuilder = new StringBuilder();
            if (AllSpawnPopulations == null) stringBuilder.AppendLine("Spawn population array is null.");
            if (SpawnDistributions == null) stringBuilder.AppendLine("Spawn distribution array is null.");
            if (AllSpawnPopulations != null && SpawnDistributions != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("SpawnPopulationName".PadRight(40) + "MaximumPopulation".PadRight(25) + "CurrentPopulation");
                for (int i = 0; i < AllSpawnPopulations.Length; i++)
                {
                    if (AllSpawnPopulations[i] == null) continue;
                    SpawnPopulation spawnPopulation = AllSpawnPopulations[i];
                    SpawnDistribution spawnDistribution = SpawnDistributions[i];
                    if (spawnPopulation != null && spawnDistribution != null)
                    {
                        int currentCount = spawnHandler.GetCurrentCount(spawnPopulation, spawnDistribution);
                        int targetCount = spawnHandler.GetTargetCount(spawnPopulation, spawnDistribution);
                        stringBuilder.AppendLine(spawnPopulation.name.PadRight(40) + targetCount.ToString().PadRight(25) + currentCount);
                    }
                }
            }
            return stringBuilder.ToString();
        }

        private void Print(ConsoleSystem.Arg arg, string message)
        {
            var player = arg?.Player();
            if (player == null) Puts(message);
            else PrintToConsole(player, message);
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Enabled plugin")]
            public bool pluginEnabled = false;

            [JsonProperty(PropertyName = "Population settings")]
            public Dictionary<string, int> populationSettings = new Dictionary<string, int>();
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