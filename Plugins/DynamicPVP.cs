//Requires: ZoneManager
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Dynamic PVP", "CatMeat/Arainrr", "4.2.1", ResourceId = 2728)]
    [Description("Creates temporary PvP zones on certain actions/events")]
    public class DynamicPVP : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin ZoneManager, TruePVE, NextGenPVE, BotSpawn;

        private const string PERMISSION_ADMIN = "dynamicpvp.admin";
        private const string PREFAB_SPHERE = "assets/prefabs/visualization/sphere.prefab";
        private const string ZONE_NAME = "DynamicPVP";
        private static readonly object True = true, False = false, Null = null;

        private readonly Dictionary<ulong, LeftZone> pvpDelays = new Dictionary<ulong, LeftZone>();
        private readonly Dictionary<string, string> activeDynamicZones = new Dictionary<string, string>();//ID -> EventName

        private bool dataChanged;
        private Vector3 oilRigPosition;
        private Vector3 largeOilRigPosition;
        private Coroutine createEventsCoroutine;

        private class LeftZone : Pool.IPooled
        {
            public string zoneID;
            public Timer zoneTimer;

            public void EnterPool()
            {
                zoneID = null;
                zoneTimer?.Destroy();
                zoneTimer = null;
            }

            public void LeavePool()
            {
            }
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        private enum PVPDelayFlags
        {
            None = 0,
            ZonePlayersCanDamageDelayedPlayers = 1,
            DelayedPlayersCanDamageZonePlayers = 1 << 1,
            DelayedPlayersCanDamageDelayedPlayers = 1 << 2,
        }

        private enum GeneralEventType
        {
            Bradley,
            Helicopter,
            SupplyDrop,
            SupplySignal,
            CargoShip,
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
            Unsubscribe(nameof(OnCargoPlaneSignaled));
            Unsubscribe(nameof(OnCrateHack));
            Unsubscribe(nameof(OnDieselEngineToggled));
            Unsubscribe(nameof(OnEntityDeath));
            Unsubscribe(nameof(OnSupplyDropLanded));
            Unsubscribe(nameof(OnEntityKill));
            Unsubscribe(nameof(OnPlayerCommand));
            Unsubscribe(nameof(OnServerCommand));
            Unsubscribe(nameof(CanEntityTakeDamage));
            Unsubscribe(nameof(OnEnterZone));
            Unsubscribe(nameof(OnExitZone));
        }

        private void OnServerInitialized()
        {
            //Pool.FillBuffer<LeftZone>();
            DeleteOldDynamicZones();
            createEventsCoroutine = ServerMgr.Instance.StartCoroutine(CreateMonumentEvents());
            if (configData.generalEvents.excavatorIgnition.enabled)
            {
                Subscribe(nameof(OnDieselEngineToggled));
            }
            if (configData.generalEvents.patrolHelicopter.enabled || configData.generalEvents.bradleyAPC.enabled)
            {
                Subscribe(nameof(OnEntityDeath));
            }
            if (configData.generalEvents.supplySignal.enabled || configData.generalEvents.timedSupply.enabled)
            {
                Subscribe(nameof(OnCargoPlaneSignaled));
            }
            if (configData.generalEvents.hackableCrate.enabled && !configData.generalEvents.hackableCrate.spawnStart)
            {
                Subscribe(nameof(OnCrateHack));
            }
            if (configData.generalEvents.timedSupply.enabled && !configData.generalEvents.timedSupply.spawnStart ||
                configData.generalEvents.supplySignal.enabled && !configData.generalEvents.supplySignal.spawnStart)
            {
                Subscribe(nameof(OnSupplyDropLanded));
            }
            if (configData.generalEvents.timedSupply.enabled && configData.generalEvents.timedSupply.spawnStart ||
                configData.generalEvents.supplySignal.enabled && configData.generalEvents.supplySignal.spawnStart ||
                configData.generalEvents.hackableCrate.enabled && configData.generalEvents.hackableCrate.spawnStart)
            {
                Subscribe(nameof(OnEntitySpawned));
            }
            if (configData.generalEvents.cargoShip.enabled)
            {
                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnEntityKill));
                foreach (var serverEntity in BaseNetworkable.serverEntities)
                {
                    var cargoShip = serverEntity as CargoShip;
                    if (cargoShip == null) continue;
                    OnEntitySpawned(cargoShip);
                }
            }
        }

        private void Unload()
        {
            if (createEventsCoroutine != null)
            {
                ServerMgr.Instance.StopCoroutine(createEventsCoroutine);
            }

            if (activeDynamicZones.Count > 0)
            {
                PrintDebug($"Deleting {activeDynamicZones.Count} active zones.", true);
                foreach (var entry in activeDynamicZones.ToArray())
                {
                    DeleteDynamicZone(entry.Key);
                }
            }

            var leftZones = pvpDelays.Values.ToArray();
            for (var i = leftZones.Length - 1; i >= 0; i--)
            {
                var value = leftZones[i];
                Pool.Free(ref value);
            }

            var spheres = zoneSpheres.Values.ToArray();
            for (var i = spheres.Length - 1; i >= 0; i--)
            {
                var sphereEntities = spheres[i];
                foreach (var sphereEntity in sphereEntities)
                {
                    if (sphereEntity != null && !sphereEntity.IsDestroyed)
                        sphereEntity.KillMessage();
                }
                Pool.FreeList(ref sphereEntities);
            }

            SaveData();
            SaveDebug();
            Pool.directory.Remove(typeof(LeftZone));
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), () =>
        {
            SaveDebug();
            if (dataChanged)
            {
                SaveData();
                dataChanged = false;
            }
        });

        private void OnPlayerRespawned(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            TryRemovePVPDelay(player.userID, player.displayName);
        }

        #endregion Oxide Hooks

        #region Methods

        private LeftZone GetOrAddPVPDelay(ulong playerID, string playerName)
        {
            PrintDebug($"Adding {playerName} to pvp delay.");
            LeftZone leftZone;
            if (pvpDelays.TryGetValue(playerID, out leftZone))
            {
                leftZone.zoneTimer?.Destroy();
            }
            else
            {
                leftZone = Pool.Get<LeftZone>();
                pvpDelays.Add(playerID, leftZone);
                CheckHooks(true);
            }

            return leftZone;
        }

        private void TryRemovePVPDelay(ulong playerID, string playerName)
        {
            PrintDebug($"Removing {playerName} from pvp delay.");
            LeftZone leftZone;
            if (pvpDelays.TryGetValue(playerID, out leftZone))
            {
                pvpDelays.Remove(playerID);
                Pool.Free(ref leftZone);
                CheckHooks(true);
            }
        }

        private bool CheckEntityOwner(BaseEntity baseEntity)
        {
            if (configData.global.checkEntityOwner && baseEntity.OwnerID.IsSteamId())
            {
                PrintDebug($"{baseEntity} is owned by the player({baseEntity.OwnerID}). Skipping event creation.");
                return false;
            }
            return true;
        }

        private bool CanCreateDynamicPVP(string eventName, BaseEntity entity)
        {
            if (Interface.CallHook("OnCreateDynamicPVP", eventName, entity) != null)
            {
                PrintDebug($"There are other plugins that prevent {eventName} events from being created.", true);
                return false;
            }
            return true;
        }

        private void CheckHooks(bool pvpDelay = false)
        {
            if (pvpDelay)
            {
                if (pvpDelays.Count > 0)
                {
                    Subscribe(nameof(CanEntityTakeDamage));
                }
                else
                {
                    Unsubscribe(nameof(CanEntityTakeDamage));
                }
            }
            else
            {
                if (activeDynamicZones.Count > 0)
                {
                    Subscribe(nameof(OnEnterZone));
                    Subscribe(nameof(OnExitZone));
                }
                else
                {
                    Unsubscribe(nameof(OnEnterZone));
                    Unsubscribe(nameof(OnExitZone));
                }

                bool hasCommands = false;
                foreach (var entry in activeDynamicZones)
                {
                    var baseEventS = GetBaseEventS(entry.Value);
                    if (baseEventS != null)
                    {
                        if (baseEventS.commandList.Count > 0)
                        {
                            hasCommands = true;
                            break;
                        }
                    }
                }
                if (hasCommands)
                {
                    Subscribe(nameof(OnPlayerCommand));
                    Subscribe(nameof(OnServerCommand));
                }
                else
                {
                    Unsubscribe(nameof(OnPlayerCommand));
                    Unsubscribe(nameof(OnServerCommand));
                }
            }
        }

        private BaseEventS GetBaseEventS(string eventName)
        {
            if (Enum.IsDefined(typeof(GeneralEventType), eventName))
            {
                GeneralEventType generalEventType;
                if (Enum.TryParse(eventName, true, out generalEventType))
                {
                    switch (generalEventType)
                    {
                        case GeneralEventType.Bradley: return configData.generalEvents.bradleyAPC;
                        case GeneralEventType.HackableCrate: return configData.generalEvents.hackableCrate;
                        case GeneralEventType.Helicopter: return configData.generalEvents.patrolHelicopter;
                        case GeneralEventType.SupplyDrop: return configData.generalEvents.timedSupply;
                        case GeneralEventType.SupplySignal: return configData.generalEvents.supplySignal;
                        case GeneralEventType.ExcavatorIgnition: return configData.generalEvents.excavatorIgnition;
                        case GeneralEventType.CargoShip: return configData.generalEvents.cargoShip;
                        default:
                            PrintDebug($"ERROR: Unknown GeneralEventType: {generalEventType} | {eventName}.", error: true);
                            break;
                    }
                }
            }
            AutoEventS autoEventS;
            if (storedData.autoEvents.TryGetValue(eventName, out autoEventS))
                return autoEventS;
            TimedEventS timedEventS;
            if (storedData.timedEvents.TryGetValue(eventName, out timedEventS))
                return timedEventS;
            MonumentEventS monumentEventS;
            if (configData.monumentEvents.TryGetValue(eventName, out monumentEventS))
                return monumentEventS;
            PrintDebug($"ERROR: Failed to get base event settings for {eventName}.", error: true);
            return null;
        }

        #endregion Methods

        #region Events

        #region General Event

        #region ExcavatorIgnition Event

        private string excavatorIgnitionEventZoneID;

        private void OnDieselEngineToggled(DieselEngine dieselEngine)
        {
            if (dieselEngine == null) return;
            if (dieselEngine.IsOn())
            {
                if (!string.IsNullOrEmpty(excavatorIgnitionEventZoneID))
                {
                    DeleteDynamicZone(excavatorIgnitionEventZoneID);
                }
                HandleGeneralEvent(GeneralEventType.ExcavatorIgnition, dieselEngine);
            }
            else
            {
                if (string.IsNullOrEmpty(excavatorIgnitionEventZoneID))
                {
                    return;
                }
                DeleteDynamicZone(excavatorIgnitionEventZoneID);
            }
        }

        #endregion ExcavatorIgnition Event

        #region HackableLockedCrate Event

        private void OnEntitySpawned(HackableLockedCrate hackableLockedCrate)
        {
            if (hackableLockedCrate == null) return;
            if (!configData.generalEvents.hackableCrate.enabled || !configData.generalEvents.hackableCrate.spawnStart) return;
            PrintDebug("Trying to create the event when the hackable locked crate is spawned.");
            NextTick(() => LockedCrateEvent(hackableLockedCrate));
        }

        private void OnCrateHack(HackableLockedCrate hackableLockedCrate)
        {
            if (hackableLockedCrate == null) return;
            PrintDebug("Trying to create the event when the hackable locked crate is starting hack.");
            NextTick(() => LockedCrateEvent(hackableLockedCrate));
        }

        private void LockedCrateEvent(HackableLockedCrate hackableLockedCrate)
        {
            if (!CheckEntityOwner(hackableLockedCrate))
            {
                return;
            }
            if (configData.generalEvents.hackableCrate.excludeOilRig && OnTheOilRig(hackableLockedCrate))
            {
                PrintDebug("The hackable locked crate is on the oil rig. Skipping event creation.");
                return;
            }
            if (configData.generalEvents.hackableCrate.excludeCargoShip && OnTheCargoShip(hackableLockedCrate))
            {
                PrintDebug("The hackable locked crate is on the cargo ship. Skipping event creation.");
                return;
            }
            HandleGeneralEvent(GeneralEventType.HackableCrate, hackableLockedCrate);
        }

        private bool OnTheCargoShip(HackableLockedCrate hackableLockedCrate) =>
            hackableLockedCrate.GetComponentInParent<CargoShip>() != null;

        private bool OnTheOilRig(HackableLockedCrate hackableLockedCrate)
        {
            if (oilRigPosition != Vector3.zero &&
                Vector3Ex.Distance2D(hackableLockedCrate.transform.position, oilRigPosition) < 50f)
            {
                return true;
            }

            if (largeOilRigPosition != Vector3.zero &&
                Vector3Ex.Distance2D(hackableLockedCrate.transform.position, largeOilRigPosition) < 50f)
            {
                return true;
            }
            return false;
        }

        #endregion HackableLockedCrate Event

        #region BaseHelicopter And BradleyAPC Event

        private void OnEntityDeath(BaseHelicopter baseHelicopter, HitInfo info)
        {
            if (baseHelicopter == null) return;
            PatrolHelicopterEvent(baseHelicopter);
        }

        private void OnEntityDeath(BradleyAPC bradleyApc, HitInfo info)
        {
            if (bradleyApc == null) return;
            BradleyApcEvent(bradleyApc);
        }

        private void PatrolHelicopterEvent(BaseHelicopter baseHelicopter)
        {
            if (!configData.generalEvents.patrolHelicopter.enabled) return;
            PrintDebug("Trying to create the event when the helicopter is dead.");
            if (!CheckEntityOwner(baseHelicopter))
            {
                return;
            }
            HandleGeneralEvent(GeneralEventType.Helicopter, baseHelicopter);
        }

        private void BradleyApcEvent(BradleyAPC bradleyAPC)
        {
            if (!configData.generalEvents.bradleyAPC.enabled) return;
            PrintDebug("Trying to create the event when the bradley apc is dead.");
            if (!CheckEntityOwner(bradleyAPC))
            {
                return;
            }
            HandleGeneralEvent(GeneralEventType.Bradley, bradleyAPC);
        }

        #endregion BaseHelicopter And BradleyAPC Event

        #region SupplyDrop And SupplySignal Event

        private readonly Dictionary<Vector3, Timer> activeSupplySignals = new Dictionary<Vector3, Timer>();

        private void OnCargoPlaneSignaled(CargoPlane cargoPlane, SupplySignal supplySignal)
        {
            if (supplySignal == null || cargoPlane == null) return;
            var dropPosition = cargoPlane.dropPosition;
            if (activeSupplySignals.ContainsKey(dropPosition)) return;
            var value = timer.Once(900f, () => activeSupplySignals.Remove(dropPosition));
            activeSupplySignals.Add(dropPosition, value);
            PrintDebug($"A supply signal is thrown at {dropPosition}");
        }

        private void OnEntitySpawned(SupplyDrop supplyDrop)
        {
            if (supplyDrop == null) return;
            NextTick(() => SupplyDropEvent(supplyDrop, false));
        }

        private void OnSupplyDropLanded(SupplyDrop supplyDrop)
        {
            if (supplyDrop == null) return;
            NextTick(() => SupplyDropEvent(supplyDrop, true));
        }

        private void SupplyDropEvent(SupplyDrop supplyDrop, bool isLanded)
        {
            PrintDebug($"Trying to create the event when the supply drop is {(isLanded ? "landed" : "spawned")} at {supplyDrop.transform.position}.");
            if (!CheckEntityOwner(supplyDrop))
            {
                return;
            }
            var isFromSupplySignal = IsProbablySupplySignal(supplyDrop.transform.position);
            PrintDebug($"Supply drop is from supply signal: {isFromSupplySignal}");
            if (isFromSupplySignal)
            {
                if (!configData.generalEvents.supplySignal.enabled)
                {
                    PrintDebug("Event for supply signals disabled. Skipping event creation.");
                    return;
                }

                if (isLanded ? configData.generalEvents.supplySignal.spawnStart : !configData.generalEvents.supplySignal.spawnStart)
                {
                    PrintDebug($"{(isLanded ? "Landed" : "Spawned")} for supply signals disabled. Skipping event creation.");
                    return;
                }
                HandleGeneralEvent(GeneralEventType.SupplySignal, supplyDrop);
            }
            else
            {
                if (!configData.generalEvents.timedSupply.enabled)
                {
                    PrintDebug("Event for timed supply disabled. Skipping event creation.");
                    return;
                }
                if (isLanded ? configData.generalEvents.timedSupply.spawnStart : !configData.generalEvents.timedSupply.spawnStart)
                {
                    PrintDebug($"{(isLanded ? "Landed" : "Spawned")} for timed supply disabled. Skipping event creation.");
                    return;
                }
                HandleGeneralEvent(GeneralEventType.SupplyDrop, supplyDrop);
            }
        }

        private bool IsProbablySupplySignal(Vector3 position)
        {
            PrintDebug($"Checking {activeSupplySignals.Count} active supply signals");
            if (activeSupplySignals.Count > 0)
            {
                foreach (var entry in activeSupplySignals)
                {
                    var distance = Vector3Ex.Distance2D(entry.Key, position);
                    PrintDebug($"Found a supply signal at {entry.Key} located {distance}m away.");
                    if (distance <= configData.global.compareRadius)
                    {
                        entry.Value?.Destroy();
                        activeSupplySignals.Remove(entry.Key);
                        PrintDebug($"Found matching a supply signal. Removing Supply signal from active list. Active supply signals remaining: {activeSupplySignals.Count}");
                        return true;
                    }
                }
                PrintDebug("No matches found, probably from a timed event cargo plane");
                return false;
            }
            PrintDebug("No active signals, must be from a timed event cargo plane");
            return false;
        }

        #endregion SupplyDrop And SupplySignal Event

        #region CargoShip Event

        private void OnEntitySpawned(CargoShip cargoShip)
        {
            if (cargoShip == null || cargoShip.net == null) return;
            if (!configData.generalEvents.cargoShip.enabled) return;
            PrintDebug("Trying to create the event when the cargo ship is spawned.");
            if (!CheckEntityOwner(cargoShip))
            {
                return;
            }
            var eventName = GeneralEventType.CargoShip.ToString();
            if (!CanCreateDynamicPVP(eventName, cargoShip))
            {
                return;
            }

            NextTick(() => HandleParentedEntityEvent(eventName, cargoShip));
        }

        private void OnEntityKill(CargoShip cargoShip)
        {
            if (cargoShip == null || cargoShip.net == null) return;
            DeleteDynamicZone(cargoShip.net.ID.ToString());
        }

        #endregion CargoShip Event

        #endregion General Event

        #region Monument Event

        private IEnumerator CreateMonumentEvents()
        {
            var changed = false;
            var createdEvents = Pool.GetList<string>();
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
                        //case "assets/bundled/prefabs/autospawn/monument/harbor/fishing_village_b.prefab": monumentName += " A"; break;
                        //case "assets/bundled/prefabs/autospawn/monument/harbor/fishing_village_c.prefab": monumentName += " B"; break;
                }

                MonumentEventS monumentEventS;
                if (!configData.monumentEvents.TryGetValue(monumentName, out monumentEventS))
                {
                    changed = true;
                    monumentEventS = new MonumentEventS();
                    configData.monumentEvents.Add(monumentName, monumentEventS);
                    Puts($"A new monument {monumentName} was found and added to the config.");
                }
                if (!monumentEventS.enabled) continue;
                if (HandleMonumentEvent(monumentName, monumentInfo.transform, monumentEventS))
                {
                    createdEvents.Add(monumentName);
                }
                yield return CoroutineEx.waitForSeconds(0.5f);
            }
            if (changed)
            {
                SaveConfig();
            }
            if (createdEvents.Count > 0)
            {
                PrintDebug($"{createdEvents.Count}({string.Join(", ", createdEvents)}) monument events were successfully created.", true);
            }

            createdEvents.Clear();
            foreach (var entry in storedData.autoEvents)
            {
                if (entry.Value.autoStart)
                {
                    if (CreateDynamicZone(entry.Key, entry.Value.position, entry.Value.zoneID))
                    {
                        createdEvents.Add(entry.Key);
                    }
                    yield return CoroutineEx.waitForSeconds(0.5f);
                }
            }
            if (createdEvents.Count > 0)
            {
                PrintDebug($"{createdEvents.Count}({string.Join(", ", createdEvents)}) auto events were successfully created.", true);
            }
            Pool.FreeList(ref createdEvents);
            createEventsCoroutine = null;
        }

        #endregion Monument Event

        #region Chat/Console Command Handler

        private object OnPlayerCommand(BasePlayer player, string command, string[] args) => CheckCommand(player, command, true);

        private object OnServerCommand(ConsoleSystem.Arg arg) => CheckCommand(arg?.Player(), arg?.cmd?.FullName, false);

        private object CheckCommand(BasePlayer player, string command, bool isChat)
        {
            if (player == null || string.IsNullOrEmpty(command)) return Null;
            command = command.ToLower().TrimStart('/');
            if (string.IsNullOrEmpty(command)) return Null;

            string[] result = GetPlayerZoneIDs(player);
            if (result == null || result.Length == 0 || result.Length == 1 && string.IsNullOrEmpty(result[0])) return Null;

            foreach (var zoneID in result)
            {
                string eventName;
                if (activeDynamicZones.TryGetValue(zoneID, out eventName))
                {
                    PrintDebug($"Checking command: {command} , zoneID: {zoneID}");
                    var baseEventS = GetBaseEventS(eventName);
                    if (baseEventS == null || baseEventS.commandList.Count <= 0) continue;

                    var commandExist = baseEventS.commandList.Any(entry =>
                       isChat
                           ? entry.StartsWith("/") && entry.Substring(1).Equals(command)
                           : !entry.StartsWith("/") && command.Contains(entry));

                    if (baseEventS.useBlacklistCommands)
                    {
                        if (commandExist)
                        {
                            PrintDebug($"Use blacklist, Blocked command: {command}", true);
                            return False;
                        }
                    }
                    else
                    {
                        if (!commandExist)
                        {
                            PrintDebug($"Use whitelist, Blocked command: {command}", true);
                            return False;
                        }
                    }
                }
            }
            return Null;
        }

        #endregion Chat/Console Command Handler

        #endregion Events

        #region DynamicZone Handler

        private void HandleParentedEntityEvent(string eventName, BaseEntity parentEntity, bool delay = true)
        {
            if (parentEntity == null || parentEntity.net == null) return;
            var baseEventS = GetBaseEventS(eventName);
            if (baseEventS == null) return;
            if (delay && baseEventS.eventStartDelay > 0)
            {
                timer.Once(baseEventS.eventStartDelay, () => HandleParentedEntityEvent(eventName, parentEntity, false));
                return;
            }

            PrintDebug($"Trying to create parented entity event {eventName} on entity({parentEntity}).");
            var zoneID = parentEntity.net.ID.ToString();
            if (CreateDynamicZone(eventName, parentEntity.transform.position, zoneID, false))
            {
                NextTick(() =>
                {
                    var zone = GetZoneByID(zoneID);
                    PrintDebug($"Gets the zone({zoneID} | {zone}) created by the {eventName} event.", true);
                    if (parentEntity != null && zone != null)
                    {
                        var iParentZone = baseEventS.GetDynamicZoneS() as IParentZone;
                        zone.transform.SetParent(parentEntity.transform);
                        zone.transform.rotation = parentEntity.transform.rotation;
                        if (iParentZone != null)
                        {
                            zone.transform.position = parentEntity.transform.TransformPoint(iParentZone.center);
                        }
                        else
                        {
                            zone.transform.position = parentEntity.transform.position;
                        }
                        PrintDebug($"The zone({zoneID} | {eventName}) was parented to entity({parentEntity}).", true);
                    }
                    else
                    {
                        PrintDebug($"ERROR: The zone({zoneID} | {zone} | {parentEntity}) created by the {eventName} event is empty.", error: true);
                        DeleteDynamicZone(zoneID);
                    }
                });
            }
        }

        private bool HandleMonumentEvent(string eventName, Transform transform, MonumentEventS monumentEventS)
        {
            Vector3 position;
            if (monumentEventS.transformPosition != Vector3.zero)
            {
                position = transform.TransformPoint(monumentEventS.transformPosition);
            }
            else
            {
                position = transform.position;
            }
            return CreateDynamicZone(eventName, position, monumentEventS.zoneID);
        }

        private bool HandleGeneralEvent(GeneralEventType generalEventType, BaseEntity baseEntity)
        {
            var eventName = generalEventType.ToString();
            if (!CanCreateDynamicPVP(eventName, baseEntity))
            {
                return false;
            }
            var baseEventS = GetBaseEventS(eventName);
            if (baseEventS == null) return false;
            var position = baseEntity.transform.position;
            position.y = TerrainMeta.HeightMap.GetHeight(position);
            return CreateDynamicZone(eventName, position);
        }

        private readonly StringBuilder successStringBuilder = new StringBuilder();

        private bool CreateDynamicZone(string eventName, Vector3 position, string zoneID = "", bool delay = true)
        {
            if (position == Vector3.zero)
            {
                PrintDebug($"ERROR: Invalid location, zone({eventName}) creation failed. Skipping event creation.", error: true);
                return false;
            }
            var baseEventS = GetBaseEventS(eventName);
            if (baseEventS == null) return false;
            if (delay && baseEventS.eventStartDelay > 0)
            {
                timer.Once(baseEventS.eventStartDelay, () => CreateDynamicZone(eventName, position, zoneID, false));
                return false;
            }

            var dynamicZoneS = baseEventS.GetDynamicZoneS();
            var zoneSettings = dynamicZoneS.ZoneSettings;
            var duration = (baseEventS as ITimedEvent)?.duration ?? -1;
            if (string.IsNullOrEmpty(zoneID))
            {
                zoneID = DateTime.Now.ToString("HHmmssffff");
            }

            PrintDebug($"Trying create zone: {eventName}({zoneID} | {position}) {(dynamicZoneS is ISphereZone ? $"(Radius: {(dynamicZoneS as ISphereZone).radius}m)" : $"(Size: {(dynamicZoneS as ICubeZone)?.size})")} {(dynamicZoneS is IParentZone ? $"(Center: {(dynamicZoneS as IParentZone).center}) " : null)}(Duration: {duration}s).");
            var zoneAdded = CreateZone(zoneID, zoneSettings, position);
            if (zoneAdded)
            {
                if (!activeDynamicZones.ContainsKey(zoneID))
                {
                    activeDynamicZones.Add(zoneID, eventName);
                    CheckHooks();
                }

                if (eventName == GeneralEventType.ExcavatorIgnition.ToString())
                {
                    excavatorIgnitionEventZoneID = zoneID;
                }

                successStringBuilder.Clear();
                var mappingAdded = CreateMapping(zoneID, baseEventS.mapping);
                if (!mappingAdded) PrintDebug($"ERROR: Mapping Not added for zone({zoneID}).", error: true);
                else successStringBuilder.Append("Mapping,");

                var iSphereZone = dynamicZoneS as ISphereZone;
                var iDomeEventS = baseEventS as IDomeEvent;
                if (DomeCreateAllowed(iDomeEventS, iSphereZone))
                {
                    var domeAdded = CreateDome(zoneID, position, iSphereZone.radius, iDomeEventS.domesDarkness);
                    if (!domeAdded) PrintDebug($"ERROR: Dome NOT added for zone({zoneID}).", error: true);
                    else successStringBuilder.Append("Dome,");
                }

                var iBotSpawnEventS = baseEventS as IBotSpawnEvent;
                if (BotSpawnAllowed(iBotSpawnEventS))
                {
                    var botsSpawned = SpawnBots(position, iBotSpawnEventS.botProfileName, zoneID);
                    if (!botsSpawned) PrintDebug($"ERROR: Bot NOT spawned for zone({zoneID}).", error: true);
                    else successStringBuilder.Append("Bots,");
                }

                if (duration > 0)
                {
                    timer.Once(duration, () => DeleteDynamicZone(zoneID));
                }
                PrintDebug($"Created zone({zoneID} | {eventName}) ({successStringBuilder.ToString().TrimEnd(',')}).", true);
                return true;
            }

            PrintDebug($"ERROR: Create zone({eventName}) failed.", error: true);
            return false;
        }

        private bool DeleteDynamicZone(string zoneID)
        {
            string eventName;
            if (string.IsNullOrEmpty(zoneID) || !activeDynamicZones.TryGetValue(zoneID, out eventName))
            {
                PrintDebug($"ERROR: Invalid zoneID: {zoneID}.", error: true);
                return false;
            }

            successStringBuilder.Clear();
            var baseEventS = GetBaseEventS(eventName);
            var dynamicZoneS = baseEventS.GetDynamicZoneS();
            if (DomeCreateAllowed(baseEventS as IDomeEvent, dynamicZoneS as ISphereZone))
            {
                var domeRemoved = RemoveDome(zoneID);
                if (!domeRemoved) PrintDebug($"ERROR: Dome NOT removed for zone({zoneID} | {eventName}).", error: true);
                else successStringBuilder.Append("Dome,");
            }

            if (BotSpawnAllowed(baseEventS as IBotSpawnEvent))
            {
                var botsRemoved = KillBots(zoneID);
                if (!botsRemoved) PrintDebug($"ERROR: Bot NOT killed for zone({zoneID} | {eventName}).", error: true);
                else successStringBuilder.Append("Bots,");
            }

            var mappingRemoved = RemoveMapping(zoneID);
            if (!mappingRemoved) PrintDebug($"ERROR: Mapping NOT removed for zone({zoneID} | {eventName}).", error: true);
            else successStringBuilder.Append("Mapping,");

            var zoneRemoved = RemoveZone(zoneID, eventName);
            if (zoneRemoved)
            {
                if (activeDynamicZones.Remove(zoneID))
                {
                    CheckHooks();
                }
                if (eventName == GeneralEventType.ExcavatorIgnition.ToString())
                {
                    excavatorIgnitionEventZoneID = null;
                }

                PrintDebug($"Deleted zone({zoneID} | {eventName}) ({successStringBuilder.ToString().TrimEnd(',')}).", true);
                return true;
            }
            PrintDebug($"ERROR: Delete zone({zoneID} | {eventName} | {successStringBuilder.ToString().TrimEnd(',')}) failed.", error: true);
            return false;
        }

        private void DeleteOldDynamicZones()
        {
            var zoneIDs = GetZoneIDs();
            if (zoneIDs == null || zoneIDs.Length <= 0) return;
            int attempts = 0, successes = 0;
            foreach (var zoneID in zoneIDs)
            {
                string zoneName = GetZoneName(zoneID);
                if (zoneName == ZONE_NAME)
                {
                    attempts++;
                    var zoneRemoved = RemoveZone(zoneID);
                    if (zoneRemoved) successes++;
                    RemoveMapping(zoneID);
                }
            }
            PrintDebug($"Deleted {successes} of {attempts} existing DynamicPVP zones", true);
        }

        #endregion DynamicZone Handler

        #region ZoneDome Integration

        private readonly Dictionary<string, List<SphereEntity>> zoneSpheres = new Dictionary<string, List<SphereEntity>>();

        private static bool DomeCreateAllowed(IDomeEvent iDomeEventS, ISphereZone iSphereZone) => iDomeEventS != null && iDomeEventS.domesEnabled && iSphereZone?.radius > 0;

        private bool CreateDome(string zoneID, Vector3 position, float radius, int darkness)
        {
            if (radius <= 0) return false;
            var sphereEntities = Pool.GetList<SphereEntity>();
            for (int i = 0; i < darkness; i++)
            {
                var sphereEntity = GameManager.server.CreateEntity(PREFAB_SPHERE, position) as SphereEntity;
                if (sphereEntity == null) { PrintDebug("ERROR: sphere entity is null", error: true); return false; }
                sphereEntity.enableSaving = false;
                sphereEntity.Spawn();
                sphereEntity.LerpRadiusTo(radius * 2f, radius);
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
            {
                sphereEntity.LerpRadiusTo(0, sphereEntity.currentRadius);
            }
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
                Pool.FreeList(ref sphereEntities);
            });
            return true;
        }

        #endregion ZoneDome Integration

        #region TruePVE/NextGenPVE Integration

        private object CanEntityTakeDamage(BasePlayer victim, HitInfo info)
        {
            if (info == null || victim == null || !victim.userID.IsSteamId()) return Null;
            var attacker = info.InitiatorPlayer ?? (info.Initiator != null && info.Initiator.OwnerID.IsSteamId() ? BasePlayer.FindByID(info.Initiator.OwnerID) : null);//The attacker cannot be fully captured
            if (attacker == null || !attacker.userID.IsSteamId()) return Null;
            LeftZone victimLeftZone;
            if (pvpDelays.TryGetValue(victim.userID, out victimLeftZone))
            {
                if (configData.global.pvpDelayFlags.HasFlag(PVPDelayFlags.ZonePlayersCanDamageDelayedPlayers) && !string.IsNullOrEmpty(victimLeftZone.zoneID) && IsPlayerInZone(victimLeftZone, attacker))//ZonePlayer attack DelayedPlayer
                {
                    return True;
                }
                LeftZone attackerLeftZone;
                if (configData.global.pvpDelayFlags.HasFlag(PVPDelayFlags.DelayedPlayersCanDamageDelayedPlayers) && pvpDelays.TryGetValue(attacker.userID, out attackerLeftZone) && victimLeftZone.zoneID == attackerLeftZone.zoneID)//DelayedPlayer attack DelayedPlayer
                {
                    return True;
                }

                return Null;
            }
            else
            {
                LeftZone attackerLeftZone;
                if (pvpDelays.TryGetValue(attacker.userID, out attackerLeftZone))
                {
                    if (configData.global.pvpDelayFlags.HasFlag(PVPDelayFlags.DelayedPlayersCanDamageZonePlayers) && !string.IsNullOrEmpty(attackerLeftZone.zoneID) && IsPlayerInZone(attackerLeftZone, victim))//DelayedPlayer attack ZonePlayer
                    {
                        return True;
                    }

                    return Null;
                }
            }
            return Null;
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

        private bool BotSpawnAllowed(IBotSpawnEvent iBotSpawnEventS)
        {
            if (BotSpawn == null || iBotSpawnEventS == null || string.IsNullOrEmpty(iBotSpawnEventS.botProfileName)) return false;
            return iBotSpawnEventS.botsEnabled;
        }

        private bool SpawnBots(Vector3 zoneLocation, string zoneProfile, string zoneGroupID)
        {
            var result = CreateGroupSpawn(zoneLocation, zoneProfile, zoneGroupID);
            if (result == null || result.Length < 2)
            {
                PrintDebug("AddGroupSpawn returned invalid response.");
                return false;
            }
            switch (result[0])
            {
                case "true": return true;
                case "false": return false;
                case "error": PrintDebug($"ERROR: AddGroupSpawn failed: {result[1]}", error: true); return false;
            }
            return false;
        }

        private bool KillBots(string zoneGroupID)
        {
            var result = RemoveGroupSpawn(zoneGroupID);
            if (result == null || result.Length < 2)
            {
                PrintDebug("RemoveGroupSpawn returned invalid response.");
                return false;
            }
            if (result[0] == "error")
            {
                PrintDebug($"ERROR: RemoveGroupSpawn failed: {result[1]}", error: true);
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
            if (player == null || !player.userID.IsSteamId()) return;
            string eventName;
            if (!activeDynamicZones.TryGetValue(zoneID, out eventName)) return;
            PrintDebug($"{player.displayName} has entered a pvp zone({zoneID} | {eventName}).", true);

            TryRemovePVPDelay(player.userID, player.displayName);
        }

        private void OnExitZone(string zoneID, BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            string eventName;
            if (!activeDynamicZones.TryGetValue(zoneID, out eventName)) return;
            PrintDebug($"{player.displayName} has left a pvp zone({zoneID} | {eventName}).", true);

            var baseEventS = GetBaseEventS(eventName);
            if (!baseEventS.pvpDelayEnabled || baseEventS.pvpDelayTime <= 0) return;

            var playerID = player.userID;
            var playerName = player.displayName;
            var leftZone = GetOrAddPVPDelay(playerID, playerName);
            leftZone.zoneID = zoneID;
            leftZone.zoneTimer = timer.Once(baseEventS.pvpDelayTime, () =>
            {
                TryRemovePVPDelay(playerID, playerName);
            });
        }

        private bool CreateZone(string zoneID, string[] zoneArgs, Vector3 zoneLocation) => (bool)ZoneManager.Call("CreateOrUpdateZone", zoneID, zoneArgs, zoneLocation);

        //private readonly FieldInfo zonesFieldInfo = typeof(ZoneManager).GetField("zones", BindingFlags.Instance | BindingFlags.NonPublic);

        private bool RemoveZone(string zoneID, string eventName = "")
        {
            //if (eventName == GeneralEventType.CargoShip.ToString())
            //{
            //    var zones = zonesFieldInfo.GetValue(ZoneManager) as Hash<string, ZoneManager.Zone>;
            //    if (zones == null)
            //    {
            //        PrintError("Zones == null !!!!!!");
            //    }
            //    zones?.Remove(zoneID);
            //    ZoneManager.Call("SaveData");
            //    return true;
            //}

            try
            {
                return (bool)ZoneManager.Call("EraseZone", zoneID);
            }
            catch (Exception exception)
            {
                PrintDebug($"ERROR: EraseZone failed. {exception}");
                PrintError("RemoveZone ERROR:" + exception);
                return true;
            }
        }

        private string[] GetZoneIDs() => (string[])ZoneManager.Call("GetZoneIDs");

        private string GetZoneName(string zoneID) => (string)ZoneManager.Call("GetZoneName", zoneID);

        private ZoneManager.Zone GetZoneByID(string zoneID) => (ZoneManager.Zone)ZoneManager.Call("GetZoneByID", zoneID);

        private string[] GetPlayerZoneIDs(BasePlayer player) => (string[])ZoneManager.Call("GetPlayerZoneIDs", player);

        private bool IsPlayerInZone(LeftZone leftZone, BasePlayer player) => (bool)ZoneManager.Call("IsPlayerInZone", leftZone.zoneID, player);

        #endregion ZoneManager Integration

        #region Debug

        private readonly StringBuilder debugStringBuilder = new StringBuilder();

        private void PrintDebug(string message, bool warning = false, bool error = false)
        {
            if (configData.global.debugEnabled)
            {
                if (error) PrintError(message);
                else if (warning) PrintWarning(message);
                else Puts(message);
            }

            if (configData.global.logToFile)
            {
                debugStringBuilder.AppendLine($"[{DateTime.Now.ToString(CultureInfo.InstalledUICulture)}] | {message}");
            }
        }

        private void SaveDebug()
        {
            if (!configData.global.logToFile) return;
            var debugText = debugStringBuilder.ToString().Trim();
            debugStringBuilder.Clear();
            if (!string.IsNullOrEmpty(debugText))
            {
                LogToFile("debug", debugText, this);
            }
        }

        #endregion Debug

        #region API

        private string[] AllDynamicPVPZones() => activeDynamicZones.Keys.ToArray();

        private bool IsDynamicPVPZone(string zoneID) => activeDynamicZones.ContainsKey(zoneID);

        private bool EventDataExists(string eventName) => storedData.EventDataExists(eventName);

        private bool IsPlayerInPVPDelay(ulong playerID) => pvpDelays.ContainsKey(playerID);

        private string GetEventName(string zoneID)
        {
            string eventName;
            if (activeDynamicZones.TryGetValue(zoneID, out eventName))
            {
                return eventName;
            }
            return null;
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
                if (autoEventS.autoStart)
                {
                    CreateDynamicZone(eventName, autoEventS.position, autoEventS.zoneID);
                }
            }
            dataChanged = true;
            return true;
        }

        #endregion API

        #region Commands

        private void CmdDynamicPVP(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin && !iPlayer.HasPermission(PERMISSION_ADMIN))
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
                var customEventCount = storedData.CustomEventsCount;
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
                            switch (args[2].ToLower())
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
            bool closed = false;
            foreach (var entry in activeDynamicZones.ToArray())
            {
                if (entry.Value == eventName)
                {
                    if (DeleteDynamicZone(entry.Key))
                    {
                        closed = true;
                    }
                }
            }
            return closed;
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

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version;
        }

        private class GlobalS
        {
            [JsonProperty(PropertyName = "Enable Debug Mode")]
            public bool debugEnabled;

            [JsonProperty(PropertyName = "Log Debug To File")]
            public bool logToFile;

            [JsonProperty(PropertyName = "Compare Radius (Used to determine if it is a SupplySignal)")]
            public float compareRadius = 50f;

            [JsonProperty(PropertyName = "If the entity has an owner, don't create a PVP zone")]
            public bool checkEntityOwner = true;

            [JsonProperty(PropertyName = "PVP Delay Flags")]
            public PVPDelayFlags pvpDelayFlags = PVPDelayFlags.ZonePlayersCanDamageDelayedPlayers | PVPDelayFlags.DelayedPlayersCanDamageDelayedPlayers | PVPDelayFlags.DelayedPlayersCanDamageZonePlayers;
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
            public SupplyDropEventS supplySignal = new SupplyDropEventS();

            [JsonProperty(PropertyName = "Timed Supply Event")]
            public SupplyDropEventS timedSupply = new SupplyDropEventS();

            [JsonProperty(PropertyName = "Hackable Crate Event")]
            public HackEventS hackableCrate = new HackEventS();

            [JsonProperty(PropertyName = "Excavator Ignition Event")]
            public MonumentEventS excavatorIgnition = new MonumentEventS();

            [JsonProperty(PropertyName = "Cargo Ship Event")]
            public CargoShipEventS cargoShip = new CargoShipEventS();
        }

        #region Event

        private class BaseEventS
        {
            [JsonProperty(PropertyName = "Enable Event", Order = 1)]
            public bool enabled;

            [JsonProperty(PropertyName = "Enable PVP Delay", Order = 2)]
            public bool pvpDelayEnabled;

            [JsonProperty(PropertyName = "PVP Delay Time", Order = 3)]
            public float pvpDelayTime = 10f;

            [JsonProperty(PropertyName = "Delay In Starting Event", Order = 6)]
            public float eventStartDelay;

            [JsonProperty(PropertyName = "TruePVE Mapping", Order = 7)]
            public string mapping = "exclude";

            [JsonProperty(PropertyName = "Use Blacklist Commands (If false, a whitelist is used)", Order = 8)]
            public bool useBlacklistCommands = true;

            [JsonProperty(PropertyName = "Command List (If there is a '/' at the front, it is a chat command)", Order = 9)]
            public List<string> commandList = new List<string>();

            public virtual BaseDynamicZoneS GetDynamicZoneS()
            {
                return null;
            }
        }

        private class DomeSphereEventS : BaseEventS, IDomeEvent, ISphereEvent
        {
            public bool domesEnabled { get; set; } = true;
            public int domesDarkness { get; set; } = 8;
            public SphereDynamicZoneS dynamicZoneS { get; set; } = new SphereDynamicZoneS();

            public override BaseDynamicZoneS GetDynamicZoneS()
            {
                return dynamicZoneS;
            }
        }

        private class MonumentEventS : DomeSphereEventS
        {
            [JsonProperty(PropertyName = "Zone ID", Order = 11)]
            public string zoneID = string.Empty;

            [JsonProperty(PropertyName = "Transform Position", Order = 12)]
            public Vector3 transformPosition;
        }

        private class BotDomeSphereEventS : DomeSphereEventS, IBotSpawnEvent
        {
            public bool botsEnabled { get; set; }
            public string botProfileName { get; set; } = string.Empty;
        }

        private class TimedEventS : BotDomeSphereEventS, ITimedEvent
        {
            public float duration { get; set; } = 600f;
        }

        private class AutoEventS : BotDomeSphereEventS
        {
            [JsonProperty(PropertyName = "Auto Start", Order = 13)]
            public bool autoStart;

            [JsonProperty(PropertyName = "Zone ID", Order = 14)]
            public string zoneID = string.Empty;

            [JsonProperty(PropertyName = "Position", Order = 15)]
            public Vector3 position;
        }

        private class HackEventS : TimedEventS
        {
            [JsonProperty(PropertyName = "Hackable Crate Event Start When Spawned (If false, the event starts when unlocking)", Order = 14)]
            public bool spawnStart;

            [JsonProperty(PropertyName = "Excluding Hackable Crate On OilRig", Order = 15)]
            public bool excludeOilRig = true;

            [JsonProperty(PropertyName = "Excluding Hackable Crate on Cargo Ship", Order = 16)]
            public bool excludeCargoShip = true;
        }

        private class SupplyDropEventS : TimedEventS
        {
            [JsonProperty(PropertyName = "Supply Drop Event Start When Spawned (If false, the event starts when landed)", Order = 14)]
            public bool spawnStart = true;
        }

        private class CargoShipEventS : BaseEventS, ICubeEvent
        {
            public CubeDynamicZoneS dynamicZoneS { get; set; } = new CubeDynamicZoneS();

            public override BaseDynamicZoneS GetDynamicZoneS()
            {
                return dynamicZoneS;
            }
        }

        #region Interface

        private interface ITimedEvent
        {
            [JsonProperty(PropertyName = "Event Duration", Order = 13)]
            float duration { get; set; }
        }

        private interface IBotSpawnEvent
        {
            [JsonProperty(PropertyName = "Enable Bots (Need BotSpawn Plugin)", Order = 11)]
            bool botsEnabled { get; set; }

            [JsonProperty(PropertyName = "BotSpawn Profile Name", Order = 12)]
            string botProfileName { get; set; }
        }

        private interface IDomeEvent
        {
            [JsonProperty(PropertyName = "Enable Domes", Order = 4)]
            bool domesEnabled { get; set; }

            [JsonProperty(PropertyName = "Domes Darkness", Order = 5)]
            int domesDarkness { get; set; }
        }

        private interface ISphereEvent
        {
            [JsonProperty(PropertyName = "Dynamic PVP Zone Settings", Order = 10)]
            SphereDynamicZoneS dynamicZoneS { get; set; }
        }

        private interface ICubeEvent
        {
            [JsonProperty(PropertyName = "Dynamic PVP Zone Settings", Order = 10)]
            CubeDynamicZoneS dynamicZoneS { get; set; }
        }

        #endregion Interface

        #endregion Event

        #region Zone

        private class BaseDynamicZoneS
        {
            [JsonProperty(PropertyName = "Zone Comfort", Order = 13)]
            public float comfort;

            [JsonProperty(PropertyName = "Zone Radiation", Order = 14)]
            public float radiation;

            [JsonProperty(PropertyName = "Zone Temperature", Order = 15)]
            public float temperature;

            [JsonProperty(PropertyName = "Eject Spawns", Order = 18)]
            public string ejectSpawns = string.Empty;

            [JsonProperty(PropertyName = "Enter Message", Order = 16)]
            public string enterMessage = "Entering a PVP area!";

            [JsonProperty(PropertyName = "Leave Message", Order = 17)]
            public string leaveMessage = "Leaving a PVP area.";

            [JsonProperty(PropertyName = "Permission Required To Enter Zone", Order = 19)]
            public string permission = string.Empty;

            [JsonProperty(PropertyName = "Extra Zone Flags", Order = 20)]
            public List<string> extraZoneFlags = new List<string>();

            private string[] _zoneSettings;

            [JsonIgnore]
            public string[] ZoneSettings
            {
                get
                {
                    if (_zoneSettings == null)
                    {
                        _zoneSettings = GetZoneSettings();
                    }

                    return _zoneSettings;
                }
            }

            protected void GetZoneSettings(List<string> zoneSettings)
            {
                zoneSettings.Add("name");
                zoneSettings.Add(ZONE_NAME);
                if (comfort > 0f)
                {
                    zoneSettings.Add("comfort");
                    zoneSettings.Add(comfort.ToString(CultureInfo.InvariantCulture));
                }

                if (radiation > 0f)
                {
                    zoneSettings.Add("radiation");
                    zoneSettings.Add(radiation.ToString(CultureInfo.InvariantCulture));
                }

                if (temperature != 0f)
                {
                    zoneSettings.Add("temperature");
                    zoneSettings.Add(temperature.ToString(CultureInfo.InvariantCulture));
                }

                if (!string.IsNullOrEmpty(enterMessage))
                {
                    zoneSettings.Add("enter_message");
                    zoneSettings.Add(enterMessage);
                }
                if (!string.IsNullOrEmpty(leaveMessage))
                {
                    zoneSettings.Add("leave_message");
                    zoneSettings.Add(leaveMessage);
                }
                if (!string.IsNullOrEmpty(ejectSpawns))
                {
                    zoneSettings.Add("ejectspawns");
                    zoneSettings.Add(ejectSpawns);
                }
                if (!string.IsNullOrEmpty(permission))
                {
                    zoneSettings.Add("permission");
                    zoneSettings.Add(permission);
                }
                foreach (var flag in extraZoneFlags)
                {
                    if (string.IsNullOrEmpty(flag)) continue;
                    zoneSettings.Add(flag);
                    zoneSettings.Add("true");
                }
            }

            protected virtual string[] GetZoneSettings()
            {
                return null;
            }
        }

        private class SphereDynamicZoneS : BaseDynamicZoneS, ISphereZone
        {
            public float radius { get; set; } = 100;

            protected override string[] GetZoneSettings()
            {
                var zoneSettings = new List<string> {
                    "radius", radius.ToString(CultureInfo.InvariantCulture)
                };
                GetZoneSettings(zoneSettings);
                var array = zoneSettings.ToArray();
                return array;
            }
        }

        private class CubeDynamicZoneS : BaseDynamicZoneS, ICubeZone, IParentZone
        {
            public Vector3 size { get; set; } = new Vector3(25.9f, 43.3f, 152.8f);
            public Vector3 center { get; set; } = new Vector3(0f, 21.6f, 6.6f);

            protected override string[] GetZoneSettings()
            {
                var zoneSettings = new List<string> {
                    "size", FormatSize()
                };
                GetZoneSettings(zoneSettings);
                var array = zoneSettings.ToArray();
                return array;
            }

            private string FormatSize()
            {
                return $"{size.x} {size.y} {size.z}";
            }
        }

        #region Interface

        public interface ISphereZone
        {
            [JsonProperty(PropertyName = "Zone Radius", Order = 0)]
            float radius { get; set; }
        }

        public interface ICubeZone
        {
            [JsonProperty(PropertyName = "Zone Size", Order = 0)]
            Vector3 size { get; set; }
        }

        public interface IParentZone
        {
            [JsonProperty(PropertyName = "Transform Position", Order = 1)]
            Vector3 center { get; set; }
        }

        #endregion Interface

        #endregion Zone

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                {
                    LoadDefaultConfig();
                }
                else
                {
                    UpdateConfigValues();
                }
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
            configData.version = Version;
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= default(VersionNumber))
                {
                    //string prefix, prefixColor;
                    //if (GetConfigValue(out prefix, "Chat Settings", "Chat Prefix") && GetConfigValue(out prefixColor, "Chat Settings", "Chat Prefix Color"))
                    //{
                    //    configData.chatS.prefix = $"<color={prefixColor}>{prefix}</color>: ";
                    //}
                }

                configData.version = Version;
            }
        }

        private bool GetConfigValue<T>(out T value, params string[] path)
        {
            var configValue = Config.Get(path);
            if (configValue == null)
            {
                value = default(T);
                return false;
            }
            value = Config.ConvertValue<T>(configValue);
            return true;
        }

        #endregion ConfigurationFile

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public readonly Dictionary<string, TimedEventS> timedEvents = new Dictionary<string, TimedEventS>();
            public readonly Dictionary<string, AutoEventS> autoEvents = new Dictionary<string, AutoEventS>();

            public bool EventDataExists(string eventName) => timedEvents.ContainsKey(eventName) || autoEvents.ContainsKey(eventName);

            public bool RemoveEventData(string eventName) => timedEvents.Remove(eventName) || autoEvents.Remove(eventName);

            [JsonIgnore] public int CustomEventsCount => timedEvents.Count + autoEvents.Count;
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

        private void Print(BasePlayer player, string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            Player.Message(player, message, string.IsNullOrEmpty(configData.chatS.prefix) ? null : $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);
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