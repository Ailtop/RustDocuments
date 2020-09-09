using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("ItemCostCalculator", "Absolut/Arainrr", "2.0.8", ResourceId = 2109)]
    internal class ItemCostCalculator : RustPlugin
    {
        [PluginReference] private Plugin ImageLibrary;
        private readonly Dictionary<ItemDefinition, double> costs = new Dictionary<ItemDefinition, double>();

        private void OnServerInitialized()
        {
            if (ImageLibrary == null)
                PrintError("Unable to get image url of item without ImageLibrary loaded");
            foreach (var item in ItemManager.GetItemDefinitions())
            {
                if (!configData.displayNames.ContainsKey(item.shortname))
                    configData.displayNames.Add(item.shortname, item.displayName.english);
                var itemBlueprint = ItemManager.FindBlueprint(item);
                if (itemBlueprint != null)
                {
                    foreach (var itemAmount in itemBlueprint.ingredients)
                        if (!configData.materials.ContainsKey(itemAmount.itemDef.shortname))
                            configData.materials.Add(itemAmount.itemDef.shortname, 1);
                }
                else if (!configData.noMaterials.ContainsKey(item.shortname))
                    configData.noMaterials.Add(item.shortname, 1);
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
            if (ImageLibrary == null) return string.Empty;
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
            costs.Clear();
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
                        cost = cost * configData.gatherRateOffset;
                    int rarity;
                    if (configData.rarityList.TryGetValue(itemDefinition.shortname, out rarity))
                        cost = cost + cost * (rarity / 100);
                    float level;
                    if (configData.workbenchMultiplier.TryGetValue(itemBlueprint.workbenchLevelRequired, out level))
                        cost = cost + cost * (level / 100);
                    costs.Add(itemDefinition, cost);
                }
            }
            CreatDataFile(isGUIShop);
        }

        private void CreatDataFile(bool isGUIShop = true)
        {
            if (isGUIShop)
            {
                Dictionary<string, ShopData> GUIShopData = new Dictionary<string, ShopData>();
                foreach (var entry in costs)
                {
                    var imageUrl = GetImageUrl(entry.Key);
                    string displayName = entry.Key.displayName.english;
                    configData.displayNames.TryGetValue(entry.Key.shortname, out displayName);
                    if (GUIShopData.ContainsKey(displayName)) displayName += $"_repeat{UnityEngine.Random.Range(0, 1000)}";
                    if (!GUIShopData.ContainsKey(displayName))
                    {
                        GUIShopData.Add(displayName, new ShopData
                        {
                            item = entry.Key.shortname,
                            buy = Math.Round(entry.Value, configData.keepdecimal).ToString(),
                            sell = Math.Round(entry.Value * configData.recoveryRate, configData.keepdecimal).ToString(),
                            img = imageUrl,
                            cooldown = "0",
                        });
                    }
                }
                GUIShopData = GUIShopData.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                SaveData("GUIShop", GUIShopData);
                PrintWarning("GUIShop successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_GUIShop.json");
            }
            else
            {
                Dictionary<string, RewardData> ServerRewardsData = new Dictionary<string, RewardData>();
                foreach (var entry in costs)
                {
                    string displayName = entry.Key.displayName.english;
                    configData.displayNames.TryGetValue(entry.Key.shortname, out displayName);
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
            public string displayName;
            public int cost;
            public int cooldown;
            public string shortname;
            public string customIcon;
            public int amount;
            public ulong skinId;
            public bool isBp;
            public Category category;
        }

        private class ShopData
        {
            public string buy;
            public string sell;
            public string item;
            public string cooldown;
            public string img;
        }

        private void SaveData<T>(string name, T data) => Interface.Oxide.DataFileSystem.WriteObject(Name + "/" + Name + "_" + name, data);

        #endregion DataFile
    }
}