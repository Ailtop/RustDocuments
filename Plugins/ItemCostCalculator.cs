using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Item Cost Calculator", "Absolut/Arainrr", "2.0.14", ResourceId = 2109)]
    internal class ItemCostCalculator : RustPlugin
    {
        #region Fields

        [PluginReference] private Plugin ImageLibrary;
        private readonly Dictionary<ItemDefinition, double> itemsCost = new Dictionary<ItemDefinition, double>();

        private enum FileType
        {
            GUIShop,
            ServerReward,
            Ingredient,
        }

        #endregion Fields

        #region Oxide Hooks

        private void OnServerInitialized()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (!configData.displayNames.ContainsKey(itemDefinition.shortname))
                {
                    configData.displayNames.Add(itemDefinition.shortname, itemDefinition.displayName.english);
                }
                var itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
                if (itemBlueprint != null)
                {
                    foreach (var itemAmount in itemBlueprint.ingredients)
                    {
                        if (!configData.materials.ContainsKey(itemAmount.itemDef.shortname))
                        {
                            configData.materials.Add(itemAmount.itemDef.shortname, 1);
                        }
                    }
                }
                else if (!configData.noMaterials.ContainsKey(itemDefinition.shortname))
                {
                    configData.noMaterials.Add(itemDefinition.shortname, 1);
                }
            }
            foreach (var material in configData.materials)
            {
                if (configData.noMaterials.ContainsKey(material.Key))
                {
                    configData.noMaterials.Remove(material.Key);
                }
            }

            SaveConfig();
            CalculateItemsCost();
            foreach (FileType value in Enum.GetValues(typeof(FileType)))
            {
                CreateDataFile(value);
            }
        }

        #endregion Oxide Hooks

        #region Methods

        private void CalculateItemsCost()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                double amount;
                if (configData.materials.TryGetValue(itemDefinition.shortname, out amount))
                {
                    itemsCost.Add(itemDefinition, amount);
                    continue;
                }
                if (configData.noMaterials.TryGetValue(itemDefinition.shortname, out amount))
                {
                    itemsCost.Add(itemDefinition, amount);
                    continue;
                }

                var itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
                if (itemBlueprint == null) continue;

                double cost = 0;
                var ingredients = GetItemIngredients(itemBlueprint);
                foreach (var ingredient in ingredients)
                {
                    if (configData.materials.TryGetValue(ingredient.Key.shortname, out amount))
                    {
                        cost += ingredient.Value * amount;
                    }
                    else
                    {
                        if (itemsCost.TryGetValue(ingredient.Key, out amount))
                        {
                            cost += ingredient.Value * amount;
                        }
                    }
                }
                if (cost > 0)
                {
                    cost /= itemBlueprint.amountToCreate;
                    if (configData.gatherRateOffset > 0f)
                    {
                        cost *= configData.gatherRateOffset;
                    }
                    int rarity;
                    if (configData.rarityList.TryGetValue(itemDefinition.shortname, out rarity) && rarity > 0f)
                    {
                        cost += cost * (rarity / 100d);
                    }
                    float level;
                    if (configData.workbenchMultiplier.TryGetValue(itemBlueprint.workbenchLevelRequired, out level) && level > 0f)
                    {
                        cost += cost * (level / 100d);
                    }
                    itemsCost.Add(itemDefinition, cost);
                }
            }
        }

        private Dictionary<ItemDefinition, int> GetItemIngredients(ItemBlueprint itemBlueprint)
        {
            var ingredients = new Dictionary<ItemDefinition, int>();
            Dictionary<string, int> ingredientsOverride;
            if (configData.ingredientsOverride.TryGetValue(itemBlueprint.targetItem.shortname, out ingredientsOverride))
            {
                foreach (var ingredientOverride in ingredientsOverride)
                {
                    var itemDefinition = ItemManager.FindItemDefinition(ingredientOverride.Key);
                    if (itemDefinition != null)
                    {
                        ingredients.Add(itemDefinition, ingredientOverride.Value);
                    }
                }
                return ingredients;
            }
            foreach (var itemAmount in itemBlueprint.ingredients)
            {
                ingredients.Add(itemAmount.itemDef, (int)itemAmount.amount);
            }
            return ingredients;
        }

        private void CreateDataFile(FileType fileType)
        {
            switch (fileType)
            {
                case FileType.GUIShop:
                    {
                        var guiShopData = new ShopData();
                        var itemDisplayNames = new Dictionary<string, string>();
                        foreach (var entry in itemsCost)
                        {
                            var displayName = GetItemDisplayName(entry.Key);
                            if (string.IsNullOrEmpty(displayName)) continue;
                            var displayNameKey = displayName;
                            var imageUrl = GetImageUrl(entry.Key);
                            if (guiShopData.ShopItems.ContainsKey(displayNameKey))
                            {
                                displayNameKey += $"_Repeat_{UnityEngine.Random.Range(0, 9999)}";
                            }

                            if (!guiShopData.ShopItems.ContainsKey(displayNameKey))
                            {
                                itemDisplayNames.Add(entry.Key.shortname, displayNameKey);
                                guiShopData.ShopItems.Add(displayNameKey, new ShopItem
                                {
                                    DisplayName = displayName,
                                    Shortname = entry.Key.shortname,
                                    EnableBuy = true,
                                    EnableSell = true,
                                    BuyPrice = Math.Round(entry.Value, configData.keepdecimal),
                                    SellPrice = Math.Round(entry.Value * configData.recoveryRate, configData.keepdecimal),
                                    Image = !string.IsNullOrEmpty(imageUrl) ? imageUrl : "https://rustlabs.com/img/items180/{entry.Key.shortname}.png",
                                });
                            }
                        }

                        guiShopData.ShopItems = guiShopData.ShopItems.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);

                        foreach (var itemDefinition in ItemManager.GetItemDefinitions())
                        {
                            ShopCategory shopCategory;
                            var categoryKey = itemDefinition.category.ToString();
                            if (!guiShopData.ShopCategories.TryGetValue(categoryKey, out shopCategory))
                            {
                                shopCategory = new ShopCategory
                                {
                                    DisplayName = categoryKey,
                                    EnabledCategory = true,
                                    Description = "You currently have {0} coins to spend in the " + categoryKey + " shop",
                                };
                                guiShopData.ShopCategories.Add(categoryKey, shopCategory);
                            }

                            string displayName;
                            if (!itemDisplayNames.TryGetValue(itemDefinition.shortname, out displayName))
                            {
                                displayName = itemDefinition.displayName.english;
                            }
                            if (!string.IsNullOrEmpty(displayName))
                            {
                                shopCategory.Items.Add(displayName);
                            }
                        }

                        SaveData("GUIShop", guiShopData);
                        PrintWarning("GUIShop successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_GUIShop.json");
                    }
                    return;

                case FileType.ServerReward:
                    {
                        var serverRewardsData = new Dictionary<string, RewardData>();
                        var skin = 0UL;
                        foreach (var entry in itemsCost)
                        {
                            var displayName = GetItemDisplayName(entry.Key);
                            if (string.IsNullOrEmpty(displayName)) continue;
                            var shortName = $"{entry.Key.shortname}_{skin}";
                            Category category;
                            if (!Enum.TryParse(entry.Key.category.ToString(), out category))
                            {
                                category = Category.None;
                            }
                            if (!serverRewardsData.ContainsKey(shortName))
                            {
                                serverRewardsData.Add(shortName, new RewardData
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

                        serverRewardsData = serverRewardsData.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
                        SaveData("ServerRewards", serverRewardsData);
                        PrintWarning("ServerRewards successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_ServerRewards.json");
                    }
                    return;

                case FileType.Ingredient:
                    {
                        var itemIngredients = new Dictionary<string, Ingredient>();
                        foreach (var itemDefinition in ItemManager.GetItemDefinitions())
                        {
                            var itemBlueprint = ItemManager.FindBlueprint(itemDefinition);
                            if (itemBlueprint != null)
                            {
                                Ingredient ingredient = new Ingredient();
                                ingredient.description = $"{string.Join(", ", itemBlueprint.ingredients.Select(x => $"'{x.itemDef.shortname} x{x.amount}"))}' to craft '{itemDefinition.shortname} x{itemBlueprint.amountToCreate}'.";
                                ingredient.amountToCreate = itemBlueprint.amountToCreate;
                                ingredient.ingredients = new Dictionary<string, int>();
                                foreach (var itemAmount in itemBlueprint.ingredients)
                                {
                                    ingredient.ingredients.Add(itemAmount.itemDef.shortname, (int)itemAmount.amount);
                                }
                                itemIngredients.Add(itemDefinition.shortname, ingredient);
                            }
                        }
                        SaveData("ItemIngredients", itemIngredients);
                        PrintWarning("ItemIngredients successfully created, data file path: data/ItemCostCalculator/ItemCostCalculator_ItemIngredients.json");
                    }
                    return;
            }
        }

        private struct Ingredient
        {
            public string description;
            public int amountToCreate;
            public Dictionary<string, int> ingredients;
        }

        private string GetItemDisplayName(ItemDefinition itemDefinition)
        {
            string displayName;
            if (configData.displayNames.TryGetValue(itemDefinition.shortname, out displayName))
            {
                return displayName;
            }
            return itemDefinition.displayName.english;
        }

        private string GetImageUrl(ItemDefinition itemDefinition, ulong skin = 0)
        {
            if (ImageLibrary == null) return null;
            return (string)ImageLibrary.Call("GetImageURL", itemDefinition.shortname, skin);
        }

        #endregion Methods

        #region API

        private double GetItemCost(string shortname)
        {
            var itemDefinition = ItemManager.FindItemDefinition(shortname);
            if (itemDefinition == null) return -1;
            return GetItemCost(itemDefinition);
        }

        private double GetItemCost(int itemID)
        {
            var itemDefinition = ItemManager.FindItemDefinition(itemID);
            if (itemDefinition == null) return -1;
            return GetItemCost(itemDefinition);
        }

        private double GetItemCost(ItemDefinition itemDefinition)
        {
            double cost;
            if (!itemsCost.TryGetValue(itemDefinition, out cost))
            {
                return -1;
            }
            return cost;
        }

        private Dictionary<string, double> GetItemsCostByShortName()
        {
            return itemsCost.ToDictionary(x => x.Key.shortname, y => y.Value);
        }

        private Dictionary<int, double> GetItemsCostByID()
        {
            return itemsCost.ToDictionary(x => x.Key.itemid, y => y.Value);
        }

        private Dictionary<ItemDefinition, double> GetItemsCostByDefinition()
        {
            return itemsCost.ToDictionary(x => x.Key, y => y.Value);
        }

        #endregion API

        #region Commands

        [ConsoleCommand("costfile")]
        private void CmdCostFile(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length != 1 || !arg.IsAdmin)
            {
                goto SyntaxError;
            }
            switch (arg.Args[0].ToLower())
            {
                case "shop":
                    CreateDataFile(FileType.GUIShop);
                    return;

                case "reward":
                    CreateDataFile(FileType.ServerReward);
                    return;

                case "ingredients":
                    CreateDataFile(FileType.Ingredient);
                    return;
            }
SyntaxError:
            SendReply(arg, "Syntax error, please type 'costfile <shop / reward / ingredients>'");
        }

        #endregion Commands

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

            [JsonProperty(PropertyName = "Item ingredients override", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public Dictionary<string, Dictionary<string, int>> ingredientsOverride =
                new Dictionary<string, Dictionary<string, int>>
                {
                    ["horse.shoes.basic"] = new Dictionary<string, int>
                    {
                        ["metal.fragments"] = 50,
                    }
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

        protected override void SaveConfig()
        {
            //configData.materials = configData.materials.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            //configData.noMaterials = configData.noMaterials.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
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
            [JsonProperty("Shop - Shop Categories")]
            public Dictionary<string, ShopCategory> ShopCategories = new Dictionary<string, ShopCategory>();

            [JsonProperty("Shop - Shop List")]
            public Dictionary<string, ShopItem> ShopItems = new Dictionary<string, ShopItem>();
        }

        public class ShopItem
        {
            public string DisplayName;
            public string Shortname;
            public bool EnableBuy;
            public bool EnableSell;
            public string Image;
            public double SellPrice;
            public double BuyPrice;
            public int BuyCooldown;
            public int SellCooldown;

            //public int BuyLimit;
            //public int SellLimit;
            public string KitName;

            public List<string> Command;
            public ulong SkinId;
        }

        public class ShopCategory
        {
            public string DisplayName;
            public string Description;
            public bool EnabledCategory;

            //public bool BluePrints;
            public bool EnableNPC;

            public string NPCId = "";
            public HashSet<string> Items = new HashSet<string>();
        }

        private void SaveData<T>(string name, T data) => Interface.Oxide.DataFileSystem.WriteObject(Name + "/" + Name + "_" + name, data);

        #endregion DataFile
    }
}