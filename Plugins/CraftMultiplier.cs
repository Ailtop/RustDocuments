using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Craft Multiplier", "Arainrr", "1.4.1")]
    [Description("Multiplier in craft, can craft more items at once")]
    internal class CraftMultiplier : RustPlugin
    {
        #region Fields

        private const string PERMISSION_USE = "craftmultiplier.use";
        private bool initialized;
        private readonly Dictionary<ulong, int> enabledMultiplier = new Dictionary<ulong, int>();
        private readonly Dictionary<ulong, Timer> autoDisableTimer = new Dictionary<ulong, Timer>();

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            permission.RegisterPermission(PERMISSION_USE, this);
            cmd.AddChatCommand(configData.command, this, nameof(CmdCraftMultiplier));
        }

        private void Unload()
        {
            foreach (var value in autoDisableTimer.Values)
            {
                value?.Destroy();
            }
        }

        private void OnServerInitialized()
        {
            var hookSubscriptionsFieldInfo = typeof(PluginManager).GetField("hookSubscriptions", BindingFlags.Instance | BindingFlags.NonPublic);
            var hookSubscriptions = hookSubscriptionsFieldInfo?.GetValue(Interface.Oxide.RootPluginManager) as IDictionary<string, IList<Plugin>>;
            if (hookSubscriptions != null)
            {
                foreach (var entry in hookSubscriptions)
                {
                    if (entry.Key == nameof(OnItemCraft))
                    {
                        entry.Value.Remove(this);
                        entry.Value.Insert(0, this);
                    }
                }
                initialized = true;
            }
            else
            {
                PrintError("An error occurred. Please notify the plugin developer");
            }
        }

        private object OnItemCraft(ItemCraftTask itemCraftTask, BasePlayer player, Item item)
        {
            if (player == null) return null;
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return null;
            int multiplier;
            if (enabledMultiplier.TryGetValue(player.userID, out multiplier) && multiplier > 1)
            {
                var itemCrafter = player.inventory.crafting;
                if (itemCrafter == null) return null;
                var blueprint = itemCraftTask.blueprint;
                if (blueprint == null || blueprint.targetItem == null) return null;
                var existing = configData.itemList.Contains(blueprint.targetItem.shortname);
                if (configData.useBlacklist ? existing : !existing)
                {
                    Print(player, Lang("IsBlocked", player.UserIDString));
                    return null;
                }

                var amount = itemCraftTask.amount * multiplier;
                if (CanAffordCraftMultiplier(itemCrafter, blueprint, amount - itemCraftTask.amount))
                {
                    CollectIngredients(itemCrafter, blueprint, itemCraftTask, amount - itemCraftTask.amount, player);
                    itemCraftTask.amount = amount;
                }
                else
                {
                    Print(player, Lang("CantAfford", player.UserIDString));
                    return null;
                }

                itemCrafter.queue.AddLast(itemCraftTask);
                if (itemCraftTask.owner != null)
                    itemCraftTask.owner.Command("note.craft_add", itemCraftTask.taskUID,
                        itemCraftTask.blueprint.targetItem.itemid, itemCraftTask.amount, itemCraftTask.skinID);
                return false;
            }

            return null;
        }

        #endregion Oxide Hooks

        #region Methods

        private static void CollectIngredients(ItemCrafter itemCrafter, ItemBlueprint bp, ItemCraftTask task,
            int amount, BasePlayer player)
        {
            var collect = new List<Item>();
            foreach (var ingredient in bp.ingredients)
            {
                CollectIngredient(itemCrafter, ingredient.itemid, (int)ingredient.amount * amount, collect);
            }

            foreach (var item in collect)
            {
                item.CollectedForCrafting(player);
            }

            task.takenItems.AddRange(collect);
        }

        private static void CollectIngredient(ItemCrafter itemCrafter, int item, int amount, List<Item> collect)
        {
            foreach (var container in itemCrafter.containers)
            {
                amount -= container.Take(collect, item, amount);
                if (amount <= 0) break;
            }
        }

        private static bool CanAffordCraftMultiplier(ItemCrafter itemCrafter, ItemBlueprint bp, int amount)
        {
            foreach (var ingredient in bp.ingredients)
            {
                if (!DoesHaveUsableItem(itemCrafter, ingredient.itemid, (int)ingredient.amount * amount))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool DoesHaveUsableItem(ItemCrafter itemCrafter, int item, int iAmount)
        {
            var num = itemCrafter.containers.Sum(container => container.GetAmount(item, true));
            return num >= iAmount;
        }

        #endregion Methods

        #region Chat Command

        private void CmdCraftMultiplier(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }

            if (!initialized)
            {
                Print(player, Lang("CantUse", player.UserIDString));
                return;
            }

            if (args.Length <= 0)
            {
                if (enabledMultiplier.Remove(player.userID))
                {
                    Timer value;
                    if (autoDisableTimer.TryGetValue(player.userID, out value))
                    {
                        value?.Destroy();
                    }
                    ;
                    Print(player, Lang("DisabledCraftMultiplier", player.UserIDString));
                    return;
                }

                Print(player, Lang("MultiplierNotEnable", player.UserIDString, configData.command));
                return;
            }

            int multiplier;
            if (!int.TryParse(args[0], out multiplier) || multiplier <= 0)
            {
                Print(player, Lang("NotIntValue", player.UserIDString));
                return;
            }

            if (configData.maxMultiplier > 0 && multiplier > configData.maxMultiplier)
            {
                Print(player, Lang("MultiplierLimit", player.UserIDString, configData.maxMultiplier));
                return;
            }
            StringBuilder sb = new StringBuilder();
            if (enabledMultiplier.ContainsKey(player.userID))
            {
                enabledMultiplier[player.userID] = multiplier;
                sb.Append(Lang("ChangedCraftMultiplier", player.UserIDString, multiplier));
            }
            else
            {
                enabledMultiplier.Add(player.userID, multiplier);
                sb.Append(Lang("EnabledCraftMultiplier", player.UserIDString, multiplier));
            }
            Timer value1;
            if (autoDisableTimer.TryGetValue(player.userID, out value1))
            {
                value1?.Destroy();
            }
            if (configData.timeBeforeDisable > 0)
            {
                var playerID = player.userID;
                autoDisableTimer[player.userID] = timer.Once(configData.timeBeforeDisable, () =>
                {
                    autoDisableTimer.Remove(playerID);
                    enabledMultiplier.Remove(playerID);
                });
                sb.Append(Lang("AutoDisable", player.UserIDString, configData.timeBeforeDisable));
            }
            Print(player, sb.ToString());
        }

        #endregion Chat Command

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Chat Command")]
            public string command = "cm";

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "<color=#00FFFF>[CraftMultiplier]</color>: ";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Maximum Multiplier")]
            public int maxMultiplier = 10;

            [JsonProperty(PropertyName = "Time Before The Multiplier Is Disabled")]
            public float timeBeforeDisable = 120;

            [JsonProperty(PropertyName = "Use Blacklist (If false, a whitelist will be used)")]
            public bool useBlacklist = true;

            [JsonProperty(PropertyName = "Item List (Item short name)")]
            public HashSet<string> itemList = new HashSet<string>();
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
            Config.WriteObject(configData);
        }

        #endregion ConfigurationFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.prefix, configData.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args)
        {
            return string.Format(lang.GetMessage(key, this, id), args);
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You don't have permission to use this command.",
                ["NotIntValue"] = "Please use an integer number greater than zero.",
                ["EnabledCraftMultiplier"] = "You have <color=#7FFF00>Enabled</color> craft multiplier, multiplier is <color=#1E90FF>{0}</color>. ",
                ["ChangedCraftMultiplier"] = "Craft multiplier has been changed to <color=#1E90FF>{0}</color>. ",
                ["DisabledCraftMultiplier"] = "You have <color=#FF8C00>Disabled</color> craft multiplier.",
                ["MultiplierNotEnable"] = "You have not enabled the craft multiplier, usage: '<color=#1E90FF>/{0} #</color>' -- # is the multiplier.",
                ["CantAfford"] = "You don't have enough resources to use craft multiplier.",
                ["IsBlocked"] = "This item is blocked by craft multiplier and cannot be multiplied.",
                ["CantUse"] = "Unable to use, please wait for server initialization.",
                ["MultiplierLimit"] = "The craft multiplier cannot be greater than {0}",
                ["AutoDisable"] = "The craft multiplier will disabled after {0} seconds"
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["NotIntValue"] = "制作倍数必须是大于0的整数",
                ["EnabledCraftMultiplier"] = "您 <color=#7FFF00>已启用</color> 制作翻倍, 制作倍数为 <color=#1E90FF>{0}</color>。",
                ["ChangedCraftMultiplier"] = "您的制作倍数更改为 <color=#1E90FF>{0}</color>。",
                ["DisabledCraftMultiplier"] = "您 <color=#FF8C00>已禁用</color> 制作翻倍",
                ["MultiplierNotEnable"] = "您没有启用制作倍数, 语法: '<color=#1E90FF>/{0} #</color>' -- #是制作倍数",
                ["CantAfford"] = "您没有足够的资源使物品制作翻倍",
                ["IsBlocked"] = "该物品被阻止了，无法翻倍",
                ["CantUse"] = "无法使用制作翻倍, 请等待服务器初始化",
                ["MultiplierLimit"] = "制作倍数不能大于 {0}",
                ["AutoDisable"] = "制作倍数将在 {0} 秒后禁用"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}