//#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [Info("Vehicle Licence", "Sorrow/TheDoc/Arainrr", "1.7.4")]
    [Description("Allows players to buy vehicles and then spawn or store it")]
    public class VehicleLicence : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Economics, ServerRewards, Friends, Clans, NoEscape;

        private const string PERMISSION_USE = "vehiclelicence.use";
        private const string PERMISSION_ALL = "vehiclelicence.all";
        private const string PERMISSION_BYPASS_COST = "vehiclelicence.bypasscost";

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
        public static VehicleLicence Instance { get; private set; }
        public readonly Dictionary<BaseEntity, Vehicle> vehiclesCache = new Dictionary<BaseEntity, Vehicle>();
        public readonly Dictionary<string, BaseVehicleS> allBaseVehicleSettings = new Dictionary<string, BaseVehicleS>();

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
            LoadDefaultMessages();
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

            foreach (var baseVehicleS in allBaseVehicleSettings.Values)
            {
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
                if (configData.chatS.useUniversalCommand)
                {
                    foreach (var command in baseVehicleS.commands)
                    {
                        if (string.IsNullOrEmpty(command)) continue;
                        cmd.AddChatCommand(command, this, nameof(CmdUniversal));
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
        }

        private void OnServerInitialized()
        {
            if (configData.globalS.storeVehicle)
            {
                var currentTimestamp = TimeEx.currentTimestamp;
                foreach (var playerEntry in storedData.playerData)
                {
                    foreach (var entry in playerEntry.Value)
                    {
                        entry.Value.lastRecall = entry.Value.lastDismount = currentTimestamp;
                        entry.Value.playerID = playerEntry.Key;
                        entry.Value.vehicleType = entry.Key;
                        if (entry.Value.entityID == 0)
                        {
                            continue;
                        }
                        entry.Value.entity = BaseNetworkable.serverEntities.Find(entry.Value.entityID) as BaseEntity;
                        if (entry.Value.entity == null || entry.Value.entity.IsDestroyed)
                        {
                            entry.Value.entityID = 0;
                        }
                        else
                        {
                            vehiclesCache.Add(entry.Value.entity, entry.Value);
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
                foreach (var entry in vehiclesCache.ToList())
                {
                    if (entry.Key != null && !entry.Key.IsDestroyed)
                    {
                        RefundVehicleItems(entry.Value, entry.Key, isUnload: true);
                        entry.Key.Kill(BaseNetworkable.DestroyMode.Gib);
                    }
                }

                foreach (var vehicles in storedData.playerData.Values)
                {
                    foreach (var entry in vehicles)
                    {
                        entry.Value.entityID = 0;
                    }
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
                PurchaseAllVehicles(player.userID);
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
            if (configData.globalS.preventDriverSeat && vehicleParent.HasMountPoints() && entity != vehicleParent.mountPoints[0].mountable) return null;
            return false;
        }

        private void OnEntityTakeDamage(BaseCombatEntity entity, HitInfo hitInfo)
        {
            if (entity == null | hitInfo?.damageTypes == null) return;
            if (hitInfo.damageTypes.Get(Rust.DamageType.Decay) > 0)
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
            if (triggerHurtNotChild?.SourceEntity == null || player == null || !player.userID.IsSteamId()) return null;
            var sourceEntity = triggerHurtNotChild.SourceEntity;
            if (vehiclesCache.ContainsKey(sourceEntity))
            {
                var baseVehicle = sourceEntity as BaseVehicle;
                if (baseVehicle != null)
                {
                    /*
                    var dismountPositions = baseVehicle.dismountPositions;
                    var position = dismountPositions.Select(x => x.position)
                        .OrderBy(x => Vector3.Distance(x, player.transform.position)).FirstOrDefault();
                    MoveToPosition(player, position);
                    */
                    MoveToPosition(player, triggerHurtNotChild.transform.position + triggerHurtNotChild.transform.right * 3f);
                }
                //triggerHurtNotChild.enabled = false;
                return false;
            }
            return null;
        }

        //HotAirBalloon
        private object OnEntityEnter(TriggerHurt triggerHurt, BasePlayer player)
        {
            if (triggerHurt == null || player == null || !player.userID.IsSteamId()) return null;
            var sourceEntity = triggerHurt.gameObject?.ToBaseEntity();
            if (sourceEntity == null) return null;
            if (vehiclesCache.ContainsKey(sourceEntity))
            {
                MoveToPosition(player, triggerHurt.transform.position + triggerHurt.transform.forward * 2f);
                //triggerHurt.enabled = false;
                return false;
            }
            return null;
        }

        #endregion Oxide Hooks

        #region Methods

        private void CheckEntity(BaseCombatEntity entity, bool isCrash = false)
        {
            if (entity == null) return;
            Vehicle vehicle;
            if (!vehiclesCache.TryGetValue(entity, out vehicle)) return;
            vehiclesCache.Remove(entity);
            RefundVehicleItems(vehicle, entity, isCrash);

            Dictionary<string, Vehicle> vehicles;
            if (storedData.playerData.TryGetValue(vehicle.playerID, out vehicles))
            {
                var baseVehicleS = GetBaseVehicleS(vehicle.vehicleType);
                if (isCrash && baseVehicleS.removeLicenseOnceCrash)
                {
                    vehicles.Remove(vehicle.vehicleType);
                    return;
                }
                if (vehicles.TryGetValue(vehicle.vehicleType, out vehicle))
                {
                    vehicle.OnDeath();
                }
            }
        }

        #region CheckVehicles

        private void CheckVehicles()
        {
            var currentTimestamp = TimeEx.currentTimestamp;
            foreach (var entry in vehiclesCache.ToList())
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

        private static void CanModularCarRefund(BaseVehicleS baseVehicleS, bool isCrash, bool isUnload, out bool refundFuel, out bool refundInventory, out bool refundEngine, out bool refundModule)
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

        private void RefundVehicleItems(Vehicle vehicle, BaseEntity entity = null, bool isCrash = false, bool isUnload = false)
        {
            if (entity == null) entity = vehicle.entity;
            if (entity == null) return;
            var baseVehicleS = GetBaseVehicleS(vehicle.vehicleType);
            if (baseVehicleS == null) return;

            NormalVehicleType normalVehicleType;
            var collect = new List<Item>();
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
                CanModularCarRefund(baseVehicleS, isCrash, isUnload, out refundFuel, out refundInventory, out refundEngine, out refundModule);

                foreach (var moduleEntity in modularCar.AttachedModuleEntities.ToArray())
                {
                    var moduleEngine = moduleEntity as VehicleModuleEngine;
                    if (moduleEngine != null)
                    {
                        var engineContainer = moduleEngine.GetContainer()?.inventory;
                        if (engineContainer != null)
                        {
                            if (refundEngine)
                            {
                                collect.AddRange(engineContainer.itemList);
                            }
                            else
                            {
                                engineContainer.Clear();
                            }
                        }
                        continue;
                    }
                    var moduleStorage = moduleEntity as VehicleModuleStorage;
                    if (moduleStorage != null && refundInventory)
                    {
                        var storageContainer = moduleStorage.GetContainer()?.inventory;
                        if (storageContainer != null) collect.AddRange(storageContainer.itemList);
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
                /*var chassisContainer = modularCar.Inventory?.ChassisContainer;
                if (chassisContainer != null)
                {
                    collect.AddRange(chassisContainer.itemList);
                }*/
            }
            if (collect.Count <= 0) return;
            var player = RustCore.FindPlayerById(vehicle.playerID);
            if (player == null)
            {
                DropItemContainer(entity, vehicle.playerID, collect);
            }
            else
            {
                foreach (var item in collect.ToArray())
                {
                    player.GiveItem(item);
                }
                if (player.IsConnected)
                {
                    Print(player, Lang("RefundedVehicleItems", player.UserIDString, baseVehicleS.displayName));
                }
            }
        }

        private static void DropItemContainer(BaseEntity entity, ulong playerID, List<Item> collect)
        {
            var droppedItemContainer = GameManager.server.CreateEntity(PREFAB_ITEM_DROP, entity.GetDropPosition(), entity.transform.rotation) as DroppedItemContainer;
            droppedItemContainer.inventory = new ItemContainer();
            droppedItemContainer.inventory.ServerInitialize(null, Mathf.Min(collect.Count, droppedItemContainer.maxItemCount));
            droppedItemContainer.inventory.GiveUID();
            droppedItemContainer.inventory.entityOwner = droppedItemContainer;
            droppedItemContainer.inventory.SetFlag(ItemContainer.Flag.NoItemInput, true);
            foreach (var item in collect.ToArray())
            {
                if (!item.MoveToContainer(droppedItemContainer.inventory))
                {
                    item.DropAndTossUpwards(droppedItemContainer.transform.position);
                }
            }

            droppedItemContainer.OwnerID = playerID;
            droppedItemContainer.Spawn();
        }

        #endregion Refund

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
                foreach (var moduleEntity in modularCar.AttachedModuleEntities.ToArray())
                {
                    if (moduleEntity is VehicleModuleEngine) continue;
                    var moduleStorage = moduleEntity as VehicleModuleStorage;
                    if (moduleStorage != null)
                    {
                        droppedItemContainer = moduleStorage.GetContainer()?.inventory?.Drop(PREFAB_ITEM_DROP, entity.GetDropPosition(),
                            entity.transform.rotation);
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
            var collect = new List<Item>();
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
            foreach (var item in collect) item.Remove();
            missingResources = null;
            return true;
        }

        private bool CanPay(BasePlayer player, Dictionary<string, PriceInfo> prices, out string missingResources)
        {
            var collect = new Dictionary<string, int>();
            foreach (var entry in prices)
            {
                if (entry.Value.amount <= 0) continue;
                int missingAmount;
                var itemDefinition = ItemManager.FindItemDefinition(entry.Key);
                if (itemDefinition != null) missingAmount = entry.Value.amount - player.inventory.GetAmount(itemDefinition.itemid);
                else missingAmount = MissingMoney(entry.Key, entry.Value.amount, player.userID);
                if (missingAmount <= 0) continue;
                if (!collect.ContainsKey(entry.Value.displayName))
                {
                    collect.Add(entry.Value.displayName, missingAmount);
                }
                else
                {
                    collect[entry.Value.displayName] += missingAmount;
                }
            }
            if (collect.Count > 0)
            {
                missingResources = collect.Aggregate(string.Empty, (current, entry) => current + $"\n* <color=#FF1919>{entry.Key}</color> x{entry.Value}");
                return false;
            }
            missingResources = null;
            return true;
        }

        private int MissingMoney(string key, int price, ulong playerID)
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

        #region PurchaseAllVehicles

        private void PurchaseAllVehicles(ulong playerID)
        {
            bool changed = false;
            Dictionary<string, Vehicle> vehicles;
            var allVehicleNames = GetAllVehicleNames();
            if (!storedData.playerData.TryGetValue(playerID, out vehicles))
            {
                vehicles = allVehicleNames.ToDictionary(x => x, y => new Vehicle());
                storedData.playerData.Add(playerID, vehicles);
                changed = true;
            }
            else
            {
                foreach (var vehicleName in allVehicleNames)
                {
                    if (!vehicles.ContainsKey(vehicleName))
                    {
                        vehicles.Add(vehicleName, new Vehicle());
                        changed = true;
                    }
                }
            }
            if (changed) SaveData();
        }

        private IEnumerable<string> GetAllVehicleNames()
        {
            foreach (var vehicleName in allBaseVehicleSettings.Keys)
            {
                yield return vehicleName;
            }
        }

        #endregion PurchaseAllVehicles

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

        private bool PlayerIsBlocked(BasePlayer player)
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

        #region Helpers

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
                    var mounted = mountPointInfo.mountable?._mounted;
                    if (mounted != null)
                    {
                        mountPointInfo.mountable.DismountPlayer(mounted);
                    }
                }
            }
            var players = entity.GetComponentsInChildren<BasePlayer>();
            foreach (var p in players)
            {
                p.SetParent(null, true, true);
            }
        }

        private static Vector3 GetLookingAtGroundPos(BasePlayer player, float distance)
        {
            RaycastHit hit;
            var ray = player.eyes.HeadRay();
            if (Physics.Raycast(ray, out hit, distance, LAYER_GROUND))
                return hit.point;
            var position = ray.origin + ray.direction * distance;
            if (Physics.Raycast(position + Vector3.up * 200, Vector3.down, out hit, 500, LAYER_GROUND))
                return hit.point;
            position.y = TerrainMeta.HeightMap.GetHeight(position);
            return position;
        }

        private static bool PositionIsInWater(Vector3 position)
        {
            var colliders = Facepunch.Pool.GetList<Collider>();
            Vis.Colliders(position, 0.5f, colliders);
            var flag = colliders.Any(x => x.gameObject?.layer == (int)Rust.Layer.Water);
            Facepunch.Pool.FreeList(ref colliders);
            return flag;
            //return WaterLevel.Test(lookingAt);
        }

        private static Vector3 GetGroundPosition(Vector3 position)
        {
            RaycastHit hitInfo;
            position.y = Physics.Raycast(position + Vector3.up * 200, Vector3.down, out hitInfo, 500f, LAYER_GROUND)
                ? hitInfo.point.y
                : TerrainMeta.HeightMap.GetHeight(position);
            return position;
        }

        private static void MoveToPosition(BasePlayer player, Vector3 position)
        {
            player.MovePosition(position);
            player.SendNetworkUpdateImmediate();
            player.ForceUpdateTriggers();
            if (player.HasParent()) player.SetParent(null, true, true);
            player.ClientRPCPlayer(null, player, "ForcePositionTo", position);
        }

        #endregion Helpers

        #endregion Methods

        #region API

        private BaseEntity GetLicensedVehicle(ulong playerID, string license)
        {
            Dictionary<string, Vehicle> vehicles;
            if (storedData.playerData.TryGetValue(playerID, out vehicles))
            {
                Vehicle vehicle;
                if (vehicles.TryGetValue(license, out vehicle))
                {
                    return vehicle.entity;
                }
            }
            return null;
        }

        private bool IsLicensedVehicle(BaseEntity entity)
        {
            foreach (var playerData in storedData.playerData)
            {
                foreach (var vehicle in playerData.Value)
                {
                    if (vehicle.Value.entity == entity)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool HasVehicleLicense(ulong playerID, string license)
        {
            Dictionary<string, Vehicle> vehicles;
            return storedData.playerData.TryGetValue(playerID, out vehicles) && vehicles.ContainsKey(license);
        }

        private bool RemoveVehicleLicense(ulong playerID, string license)
        {
            Dictionary<string, Vehicle> vehicles;
            return storedData.playerData.TryGetValue(playerID, out vehicles) && vehicles.Remove(license);
        }

        private bool AddVehicleLicense(ulong playerID, string license)
        {
            Dictionary<string, Vehicle> vehicles;
            if (!storedData.playerData.TryGetValue(playerID, out vehicles))
            {
                vehicles = new Dictionary<string, Vehicle> { [license.ToString()] = new Vehicle() };
                storedData.playerData.Add(playerID, vehicles);
            }
            else
            {
                if (!vehicles.ContainsKey(license))
                {
                    vehicles.Add(license, new Vehicle());
                }
                else return false;
            }
            SaveData();
            return true;
        }

        private List<string> GetVehicleLicenses(ulong playerID)
        {
            Dictionary<string, Vehicle> vehicles;
            if (storedData.playerData.TryGetValue(playerID, out vehicles))
            {
                return vehicles.Keys.ToList();
            }
            return null;
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
            command = command.ToLower();
            foreach (var entry in allBaseVehicleSettings)
            {
                if (entry.Value.commands.Any(x => x.ToLower() == command))
                {
                    HandleUniversalCmd(player, entry.Key);
                    return;
                }
            }
        }

        private void HandleUniversalCmd(BasePlayer player, string vehicleType)
        {
            if (!HasVehiclePermission(player, vehicleType))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            if (PlayerIsBlocked(player)) return;
            Vehicle vehicle;
            Dictionary<string, Vehicle> vehicles;
            storedData.playerData.TryGetValue(player.userID, out vehicles);
            if (vehicles != null && vehicles.TryGetValue(vehicleType, out vehicle))
            {
                string reason;
                bool checkWater = vehicleType == nameof(NormalVehicleType.Rowboat) || vehicleType == nameof(NormalVehicleType.RHIB);
                if (vehicle.entity != null && !vehicle.entity.IsDestroyed)//recall
                {
                    if (CanRecall(player, vehicle, vehicleType, checkWater, out reason))
                    {
                        RecallVehicle(player, vehicle, vehicleType, checkWater);
                        return;
                    }
                }
                else//spawn
                {
                    if (CanSpawn(player, vehicleType, checkWater, out reason))
                    {
                        SpawnVehicle(player, vehicleType, checkWater);
                        return;
                    }
                }
                Print(player, reason);
                return;
            }
            BuyVehicle(player, vehicleType); //buy
        }

        #endregion Universal Command

        #region Help Command

        private void CmdLicenseHelp(BasePlayer player, string command, string[] args)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Lang("Help", player.UserIDString));
            stringBuilder.AppendLine(Lang("HelpLicence1", player.UserIDString, configData.chatS.buyCommand));
            stringBuilder.AppendLine(Lang("HelpLicence2", player.UserIDString, configData.chatS.spawnCommand));
            stringBuilder.AppendLine(Lang("HelpLicence3", player.UserIDString, configData.chatS.recallCommand));
            stringBuilder.AppendLine(Lang("HelpLicence4", player.UserIDString, configData.chatS.killCommand));
            if (configData.chatS.useUniversalCommand)
            {
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        stringBuilder.AppendLine(Lang("HelpLicence5", player.UserIDString, entry.Value.commands[0],
                            entry.Value.displayName));
                    }
                }
            }
            Print(player, stringBuilder.ToString());
        }

        #endregion Help Command

        #region Remove Command

        [ConsoleCommand("vl.remove")]
        private void CCmdRemoveVehicle(ConsoleSystem.Arg arg)
        {
            if (arg.IsAdmin && arg.Args != null && arg.Args.Length == 2)
            {
                var option = arg.Args[0].ToLower();
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
                            foreach (var entry in storedData.playerData)
                            {
                                entry.Value.Remove(vehicleType);
                            }
                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            Print(arg, $"You successfully deleted the {vehicleName} vehicle of all players");
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
                                Print(arg, $"You successfully deleted the {vehicleName} vehicle of {target.displayName}");
                                return;
                            }

                            Print(arg, $"{target.displayName} has not purchased {vehicleName} vehicle and cannot be deleted");
                        }
                        return;
                }
            }
        }

        #endregion Remove Command

        #region Buy Command

        [ConsoleCommand("vl.buy")]
        private void CCmdBuyVehicle(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (arg.IsAdmin && arg.Args != null && arg.Args.Length == 2)
            {
                var option = arg.Args[0].ToLower();
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
                            foreach (var entry in storedData.playerData)
                            {
                                if (!entry.Value.ContainsKey(vehicleType))
                                    entry.Value.Add(vehicleType, new Vehicle());
                            }
                            var vehicleName = GetBaseVehicleS(vehicleType).displayName;
                            Print(arg, $"You successfully purchased the {vehicleName} vehicle for all players");
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
                                Print(arg, $"You successfully purchased the {vehicleName} vehicle for {target.displayName}");
                                return;
                            }

                            Print(arg, $"{target.displayName} has purchased {vehicleName} vehicle");
                        }
                        return;
                }
            }
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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        if (entry.Value.purchasePrices.Count > 0)
                        {
                            var prices = string.Join(", ", from p in entry.Value.purchasePrices select $"<color=#FF1919>{p.Value.displayName}</color> x{p.Value.amount}");
                            stringBuilder.AppendLine(Lang("HelpBuyPrice", player.UserIDString, configData.chatS.buyCommand, entry.Value.commands[0], entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpBuy", player.UserIDString, configData.chatS.buyCommand, entry.Value.commands[0], entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                return;
            }
            if (PlayerIsBlocked(player)) return;
            HandleBuyCmd(player, args[0].ToLower());
        }

        private void HandleBuyCmd(BasePlayer player, string option)
        {
            string vehicleType;
            if (IsValidOption(player, option, out vehicleType))
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
            Dictionary<string, Vehicle> vehicles;
            if (!storedData.playerData.TryGetValue(player.userID, out vehicles))
            {
                vehicles = new Dictionary<string, Vehicle>();
                storedData.playerData.Add(player.userID, vehicles);
            }
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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        if (entry.Value.spawnPrices.Count > 0)
                        {
                            var prices = string.Join(", ", from p in entry.Value.spawnPrices select $"<color=#FF1919>{p.Value.displayName}</color> x{p.Value.amount}");
                            stringBuilder.AppendLine(Lang("HelpSpawnPrice", player.UserIDString, configData.chatS.spawnCommand, entry.Value.commands[0], entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpSpawn", player.UserIDString, configData.chatS.spawnCommand, entry.Value.commands[0], entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                return;
            }
            if (PlayerIsBlocked(player)) return;
            HandleSpawnCmd(player, args[0].ToLower());
        }

        private void HandleSpawnCmd(BasePlayer player, string option)
        {
            string vehicleType;
            if (IsValidOption(player, option, out vehicleType))
            {
                string reason;
                bool checkWater = vehicleType == nameof(NormalVehicleType.Rowboat) || vehicleType == nameof(NormalVehicleType.RHIB);
                if (CanSpawn(player, vehicleType, checkWater, out reason))
                {
                    SpawnVehicle(player, vehicleType, checkWater);
                    return;
                }
                Print(player, reason);
            }
        }

        private bool CanSpawn(BasePlayer player, string vehicleType, bool checkWater, out string reason)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            if (!HasVehiclePermission(player, vehicleType, baseVehicleS))
            {
                reason = Lang("NotAllowed", player.UserIDString);
                return false;
            }
            if (player.IsBuildingBlocked())
            {
                reason = Lang("BuildingBlocked", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (player.GetMountedVehicle() != null || player.HasParent())
            {
                reason = Lang("MountedOrParented", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.preventSafeZone && player.InSafeZone())
            {
                reason = Lang("PlayerInSafeZone", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            Vehicle vehicle;
            Dictionary<string, Vehicle> vehicles;
            if (!storedData.playerData.TryGetValue(player.userID, out vehicles) || !vehicles.TryGetValue(vehicleType, out vehicle))
            {
                reason = Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand);
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
            {
                reason = Lang("AlreadyVehicleOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.recallCommand);
                return false;
            }
            if (!CheckPosition(player, baseVehicleS, checkWater, out reason))
            {
                return false;
            }
#if DEBUG
            if (player.IsAdmin)
            {
                reason = null;
                return true;
            }
#endif
            var spawnCooldown = GetSpawnCooldown(player, baseVehicleS);
            if (spawnCooldown > 0)
            {
                var timeLeft = Math.Ceiling(spawnCooldown - (TimeEx.currentTimestamp - vehicle.lastDeath));
                if (timeLeft > 0)
                {
                    reason = Lang("VehicleOnSpawnCooldown", player.UserIDString, timeLeft, baseVehicleS.displayName);
                    return false;
                }
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

        private void SpawnVehicle(BasePlayer player, string vehicleType, bool checkWater = false)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            var prefab = GetVehiclePrefab(vehicleType, baseVehicleS);
            if (string.IsNullOrEmpty(prefab)) return;
            Vector3 position; Quaternion rotation;
            GetVehicleSpawnPos(player, baseVehicleS, checkWater, vehicleType, out position, out rotation);
            var entity = GameManager.server.CreateEntity(prefab, position, rotation);
            if (entity == null) return;
            entity.enableSaving = configData.globalS.storeVehicle;
            entity.OwnerID = player.userID;
            entity.Spawn();

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

            var vehicle = new Vehicle { playerID = player.userID, vehicleType = vehicleType, entity = entity, entityID = entity.net.ID };
            vehicle.lastDismount = vehicle.lastRecall = TimeEx.currentTimestamp;
            vehiclesCache.Add(entity, vehicle);
            storedData.playerData[player.userID][vehicleType] = vehicle;
            Print(player, Lang("VehicleSpawned", player.UserIDString, baseVehicleS.displayName));
            Interface.CallHook("OnLicensedVehicleSpawned", entity, player, vehicleType);
        }

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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        if (entry.Value.recallPrices.Count > 0)
                        {
                            var prices = string.Join(", ", from p in entry.Value.recallPrices select $"<color=#FF1919>{p.Value.displayName}</color> x{p.Value.amount}");
                            stringBuilder.AppendLine(Lang("HelpRecallPrice", player.UserIDString, configData.chatS.recallCommand, entry.Value.commands[0], entry.Value.displayName, prices));
                        }
                        else
                        {
                            stringBuilder.AppendLine(Lang("HelpRecall", player.UserIDString, configData.chatS.recallCommand, entry.Value.commands[0], entry.Value.displayName));
                        }
                    }
                }
                Print(player, stringBuilder.ToString());
                return;
            }
            if (PlayerIsBlocked(player)) return;
            HandleRecallCmd(player, args[0].ToLower());
        }

        private void HandleRecallCmd(BasePlayer player, string option)
        {
            string vehicleType;
            if (IsValidOption(player, option, out vehicleType))
            {
                RecallVehicle(player, vehicleType);
            }
        }

        private bool RecallVehicle(BasePlayer player, string vehicleType)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            Vehicle vehicle;
            Dictionary<string, Vehicle> vehicles;
            if (!storedData.playerData.TryGetValue(player.userID, out vehicles) || !vehicles.TryGetValue(vehicleType, out vehicle))
            {
                Print(player, Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand));
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
            {
                string reason;
                bool checkWater = vehicleType == nameof(NormalVehicleType.Rowboat) || vehicleType == nameof(NormalVehicleType.RHIB);
                if (CanRecall(player, vehicle, vehicleType, checkWater, out reason))
                {
                    RecallVehicle(player, vehicle, vehicleType, checkWater);
                    return true;
                }
                Print(player, reason);
                return false;
            }
            Print(player, Lang("VehicleNotOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.spawnCommand));
            return false;
        }

        private bool CanRecall(BasePlayer player, Vehicle vehicle, string vehicleType, bool checkWater, out string reason)
        {
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            if (player.IsBuildingBlocked())
            {
                reason = Lang("BuildingBlocked", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (player.GetMountedVehicle() != null || player.HasParent())
            {
                reason = Lang("MountedOrParented", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.preventSafeZone && player.InSafeZone())
            {
                reason = Lang("PlayerInSafeZone", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (configData.globalS.anyMountedRecall && VehicleAnyMounted(vehicle.entity))
            {
                reason = Lang("PlayerMountedOnVehicle", player.UserIDString, baseVehicleS.displayName);
                return false;
            }
            if (baseVehicleS.recallMaxDistance > 0 && Vector3.Distance(player.transform.position, vehicle.entity.transform.position) > baseVehicleS.recallMaxDistance)
            {
                reason = Lang("RecallTooFar", player.UserIDString, baseVehicleS.recallMaxDistance, baseVehicleS.displayName);
                return false;
            }
            if (!CheckPosition(player, baseVehicleS, checkWater, out reason))
            {
                return false;
            }
#if DEBUG
            if (player.IsAdmin)
            {
                reason = null;
                return true;
            }
#endif
            var recallCooldown = GetRecallCooldown(player, baseVehicleS);
            if (recallCooldown > 0)
            {
                var timeLeft = Math.Ceiling(recallCooldown - (TimeEx.currentTimestamp - vehicle.lastRecall));
                if (timeLeft > 0)
                {
                    reason = Lang("VehicleOnRecallCooldown", player.UserIDString, timeLeft, baseVehicleS.displayName);
                    return false;
                }
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

        private void RecallVehicle(BasePlayer player, Vehicle vehicle, string vehicleType, bool checkWater)
        {
            var entity = vehicle.entity;
            if (configData.globalS.dismountAllPlayersRecall)
            {
                DismountAllPlayers(entity);
            }
            var baseVehicleS = GetBaseVehicleS(vehicleType);
            if (CanDropInventory(baseVehicleS))
            {
                DropVehicleInventoryItems(player, vehicle.vehicleType, entity, baseVehicleS);
            }
            if (entity.HasParent()) entity.SetParent(null, true, true);

            vehicle.OnRecall();
            Vector3 position; Quaternion rotation;
            GetVehicleSpawnPos(player, baseVehicleS, checkWater, vehicleType, out position, out rotation);
            entity.transform.position = position;
            entity.transform.rotation = rotation;
            entity.transform.hasChanged = true;
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
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("Help", player.UserIDString));
                foreach (var entry in allBaseVehicleSettings)
                {
                    if (CanViewVehicleInfo(player, entry.Key, entry.Value))
                    {
                        stringBuilder.AppendLine(Lang("HelpKill", player.UserIDString, configData.chatS.killCommand, entry.Value.commands[0], entry.Value.displayName));
                    }
                }
                Print(player, stringBuilder.ToString());
                return;
            }
            if (PlayerIsBlocked(player)) return;
            HandleKillCmd(player, args[0].ToLower());
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
            Dictionary<string, Vehicle> vehicles;
            if (!storedData.playerData.TryGetValue(player.userID, out vehicles) || !vehicles.TryGetValue(vehicleType, out vehicle))
            {
                Print(player, Lang("VehicleNotYetPurchased", player.UserIDString, baseVehicleS.displayName, configData.chatS.buyCommand));
                return false;
            }
            if (vehicle.entity != null && !vehicle.entity.IsDestroyed)
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
                vehicle.entity.Kill(BaseNetworkable.DestroyMode.Gib);
                Print(player, Lang("VehicleKilled", player.UserIDString, baseVehicleS.displayName));
                return true;
            }
            Print(player, Lang("VehicleNotOut", player.UserIDString, baseVehicleS.displayName, configData.chatS.spawnCommand));
            return false;
        }

        #endregion Kill Command

        #region Command Helper

        private bool CheckPosition(BasePlayer player, BaseVehicleS baseVehicleS, bool checkWater, out string reason)
        {
            if (checkWater || configData.globalS.spawnLookingAt)
            {
                var lookingAt = GetLookingAtGroundPos(player, baseVehicleS.distance);
                if (checkWater && !PositionIsInWater(lookingAt))
                {
                    reason = Lang("NotLookingAtWater", player.UserIDString, baseVehicleS.displayName);
                    return false;
                }
                if (configData.globalS.spawnLookingAt && baseVehicleS.minDistanceForPlayers > 0)
                {
                    var nearbyPlayers = Facepunch.Pool.GetList<BasePlayer>();
                    Vis.Entities(lookingAt, baseVehicleS.minDistanceForPlayers, nearbyPlayers, Rust.Layers.Mask.Player_Server);
                    bool flag = nearbyPlayers.Any(x => x.userID.IsSteamId() && x != player);
                    Facepunch.Pool.FreeList(ref nearbyPlayers);
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

        private bool IsValidVehicleType(string option, out string vehicleType)
        {
            foreach (var entry in allBaseVehicleSettings)
            {
                if (entry.Key.ToLower() == option)
                {
                    vehicleType = entry.Key;
                    return true;
                }
            }

            vehicleType = null;
            return false;
        }

        private bool IsValidOption(BasePlayer player, string option, out string vehicleType)
        {
            foreach (var entry in allBaseVehicleSettings)
            {
                if (entry.Value.commands.Any(x => x.ToLower() == option))
                {
                    if (!HasVehiclePermission(player, entry.Key, entry.Value))
                    {
                        Print(player, Lang("NotAllowed", player.UserIDString));
                        vehicleType = null;
                        return false;
                    }
                    vehicleType = entry.Key;
                    return true;
                }
            }
            Print(player, Lang("OptionNotFound", player.UserIDString, option));
            vehicleType = null;
            return false;
        }

        private void GetVehicleSpawnPos(BasePlayer player, BaseVehicleS baseVehicleS, bool checkWater, string vehicleType, out Vector3 spawnPos, out Quaternion spawnRot)
        {
            if (configData.globalS.spawnLookingAt)
            {
                bool needGetGround = true;
                spawnPos = GetLookingAtGroundPos(player, baseVehicleS.distance);
                if (checkWater)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(spawnPos, Vector3.up, out hit, 200, LAYER_GROUND) &&
                        hit.GetEntity() is StabilityEntity)
                    {
                        needGetGround = false;
                    }
                }
                else
                {
                    var buildingBlocks = Facepunch.Pool.GetList<BuildingBlock>();
                    Vis.Entities(spawnPos, 2f, buildingBlocks, Rust.Layers.Mask.Construction);
                    if (buildingBlocks.Count > 0)
                    {
                        var pos = spawnPos;
                        var closestBuildingBlock = buildingBlocks
                            .Where(x => !x.ShortPrefabName.Contains("wall"))
                            .OrderBy(x => Vector3.Distance(x.transform.position, pos)).FirstOrDefault();
                        if (closestBuildingBlock != null)
                        {
                            spawnPos = closestBuildingBlock.CenterPoint();
                            spawnPos.y = closestBuildingBlock.WorldSpaceBounds().ToBounds().max.y;
                            needGetGround = false;
                        }
                    }
                    Facepunch.Pool.FreeList(ref buildingBlocks);
                }
                if (needGetGround)
                {
                    spawnPos = GetGroundPosition(spawnPos);
                }
            }
            else
            {
                var minDistanceForPlayers = baseVehicleS.minDistanceForPlayers > 2 ? baseVehicleS.minDistanceForPlayers : 3;
                var distance = Mathf.Max(baseVehicleS.distance, minDistanceForPlayers);
                spawnPos = player.transform.position;
                var nearbyPlayers = Facepunch.Pool.GetList<BasePlayer>();
                var originPos = checkWater ? GetLookingAtGroundPos(player, distance) : player.transform.position;
                for (int i = 0; i < 100; i++)
                {
                    spawnPos.x = originPos.x + UnityEngine.Random.Range(minDistanceForPlayers, distance) * (UnityEngine.Random.Range(0, 2) > 0 ? 1 : -1);
                    spawnPos.z = originPos.z + UnityEngine.Random.Range(minDistanceForPlayers, distance) * (UnityEngine.Random.Range(0, 2) > 0 ? 1 : -1);
                    spawnPos = GetGroundPosition(spawnPos);
                    nearbyPlayers.Clear();
                    Vis.Entities(spawnPos, minDistanceForPlayers, nearbyPlayers, Rust.Layers.Mask.Player_Server);
                    if (!nearbyPlayers.Any(x => x.userID.IsSteamId()))
                    {
                        break;
                    }
                }
                Facepunch.Pool.FreeList(ref nearbyPlayers);
            }

            var normalized = (spawnPos - player.transform.position).normalized;
            var angle = normalized != Vector3.zero ? Quaternion.LookRotation(normalized).eulerAngles.y : UnityEngine.Random.Range(0f, 360f);
            spawnRot = Quaternion.Euler(Vector3.up * (angle + 90f));
            if (vehicleType != nameof(NormalVehicleType.RidableHorse)) spawnPos += Vector3.up * 0.3f;
        }

        #endregion Command Helper

        #endregion Commands

        #region ConfigurationFile

        public ConfigData configData { get; private set; }

        public class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public Settings globalS = new Settings();

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
            public VersionNumber version = new VersionNumber(1, 7, 0);
        }

        public class ChatSettings
        {
            [JsonProperty(PropertyName = "Use Universal Chat Command")] public bool useUniversalCommand = true;
            [JsonProperty(PropertyName = "Help Chat Command")] public string helpCommand = "license";
            [JsonProperty(PropertyName = "Buy Chat Command")] public string buyCommand = "buy";
            [JsonProperty(PropertyName = "Spawn Chat Command")] public string spawnCommand = "spawn";
            [JsonProperty(PropertyName = "Recall Chat Command")] public string recallCommand = "recall";
            [JsonProperty(PropertyName = "Kill Chat Command")] public string killCommand = "kill";
            [JsonProperty(PropertyName = "Chat Prefix")] public string prefix = "<color=#00FFFF>[VehicleLicense]</color>: ";
            [JsonProperty(PropertyName = "Chat SteamID Icon")] public ulong steamIDIcon = 76561198924840872;
        }

        public class Settings
        {
            [JsonProperty(PropertyName = "Store Vehicle On Plugin Unloaded / Server Restart")] public bool storeVehicle = true;
            [JsonProperty(PropertyName = "Clear Vehicle Data On Map Wipe")] public bool clearVehicleOnWipe;

            [JsonProperty(PropertyName = "Interval to check vehicle for wipe (Seconds)")] public float checkVehiclesInterval = 300;
            [JsonProperty(PropertyName = "Spawn vehicle in the direction you are looking at")] public bool spawnLookingAt = true;

            [JsonProperty(PropertyName = "Check if any player mounted when recalling a vehicle")] public bool anyMountedRecall = true;
            [JsonProperty(PropertyName = "Check if any player mounted when killing a vehicle")] public bool anyMountedKill = true;
            [JsonProperty(PropertyName = "Dismount all players when a vehicle is recalled")] public bool dismountAllPlayersRecall = true;
            [JsonProperty(PropertyName = "Prevent vehicles from spawning or recalling in safe zone")] public bool preventSafeZone = true;

            [JsonProperty(PropertyName = "Prevent other players from mounting vehicle")] public bool preventMounting = true;
            [JsonProperty(PropertyName = "Prevent mounting on driver's seat only")] public bool preventDriverSeat = true;
            [JsonProperty(PropertyName = "Prevent vehicles from damaging players")] public bool preventDamagePlayer = true;
            [JsonProperty(PropertyName = "Use Teams")] public bool useTeams;
            [JsonProperty(PropertyName = "Use Clans")] public bool useClans = true;
            [JsonProperty(PropertyName = "Use Friends")] public bool useFriends = true;

            [JsonProperty(PropertyName = "Vehicle No Decay")] public bool noDecay;
            [JsonProperty(PropertyName = "Vehicle No Fire Ball")] public bool noFireBall = true;
            [JsonProperty(PropertyName = "Vehicle No Server Gibs")] public bool noServerGibs = true;
            [JsonProperty(PropertyName = "Chinook No Map Marker")] public bool noMapMarker = true;

            [JsonProperty(PropertyName = "Use Raid Blocker (Need NoEscape Plugin)")] public bool useRaidBlocker;
            [JsonProperty(PropertyName = "Use Combat Blocker (Need NoEscape Plugin)")] public bool useCombatBlocker;
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
                spawnPrices = new Dictionary<string, PriceInfo>(),
                recallPrices = new Dictionary<string, PriceInfo>(),
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
            [JsonProperty(PropertyName = "Purchasable")] public bool purchasable;
            [JsonProperty(PropertyName = "Display Name")] public string displayName { get; set; }
            [JsonProperty(PropertyName = "Use Permission")] public bool usePermission { get; set; }
            [JsonProperty(PropertyName = "Permission")] public string permission { get; set; }
            [JsonProperty(PropertyName = "Distance To Spawn")] public float distance { get; set; }
            [JsonProperty(PropertyName = "Time Before Vehicle Wipe (Seconds)")] public double wipeTime { get; set; }

            [JsonProperty(PropertyName = "Maximum Health")] public float maxHealth { get; set; }
            [JsonProperty(PropertyName = "Can Recall Maximum Distance")] public float recallMaxDistance { get; set; }
            [JsonProperty(PropertyName = "Can Kill Maximum Distance")] public float killMaxDistance { get; set; }
            [JsonProperty(PropertyName = "Minimum distance from player to recall or spawn")] public float minDistanceForPlayers { get; set; }

            [JsonProperty(PropertyName = "Remove License Once Crashed")] public bool removeLicenseOnceCrash { get; set; }

            [JsonProperty(PropertyName = "Purchase Prices")] public Dictionary<string, PriceInfo> purchasePrices { get; set; }
            [JsonProperty(PropertyName = "Spawn Prices")] public Dictionary<string, PriceInfo> spawnPrices { get; set; }
            [JsonProperty(PropertyName = "Recall Prices")] public Dictionary<string, PriceInfo> recallPrices { get; set; }

            [JsonProperty(PropertyName = "Commands")] public List<string> commands { get; set; }
            [JsonProperty(PropertyName = "Spawn Cooldown (Seconds)")] public double spawnCooldown { get; set; }
            [JsonProperty(PropertyName = "Recall Cooldown (Seconds)")] public double recallCooldown { get; set; }
            [JsonProperty(PropertyName = "Cooldown Permissions")] public Dictionary<string, PermissionS> cooldownPermissions { get; set; }
        }

        public interface IFuelVehicle
        {
            [JsonProperty(PropertyName = "Refund Fuel On Kill", Order = 20)] bool refundFuelOnKill { get; }
            [JsonProperty(PropertyName = "Refund Fuel On Crash", Order = 21)] bool refundFuelOnCrash { get; }
        }

        public interface IInventoryVehicle
        {
            [JsonProperty(PropertyName = "Refund Inventory On Kill", Order = 22)] bool refundInventoryOnKill { get; }
            [JsonProperty(PropertyName = "Refund Inventory On Crash", Order = 23)] bool refundInventoryOnCrash { get; }
            [JsonProperty(PropertyName = "Drop Inventory Items When Vehicle Recall", Order = 29)] bool dropInventoryOnRecall { get; }
        }

        public interface IModularVehicle
        {
            [JsonProperty(PropertyName = "Refund Engine Items On Kill", Order = 24)] bool refundEngineOnKill { get; }
            [JsonProperty(PropertyName = "Refund Engine Items On Crash", Order = 25)] bool refundEngineOnCrash { get; }
            [JsonProperty(PropertyName = "Refund Module Items On Kill", Order = 26)] bool refundModuleOnKill { get; }
            [JsonProperty(PropertyName = "Refund Module Items On Crash", Order = 27)] bool refundModuleOnCrash { get; }
        }

        public class FuelVehicleS : BaseVehicleS, IFuelVehicle
        {
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
            [JsonProperty(PropertyName = "Chassis Type (Small, Medium, Large)", Order = 30)] public ChassisType chassisType { get; set; }

            [JsonProperty(PropertyName = "Vehicle Module Items", Order = 31)] public List<ModuleItem> moduleItems { get; set; }
            [JsonProperty(PropertyName = "Vehicle Engine Items", Order = 32)] public List<EngineItem> engineItems { get; set; }

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

        protected override void SaveConfig() => Config.WriteObject(configData, true);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= new VersionNumber(1, 7, 0))
                {
                    if (configData.chatS.prefix == "[VehicleLicense]: ")
                    {
                        configData.chatS.prefix = "<color=#00FFFF>[VehicleLicense]</color>: ";
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

        #endregion ConfigurationFile

        #region DataFile

        public StoredData storedData { get; private set; }

        public class StoredData
        {
            public readonly Dictionary<ulong, Dictionary<string, Vehicle>> playerData = new Dictionary<ulong, Dictionary<string, Vehicle>>();
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
                foreach (var playerEntry in storedData.playerData)
                {
                    foreach (var entry in playerEntry.Value)
                    {
                        entry.Value.entityID = 0;
                        entry.Value.lastDeath = 0;
                    }
                }
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

                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- To buy a <color=#009EFF>{2}</color>",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To buy a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- To spawn a <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To spawn a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- To recall a <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- To recall a <color=#009EFF>{2}</color>. Price: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- To kill a <color=#009EFF>{2}</color>",

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
                ["PlayerMountedOnVehicle"] = "It cannot be recalled when players mounted on your <color=#009EFF>{0}</color>.",
                ["PlayerInSafeZone"] = "You cannot spawn or recall your <color=#009EFF>{0}</color> in the safe zone.",
                ["VehicleInventoryDropped"] = "Your <color=#009EFF>{0}</color> vehicle inventory cannot be recalled, it have dropped to the ground.",
                ["NoResourcesToPurchaseVehicle"] = "You don't have enough resources to buy a <color=#009EFF>{0}</color>. You are missing: {1}",
                ["NoResourcesToSpawnVehicle"] = "You don't have enough resources to spawn a <color=#009EFF>{0}</color>. You are missing: {1}",
                ["NoResourcesToRecallVehicle"] = "You don't have enough resources to recall a <color=#009EFF>{0}</color>. You are missing: {1}",
                ["MountedOrParented"] = "You cannot spawn or recall a <color=#009EFF>{0}</color> when mounted or parented.",
                ["RecallTooFar"] = "You must be within <color=#FF1919>{0}</color> meters of <color=#009EFF>{1}</color> to recall.",
                ["KillTooFar"] = "You must be within <color=#FF1919>{0}</color> meters of <color=#009EFF>{1}</color> to kill.",
                ["PlayersOnNearby"] = "You cannot spawn or recall a <color=#009EFF>{0}</color> when there are players near the position you are looking at.",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help"] = "可用命令列表:",
                ["HelpLicence1"] = "<color=#4DFF4D>/{0}</color> -- 购买一辆载具",
                ["HelpLicence2"] = "<color=#4DFF4D>/{0}</color> -- 生成一辆载具",
                ["HelpLicence3"] = "<color=#4DFF4D>/{0}</color> -- 召回一辆载具",
                ["HelpLicence4"] = "<color=#4DFF4D>/{0}</color> -- 摧毁一辆载具",
                ["HelpLicence5"] = "<color=#4DFF4D>/{0}</color> -- 购买，生成，召回一辆 <color=#009EFF>{1}</color>",

                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- 购买一辆 <color=#009EFF>{2}</color>",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 购买一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- 生成一辆 <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 生成一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- 召回一辆 <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- 召回一辆 <color=#009EFF>{2}</color>，价格: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- 摧毁一辆 <color=#009EFF>{2}</color>",

                ["NotAllowed"] = "您没有权限使用该命令",
                ["RaidBlocked"] = "<color=#FF1919>您被突袭阻止了，不能使用该命令</color>",
                ["CombatBlocked"] = "<color=#FF1919>您被战斗阻止了，不能使用该命令</color>",
                ["OptionNotFound"] = "该 <color=#009EFF>{0}</color> 选项不存在",
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
                ["PlayerMountedOnVehicle"] = "您的 <color=#009EFF>{0}</color> 上坐着玩家，无法被召回",
                ["PlayerInSafeZone"] = "您不能在安全区域内生成或召回您的 <color=#009EFF>{0}</color>",
                ["VehicleInventoryDropped"] = "您的 <color=#009EFF>{0}</color> 载具物品不能召回，它已经掉落在地上了",
                ["NoResourcesToPurchaseVehicle"] = "您没有足够的资源购买 <color=#009EFF>{0}</color>，还需要: {1}",
                ["NoResourcesToSpawnVehicle"] = "您没有足够的资源生成 <color=#009EFF>{0}</color>，还需要: {1}",
                ["NoResourcesToRecallVehicle"] = "您没有足够的资源召回 <color=#009EFF>{0}</color>，还需要: {1}",
                ["MountedOrParented"] = "当您坐着或者在附着在实体上时无法生成或召回 <color=#009EFF>{0}</color>",
                ["RecallTooFar"] = "您必须在 <color=#FF1919>{0}</color> 米内才能召回您的 <color=#009EFF>{1}</color>",
                ["KillTooFar"] = "您必须在 <color=#FF1919>{0}</color> 米内才能摧毁您的 <color=#009EFF>{1}</color>",
                ["PlayersOnNearby"] = "您正在看着的位置附近有玩家时无法生成或召回 <color=#009EFF>{0}</color>",
            }, this, "zh-CN");
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Help"] = "Список доступных команд:",
                ["HelpLicence1"] = "<color=#4DFF4D>/{0}</color> -- Купить транспорт",
                ["HelpLicence2"] = "<color=#4DFF4D>/{0}</color> -- Создать транспорт",
                ["HelpLicence3"] = "<color=#4DFF4D>/{0}</color> -- Вызвать транспорт",
                ["HelpLicence4"] = "<color=#4DFF4D>/{0}</color> -- Уничтожить транспорт",
                ["HelpLicence5"] = "<color=#4DFF4D>/{0}</color> -- Купить, создать, или вызвать <color=#009EFF>{1}</color>",

                ["HelpBuy"] = "<color=#4DFF4D>/{0} {1}</color> -- Купить <color=#009EFF>{2}</color>.",
                ["HelpBuyPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Купить <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpSpawn"] = "<color=#4DFF4D>/{0} {1}</color> -- Создать <color=#009EFF>{2}</color>",
                ["HelpSpawnPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызывать <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpRecall"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызвать <color=#009EFF>{2}</color>",
                ["HelpRecallPrice"] = "<color=#4DFF4D>/{0} {1}</color> -- Вызвать <color=#009EFF>{2}</color>. Цена: {3}",
                ["HelpKill"] = "<color=#4DFF4D>/{0} {1}</color> -- Уничтожить <color=#009EFF>{2}</color>",

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
                ["NoResourcesToPurchaseVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: {1}",
                ["NoResourcesToSpawnVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: {1}",
                ["NoResourcesToRecallVehicle"] = "У вас недостаточно ресурсов для покупки <color=#009EFF>{0}</color>. Вам не хватает: {1}",
                ["MountedOrParented"] = "Вы не можете создать <color=#009EFF>{0}</color> когда сидите или привязаны к объекту.",
                ["RecallTooFar"] = "Вы должны быть в пределах <color=#FF1919>{0}</color> метров от <color=#009EFF>{1}</color>, чтобы вызывать.",
                ["KillTooFar"] = "Вы должны быть в пределах <color=#FF1919>{0}</color> метров от <color=#009EFF>{1}</color>, уничтожить.",
                ["PlayersOnNearby"] = "Вы не можете создать <color=#009EFF>{0}</color> когда рядом с той позицией, на которую вы смотрите, есть игроки.",
            }, this, "ru");
        }

        #endregion LanguageFile
    }
}