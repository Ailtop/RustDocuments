using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("Everlight", "Wulf/lukespragg/Arainrr", "3.4.13")]
    [Description("Allows infinite light from configured objects by not consuming fuel")]
    public class Everlight : RustPlugin
    {
        private readonly Dictionary<ItemModBurnable, ItemDefinition> itemModBurnables = new Dictionary<ItemModBurnable, ItemDefinition>();

        private readonly Dictionary<string, EverlightEntry> everlightItems = new Dictionary<string, EverlightEntry>
        {
            //OnFindBurnable
            ["Barbeque"] = new EverlightEntry("bbq.deployed", new EverlightS(false, false, "everlight.bbq")),
            ["Camp Fire"] = new EverlightEntry("campfire", new EverlightS(false, false, "everlight.campfire")),
            ["Cursed Cauldron"] = new EverlightEntry("cursedcauldron.deployed", new EverlightS(false, false, "everlight.cursedcauldron")),
            ["Chinese Lantern"] = new EverlightEntry("chineselantern.deployed", new EverlightS(false, false, "everlight.chineselantern")),
            ["Stone Fireplace"] = new EverlightEntry("fireplace.deployed", new EverlightS(false, false, "everlight.fireplace")),
            ["Furnace"] = new EverlightEntry("furnace", new EverlightS(false, false, "everlight.furnace")),
            ["Large Furnace"] = new EverlightEntry("furnace.large", new EverlightS(false, false, "everlight.furnace.large")),
            ["Jack O Lantern Angry"] = new EverlightEntry("jackolantern.angry", new EverlightS(false, false, "everlight.jackolantern.angry")),
            ["Jack O Lantern Happy"] = new EverlightEntry("jackolantern.happy", new EverlightS(false, false, "everlight.jackolantern.happy")),
            ["Lantern"] = new EverlightEntry("lantern.deployed", new EverlightS(false, false, "everlight.lantern")),
            ["Skull Fire Pit"] = new EverlightEntry("skull_fire_pit", new EverlightS(false, false, "everlight.skull_fire_pit")),
            ["Small Oil Refinery"] = new EverlightEntry("refinery_small_deployed", new EverlightS(false, false, "everlight.refinery_small")),
            ["Tuna Can Lamp"] = new EverlightEntry("tunalight.deployed", new EverlightS(false, false, "everlight.tunalight")),
            ["Hobo Barrel"] = new EverlightEntry("hobobarrel.deployed", new EverlightS(false, false, "everlight.hobobarrel")),
            //OnItemUse,
            ["Miners Hat"] = new EverlightEntry("hat.miner", new EverlightS(false, false, "everlight.hat.miner")),
            ["Candle Hat"] = new EverlightEntry("hat.candle", new EverlightS(false, false, "everlight.hat.candle")),
            //OnEntitySpawned
            ["Search Light"] = new EverlightEntry("searchlight.deployed", new EverlightS(false, false, "everlight.searchlight")),
            ["Small Candle Set"] = new EverlightEntry("smallcandleset", new EverlightS(false, false, "everlight.smallcandleset")),
            ["Large Candle Set"] = new EverlightEntry("largecandleset", new EverlightS(false, false, "everlight.largecandleset")),
            ["Ceiling Light"] = new EverlightEntry("ceilinglight.deployed", new EverlightS(false, false, "everlight.ceilinglight")),
            ["Siren Light"] = new EverlightEntry("electric.sirenlight.deployed", new EverlightS(false, false, "everlight.sirenlight")),
            ["Flasher Light"] = new EverlightEntry("electric.flasherlight.deployed", new EverlightS(false, false, "everlight.flasherlight")),
            ["Simple Light"] = new EverlightEntry("simplelight", new EverlightS(false, false, "everlight.simplelight")),
            ["Strobe Light"] = new EverlightEntry("strobelight", new EverlightS(false, false, "everlight.strobelight")),
            ["Deluxe Christmas Lights"] = new EverlightEntry("xmas.advanced.lights.deployed", new EverlightS(false, false, "everlight.advanced.lights")),
            ["Igniter"] = new EverlightEntry("igniter.deployed", new EverlightS(false, false, "everlight.igniter")),
            ["Firework"] = new EverlightEntry("firework", new EverlightS(false, false, "everlight.firework")),
            ["Neon Sign"] = new EverlightEntry("neonSign", new EverlightS(false, false, "everlight.neonSign")),
            ["Night Vision Goggles"] = new EverlightEntry("nightvisiongoggles", new EverlightS(false, false, "everlight.nightvisiongoggles")),
        };

        private readonly Dictionary<string, List<string>> hookItems = new Dictionary<string, List<string>>
        {
            [nameof(OnItemUse)] = new List<string> { "Miners Hat", "Candle Hat" },
            [nameof(OnLoseCondition)] = new List<string> { "Night Vision Goggles" },
            [nameof(OnEntitySpawned)] = new List<string> { "Search Light", "Small Candle Set", "Large Candle Set", "Ceiling Light", "Siren Light", "Flasher Light", "Simple Light", "Strobe Light", "Deluxe Christmas Lights", "Igniter", "Firework", "Neon Sign" },
            [nameof(OnFindBurnable)] = new List<string> { "Barbeque", "Camp Fire", "Cursed Cauldron", "Chinese Lantern", "Stone Fireplace", "Furnace", "Large Furnace", "Jack O Lantern Angry", "Jack O Lantern Happy", "Lantern", "Skull Fire Pit", "Small Oil Refinery", "Tuna Can Lamp", "Hobo Barrel" },
            [nameof(OnEntityDistanceCheck)] = new List<string> { "Barbeque", "Camp Fire", "Cursed Cauldron", "Chinese Lantern", "Stone Fireplace", "Furnace", "Large Furnace", "Jack O Lantern Angry", "Jack O Lantern Happy", "Lantern", "Skull Fire Pit", "Small Oil Refinery", "Tuna Can Lamp", "Hobo Barrel" },
        };

        private void Init()
        {
            UpdateConfig();
            Unsubscribe(nameof(OnItemUse));
            Unsubscribe(nameof(OnLoseCondition));
            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnFindBurnable));
            Unsubscribe(nameof(OnEntityDistanceCheck));
            foreach (var entry in configData.everlightList)
            {
                if (!permission.PermissionExists(entry.Value.permission))
                    permission.RegisterPermission(entry.Value.permission, this);
            }
        }

        private void UpdateConfig()
        {
            foreach (var entry in everlightItems)
            {
                EverlightS everlightS;
                if (configData.everlightList.TryGetValue(entry.Key, out everlightS))
                    entry.Value.everlightS = everlightS;
                else configData.everlightList.Add(entry.Key, entry.Value.everlightS);
            }
            SaveConfig();
        }

        private void OnServerInitialized()
        {
            foreach (var entry in hookItems)
            {
                if (entry.Value.Any(x => everlightItems[x].everlightS.enabled))
                {
                    Subscribe(entry.Key);
                    if (entry.Key == nameof(OnEntitySpawned))
                    {
                        foreach (var baseNetworkable in BaseNetworkable.serverEntities)
                        {
                            OnEntitySpawned(baseNetworkable as BaseCombatEntity);
                        }
                    }
                }
            }
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                var itemModBurnable = itemDefinition.GetComponent<ItemModBurnable>();
                if (itemModBurnable != null)
                {
                    itemModBurnables.Add(itemModBurnable, itemDefinition);
                }
            }
        }

        private void OnEntitySpawned(BaseCombatEntity baseCombatEntity)
        {
            if (baseCombatEntity == null) return;
            if (baseCombatEntity is Candle || baseCombatEntity is IOEntity || baseCombatEntity is StrobeLight || baseCombatEntity is BaseFirework)
            {
                if (!CanEverlight(baseCombatEntity is NeonSign ? "neonSign"
                    : baseCombatEntity is BaseFirework ? "firework"
                    : baseCombatEntity.ShortPrefabName, baseCombatEntity.OwnerID.ToString())) return;
                var candle = baseCombatEntity as Candle;
                if (candle != null)
                {
                    candle.burnRate = 0f;
                    return;
                }

                var light = baseCombatEntity as StrobeLight;
                if (light != null)
                {
                    light.burnRate = 0f;
                    return;
                }

                var baseFirework = baseCombatEntity as BaseFirework;
                if (baseFirework != null)
                {
                    baseFirework.limitActiveCount = false;
                    baseFirework.StaggeredTryLightFuse();
                    baseFirework.Invoke(() => baseFirework.CancelInvoke(baseFirework.OnExhausted), baseFirework.fuseLength + 1);
                    var repeatingFirework = baseFirework as RepeatingFirework;
                    if (repeatingFirework != null)
                    {
                        repeatingFirework.maxRepeats = int.MaxValue;
                    }
                    return;
                }

                var iOEntity = (IOEntity)baseCombatEntity;
                iOEntity.UpdateHasPower(1000, 100);
                iOEntity.IOStateChanged(1000, 100);
                iOEntity.SendNetworkUpdate();

                var igniter = iOEntity as Igniter;
                if (igniter != null)
                {
                    igniter.SelfDamagePerIgnite = 0;
                }
            }
        }

        private void OnLoseCondition(Item item, ref float amount)
        {
            if (item?.info.shortname != "nightvisiongoggles") return;
            var player = item.GetOwnerPlayer();
            if (player == null) return;
            if (!CanEverlight(item.info.shortname, player.UserIDString)) return;
            amount = 0;
        }

        private object OnFindBurnable(BaseOven baseOven)
        {
            if (baseOven == null) return null;
            if (!CanEverlight(baseOven.ShortPrefabName, baseOven.OwnerID.ToString())) return null;
            var itemID = itemModBurnables.FirstOrDefault(x => baseOven.fuelType == null || x.Value == baseOven.fuelType).Value?.itemid ?? 0;
            return itemID == 0 ? null : ItemManager.CreateByItemID(itemID);
        }

        private void OnEntityDistanceCheck(BaseOven baseOven, BasePlayer player, uint id, string debugName, float maximumDistance)
        {
            if (id == 4167839872u && debugName == "SVSwitch")
            {
                if (baseOven.Distance(player.eyes.position) > maximumDistance)
                {
                    return;
                }
                if (baseOven.IsOn()) return;
                var hasFuel = baseOven.inventory?.itemList?.FirstOrDefault(x => x.info.GetComponent<ItemModBurnable>() != null && (baseOven.fuelType == null || x.info == baseOven.fuelType)) != null;
                if (hasFuel) return;
                if (Interface.CallHook("OnOvenToggle", baseOven, player) == null && (!baseOven.needsBuildingPrivilegeToUse || player.CanBuild()))
                {
                    if (CanEverlight(baseOven.ShortPrefabName, baseOven.OwnerID.ToString()))
                    {
                        baseOven.StartCooking();
                    }
                }
            }
        }

        private object OnItemUse(Item item, int amount)
        {
            if (item.info.shortname != "lowgradefuel") return null;
            var shortName = item.parent?.parent?.info?.shortname;
            if (shortName != "hat.candle" && shortName != "hat.miner") return null;
            var playerID = item.GetRootContainer()?.GetOwnerPlayer()?.UserIDString;
            if (!CanEverlight(shortName, playerID)) return null;
            return 0;
        }

        private bool CanEverlight(string shortName, string playerID)
        {
            if (string.IsNullOrEmpty(shortName)) return false;
            foreach (var entry in everlightItems)
            {
                if (entry.Value.name == shortName)
                {
                    if (!entry.Value.everlightS.enabled) return false;
                    if (entry.Value.everlightS.usePermission && (string.IsNullOrEmpty(playerID) || !permission.UserHasPermission(playerID, entry.Value.everlightS.permission)))
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class EverlightEntry
        {
            public string name;
            public EverlightS everlightS;

            public EverlightEntry(string name, EverlightS everlightS)
            {
                this.name = name;
                this.everlightS = everlightS;
            }
        }

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Everlight entity list")]
            public Dictionary<string, EverlightS> everlightList = new Dictionary<string, EverlightS>();
        }

        private class EverlightS
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Use permission")]
            public bool usePermission = false;

            [JsonProperty(PropertyName = "Permission")]
            public string permission = string.Empty;

            public EverlightS(bool enabled, bool usePermission, string permission)
            {
                this.enabled = enabled;
                this.usePermission = usePermission;
                this.permission = permission;
            }
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