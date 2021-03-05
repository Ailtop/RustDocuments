//#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Facepunch;
using Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Rust.Modular;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Vehicle Licence", "Sorrow/TheDoc/Arainrr", "1.8.0")]
    [Description("Allows players to buy vehicles and then spawn or store it")]
    public class VehicleLicence : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Economics, ServerRewards, Friends, Clans, NoEscape, LandOnCargoShip;

        private const string PERMISSION_USE = "vehiclelicence.use";
        private const string PERMISSION_ALL = "vehiclelicence.all";
        private const string PERMISSION_BYPASS_COST = "vehiclelicence.bypasscost";

        private const int ITEMID_FUEL = -946369541;
        private const string PREFAB_ITEM_DROP = "assets/prefabs/misc/item drop/item_drop.prefab";

        private const string PREFAB_ROWBOAT = "assets/content/vehicles/boats/rowboat/rowboat.prefab";
        private const string PREFAB_RHIB = "assets/content/vehicles/boats/rhib/rhib.prefab";
        private const string PREFAB_SEDAN = "assets/content/vehicles/sedan_a/sedantest.entity.prefab";
        private const string PREFAB_HOTAIRBALLOON = "assets/prefabs/deployable/hot air balloon/hotairballoon.prefab";
        private const string PREFAB_MINICOPTER = "assets/content/vehicles/minicopter/minicopter.entity.prefab";
        private const string PREFAB_TRANSPORTCOPTER = "assets/content/vehicles/scrap heli carrier/scraptransporthelicopter.prefab";
        private const string PREFAB_CHINOOK = "assets/prefabs/npc/ch47/ch47.entity.prefab";
        private const string PREFAB_RIDABLEHORSE = "assets/rust.ai/nextai/testridablehorse.prefab";

        private const string PREFAB_CHASSIS_SMALL = "assets/content/vehicles/modularcar/car_chassis_2module.entity.prefab";
        private const string PREFAB_CHASSIS_MEDIUM = "assets/content/vehicles/modularcar/car_chassis_3module.entity.prefab";
        private const string PREFAB_CHASSIS_LARGE = "assets/content/vehicles/modularcar/car_chassis_4module.entity.prefab";

        private const int LAYER_GROUND = Rust.Layers.Solid | Rust.Layers.Mask.Water;

        private Timer checkVehiclesTimer;
        private static readonly object True = true, False = false;
        public static VehicleLicence Instance { get; private set; }
        public readonly Dictionary<BaseEntity, Vehicle> vehiclesCache = new Dictionary<BaseEntity, Vehicle>();
        public readonly Dictionary<string, BaseVehicleS> allBaseVehicleSettings = new Dictionary<string, BaseVehicleS>();
        public readonly Dictionary<string, string> commandToVehicleType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public enum NormalVehicleType
        {
            Rowboat,
            RHIB,
            Sedan,
            HotAirBalloon,
            MiniCopter,
            TransportHelicopter,
            Chinook,
            RidableHorse,
        }

        public enum ChassisType
        {
            Small,
            Medium,
            Large
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            Instance = this;
            permission.RegisterPermission(PERMISSION_USE, this);
            permission.RegisterPermission(PERMISSION_ALL, this);
            permission.RegisterPermission(PERMISSION_BYPASS_COST, this);

            foreach (NormalVehicleType value in Enum.GetValues(typeof(NormalVehicleType)))
            {
                allBaseVehicleSettings.Add(value.ToString(), GetBaseVehicleS(value));
            }
            foreach (var entry in configData.modularCarS)
            {
                allBaseVehicleSettings.Add(entry.Key, entry.Value);
            }

            foreach (var entry in allBaseVehicleSettings)
            {
                var baseVehicleS = entry.Value;
                if (baseVehicleS.usePermission && !string.IsNullOrEmpty(baseVehicleS.permission) &&
                    !permission.PermissionExists(baseVehicleS.permission, this))
                {
                    permission.RegisterPermission(baseVehicleS.permission, this);
                }

                foreach (var perm in baseVehicleS.cooldownPermissions.Keys)
                {
                    if (!permission.PermissionExists(perm, this))
                    {
                        permission.RegisterPermission(perm, this);
                    }
                }
                foreach (var command in baseVehicleS.commands)
                {
                    if (string.IsNullOrEmpty(command)) continue;
                    if (!commandToVehicleType.ContainsKey(command))
                    {
                        commandToVehicleType.Add(command, entry.Key);
                    }
                    else
                    {
                        PrintError($"You have the same two commands({command}).");
                    }
                    if (configData.chatS.useUniversalCommand)
                    {
                        cmd.AddChatCommand(command, this, nameof(CmdUniversal));
                    }

                    if (!string.IsNullOrEmpty(configData.chatS.customKillCommandPrefix))
                    {
                        cmd.AddChatCommand(configData.chatS.customKillCommandPrefix + command, this, nameof(CmdCustomKill));
                    }
                }
            }

            cmd.AddChatCommand(configData.chatS.helpCommand, this, nameof(CmdLicenseHelp));
            cmd.AddChatCommand(configData.chatS.buyCommand, this, nameof(CmdBuyVehicle));
            cmd.AddChatCommand(configData.chatS.spawnCommand, this, nameof(CmdSpawnVehicle));
            cmd.AddChatCommand(configData.chatS.recallCommand, this, nameof(CmdRecallVehicle));
            cmd.AddChatCommand(configData.chatS.killCommand, this, nameof(CmdKillVehicle));
            Unsubscribe(nameof(CanMountEntity));
            Unsubscribe(nameof(OnEntityTakeDamage));
            Unsubscribe(nameof(OnEntityDismounted));
            Unsubscribe(nameof(OnEntityEnter));
            Unsubscribe(nameof(CanLootEntity));
            Unsubscribe(nameof(OnEntitySpawned));
        }

        private void OnServerInitialized()
        {
            if (configData.globalS.storeVehicle)
            {
                var currentTimestamp = TimeEx.currentTimestamp;
                foreach (var vehicleEntries in storedData.playerData)
                {
                    foreach (var vehicleEntry in vehicleEntries.Value)
                    {
                        vehicleEntry.Value.lastRecall = vehicleEntry.Value.lastDismount = currentTimestamp;
                        vehicleEntry.Value.playerID = vehicleEntries.Key;
                        vehicleEntry.Value.vehicleType = vehicleEntry.Key;
                        if (vehicleEntry.Value.entityID == 0)
                        {
                            continue;
                        }
                        vehicleEntry.Value.entity = BaseNetworkable.serverEntities.Find(vehicleEntry.Value.entityID) as BaseEntity;
                        if (vehicleEntry.Value.entity == null || vehicleEntry.Value.entity.IsDestroyed)
                        {
                            vehicleEntry.Value.entityID = 0;
                        }
                        else
                        {
                            vehiclesCache.Add(vehicleEntry.Value.entity, vehicleEntry.Value);
                        }
                    }
                }
            }
            if (configData.globalS.preventMounting)
            {
                Subscribe(nameof(CanMountEntity));
            }
            if (configData.globalS.noDecay)
            {
                Subscribe(nameof(OnEntityTakeDamage));
            }
            if (configData.globalS.preventDamagePlayer)
            {
                Subscribe(nameof(OnEntityEnter));
            }
            if (configData.globalS.preventLooting)
            {
                Subscribe(nameof(CanLootEntity));
            }
            if (configData.globalS.autoClaimFromVendor)
            {
                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnRidableAnimalClaimed));
            }
            if (configData.globalS.checkVehiclesInterval > 0 && allBaseVehicleSettings.Any(x => x.Value.wipeTime > 0))
            {
                Subscribe(nameof(OnEntityDismounted));
                checkVehiclesTimer = timer.Every(configData.globalS.checkVehiclesInterval, CheckVehicles);
            }
        }

        private void Unload()
        {
            checkVehiclesTimer?.Destroy();
            if (!configData.globalS.storeVehicle)
            {
                foreach (var entry in vehiclesCache.ToArray())
                {
                    if (entry.Key != null && !entry.Key.IsDestroyed)
                    {
                        RefundVehicleItems(entry.Value, entry.Key, isUnload: true);
                        entry.Key.Kill(BaseNetworkable.DestroyMode.Gib);
                    }
                    entry.Value.entityID = 0;
                }
            }
            SaveData();
            Instance = null;
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_BYPASS_COST))
            {
                PurchaseAllVehicles(player.userID);
            }
        }

        private void OnEntityDismounted(BaseMountable entity, BasePlayer player)
        {
            var vehicleParent = entity?.VehicleParent();
            if (vehicleParent == null || vehicleParent.IsDestroyed) return;
            Vehicle vehicle;
            if (!vehiclesCache.TryGetValue(vehicleParent, out vehicle)) return;
            vehicle.OnDismount();
        }

        private object CanMountEntity(BasePlayer friend, BaseMountable entity)
        {
            var vehicleParent = entity?.VehicleParent();
            if (vehicleParent == null || vehicleParent.IsDestroyed) return null;
            Vehicle vehicle;
            if (!vehiclesCache.TryGetValue(vehicleParent, out vehicle)) return null;
            if (AreFriends(vehicle.playerID, friend.userID)) return null;
            if (configData.globalS.preventDriverSeat && vehicleParent.HasMountPoints() &&
                entity != vehicleParent.mountPoints[0].mountable) return null;
            return False;
        }

        private void OnEntitySpawned(MotorRowboat motorRowboat)
        {
            NextTick(() =>
            {
                var player = motorRowboat?.creatorEntity as BasePlayer;
                if (player == null || !player.userID.IsSteamId() || !motorRowboat.OnlyOwnerAccessible()) return;
                TryClaimVehicle(player, motorRowboat, motorRowboat is RHIB ? NormalVehicleType.RHIB : NormalVehicleType.Rowboat);
            });
        }

        private void OnEntitySpawned(MiniCopter miniCopter)
        {
            NextTick(() =>
            {
                var player = miniCopter?.creatorEntity as BasePlayer;
                if (player == null || !player.userID.IsSteamId() || !miniCopter.OnlyOwnerAccessible()) return;
                TryClaimVehicle(player, miniCopter, miniCopter is ScrapTransportHelicopter ? NormalVehicleType.TransportHelicopter : NormalVehicleType.MiniCopter);
            });
        }

        private void OnRidableAnimalClaimed(BaseRidableAnimal baseRidableAnimal, BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            TryClaimVehicle(player, baseRidableAnimal, NormalVehicleType.RidableHorse);
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (entity == null | hitInfo?.damageTypes == null) return;
            if (hitInfo.damageTypes.Has(Rust.DamageType.Decay))
            {
                if (!vehiclesCache.ContainsKey(entity))
                {
                    var vehicle = (entity as BaseVehicleModule)?.Vehicle;
                    if (vehicle == null || !vehiclesCache.ContainsKey(vehicle))
                    {
                        return;
                    }
                }
                hitInfo.damageTypes.Scale(Rust.DamageType.Decay, 0);
            }
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info) => CheckEntity(entity, true);

        private void OnEntityKill(BaseCombatEntity entity) => CheckEntity(entity);

        //ScrapTransportHelicopter And ModularCar
        private object OnEntityEnter(TriggerHurtNotChild triggerHurtNotChild, BasePlayer player)
        {
            if (triggerHurtNotChild == null || (triggerHurtNotChild.SourceEntity == null || player == null)) return null;
            var sourceEntity = triggerHurtNotChild.SourceEntity;
            if (vehiclesCache.ContainsKey(sourceEntity))
            {
                var baseVehicle = sourceEntity as BaseVehicle;
                if (baseVehicle != null && player.userID.IsSteamId())
                {
                    Vector3 pos;
                    if (GetDismountPosition(baseVehicle, player, out pos))
                    {
                        MoveToPosition(player, pos);
                    } 
                }
                //triggerHurtNotChild.enabled = false;
                return False;
            }
            return null;
        }

        //HotAirBalloon
        private object OnEntityEnter(TriggerHurt triggerHurt, BasePlayer player)
        {
            if (triggerHurt == null || player == null) return null;
            var sourceEntity = triggerHurt.gameObject.ToBaseEntity();
            if (sourceEntity == null) return null;
            if (vehiclesCache.ContainsKey(sourceEntity))
            {
                if (player.userID.IsSteamId())
                {
                    MoveToPosition(player, sourceEntity.CenterPoint() + Vector3.down); 
                }
                //triggerHurt.enabled = false;
                return False;
            }
            return null;
        }

        private object CanLootEntity(BasePlayer player, StorageContainer container)
        {
            if (player == null || container == null) return null;
            var parentEntity = container.GetParentEntity();
            if (parentEntity == null) return null;
            Vehicle vehicle;
            if (!vehiclesCache.TryGetValue(parentEntity, out vehicle))
            {
                var vehicleParent = (parentEntity as BaseVehicleModule)?.Vehicle;
                if (vehicleParent == null || !vehiclesCache.TryGetValue(vehicleParent, out vehicle))
                {
                    return null;
                }
            }
            return AreFriends(vehicle.playerID, player.userID) ? null : False;
        }

        #endregion Oxide Hooks

        #region Methods

        #region CheckEntity

        private void CheckEntity(BaseCombatEntity entity, bool isCrash = false)
        {
            if (entity == null) return;
            Vehicle vehicle;
            if (!vehiclesCache.TryGetValue(entity, out vehicle)) return;
            vehiclesCache.Remove(entity);
            vehicle.OnDeath();
            var baseVehicleS = GetBaseVehicleS(vehicle.vehicleType);
            RefundVehicleItems(vehicle, entity, isCrash: isCrash, baseVehicleS: baseVehicleS);
            if (isCrash && baseVehicleS.removeLicenseOnceCrash)
            {
                RemoveVehicleLicense(vehicle.playerID, vehicle.vehicleType);
            }
        }

        #endregion CheckEntity

        #region CheckVehicles

        private void CheckVehicles()
        {
            var currentTimestamp = TimeEx.currentTimestamp;
            foreach (var entry in vehiclesCache.ToArray())
            {
                if (entry.Key == null || entry.Key.IsDestroyed) continue;
                if (VehicleIsActive(entry.Value, currentTimestamp)) continue;
                if (VehicleAnyMounted(entry.Key)) continue;
                entry.Key.Kill(BaseNetworkable.DestroyMode.Gib);
            }
        }

        private bool VehicleIsActive(Vehicle vehicle, double currentTimestamp)
        {
            var baseVehicleS = GetBaseVehicleS(vehicle.vehicleType);
            if (baseVehicleS.wipeTime <= 0) return true;
            return currentTimestamp - vehicle.lastDismount < baseVehicleS.wipeTime;
        }

        #endregion CheckVehicles

        #region Refund

        private static bool CanRefundFuel(BaseVehicleS baseVehicleS, bool isCrash, bool isUnload)
        {
            if (isUnload) return true;
            var fuelVehicleS = baseVehicleS as IFuelVehicle;
            return fuelVehicleS != null && (isCrash ? fuelVehicleS.refundFuelOnCrash : fuelVehicleS.refundFuelOnKill);
        }

        private static bool CanRefundInventory(BaseVehicleS baseVehicleS, bool isCrash, bool isUnload)
        {
            if (isUnload) return true;
            var inventoryVehicleS = baseVehicleS as IInventoryVehicle;
            return inventoryVehicleS != null && (isCrash ? inventoryVehicleS.refundInventoryOnCrash : inventoryVehicleS.refundInventoryOnKill);
        }

        private static void ModularCarRefundState(BaseVehicleS baseVehicleS, bool isCrash, bool isUnload, out bool refundFuel, out bool refundInventory, out bool refundEngine, out bool refundModule)
        {
            if (isUnload)
            {
                refundFuel = refundInventory = refundEngine = refundModule = true;
                return;
            }
            var modularVehicleS = baseVehicleS as ModularVehicleS;
            if (modularVehicleS == null)
            {
                refundFuel = refundInventory = refundEngine = refundModule = false;
                return;
            }
            refundFuel = isCrash ? modularVehicleS.refundFuelOnCrash : modularVehicleS.refundFuelOnKill;
            refundInventory = isCrash ? modularVehicleS.refundInventoryOnCrash : modularVehicleS.refundInventoryOnKill;
            refundEngine = isCrash ? modularVehicleS.refundEngineOnCrash : modularVehicleS.refundEngineOnKill;
            refundModule = isCrash ? modularVehicleS.refundModuleOnCrash : modularVehicleS.refundModuleOnKill;
        }

        private void RefundVehicleItems(Vehicle vehicle, BaseEntity entity, BaseVehicleS baseVehicleS = null, bool isCrash = false, bool isUnload = false)
        {
            if (entity == null) entity = vehicle.entity;
            if (entity == null) return;
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicle.vehicleType);
            if (baseVehicleS == null) return;

            NormalVehicleType normalVehicleType;
            var collect = Pool.GetList<Item>();
            if (Enum.TryParse(vehicle.vehicleType, out normalVehicleType))
            {
                switch (normalVehicleType)
                {
                    case NormalVehicleType.Sedan:
                    case NormalVehicleType.Chinook:
                        return;

                    case NormalVehicleType.MiniCopter:
                    case NormalVehicleType.TransportHelicopter:
                        {
                            if (CanRefundFuel(baseVehicleS, isCrash, isUnload))
                            {
                                var fuelContainer = (entity as MiniCopter)?.GetFuelSystem()?.GetFuelContainer()?.inventory;
                                if (fuelContainer != null) collect.AddRange(fuelContainer.itemList);
                            }
                        }
                        break;

                    case NormalVehicleType.HotAirBalloon:
                        {
                            if (CanRefundFuel(baseVehicleS, isCrash, isUnload))
                            {
                                var fuelContainer = (entity as HotAirBalloon)?.fuelSystem?.GetFuelContainer()?.inventory;
                                if (fuelContainer != null) collect.AddRange(fuelContainer.itemList);
                            }
                            if (CanRefundInventory(baseVehicleS, isCrash, isUnload))
                            {
                                var itemContainer = ((entity as HotAirBalloon)?.storageUnitInstance.Get(true) as StorageContainer)?.inventory;
                                if (itemContainer != null) collect.AddRange(itemContainer.itemList);
                            }
                        }
                        break;

                    case NormalVehicleType.RHIB:
                    case NormalVehicleType.Rowboat:
                        {
                            if (CanRefundFuel(baseVehicleS, isCrash, isUnload))
                            {
                                var fuelContainer = (entity as MotorRowboat)?.GetFuelSystem()?.GetFuelContainer()?.inventory;
                                if (fuelContainer != null) collect.AddRange(fuelContainer.itemList);
                            }

                            if (CanRefundInventory(baseVehicleS, isCrash, isUnload))
                            {
                                var itemContainer = ((entity as MotorRowboat)?.storageUnitInstance.Get(true) as StorageContainer)?.inventory;
                                if (itemContainer != null) collect.AddRange(itemContainer.itemList);
                            }
                        }
                        break;

                    case NormalVehicleType.RidableHorse:
                        {
                            if (CanRefundInventory(baseVehicleS, isCrash, isUnload))
                            {
                                var itemContainer = (entity as RidableHorse)?.inventory;
                                if (itemContainer != null) collect.AddRange(itemContainer.itemList);
                            }
                        }
                        break;

                    default: return;
                }
            }
            else
            {
                var modularCar = entity as ModularCar;
                if (modularCar == null) return;

                bool refundFuel, refundInventory, refundEngine, refundModule;
                ModularCarRefundState(baseVehicleS, isCrash, isUnload, out refundFuel, out refundInventory, out refundEngine, out refundModule);

                foreach (var moduleEntity in modularCar.AttachedModuleEntities)
                {
                    if (refundEngine)
                    {
                        var moduleEngine = moduleEntity as VehicleModuleEngine;
                        if (moduleEngine != null)
                        {
                            var engineContainer = moduleEngine.GetContainer()?.inventory;
                            if (engineContainer != null) collect.AddRange(engineContainer.itemList);
                            continue;
                        }
                    }
                    if (refundInventory)
                    {
                        var moduleStorage = moduleEntity as VehicleModuleStorage;
                        if (moduleStorage != null)
                        {
                            var storageContainer = moduleStorage.GetContainer()?.inventory;
                            if (storageContainer != null) collect.AddRange(storageContainer.itemList);
                        }
                    }
                }
                if (refundFuel)
                {
                    var fuelContainer = modularCar.fuelSystem?.GetFuelContainer()?.inventory;
                    if (fuelContainer != null) collect.AddRange(fuelContainer.itemList);
                }
                if (refundModule)
                {
                    var moduleContainer = modularCar.Inventory?.ModuleContainer;
                    if (moduleContainer != null) collect.AddRange(moduleContainer.itemList);
                }
                //var chassisContainer = modularCar.Inventory?.ChassisContainer;
                //if (chassisContainer != null)
                //{
                //    collect.AddRange(chassisContainer.itemList);
                //}
            }

            if (collect.Count > 0)
            {
                var player = RustCore.FindPlayerById(vehicle.playerID);
                if (player == null)
                {
                    DropItemContainer(entity, vehicle.playerID, collect);
                }
                else
                {
                    for (var i = collect.Count - 1; i >= 0; i--)
                    {
                        var item = collect[i];
                        player.GiveItem(item);
                    }

                    if (player.IsConnected)
                    {
                        Print(player, Lang("RefundedVehicleItems", player.UserIDString, baseVehicleS.displayName));
                    }
                }
            }
            Pool.FreeList(ref collect);
        }

        private static void DropItemContainer(BaseEntity entity, ulong playerID, List<Item> collect)
        {
            var droppedItemContainer = GameManager.server.CreateEntity(PREFAB_ITEM_DROP, entity.GetDropPosition(), entity.transform.rotation) as DroppedItemContainer;
            droppedItemContainer.inventory = new ItemContainer();
            droppedItemContainer.inventory.ServerInitialize(null, Mathf.Min(collect.Count, droppedItemContainer.maxItemCount));
            droppedItemContainer.inventory.GiveUID();
            droppedItemContainer.inventory.entityOwner = droppedItemContainer;
            droppedItemContainer.inventory.SetFlag(ItemContainer.Flag.NoItemInput, true);
            for (var i = collect.Count - 1; i >= 0; i--)
            {
                var item = collect[i];
                if (!item.MoveToContainer(droppedItemContainer.inventory))
                {
                    item.DropAndTossUpwards(droppedItemContainer.transform.position);
                }
            }

            droppedItemContainer.OwnerID = playerID;
            droppedItemContainer.Spawn();
        }

        #endregion Refund

        #region GiveFuel

        private static void TryGiveFuel(BaseEntity entity, string vehicleType, IFuelVehicle iFuelVehicle)
        {
            if (iFuelVehicle == null || iFuelVehicle.spawnFuelAmount <= 0) return;
            ItemContainer fuelContainer;
            NormalVehicleType normalVehicleType;
            if (Enum.TryParse(vehicleType, out normalVehicleType))
            {
                switch (normalVehicleType)
                {
                    case NormalVehicleType.Sedan:
                    case NormalVehicleType.Chinook:
                    case NormalVehicleType.RidableHorse:
                        return;

                    case NormalVehicleType.MiniCopter:
                    case NormalVehicleType.TransportHelicopter:
                        fuelContainer = (entity as MiniCopter)?.GetFuelSystem()?.GetFuelContainer()?.inventory;
                        break;

                    case NormalVehicleType.HotAirBalloon:
                        fuelContainer = (entity as HotAirBalloon)?.fuelSystem?.GetFuelContainer()?.inventory;
                        break;

                    case NormalVehicleType.RHIB:
                    case NormalVehicleType.Rowboat:
                        fuelContainer = (entity as MotorRowboat)?.GetFuelSystem()?.GetFuelContainer()?.inventory;
                        break;

                    default: return;
                }
            }
            else
            {
                fuelContainer = (entity as ModularCar)?.fuelSystem?.GetFuelContainer()?.inventory;
            }

            if (fuelContainer == null /*|| fuelContainer.FindItemByItemID(ITEMID_FUEL) != null*/) return;
            var fuel = ItemManager.CreateByItemID(ITEMID_FUEL, iFuelVehicle.spawnFuelAmount);
            if (!fuel.MoveToContainer(fuelContainer))
            {
                fuel.Remove();
            }
        }

        #endregion GiveFuel

        #region VehicleModules

        private void AttacheVehicleModules(ModularCar modularCar, ModularVehicleS modularVehicleS, string vehicleType)
        {
            foreach (var moduleItem in modularVehicleS.CreateModuleItems())
            {
                if (!modularCar.TryAddModule(moduleItem))
                {
                    PrintError($"Module item '{moduleItem.info.shortname}' in '{vehicleType}' cannot be attached to the vehicle");
                    moduleItem.Remove();
                }
            }
        }

        private void AddItemsToVehicleEngine(ModularCar modularCar, ModularVehicleS modularVehicleS, string vehicleType)
        {
            if (modularCar == null || modularCar.IsDestroyed) return;
            foreach (var moduleEntity in modularCar.AttachedModuleEntities)
            {
                var vehicleModuleEngine = moduleEntity as VehicleModuleEngine;
                if (vehicleModuleEngine != null)
                {
                    var engineInventory = vehicleModuleEngine.GetContainer()?.inventory;
                    if (engineInventory != null)
                    {
                        foreach (var engineItem in modularVehicleS.CreateEngineItems())
                        {
                            bool moved = false;
                            for (int i = 0; i < engineInventory.capacity; i++)
                            {
                                if (engineItem.MoveToContainer(engineInventory, i, false))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                            if (!moved)
                            {
                                PrintError($"Engine item '{engineItem.info.shortname}' in '{vehicleType}' cannot be move to the vehicle engine inventory");
                                engineItem.Remove();
                            }
                        }
                    }
                }
            }
        }

        #endregion VehicleModules

        #region Drop

        private static bool CanDropInventory(BaseVehicleS baseVehicleS)
        {
            var inventoryVehicle = baseVehicleS as IInventoryVehicle;
            return inventoryVehicle != null && inventoryVehicle.dropInventoryOnRecall;
        }

        private void DropVehicleInventoryItems(BasePlayer player, string vehicleType, BaseEntity entity, BaseVehicleS baseVehicleS)
        {
            DroppedItemContainer droppedItemContainer = null;
            NormalVehicleType normalVehicleType;
            if (Enum.TryParse(vehicleType, out normalVehicleType))
            {
                switch (normalVehicleType)
                {
                    case NormalVehicleType.Rowboat:
                    case NormalVehicleType.RHIB:
                        {
                            var storageContainer = (entity as MotorRowboat)?.storageUnitInstance.Get(true) as StorageContainer;
                            droppedItemContainer = storageContainer?.inventory?.Drop(PREFAB_ITEM_DROP, entity.GetDropPosition(),
                                entity.transform.rotation);
                        }
                        break;

                    case NormalVehicleType.HotAirBalloon:
                        {
                            var storageContainer = (entity as HotAirBalloon)?.storageUnitInstance.Get(true) as StorageContainer;
                            droppedItemContainer = storageContainer?.inventory?.Drop(PREFAB_ITEM_DROP, entity.GetDropPosition(),
                                entity.transform.rotation);
                        }
                        break;

                    case NormalVehicleType.RidableHorse:
                        {
                            droppedItemContainer = (entity as RidableHorse)?.inventory?.Drop(PREFAB_ITEM_DROP, entity.GetDropPosition(),
                                entity.transform.rotation);
                        }
                        break;
                }
            }
            else
            {
                var modularCar = entity as ModularCar;
                if (modularCar == null) return;
                foreach (var moduleEntity in modularCar.AttachedModuleEntities)
                {
                    if (moduleEntity is VehicleModuleEngine) continue;
                    var moduleStorage = moduleEntity as VehicleModuleStorage;
                    if (moduleStorage != null)
                    {
                        droppedItemContainer = moduleStorage.GetContainer()?.inventory?.Drop(PREFAB_ITEM_DROP, entity.GetDropPosition(), entity.transform.rotation);
                    }
                }
            }
            if (droppedItemContainer != null)
            {
                Print(player, Lang("VehicleInventoryDropped", player.UserIDString, baseVehicleS.displayName));
            }
        }

        #endregion Drop

        #region TryPay

        private bool TryPay(BasePlayer player, Dictionary<string, PriceInfo> prices, out string missingResources)
        {
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_BYPASS_COST))
            {
                missingResources = null;
                return true;
            }

            if (!CanPay(player, prices, out missingResources))
            {
                return false;
            }

            var collect = Pool.GetList<Item>();
            foreach (var entry in prices)
            {
                if (entry.Value.amount <= 0) continue;
                var itemDefinition = ItemManager.FindItemDefinition(entry.Key);
                if (itemDefinition != null)
                {
                    player.inventory.Take(collect, itemDefinition.itemid, entry.Value.amount);
                    player.Command("note.inv", itemDefinition.itemid, -entry.Value.amount);
                    continue;
                }
                switch (entry.Key.ToLower())
                {
                    case "economics":
                        Economics?.Call("Withdraw", player.userID, (double)entry.Value.amount);
                        continue;

                    case "serverrewards":
                        ServerRewards?.Call("TakePoints", player.userID, entry.Value.amount);
                        continue;
                }
            }

            foreach (var item in collect)
            {
                item.Remove();
            }
            Pool.FreeList(ref collect);
            missingResources = null;
            return true;
        }

        private readonly Dictionary<string, int> missingDictionary = new Dictionary<string, int>();

        private bool CanPay(BasePlayer player, Dictionary<string, PriceInfo> prices, out string missingResources)
        {
            missingDictionary.Clear();
            foreach (var entry in prices)
            {
                if (entry.Value.amount <= 0) continue;
                int missingAmount;
                var itemDefinition = ItemManager.FindItemDefinition(entry.Key);
                if (itemDefinition != null) missingAmount = entry.Value.amount - player.inventory.GetAmount(itemDefinition.itemid);
                else missingAmount = CheckBalance(entry.Key, entry.Value.amount, player.userID);
                if (missingAmount <= 0) continue;
                if (!missingDictionary.ContainsKey(entry.Value.displayName))
                {
                    missingDictionary.Add(entry.Value.displayName, missingAmount);
                }
                else
                {
                    missingDictionary[entry.Value.displayName] += missingAmount;
                }
            }
            if (missingDictionary.Count > 0)
            { 
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                foreach (var entry in missingDictionary)
                {
                    stringBuilder.AppendLine($"* {Lang("PriceFormat", player.UserIDString, entry.Key, entry.Value)}");
                }
                missingResources = stringBuilder.ToString();
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return false;
            }
            missingResources = null;
            return true;
        }

        private int CheckBalance(string key, int price, ulong playerID)
        {
            switch (key.ToLower())
            {
                case "economics":
                    var balance = Economics?.Call("Balance", playerID);
                    if (balance is double)
                    {
                        var n = price - (double)balance;
                        return n <= 0 ? 0 : (int)Math.Ceiling(n);
                    }
                    return price;

                case "serverrewards":
                    var points = ServerRewards?.Call("CheckPoints", playerID);
                    if (points is int)
                    {
                        var n = price - (int)points;
                        return n <= 0 ? 0 : n;
                    }
                    return price;

                default:
                    PrintError($"Unknown Currency Type '{key}'");
                    return price;
            }
        }

        #endregion TryPay

        #region AreFriends

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (playerID == friendID) return true;
            if (configData.globalS.useTeams && SameTeam(playerID, friendID)) return true;
            if (configData.globalS.useFriends && HasFriend(playerID, friendID)) return true;
            if (configData.globalS.useClans && SameClan(playerID, friendID)) return true;
            return false;
        }

        private static bool SameTeam(ulong playerID, ulong friendID)
        {
            if (!RelationshipManager.TeamsEnabled()) return false;
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam == null) return false;
            var friendTeam = RelationshipManager.Instance.FindPlayersTeam(friendID);
            if (friendTeam == null) return false;
            return playerTeam == friendTeam;
        }

        private bool HasFriend(ulong playerID, ulong friendID)
        {
            if (Friends == null) return false;
            return (bool)Friends.Call("HasFriend", playerID, friendID);
        }

        private bool SameClan(ulong playerID, ulong friendID)
        {
            if (Clans == null) return false;
            //Clans
            var isMember = Clans.Call("IsClanMember", playerID.ToString(), friendID.ToString());
            if (isMember != null) return (bool)isMember;
            //Rust:IO Clans
            var playerClan = Clans.Call("GetClanOf", playerID);
            if (playerClan == null) return false;
            var friendClan = Clans.Call("GetClanOf", friendID);
            if (friendClan == null) return false;
            return (string)playerClan == (string)friendClan;
        }

        #endregion AreFriends

        #region PlayerIsBlocked

        private bool IsPlayerBlocked(BasePlayer player)
        {
            if (NoEscape == null) return false;
            if (configData.globalS.useRaidBlocker && IsRaidBlocked(player.UserIDString))
            {
                Print(player, Lang("RaidBlocked", player.UserIDString));
                return true;
            }
            if (configData.globalS.useCombatBlocker && IsCombatBlocked(player.UserIDString))
            {
                Print(player, Lang("CombatBlocked", player.UserIDString));
                return true;
            }
            return false;
        }

        private bool IsRaidBlocked(string playerID) => (bool)NoEscape.Call("IsRaidBlocked", playerID);

        private bool IsCombatBlocked(string playerID) => (bool)NoEscape.Call("IsCombatBlocked", playerID);

        #endregion PlayerIsBlocked

        #region GetBaseVehicleS

        private BaseVehicleS GetBaseVehicleS(string vehicleName)
        {
            BaseVehicleS baseVehicleS;
            return allBaseVehicleSettings.TryGetValue(vehicleName, out baseVehicleS) ? baseVehicleS : null;
        }

        private BaseVehicleS GetBaseVehicleS(NormalVehicleType normalVehicleType)
        {
            switch (normalVehicleType)
            {
                case NormalVehicleType.Rowboat: return configData.normalVehicleS.rowboatS;
                case NormalVehicleType.RHIB: return configData.normalVehicleS.rhibS;
                case NormalVehicleType.Sedan: return configData.normalVehicleS.sedanS;
                case NormalVehicleType.HotAirBalloon: return configData.normalVehicleS.hotAirBalloonS;
                case NormalVehicleType.MiniCopter: return configData.normalVehicleS.miniCopterS;
                case NormalVehicleType.TransportHelicopter: return configData.normalVehicleS.transportHelicopterS;
                case NormalVehicleType.Chinook: return configData.normalVehicleS.chinookS;
                case NormalVehicleType.RidableHorse: return configData.normalVehicleS.ridableHorseS;
                default: return null;
            }
        }

        #endregion GetBaseVehicleS

        #region HasVehiclePermission

        private bool CanViewVehicleInfo(BasePlayer player, string vehicleType, BaseVehicleS baseVehicleS)
        {
            if (baseVehicleS.purchasable && baseVehicleS.commands.Count > 0)
            {
                return HasVehiclePermission(player, vehicleType, baseVehicleS);
            }
            return false;
        }

        private bool HasVehiclePermission(BasePlayer player, string vehicleType, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            if (!baseVehicleS.usePermission || string.IsNullOrEmpty(baseVehicleS.permission)) return true;
            return permission.UserHasPermission(player.UserIDString, PERMISSION_ALL) ||
                   permission.UserHasPermission(player.UserIDString, baseVehicleS.permission);
        }

        #endregion HasVehiclePermission

        #region GetCooldown

        private double GetSpawnCooldown(BasePlayer player, BaseVehicleS baseVehicleS)
        {
            double cooldown = baseVehicleS.spawnCooldown;
            foreach (var entry in baseVehicleS.cooldownPermissions)
            {
                if (cooldown > entry.Value.spawnCooldown && permission.UserHasPermission(player.UserIDString, entry.Key))
                {
                    cooldown = entry.Value.spawnCooldown;
                }
            }
            return cooldown;
        }

        private double GetRecallCooldown(BasePlayer player, BaseVehicleS baseVehicleS)
        {
            double cooldown = baseVehicleS.recallCooldown;
            foreach (var entry in baseVehicleS.cooldownPermissions)
            {
                if (cooldown > entry.Value.recallCooldown && permission.UserHasPermission(player.UserIDString, entry.Key))
                {
                    cooldown = entry.Value.recallCooldown;
                }
            }
            return cooldown;
        }

        #endregion GetCooldown

        #region ClaimVehicle

        private bool TryClaimVehicle(BasePlayer player, BaseEntity entity, NormalVehicleType normalVehicleType)
        {
            return TryClaimVehicle(player, entity, normalVehicleType.ToString());
        }

        private bool TryClaimVehicle(BasePlayer player, BaseEntity entity, string vehicleType)
        {
            Vehicle vehicle;
            if (!storedData.IsVehiclePurchased(player.userID, vehicleType, out vehicle))
            {
                if (!configData.globalS.autoUnlockFromVendor)
                {
                    return false;
                }
                storedData.AddVehicleLicense(player.userID, vehicleType);
                vehicle = storedData.GetVehicleLicense(player.userID, vehicleType);
            }
            if (vehicle.entity == null || vehicle.entity.IsDestroyed)
            {
                entity.OwnerID = player.userID;
                SetupVehicleEntity(entity, vehicle, player, vehicleType, false);
                return true;
            }
            return false;
        }

        #endregion ClaimVehicle

        #region Helpers

        private static bool GetDismountPosition(BaseVehicle baseVehicle, BasePlayer player, out Vector3 result)
        {
            var parentVehicle = baseVehicle.VehicleParent();
            if (parentVehicle != null)
            {
                return GetDismountPosition(parentVehicle, player, out result);
            }
            var list = Pool.GetList<Vector3>();
            foreach (var transform in baseVehicle.dismountPositions)
            {
                var visualCheckOrigin = transform.position + Vector3.up * 0.6f;
                if (baseVehicle.ValidDismountPosition(transform.position, visualCheckOrigin))
                {
                    list.Add(transform.position);
                }
            }
            if (list.Count == 0)
            {
                result = Vector3.zero;
                Pool.FreeList(ref list);
                return false;
            }
            Vector3 pos = player.transform.position;
            list.Sort((a, b) => Vector3.Distance(a, pos).CompareTo(Vector3.Distance(b, pos)));
            result = list[0];
            Pool.FreeList(ref list);
            return true;
        }

        private static string GetVehiclePrefab(string vehicleType, BaseVehicleS baseVehicleS)
        {
            NormalVehicleType normalVehicleType;
            if (Enum.TryParse(vehicleType, out normalVehicleType))
            {
                switch (normalVehicleType)
                {
                    case NormalVehicleType.Rowboat: return PREFAB_ROWBOAT;
                    case NormalVehicleType.RHIB: return PREFAB_RHIB;
                    case NormalVehicleType.Sedan: return PREFAB_SEDAN;
                    case NormalVehicleType.HotAirBalloon: return PREFAB_HOTAIRBALLOON;
                    case NormalVehicleType.MiniCopter: return PREFAB_MINICOPTER;
                    case NormalVehicleType.TransportHelicopter: return PREFAB_TRANSPORTCOPTER;
                    case NormalVehicleType.Chinook: return PREFAB_CHINOOK;
                    case NormalVehicleType.RidableHorse: return PREFAB_RIDABLEHORSE;
                }
            }
            else
            {
                var modularVehicleS = baseVehicleS as ModularVehicleS;
                if (modularVehicleS != null)
                {
                    switch (modularVehicleS.chassisType)
                    {
                        case ChassisType.Small: return PREFAB_CHASSIS_SMALL;
                        case ChassisType.Medium: return PREFAB_CHASSIS_MEDIUM;
                        case ChassisType.Large: return PREFAB_CHASSIS_LARGE;
                    }
                }
            }

            return null;
        }

        private static bool VehicleAnyMounted(BaseEntity entity)
        {
            var baseVehicle = entity as BaseVehicle;
            if (baseVehicle != null && baseVehicle.AnyMounted())
            {
                return true;
            }

            return entity.GetComponentsInChildren<BasePlayer>()?.Length > 0;
        }

        private static void DismountAllPlayers(BaseEntity entity)
        {
            var baseVehicle = entity as BaseVehicle;
            if (baseVehicle != null)
            {
                //(vehicle as BaseVehicle).DismountAllPlayers();
                foreach (var mountPointInfo in baseVehicle.mountPoints)
                {
                    if (mountPointInfo.mountable != null)
                    {
                        var mounted = mountPointInfo.mountable.GetMounted();
                        if (mounted != null)
                        {
                            mountPointInfo.mountable.DismountPlayer(mounted);
                        }
                    }
                }
            }
            var players = entity.GetComponentsInChildren<BasePlayer>();
            foreach (var player in players)
            {
                player.SetParent(null, true, true);
            }
        }

        private static Vector3 GetGroundPositionLookingAt(BasePlayer player, float distance)
        {
            RaycastHit hitInfo;
            var headRay = player.eyes.HeadRay();
            if (Physics.Raycast(headRay, out hitInfo, distance, LAYER_GROUND))
            {
                return hitInfo.point;
            }
            return GetGroundPosition(headRay.origin + headRay.direction * distance);
        }

        private static Vector3 GetGroundPosition(Vector3 position)
        {
            RaycastHit hitInfo;
            position.y = Physics.Raycast(position + Vector3.up * 200, Vector3.down, out hitInfo, 400f, LAYER_GROUND)
                ? hitInfo.point.y
                : TerrainMeta.HeightMap.GetHeight(position);
            return position;
        }

        private static bool IsInWater(Vector3 position)
        {
            var colliders = Pool.GetList<Collider>();
            Vis.Colliders(position, 0.5f, colliders);
            var flag = colliders.Any(x => x.gameObject.layer == (int)Rust.Layer.Water);
            Pool.FreeList(ref colliders);
            return flag;
            //return WaterLevel.Test(lookingAt);
        }

        private static void MoveToPosition(BasePlayer player, Vector3 position)
        {
            player.Teleport(position);
            player.ForceUpdateTriggers();
            //if (player.HasParent()) player.SetParent(null, true, true);
            player.SendNetworkUpdateImmediate(); 
        }

        #endregion Helpers

        #endregion Methods

        #region API

        private bool IsLicensedVehicle(BaseEntity entity)
        {
            return vehiclesCache.ContainsKey(entity);
        }

        private BaseEntity GetLicensedVehicle(ulong playerID, string license)
        {
            return storedData.GetVehicleLicense(playerID, license)?.entity;
        }

        private bool HasVehicleLicense(ulong playerID, string license)
        {
            return storedData.HasVehicleLicense(playerID, license);
        }

        private bool RemoveVehicleLicense(ulong playerID, string license)
        {
            return storedData.RemoveVehicleLicense(playerID, license);
        }

        private bool AddVehicleLicense(ulong playerID, string license)
        {
            return storedData.AddVehicleLicense(playerID, license);
        }

        private List<string> GetVehicleLicenses(ulong playerID)
        {
            return storedData.GetVehicleLicenseNames(playerID);
        }

        private void PurchaseAllVehicles(ulong playerID)
        {
            storedData.PurchaseAllVehicles(playerID);
        }

        #endregion API

        #region Commands

        #region Universal Command

        private void CmdUniversal(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }

            string vehicleType;
            if (IsValidOption(player, command, out vehicleType))
            {
                var bypassCooldown = args.Length >= 1 && IsValidBypassCooldownOption(args[0]);
                HandleUniversalCmd(player, vehicleType, bypassCooldown);
            }
        }

        private void HandleUniversalCmd(BasePlayer player, string vehicleType, bool bypassCooldown)
        {
            Vehicle vehicle;
            if (storedData.IsVehiclePurchased(player.userID, vehicleType, out vehicle))
            {
                bool checkWater = NeedCheckWater(vehicleType);
                string reason; Vector3 position = Vector3.zero; Quaternion rotation = Quaternion.identity;
                if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
                {
                    //recall
                    if (CanRecall(player, vehicle, vehicleType, checkWater, bypassCooldown, out reason, ref position, ref rotation))
                    {
                        RecallVehicle(player, vehicle, vehicleType, position, rotation);
                        return;
                    }
                }
                else
                {
                    //spawn
                    if (CanSpawn(player, vehicle, vehicleType, checkWater, bypassCooldown, out reason, ref position, ref rotation))
                    {
                        SpawnVehicle(player, vehicle, vehicleType, position, rotation);
                        return;
                    }
                }
                Print(player, reason);
                return;
            }
            //buy
            BuyVehicle(player, vehicleType);
        }

        #endregion Universal Command

        #region Custom Kill Command

        private void CmdCustomKill(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            command = command.Remove(0, configData.chatS.customKillCommandPrefix.Length);
            HandleKillCmd(player, command);
        }

        #endregion Custom Kill Command

        #region Help Command

        private void CmdLicenseHelp(BasePlayer player, string command, string[] args)
        {
            StringBuilder stringBuilder = Pool.Get<StringBuilder>();
            stringBuilder.AppendLine(Lang("Help", player.UserIDString));
            stringBuilder.AppendLine(Lang("HelpLicence1", player.UserIDString, configData.chatS.buyCommand));
            stringBuilder.AppendLine(Lang("HelpLicence2", player.UserIDString, configData.chatS.spawnCommand));
            stringBuilder.AppendLine(Lang("HelpLicence3", player.UserIDString, configData.chatS.recallCommand));
            stringBuilder.AppendLine(Lang("HelpLicence4", player.UserIDString, configData.chatS.killCommand));

            foreach (var entry in allBaseVehicleSettings)
            {
                if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                {
                    if (configData.chatS.useUniversalCommand)
                    {
                        var firstCmd = entry.Value.commands[0];
                        stringBuilder.AppendLine(Lang("HelpLicence5", player.UserIDString, firstCmd, entry.Value.displayName));
                    }
                    //if (!string.IsNullOrEmpty(configData.chatS.customKillCommandPrefix))
                    //{
                    //    stringBuilder.AppendLine(Lang("HelpLicence6", player.UserIDString, configData.chatS.customKillCommandPrefix + firstCmd, entry.Value.displayName));
                    //}
                }
            }
            Print(player, stringBuilder.ToString());
            stringBuilder.Clear();
            Pool.Free(ref stringBuilder);
        }

        #endregion Help Command

        #region Remove Command

        [ConsoleCommand("vl.remove")]
        private void CCmdRemoveVehicle(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin && arg.Args != null && arg.Args.Length == 2)
            {
                var option = arg.Args[0];
                string vehicleType;
                if (!IsValidVehicleType(option, out vehicleType))
                {
                    Print(arg, $"{option} is not a valid vehicle type");
                    return;
                }
                switch (arg.Args[1].ToLower())
                {
                    case "*":
                    case "all":
                        {
                            storedData.RemoveLicenseForAllPlayers(vehicleType);
                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            Print(arg, $"You successfully removed the vehicle({vehicleName}) of all players");
                        }
                        return;

                    default:
                        {
                            var target = RustCore.FindPlayer(arg.Args[1]);
                            if (target == null)
                            {
                                Print(arg, $"Player '{arg.Args[1]}' not found");
                                return;
                            }

                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            if (RemoveVehicleLicense(target.userID, vehicleType))
                            {
                                Print(arg, $"You successfully removed the vehicle({vehicleName}) of {target.displayName}");
                                return;
                            }

                            Print(arg, $"{target.displayName} has not purchased vehicle({vehicleName}) and cannot be removed");
                        }
                        return;
                }
            }
        }

        [ConsoleCommand("vl.cleardata")]
        private void CCmdClearVehicle(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin)
            {
                foreach (var vehicle in vehiclesCache.Keys.ToArray())
                {
                    vehicle.Kill(BaseNetworkable.DestroyMode.Gib);
                }
                vehiclesCache.Clear();
                ClearData();
                Print(arg, "You successfully cleaned up all vehicle data");
            }
        }

        #endregion Remove Command

        #region Buy Command

        [ConsoleCommand("vl.buy")]
        private void CCmdBuyVehicle(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin && arg.Args != null && arg.Args.Length == 2)
            {
                var option = arg.Args[0];
                string vehicleType;
                if (!IsValidVehicleType(option, out vehicleType))
                {
                    Print(arg, $"{option} is not a valid vehicle type");
                    return;
                }
                switch (arg.Args[1].ToLower())
                {
                    case "*":
                    case "all":
                        {
                            storedData.AddLicenseForAllPlayers(vehicleType);
                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            Print(arg, $"You successfully purchased the vehicle({vehicleName}) for all players");
                        }
                        return;

                    default:
                        {
                            var target = RustCore.FindPlayer(arg.Args[1]);
                            if (target == null)
                            {
                                Print(arg, $"Player '{arg.Args[1]}' not found");
                                return;
                            }

                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            if (AddVehicleLicense(target.userID, vehicleType))
                            {
                                Print(arg, $"You successfully purchased the vehicle({vehicleName}) for {target.displayName}");
                                return;
                            }

                            Print(arg, $"{target.displayName} has purchased vehicle({vehicleName})");
                        }
                        return;
                }
            }
            var player = arg.Player();
            if (player != null) CmdBuyVehicle(player, string.Empty, arg.Args);
            else Print(arg, $"The server console cannot use the '{arg.cmd.FullName}' command");
        }

        private void CmdBuyVehicle(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            if (args == null || args.Length < 1)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        var firstCmd = entry.Value.commands[0];
                        if (entry.Value.purchasePrices.Count > 0)
                        {
                            var prices = FormatPriceInfo(player, entry.Value.purchasePrices);
                            stringBuilder.AppendLine(Lang("HelpBuyPrice", player.UserIDString, configData.chatS.buyCommand, firstCmd, entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpBuy", player.UserIDString, configData.chatS.buyCommand, firstCmd, entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }
            string vehicleType;
            if (IsValidOption(player, args[0], out vehicleType))
            {
                BuyVehicle(player, vehicleType);
            }
        }

        private bool BuyVehicle(BasePlayer player, string vehicleType)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            if (!baseVehicleS.purchasable)
            {
                Print(player, Lang("VehicleCannotBeBought", player.UserIDString, baseVehicleS.displayName));
                return false;
            }
            var vehicles = storedData.GetPlayerVehicles(player.userID, false);
            if (vehicles.ContainsKey(vehicleType))
            {
                Print(player, Lang("VehicleAlreadyPurchased", player.UserIDString, baseVehicleS.displayName));
                return false;
            }
            string missingResources;
            if (baseVehicleS.purchasePrices.Count > 0 && !TryPay(player, baseVehicleS.purchasePrices, out missingResources))
            {
                Print(player, Lang("NoResourcesToPurchaseVehicle", player.UserIDString, baseVehicleS.displayName, missingResources));
                return false;
            }
            vehicles.Add(vehicleType, new Vehicle());
            SaveData();
            Print(player, Lang("VehiclePurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.spawnCommand));
            return true;
        }

        #endregion Buy Command

        #region Spawn Command

        [ConsoleCommand("vl.spawn")]
        private void CCmdSpawnVehicle(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) Print(arg, $"The server console cannot use the '{arg.cmd.FullName}' command");
            else CmdSpawnVehicle(player, string.Empty, arg.Args);
        }

        private void CmdSpawnVehicle(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            if (args == null || args.Length < 1)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        var firstCmd = entry.Value.commands[0];
                        if (entry.Value.spawnPrices.Count > 0)
                        {
                            var prices = FormatPriceInfo(player, entry.Value.spawnPrices);
                            stringBuilder.AppendLine(Lang("HelpSpawnPrice", player.UserIDString, configData.chatS.spawnCommand, firstCmd, entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpSpawn", player.UserIDString, configData.chatS.spawnCommand, firstCmd, entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }
            string vehicleType;
            if (IsValidOption(player, args[0], out vehicleType))
            {
                var bypassCooldown = args.Length > 1 && IsValidBypassCooldownOption(args[1]);
                SpawnVehicle(player, vehicleType, bypassCooldown);
            }
        }

        private bool SpawnVehicle(BasePlayer player, string vehicleType, bool bypassCooldown)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            Vehicle vehicle;
            if (!storedData.IsVehiclePurchased(player.userID, vehicleType, out vehicle))
            {
                Print(player, Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand));
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
            {
                Print(player, Lang("AlreadyVehicleOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.recallCommand));
                return false;
            }
            bool checkWater = NeedCheckWater(vehicleType);
            string reason; Vector3 position = Vector3.zero; Quaternion rotation = Quaternion.identity;
            if (CanSpawn(player, vehicle, vehicleType, checkWater, bypassCooldown, out reason, ref position, ref rotation, baseVehicleS))
            {
                SpawnVehicle(player, vehicle, vehicleType, position, rotation, baseVehicleS);
                return false;
            }
            Print(player, reason);
            return true;
        }

        private bool CanSpawn(BasePlayer player, Vehicle vehicle, string vehicleType, bool checkWater, bool bypassCooldown, out string reason, ref Vector3 position, ref Quaternion rotation, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            if (configData.globalS.limitVehicles > 0)
            {
                var activeVehicles = storedData.ActiveVehiclesCount(player.userID);
                if (activeVehicles >= configData.globalS.limitVehicles)
                {
                    reason = Lang("VehiclesLimit", player.UserIDString, configData.globalS.limitVehicles);
                    return false;
                }
            }
            if (!CanPlayerAction(player, vehicleType, checkWater, baseVehicleS, out reason, ref position, ref rotation))
            {
                return false;
            }
            var obj = Interface.CallHook("CanLicensedVehicleSpawn", player, vehicleType, position, rotation);
            if (obj != null)
            {
                var s = obj as string;
                reason = s ?? Lang("SpawnWasBlocked", player.UserIDString, baseVehicleS.displayName);
                return false;
            }

#if DEBUG
            if (player.IsAdmin)
            {
                reason = null;
                return true;
            }
#endif
            if (!CheckCooldown(player, vehicle, baseVehicleS, bypassCooldown, out reason, true))
            {
                return false;
            }

            string missingResources;
            if (baseVehicleS.spawnPrices.Count > 0 && !TryPay(player, baseVehicleS.spawnPrices, out missingResources))
            {
                reason = Lang("NoResourcesToSpawnVehicle", player.UserIDString, baseVehicleS.displayName, missingResources);
                return false;
            }
            reason = null;
            return true;
        }

        private void SpawnVehicle(BasePlayer player, Vehicle vehicle, string vehicleType, Vector3 position, Quaternion rotation, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            var prefab = GetVehiclePrefab(vehicleType, baseVehicleS);
            if (string.IsNullOrEmpty(prefab)) return;
            var entity = GameManager.server.CreateEntity(prefab, position, rotation);
            if (entity == null) return;
            entity.enableSaving = configData.globalS.storeVehicle;
            entity.OwnerID = player.userID;
            entity.Spawn();

            SetupVehicleEntity(entity, vehicle, player, vehicleType, baseVehicleS: baseVehicleS);

            Interface.CallHook("OnLicensedVehicleSpawned", entity, player, vehicleType);
            Print(player, Lang("VehicleSpawned", player.UserIDString, baseVehicleS.displayName));
        }

        private void SetupVehicleEntity(BaseEntity entity, Vehicle vehicle, BasePlayer player, string vehicleType, bool giveFuel = true, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            if (giveFuel) TryGiveFuel(entity, vehicleType, baseVehicleS as IFuelVehicle);
            if (baseVehicleS.maxHealth > 0 && Math.Abs(baseVehicleS.maxHealth - entity.MaxHealth()) > 0f)
            {
                (entity as BaseCombatEntity)?.InitializeHealth(baseVehicleS.maxHealth, baseVehicleS.maxHealth);
            }
            var modularCar = entity as ModularCar;
            if (modularCar != null)
            {
                var modularVehicleS = baseVehicleS as ModularVehicleS;
                if (modularVehicleS != null)
                {
                    if (modularVehicleS.ModuleItems.Any())
                    {
                        AttacheVehicleModules(modularCar, modularVehicleS, vehicleType);
                    }
                    if (modularVehicleS.EngineItems.Any())
                    {
                        NextTick(() => AddItemsToVehicleEngine(modularCar, modularVehicleS, vehicleType));
                    }
                }
            }
            else
            {
                var helicopterVehicle = entity as BaseHelicopterVehicle;
                if (helicopterVehicle != null)
                {
                    if (configData.globalS.noServerGibs) helicopterVehicle.serverGibs.guid = string.Empty;
                    if (configData.globalS.noFireBall) helicopterVehicle.fireBall.guid = string.Empty;
                    if (configData.globalS.noMapMarker)
                    {
                        var ch47Helicopter = entity as CH47Helicopter;
                        if (ch47Helicopter != null)
                        {
                            ch47Helicopter.mapMarkerInstance?.Kill();
                            ch47Helicopter.mapMarkerEntityPrefab.guid = string.Empty;
                        }
                    }
                }
            }

            vehicle.playerID = player.userID;
            vehicle.vehicleType = vehicleType;
            vehicle.entity = entity;
            vehicle.entityID = entity.net.ID;
            vehicle.lastDismount = vehicle.lastRecall = TimeEx.currentTimestamp;
            vehiclesCache.Add(entity, vehicle);
        }

        #endregion Spawn Command

        #region Recall Command

        [ConsoleCommand("vl.recall")]
        private void CCmdRecallVehicle(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) Print(arg, $"The server console cannot use the '{arg.cmd.FullName}' command");
            else CmdRecallVehicle(player, string.Empty, arg.Args);
        }

        private void CmdRecallVehicle(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            if (args == null || args.Length < 1)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        var firstCmd = entry.Value.commands[0];
                        if (entry.Value.recallPrices.Count > 0)
                        {
                            var prices = FormatPriceInfo(player, entry.Value.recallPrices);
                            stringBuilder.AppendLine(Lang("HelpRecallPrice", player.UserIDString, configData.chatS.recallCommand, firstCmd, entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpRecall", player.UserIDString, configData.chatS.recallCommand, firstCmd, entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }
            string vehicleType;
            if (IsValidOption(player, args[0], out vehicleType))
            {
                var bypassCooldown = args.Length > 1 && IsValidBypassCooldownOption(args[1]);
                RecallVehicle(player, vehicleType, bypassCooldown);
            }
        }

        private bool RecallVehicle(BasePlayer player, string vehicleType, bool bypassCooldown)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            Vehicle vehicle;
            if (!storedData.IsVehiclePurchased(player.userID, vehicleType, out vehicle))
            {
                Print(player, Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand));
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
            {
                bool checkWater = NeedCheckWater(vehicleType);
                string reason; Vector3 position = Vector3.zero; Quaternion rotation = Quaternion.identity;
                if (CanRecall(player, vehicle, vehicleType, checkWater, bypassCooldown, out reason, ref position, ref rotation, baseVehicleS))
                {
                    RecallVehicle(player, vehicle, vehicleType, position, rotation, baseVehicleS);
                    return true;
                }
                Print(player, reason);
                return false;
            }
            Print(player, Lang("VehicleNotOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.spawnCommand));
            return false;
        }

        private bool CanRecall(BasePlayer player, Vehicle vehicle, string vehicleType, bool checkWater, bool bypassCooldown, out string reason, ref Vector3 position, ref Quaternion rotation, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            if (baseVehicleS.recallMaxDistance > 0 && Vector3.Distance(player.transform.position, vehicle.entity.transform.position) > baseVehicleS.recallMaxDistance)
            {
                reason = Lang("RecallTooFar", player.UserIDString, baseVehicleS.recallMaxDistance, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.anyMountedRecall && VehicleAnyMounted(vehicle.entity))
            {
                reason = Lang("PlayerMountedOnVehicle", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (!CanPlayerAction(player, vehicleType, checkWater, baseVehicleS, out reason, ref position, ref rotation))
            {
                return false;
            }

            var obj = Interface.CallHook("CanLicensedVehicleRecall", vehicle.entity, player, vehicleType, position, rotation);
            if (obj != null)
            {
                var s = obj as string;
                reason = s ?? Lang("RecallWasBlocked", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
#if DEBUG
            if (player.IsAdmin)
            {
                reason = null;
                return true;
            }
#endif
            if (!CheckCooldown(player, vehicle, baseVehicleS, bypassCooldown, out reason, false))
            {
                return false;
            }
            string missingResources;
            if (baseVehicleS.recallPrices.Count > 0 && !TryPay(player, baseVehicleS.recallPrices, out missingResources))
            {
                reason = Lang("NoResourcesToRecallVehicle", player.UserIDString, baseVehicleS.displayName, missingResources);
                return false;
            }
            reason = null;
            return true;
        }

        private void RecallVehicle(BasePlayer player, Vehicle vehicle, string vehicleType, Vector3 position, Quaternion rotation, BaseVehicleS baseVehicleS = null)
        {
            if (baseVehicleS == null) baseVehicleS = GetBaseVehicleS(vehicleType);
            var entity = vehicle.entity;
            if (configData.globalS.dismountAllPlayersRecall)
            {
                DismountAllPlayers(entity);
            }
            if (CanDropInventory(baseVehicleS))
            {
                DropVehicleInventoryItems(player, vehicle.vehicleType, entity, baseVehicleS);
            }
            if (entity.HasParent()) entity.SetParent(null, true, true);
            if (entity is ModularCar)
            {
                var modularCarGarages = Pool.GetList<ModularCarGarage>();
                Vis.Entities(entity.transform.position, 3f, modularCarGarages, Rust.Layers.Mask.Deployed | Rust.Layers.Mask.Default);
                var modularCarGarage = modularCarGarages.FirstOrDefault(x => x.carOccupant == entity);
                Pool.FreeList(ref modularCarGarages);
                if (modularCarGarage != null)
                {
                    modularCarGarage.enabled = false;
                    modularCarGarage.ReleaseOccupant();
                    modularCarGarage.Invoke(() => modularCarGarage.enabled = true, 0.25f);
                }
            }

            vehicle.OnRecall();
            var entityTransform = entity.transform;
            entityTransform.position = position;
            entityTransform.rotation = rotation;
            entityTransform.hasChanged = true;

            var ridableHorse = entity as RidableHorse;
            if (ridableHorse != null)
            {
                ridableHorse.TryLeaveHitch();
                ridableHorse.DropToGround(ridableHorse.transform.position, true);//ridableHorse.UpdateDropToGroundForDuration(2f);
            }

            Interface.CallHook("OnLicensedVehicleRecalled", entity, player, vehicleType);
            Print(player, Lang("VehicleRecalled", player.UserIDString, baseVehicleS.displayName));
        }

        #endregion Recall Command

        #region Kill Command

        [ConsoleCommand("vl.kill")]
        private void CCmdKillVehicle(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) Print(arg, $"The server console cannot use the '{arg.cmd.FullName}' command");
            else CmdKillVehicle(player, string.Empty, arg.Args);
        }

        private void CmdKillVehicle(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            if (args == null || args.Length < 1)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        var firstCmd = entry.Value.commands[0];
                        if (!string.IsNullOrEmpty(configData.chatS.customKillCommandPrefix))
                        {
                            stringBuilder.AppendLine(Lang("HelpKillCustom", player.UserIDString, configData.chatS.killCommand, firstCmd, configData.chatS.customKillCommandPrefix + firstCmd, entry.Value.displayName));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpKill", player.UserIDString, configData.chatS.killCommand, firstCmd, entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }

            HandleKillCmd(player, args[0]);
        }

        private void HandleKillCmd(BasePlayer player, string option)
        {
            string vehicleType;
            if (IsValidOption(player, option, out vehicleType))
            {
                KillVehicle(player, vehicleType);
            }
        }

        private bool KillVehicle(BasePlayer player, string vehicleType)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            Vehicle vehicle;
            if (!storedData.IsVehiclePurchased(player.userID, vehicleType, out vehicle))
            {
                Print(player, Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand));
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
            {
                if (!CanKill(player, vehicle, baseVehicleS))
                {
                    return false;
                }
                vehicle.entity.Kill(BaseNetworkable.DestroyMode.Gib);
                Print(player, Lang("VehicleKilled", player.UserIDString, baseVehicleS.displayName));
                return true;
            }
            Print(player, Lang("VehicleNotOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.spawnCommand));
            return false;
        }

        private bool CanKill(BasePlayer player, Vehicle vehicle, BaseVehicleS baseVehicleS)
        {
            if (configData.globalS.anyMountedKill && VehicleAnyMounted(vehicle.entity))
            {
                Print(player, Lang("PlayerMountedOnVehicle", player.UserIDString, baseVehicleS.displayName));
                return false;
            }
            if (baseVehicleS.killMaxDistance > 0 && Vector3.Distance(player.transform.position, vehicle.entity.transform.position) > baseVehicleS.killMaxDistance)
            {
                Print(player, Lang("KillTooFar", player.UserIDString, baseVehicleS.killMaxDistance, baseVehicleS.displayName));
                return false;
            }

            return true;
        }

        #endregion Kill Command

        #region Command Helpers

        private static bool NeedCheckWater(string vehicleType) => vehicleType == nameof(NormalVehicleType.Rowboat) || vehicleType == nameof(NormalVehicleType.RHIB);

        private bool IsValidBypassCooldownOption(string option)
        {
            return !string.IsNullOrEmpty(configData.chatS.bypassCooldownCommand) && string.Equals(option, configData.chatS.bypassCooldownCommand, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidOption(BasePlayer player, string option, out string vehicleType)
        {
            if (!commandToVehicleType.TryGetValue(option, out vehicleType))
            {
                Print(player, Lang("OptionNotFound", player.UserIDString, option));
                return false;
            }
            if (!HasVehiclePermission(player, vehicleType))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                vehicleType = null;
                return false;
            }
            if (IsPlayerBlocked(player))
            {
                vehicleType = null;
                return false;
            }
            return true;
        }

        private bool IsValidVehicleType(string option, out string vehicleType)
        {
            foreach (var entry in allBaseVehicleSettings)
            {
                if (string.Equals(entry.Key, option, StringComparison.OrdinalIgnoreCase))
                {
                    vehicleType = entry.Key;
                    return true;
                }
            }

            vehicleType = null;
            return false;
        }

        private string FormatPriceInfo(BasePlayer player, Dictionary<string, PriceInfo> prices)
        {
            return string.Join(", ",
                from p in prices
                select Lang("PriceFormat", player.UserIDString, p.Value.displayName, p.Value.amount));
        }

        private bool CanPlayerAction(BasePlayer player, string vehicleType, bool checkWater, BaseVehicleS baseVehicleS, out string reason, ref Vector3 position, ref Quaternion rotation)
        {
            if (configData.globalS.preventBuildingBlocked && player.IsBuildingBlocked())
            {
                reason = Lang("BuildingBlocked", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.preventSafeZone && player.InSafeZone())
            {
                reason = Lang("PlayerInSafeZone", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.preventMountedOrParented && HasMountedOrParented(player, vehicleType))
            {
                reason = Lang("MountedOrParented", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            Vector3 lookingAt = Vector3.zero;
            if (!CheckPosition(player, baseVehicleS, checkWater, out reason, ref lookingAt))
            {
                return false;
            }
            FindVehicleSpawnPositionAndRotation(player, baseVehicleS, checkWater, vehicleType, lookingAt, out position, out rotation);
            reason = null;
            return true;
        }

        private bool HasMountedOrParented(BasePlayer player, string vehicleType)
        {
            if (player.GetMountedVehicle() != null) return true;
            var parentEntity = player.GetParentEntity();
            if (parentEntity != null)
            {
                if (configData.globalS.spawnLookingAt && LandOnCargoShip != null && parentEntity is CargoShip &&
                    (vehicleType == nameof(NormalVehicleType.MiniCopter) ||
                     vehicleType == nameof(NormalVehicleType.TransportHelicopter)))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private bool CheckCooldown(BasePlayer player, Vehicle vehicle, BaseVehicleS baseVehicleS, bool bypassCooldown, out string reason, bool isSpawnCooldown)
        {
            var cooldown = isSpawnCooldown ? GetSpawnCooldown(player, baseVehicleS) : GetRecallCooldown(player, baseVehicleS);
            if (cooldown > 0)
            {
                var timeLeft = Math.Ceiling(cooldown - (TimeEx.currentTimestamp - (isSpawnCooldown ? vehicle.lastDeath : vehicle.lastRecall)));
                if (timeLeft > 0)
                {
                    var bypassPrices = isSpawnCooldown
                        ? baseVehicleS.bypassSpawnCooldownPrices
                        : baseVehicleS.bypassRecallCooldownPrices;
                    if (bypassCooldown && bypassPrices.Count > 0)
                    {
                        string missingResources;
                        if (!TryPay(player, bypassPrices, out missingResources))
                        {
                            reason = Lang(isSpawnCooldown ? "NoResourcesToSpawnVehicleBypass" : "NoResourcesToRecallVehicleBypass", player.UserIDString, baseVehicleS.displayName, missingResources);
                            return false;
                        }

                        if (isSpawnCooldown) vehicle.lastDeath = 0;
                        else vehicle.lastRecall = 0;
                    }
                    else
                    {
                        reason = Lang(isSpawnCooldown ? "VehicleOnSpawnCooldown" : "VehicleOnRecallCooldown", player.UserIDString, timeLeft, baseVehicleS.displayName);
                        return false;
                    }
                }
            }
            reason = null;
            return true;
        }

        private bool CheckPosition(BasePlayer player, BaseVehicleS baseVehicleS, bool checkWater, out string reason, ref Vector3 lookingAt)
        {
            if (checkWater || configData.globalS.spawnLookingAt)
            {
                lookingAt = GetGroundPositionLookingAt(player, baseVehicleS.distance);
                if (checkWater && !IsInWater(lookingAt))
                {
                    reason = Lang("NotLookingAtWater", player.UserIDString, baseVehicleS.displayName);
                    return false;
                }
                if (configData.globalS.spawnLookingAt && baseVehicleS.minDistanceForPlayers > 0)
                {
                    var nearbyPlayers = Pool.GetList<BasePlayer>();
                    Vis.Entities(lookingAt, baseVehicleS.minDistanceForPlayers, nearbyPlayers, Rust.Layers.Mask.Player_Server);
                    bool flag = nearbyPlayers.Any(x => x.userID.IsSteamId() && x != player);
                    Pool.FreeList(ref nearbyPlayers);
                    if (flag)
                    {
                        reason = Lang("PlayersOnNearby", player.UserIDString, baseVehicleS.displayName);
                        return false;
                    }
                }
            }
            reason = null;
            return true;
        }

        private void FindVehicleSpawnPositionAndRotation(BasePlayer player, BaseVehicleS baseVehicleS, bool checkWater, string vehicleType, Vector3 lookingAt, out Vector3 spawnPos, out Quaternion spawnRot)
        {
            if (configData.globalS.spawnLookingAt)
            {
                bool needGetGround = true;
                spawnPos = lookingAt == Vector3.zero ? GetGroundPositionLookingAt(player, baseVehicleS.distance) : lookingAt;
                if (checkWater)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(spawnPos, Vector3.up, out hit, 100, LAYER_GROUND) && hit.GetEntity() is StabilityEntity)
                    {
                        //At the dock
                        needGetGround = false;
                    }
                }
                else
                {
                    var buildingBlocks = Pool.GetList<BuildingBlock>();
                    Vis.Entities(spawnPos, 2f, buildingBlocks, Rust.Layers.Mask.Construction);
                    if (buildingBlocks.Count > 0)
                    {
                        var pos = spawnPos;
                        var closestBuildingBlock = buildingBlocks
                            .Where(x => !x.ShortPrefabName.Contains("wall"))
                            .OrderBy(x => (x.transform.position - pos).magnitude).FirstOrDefault();
                        if (closestBuildingBlock != null)
                        {
                            var worldSpaceBounds = closestBuildingBlock.WorldSpaceBounds();
                            spawnPos = worldSpaceBounds.position;
                            spawnPos.y += worldSpaceBounds.extents.y;
                            needGetGround = false;
                        }
                    }
                    Pool.FreeList(ref buildingBlocks);
                }
                if (needGetGround)
                {
                    spawnPos = GetGroundPosition(spawnPos);
                }
            }
            else
            {
                var minDistance = Mathf.Min(baseVehicleS.minDistanceForPlayers, 2.5f);
                var distance = Mathf.Max(baseVehicleS.distance, minDistance);
                spawnPos = player.transform.position;
                var nearbyPlayers = Pool.GetList<BasePlayer>();
                var sourcePos = checkWater ? (lookingAt == Vector3.zero ? GetGroundPositionLookingAt(player, baseVehicleS.distance) : lookingAt) : spawnPos;
                for (int i = 0; i < 100; i++)
                {
                    spawnPos.x = sourcePos.x + UnityEngine.Random.Range(minDistance, distance) * (UnityEngine.Random.value >= 0.5f ? 1 : -1);
                    spawnPos.z = sourcePos.z + UnityEngine.Random.Range(minDistance, distance) * (UnityEngine.Random.value >= 0.5f ? 1 : -1);
                    spawnPos = GetGroundPosition(spawnPos);
                    nearbyPlayers.Clear();
                    Vis.Entities(spawnPos, minDistance, nearbyPlayers, Rust.Layers.Mask.Player_Server);
                    if (!nearbyPlayers.Any(x => x.userID.IsSteamId()))
                    {
                        break;
                    }
                }
                Pool.FreeList(ref nearbyPlayers);
            }

            var normalized = (spawnPos - player.transform.position).normalized;
            var angle = normalized != Vector3.zero ? Quaternion.LookRotation(normalized).eulerAngles.y : UnityEngine.Random.Range(0f, 360f);
            var rot = vehicleType == nameof(NormalVehicleType.HotAirBalloon) ? 180f : 90f;
            spawnRot = Quaternion.Euler(Vector3.up * (angle + rot));
            if (vehicleType != nameof(NormalVehicleType.RidableHorse)) spawnPos += Vector3.up * 0.3f;
        }

        #endregion Command Helpers

        #endregion Commands

        #region ConfigurationFile

        public ConfigData configData { get; private set; }

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public GlobalSettings globalS = new GlobalSettings();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            [JsonProperty(PropertyName = "Normal Vehicle Settings")]
            public NormalVehicleSettings normalVehicleS = new NormalVehicleSettings();

            [JsonProperty(PropertyName = "Modular Vehicle Settings", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, ModularVehicleS> modularCarS = new Dictionary<string, ModularVehicleS>
            {
                ["SmallCar"] = new ModularVehicleS
                {
                    purchasable = true,
                    displayName = "Small Modular Car",
                    distance = 5,
                    minDistanceForPlayers = 3,
                    usePermission = true,
                    permission = "vehiclelicence.smallmodularcar",
                    commands = new List<string> { "small", "smallcar" },
                    purchasePrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 1600, displayName = "Scrap" }
                    },
                    spawnPrices = new Dictionary<string, PriceInfo>
                    {
                        ["metal.refined"] = new PriceInfo { amount = 10, displayName = "High Quality Metal" }
                    },
                    recallPrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 5, displayName = "Scrap" }
                    },
                    spawnCooldown = 7200,
                    recallCooldown = 30,
                    cooldownPermissions = new Dictionary<string, PermissionS>
                    {
                        ["vehiclelicence.vip"] = new PermissionS
                        {
                            spawnCooldown = 3600,
                            recallCooldown = 10,
                        }
                    },
                    chassisType = ChassisType.Small,
                    moduleItems = new List<ModuleItem>
                    {
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.cockpit.with.engine" ,healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.storage" ,healthPercentage = 50f
                        },
                    },
                    engineItems = new List<EngineItem>
                    {
                        new EngineItem
                        {
                            shortName = "carburetor1",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "crankshaft1",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "piston1",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "sparkplug1",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "valve1",conditionPercentage = 20f
                        }
                    }
                },
                ["MediumCar"] = new ModularVehicleS
                {
                    purchasable = true,
                    displayName = "Medium Modular Car",
                    distance = 5,
                    minDistanceForPlayers = 3,
                    usePermission = true,
                    permission = "vehiclelicence.mediumodularcar",
                    commands = new List<string> { "medium", "mediumcar" },
                    purchasePrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 2400, displayName = "Scrap" }
                    },
                    spawnPrices = new Dictionary<string, PriceInfo>
                    {
                        ["metal.refined"] = new PriceInfo { amount = 50, displayName = "High Quality Metal" }
                    },
                    recallPrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 8, displayName = "Scrap" }
                    },
                    spawnCooldown = 9000,
                    recallCooldown = 30,
                    cooldownPermissions = new Dictionary<string, PermissionS>
                    {
                        ["vehiclelicence.vip"] = new PermissionS
                        {
                            spawnCooldown = 4500,
                            recallCooldown = 10,
                        }
                    },
                    chassisType = ChassisType.Medium,
                    moduleItems = new List<ModuleItem>
                    {
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.cockpit.with.engine" ,healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.rear.seats" ,healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.flatbed" ,healthPercentage = 50f
                        },
                    },
                    engineItems = new List<EngineItem>
                    {
                        new EngineItem
                        {
                            shortName = "carburetor2",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "crankshaft2",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "piston2",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "sparkplug2",conditionPercentage = 20f
                        },
                        new EngineItem
                        {
                            shortName = "valve2",conditionPercentage = 20f
                        }
                    }
                },
                ["LargeCar"] = new ModularVehicleS
                {
                    purchasable = true,
                    displayName = "Large Modular Car",
                    distance = 6,
                    minDistanceForPlayers = 3,
                    usePermission = true,
                    permission = "vehiclelicence.largemodularcar",
                    commands = new List<string> { "large", "largecar" },
                    purchasePrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 3000, displayName = "Scrap" }
                    },
                    spawnPrices = new Dictionary<string, PriceInfo>
                    {
                        ["metal.refined"] = new PriceInfo { amount = 100, displayName = "High Quality Metal" }
                    },
                    recallPrices = new Dictionary<string, PriceInfo>
                    {
                        ["scrap"] = new PriceInfo { amount = 10, displayName = "Scrap" }
                    },
                    spawnCooldown = 10800,
                    recallCooldown = 30,
                    cooldownPermissions = new Dictionary<string, PermissionS>
                    {
                        ["vehiclelicence.vip"] = new PermissionS
                        {
                            spawnCooldown = 5400,
                            recallCooldown = 10,
                        }
                    },
                    chassisType = ChassisType.Large,
                    moduleItems = new List<ModuleItem>
                    {
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.engine",healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.cockpit.armored",healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.passengers.armored",healthPercentage = 50f
                        },
                        new ModuleItem
                        {
                            shortName = "vehicle.1mod.storage",healthPercentage = 50f
                        },
                    },
                    engineItems = new List<EngineItem>
                    {
                        new EngineItem
                        {
                            shortName = "carburetor3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "crankshaft3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "piston3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "piston3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "sparkplug3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "sparkplug3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "valve3",conditionPercentage = 10f
                        },
                        new EngineItem
                        {
                            shortName = "valve3",conditionPercentage = 10f
                        }
                    }
                },
            };

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version;
        }

        public class ChatSettings
        {
            [JsonProperty(PropertyName = "Use Universal Chat Command")]
            public bool useUniversalCommand = true;

            [JsonProperty(PropertyName = "Help Chat Command")]
            public string helpCommand = "license";

            [JsonProperty(PropertyName = "Buy Chat Command")]
            public string buyCommand = "buy";

            [JsonProperty(PropertyName = "Spawn Chat Command")]
            public string spawnCommand = "spawn";

            [JsonProperty(PropertyName = "Recall Chat Command")]
            public string recallCommand = "recall";

            [JsonProperty(PropertyName = "Kill Chat Command")]
            public string killCommand = "kill";

            [JsonProperty(PropertyName = "Custom Kill Chat Command Prefix")]
            public string customKillCommandPrefix = "no";

            [JsonProperty(PropertyName = "Bypass Cooldown Command")]
            public string bypassCooldownCommand = "pay";

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "<color=#00FFFF>[VehicleLicense]</color>: ";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 76561198924840872;
        }

        public class GlobalSettings
        {
            [JsonProperty(PropertyName = "Store Vehicle On Plugin Unloaded / Server Restart")]
            public bool storeVehicle = true;

            [JsonProperty(PropertyName = "Clear Vehicle Data On Map Wipe")]
            public bool clearVehicleOnWipe;

            [JsonProperty(PropertyName = "Interval to check vehicle for wipe (Seconds)")]
            public float checkVehiclesInterval = 300;

            [JsonProperty(PropertyName = "Spawn vehicle in the direction you are looking at")]
            public bool spawnLookingAt = true;

            [JsonProperty(PropertyName = "Automatically claim vehicles purchased from vehicle vendors")]
            public bool autoClaimFromVendor;

            [JsonProperty(PropertyName = "Vehicle vendor purchases to unlock the license for the player")]
            public bool autoUnlockFromVendor;

            [JsonProperty(PropertyName = "Limit the count of vehicles at a time")]
            public int limitVehicles;

            [JsonProperty(PropertyName = "Prevent vehicles from damaging players")]
            public bool preventDamagePlayer = true;

            [JsonProperty(PropertyName = "Prevent vehicles from spawning or recalling in safe zone")]
            public bool preventSafeZone = true;

            [JsonProperty(PropertyName = "Prevent vehicles from spawning or recalling when the player in the building blocked")]
            public bool preventBuildingBlocked = true;

            [JsonProperty(PropertyName = "Prevent vehicles from spawning or recalling when the player has mounted or parented")]
            public bool preventMountedOrParented = true;

            [JsonProperty(PropertyName = "Check if any player mounted when recalling a vehicle")]
            public bool anyMountedRecall = true;

            [JsonProperty(PropertyName = "Check if any player mounted when killing a vehicle")]
            public bool anyMountedKill;

            [JsonProperty(PropertyName = "Dismount all players when a vehicle is recalled")]
            public bool dismountAllPlayersRecall = true;

            [JsonProperty(PropertyName = "Prevent other players from mounting vehicle")]
            public bool preventMounting = true;

            [JsonProperty(PropertyName = "Prevent mounting on driver's seat only")]
            public bool preventDriverSeat = true;

            [JsonProperty(PropertyName = "Prevent other players from looting fuel container and inventory")]
            public bool preventLooting = true;

            [JsonProperty(PropertyName = "Use Teams")]
            public bool useTeams;

            [JsonProperty(PropertyName = "Use Clans")]
            public bool useClans = true;

            [JsonProperty(PropertyName = "Use Friends")]
            public bool useFriends = true;

            [JsonProperty(PropertyName = "Vehicle No Decay")]
            public bool noDecay;

            [JsonProperty(PropertyName = "Vehicle No Fire Ball")]
            public bool noFireBall = true;

            [JsonProperty(PropertyName = "Vehicle No Server Gibs")]
            public bool noServerGibs = true;

            [JsonProperty(PropertyName = "Chinook No Map Marker")]
            public bool noMapMarker = true;

            [JsonProperty(PropertyName = "Use Raid Blocker (Need NoEscape Plugin)")]
            public bool useRaidBlocker;

            [JsonProperty(PropertyName = "Use Combat Blocker (Need NoEscape Plugin)")]
            public bool useCombatBlocker;
        }

        public class NormalVehicleSettings
        {
            [JsonProperty(PropertyName = "Sedan Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public BaseVehicleS sedanS = new BaseVehicleS
            {
                purchasable = true,
                displayName = "Sedan",
                distance = 5,
                minDistanceForPlayers = 3,
                usePermission = true,
                permission = "vehiclelicence.sedan",
                commands = new List<string> { "car", "sedan" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 300, displayName = "Scrap" }
                },
                spawnCooldown = 300,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 150,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Chinook Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public BaseVehicleS chinookS = new BaseVehicleS
            {
                purchasable = true,
                displayName = "Chinook",
                distance = 15,
                minDistanceForPlayers = 6,
                usePermission = true,
                permission = "vehiclelicence.chinook",
                commands = new List<string> { "ch47", "chinook" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 3000, displayName = "Scrap" }
                },
                spawnCooldown = 3000,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 1500,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Rowboat Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public InvFuelVehicleS rowboatS = new InvFuelVehicleS
            {
                purchasable = true,
                displayName = "Row Boat",
                distance = 5,
                minDistanceForPlayers = 2,
                usePermission = true,
                permission = "vehiclelicence.rowboat",
                commands = new List<string> { "row", "rowboat" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 500, displayName = "Scrap" }
                },
                spawnCooldown = 300,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 150,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "RHIB Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public InvFuelVehicleS rhibS = new InvFuelVehicleS
            {
                purchasable = true,
                displayName = "Rigid Hulled Inflatable Boat",
                distance = 10,
                minDistanceForPlayers = 3,
                usePermission = true,
                permission = "vehiclelicence.rhib",
                commands = new List<string> { "rhib" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 1000, displayName = "Scrap" }
                },
                spawnCooldown = 450,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 225,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Hot Air Balloon Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public InvFuelVehicleS hotAirBalloonS = new InvFuelVehicleS
            {
                purchasable = true,
                displayName = "Hot Air Balloon",
                distance = 20,
                minDistanceForPlayers = 5,
                usePermission = true,
                permission = "vehiclelicence.hotairballoon",
                commands = new List<string> { "hab", "hotairballoon" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 500, displayName = "Scrap" }
                },
                spawnCooldown = 900,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 450,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Ridable Horse Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public InventoryVehicleS ridableHorseS = new InventoryVehicleS
            {
                purchasable = true,
                displayName = "Ridable Horse",
                distance = 6,
                minDistanceForPlayers = 1,
                usePermission = true,
                permission = "vehiclelicence.ridablehorse",
                commands = new List<string> { "horse", "ridablehorse" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 700, displayName = "Scrap" }
                },
                spawnCooldown = 3000,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 1500,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Mini Copter Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public FuelVehicleS miniCopterS = new FuelVehicleS
            {
                purchasable = true,
                displayName = "Mini Copter",
                distance = 8,
                minDistanceForPlayers = 2,
                usePermission = true,
                permission = "vehiclelicence.minicopter",
                commands = new List<string> { "mini", "minicopter" },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 4000, displayName = "Scrap" }
                },
                spawnCooldown = 1800,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 900,
                        recallCooldown = 10,
                    }
                },
            };

            [JsonProperty(PropertyName = "Transport Helicopter Vehicle", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public FuelVehicleS transportHelicopterS = new FuelVehicleS
            {
                purchasable = true,
                displayName = "Transport Copter",
                distance = 10,
                minDistanceForPlayers = 4,
                usePermission = true,
                permission = "vehiclelicence.transportcopter",
                commands = new List<string>
                {
                    "tcop", "transportcopter"
                },
                purchasePrices = new Dictionary<string, PriceInfo>
                {
                    ["scrap"] = new PriceInfo { amount = 5000, displayName = "Scrap" }
                },
                spawnCooldown = 2400,
                recallCooldown = 30,
                cooldownPermissions = new Dictionary<string, PermissionS>
                {
                    ["vehiclelicence.vip"] = new PermissionS
                    {
                        spawnCooldown = 1200,
                        recallCooldown = 10,
                    }
                },
            };
        }

        public class BaseVehicleS
        {
            [JsonProperty(PropertyName = "Purchasable")]
            public bool purchasable;

            [JsonProperty(PropertyName = "Display Name")]
            public string displayName;

            [JsonProperty(PropertyName = "Use Permission")]
            public bool usePermission;

            [JsonProperty(PropertyName = "Permission")]
            public string permission;

            [JsonProperty(PropertyName = "Distance To Spawn")]
            public float distance;

            [JsonProperty(PropertyName = "Time Before Vehicle Wipe (Seconds)")]
            public double wipeTime;

            [JsonProperty(PropertyName = "Maximum Health")]
            public float maxHealth;

            [JsonProperty(PropertyName = "Can Recall Maximum Distance")]
            public float recallMaxDistance;

            [JsonProperty(PropertyName = "Can Kill Maximum Distance")]
            public float killMaxDistance;

            [JsonProperty(PropertyName = "Minimum distance from player to recall or spawn")]
            public float minDistanceForPlayers = 3f;

            [JsonProperty(PropertyName = "Remove License Once Crashed")]
            public bool removeLicenseOnceCrash;

            [JsonProperty(PropertyName = "Purchase Prices")]
            public Dictionary<string, PriceInfo> purchasePrices = new Dictionary<string, PriceInfo>();

            [JsonProperty(PropertyName = "Spawn Prices")]
            public Dictionary<string, PriceInfo> spawnPrices = new Dictionary<string, PriceInfo>();

            [JsonProperty(PropertyName = "Recall Prices")]
            public Dictionary<string, PriceInfo> recallPrices = new Dictionary<string, PriceInfo>();

            [JsonProperty(PropertyName = "Recall Cooldown Bypass Prices")]
            public Dictionary<string, PriceInfo> bypassRecallCooldownPrices = new Dictionary<string, PriceInfo>();

            [JsonProperty(PropertyName = "Spawn Cooldown Bypass Prices")]
            public Dictionary<string, PriceInfo> bypassSpawnCooldownPrices = new Dictionary<string, PriceInfo>();

            [JsonProperty(PropertyName = "Commands")]
            public List<string> commands = new List<string>();

            [JsonProperty(PropertyName = "Spawn Cooldown (Seconds)")]
            public double spawnCooldown;

            [JsonProperty(PropertyName = "Recall Cooldown (Seconds)")]
            public double recallCooldown;

            [JsonProperty(PropertyName = "Cooldown Permissions")]
            public Dictionary<string, PermissionS> cooldownPermissions = new Dictionary<string, PermissionS>();
        }

        public class FuelVehicleS : BaseVehicleS, IFuelVehicle
        {
            public int spawnFuelAmount { get; set; }
            public bool refundFuelOnKill { get; set; } = true;
            public bool refundFuelOnCrash { get; set; } = true;
        }

        public class InventoryVehicleS : BaseVehicleS, IInventoryVehicle
        {
            public bool refundInventoryOnKill { get; set; } = true;
            public bool refundInventoryOnCrash { get; set; } = true;
            public bool dropInventoryOnRecall { get; set; }
        }

        public class InvFuelVehicleS : BaseVehicleS, IFuelVehicle, IInventoryVehicle
        {
            public int spawnFuelAmount { get; set; }
            public bool refundFuelOnKill { get; set; } = true;
            public bool refundFuelOnCrash { get; set; } = true;
            public bool refundInventoryOnKill { get; set; } = true;
            public bool refundInventoryOnCrash { get; set; } = true;
            public bool dropInventoryOnRecall { get; set; }
        }

        public class ModularVehicleS : InvFuelVehicleS, IModularVehicle
        {
            public bool refundEngineOnKill { get; set; } = true;
            public bool refundEngineOnCrash { get; set; } = true;
            public bool refundModuleOnKill { get; set; } = true;
            public bool refundModuleOnCrash { get; set; } = true;

            [JsonConverter(typeof(StringEnumConverter))]
            [JsonProperty(PropertyName = "Chassis Type (Small, Medium, Large)", Order = 40)]
            public ChassisType chassisType = ChassisType.Small;

            [JsonProperty(PropertyName = "Vehicle Module Items", Order = 41)]
            public List<ModuleItem> moduleItems = new List<ModuleItem>();

            [JsonProperty(PropertyName = "Vehicle Engine Items", Order = 42)]
            public List<EngineItem> engineItems = new List<EngineItem>();

            #region ModuleItems

            [JsonIgnore] private List<ModuleItem> _validModuleItems;

            [JsonIgnore]
            public IEnumerable<ModuleItem> ModuleItems
            {
                get
                {
                    if (_validModuleItems == null)
                    {
                        _validModuleItems = new List<ModuleItem>();
                        foreach (var modularItem in moduleItems)
                        {
                            var itemDefinition = ItemManager.FindItemDefinition(modularItem.shortName);
                            if (itemDefinition != null)
                            {
                                var itemModVehicleModule = itemDefinition.GetComponent<ItemModVehicleModule>();
                                if (itemModVehicleModule == null || !itemModVehicleModule.entityPrefab.isValid)
                                {
                                    Instance.PrintError($"'{modularItem}' is not a valid vehicle module");
                                    continue;
                                }
                                _validModuleItems.Add(modularItem);
                            }
                        }
                    }
                    return _validModuleItems;
                }
            }

            public IEnumerable<Item> CreateModuleItems()
            {
                foreach (var moduleItem in ModuleItems)
                {
                    var item = ItemManager.CreateByName(moduleItem.shortName);
                    if (item != null)
                    {
                        item.condition = item.maxCondition * (moduleItem.healthPercentage / 100f);
                        item.MarkDirty();
                        yield return item;
                    }
                }
            }

            #endregion ModuleItems

            #region EngineItems

            [JsonIgnore] private List<EngineItem> _validEngineItems;

            [JsonIgnore]
            public IEnumerable<EngineItem> EngineItems
            {
                get
                {
                    if (_validEngineItems == null)
                    {
                        _validEngineItems = new List<EngineItem>();
                        foreach (var modularItem in engineItems)
                        {
                            var itemDefinition = ItemManager.FindItemDefinition(modularItem.shortName);
                            if (itemDefinition != null)
                            {
                                var itemModEngineItem = itemDefinition.GetComponent<ItemModEngineItem>();
                                if (itemModEngineItem == null)
                                {
                                    Instance.PrintError($"'{modularItem}' is not a valid engine item");
                                    continue;
                                }
                                _validEngineItems.Add(modularItem);
                            }
                        }
                    }
                    return _validEngineItems;
                }
            }

            public IEnumerable<Item> CreateEngineItems()
            {
                foreach (var engineItem in EngineItems)
                {
                    var item = ItemManager.CreateByName(engineItem.shortName);
                    if (item != null)
                    {
                        item.condition = item.maxCondition * (engineItem.conditionPercentage / 100f);
                        item.MarkDirty();
                        yield return item;
                    }
                }
            }

            #endregion EngineItems
        }

        #region Interfaces

        public interface IFuelVehicle
        {
            [JsonProperty(PropertyName = "Amount Of Fuel To Spawn", Order = 23)] int spawnFuelAmount { get; set; }
            [JsonProperty(PropertyName = "Refund Fuel On Kill", Order = 26)] bool refundFuelOnKill { get; set; }
            [JsonProperty(PropertyName = "Refund Fuel On Crash", Order = 27)] bool refundFuelOnCrash { get; set; }
        }

        public interface IInventoryVehicle
        {
            [JsonProperty(PropertyName = "Refund Inventory On Kill", Order = 28)] bool refundInventoryOnKill { get; set; }
            [JsonProperty(PropertyName = "Refund Inventory On Crash", Order = 29)] bool refundInventoryOnCrash { get; set; }
            [JsonProperty(PropertyName = "Drop Inventory Items When Vehicle Recall", Order = 39)] bool dropInventoryOnRecall { get; set; }
        }

        public interface IModularVehicle
        {
            [JsonProperty(PropertyName = "Refund Engine Items On Kill", Order = 35)] bool refundEngineOnKill { get; set; }
            [JsonProperty(PropertyName = "Refund Engine Items On Crash", Order = 36)] bool refundEngineOnCrash { get; set; }
            [JsonProperty(PropertyName = "Refund Module Items On Kill", Order = 37)] bool refundModuleOnKill { get; set; }
            [JsonProperty(PropertyName = "Refund Module Items On Crash", Order = 38)] bool refundModuleOnCrash { get; set; }
        }

        #endregion Interfaces

        #region Structs

        public struct PermissionS
        {
            public double spawnCooldown;
            public double recallCooldown;
        }

        public struct ModuleItem
        {
            public string shortName;
            public float healthPercentage;
        }

        public struct EngineItem
        {
            public string shortName;
            public float conditionPercentage;
        }

        public struct PriceInfo
        {
            public int amount;
            public string displayName;
        }

        #endregion Structs

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
                    string prefix, prefixColor;
                    if (GetConfigValue(out prefix, "Chat Settings", "Chat Prefix") && GetConfigValue(out prefixColor, "Chat Settings", "Chat Prefix Color"))
                    {
                        configData.chatS.prefix = $"<color={prefixColor}>{prefix}</color>: ";
                    }
                }
                if (configData.version <= new VersionNumber(1, 7, 3))
                {
                    configData.normalVehicleS.sedanS.minDistanceForPlayers = 3f;
                    configData.normalVehicleS.chinookS.minDistanceForPlayers = 5f;
                    configData.normalVehicleS.rowboatS.minDistanceForPlayers = 2f;
                    configData.normalVehicleS.rhibS.minDistanceForPlayers = 3f;
                    configData.normalVehicleS.hotAirBalloonS.minDistanceForPlayers = 4f;
                    configData.normalVehicleS.ridableHorseS.minDistanceForPlayers = 1f;
                    configData.normalVehicleS.miniCopterS.minDistanceForPlayers = 2f;
                    configData.normalVehicleS.transportHelicopterS.minDistanceForPlayers = 4f;
                    foreach (var entry in configData.modularCarS)
                    {
                        switch (entry.Value.chassisType)
                        {
                            case ChassisType.Small:
                                entry.Value.minDistanceForPlayers = 2f;
                                break;

                            case ChassisType.Medium:
                                entry.Value.minDistanceForPlayers = 2.5f;
                                break;

                            case ChassisType.Large:
                                entry.Value.minDistanceForPlayers = 3f;
                                break;

                            default: continue;
                        }
                    }
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

        public StoredData storedData { get; private set; }

        public class StoredData
        {
            public readonly Dictionary<ulong, Dictionary<string, Vehicle>> playerData = new Dictionary<ulong, Dictionary<string, Vehicle>>();

            public int ActiveVehiclesCount(ulong playerID)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    return 0;
                }

                int count = 0;
                foreach (var vehicle in vehicles.Values)
                {
                    if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
                    {
                        count++;
                    }
                }
                return count;
            }

            public Dictionary<string, Vehicle> GetPlayerVehicles(ulong playerID, bool readOnly = true)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    if (!readOnly)
                    {
                        vehicles = new Dictionary<string, Vehicle>();
                        playerData.Add(playerID, vehicles);
                        return vehicles;
                    }
                    return null;
                }
                return vehicles;
            }

            public bool IsVehiclePurchased(ulong playerID, string vehicleType, out Vehicle vehicle)
            {
                vehicle = GetVehicleLicense(playerID, vehicleType);
                if (vehicle == null)
                {
                    return false;
                }
                return true;
            }

            public Vehicle GetVehicleLicense(ulong playerID, string vehicleType)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    return null;
                }
                Vehicle vehicle;
                if (!vehicles.TryGetValue(vehicleType, out vehicle))
                {
                    return null;
                }
                return vehicle;
            }

            public bool HasVehicleLicense(ulong playerID, string vehicleType)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    return false;
                }
                return vehicles.ContainsKey(vehicleType);
            }

            public bool AddVehicleLicense(ulong playerID, string vehicleType)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    vehicles = new Dictionary<string, Vehicle>();
                    playerData.Add(playerID, vehicles);
                }
                if (vehicles.ContainsKey(vehicleType))
                {
                    return false;
                }
                vehicles.Add(vehicleType, new Vehicle());
                Instance.SaveData();
                return true;
            }

            public bool RemoveVehicleLicense(ulong playerID, string vehicleType)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    return false;
                }

                if (!vehicles.Remove(vehicleType))
                {
                    return false;
                }
                Instance.SaveData();
                return true;
            }

            public List<string> GetVehicleLicenseNames(ulong playerID)
            {
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    return new List<string>();
                }
                return vehicles.Keys.ToList();
            }

            public void PurchaseAllVehicles(ulong playerID)
            {
                bool changed = false;
                Dictionary<string, Vehicle> vehicles;
                if (!playerData.TryGetValue(playerID, out vehicles))
                {
                    vehicles = new Dictionary<string, Vehicle>();
                    playerData.Add(playerID, vehicles);
                }
                foreach (var vehicleType in Instance.allBaseVehicleSettings.Keys)
                {
                    if (!vehicles.ContainsKey(vehicleType))
                    {
                        vehicles.Add(vehicleType, new Vehicle());
                        changed = true;
                    }
                }
                if (changed) Instance.SaveData();
            }

            public void AddLicenseForAllPlayers(string vehicleType)
            {
                foreach (var entry in playerData)
                {
                    if (!entry.Value.ContainsKey(vehicleType))
                    {
                        entry.Value.Add(vehicleType, new Vehicle());
                    }
                }
            }

            public void RemoveLicenseForAllPlayers(string vehicleType)
            {
                foreach (var entry in playerData)
                {
                    entry.Value.Remove(vehicleType);
                }
            }

            public void ResetPlayerData()
            {
                foreach (var vehicleEntries in playerData)
                {
                    foreach (var vehicleEntry in vehicleEntries.Value)
                    {
                        vehicleEntry.Value.Reset();
                    }
                }
            }
        }

        public class Vehicle
        {
            public uint entityID;
            public double lastDeath;
            [JsonIgnore] public ulong playerID;
            [JsonIgnore] public BaseEntity entity;
            [JsonIgnore] public string vehicleType;
            [JsonIgnore] public double lastRecall;
            [JsonIgnore] public double lastDismount;

            public void OnDismount() => lastDismount = TimeEx.currentTimestamp;

            public void OnRecall() => lastRecall = TimeEx.currentTimestamp;

            public void OnDeath()
            {
                entity = null;
                entityID = 0;
                lastDeath = TimeEx.currentTimestamp;
            }

            public void Reset()
            {
                entityID = 0;
                lastDeath = 0;
            }
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

        private void OnNewSave(string filename)
        {
            if (configData.globalS.clearVehicleOnWipe)
            {
                ClearData();
            }
            else
            {
                storedData.ResetPlayerData();
                SaveData();
            }
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private void Print(ConsoleSystem.Arg arg, string message)
        {
            var player = arg.Player();
            if (player == null) Puts(message);
            else PrintToConsole(player, message);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help"] = "These are the available commands:",
                ["HelpLicence1"] = "<color=#4DFF4D>/{0}</color> -- To buy a vehicle",
                ["HelpLicence2"] = "<color=#4DFF4D>/{0}</color> -- To spawn a vehicle",
                ["HelpLicence3"] = "<color=#4DFF4D>/{0}</color> -- To recall a vehicle",
                ["HelpLicence4"] = "<color=#4DFF4D>/{0}</color> -- To kill a vehicle",
                ["HelpLicence5"] = "<color=#4DFF4D>/{0}</color> -- To buy, spawn or recall a <color=#009EFF>{1}</color>",
                //["HelpLicence6"] = "<color=#4DFF4D>/{0}</color> -- To kill a <color=#009EFF>{1}</color>",

                ["PriceFormat"] = "<color=#FF1919>{0}</color> x{1}",
                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- To buy a <color=#009EFF>{2}</color>",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To buy a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- To spawn a <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To spawn a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- To recall a <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To recall a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- To kill a <color=#009EFF>{2}</color>",
                ["HelpKillCustom"] = "<color=#4DFF4D>/{0} {1}</color> or <color=#4DFF4D>/{2}</color>  -- To kill a <color=#009EFF>{3}</color>",

                ["NotAllowed"] = "You do not have permission to use this command.",
                ["RaidBlocked"] = "<color=#FF1919>You may not do that while raid blocked</color>.",
                ["CombatBlocked"] = "<color=#FF1919>You may not do that while combat blocked</color>.",
                ["OptionNotFound"] = "This <color=#009EFF>{0}</color> option doesn't exist.",
                ["VehiclePurchased"] = "You have purchased a <color=#009EFF>{0}</color>, type <color=#4DFF4D>/{1}</color> for more information.",
                ["VehicleAlreadyPurchased"] = "You have already purchased <color=#009EFF>{0}</color>.",
                ["VehicleCannotBeBought"] = "<color=#009EFF>{0}</color> is unpurchasable",
                ["VehicleNotOut"] = "<color=#009EFF>{0}</color> is not out, type <color=#4DFF4D>/{1}</color> for more information.",
                ["AlreadyVehicleOut"] = "You already have a <color=#009EFF>{0}</color> outside, type <color=#4DFF4D>/{1}</color> for more information.",
                ["VehicleNotYetPurchased"] = "You have not yet purchased a <color=#009EFF>{0}</color>, type <color=#4DFF4D>/{1}</color> for more information.",
                ["VehicleSpawned"] = "You spawned your <color=#009EFF>{0}</color>.",
                ["VehicleRecalled"] = "You recalled your <color=#009EFF>{0}</color>.",
                ["VehicleKilled"] = "You killed your <color=#009EFF>{0}</color>.",
                ["VehicleOnSpawnCooldown"] = "You must wait <color=#FF1919>{0}</color> seconds before you can spawn your <color=#009EFF>{1}</color>.",
                ["VehicleOnRecallCooldown"] = "You must wait <color=#FF1919>{0}</color> seconds before you can recall your <color=#009EFF>{1}</color>.",
                ["NotLookingAtWater"] = "You must be looking at water to spawn or recall a <color=#009EFF>{0}</color>.",
                ["BuildingBlocked"] = "You can't spawn a <color=#009EFF>{0}</color> appear if you don't have the building privileges.",
                ["RefundedVehicleItems"] = "Your <color=#009EFF>{0}</color> vehicle items was refunded to your inventory.",
                ["PlayerMountedOnVehicle"] = "It cannot be recalled or killed when players mounted on your <color=#009EFF>{0}</color>.",
                ["PlayerInSafeZone"] = "You cannot spawn or recall your <color=#009EFF>{0}</color> in the safe zone.",
                ["VehicleInventoryDropped"] = "Your <color=#009EFF>{0}</color> vehicle inventory cannot be recalled, it have dropped to the ground.",
                ["NoResourcesToPurchaseVehicle"] = "You don't have enough resources to buy a <color=#009EFF>{0}</color>. You are missing: \n{1}",
                ["NoResourcesToSpawnVehicle"] = "You don't have enough resources to spawn a <color=#009EFF>{0}</color>. You are missing: \n{1}",
                ["NoResourcesToSpawnVehicleBypass"] = "You don't have enough resources to bypass the cooldown to spawn a <color=#009EFF>{0}</color>. You are missing: \n{1}",
                ["NoResourcesToRecallVehicle"] = "You don't have enough resources to recall a <color=#009EFF>{0}</color>. You are missing: \n{1}",
                ["NoResourcesToRecallVehicleBypass"] = "You don't have enough resources to bypass the cooldown to recall a <color=#009EFF>{0}</color>. You are missing: \n{1}",
                ["MountedOrParented"] = "You cannot spawn or recall a <color=#009EFF>{0}</color> when mounted or parented.",
                ["RecallTooFar"] = "You must be within <color=#FF1919>{0}</color> meters of <color=#009EFF>{1}</color> to recall.",
                ["KillTooFar"] = "You must be within <color=#FF1919>{0}</color> meters of <color=#009EFF>{1}</color> to kill.",
                ["PlayersOnNearby"] = "You cannot spawn or recall a <color=#009EFF>{0}</color> when there are players near the position you are looking at.",
                ["RecallWasBlocked"] = "An external plugin blocked you from recalling a <color=#009EFF>{0}</color>.",
                ["SpawnWasBlocked"] = "An external plugin blocked you from spawning a <color=#009EFF>{0}</color>.",
                ["VehiclesLimit"] = "You can have up to <color=#009EFF>{0}</color> vehicles at a time",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help"] = "可用命令列表:",
                ["HelpLicence1"] = "<color=#4DFF4D>/{0}</color> -- 购买一辆载具",
                ["HelpLicence2"] = "<color=#4DFF4D>/{0}</color> -- 生成一辆载具",
                ["HelpLicence3"] = "<color=#4DFF4D>/{0}</color> -- 召回一辆载具",
                ["HelpLicence4"] = "<color=#4DFF4D>/{0}</color> -- 摧毁一辆载具",
                ["HelpLicence5"] = "<color=#4DFF4D>/{0}</color> -- 购买，生成，召回一辆 <color=#009EFF>{1}</color>",
                //["HelpLicence6"] = "<color=#4DFF4D>/{0}</color> -- 摧毁一辆 <color=#009EFF>{1}</color>",

                ["PriceFormat"] = "<color=#FF1919>{0}</color> x{1}",
                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- 购买一辆 <color=#009EFF>{2}</color>",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 购买一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- 生成一辆 <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 生成一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- 召回一辆 <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 召回一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- 摧毁一辆 <color=#009EFF>{2}</color>",
                ["HelpKillCustom"] = "<color=#4DFF4D>/{0} {1}</color> 或者 <color=#4DFF4D>/{2}</color>  -- 摧毁一辆 <color=#009EFF>{3}</color>",

                ["NotAllowed"] = "您没有权限使用该命令",
                ["RaidBlocked"] = "<color=#FF1919>您被突袭阻止了，不能使用该命令</color>",
                ["CombatBlocked"] = "<color=#FF1919>您被战斗阻止了，不能使用该命令</color>",
                ["OptionNotFound"] = "选项 <color=#009EFF>{0}</color> 不存在",
                ["VehiclePurchased"] = "您购买了 <color=#009EFF>{0}</color>, 输入 <color=#4DFF4D>/{1}</color> 了解更多信息",
                ["VehicleAlreadyPurchased"] = "您已经购买了 <color=#009EFF>{0}</color>",
                ["VehicleCannotBeBought"] = "<color=#009EFF>{0}</color> 是不可购买的",
                ["VehicleNotOut"] = "您还没有生成您的 <color=#009EFF>{0}</color>, 输入 <color=#4DFF4D>/{1}</color> 了解更多信息",
                ["AlreadyVehicleOut"] = "您已经生成了您的 <color=#009EFF>{0}</color>, 输入 <color=#4DFF4D>/{1}</color> 了解更多信息",
                ["VehicleNotYetPurchased"] = "您还没有购买 <color=#009EFF>{0}</color>, 输入 <color=#4DFF4D>/{1}</color> 了解更多信息",
                ["VehicleSpawned"] = "您生成了您的 <color=#009EFF>{0}</color>",
                ["VehicleRecalled"] = "您召回了您的 <color=#009EFF>{0}</color>",
                ["VehicleKilled"] = "您摧毁了您的 <color=#009EFF>{0}</color>",
                ["VehicleOnSpawnCooldown"] = "您必须等待 <color=#FF1919>{0}</color> 秒，才能生成您的 <color=#009EFF>{1}</color>",
                ["VehicleOnRecallCooldown"] = "您必须等待 <color=#FF1919>{0}</color> 秒，才能召回您的 <color=#009EFF>{1}</color>",
                ["NotLookingAtWater"] = "您必须看着水面才能生成您的 <color=#009EFF>{0}</color>",
                ["BuildingBlocked"] = "您没有领地柜权限，无法生成您的 <color=#009EFF>{0}</color>",
                ["RefundedVehicleItems"] = "您的 <color=#009EFF>{0}</color> 载具物品已经归还回您的库存",
                ["PlayerMountedOnVehicle"] = "您的 <color=#009EFF>{0}</color> 上坐着玩家，无法被召回或摧毁",
                ["PlayerInSafeZone"] = "您不能在安全区域内生成或召回您的 <color=#009EFF>{0}</color>",
                ["VehicleInventoryDropped"] = "您的 <color=#009EFF>{0}</color> 载具物品不能召回，它已经掉落在地上了",
                ["NoResourcesToPurchaseVehicle"] = "您没有足够的资源购买 <color=#009EFF>{0}</color>，还需要: \n{1}",
                ["NoResourcesToSpawnVehicle"] = "您没有足够的资源生成 <color=#009EFF>{0}</color>，还需要: \n{1}",
                ["NoResourcesToSpawnVehicleBypass"] = "您没有足够的资源绕过冷却时间来生成 <color=#009EFF>{0}</color>，还需要: \n{1}",
                ["NoResourcesToRecallVehicle"] = "您没有足够的资源召回 <color=#009EFF>{0}</color>，还需要: \n{1}",
                ["NoResourcesToRecallVehicleBypass"] = "您没有足够的资源绕过冷却时间来召回 <color=#009EFF>{0}</color>，还需要: \n{1}",
                ["MountedOrParented"] = "当您坐着或者在附着在实体上时无法生成或召回 <color=#009EFF>{0}</color>",
                ["RecallTooFar"] = "您必须在 <color=#FF1919>{0}</color> 米内才能召回您的 <color=#009EFF>{1}</color>",
                ["KillTooFar"] = "您必须在 <color=#FF1919>{0}</color> 米内才能摧毁您的 <color=#009EFF>{1}</color>",
                ["PlayersOnNearby"] = "您正在看着的位置附近有玩家时无法生成或召回 <color=#009EFF>{0}</color>",
                ["RecallWasBlocked"] = "有其他插件阻止您召回 <color=#009EFF>{0}</color>.",
                ["SpawnWasBlocked"] = "有其他插件阻止您生成 <color=#009EFF>{0}</color>.",
                ["VehiclesLimit"] = "您在同一时间内最多可以拥有 <color=#009EFF>{0}</color> 辆载具",
            }, this, "zh-CN");
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help"] = "Список доступных команд:",
                ["HelpLicence1"] = "<color=#4DFF4D>/{0}</color> -- Купить транспорт",
                ["HelpLicence2"] = "<color=#4DFF4D>/{0}</color> -- Создать транспорт",
                ["HelpLicence3"] = "<color=#4DFF4D>/{0}</color> -- Вызвать транспорт",
                ["HelpLicence4"] = "<color=#4DFF4D>/{0}</color> -- Уничтожить транспорт",
                ["HelpLicence5"] = "<color=#4DFF4D>/{0}</color> -- Купить, создать, или вызвать <color=#009EFF>{1}</color>",
                //["HelpLicence6"] = "<color=#4DFF4D>/{0}</color> -- Уничтожить <color=#009EFF>{1}</color>",

                ["PriceFormat"] = "<color=#FF1919>{0}</color> x{1}",
                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- Купить <color=#009EFF>{2}</color>.",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Купить <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- Создать <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызывать <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызвать <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызвать <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- Уничтожить <color=#009EFF>{2}</color>",
                ["HelpKillCustom"] = "<color=#4DFF4D>/{0} {1}</color> или же <color=#4DFF4D>/{2}</color>  -- Уничтожить <color=#009EFF>{3}</color>",

                ["NotAllowed"] = "У вас нет разрешения для использования данной команды.",
                ["RaidBlocked"] = "<color=#FF1919>Вы не можете это сделать из-за блокировки (рейд)</color>.",
                ["CombatBlocked"] = "<color=#FF1919>Вы не можете это сделать из-за блокировки (бой)</color>.",
                ["OptionNotFound"] = "Опция <color=#009EFF>{0}</color> не существует.",
                ["VehiclePurchased"] = "Вы приобрели <color=#009EFF>{0}</color>, напишите <color=#4DFF4D>/{1}</color> для получения дополнительной информации.",
                ["VehicleAlreadyPurchased"] = "Вы уже приобрели <color=#009EFF>{0}</color>.",
                ["VehicleCannotBeBought"] = "<color=#009EFF>{0}</color> приобрести невозможно",
                ["VehicleNotOut"] = "<color=#009EFF>{0}</color> отсутствует. Напишите <color=#4DFF4D>/{1}</color> для получения дополнительной информации.",
                ["AlreadyVehicleOut"] = "У вас уже есть <color=#009EFF>{0}</color>, напишите <color=#4DFF4D>/{1}</color>  для получения дополнительной информации.",
                ["VehicleNotYetPurchased"] = "Вы ещё не приобрели <color=#009EFF>{0}</color>. Напишите <color=#4DFF4D>/{1}</color> для получения дополнительной информации.",
                ["VehicleSpawned"] = "Вы создали ваш <color=#009EFF>{0}</color>.",
                ["VehicleRecalled"] = "Вы вызвали ваш <color=#009EFF>{0}</color>.",
                ["VehicleKilled"] = "Вы уничтожили ваш <color=#009EFF>{0}</color>.",
                ["VehicleOnSpawnCooldown"] = "Вам необходимо подождать <color=#FF1919>{0}</color> секунд прежде, чем создать свой <color=#009EFF>{1}</color>.",
                ["VehicleOnRecallCooldown"] = "Вам необходимо подождать <color=#FF1919>{0}</color> секунд прежде, чем вызвать свой <color=#009EFF>{1}</color>.",
                ["NotLookingAtWater"] = "Вы должны смотреть на воду, чтобы создать или вызвать <color=#009EFF>{0}</color>.",
                ["BuildingBlocked"] = "Вы не можете создать <color=#009EFF>{0}</color> если отсутствует право строительства.",
                ["RefundedVehicleItems"] = "Запчасти от вашего <color=#009EFF>{0}</color> были возвращены в ваш инвентарь.",
                ["PlayerMountedOnVehicle"] = "Нельзя вызвать, когда игрок находится в вашем <color=#009EFF>{0}</color>.",
                ["PlayerInSafeZone"] = "Вы не можете создать, или вызвать ваш <color=#009EFF>{0}</color> в безопасной зоне.",
                ["VehicleInventoryDropped"] = "Инвентарь из вашего <color=#009EFF>{0}</color> не может быть вызван, он выброшен на землю.",
                ["NoResourcesToPurchaseVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: \n{1}",
                ["NoResourcesToSpawnVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: \n{1}",
                ["NoResourcesToSpawnVehicleBypass"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: \n{1}",
                ["NoResourcesToRecallVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: \n{1}",
                ["NoResourcesToRecallVehicleBypass"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: \n{1}",
                ["MountedOrParented"] = "Вы не можете создать <color=#009EFF>{0}</color> когда сидите или привязаны к объекту.",
                ["RecallTooFar"] = "Вы должны быть в пределах <color=#FF1919>{0}</color> метров от <color=#009EFF>{1}</color>, чтобы вызывать.",
                ["KillTooFar"] = "Вы должны быть в пределах <color=#FF1919>{0}</color> метров от <color=#009EFF>{1}</color>, уничтожить.",
                ["PlayersOnNearby"] = "Вы не можете создать <color=#009EFF>{0}</color> когда рядом с той позицией, на которую вы смотрите, есть игроки.",
                ["RecallWasBlocked"] = "Внешний плагин заблокировал вам вызвать <color=#009EFF>{0}</color>.",
                ["SpawnWasBlocked"] = "Внешний плагин заблокировал вам создать <color=#009EFF>{0}</color>.",
                ["VehiclesLimit"] = "У вас может быть до <color=#009EFF>{0}</color> автомобилей одновременно",
            }, this, "ru");
        }

        #endregion LanguageFile
    }
}