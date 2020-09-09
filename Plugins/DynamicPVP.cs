//Requires: ZoneManager
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Dynamic PVP", "CatMeat/Arainrr", "4.1.10", ResourceId = 2728)]
    [Description("Creates temporary PvP zones on certain actions/events")]
    public class DynamicPVP : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin ZoneManager, TruePVE, NextGenPVE, BotSpawn;
        private const string PERMISSION_ADMIN = "dynamicpvp.admin";
        private const string PREFAB_SPHERE = "assets/prefabs/visualization/sphere.prefab";

        private readonly Dictionary<ulong, LeftZone> pvpDelays = new Dictionary<ulong, LeftZone>();
        private readonly Dictionary<string, string> activeDynamicZones = new Dictionary<string, string>();

        private bool dataChanged;
        private Vector3 oilRigPosition;
        private Vector3 largeOilRigPosition;
        private Coroutine monumentEventCoroutine;

        private class LeftZone
        {
            public string zoneID;
            public Timer zoneTimer;
        }

        private enum GeneralEventType
        {
            Bradley,
            Helicopter,
            SupplyDrop,
            SupplySignal,
            HackableCrate,
            ExcavatorIgnition,
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            permission.RegisterPermission(PERMISSION_ADMIN, this);
            AddCovalenceCommand(configData.chatS.command, nameof(CmdDynamicPVP));
            Unsubscribe(nameof(OnEntitySpawned));
            if (configData.generalEvents.supplySignal.enabled)
            {
                Unsubscribe(nameof(OnExplosiveThrown));
                Unsubscribe(nameof(OnExplosiveDropped));
            }
            if (configData.generalEvents.hackableCrate.spawnStart)
            {
                Unsubscribe(nameof(OnCrateHack));
            }
            if (!configData.generalEvents.excavatorIgnition.enabled)
            {
                Unsubscribe(nameof(OnDieselEngineToggled));
            }
            if (!configData.generalEvents.patrolHelicopter.enabled && !configData.generalEvents.bradleyAPC.enabled)
            {
                Unsubscribe(nameof(OnEntityDeath));
            }
        }

        private void OnServerInitialized()
        {
            DeleteOldDynamicZone();
            if (configData.generalEvents.supplyDrop.enabled || configData.generalEvents.supplySignal.enabled ||
                configData.generalEvents.hackableCrate.enabled && configData.generalEvents.hackableCrate.spawnStart)
            {
                Subscribe(nameof(OnEntitySpawned));
            }
            monumentEventCoroutine = ServerMgr.Instance.StartCoroutine(CreateMonumentEvents());
        }

        private void Unload()
        {
            if (monumentEventCoroutine != null)
            {
                ServerMgr.Instance.StopCoroutine(monumentEventCoroutine);
            }

            List<string> zoneIDs = new List<string>(activeDynamicZones.Keys);
            if (zoneIDs.Count > 0)
            {
                DebugPrint($"Deleting {zoneIDs.Count} ActiveZones", false);
                foreach (string zoneID in zoneIDs)
                {
                    DeleteDynamicZone(zoneID);
                }
            }
            foreach (var value in pvpDelays.Values)
            {
                value.zoneTimer?.Destroy();
            }
            foreach (var sphereEntities in zoneSpheres.Values)
            {
                foreach (var sphereEntity in sphereEntities)
                {
                    if (sphereEntity != null && !sphereEntity.IsDestroyed)
                        sphereEntity.KillMessage();
                }
            }
            if (configData.global.logToFile)
            {
                SaveDebug();
            }
            SaveData();
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), () =>
        {
            if (configData.global.logToFile)
            {
                SaveDebug();
            }
            if (dataChanged)
            {
                SaveData();
                dataChanged = false;
            }
        });

        private void OnEntitySpawned(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null) return;
            if (baseCombatEntity is HackableLockedCrate)
            {
                if (configData.generalEvents.hackableCrate.spawnStart)
                {
                    DebugPrint($"Try to start the event when the hackable crate spawning.", false);
                    LockedCrateEvent(baseCombatEntity);
                }
                return;
            }
            if (baseCombatEntity is SupplyDrop)
            {
                SupplyDropEvent(baseCombatEntity);
            }
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            TryRemovePVPDelay(player.userID);
        }

        #endregion Oxide Hooks

        #region Methods

        private BaseEventS GetBaseEventS(string eventName)
        {
            AutoEventS autoEventS;
            if (storedData.autoEvents.TryGetValue(eventName, out autoEventS))
                return autoEventS;
            TimedEventS timedEventS;
            if (storedData.timedEvents.TryGetValue(eventName, out timedEventS))
                return timedEventS;
            MonumentEventS monumentEventS;
            if (configData.monumentEvents.TryGetValue(eventName, out monumentEventS))
                return monumentEventS;
            GeneralEventType generalEventType;
            if (Enum.TryParse(eventName, true, out generalEventType))
            {
                switch (generalEventType)
                {
                    case GeneralEventType.Bradley: return configData.generalEvents.bradleyAPC;
                    case GeneralEventType.HackableCrate: return configData.generalEvents.hackableCrate;
                    case GeneralEventType.Helicopter: return configData.generalEvents.patrolHelicopter;
                    case GeneralEventType.SupplyDrop: return configData.generalEvents.supplyDrop;
                    case GeneralEventType.SupplySignal: return configData.generalEvents.supplySignal;
                    case GeneralEventType.ExcavatorIgnition: return configData.generalEvents.excavatorIgnition;
                }
            }
            return null;
        }

        private IEnumerator CreateMonumentEvents()
        {
            bool changed = false;
            List<string> monumentNames = new List<string>();
            foreach (var monumentInfo in TerrainMeta.Path.Monuments)
            {
                if (!monumentInfo.shouldDisplayOnMap) continue;
                var monumentName = monumentInfo.displayPhrase.english.Replace("\n", "");
                if (string.IsNullOrEmpty(monumentName)) continue;
                switch (monumentInfo.name)
                {
                    case "OilrigAI": oilRigPosition = monumentInfo.transform.position; break;
                    case "OilrigAI2": largeOilRigPosition = monumentInfo.transform.position; break;
                    case "assets/bundled/prefabs/autospawn/monument/harbor/harbor_1.prefab": monumentName += " A"; break;
                    case "assets/bundled/prefabs/autospawn/monument/harbor/harbor_2.prefab": monumentName += " B"; break;
                }

                MonumentEventS monumentEventS;
                if (!configData.monumentEvents.TryGetValue(monumentName, out monumentEventS))
                {
                    changed = true;
                    monumentEventS = new MonumentEventS();
                    configData.monumentEvents.Add(monumentName, monumentEventS);
                }
                if (!monumentEventS.enabled) continue;
                monumentNames.Add(monumentName);
                CreateDynamicZoneHandler(monumentName, monumentInfo.transform);
                yield return CoroutineEx.waitForSeconds(0.5f);
            }
            if (monumentNames.Count > 0)
                DebugPrint($"Successfully created {monumentNames.Count}({string.Join(", ", monumentNames)}) monument PVP events.", false);
            if (changed) SaveConfig();

            List<string> autoEvents = new List<string>();
            foreach (var entry in storedData.autoEvents)
            {
                if (entry.Value.autoStart)
                {
                    CreateDynamicZone(entry.Key);
                    autoEvents.Add(entry.Key);
                    yield return CoroutineEx.waitForSeconds(0.5f);
                }
            }
            if (autoEvents.Count > 0)
                DebugPrint($"Successfully created {autoEvents.Count}({string.Join(", ", autoEvents)}) auto PVP events.", false);

            monumentEventCoroutine = null;
            yield break;
        }

        private void TryRemovePVPDelay(ulong playerID)
        {
            LeftZone leftZone;
            if (pvpDelays.TryGetValue(playerID, out leftZone))
            {
                leftZone.zoneTimer?.Destroy();
                pvpDelays.Remove(playerID);
            }
        }

        #endregion Methods

        #region Events

        #region ExcavatorIgnition Event

        private string excavatorIgnitionEventZoneID = string.Empty;

        private void OnDieselEngineToggled(DieselEngine dieselEngine)
        {
            bool currentState = dieselEngine.HasFlag(BaseEntity.Flags.On);
            if (currentState) CreateDynamicZoneHandler(GeneralEventType.ExcavatorIgnition.ToString(), dieselEngine.transform);
            else DeleteDynamicZone(excavatorIgnitionEventZoneID);
        }

        #endregion ExcavatorIgnition Event

        #region HackableLockedCrate Event

        private void OnCrateHack(HackableLockedCrate hackableLockedCrate)
        {
            if (hackableLockedCrate == null) return;
            LockedCrateEvent(hackableLockedCrate);
            DebugPrint($"Try to start the event when hackable crate unlocks.", false);
        }

        private void LockedCrateEvent(BaseCombatEntity hackableLockedCrate)
        {
            if (!configData.generalEvents.hackableCrate.enabled) return;
            if (configData.global.checkEntityOwner && hackableLockedCrate.OwnerID.IsSteamId()) return;
            if (configData.generalEvents.hackableCrate.excludeOilRig && OnTheOilRig(hackableLockedCrate))
            {
                DebugPrint($"Hackable crate on oilrig, don't start event.", false);
                return;
            }
            if (configData.generalEvents.hackableCrate.excludeCargoShip && hackableLockedCrate.GetComponentInParent<CargoShip>() != null)
            {
                DebugPrint($"Hackable crate on cargo ship, don't start event.", false);
                return;
            }
            CreateDynamicZoneHandler(GeneralEventType.HackableCrate.ToString(), hackableLockedCrate.transform);
        }

        private bool OnTheOilRig(BaseCombatEntity hackableLockedCrate)
        {
            if (oilRigPosition != default(Vector3) && Vector3Ex.Distance2D(hackableLockedCrate.transform.position, oilRigPosition) < 50f) return true;
            if (largeOilRigPosition != default(Vector3) && Vector3Ex.Distance2D(hackableLockedCrate.transform.position, largeOilRigPosition) < 50f) return true;
            return false;
        }

        #endregion HackableLockedCrate Event

        #region BaseHelicopter And BradleyAPC Event

        private void OnEntityDeath(BaseCombatEntity baseCombatEntity, HitInfo info)
        {
            if (baseCombatEntity is BaseHelicopter)
                PatrolHelicopterEvent(baseCombatEntity);
            else if (baseCombatEntity is BradleyAPC)
                BradleyApcEvent(baseCombatEntity);
        }

        private void PatrolHelicopterEvent(BaseCombatEntity baseHelicopter)
        {
            if (!configData.generalEvents.patrolHelicopter.enabled) return;
            if (configData.global.checkEntityOwner && baseHelicopter.OwnerID.IsSteamId()) return;
            CreateDynamicZoneHandler(GeneralEventType.Helicopter.ToString(), baseHelicopter.transform);
        }

        private void BradleyApcEvent(BaseCombatEntity bradleyAPC)
        {
            if (!configData.generalEvents.bradleyAPC.enabled) return;
            if (configData.global.checkEntityOwner && bradleyAPC.OwnerID.IsSteamId()) return;
            CreateDynamicZoneHandler(GeneralEventType.Bradley.ToString(), bradleyAPC.transform);
        }

        #endregion BaseHelicopter And BradleyAPC Event

        #region SupplyDrop And SupplySignal Event

        private readonly List<Vector3> activeSupplySignals = new List<Vector3>();

        private void OnExplosiveThrown(BasePlayer player, SupplySignal supplySignal) => SupplySignalThrown(player, supplySignal);

        private void OnExplosiveDropped(BasePlayer player, SupplySignal supplySignal) => SupplySignalThrown(player, supplySignal);

        private void SupplySignalThrown(BasePlayer player, SupplySignal supplySignal)
        {
            timer.Once(3.5f, () =>
            {
                if (supplySignal == null) return;
                Vector3 pos = supplySignal.transform.position;
                activeSupplySignals.Add(pos);
                timer.Once(300f, () => activeSupplySignals.Remove(pos));
                DebugPrint($"SupplySignal thrown at position of {supplySignal.transform.position}", false);
            });
        }

        private void SupplyDropEvent(BaseCombatEntity supplyDrop)
        {
            if (configData.global.checkEntityOwner && supplyDrop.OwnerID.IsSteamId()) return;
            DebugPrint($"Supply drop spawned at {supplyDrop.transform.position}", false);
            bool isFromSupplySignal = IsProbablySupplySignal(supplyDrop.transform.position);
            DebugPrint($"isFromSupplySignal: {isFromSupplySignal}", false);
            if (isFromSupplySignal)
            {
                DebugPrint($"configData.Events.SupplySignal.Enabled: {configData.generalEvents.supplySignal.enabled}", false);
                if (!configData.generalEvents.supplySignal.enabled) DebugPrint($"PVP for Supply Signals disabled: Skipping zone creation", false);
                else CreateDynamicZoneHandler(GeneralEventType.SupplySignal.ToString(), supplyDrop.transform);
            }
            else
            {
                if (!configData.generalEvents.supplyDrop.enabled) return;
                CreateDynamicZoneHandler(GeneralEventType.SupplyDrop.ToString(), supplyDrop.transform);
            }
        }

        private bool IsProbablySupplySignal(Vector3 landingPosition)
        {
            DebugPrint($"Checking {activeSupplySignals.Count} active supply signals", false);
            if (activeSupplySignals.Count > 0)
            {
                for (int i = 0; i < activeSupplySignals.Count; i++)
                {
                    var thrownPosition = activeSupplySignals.ElementAt(i);
                    landingPosition.y = thrownPosition.y;
                    var distance = Vector3.Distance(thrownPosition, landingPosition);
                    DebugPrint($"Found SupplySignal at {thrownPosition} located {distance}m away.", false);
                    if (distance <= configData.global.compareRadius)
                    {
                        activeSupplySignals.Remove(thrownPosition);
                        DebugPrint($"Found matching SupplySignal. Removing Supply signal from active list", false);

                        DebugPrint($"Active supply signals remaining: {activeSupplySignals.Count()}", false);
                        return true;
                    }
                }
                DebugPrint($"No matches found, probably from a timed event cargo_plane", false);
                return false;
            }
            DebugPrint($"No active signals, must be from a timed event cargo_plane", false);
            return false;
        }

        #endregion SupplyDrop And SupplySignal Event

        #endregion Events

        #region DynamicZone Handler

        private void CreateDynamicZoneHandler(string eventName, Transform transform, Vector3 savedLocation = default(Vector3), bool delay = true)
        {
            var baseEventS = GetBaseEventS(eventName);
            if (baseEventS == null) return;
            Vector3 position;
            if (delay && transform != null)
            {
                position = transform.position;
                position.y = TerrainMeta.HeightMap.GetHeight(position);
                var monumentEventS = baseEventS as MonumentEventS;
                if (monumentEventS != null && monumentEventS.transformPosition != Vector3.zero)
                {
                    position = transform.TransformPoint(monumentEventS.transformPosition);
                }
                if (baseEventS.eventStartDelay > 0)
                {
                    timer.Once(baseEventS.eventStartDelay, () => CreateDynamicZoneHandler(eventName, null, position, false));
                    return;
                }
            }
            else position = savedLocation;
            CreateDynamicZone(eventName, position, false);
        }

        private bool CreateDynamicZone(string eventName, Vector3 position = default(Vector3), bool delay = true)
        {
            var baseEventS = GetBaseEventS(eventName);
            if (delay && baseEventS.eventStartDelay > 0)
            {
                timer.Once(baseEventS.eventStartDelay, () => CreateDynamicZone(eventName, position, false));
                return false;
            }
            string zoneID = DateTime.Now.ToString("HHmmssffff");
            var autoEventS = baseEventS as AutoEventS;
            if (autoEventS != null)
            {
                if (!string.IsNullOrEmpty(autoEventS.zoneID))
                {
                    zoneID = autoEventS.zoneID;
                }
                position = autoEventS.position;
            }
            else
            {
                var monumentEventS = baseEventS as MonumentEventS;
                if (monumentEventS != null && !string.IsNullOrEmpty(monumentEventS.zoneID))
                {
                    zoneID = monumentEventS.zoneID;
                }
            }
            if (position == default(Vector3))
            {
                DebugPrint($"Invalid location, zone creation failed", false);
                return false;
            }
            string[] zoneSettings = GetZoneSettings(baseEventS);
            float duration = (baseEventS as TimedEventS)?.duration ?? -1;

            DebugPrint($"Event name: {zoneID} - {eventName} - {position} - {baseEventS.dynamicZoneS.radius}m - {duration}s", false);
            bool zoneAdded = CreateZone(zoneID, zoneSettings, position);
            if (zoneAdded)
            {
                activeDynamicZones.Add(zoneID, eventName);
                if (eventName == GeneralEventType.ExcavatorIgnition.ToString()) excavatorIgnitionEventZoneID = zoneID;

                string successMessage = string.Empty;
                bool mappingAdded = CreateMapping(zoneID, baseEventS.mapping);
                if (!mappingAdded) DebugPrint("ERROR: PVP Mapping failed.", true);
                else successMessage += " Mapping,";

                if (DomeCreateAllowed(baseEventS))
                {
                    bool domeAdded = CreateDome(zoneID, position, baseEventS.dynamicZoneS.radius, baseEventS.domesDarkness);
                    if (!domeAdded) DebugPrint("ERROR: Dome NOT added for Zone: " + zoneID, true);
                    else successMessage += " Dome,";
                }

                var botSpawnEventS = baseEventS as BotSpawnEventS;
                if (BotSpawnAllowed(botSpawnEventS))
                {
                    bool botsSpawned = SpawnBots(position, botSpawnEventS.botProfileName, zoneID);
                    if (botsSpawned) successMessage += " Bots,";
                }
                if (duration > 0) timer.Once(duration, () => DeleteDynamicZone(zoneID));
                DebugPrint($"Created Zone {zoneID} ({successMessage.TrimEnd(',')})", true);
                return true;
            }

            return false;
        }

        private bool DeleteDynamicZone(string zoneID)
        {
            string eventName;
            if (string.IsNullOrEmpty(zoneID) || !activeDynamicZones.TryGetValue(zoneID, out eventName))
            {
                DebugPrint("Invalid ZoneID", false);
                return false;
            }
            string successMessage = string.Empty;
            var baseEventS = GetBaseEventS(eventName);
            if (BotSpawnAllowed(baseEventS as BotSpawnEventS))
            {
                DebugPrint($"Calling RemoveBots, ZoneID {zoneID}", false);
                bool botsRemoved = RemoveBots(zoneID);
                if (botsRemoved) successMessage += " Bots,";
            }
            if (DomeCreateAllowed(baseEventS))
            {
                DebugPrint($"Calling RemoveDome, ZoneID {zoneID}", false);
                bool domeRemoved = RemoveDome(zoneID);
                if (!domeRemoved) DebugPrint("ERROR: Dome NOT removed for Zone: " + zoneID, true);
                else successMessage += " Dome,";
            }
            DebugPrint($"Calling RemoveMapping, ZoneID {zoneID}", false);
            bool mappingRemoved = RemoveMapping(zoneID);
            if (!mappingRemoved) DebugPrint("ERROR: PVP NOT disabled for Zone: " + zoneID, true);
            else successMessage += " Mapping,";
            DebugPrint($"Calling RemoveZone, ZoneID {zoneID}", true);
            bool zoneRemoved = RemoveZone(zoneID);
            if (!zoneRemoved) DebugPrint("ERROR: Zone removal failed.", true);
            else
            {
                DebugPrint($"Deleted Zone {zoneID} ({successMessage.TrimEnd(',')})", false);
                activeDynamicZones.Remove(zoneID);
                return true;
            }
            return false;
        }

        #endregion DynamicZone Handler

        #region ZoneDome Integration

        private readonly Dictionary<string, List<SphereEntity>> zoneSpheres = new Dictionary<string, List<SphereEntity>>();

        private bool DomeCreateAllowed(BaseEventS baseEventS) => baseEventS.domesEnabled;

        private bool CreateDome(string zoneID, Vector3 position, string radiusString, int darkness)
        {
            if (zoneSpheres.ContainsKey(zoneID)) return false;
            float radius;
            if (!float.TryParse(radiusString, out radius)) return false;
            List<SphereEntity> sphereEntities = new List<SphereEntity>();
            for (int i = 0; i < darkness; i++)
            {
                var sphereEntity = GameManager.server.CreateEntity(PREFAB_SPHERE, position) as SphereEntity;
                if (sphereEntity == null) { DebugPrint($"ERROR: sphere entity is null", false); return false; }
                sphereEntity.lerpSpeed = 0f;
                sphereEntity.currentRadius = radius * 2;
                sphereEntity.Spawn();
                sphereEntities.Add(sphereEntity);
            }
            zoneSpheres.Add(zoneID, sphereEntities);
            return true;
        }

        private bool RemoveDome(string zoneID)
        {
            List<SphereEntity> sphereEntities;
            if (!zoneSpheres.TryGetValue(zoneID, out sphereEntities)) return false;
            foreach (var sphereEntity in sphereEntities)
                sphereEntity.lerpSpeed = sphereEntity.currentRadius;
            timer.Once(5f, () =>
            {
                foreach (var sphereEntity in sphereEntities)
                {
                    if (sphereEntity != null && !sphereEntity.IsDestroyed)
                    {
                        sphereEntity.KillMessage();
                    }
                }
                zoneSpheres.Remove(zoneID);
            });
            return true;
        }

        #endregion ZoneDome Integration

        #region TruePVE/NextGenPVE Integration

        private object CanEntityTakeDamage(BasePlayer victim, HitInfo info)
        {
            if (victim == null || info == null) return null;
            var attacker = info.InitiatorPlayer ?? (info.Initiator != null && info.Initiator.OwnerID.IsSteamId() ? BasePlayer.FindByID(info.Initiator.OwnerID) : null);//The attacker cannot be fully captured
            if (attacker == null || !attacker.userID.IsSteamId()) return null;
            LeftZone victimLeftZone;
            if (pvpDelays.TryGetValue(victim.userID, out victimLeftZone))
            {
                if (!string.IsNullOrEmpty(victimLeftZone.zoneID) && IsPlayerInZone(victimLeftZone.zoneID, attacker))//ZonePlayer attack PVPDelayPlayer
                {
                    return true;
                }
                LeftZone attackerLeftZone;
                if (pvpDelays.TryGetValue(attacker.userID, out attackerLeftZone) && victimLeftZone.zoneID == attackerLeftZone.zoneID)//PVPDelayPlayer attack PVPDelayPlayer
                {
                    return true;
                }
            }
            else
            {
                LeftZone attackerLeftZone;
                if (pvpDelays.TryGetValue(attacker.userID, out attackerLeftZone))
                {
                    if (!string.IsNullOrEmpty(attackerLeftZone.zoneID) && IsPlayerInZone(attackerLeftZone.zoneID, victim))//PVPDelayPlayer attack ZonePlayer
                    {
                        return true;
                    }
                }
            }
            return null;
        }

        private bool CreateMapping(string zoneID, string mapping)
        {
            if (TruePVE != null) return (bool)TruePVE.Call("AddOrUpdateMapping", zoneID, mapping);
            if (NextGenPVE != null) return (bool)NextGenPVE.Call("AddOrUpdateMapping", zoneID, mapping);
            return false;
        }

        private bool RemoveMapping(string zoneID)
        {
            if (TruePVE != null) return (bool)TruePVE.Call("RemoveMapping", zoneID);
            if (NextGenPVE != null) return (bool)NextGenPVE.Call("RemoveMapping", zoneID);
            return false;
        }

        #endregion TruePVE/NextGenPVE Integration

        #region BotSpawn Integration

        private bool BotSpawnAllowed(BotSpawnEventS botSpawnEventS)
        {
            if (BotSpawn == null || botSpawnEventS == null || string.IsNullOrEmpty(botSpawnEventS.botProfileName)) return false;
            return botSpawnEventS.botsEnabled;
        }

        private bool SpawnBots(Vector3 zoneLocation, string zoneProfile, string zoneGroupID)
        {
            string[] result = CreateGroupSpawn(zoneLocation, zoneProfile, zoneGroupID);
            if (result == null || result.Length < 2)
            {
                DebugPrint("AddGroupSpawn returned invalid response.", false);
                return false;
            }
            switch (result[0])
            {
                case "true": return true;
                case "false": return false;
                case "error": DebugPrint($"ERROR: AddGroupSpawn failed: {result[1]}", true); return false;
            }
            return false;
        }

        private bool RemoveBots(string zoneGroupID)
        {
            string[] result = RemoveGroupSpawn(zoneGroupID);
            if (result == null || result.Length < 2)
            {
                DebugPrint("RemoveGroupSpawn returned invalid response.", false);
                return false;
            }
            if (result[0] == "error")
            {
                DebugPrint($"ERROR: RemoveGroupSpawn failed: {result[1]}", true);
                return false;
            }
            return true;
        }

        private string[] CreateGroupSpawn(Vector3 location, string profileName, string group) => (string[])BotSpawn?.Call("AddGroupSpawn", location, profileName, group);

        private string[] RemoveGroupSpawn(string group) => (string[])BotSpawn?.Call("RemoveGroupSpawn", group);

        #endregion BotSpawn Integration

        #region ZoneManager Integration

        private void OnEnterZone(string zoneID, BasePlayer player)
        {
            if (!activeDynamicZones.ContainsKey(zoneID)) return;
            if (player == null || !player.userID.IsSteamId()) return;
            TryRemovePVPDelay(player.userID);
            DebugPrint($"{player.displayName} has entered PVP Zone {zoneID}.", true);
        }

        private void OnExitZone(string zoneID, BasePlayer player)
        {
            string eventName;
            if (!activeDynamicZones.TryGetValue(zoneID, out eventName)) return;
            if (player == null || !player.userID.IsSteamId()) return;
            DebugPrint($"{player.displayName} has left a PVP Zone {zoneID}.", true);

            var baseEventS = GetBaseEventS(eventName);
            if (!baseEventS.pvpDelayEnabled) return;
            float delay = baseEventS.pvpDelayTime;
            if (delay <= 0) return;
            DebugPrint($"Adding {player.displayName} to PVPDelay.", true);
            ulong playerID = player.userID;
            string playerName = player.displayName;
            LeftZone leftZone;
            if (pvpDelays.TryGetValue(player.userID, out leftZone))
            {
                leftZone.zoneTimer?.Destroy();
            }
            else
            {
                leftZone = new LeftZone();
                pvpDelays.Add(player.userID, leftZone);
            }

            leftZone.zoneID = zoneID;
            leftZone.zoneTimer = timer.Once(delay, () =>
            {
                DebugPrint($"Remove {playerName} from PVPDelay.", true);
                pvpDelays.Remove(playerID);
            });
        }

        private void DeleteOldDynamicZone()
        {
            int attempts = 0, sucesses = 0;
            var zoneIDs = GetZoneIDs();
            if (zoneIDs == null || zoneIDs.Length <= 0) return;
            foreach (var zoneID in zoneIDs)
            {
                string zoneName = GetZoneName(zoneID);
                if (zoneName == "DynamicPVP")
                {
                    attempts++;
                    bool success = RemoveZone(zoneID);
                    if (success) sucesses++;
                    RemoveMapping(zoneID);
                }
            }
            DebugPrint($"Deleted {sucesses} of {attempts} existing DynamicPVP zones", true);
        }

        private static string[] GetZoneSettings(BaseEventS baseEventS)
        {
            List<string> zoneSettings = new List<string>
            {
                "name", "DynamicPVP",
                "radius",baseEventS.dynamicZoneS.radius,
                "enter_message",  baseEventS.dynamicZoneS.enterMessage ,
                "leave_message",   baseEventS.dynamicZoneS.leaveMessage ,
                "comfort",   baseEventS.dynamicZoneS.comfort ,
                "temperature",   baseEventS.dynamicZoneS.temperature ,
                "radiation",   baseEventS.dynamicZoneS.radiation ,
                "ejectspawns",   baseEventS.dynamicZoneS.ejectSpawns ,
            };
            if (!string.IsNullOrEmpty(baseEventS.dynamicZoneS.permission))
            {
                zoneSettings.Add("permission");
                zoneSettings.Add(baseEventS.dynamicZoneS.permission);
            }
            foreach (var flag in baseEventS.dynamicZoneS.extraZoneFlags)
            {
                if (string.IsNullOrEmpty(flag)) continue;
                zoneSettings.Add(flag);
                zoneSettings.Add("true");
            }
            return zoneSettings.ToArray();
        }

        private class DynamicZoneS
        {
            [JsonProperty(PropertyName = "Zone Comfort")]
            public string comfort = "0";

            [JsonProperty(PropertyName = "Zone Temperature")]
            public string temperature = "0";

            [JsonProperty(PropertyName = "Zone Radiation")]
            public string radiation = "0";

            [JsonProperty(PropertyName = "Zone Radius")]
            public string radius = "100";

            [JsonProperty(PropertyName = "Enter Message")]
            public string enterMessage = "Entering a PVP area!";

            [JsonProperty(PropertyName = "Leave Message")]
            public string leaveMessage = "Leaving a PVP area.";

            [JsonProperty(PropertyName = "Permission Required To Enter Zone")]
            public string permission = string.Empty;

            [JsonProperty(PropertyName = "Eject Spawns")]
            public string ejectSpawns = string.Empty;

            [JsonProperty(PropertyName = "Extra Zone Flags")]
            public List<string> extraZoneFlags = new List<string>();
        }

        private bool CreateZone(string zoneID, string[] zoneArgs, Vector3 zoneLocation) => (bool)ZoneManager.Call("CreateOrUpdateZone", zoneID, zoneArgs, zoneLocation);

        private bool RemoveZone(string zoneID) => (bool)ZoneManager.Call("EraseZone", zoneID);

        private string[] GetZoneIDs() => (string[])ZoneManager.Call("GetZoneIDs");

        private string GetZoneName(string zoneID) => (string)ZoneManager.Call("GetZoneName", zoneID);

        private string[] GetPlayerZoneIDs(BasePlayer player) => (string[])ZoneManager.Call("GetPlayerZoneIDs", player);

        private bool IsPlayerInZone(string zoneID, BasePlayer player) => (bool)ZoneManager.Call("IsPlayerInZone", zoneID, player);

        #endregion ZoneManager Integration

        #region Chat And Console Command Handler

        private object OnPlayerCommand(BasePlayer player, string command, string[] args) => CheckCommand(player, command, true);

        private object OnServerCommand(ConsoleSystem.Arg arg) => CheckCommand(arg?.Player(), arg?.cmd?.FullName, false);

        private object CheckCommand(BasePlayer player, string command, bool isChat)
        {
            if (player == null) return null;
            command = command?.ToLower()?.TrimStart('/');
            if (string.IsNullOrEmpty(command)) return null;

            string[] result = GetPlayerZoneIDs(player);
            if (result == null || result.Length == 0 || (result.Length == 1 && string.IsNullOrEmpty(result[0]))) return null;

            foreach (var zoneID in result)
            {
                string eventName;
                if (activeDynamicZones.TryGetValue(zoneID, out eventName))
                {
                    DebugPrint($"Checking command: {command} , zoneID: {zoneID}", false);
                    var baseEventS = GetBaseEventS(eventName);
                    if (baseEventS.commandList.Count <= 0) continue;

                    var commandExist = baseEventS.commandList.Any(entry =>
                        isChat
                            ? entry.StartsWith("/") && entry.Substring(1).Equals(command)
                            : !entry.StartsWith("/") && command.Contains(entry));

                    if (baseEventS.useBlacklistCommands)
                    {
                        if (commandExist)
                        {
                            DebugPrint($"Use blacklist, Blocked command: {command}", false);
                            return false;
                        }
                    }
                    else
                    {
                        if (!commandExist)
                        {
                            DebugPrint($"Use whitelist, Blocked command: {command}", false);
                            return false;
                        }
                    }
                }
            }
            return null;
        }

        #endregion Chat And Console Command Handler

        #region Debug

        private readonly StringBuilder debugStringBuilder = new StringBuilder();

        private void DebugPrint(string message, bool warning)
        {
            if (configData.global.debugEnabled)
            {
                if (warning) PrintWarning(message);
                else Puts(message);
            }

            if (configData.global.logToFile)
            {
                debugStringBuilder.AppendLine($"[{DateTime.Now.ToString(CultureInfo.InvariantCulture)}] | {message}");
            }
        }

        private void SaveDebug()
        {
            var text = debugStringBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(text))
            {
                LogToFile("debug", text, this);
                debugStringBuilder.Clear();
            }
        }

        #endregion Debug

        #region API

        private string[] AllDynamicPVPZones => activeDynamicZones.Keys.ToArray();

        private bool IsDynamicPVPZone(string zoneID) => activeDynamicZones.ContainsKey(zoneID);

        private bool EventDataExists(string eventName) => storedData.EventDataExists(eventName);

        private bool IsPlayerInPVPDelay(ulong playerID) => pvpDelays.ContainsKey(playerID);

        private string GetEventName(string zoneID)
        {
            string eventName;
            activeDynamicZones.TryGetValue(zoneID, out eventName);
            return eventName;
        }

        private bool CreateOrUpdateEventData(string eventName, string eventData, bool isTimed = false)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(eventData)) return false;
            if (EventDataExists(eventName)) RemoveEventData(eventName);
            if (isTimed)
            {
                TimedEventS timedEventS;
                try { timedEventS = JsonConvert.DeserializeObject<TimedEventS>(eventData); } catch { return false; }
                storedData.timedEvents.Add(eventName, timedEventS);
            }
            else
            {
                AutoEventS autoEventS;
                try { autoEventS = JsonConvert.DeserializeObject<AutoEventS>(eventData); } catch { return false; }
                storedData.autoEvents.Add(eventName, autoEventS);
                if (autoEventS.autoStart) CreateDynamicZone(eventName);
            }
            dataChanged = true;
            return true;
        }

        #endregion API

        #region Commands

        private void CmdDynamicPVP(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.HasPermission(PERMISSION_ADMIN) && !iPlayer.IsAdmin)
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id));
                return;
            }
            if (args == null || args.Length < 1)
            {
                Print(iPlayer, Lang("SyntaxError", iPlayer.Id, configData.chatS.command));
                return;
            }
            if (args[0].ToLower() == "list")
            {
                var customEventCount = storedData.CustomEventCount;
                if (customEventCount <= 0)
                {
                    Print(iPlayer, Lang("NoCustomEvent", iPlayer.Id));
                    return;
                }
                int i = 0;
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("CustomEvents", iPlayer.Id, customEventCount));
                foreach (var entry in storedData.autoEvents)
                {
                    i++;
                    stringBuilder.AppendLine(Lang("AutoEvent", iPlayer.Id, i, entry.Key, entry.Value.autoStart, entry.Value.position));
                }
                foreach (var entry in storedData.timedEvents)
                {
                    i++;
                    stringBuilder.AppendLine(Lang("TimedEvent", iPlayer.Id, i, entry.Key, entry.Value.duration));
                }
                Print(iPlayer, stringBuilder.ToString());
                return;
            }
            if (args.Length < 2)
            {
                Print(iPlayer, Lang("NoEventName", iPlayer.Id));
                return;
            }
            string eventName = args[1];
            Vector3 position = (iPlayer.Object as BasePlayer)?.transform.position ?? Vector3.zero;
            switch (args[0].ToLower())
            {
                case "add":
                    bool isTimed = args.Length >= 3;
                    Print(iPlayer,
                        !CreateEventData(eventName, position, isTimed)
                            ? Lang("EventNameExist", iPlayer.Id, eventName)
                            : Lang("EventDataAdded", iPlayer.Id, eventName));
                    return;

                case "remove":
                    Print(iPlayer,
                        !RemoveEventData(eventName)
                            ? Lang("EventNameNotExist", iPlayer.Id, eventName)
                            : Lang("EventDataRemoved", iPlayer.Id, eventName));
                    return;

                case "start":
                    Print(iPlayer,
                        !StartEvent(eventName, position)
                            ? Lang("EventNameNotExist", iPlayer.Id, eventName)
                            : Lang("EventStarted", iPlayer.Id, eventName));
                    return;

                case "stop":
                    Print(iPlayer,
                        !StopEvent(eventName)
                            ? Lang("EventNameExist", iPlayer.Id, eventName)
                            : Lang("EventStopped", iPlayer.Id, eventName));
                    return;

                case "edit":
                    if (args.Length >= 3)
                    {
                        AutoEventS autoEventS;
                        if (storedData.autoEvents.TryGetValue(eventName, out autoEventS))
                        {
                            switch (args[2])
                            {
                                case "1":
                                case "true":
                                    autoEventS.autoStart = true;
                                    Print(iPlayer, Lang("AutoEventAutoStart", iPlayer.Id, eventName, true));
                                    dataChanged = true;
                                    return;

                                case "0":
                                case "false":
                                    autoEventS.autoStart = false;
                                    Print(iPlayer, Lang("AutoEventAutoStart", iPlayer.Id, eventName, false));
                                    dataChanged = true;
                                    return;

                                case "move":
                                    autoEventS.position = position;
                                    Print(iPlayer, Lang("AutoEventMove", iPlayer.Id, eventName));
                                    dataChanged = true;
                                    return;
                            }
                        }
                        else
                        {
                            TimedEventS timedEventS;
                            if (storedData.timedEvents.TryGetValue(eventName, out timedEventS))
                            {
                                float duration;
                                if (float.TryParse(args[2], out duration))
                                {
                                    timedEventS.duration = duration;
                                    Print(iPlayer, Lang("TimedEventDuration", iPlayer.Id, eventName, duration));
                                    dataChanged = true;
                                    return;
                                }
                            }
                        }
                    }
                    Print(iPlayer, Lang("SyntaxError", iPlayer.Id, configData.chatS.command));
                    return;

                case "h":
                case "help":
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLine(Lang("Syntax", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax1", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax2", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax3", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax4", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax5", iPlayer.Id, configData.chatS.command));
                    stringBuilder.AppendLine(Lang("Syntax6", iPlayer.Id, configData.chatS.command));
                    Print(iPlayer, stringBuilder.ToString());
                    return;

                default:
                    Print(iPlayer, Lang("SyntaxError", iPlayer.Id, configData.chatS.command));
                    return;
            }
        }

        private bool CreateEventData(string eventName, Vector3 position, bool isTimed)
        {
            if (EventDataExists(eventName)) return false;
            if (isTimed)
            {
                var timedEventS = new TimedEventS();
                storedData.timedEvents.Add(eventName, timedEventS);
            }
            else
            {
                var autoEventS = new AutoEventS { position = position };
                storedData.autoEvents.Add(eventName, autoEventS);
            }
            dataChanged = true;
            return true;
        }

        private bool RemoveEventData(string eventName)
        {
            if (!EventDataExists(eventName)) return false;
            storedData.RemoveEventData(eventName);
            ForceCloseZones(eventName);
            dataChanged = true;
            return true;
        }

        private bool StartEvent(string eventName, Vector3 position)
        {
            if (!EventDataExists(eventName)) return false;
            CreateDynamicZone(eventName, position);
            return true;
        }

        private bool StopEvent(string eventName)
        {
            if (!EventDataExists(eventName)) return false;
            ForceCloseZones(eventName);
            return true;
        }

        private bool ForceCloseZones(string eventName)
        {
            foreach (var entry in activeDynamicZones.ToArray())
            {
                if (entry.Value == eventName)
                {
                    DeleteDynamicZone(entry.Key);
                    return true;
                }
            }

            return false;
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Global Settings")]
            public GlobalS global = new GlobalS();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatS chatS = new ChatS();

            [JsonProperty(PropertyName = "General Event Settings")]
            public GeneralEventS generalEvents = new GeneralEventS();

            [JsonProperty(PropertyName = "Monument Event Settings")]
            public Dictionary<string, MonumentEventS> monumentEvents = new Dictionary<string, MonumentEventS>();
        }

        private class GlobalS
        {
            [JsonProperty(PropertyName = "Enable Debug Mode")]
            public bool debugEnabled = false;

            [JsonProperty(PropertyName = "Log Debug To File")]
            public bool logToFile = true;

            [JsonProperty(PropertyName = "Compare Radius (Used to determine if it is a SupplySignal)")]
            public float compareRadius = 50f;

            [JsonProperty(PropertyName = "If the entity has an owner, don't create a PVP zone")]
            public bool checkEntityOwner = true;
        }

        public class ChatS
        {
            [JsonProperty(PropertyName = "Command")]
            public string command = "dynpvp";

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "[DynamicPVP]: ";

            [JsonProperty(PropertyName = "Chat Prefix Color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;
        }

        private class GeneralEventS
        {
            [JsonProperty(PropertyName = "Bradley Event")]
            public TimedEventS bradleyAPC = new TimedEventS();

            [JsonProperty(PropertyName = "Patrol Helicopter Event")]
            public TimedEventS patrolHelicopter = new TimedEventS();

            [JsonProperty(PropertyName = "Supply Signal Event")]
            public TimedEventS supplySignal = new TimedEventS();

            [JsonProperty(PropertyName = "Timed Supply Event")]
            public TimedEventS supplyDrop = new TimedEventS();

            [JsonProperty(PropertyName = "Hackable Crate Event")]
            public HackEventS hackableCrate = new HackEventS();

            [JsonProperty(PropertyName = "Excavator Ignition Event")]
            public MonumentEventS excavatorIgnition = new MonumentEventS();
        }

        private class BaseEventS
        {
            [JsonProperty(PropertyName = "Enable Event", Order = 1)]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Enable PVP Delay", Order = 2)]
            public bool pvpDelayEnabled = false;

            [JsonProperty(PropertyName = "PVP Delay Time", Order = 3)]
            public float pvpDelayTime = 10f;

            [JsonProperty(PropertyName = "Enable Domes", Order = 4)]
            public bool domesEnabled = true;

            [JsonProperty(PropertyName = "Domes Darkness", Order = 5)]
            public int domesDarkness = 8;

            [JsonProperty(PropertyName = "Delay In Starting Event", Order = 6)]
            public float eventStartDelay = 0f;

            [JsonProperty(PropertyName = "TruePVE Mapping", Order = 7)]
            public string mapping = "exclude";

            [JsonProperty(PropertyName = "Use Blacklist Commands (If false, a whitelist is used)", Order = 8)]
            public bool useBlacklistCommands = true;

            [JsonProperty(PropertyName = "Command List (If there is a '/' at the front, it is a chat command)", Order = 9)]
            public List<string> commandList = new List<string>();

            [JsonProperty(PropertyName = "Dynamic PVP Zone Settings", Order = 10)]
            public DynamicZoneS dynamicZoneS = new DynamicZoneS();
        }

        private class MonumentEventS : BaseEventS
        {
            [JsonProperty(PropertyName = "Zone ID", Order = 11)]
            public string zoneID = string.Empty;

            [JsonProperty(PropertyName = "Transform Position", Order = 12)]
            public Vector3 transformPosition = Vector3.zero;
        }

        private class BotSpawnEventS : BaseEventS
        {
            [JsonProperty(PropertyName = "Enable Bots (Need BotSpawn Plugin)", Order = 11)]
            public bool botsEnabled = false;

            [JsonProperty(PropertyName = "BotSpawn Profile Name", Order = 12)]
            public string botProfileName = string.Empty;
        }

        private class TimedEventS : BotSpawnEventS
        {
            [JsonProperty(PropertyName = "Event Duration", Order = 13)]
            public float duration = 600;
        }

        private class AutoEventS : BotSpawnEventS
        {
            [JsonProperty(PropertyName = "Auto Start", Order = 13)]
            public bool autoStart = false;

            [JsonProperty(PropertyName = "Zone ID", Order = 14)]
            public string zoneID = string.Empty;

            [JsonProperty(PropertyName = "Position", Order = 15)]
            public Vector3 position = Vector3.zero;
        }

        private class HackEventS : TimedEventS
        {
            [JsonProperty(PropertyName = "Hackable Crate Event Start When Spawned (If false, the event starts when unlocking)", Order = 14)]
            public bool spawnStart = false;

            [JsonProperty(PropertyName = "Excluding Hackable Crate On OilRig", Order = 15)]
            public bool excludeOilRig = false;

            [JsonProperty(PropertyName = "Excluding Hackable Crate on Cargo Ship", Order = 16)]
            public bool excludeCargoShip = true;
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

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public readonly Dictionary<string, TimedEventS> timedEvents = new Dictionary<string, TimedEventS>();
            public readonly Dictionary<string, AutoEventS> autoEvents = new Dictionary<string, AutoEventS>();

            public bool EventDataExists(string eventName) => timedEvents.ContainsKey(eventName) || autoEvents.ContainsKey(eventName);

            public bool RemoveEventData(string eventName) => timedEvents.Remove(eventName) || autoEvents.Remove(eventName);

            [JsonIgnore] public int CustomEventCount => timedEvents.Count + autoEvents.Count;
        }

        private void LoadData()
        {
            try
            {
                storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
            }
            catch
            {
                storedData = null;
            }
            finally
            {
                if (storedData == null)
                {
                    ClearData();
                }
            }
        }

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        #endregion DataFile

        #region LanguageFile

        private void Print(IPlayer iPlayer, string message)
        {
            if (iPlayer == null) return;
            if (iPlayer.Id == "server_console") iPlayer.Reply(message, configData.chatS.prefix);
            else
            {
                var player = iPlayer.Object as BasePlayer;
                if (player != null) Player.Message(player, message, $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);
                else iPlayer.Reply(message, $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>");
            }
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command",
                ["NoCustomEvent"] = "There is no custom event data",
                ["CustomEvents"] = "There are {0} custom event data",
                ["AutoEvent"] = "{0}.[AutoEvent]: '{1}'. AutoStart: {2}. Position: {3}",
                ["TimedEvent"] = "{0}.[TimedEvent]: '{1}'. Duration: {2}",
                ["NoEventName"] = "Please type event name",
                ["EventNameExist"] = "The event name {0} already exists",
                ["EventNameNotExist"] = "The event name {0} does not exist",
                ["EventDataAdded"] = "'{0}' event data was added successfully",
                ["EventDataRemoved"] = "'{0}' event data was removed successfully",
                ["EventStarted"] = "'{0}' event started successfully",
                ["EventStopped"] = "'{0}' event stopped successfully",

                ["AutoEventAutoStart"] = "'{0}' event auto start is {1}",
                ["AutoEventMove"] = "'{0}' event moves to your current location",
                ["TimedEventDuration"] = "'{0}' event duration is changed to {1} seconds",

                ["SyntaxError"] = "Syntax error, please type '<color=#ce422b>/{0} <help | h></color>' to view help",
                ["Syntax"] = "<color=#ce422b>/{0} add <eventName> [timed]</color> - Add event data. If added 'timed', it will be a timed event",
                ["Syntax1"] = "<color=#ce422b>/{0} remove <eventName></color> - Remove event data",
                ["Syntax2"] = "<color=#ce422b>/{0} start <eventName></color> - Start event",
                ["Syntax3"] = "<color=#ce422b>/{0} stop <eventName></color> - Stop event",
                ["Syntax4"] = "<color=#ce422b>/{0} edit <eventName> <true/false></color> - Changes auto start state of auto event",
                ["Syntax5"] = "<color=#ce422b>/{0} edit <eventName> <move></color> - Move auto event to your current location",
                ["Syntax6"] = "<color=#ce422b>/{0} edit <eventName> <time(seconds)></color> - Changes the duration of a timed event",
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["NoCustomEvent"] = "您没有创建任何自定义事件数据",
                ["CustomEvents"] = "当前自定义事件数有 {0}个",
                ["AutoEvent"] = "{0}.[自动事件]: '{1}'. 自动启用: {2}. 位置: {3}",
                ["TimedEvent"] = "{0}.[定时事件]: '{1}'. 持续时间: {2}",
                ["NoEventName"] = "请输入事件名字",
                ["EventNameExist"] = "'{0}' 事件名字已存在",
                ["EventNameNotExist"] = "'{0}' 事件名字不存在",
                ["EventDataAdded"] = "'{0}' 事件数据添加成功",
                ["EventDataRemoved"] = "'{0}' 事件数据删除成功",
                ["EventStarted"] = "'{0}' 事件成功开启",
                ["EventStopped"] = "'{0}' 事件成功停止",

                ["AutoEventAutoStart"] = "'{0}' 事件自动开启状态为 {1}",
                ["AutoEventMove"] = "'{0}' 事件移到了您的当前位置",
                ["TimedEventDuration"] = "'{0}' 事件的持续时间改为了 {1}秒",

                ["SyntaxError"] = "语法错误, 输入 '<color=#ce422b>/{0} <help | h></color>' 查看帮助",
                ["Syntax"] = "<color=#ce422b>/{0} add <eventName> [timed]</color> - 添加事件数据。如果后面加上'timed'，将添加定时事件数据",
                ["Syntax1"] = "<color=#ce422b>/{0} remove <eventName></color> - 删除事件数据",
                ["Syntax2"] = "<color=#ce422b>/{0} start <eventName></color> - 开启事件",
                ["Syntax3"] = "<color=#ce422b>/{0} stop <eventName></color> - 停止事件",
                ["Syntax4"] = "<color=#ce422b>/{0} edit <eventName> <true/false></color> - 改变自动事件的自动启动状态",
                ["Syntax5"] = "<color=#ce422b>/{0} edit <eventName> <move></color> - 移动自动事件的位置到您的当前位置",
                ["Syntax6"] = "<color=#ce422b>/{0} edit <eventName> <time(seconds)></color> - 修改定时事件的持续时间",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}