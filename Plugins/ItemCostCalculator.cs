using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Item Cost Calculator", "Absolut/Arainrr", "2.0.10", ResourceId = 2109)]
    internal class ItemCostCalculator : RustPlugin
    {
        [PluginReference] private Plugin ImageLibrary;

        private void OnServerInitialized()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (!configData.displayNames.ContainsKey(itemDefinition.shortname))
                    configData.displayNames.Add(itemDefinition.shortname, itemDefinition.displayName.english);
                var itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
                if (itemBlueprint != null)
                {
                    foreach (var itemAmount in itemBlueprint.ingredients)
                        if (!configData.materials.ContainsKey(itemAmount.itemDef.shortname))
                            configData.materials.Add(itemAmount.itemDef.shortname, 1);
                }
                else if (!configData.noMaterials.ContainsKey(itemDefinition.shortname))
                    configData.noMaterials.Add(itemDefinition.shortname, 1);
            }
            foreach (var material in configData.materials)
            {
                if (configData.noMaterials.ContainsKey(material.Key))
                    configData.noMaterials.Remove(material.Key);
            }

            SaveConfig();
        }

        private string GetImageUrl(ItemDefinition itemDefinition, ulong skin = 0)
        {
            if (ImageLibrary == null) return "https://rustlabs.com/img/items180/" + itemDefinition.shortname + ".png";
            return (string)ImageLibrary.Call("GetImageURL", itemDefinition.shortname, skin);
        }

        [ConsoleCommand("costfile")]
        private void CmdCostFile(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length != 1 || !arg.IsAdmin)
            {
                SendReply(arg, "Syntax error, please type 'costfile <shop / reward>'");
                return;
            }
            switch (arg.Args[0].ToLower())
            {
                case "shop":
                    GetItemCost();
                    return;

                case "reward":
                    GetItemCost(false);
                    return;
            }
            SendReply(arg, "Syntax error, please type 'costfile <shop / reward>'");
        }

        private void GetItemCost(bool isGUIShop = true)
        {
            var costs = new Dictionary<ItemDefinition, double>();
            double amount;
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (configData.materials.TryGetValue(itemDefinition.shortname, out amount))
                {
                    costs.Add(itemDefinition, amount);
                    continue;
                }
                if (configData.noMaterials.TryGetValue(itemDefinition.shortname, out amount))
                {
                    costs.Add(itemDefinition, amount);
                    continue;
                }
                var itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
                if (itemBlueprint == null) continue;
                double cost = 0;
                foreach (var itemAmount in itemBlueprint.ingredients)
                {
                    if (configData.materials.TryGetValue(itemAmount.itemDef.shortname, out amount))
                        cost += itemAmount.amount * amount;
                    else
                    {
                        if (costs.TryGetValue(itemAmount.itemDef, out amount))
                            cost += itemAmount.amount * amount;
                    }
                }
                if (cost != 0)
                {
                    cost = cost / itemBlueprint.amountToCreate;
                    if (configData.gatherRateOffset > 0)
                        cost *= configData.gatherRateOffset;
                    int rarity;
                    if (configData.rarityList.TryGetValue(itemDefinition.shortname, out rarity))
                        cost += cost * (rarity / 100d);
                    float level;
                    if (configData.workbenchMultiplier.TryGetValue(itemBlueprint.workbenchLevelRequired, out level))
                        cost += cost * (level / 100d);
                    costs.Add(itemDefinition, cost);
                }
            }
            CrateDataFile(costs, isGUIShop);
        }

        private void CrateDataFile(Dictionary<ItemDefinition, double> costs, bool isGUIShop = true)
        {
            if (isGUIShop)
            {
                ShopData GUIShopData = new ShopData();
                var itemDisplayNames = new Dictionary<string, string>();
                foreach (var entry in costs)
                {
                    var imageUrl = GetImageUrl(entry.Key);
                    string displayName;
                    if (!configData.displayNames.TryGetValue(entry.Key.shortname, out displayName))
                    {
                        displayName = entry.Key.displayName.english;
                    }

                    if (GUIShopData.items.ContainsKey(displayName))
                    {
                        displayName += $"_Repeat_{UnityEngine.Random.Range(0, 9999)}";
                    }
                    if (!GUIShopData.items.ContainsKey(displayName))
                    {
                        itemDisplayNames.Add(entry.Key.shortname, displayName);
                        GUIShopData.items.Add(displayName, new ShopItem
                        {
                            item = entry.Key.shortname,
                            img = imageUrl,
                            buyCooldown = 0,
                            sellCooldown = 0,
                            sell = Math.Round(entry.Value * configData.recoveryRate, configData.keepdecimal),
                            buy = Math.Round(entry.Value, configData.keepdecimal),
                            Fixed = false,
                            cmd = null,
                        });
                    }
                }
                GUIShopData.items = GUIShopData.items.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

                foreach (var itemDefinition in ItemManager.GetItemDefinitions())
                {
                    ShopCategory shopCategory;
                    var categoryKey = itemDefinition.category.ToString().ToLower();
                    if (!GUIShopData.shops.TryGetValue(categoryKey, out shopCategory))
                    {
                        shopCategory = new ShopCategory
                        {
                            name = itemDefinition.category.ToString(),
                            description = "You currently have {0} coins to spend in the " + itemDefinition.category + " shop",
                        };
                        GUIShopData.shops.Add(categoryKey, shopCategory);
                    }
                    string displayName;
                    if (!itemDisplayNames.TryGetValue(itemDefinition.shortname, out displayName))
                        displayName = itemDefinition.displayName.english;
                    shopCategory.buy.Add(displayName);
                    shopCategory.sell.Add(displayName);
                }

                GUIShopData.shops.Add("commands", new ShopCategory
                {
                    name = "commands",
                    description = "You currently have {0} coins to spend in the commands shop",
                });

                SaveData("GUIShop", GUIShopData);
                PrintWarning("GUIShop successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_GUIShop.json");
            }
            else
            {
                Dictionary<string, RewardData> ServerRewardsData = new Dictionary<string, RewardData>();
                foreach (var entry in costs)
                {
                    string displayName;
                    if (!configData.displayNames.TryGetValue(entry.Key.shortname, out displayName))
                        displayName = entry.Key.displayName.english;
                    ulong skin = 0;
                    Category category;
                    Enum.TryParse(entry.Key.category.ToString(), true, out category);
                    string shortName = entry.Key.shortname + $"_{skin}";
                    if (!ServerRewardsData.ContainsKey(shortName))
                    {
                        ServerRewardsData.Add(shortName, new RewardData
                        {
                            shortname = entry.Key.shortname,
                            amount = 1,
                            skinId = skin,
                            isBp = false,
                            category = category,
                            displayName = displayName,
                            cost = (int)Math.Round(entry.Value),
                            cooldown = 0,
                        });
                    }
                }
                ServerRewardsData = ServerRewardsData.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                SaveData("ServerRewards", ServerRewardsData);
                PrintWarning("ServerRewards successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_ServerRewards.json");
            }
        }

        #region Configuration

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "GUIShop - Recovery rate (Sell / Buy)")]
            public float recoveryRate = 0.5f;

            [JsonProperty(PropertyName = "GUIShop - Keep decimal")]
            public int keepdecimal = 2;

            [JsonProperty(PropertyName = "Gather rate offset")]
            public float gatherRateOffset = 1f;

            [JsonProperty(PropertyName = "Materials list")]
            public Dictionary<string, double> materials = new Dictionary<string, double>();

            [JsonProperty(PropertyName = "No materials list")]
            public Dictionary<string, double> noMaterials = new Dictionary<string, double>();

            [JsonProperty(PropertyName = "Rarity list", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, int> rarityList = new Dictionary<string, int>
            {
                ["timed.explosive"] = 50,
                ["rifle.bolt"] = 20,
                ["hammer"] = 0
            };

            [JsonProperty(PropertyName = "Workbench level multiplier")]
            public Dictionary<int, float> workbenchMultiplier = new Dictionary<int, float>
            {
                [0] = 0,
                [1] = 0,
                [2] = 0,
                [3] = 0
            };

            [JsonProperty(PropertyName = "Item displayNames")]
            public Dictionary<string, string> displayNames = new Dictionary<string, string>();
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

        protected override void SaveConfig()
        {
            configData.materials = configData.materials.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            configData.noMaterials = configData.noMaterials.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            Config.WriteObject(configData);
        }

        #endregion Configuration

        #region DataFile

        //From Server Rewards
        private enum Category { None, Weapon, Construction, Items, Resources, Attire, Tool, Medical, Food, Ammunition, Traps, Misc, Component, Electrical, Fun }

        private class RewardData
        {
            public string shortname;
            public string customIcon;
            public int amount;
            public ulong skinId;
            public bool isBp;
            public Category category;
            public string displayName;
            public int cost;
            public int cooldown;
        }

        //From GUI Shop

        private class ShopData
        {
            [JsonProperty("Shop - Shop List")]
            public Dictionary<string, ShopCategory> shops = new Dictionary<string, ShopCategory>();

            [JsonProperty("Shop - Shop Categories")]
            public Dictionary<string, ShopItem> items = new Dictionary<string, ShopItem>();
        }

        private class ShopItem
        {
            public string item;
            public string img;
            public int buyCooldown;
            public int sellCooldown;
            public double sell;
            public double buy;
            public bool Fixed;
            public List<string> cmd;
        }

        public class ShopCategory
        {
            public string description;
            public string name;
            public List<string> buy = new List<string>();
            public List<string> sell = new List<string>();
            public bool npc;
            public string npcID;
        }

        private void SaveData<T>(string name, T data) => Interface.Oxide.DataFileSystem.WriteObject(Name + "/" + Name + "_" + name, data);

        #endregion DataFile
    }
}