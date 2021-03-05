using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Rust;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Random Respawner", "Egor Blagov/Arainrr", "1.2.3")]
    [Description("Plugin respawns player in random place")]
    internal class RandomRespawner : RustPlugin
    {
        private const string PERMISSION_USE = "randomrespawner.use";
        private const int MASK_BIOME = (int)(TerrainBiome.Enum.Arid | TerrainBiome.Enum.Temperate | TerrainBiome.Enum.Tundra | TerrainBiome.Enum.Arctic);
        private const int MASK_SPLAT = (int)(TerrainSplat.Enum.Dirt | TerrainSplat.Enum.Snow | TerrainSplat.Enum.Sand | TerrainSplat.Enum.Rock | TerrainSplat.Enum.Grass | TerrainSplat.Enum.Forest | TerrainSplat.Enum.Stones | TerrainSplat.Enum.Gravel);

        private Coroutine findSpawnPosCoroutine;
        private readonly List<Vector3> spawnPositionCache = new List<Vector3>();

        private List<Collider> colliders;
        private List<BaseEntity> entities;
        private List<BasePlayer> players;

        #region Oxide Hooks

        private void Init()
        {
            colliders = Pool.GetList<Collider>();
            entities = Pool.GetList<BaseEntity>();
            players = Pool.GetList<BasePlayer>();
            permission.RegisterPermission(PERMISSION_USE, this);
        }

        private void OnServerInitialized()
        {
            findSpawnPosCoroutine = ServerMgr.Instance.StartCoroutine(FindSpawnPositions(5000));
        }

        private void Unload()
        {
            Pool.Free(ref entities);
            Pool.Free(ref colliders);
            Pool.Free(ref players);
            if (findSpawnPosCoroutine != null)
            {
                ServerMgr.Instance.StopCoroutine(findSpawnPosCoroutine);
            }
        }

        private IEnumerator FindSpawnPositions(int attempts = 1500)
        {
            List<Vector3> list = Pool.GetList<Vector3>();
            float mapSizeX = TerrainMeta.Size.x / 2;
            float mapSizeZ = TerrainMeta.Size.z / 2;
            Vector3 randomPos = Vector3.zero;
            for (int i = 0; i < attempts; i++)
            {
                randomPos.x = UnityEngine.Random.Range(-mapSizeX, mapSizeX);
                randomPos.z = UnityEngine.Random.Range(-mapSizeZ, mapSizeZ);
                if (TestPos(ref randomPos))
                {
                    list.Add(randomPos);
                }
                if (i % 20 == 0) yield return CoroutineEx.waitForEndOfFrame;
            }
            spawnPositionCache.AddRange(list);
            PrintWarning($"Successfully found {list.Count} spawn positions.");
            Pool.FreeList(ref list);
            findSpawnPosCoroutine = null;
        }

        //private void OnPlayerDeath(BasePlayer player, HitInfo info) {
        //    PrintError($"OnPlayerDeath: {player?.displayName}: {info?.damageTypes.Total()} - {info?.damageTypes?.GetMajorityDamageType()} - {player?.transform.position}");
        //    System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace();
        //    System.Diagnostics.StackFrame[] stackFrames = stackTrace.GetFrames();
        //    foreach (var sf in stackFrames)
        //    {
        //        PrintError($"{ sf.GetMethod().ReflectedType?.Name} - {sf.GetMethod().Name} - {sf.GetFileName()}");
        //    }
        //}

        private object OnPlayerRespawn(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return null;
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
            for (int i = 0; i < configData.maxAttempts; i++)
            {
                if (spawnPositionCache.Count <= 0) return null;
                var spawnPos = spawnPositionCache.GetRandom();
                if (TestPosAgain(spawnPos))
                {
                    return spawnPos;
                }
                spawnPositionCache.Remove(spawnPos);
                if (spawnPositionCache.Count < 50 && findSpawnPosCoroutine == null)
                {
                    findSpawnPosCoroutine = ServerMgr.Instance.StartCoroutine(FindSpawnPositions());
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
            if (AntiHack.TestInsideTerrain(randomPos))
            {
                return false;
            }
            if (!ValidBounds.Test(randomPos))
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

            colliders.Clear();
            Vis.Colliders(spawnPos, 3f, colliders);
            foreach (var collider in colliders)
            {
                switch (collider.gameObject.layer)
                {
                    case (int)Layer.Prevent_Building:
                        if (configData.preventSpawnAtMonument)
                        {
                            return false;
                        }
                        break;

                    case (int)Layer.Vehicle_Large: //cargoshiptest
                    case (int)Layer.Vehicle_World:
                    case (int)Layer.Vehicle_Detailed:
                        return false;
                }

                if (configData.preventSpawnAtZone && collider.name.Contains("zonemanager", CompareOptions.IgnoreCase))
                {
                    return false;
                }

                if (configData.preventSpawnAtRadZone && collider.name.Contains("radiation", CompareOptions.IgnoreCase))
                {
                    return false;
                }

                if (collider.name.Contains("fireball", CompareOptions.IgnoreCase) ||
                    collider.name.Contains("iceberg", CompareOptions.IgnoreCase) ||
                    collider.name.Contains("ice_sheet", CompareOptions.IgnoreCase))
                {
                    return false;
                }
            }

            if (configData.radiusFromPlayers > 0)
            {
                players.Clear();
                Vis.Entities(spawnPos, configData.radiusFromPlayers, players, Layers.Mask.Player_Server);
                foreach (var player in players)
                {
                    if (!player.IsSleeping())
                    {
                        return false;
                    }
                }
            }
            if (configData.radiusFromBuilding > 0)
            {
                entities.Clear();
                Vis.Entities(spawnPos, configData.radiusFromBuilding, entities, Layers.PlayerBuildings);
                if (entities.Any()) return false;
            }

            return true;
        }

        #region Helpers

        private static TerrainBiome.Enum GetPosBiome(Vector3 position) => (TerrainBiome.Enum)TerrainMeta.BiomeMap.GetBiomeMaxType(position, MASK_BIOME);

        private static TerrainSplat.Enum GetPosSplat(Vector3 position) => (TerrainSplat.Enum)TerrainMeta.SplatMap.GetSplatMaxType(position, MASK_SPLAT);

        private static float GetPosSlope(Vector3 position) => TerrainMeta.HeightMap.GetSlope(position);

        #endregion Helpers

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Maximum Attempts To Find A Respawn Position")]
            public int maxAttempts = 200;

            [JsonProperty(PropertyName = "Minimum Distance From Other Players (Including NPC Players)")]
            public float radiusFromPlayers = 20.0f;

            [JsonProperty(PropertyName = "Minimum Distance From Building")]
            public float radiusFromBuilding = 20.0f;

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