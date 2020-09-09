using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Random Respawner", "Egor Blagov/Arainrr", "1.2.1")]
    [Description("Plugin respawns player in random place")]
    internal class RandomRespawner : RustPlugin
    {
        private const int MaxTrials = 200;
        private const string PERMISSION_USE = "randomrespawner.use";
        private const int MASK_BIOME = (int)(TerrainBiome.Enum.Arid | TerrainBiome.Enum.Temperate | TerrainBiome.Enum.Tundra | TerrainBiome.Enum.Arctic);
        private const int MASK_SPLAT = (int)(TerrainSplat.Enum.Dirt | TerrainSplat.Enum.Snow | TerrainSplat.Enum.Sand | TerrainSplat.Enum.Rock | TerrainSplat.Enum.Grass | TerrainSplat.Enum.Forest | TerrainSplat.Enum.Stones | TerrainSplat.Enum.Gravel);

        private Coroutine findSpawnPosCoroutine;
        private readonly List<Vector3> spawnPositionCache = new List<Vector3>();

        #region Oxide Hooks

        private void Init()
        {
            permission.RegisterPermission(PERMISSION_USE, this);
        }

        private void OnServerInitialized()
        {
            findSpawnPosCoroutine = ServerMgr.Instance.StartCoroutine(FindSpawnPositions());
        }

        private void Unload()
        {
            if (findSpawnPosCoroutine != null) ServerMgr.Instance.StopCoroutine(findSpawnPosCoroutine);
        }

        private IEnumerator FindSpawnPositions()
        {
            float mapSizeX = TerrainMeta.Size.x / 2;
            float mapSizeZ = TerrainMeta.Size.z / 2;
            Vector3 randomPos = Vector3.zero;
            for (int i = 0; i < 3000; i++)
            {
                randomPos.x = UnityEngine.Random.Range(-mapSizeX, mapSizeX);
                randomPos.z = UnityEngine.Random.Range(-mapSizeZ, mapSizeZ);
                if (TestPos(ref randomPos))
                {
                    spawnPositionCache.Add(randomPos);
                }

                if (i % 20 == 0) yield return CoroutineEx.waitForFixedUpdate;
            }
            PrintWarning($"Successfully found {spawnPositionCache.Count} spawn positions.");
            findSpawnPosCoroutine = null;
            yield break;
        }

        private object OnPlayerRespawn(BasePlayer player)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                return null;
            }

            var spawnPos = GetRandomSpawnPos();
            if (!spawnPos.HasValue)
            {
                PrintWarning("Unable to generate random respawn position, try limit exceed, spawn as default");
                return null;
            }

            if (Interface.CallHook("OnRandomRespawn", player, spawnPos.Value) != null)
            {
                return null;
            }

            return new BasePlayer.SpawnPoint
            {
                pos = spawnPos.Value,
            };
        }

        #endregion Oxide Hooks

        private Vector3? GetRandomSpawnPos()
        {
            Vector3 spawnPos;
            for (int i = 0; i < MaxTrials; i++)
            {
                spawnPos = spawnPositionCache.GetRandom();
                if (TestPosAgain(spawnPos))
                {
                    return spawnPos;
                }
            }

            return null;
        }

        private bool TestPos(ref Vector3 randomPos)
        {
            RaycastHit hitInfo;
            if (!Physics.Raycast(randomPos + Vector3.up * 300f, Vector3.down, out hitInfo, 400f, Layers.Solid) ||
                hitInfo.GetEntity() != null)
            {
                return false;
            }

            randomPos.y = hitInfo.point.y;
            if (ConVar.AntiHack.terrain_kill && AntiHack.TestInsideTerrain(randomPos))
            {
                return false;
            }

            var slope = GetPosSlope(randomPos);
            if (slope < configData.minSlope || slope > configData.maxSlope)
            {
                return false;
            }

            bool flag;
            var biome = GetPosBiome(randomPos);
            if (configData.biomes.TryGetValue(biome, out flag) && !flag)
            {
                return false;
            }

            var splat = GetPosSplat(randomPos);
            if (configData.splats.TryGetValue(splat, out flag) && !flag)
            {
                return false;
            }
            return TestPosAgain(randomPos);
        }

        private bool TestPosAgain(Vector3 spawnPos)
        {
            if (WaterLevel.Test(spawnPos))
            {
                return false;
            }

            if (!ValidBounds.Test(spawnPos))
            {
                return false;
            }

            var colliders = Facepunch.Pool.GetList<Collider>();
            Vis.Colliders(spawnPos, 3f, colliders);
            foreach (var collider in colliders)
            {
                switch (collider.gameObject.layer)
                {
                    case (int)Layer.Prevent_Building:
                        if (configData.preventSpawnAtMonument)
                        {
                            Facepunch.Pool.FreeList(ref colliders);
                            return false;
                        }

                        break;

                    //case (int)Layer.Water:
                    case (int)Layer.Vehicle_Large: //cargoshiptest
                    case (int)Layer.Vehicle_World:
                    case (int)Layer.Vehicle_Detailed:
                        Facepunch.Pool.FreeList(ref colliders);
                        return false;
                }

                if (configData.preventSpawnAtZone && collider.name.Contains("zonemanager", CompareOptions.IgnoreCase))
                {
                    Facepunch.Pool.FreeList(ref colliders);
                    return false;
                }

                if (configData.preventSpawnAtRadZone && collider.name.Contains("radiation", CompareOptions.IgnoreCase))
                {
                    Facepunch.Pool.FreeList(ref colliders);
                    return false;
                }

                if (collider.name.Contains("fireball", CompareOptions.IgnoreCase) ||
                    collider.name.Contains("iceberg", CompareOptions.IgnoreCase) ||
                    collider.name.Contains("ice_sheet", CompareOptions.IgnoreCase))
                {
                    Facepunch.Pool.FreeList(ref colliders);
                    return false;
                }
            }

            Facepunch.Pool.FreeList(ref colliders);
            bool flag;
            if (configData.radiusFromPlayers > 0)
            {
                var players = Facepunch.Pool.GetList<BasePlayer>();
                Vis.Entities(spawnPos, configData.radiusFromPlayers, players);
                flag = players.Count(x => !x.IsSleeping()) > 0;
                Facepunch.Pool.FreeList(ref players);
                if (flag) return false;
            }

            var entities = Facepunch.Pool.GetList<BaseEntity>();
            Vis.Entities(spawnPos, 20f, entities, Layers.PlayerBuildings);
            flag = entities.Count > 0;
            Facepunch.Pool.FreeList(ref entities);
            if (flag) return false;

            return true;
        }

        #region Methods

        private static TerrainBiome.Enum GetPosBiome(Vector3 position) => (TerrainBiome.Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position, MASK_BIOME);

        private static TerrainSplat.Enum GetPosSplat(Vector3 position) => (TerrainSplat.Enum)TerrainMeta.SplatMap.GetSplatMaxType(position, MASK_SPLAT);

        private static float GetPosSlope(Vector3 position) => TerrainMeta.HeightMap.GetSlope(position);

        #endregion Methods

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Min Distance From Other Playes On Respawn (Including NPC Players)")]
            public float radiusFromPlayers = 20.0f;

            [JsonProperty(PropertyName = "Prevent Players To Be Respawn At Monuments")]
            public bool preventSpawnAtMonument = true;

            [JsonProperty(PropertyName = "Prevent Players To Be Respawn At ZoneManager")]
            public bool preventSpawnAtZone = true;

            [JsonProperty(PropertyName = "Prevent Players To Be Respawn At RadiationZone")]
            public bool preventSpawnAtRadZone = true;

            [JsonProperty(PropertyName = "Minimum Slope")]
            public float minSlope = 0f;

            [JsonProperty(PropertyName = "Maximum Slope")]
            public float maxSlope = 60f;

            [JsonProperty(PropertyName = "Biome Settings")]
            public Dictionary<TerrainBiome.Enum, bool> biomes = new Dictionary<TerrainBiome.Enum, bool>
            {
                [TerrainBiome.Enum.Arid] = true,
                [TerrainBiome.Enum.Temperate] = true,
                [TerrainBiome.Enum.Tundra] = true,
                [TerrainBiome.Enum.Arctic] = true,
            };

            [JsonProperty(PropertyName = "Splat Settings")]
            public Dictionary<TerrainSplat.Enum, bool> splats = new Dictionary<TerrainSplat.Enum, bool>
            {
                [TerrainSplat.Enum.Dirt] = true,
                [TerrainSplat.Enum.Snow] = true,
                [TerrainSplat.Enum.Sand] = true,
                [TerrainSplat.Enum.Rock] = true,
                [TerrainSplat.Enum.Grass] = true,
                [TerrainSplat.Enum.Forest] = true,
                [TerrainSplat.Enum.Stones] = true,
                [TerrainSplat.Enum.Gravel] = true,
            };
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