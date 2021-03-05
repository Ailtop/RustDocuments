using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Entity Reducer", "Arainrr", "2.0.1")]
    [Description("Control all spawn populations on your server")]
    public class EntityReducer : RustPlugin
    {
        private static SpawnHandler spawnHandler;

        private void OnServerInitialized()
        {
            spawnHandler = SingletonComponent<SpawnHandler>.Instance;
            if (spawnHandler == null || spawnHandler.AllSpawnPopulations == null)
            {
                PrintError("The SpawnHandler is missing on your server, the plugin cannot be used");
                Interface.Oxide.UnloadPlugin(Name);
                return;
            }
            UpdateConfig();
            if (configData.pluginEnabled)
            {
                ApplySpawnHandler();
            }
        }

        private void Unload()
        {
            spawnHandler = null;
        }

        private void UpdateConfig()
        {
            Dictionary<string, int> newPopulationSettings = new Dictionary<string, int>();
            for (int i = 0; i < spawnHandler.AllSpawnPopulations.Length; i++)
            {
                var spawnPopulation = spawnHandler.AllSpawnPopulations[i];
                if (spawnPopulation == null) continue;
                var spawnDistribution = spawnHandler.SpawnDistributions[i];
                if (spawnDistribution == null) continue;
                int targetCount = spawnHandler.GetTargetCount(spawnPopulation, spawnDistribution);
                int value;
                newPopulationSettings.Add(spawnPopulation.name,
                    configData.populationSettings.TryGetValue(spawnPopulation.name, out value)
                        ? value
                        : targetCount);
            }
            configData.populationSettings = newPopulationSettings;
            SaveConfig();
        }

        private void ApplySpawnHandler()
        {
            foreach (var spawnPopulation in spawnHandler.AllSpawnPopulations)
            {
                if (spawnPopulation == null) continue;
                int targetCount;
                if (configData.populationSettings.TryGetValue(spawnPopulation.name, out targetCount))
                {
                    float num = TerrainMeta.Size.x * TerrainMeta.Size.z;
                    if (!spawnPopulation.ScaleWithLargeMaps)
                    {
                        num = Mathf.Min(num, 1.6E+07f);
                    }
                    var densityToMaxPopulation = num * 1E-06f * Spawn.max_density;
                    spawnPopulation.ScaleWithSpawnFilter = false;
                    spawnPopulation.ScaleWithServerPopulation = false;
                    spawnPopulation.EnforcePopulationLimits = true;
                    spawnPopulation.ScaleWithLargeMaps = true;
                    var convarControlledSpawnPopulation = spawnPopulation as ConvarControlledSpawnPopulation;
                    if (convarControlledSpawnPopulation != null)
                    {
                        ConsoleSystem.Command command = ConsoleSystem.Index.Server.Find(convarControlledSpawnPopulation.PopulationConvar);
                        command?.Set(targetCount / densityToMaxPopulation);
                    }
                    else spawnPopulation._targetDensity = targetCount / densityToMaxPopulation;
                }
            }
            EnforceLimits();
        }

        #region Enforce Limits

        [ConsoleCommand("er.enforcelimits")]
        private void CmdEnforceLimits(ConsoleSystem.Arg arg)
        {
            SendReply(arg,
                EnforceLimits()
                    ? "Successfully enforced all population limits"
                    : "Unsuccessful enforced all population limits");
        }

        private static bool EnforceLimits()
        {
            if (spawnHandler.SpawnDistributions == null) return false;
            var spawnables = UnityEngine.Object.FindObjectsOfType<Spawnable>()?.Where(x => x.gameObject.activeInHierarchy && x.Population != null)?.GroupBy(x => x.Population)?.ToDictionary(x => x.Key, y => y.ToArray());
            if (spawnables == null) return false;
            for (int i = 0; i < spawnHandler.AllSpawnPopulations.Length; i++)
            {
                var spawnPopulation = spawnHandler.AllSpawnPopulations[i];
                var spawnDistribution = spawnHandler.SpawnDistributions[i];
                Spawnable[] array;
                if (spawnPopulation != null && spawnDistribution != null && spawnables.TryGetValue(spawnPopulation, out array))
                {
                    EnforceLimits(spawnPopulation, spawnDistribution, array);
                }
            }
            return true;
        }

        private static void EnforceLimits(SpawnPopulation population, SpawnDistribution distribution, Spawnable[] array)
        {
            int targetCount = spawnHandler.GetTargetCount(population, distribution);
            if (array.Length > targetCount)
            {
                Debug.Log(population + " has " + array.Length + " objects, but max allowed is " + targetCount);
                int num = array.Length - targetCount;
                Debug.Log(" - deleting " + num + " objects");
                foreach (Spawnable item in array.Take(num).ToArray())
                {
                    BaseEntity baseEntity = item.gameObject.ToBaseEntity();
                    if (baseEntity.IsValid()) baseEntity.Kill();
                    else GameManager.Destroy(item.gameObject);
                }
            }
        }

        #endregion Enforce Limits

        [ConsoleCommand("er.fillpopulations")]
        private void CmdFillPopulations(ConsoleSystem.Arg arg)
        {
            spawnHandler.FillPopulations();
            SendReply(arg, "Successfully filled all populations");
        }

        [ConsoleCommand("er.getreport")]
        private void CmdGetReport(ConsoleSystem.Arg arg) => SendReply(arg, GetReport());

        public string GetReport()
        {
            SpawnPopulation[] allSpawnPopulations = spawnHandler.AllSpawnPopulations;
            SpawnDistribution[] spawnDistributions = spawnHandler.SpawnDistributions;
            StringBuilder stringBuilder = new StringBuilder();
            if (allSpawnPopulations == null) stringBuilder.AppendLine("Spawn population array is null.");
            if (spawnDistributions == null) stringBuilder.AppendLine("Spawn distribution array is null.");
            if (allSpawnPopulations != null && spawnDistributions != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("SpawnPopulationName".PadRight(40) + "MaximumPopulation".PadRight(25) + "CurrentPopulation");
                for (int i = 0; i < allSpawnPopulations.Length; i++)
                {
                    var spawnPopulation = allSpawnPopulations[i];
                    if (spawnPopulation == null) continue;
                    var spawnDistribution = spawnDistributions[i];
                    if (spawnDistribution == null) continue;
                    int currentCount = spawnHandler.GetCurrentCount(spawnPopulation, spawnDistribution);
                    int targetCount = spawnHandler.GetTargetCount(spawnPopulation, spawnDistribution);
                    stringBuilder.AppendLine(spawnPopulation.name.PadRight(40) + targetCount.ToString().PadRight(25) + currentCount);
                }
            }
            return stringBuilder.ToString();
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