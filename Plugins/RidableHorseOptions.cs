using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Ridable Horse Options", "Arainrr", "1.1.3")]
    [Description("Controls all rideable horses on your server")]
    public class RidableHorseOptions : RustPlugin
    {
        private const string PREFAB_RIDABLE_HORSE = "assets/rust.ai/nextai/testridablehorse.prefab";

        private void Init() => Unsubscribe(nameof(OnEntitySpawned));

        private void OnServerInitialized()
        {
            UpdateConfig();
            Subscribe(nameof(OnEntitySpawned));
            foreach (var ridableHorse in BaseNetworkable.serverEntities.OfType<RidableHorse>())
            {
                OnEntitySpawned(ridableHorse);
            }
        }

        private void OnEntitySpawned(RidableHorse ridableHorse)
        {
            if (ridableHorse == null) return;
            ApplyHorseSettings(ridableHorse);
        }

        private void UpdateConfig()
        {
            var ridableHorse = GameManager.server.FindPrefab(PREFAB_RIDABLE_HORSE)?.GetComponent<RidableHorse>();
            if (ridableHorse?.breeds == null) return;

            for (var i = 0; i < ridableHorse.breeds.Length; i++)
            {
                var horseBreed = ridableHorse.breeds[i];
                ridableHorse.SetBreed(i);
                if (configData.horseSettings.ContainsKey(horseBreed.breedName.english))
                {
                    continue;
                }
                configData.horseSettings.Add(horseBreed.breedName.english, new HorseSettings
                {
                    maxHealth = ridableHorse.MaxHealth() * horseBreed.maxHealth,
                    maxSpeed = ridableHorse.maxSpeed * horseBreed.maxSpeed,
                    walkSpeed = ridableHorse.walkSpeed,
                    trotSpeed = ridableHorse.trotSpeed,
                    runSpeed = ridableHorse.runSpeed,
                    turnSpeed = ridableHorse.turnSpeed,
                    roadSpeedBonus = ridableHorse.roadSpeedBonus,

                    maxStaminaSeconds = ridableHorse.maxStaminaSeconds,
                    staminaCoreSpeedBonus = ridableHorse.staminaCoreSpeedBonus,
                    staminaReplenishRatioMoving = ridableHorse.staminaReplenishRatioMoving,
                    staminaReplenishRatioStanding = ridableHorse.staminaReplenishRatioStanding,
                    staminaCoreLossRatio = ridableHorse.staminaCoreLossRatio,

                    maxWaterDepth = ridableHorse.maxWaterDepth,
                    maxWallClimbSlope = ridableHorse.maxWallClimbSlope,
                    maxStepHeight = ridableHorse.maxStepHeight,
                    maxStepDownHeight = ridableHorse.maxStepDownHeight,
                    maxStaminaCoreFromWater = ridableHorse.maxStaminaCoreFromWater,

                    caloriesToDigestPerHour = ridableHorse.CaloriesToDigestPerHour,
                    dungProducedPerCalorie = ridableHorse.DungProducedPerCalorie,
                    obstacleDetectionRadius = ridableHorse.obstacleDetectionRadius,
                    calorieToStaminaRatio = ridableHorse.calorieToStaminaRatio,
                    hydrationToStaminaRatio = ridableHorse.hydrationToStaminaRatio,
                });
            }

            SaveConfig();
        }

        private void ApplyHorseSettings(RidableHorse ridableHorse)
        {
            var horseBreed = ridableHorse.GetBreed();
            if (horseBreed == null) return;
            HorseSettings horseSettings;
            if (!configData.horseSettings.TryGetValue(horseBreed.breedName.english, out horseSettings))
            {
                return;
            }
            ridableHorse.InitializeHealth(horseSettings.maxHealth, horseSettings.maxHealth);
            ridableHorse.maxSpeed = horseSettings.maxSpeed;
            ridableHorse.walkSpeed = horseSettings.walkSpeed;
            ridableHorse.trotSpeed = horseSettings.trotSpeed;
            ridableHorse.runSpeed = horseSettings.runSpeed;
            ridableHorse.turnSpeed = horseSettings.turnSpeed;
            ridableHorse.roadSpeedBonus = horseSettings.roadSpeedBonus;

            ridableHorse.maxStaminaSeconds = horseSettings.maxStaminaSeconds;
            ridableHorse.staminaCoreSpeedBonus = horseSettings.staminaCoreSpeedBonus;
            ridableHorse.staminaReplenishRatioMoving = horseSettings.staminaReplenishRatioMoving;
            ridableHorse.staminaReplenishRatioStanding = horseSettings.staminaReplenishRatioStanding;
            ridableHorse.staminaCoreLossRatio = horseSettings.staminaCoreLossRatio;

            ridableHorse.maxWaterDepth = horseSettings.maxWaterDepth;
            ridableHorse.maxWallClimbSlope = horseSettings.maxWallClimbSlope;
            ridableHorse.maxStepHeight = horseSettings.maxStepHeight;
            ridableHorse.maxStepDownHeight = horseSettings.maxStepDownHeight;
            ridableHorse.maxStaminaCoreFromWater = horseSettings.maxStaminaCoreFromWater;

            ridableHorse.CaloriesToDigestPerHour = horseSettings.caloriesToDigestPerHour;
            ridableHorse.DungProducedPerCalorie = horseSettings.dungProducedPerCalorie;
            ridableHorse.obstacleDetectionRadius = horseSettings.obstacleDetectionRadius;
            ridableHorse.calorieToStaminaRatio = horseSettings.calorieToStaminaRatio;
            ridableHorse.hydrationToStaminaRatio = horseSettings.hydrationToStaminaRatio;

            ridableHorse.SendNetworkUpdate();
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Horse Settings")]
            public Dictionary<string, HorseSettings> horseSettings = new Dictionary<string, HorseSettings>();
        }

        private class HorseSettings
        {
            public float maxHealth;
            public float maxSpeed;
            public float walkSpeed;
            public float trotSpeed;
            public float runSpeed;
            public float turnSpeed;
            public float roadSpeedBonus;

            public float maxStaminaSeconds;
            public float staminaCoreSpeedBonus;
            public float staminaReplenishRatioMoving;
            public float staminaReplenishRatioStanding;
            public float staminaCoreLossRatio;

            public float maxWaterDepth;
            public float maxWallClimbSlope;
            public float maxStepHeight;
            public float maxStepDownHeight;
            public float maxStaminaCoreFromWater;

            public float caloriesToDigestPerHour;
            public float dungProducedPerCalorie;
            public float obstacleDetectionRadius;
            public float calorieToStaminaRatio;
            public float hydrationToStaminaRatio;
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