using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Automated Events", "k1lly0u/mspeedie/Arainrr", "1.0.9")]
    internal class AutomatedEvents : RustPlugin
    {
        #region Fields

        [PluginReference] private Plugin GUIAnnouncements, AlphaChristmas, FancyDrop, PlaneCrash, RustTanic, HeliRefuel, PilotEject;

        private const string PERMISSION_USE = "automatedevents.allowed";
        private const string PERMISSION_NEXT = "automatedevents.next";

        private const string PREFAB_APC = "assets/prefabs/npc/m2bradley/bradleyapc.prefab";
        private const string PREFAB_PLANE = "assets/prefabs/npc/cargo plane/cargo_plane.prefab";
        private const string PREFAB_CHINOOK = "assets/prefabs/npc/ch47/ch47scientists.entity.prefab";
        private const string PREFAB_HELI = "assets/prefabs/npc/patrol helicopter/patrolhelicopter.prefab";
        private const string PREFAB_SHIP = "assets/content/vehicles/boats/cargoship/cargoshiptest.prefab";
        private const string PREFAB_SLEIGH = "assets/prefabs/misc/xmas/sleigh/santasleigh.prefab";
        private const string PREFAB_EASTER = "assets/prefabs/misc/easter/egghunt.prefab";
        private const string PREFAB_HALLOWEEN = "assets/prefabs/misc/halloween/halloweenhunt.prefab";
        private const string PREFAB_CHRISTMAS = "assets/prefabs/misc/xmas/xmasrefill.prefab";

        private static AutomatedEvents instance;
        private Dictionary<EventType, BaseEntity> eventEntities;
        private readonly Dictionary<EventType, Timer> eventTimers = new Dictionary<EventType, Timer>();
        private Dictionary<EventSchedule, EventType> disabledVanillaEvents = new Dictionary<EventSchedule, EventType>();

        private readonly Dictionary<string, EventType> eventSchedulePrefabShortNames = new Dictionary<string, EventType>
        {
            ["event_airdrop"] = EventType.CargoPlane,
            ["event_cargoship"] = EventType.CargoShip,
            ["event_cargoheli"] = EventType.Chinook,
            ["event_helicopter"] = EventType.Helicopter,
            ["event_xmas"] = EventType.Christmas,
            ["event_easter"] = EventType.Easter,
            ["event_halloween"] = EventType.Halloween,
        };

        private enum EventType
        {
            None,
            Bradley,
            CargoPlane,
            CargoShip,
            Chinook,
            Helicopter,
            SantaSleigh,
            Christmas,
            Easter,
            Halloween
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            instance = this;
            permission.RegisterPermission(PERMISSION_USE, this);
            permission.RegisterPermission(PERMISSION_NEXT, this);
            AddCovalenceCommand(configData.chatS.nextEventCommand, nameof(CmdNextEvent));
            AddCovalenceCommand(configData.chatS.runEventCommand, nameof(CmdRunEvent));
            AddCovalenceCommand(configData.chatS.killEventCommand, nameof(CmdKillEvent));

            var eventTypes = new List<EventType>(Enum.GetValues(typeof(EventType)).Cast<EventType>().Where(x => x != EventType.None));
            if (!eventTypes.Any(x =>
            {
                var baseEventS = GetBaseEventS(x);
                return baseEventS.enabled && baseEventS.restartTimerOnKill;
            }))
            {
                Unsubscribe(nameof(OnEntityKill));
            }
            else
            {
                eventEntities = new Dictionary<EventType, BaseEntity>();
            }
        }

        private void OnServerInitialized()
        {
            ClearExistingEvents();
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                if (eventType == EventType.None) continue;
                var baseEventS = GetBaseEventS(eventType);
                switch (eventType)
                {
                    case EventType.Bradley:
                        {
                            var bradleySpawner = BradleySpawner.singleton;
                            if (bradleySpawner != null)
                            {
                                if (baseEventS.disableVanillaEvent)
                                {
                                    ConVar.Bradley.enabled = false;
                                    bradleySpawner.enabled = false;
                                    bradleySpawner.CancelInvoke(nameof(bradleySpawner.DelayedStart));
                                    bradleySpawner.CancelInvoke(nameof(bradleySpawner.CheckIfRespawnNeeded));
                                    Puts($"The vanilla {eventType} event is disabled");
                                }
                            }
                            else if (baseEventS.enabled)
                            {
                                PrintError("There is no Bradley Spawner on your server, so the Bradley event is disabled");
                                continue;
                            }
                        }
                        break;
                }
                if (baseEventS.enabled)
                {
                    eventTimers[eventType] = timer.Once(5f, () => StartEventTimer(eventType, configData.globalS.announceOnLoaded));
                }
            }
        }

        private void Unload()
        {
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                switch (eventType)
                {
                    case EventType.Bradley:
                        {
                            var baseEventS = GetBaseEventS(eventType);
                            var bradleySpawner = BradleySpawner.singleton;
                            if (bradleySpawner != null && baseEventS.disableVanillaEvent)
                            {
                                ConVar.Bradley.enabled = true;
                                bradleySpawner.enabled = true;
                                bradleySpawner.InvokeRepeating(nameof(bradleySpawner.CheckIfRespawnNeeded), 0f, 5f);
                                Puts($"The vanilla {eventType} event is enabled");
                            }
                        }
                        continue;
                }
            }

            if (disabledVanillaEvents != null)
            {
                foreach (var entry in disabledVanillaEvents)
                {
                    entry.Key.enabled = true;
                    Puts($"The vanilla {entry.Value} event is enabled");
                }
            }

            foreach (var value in eventTimers.Values)
            {
                value?.Destroy();
            }

            instance = null;
        }

        private void OnEntityKill(BaseEntity entity)
        {
            if (entity == null) return;
            if (!eventEntities.ContainsValue(entity)) return;
            var eventType = GetEventTypeFromEntity(entity);
            if (eventType == EventType.None) return;
            StartEventTimer(eventType);
        }

        private object OnEventTrigger(TriggeredEventPrefab eventPrefab)
        {
            if (eventPrefab == null) return null;
            EventType eventType;
            var prefabShortName = GetPrefabShortName(eventPrefab.name);
            if (string.IsNullOrEmpty(prefabShortName))
            {
                PrintError($"Failed to get prefab short name ({eventPrefab.name}). Please notify the plugin developer");
                return null;
            }
            if (eventSchedulePrefabShortNames.TryGetValue(prefabShortName, out eventType))
            {
                var baseEventS = GetBaseEventS(eventType);
                if (baseEventS.disableVanillaEvent)
                {
                    PrintWarning($"The vanilla {eventType} event is disabled");
                    var eventSchedule = eventPrefab.GetComponent<EventSchedule>();
                    if (eventSchedule == null)
                    {
                        PrintError($"{eventPrefab.name} has no EventSchedule component. Please notify the plugin developer");
                        return null;
                    }
                    eventSchedule.enabled = false;
                    disabledVanillaEvents.Add(eventSchedule, eventType);
                    return false;
                }
                if (!baseEventS.enabled) return null;
                switch (eventType)
                {
                    case EventType.CargoPlane:
                        if (!CanRunEvent<CargoPlane>(eventType, baseEventS))
                        {
                            return false;
                        }
                        return null;

                    case EventType.CargoShip:
                        if (!CanRunEvent<CargoShip>(eventType, baseEventS))
                        {
                            return false;
                        }
                        return null;

                    case EventType.Chinook:
                        if (!CanRunEvent<CH47HelicopterAIController>(eventType, baseEventS))
                        {
                            return false;
                        }
                        return null;

                    case EventType.Helicopter:
                        if (!CanRunEvent<BaseHelicopter>(eventType, baseEventS))
                        {
                            return false;
                        }
                        return null;

                    case EventType.Christmas:
                        if (!CanRunEvent<XMasRefill>(eventType, baseEventS))
                        {
                            return false;
                        }
                        return null;

                    case EventType.Easter:
                    case EventType.Halloween:
                        return null;
                }
            }
            else PrintError($"Unknown Event Schedule: {eventPrefab.name}");
            return null;
        }

        #endregion Oxide Hooks

        #region Methods

        private void ClearExistingEvents()
        {
            var eventTypes = new Dictionary<EventType, bool>();
            foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
            {
                if (eventType == EventType.None) continue;
                var baseEventS = GetBaseEventS(eventType);
                if (baseEventS.enabled && baseEventS.killEventOnLoaded)
                {
                    var excludePlayerEntity = (baseEventS as CoexistEventS)?.excludePlayerEntity ?? false;
                    eventTypes.Add(eventType, excludePlayerEntity);
                }
            }

            if (eventTypes.Count <= 0) return;
            foreach (var baseEntity in BaseNetworkable.serverEntities.OfType<BaseEntity>().ToArray())
            {
                var eventType = GetEventTypeFromEntity(baseEntity);
                if (eventType == EventType.None) continue;
                bool excludePlayerEntity;
                if (eventTypes.TryGetValue(eventType, out excludePlayerEntity))
                {
                    if (excludePlayerEntity && baseEntity.OwnerID.IsSteamId()) continue;
                    Puts($"Killing a {eventType}");
                    baseEntity.Kill();
                }
            }
        }

        private void StartEventTimer(EventType eventType, bool announce = true, float time = 0f)
        {
            if (eventType == EventType.None)
            {
                return;
            }

            var baseEventS = GetBaseEventS(eventType);
            if (!baseEventS.enabled)
            {
                Puts($"Unable to running {eventType} event, because the event is disabled");
                return;
            }
            var randomTime = time <= 0f
                 ? baseEventS.minimumTimeBetween <= baseEventS.maximumTimeBetween
                     ? UnityEngine.Random.Range(baseEventS.minimumTimeBetween, baseEventS.maximumTimeBetween)
                     : UnityEngine.Random.Range(baseEventS.maximumTimeBetween, baseEventS.minimumTimeBetween)
                 : time;

            var nextDateTime = DateTime.UtcNow.AddMinutes(randomTime);
            baseEventS.nextRunTime = Facepunch.Math.Epoch.FromDateTime(nextDateTime);

            Timer value;
            if (eventTimers.TryGetValue(eventType, out value))
            {
                value?.Destroy();
            }

            eventTimers[eventType] = timer.Once(randomTime * 60f, () => RunEvent(eventType));
            var timeLeft = TimeSpan.FromSeconds(baseEventS.nextRunTime - Facepunch.Math.Epoch.Current).ToShortString();
            Puts($"Next {eventType} event will be ran after {timeLeft}");
            if (announce && baseEventS.announceNext)
            {
                SendMessageToPlayers(eventType, timeLeft);
            }
        }

        private void RunEvent(EventType eventType, bool runOnce = false, bool bypass = false)
        {
            if (eventType == EventType.None)
            {
                return;
            }

            BaseEntity eventEntity = null;
            string eventTypeStr = null;
            var baseEventS = GetBaseEventS(eventType);
            switch (eventType)
            {
                case EventType.Bradley:
                    {
                        if (bypass || CanRunEvent<BradleyAPC>(eventType, baseEventS, false))
                        {
                            var bradleySpawner = BradleySpawner.singleton;
                            if (bradleySpawner == null || bradleySpawner.path?.interestZones == null)
                            {
                                PrintError("There is no Bradley Spawner on your server, so you cannot spawn a Bradley");
                                return;
                            }
                            Puts("Spawning Bradley");
                            var bradley = GameManager.server.CreateEntity(PREFAB_APC) as BradleyAPC;
                            if (bradley == null)
                            {
                                PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                return;
                            }
                            bradley.Spawn();
                            eventEntity = bradley;
                            eventTypeStr = eventType.ToString();

                            var position = bradleySpawner.path.interestZones[UnityEngine.Random.Range(0, bradleySpawner.path.interestZones.Count)].transform.position;
                            bradley.transform.position = position;
                            bradley.DoAI = true;
                            bradley.InstallPatrolPath(bradleySpawner.path);
                            //bradleySpawner.CancelInvoke(nameof(bradleySpawner.CheckIfRespawnNeeded));
                        }
                    }
                    break;

                case EventType.CargoPlane:
                    {
                        if (bypass || CanRunEvent<CargoPlane>(eventType, baseEventS, false))
                        {
                            var planeEventS = baseEventS as PlaneEventS;
                            var weightDict = new Dictionary<int, float>();
                            if (planeEventS.normalWeight > 0)
                            {
                                weightDict.Add(0, planeEventS.normalWeight);
                            }
                            if (planeEventS.fancyDropWeight > 0 && FancyDrop != null)
                            {
                                weightDict.Add(1, planeEventS.fancyDropWeight);
                            }
                            if (planeEventS.planeCrashWeight > 0 && PlaneCrash != null)
                            {
                                weightDict.Add(2, planeEventS.planeCrashWeight);
                            }

                            var index = GetEventIndexFromWeight(weightDict);
                            switch (index)
                            {
                                case 0:
                                    Puts("Spawning Cargo Plane");
                                    var plane = GameManager.server.CreateEntity(PREFAB_PLANE) as CargoPlane;
                                    if (plane == null)
                                    {
                                        PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                        return;
                                    }
                                    plane.Spawn();
                                    eventEntity = plane;
                                    eventTypeStr = eventType.ToString();
                                    break;

                                case 1:
                                    Puts("Spawning FancyDrop Cargo Plane");
                                    rust.RunServerCommand("ad.random");
                                    eventTypeStr = "FancyDrop";
                                    break;

                                case 2:
                                    Puts("Spawning PlaneCrash Cargo Plane");
                                    rust.RunServerCommand("callcrash");
                                    eventTypeStr = "PlaneCrash";
                                    break;
                            }
                        }
                    }
                    break;

                case EventType.CargoShip:
                    {
                        if (bypass || CanRunEvent<CargoShip>(eventType, baseEventS, false))
                        {
                            var cargoShipEventS = baseEventS as ShipEventS;
                            var weightDict = new Dictionary<int, float>();
                            if (cargoShipEventS.normalWeight > 0)
                            {
                                weightDict.Add(0, cargoShipEventS.normalWeight);
                            }
                            if (cargoShipEventS.rustTanicWeight > 0 && RustTanic != null)
                            {
                                weightDict.Add(1, cargoShipEventS.rustTanicWeight);
                            }
                            var index = GetEventIndexFromWeight(weightDict);
                            switch (index)
                            {
                                case 0:
                                    Puts("Spawning Cargo Ship");
                                    var ship = GameManager.server.CreateEntity(PREFAB_SHIP) as CargoShip;
                                    if (ship == null)
                                    {
                                        PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                        return;
                                    }
                                    ship.TriggeredEventSpawn();
                                    ship.Spawn();
                                    eventEntity = ship;
                                    eventTypeStr = eventType.ToString();
                                    break;

                                case 1:
                                    Puts("Spawning RustTanic Cargo Ship");
                                    rust.RunServerCommand("calltitanic");
                                    eventTypeStr = "RustTanic";
                                    break;
                            }
                        }
                    }
                    break;

                case EventType.Chinook:
                    {
                        if (bypass || CanRunEvent<CH47HelicopterAIController>(eventType, baseEventS, false, entity => entity.landingTarget == Vector3.zero))
                        {
                            Puts("Spawning Chinook");
                            var chinook = GameManager.server.CreateEntity(PREFAB_CHINOOK) as CH47HelicopterAIController;
                            if (chinook == null)
                            {
                                PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                return;
                            }

                            chinook.TriggeredEventSpawn();
                            chinook.Spawn();
                            eventEntity = chinook;
                            eventTypeStr = eventType.ToString();
                        }
                    }
                    break;

                case EventType.Helicopter:
                    {
                        if (bypass || CanRunEvent<BaseHelicopter>(eventType, baseEventS, false))
                        {
                            var heliEventS = baseEventS as HeliEventS;
                            var weightDict = new Dictionary<int, float>();
                            if (heliEventS.normalWeight > 0)
                            {
                                weightDict.Add(0, heliEventS.normalWeight);
                            }
                            if (heliEventS.pilotEjectWeight > 0 && PilotEject != null)
                            {
                                weightDict.Add(1, heliEventS.pilotEjectWeight);
                            }
                            if (heliEventS.heliRefuelWeight > 0 && HeliRefuel != null)
                            {
                                weightDict.Add(2, heliEventS.heliRefuelWeight);
                            }

                            var index = GetEventIndexFromWeight(weightDict);
                            switch (index)
                            {
                                case 0:
                                    Puts("Spawning Helicopter");
                                    var helicopter = GameManager.server.CreateEntity(PREFAB_HELI) as BaseHelicopter;
                                    if (helicopter == null)
                                    {
                                        PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                        return;
                                    }
                                    helicopter.Spawn();
                                    eventEntity = helicopter;
                                    eventTypeStr = eventType.ToString();
                                    break;

                                case 1:
                                    Puts("Spawning PilotEject Helicopter");
                                    rust.RunServerCommand("pe call");
                                    eventTypeStr = "PilotEject";
                                    break;

                                case 2:
                                    Puts("Spawning HeliRefuel Helicopter");
                                    rust.RunServerCommand("hr call");
                                    eventTypeStr = "HeliRefuel";
                                    break;
                            }
                        }
                    }
                    break;

                case EventType.SantaSleigh:
                    {
                        if (bypass || CanRunEvent<SantaSleigh>(eventType, baseEventS, false))
                        {
                            Puts("Santa Sleigh is coming, have you been good?");
                            var santaSleigh = GameManager.server.CreateEntity(PREFAB_SLEIGH) as SantaSleigh;
                            if (santaSleigh == null)
                            {
                                PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                return;
                            }

                            santaSleigh.Spawn();
                            eventEntity = santaSleigh;
                            eventTypeStr = eventType.ToString();
                        }
                    }
                    break;

                case EventType.Christmas:
                    {
                        if (bypass || CanRunEvent<XMasRefill>(eventType, baseEventS, false))
                        {
                            var christmasEventS = baseEventS as ChristmasEventS;
                            var weightDict = new Dictionary<int, float>();
                            if (christmasEventS.normalWeight > 0)
                            {
                                weightDict.Add(0, christmasEventS.normalWeight);
                            }
                            if (christmasEventS.alphaChristmasWeight > 0 && AlphaChristmas != null)
                            {
                                weightDict.Add(1, christmasEventS.alphaChristmasWeight);
                            }
                            var index = GetEventIndexFromWeight(weightDict);
                            switch (index)
                            {
                                case 0:
                                    Puts("Christmas Refill is occurring");
                                    var xMasRefill = GameManager.server.CreateEntity(PREFAB_CHRISTMAS) as XMasRefill;
                                    if (xMasRefill == null)
                                    {
                                        PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                                        return;
                                    }

                                    bool flag = ConVar.XMas.enabled;
                                    if (!flag) ConVar.XMas.enabled = true;
                                    xMasRefill.Spawn();
                                    xMasRefill.Invoke(() => ConVar.XMas.enabled = flag, 0.5f);
                                    eventEntity = xMasRefill;
                                    eventTypeStr = eventType.ToString();
                                    break;

                                case 1:
                                    Puts("Running AlphaChristmas Refill");
                                    rust.RunServerCommand("alphachristmas.refill");
                                    eventTypeStr = "AlphaChristmas";
                                    break;
                            }
                        }
                    }
                    break;

                case EventType.Easter:
                    {
                        if (EggHuntEvent.serverEvent != null)//EggHuntEvent.serverEvent.IsEventActive()
                        {
                            var timeLeft = EggHuntEvent.durationSeconds - EggHuntEvent.serverEvent.timeAlive + EggHuntEvent.serverEvent.warmupTime + 60f;
                            PrintWarning($"There is an {(EggHuntEvent.serverEvent.ShortPrefabName == "egghunt" ? eventType : EventType.Halloween)} event running, so the {eventType} event will be delayed until {Mathf.RoundToInt(timeLeft)} seconds later");
                            if (!runOnce)
                            {
                                StartEventTimer(eventType, true, timeLeft / 60f);
                            }
                            return;
                        }

                        Puts("Happy Easter Egg Hunt is occurring");
                        var eggHuntEvent = GameManager.server.CreateEntity(PREFAB_EASTER) as EggHuntEvent;
                        if (eggHuntEvent == null)
                        {
                            PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                            return;
                        }

                        eggHuntEvent.Spawn();
                        eventEntity = eggHuntEvent;
                        eventTypeStr = eventType.ToString();
                    }
                    break;

                case EventType.Halloween:
                    {
                        if (EggHuntEvent.serverEvent != null)//EggHuntEvent.serverEvent.IsEventActive()
                        {
                            var timeLeft = EggHuntEvent.durationSeconds - EggHuntEvent.serverEvent.timeAlive + EggHuntEvent.serverEvent.warmupTime + 60f;
                            PrintWarning($"There is an {(EggHuntEvent.serverEvent.ShortPrefabName == "egghunt" ? EventType.Easter : eventType)} event running, so the {eventType} event will be delayed until {Mathf.RoundToInt(timeLeft)} seconds later");
                            if (!runOnce)
                            {
                                StartEventTimer(eventType, true, timeLeft / 60f);
                            }
                            return;
                        }

                        Puts("Spooky Halloween Hunt is occurring");
                        var halloweenHunt = GameManager.server.CreateEntity(PREFAB_HALLOWEEN) as HalloweenHunt;
                        if (halloweenHunt == null)
                        {
                            PrintError($"{eventType} prefab does not exist. Please notify the plugin developer");
                            return;
                        }

                        halloweenHunt.Spawn();
                        eventEntity = halloweenHunt;
                        eventTypeStr = eventType.ToString();
                    }
                    break;

                default:
                    PrintError($"RunEvent: Unknown EventType: {eventType}");
                    return;
            }

            if (eventEntity != null && baseEventS.enabled && baseEventS.restartTimerOnKill)
            {
                eventEntities[eventType] = eventEntity;
            }
            if (!string.IsNullOrEmpty(eventTypeStr))
            {
                Interface.CallHook("OnAutoEventTriggered", eventTypeStr, eventEntity, runOnce);
            }
            if (!runOnce)
            {
                StartEventTimer(eventType);
            }
        }

        private void KillEvent(EventType eventType)
        {
            var baseEventS = GetBaseEventS(eventType);
            switch (eventType)
            {
                case EventType.Bradley:
                    foreach (var bradley in GetEventEntities<BradleyAPC>(baseEventS).ToArray())
                    {
                        Puts("Killing a Bradley");
                        bradley.Kill();
                    }
                    return;

                case EventType.CargoPlane:
                    foreach (var cargoPlane in GetEventEntities<CargoPlane>(baseEventS).ToArray())
                    {
                        Puts("Killing a Cargo Plane");
                        cargoPlane.Kill();
                    }
                    return;

                case EventType.CargoShip:
                    foreach (var cargoShip in GetEventEntities<CargoShip>(baseEventS).ToArray())
                    {
                        Puts("Killing a Cargo Ship");
                        cargoShip.Kill();
                    }
                    return;

                case EventType.Chinook:
                    foreach (var ch47Helicopter in GetEventEntities<CH47HelicopterAIController>(baseEventS, entity => entity.landingTarget == Vector3.zero).ToArray())
                    {
                        Puts("Killing a Chinook (CH47)");
                        ch47Helicopter.Kill();
                    }
                    return;

                case EventType.Helicopter:
                    foreach (var helicopter in GetEventEntities<BaseHelicopter>(baseEventS).ToArray())
                    {
                        Puts("Killing a Helicopter");
                        helicopter.Kill();
                    }
                    return;

                case EventType.SantaSleigh:
                    foreach (var santaSleigh in GetEventEntities<SantaSleigh>(baseEventS).ToArray())
                    {
                        Puts("Killing a Santa Sleigh");
                        santaSleigh.Kill();
                    }
                    return;

                case EventType.Christmas:
                    foreach (var christmas in GetEventEntities<XMasRefill>(baseEventS).ToArray())
                    {
                        Puts("Killing a Christmas");
                        christmas.Kill();
                    }
                    return;

                case EventType.Easter:
                    foreach (var easter in GetEventEntities<EggHuntEvent>(baseEventS, entity => entity.ShortPrefabName == "egghunt").ToArray())
                    {
                        Puts("Killing a Easter");
                        easter.Kill();
                    }
                    return;

                case EventType.Halloween:
                    foreach (var halloween in GetEventEntities<HalloweenHunt>(baseEventS).ToArray())
                    {
                        Puts("Killing a Halloween");
                        halloween.Kill();
                    }
                    return;

                default:
                    PrintError($"KillEvent: Unknown EventType: {eventType}");
                    return;
            }
        }

        private bool GetNextEventRunTime(IPlayer iPlayer, EventType eventType, out string nextTime)
        {
            var baseEventS = GetBaseEventS(eventType);
            if (!baseEventS.enabled || baseEventS.nextRunTime <= 0)
            {
                nextTime = Lang("NotSet", iPlayer.Id, baseEventS.displayName);
                return false;
            }
            var timeLeft = TimeSpan.FromSeconds(baseEventS.nextRunTime - Facepunch.Math.Epoch.Current).ToShortString();
            nextTime = Lang("NextRunTime", iPlayer.Id, baseEventS.displayName, timeLeft);
            return true;
        }

        private BaseEventS GetBaseEventS(EventType eventType)
        {
            switch (eventType)
            {
                case EventType.Bradley: return configData.events.bradleyEventS;
                case EventType.CargoPlane: return configData.events.planeEventS;
                case EventType.CargoShip: return configData.events.shipEventS;
                case EventType.Chinook: return configData.events.chinookEventS;
                case EventType.Helicopter: return configData.events.helicopterEventS;
                case EventType.SantaSleigh: return configData.events.santaSleighEventS;
                case EventType.Christmas: return configData.events.christmasEventS;
                case EventType.Easter: return configData.events.easterEventS;
                case EventType.Halloween: return configData.events.halloweenEventS;
                default: PrintError($"GetBaseEventS: Unknown EventType: {eventType}"); return null;
            }
        }

        private string GetEventTypeDisplayName(EventType eventType)
        {
            if (eventType == EventType.None) return "None";
            var baseEventS = GetBaseEventS(eventType);
            return baseEventS.displayName;
        }

        private void SendMessageToPlayers(EventType eventType, string timeLeft)
        {
            if (configData.globalS.useGUIAnnouncements && GUIAnnouncements != null)
            {
                foreach (var player in BasePlayer.activePlayerList)
                {
                    GUIAnnouncements.Call("CreateAnnouncement", Lang("NextRunTime", player.UserIDString, GetEventTypeDisplayName(eventType), timeLeft), "Purple", "White", player);
                }
            }
            else
            {
                foreach (var player in BasePlayer.activePlayerList)
                {
                    Print(player, Lang("NextRunTime", player.UserIDString, GetEventTypeDisplayName(eventType), timeLeft));
                }
            }
        }

        #endregion Methods

        #region Commands

        private void CmdNextEvent(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin && !iPlayer.HasPermission(PERMISSION_NEXT))
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id, command));
                return;
            }
            if (args == null || args.Length < 1)
            {
                Print(iPlayer, Lang("BlankEvent", iPlayer.Id));
                return;
            }

            var argString = args[0].ToLower();
            switch (argString)
            {
                case "*":
                case "all":
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine();
                        foreach (EventType eventType in Enum.GetValues(typeof(EventType)))
                        {
                            if (eventType == EventType.None) continue;
                            string result;
                            if (GetNextEventRunTime(iPlayer, eventType, out result))
                            {
                                stringBuilder.AppendLine(result);
                            }
                        }
                        Print(iPlayer, stringBuilder.ToString());
                    }
                    return;

                default:
                    {
                        var eventType = GetEventTypeFromStr(argString);
                        if (eventType == EventType.None)
                        {
                            Print(iPlayer, Lang("UnknownEvent", iPlayer.Id, args[0]));
                            return;
                        }
                        string result;
                        GetNextEventRunTime(iPlayer, eventType, out result);
                        Print(iPlayer, result);
                    }
                    return;
            }
        }

        private void CmdRunEvent(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin && !iPlayer.HasPermission(PERMISSION_USE))
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id, command));
                return;
            }
            if (args == null || args.Length < 1)
            {
                Print(iPlayer, Lang("BlankEvent", iPlayer.Id));
                return;
            }

            var eventType = GetEventTypeFromStr(args[0].ToLower());
            if (eventType == EventType.None)
            {
                Print(iPlayer, Lang("UnknownEvent", iPlayer.Id, args[0]));
                return;
            }
            Print(iPlayer, Lang("Running", iPlayer.Id, iPlayer.Name, GetEventTypeDisplayName(eventType)));
            RunEvent(eventType, true, true);
        }

        private void CmdKillEvent(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin && !iPlayer.HasPermission(PERMISSION_USE))
            {
                Print(iPlayer, Lang("NotAllowed", iPlayer.Id, command));
                return;
            }
            if (args == null || args.Length < 1)
            {
                Print(iPlayer, Lang("BlankEvent", iPlayer.Id));
                return;
            }

            var eventType = GetEventTypeFromStr(args[0].ToLower());
            if (eventType == EventType.None)
            {
                Print(iPlayer, Lang("UnknownEvent", iPlayer.Id, args[0]));
                return;
            }
            Print(iPlayer, Lang("Removing", iPlayer.Id, iPlayer.Name, GetEventTypeDisplayName(eventType)));
            KillEvent(eventType);
        }

        #endregion Commands

        #region Helpers

        private static string GetPrefabShortName(string prefabName) => Utility.GetFileNameWithoutExtension(prefabName);

        private static EventType GetEventTypeFromStr(string eventTypeStr)
        {
            if (eventTypeStr.Contains("brad"))
                return EventType.Bradley;
            if (eventTypeStr.Contains("heli") || eventTypeStr.Contains("copter"))
                return EventType.Helicopter;
            if (eventTypeStr.Contains("plane"))
                return EventType.CargoPlane;
            if (eventTypeStr.Contains("ship"))
                return EventType.CargoShip;
            if (eventTypeStr.Contains("ch47") || eventTypeStr.Contains("chin"))
                return EventType.Chinook;
            if (eventTypeStr.Contains("xmas") || eventTypeStr.Contains("chris") || eventTypeStr.Contains("yule"))
                return EventType.Christmas;
            if (eventTypeStr.Contains("santa") || eventTypeStr.Contains("nick") || eventTypeStr.Contains("wodan"))
                return EventType.SantaSleigh;
            if (eventTypeStr.Contains("easter") || eventTypeStr.Contains("egg") || eventTypeStr.Contains("bunny"))
                return EventType.Easter;
            if (eventTypeStr.Contains("hall") || eventTypeStr.Contains("spooky") || eventTypeStr.Contains("candy") || eventTypeStr.Contains("samhain"))
                return EventType.Halloween;
            return EventType.None;
        }

        private static EventType GetEventTypeFromEntity(BaseEntity baseEntity)
        {
            if (baseEntity is BradleyAPC) return EventType.Bradley;
            if (baseEntity is CargoPlane) return EventType.CargoPlane;
            if (baseEntity is CargoShip) return EventType.CargoShip;
            if (baseEntity is BaseHelicopter) return EventType.Helicopter;
            if (baseEntity is SantaSleigh) return EventType.SantaSleigh;
            if (baseEntity is XMasRefill) return EventType.Christmas;
            if (baseEntity is HalloweenHunt) return EventType.Halloween;
            if (baseEntity is EggHuntEvent) return EventType.Easter;
            var controller = baseEntity as CH47HelicopterAIController;
            if (controller != null && controller.landingTarget == Vector3.zero) return EventType.Chinook;
            return EventType.None;
        }

        private static int GetEventIndexFromWeight(Dictionary<int, float> weightDict)
        {
            if (weightDict.Count <= 0) return 0;
            if (weightDict.Count == 1) return weightDict.Keys.FirstOrDefault();
            var sum = weightDict.Sum(x => x.Value);
            var rand = UnityEngine.Random.Range(0f, sum);
            foreach (var entry in weightDict)
            {
                if ((rand -= entry.Value) <= 0f)
                {
                    return entry.Key;
                }
            }
            return 0;
        }

        private static IEnumerable<T> GetEventEntities<T>(BaseEventS baseEventS, Func<T, bool> filter = null) where T : BaseEntity
        {
            var excludePlayerEntity = (baseEventS as CoexistEventS)?.excludePlayerEntity ?? false;
            foreach (var serverEntity in BaseNetworkable.serverEntities)
            {
                var entity = serverEntity as T;
                if (entity == null) continue;
                if (excludePlayerEntity && entity.OwnerID.IsSteamId()) continue;
                if (filter != null && !filter(entity)) continue;
                yield return entity;
            }
        }

        private static bool CanRunEvent<T>(EventType eventType, BaseEventS baseEventS, bool vanilla = true, Func<T, bool> filter = null) where T : BaseEntity
        {
            var onlinePlayers = BasePlayer.activePlayerList.Count;
            if (baseEventS.minimumOnlinePlayers > 0 && onlinePlayers < baseEventS.minimumOnlinePlayers)
            {
                instance?.PrintWarning($"The online players is less than {baseEventS.minimumOnlinePlayers}, so the {eventType} {(vanilla ? "vanilla" : "auto")} event cannot run");
                return false;
            }
            if (baseEventS.maximumOnlinePlayers > 0 && onlinePlayers > baseEventS.maximumOnlinePlayers)
            {
                instance?.PrintWarning($"The online players is greater than {baseEventS.maximumOnlinePlayers}, so the {eventType} {(vanilla ? "vanilla" : "auto")} event cannot run");
                return false;
            }

            var coexistEventS = baseEventS as CoexistEventS;
            if (coexistEventS != null)
            {
                if (BaseNetworkable.serverEntities.Count(x =>
                {
                    var entity = x as T;
                    if (entity == null) return false;
                    if (filter != null && !filter(entity)) return false;
                    return !coexistEventS.excludePlayerEntity || !entity.OwnerID.IsSteamId();
                }) >= coexistEventS.serverMaximumNumber)
                {
                    instance?.PrintWarning($"The number of {eventType} {(vanilla ? "vanilla" : "auto")} events has reached the limit of {coexistEventS.serverMaximumNumber}");
                    return false;
                }
            }
            return true;
        }

        #endregion Helpers

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public Settings globalS = new Settings();

            public class Settings
            {
                [JsonProperty(PropertyName = "Announce On Plugin Loaded")]
                public bool announceOnLoaded;

                [JsonProperty(PropertyName = "Use GUIAnnouncements Plugin")]
                public bool useGUIAnnouncements;
            }

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Next Event Command")]
                public string nextEventCommand = "nextevent";

                [JsonProperty(PropertyName = "Run Event Command")]
                public string runEventCommand = "runevent";

                [JsonProperty(PropertyName = "Kill Event Command")]
                public string killEventCommand = "killevent";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "[AutomatedEvents]: ";

                [JsonProperty(PropertyName = "Chat Prefix Color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Event Settings")]
            public EventSettings events = new EventSettings();

            public class EventSettings
            {
                [JsonProperty(PropertyName = "Bradley Event")]
                public CoexistEventS bradleyEventS = new CoexistEventS
                {
                    displayName = "Bradley",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 45
                };

                [JsonProperty(PropertyName = "Cargo Plane Event")]
                public PlaneEventS planeEventS = new PlaneEventS
                {
                    displayName = "Cargo Plane",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 45
                };

                [JsonProperty(PropertyName = "Cargo Ship Event")]
                public ShipEventS shipEventS = new ShipEventS
                {
                    displayName = "Cargo Ship",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 45
                };

                [JsonProperty(PropertyName = "Chinook (CH47) Event")]
                public CoexistEventS chinookEventS = new CoexistEventS
                {
                    displayName = "Chinook",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 45
                };

                [JsonProperty(PropertyName = "Helicopter Event")]
                public HeliEventS helicopterEventS = new HeliEventS
                {
                    displayName = "Helicopter",
                    minimumTimeBetween = 45,
                    maximumTimeBetween = 60
                };

                [JsonProperty(PropertyName = "Santa Sleigh Event")]
                public CoexistEventS santaSleighEventS = new CoexistEventS
                {
                    displayName = "Santa Sleigh",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 60
                };

                [JsonProperty(PropertyName = "Christmas Event")]
                public ChristmasEventS christmasEventS = new ChristmasEventS
                {
                    displayName = "Christmas",
                    minimumTimeBetween = 60,
                    maximumTimeBetween = 120
                };

                [JsonProperty(PropertyName = "Easter Event")]
                public BaseEventS easterEventS = new BaseEventS
                {
                    displayName = "Easter",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 60
                };

                [JsonProperty(PropertyName = "Halloween Event")]
                public BaseEventS halloweenEventS = new BaseEventS
                {
                    displayName = "Halloween",
                    minimumTimeBetween = 30,
                    maximumTimeBetween = 60
                };
            }
        }

        private class BaseEventS
        {
            [JsonProperty(PropertyName = "Enabled", Order = 1)]
            public bool enabled;

            [JsonProperty(PropertyName = "Display Name", Order = 2)]
            public string displayName;

            [JsonProperty(PropertyName = "Disable Vanilla Event", Order = 3)]
            public bool disableVanillaEvent;

            [JsonProperty(PropertyName = "Minimum Time Between (Minutes)", Order = 4)]
            public float minimumTimeBetween;

            [JsonProperty(PropertyName = "Maximum Time Between (Minutes)", Order = 5)]
            public float maximumTimeBetween;

            [JsonProperty(PropertyName = "Minimum Online Players Required (0 = Disabled)", Order = 6)]
            public int minimumOnlinePlayers = 0;

            [JsonProperty(PropertyName = "Maximum Online Players Required (0 = Disabled)", Order = 7)]
            public int maximumOnlinePlayers = 0;

            [JsonProperty(PropertyName = "Announce Next Run Time", Order = 8)]
            public bool announceNext;

            [JsonProperty(PropertyName = "Restart Timer On Entity Kill", Order = 9)]
            public bool restartTimerOnKill = true;

            [JsonProperty(PropertyName = "Kill Existing Event On Plugin Loaded", Order = 10)]
            public bool killEventOnLoaded;

            [JsonIgnore] public double nextRunTime;
        }

        private class CoexistEventS : BaseEventS
        {
            [JsonProperty(PropertyName = "Maximum Number On Server", Order = 11)]
            public int serverMaximumNumber = 1;

            [JsonProperty(PropertyName = "Exclude Player's Entity", Order = 12)]
            public bool excludePlayerEntity = true;
        }

        private class PlaneEventS : CoexistEventS
        {
            [JsonProperty(PropertyName = "Normal Event Weight (0 = Disable)", Order = 21)]
            public float normalWeight = 60;

            [JsonProperty(PropertyName = "FancyDrop Plugin Event Weight (0 = Disable)", Order = 22)]
            public float fancyDropWeight = 20;

            [JsonProperty(PropertyName = "PlaneCrash Plugin Event Weight (0 = Disable)", Order = 23)]
            public float planeCrashWeight = 20;
        }

        private class ShipEventS : CoexistEventS
        {
            [JsonProperty(PropertyName = "Normal Event Weight (0 = Disable)", Order = 21)]
            public float normalWeight = 80;

            [JsonProperty(PropertyName = "RustTanic Plugin Event Weight (0 = Disable)", Order = 22)]
            public float rustTanicWeight = 20;
        }

        private class HeliEventS : CoexistEventS
        {
            [JsonProperty(PropertyName = "Normal Event Weight (0 = Disable)", Order = 21)]
            public float normalWeight = 60;

            [JsonProperty(PropertyName = "HeliRefuel Plugin Event Weight (0 = Disable)", Order = 22)]
            public float heliRefuelWeight = 20;

            [JsonProperty(PropertyName = "PilotEject Plugin Event Weight (0 = Disable)", Order = 23)]
            public float pilotEjectWeight = 20;
        }

        private class ChristmasEventS : CoexistEventS
        {
            [JsonProperty(PropertyName = "Normal Event Weight (0 = Disable)", Order = 21)]
            public float normalWeight = 20;

            [JsonProperty(PropertyName = "AlphaChristmas Plugin Event Weight (0 = Disable)", Order = 22)]
            public float alphaChristmasWeight = 80;
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

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message,
                string.IsNullOrEmpty(configData.chatS.prefix)
                    ? string.Empty
                    : $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>",
                configData.chatS.steamIDIcon);
        }

        private void Print(IPlayer iPlayer, string message)
        {
            iPlayer.Reply(message,
                iPlayer.Id == "server_console"
                    ? $"{configData.chatS.prefix}"
                    : $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>");
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You are not allowed to use the '{0}' command",
                ["BlankEvent"] = "You need to specify an event",
                ["UnknownEvent"] = "'{0}' is an unknown event type",
                ["NotSet"] = "'{0}' is not set to run via Automated Events",
                ["NextRunTime"] = "Next '{0}' event will be ran after {1}",
                ["Running"] = "'{0}' attempting to run automated event: {1}",
                ["Removing"] = "'{0}' attempting to remove any current running event: {1}",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有使用 '{0}' 命令的权限",
                ["BlankEvent"] = "您需要指定一个事件的类型",
                ["UnknownEvent"] = "'{0}' 是一个未知的事件类型",
                ["NotSet"] = "'{0}' 事件没有启用",
                ["NextRunTime"] = "下次 '{0}' 事件将在 {1} 后运行",
                ["Running"] = "'{0}' 通过命令运行了 {1} 事件",
                ["Removing"] = "'{0}' 通过命令删除了 {1} 事件",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}