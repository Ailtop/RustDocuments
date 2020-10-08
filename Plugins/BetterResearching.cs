using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apex;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Better Researching", "Arainrr", "1.1.2")]
    [Description("Modify research time, cost, chance")]
    public class BetterResearching : RustPlugin
    {
        [PluginReference] private readonly Plugin PopupNotifications;

        private static BetterResearching instance;
        private const string PREFAB_RESEARCH_TABLE = "assets/prefabs/deployable/research table/researchtable_deployed.prefab";

        private ItemDefinition researchResource;
        private ItemDefinition defaultResearchResource;
        private readonly Hash<ResearchTable, BasePlayer> researchers = new Hash<ResearchTable, BasePlayer>();
        private readonly Hash<ResearchTable, HashSet<BasePlayer>> lootingPlayers = new Hash<ResearchTable, HashSet<BasePlayer>>();

        private void Init()
        {
            LoadData();
            instance = this;
            Unsubscribe(nameof(OnItemSplit));
            Unsubscribe(nameof(CanAcceptItem));
            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnItemRemovedFromContainer));
        }

        private void OnServerInitialized()
        {
            UpdateConfig();
            Subscribe(nameof(OnItemSplit));
            Subscribe(nameof(CanAcceptItem));
            Subscribe(nameof(OnItemRemovedFromContainer));
            if (configData.researchResource != "scrap")
            {
                researchResource = ItemManager.FindItemDefinition(configData.researchResource);
                if (researchResource != null)
                {
                    Subscribe(nameof(OnEntitySpawned));
                    foreach (var researchTable in BaseNetworkable.serverEntities.OfType<ResearchTable>())
                    {
                        if (defaultResearchResource != null)
                        {
                            defaultResearchResource = researchTable.researchResource;
                        }
                        OnEntitySpawned(researchTable);
                    }
                }
            }
            foreach (var player in BasePlayer.activePlayerList)
            {
                var researchTable = player.inventory?.loot?.entitySource as ResearchTable;
                if (researchTable != null)
                {
                    OnLootEntity(player, researchTable);
                }
            }
        }

        private void OnEntitySpawned(ResearchTable researchTable)
        {
            if (researchTable == null) return;
            researchTable.researchResource = researchResource;
            researchTable.SendNetworkUpdateImmediate();
        }

        private void Unload()
        {
            SaveData();
            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyAllUI(player);
            }
            if (configData.researchResource != "scrap")
            {
                researchResource = defaultResearchResource;
                foreach (var researchTable in BaseNetworkable.serverEntities.OfType<ResearchTable>())
                    OnEntitySpawned(researchTable);
            }
            instance = null;
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void UpdateConfig()
        {
            var researchTable = GameManager.server.FindPrefab(PREFAB_RESEARCH_TABLE)?.GetComponent<ResearchTable>();
            if (researchTable == null) return;
            var newResearchS = new Dictionary<string, ConfigData.ResearchSettings>();
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                var item = ItemManager.CreateByItemID(itemDefinition.itemid);
                if (researchTable.IsItemResearchable(item))
                {
                    ConfigData.ResearchSettings researchS;
                    if (configData.researchS.TryGetValue(item.info.shortname, out researchS))
                    {
                        newResearchS.Add(itemDefinition.shortname, researchS);
                    }
                    else
                    {
                        newResearchS.Add(itemDefinition.shortname, new ConfigData.ResearchSettings
                        {
                            displayName = itemDefinition.displayName.english,
                            scrapAmount = researchTable.ScrapForResearch(item),
                            itemConsumedSettings = new Dictionary<int, ConfigData.ConsumeSettings>
                            {
                                [1] = new ConfigData.ConsumeSettings()
                            }
                        });
                    }
                }
                item.Remove();
            }
            ItemManager.DoRemoves();
            configData.researchS = newResearchS;
            SaveConfig();
        }

        private object CanResearchItem(BasePlayer player, Item targetItem)
        {
            ConfigData.ResearchSettings researchS;
            if (configData.researchS.TryGetValue(targetItem.info.shortname, out researchS))
            {
                if (!researchS.canResearch)
                {
                    SendMessage(player, Lang("IsBlocked", player.UserIDString, researchS.displayName));
                    return false;
                }
                var scrapItem = GetScrapItem(targetItem.parent?.entityOwner as ResearchTable);
                if (scrapItem == null) return null;
                if (scrapItem.amount < researchS.scrapAmount)
                {
                    SendMessage(player, Lang("ResearchCantAfford", player.UserIDString, researchS.displayName, researchS.scrapAmount));
                    return false;
                }
            }
            return null;
        }

        private void OnItemResearch(ResearchTable researchTable, Item targetItem, BasePlayer player)
        {
            researchers[researchTable] = player;
            ConfigData.ResearchSettings researchS;
            if (configData.researchS.TryGetValue(targetItem.info.shortname, out researchS))
            {
                if (!researchS.canResearch) return;
                researchTable.researchDuration = researchS.researchTime;
                if (researchS.researchTime > 0)
                {
                    float timeLeft;
                    Timer researchTimer = null;
                    researchTimer = timer.Repeat(0.1f, (int)(researchS.researchTime / 0.1f) + 10, () =>
                    {
                        if (researchTable == null || researchTable.IsDestroyed)
                        {
                            researchTimer?.Destroy();
                            return;
                        }
                        timeLeft = researchTable.researchFinishedTime - Time.realtimeSinceStartup;
                        if (timeLeft <= 0)
                        {
                            researchTimer?.Destroy();
                            UpdateUI(researchTable.inventory, null);
                            return;
                        }

                        HashSet<BasePlayer> looters;
                        if (lootingPlayers.TryGetValue(researchTable, out looters))
                        {
                            foreach (var p in looters)
                            {
                                CreateResearchUI(p, timeLeft.ToString("0.00"), configData.uiS.researchTimeleftTextSize);
                            }
                        }
                        else if (player != null && player.inventory?.loot?.entitySource == researchTable)
                        {
                            CreateResearchUI(player, timeLeft.ToString("0.00"), configData.uiS.researchTimeleftTextSize);
                        }
                    });
                }
            }
        }

        private object OnItemResearched(ResearchTable researchTable, int chance)
        {
            var targetItem = researchTable.GetTargetItem();
            ConfigData.ResearchSettings researchS;
            if (configData.researchS.TryGetValue(targetItem.info.shortname, out researchS))
            {
                if (!researchS.canResearch) return null;
                BasePlayer researcher;
                if (researchers.TryGetValue(researchTable, out researcher))
                {
                    researchers.Remove(researchTable);
                }
                else
                {
                    researcher = researchTable.user;
                }

                HashSet<BasePlayer> players;
                if (!lootingPlayers.TryGetValue(researchTable, out players))
                {
                    players = new HashSet<BasePlayer> { researcher };
                }

                if (UnityEngine.Random.Range(0f, 100f) < researchS.successChance)
                {
                    foreach (var player in players)
                    {
                        if (player == null) continue;
                        SendMessage(player, Lang("ResearchSuccess", player.UserIDString, researchS.displayName));
                    }
                    return researchS.scrapAmount;
                }
                else
                {
                    var scrapItem = researchTable.GetScrapItem();
                    TryConsumeItem(researcher, scrapItem, targetItem, researchS, players);
                    return scrapItem.amount + 1;
                }
            }
            return null;
        }

        private void OnLootEntity(BasePlayer player, ResearchTable researchTable)
        {
            if (player == null || researchTable == null) return;
            HashSet<BasePlayer> looters;
            if (!lootingPlayers.TryGetValue(researchTable, out looters))
            {
                looters = new HashSet<BasePlayer> { player };
                lootingPlayers.Add(researchTable, looters);
            }
            else
            {
                looters.Add(player);
            }
            UpdateUI(researchTable.inventory, null);
        }

        private void OnLootEntityEnd(BasePlayer player, ResearchTable researchTable)
        {
            if (player == null || researchTable == null) return;
            HashSet<BasePlayer> players;
            if (lootingPlayers.TryGetValue(researchTable, out players))
            {
                players.Remove(player);
                if (players.Count <= 0)
                {
                    lootingPlayers.Remove(researchTable);
                }
                DestroyAllUI(player);
            }
        }

        private void OnItemSplit(Item item, int splitAmount) => NextTick(() => UpdateUI(null, item));

        private void CanAcceptItem(ItemContainer itemContainer, Item item, int targetPos) => NextTick(() => UpdateUI(itemContainer, item));

        private void OnItemRemovedFromContainer(ItemContainer itemContainer, Item item) => NextTick(() => UpdateUI(itemContainer, item));

        #region Methods

        private void UpdateUI(ItemContainer itemContainer, Item item)
        {
            var researchTable = itemContainer?.entityOwner as ResearchTable;
            if (researchTable == null)
            {
                researchTable = item?.parent?.entityOwner as ResearchTable;
            }

            if (researchTable == null) return;
            int scrapAmount = 0;
            bool flag = CanResearch(researchTable, ref scrapAmount);
            HashSet<BasePlayer> looters;
            if (lootingPlayers.TryGetValue(researchTable, out looters))
            {
                foreach (var player in looters)
                {
                    CreateCostUI(player, scrapAmount);
                    if (!researchTable.IsResearching())
                        CreateResearchUI(player, flag ? string.Empty : Lang("CantResearch", player.UserIDString));
                }
            }
        }

        private bool CanResearch(ResearchTable researchTable, ref int scrapAmount)
        {
            var targetItem = researchTable.GetTargetItem();
            if (targetItem != null && targetItem.amount <= 1 && !targetItem.isBroken && researchTable.IsItemResearchable(targetItem))
            {
                ConfigData.ResearchSettings researchS;
                if (configData.researchS.TryGetValue(targetItem.info.shortname, out researchS))
                {
                    if (researchS.canResearch)
                    {
                        scrapAmount = researchS.scrapAmount;
                        var scrapItem = GetScrapItem(researchTable);
                        if (scrapItem != null && scrapItem.amount >= researchS.scrapAmount)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void TryConsumeItem(BasePlayer researcher, Item scrapItem, Item targetItem, ConfigData.ResearchSettings researchS, HashSet<BasePlayer> players)
        {
            var failures = GetNumberOfFailures(researcher.userID, targetItem.info.shortname);
            var consumeS = GetConsumeSettings(researchS, failures);
            if (UnityEngine.Random.Range(0f, 100f) < consumeS.scrapChance)
            {
                if (consumeS.scrapPercentage > 0)
                {
                    var amount = (int)Math.Round(researchS.scrapAmount * consumeS.scrapPercentage / 100);
                    TakeItem(scrapItem, amount);
                    foreach (var player in players)
                    {
                        if (player == null) continue;
                        CreateResearchUI(player);
                        SendMessage(player, Lang("ResearchFailedScrap", player.UserIDString, researchS.displayName, amount));
                    }
                }
                else
                {
                    foreach (var player in players)
                    {
                        if (player == null) continue;
                        CreateResearchUI(player);
                        SendMessage(player, Lang("ResearchFailed", player.UserIDString, researchS.displayName));
                    }
                }
            }

            if (UnityEngine.Random.Range(0f, 100f) < consumeS.targetItemChance)
            {
                TakeItem(targetItem, targetItem.amount);
                foreach (var player in players)
                {
                    if (player == null) continue;
                    CreateResearchUI(player);
                    SendMessage(player, Lang("ResearchFailedTargetItem", player.UserIDString, researchS.displayName));
                }
            }
        }

        private int GetNumberOfFailures(ulong playerID, string shortname)
        {
            Dictionary<string, int> data;
            if (!storedData.playerResearchFailures.TryGetValue(playerID, out data))
            {
                data = new Dictionary<string, int>();
                storedData.playerResearchFailures.Add(playerID, data);
            }

            int failures;
            if (!data.TryGetValue(shortname, out failures))
            {
                failures = 1;
                data.Add(shortname, failures);
            }
            else
            {
                failures++;
                data[shortname] = failures;
            }
            return failures;
        }

        private static void DoResearch(ResearchTable researchTable, BasePlayer player)
        {
            BaseEntity.RPCMessage rpcMessage = default(BaseEntity.RPCMessage);
            rpcMessage.player = player;
            researchTable.DoResearch(rpcMessage);
        }

        private static Item GetScrapItem(ResearchTable researchTable)
        {
            var slot = researchTable?.inventory?.GetSlot(1);
            if (slot == null || slot.info != researchTable.researchResource)
                return null;
            return slot;
        }

        private static void TakeItem(Item item, int amount)
        {
            if (item.amount <= amount)
            {
                item.RemoveFromContainer();
                item.Remove();
            }
            else item.UseItem(amount);
        }

        private static ConfigData.ConsumeSettings GetConsumeSettings(ConfigData.ResearchSettings researchS, int failures)
        {
            return researchS.itemConsumedSettings.OrderByDescending(x => x.Key).FirstOrDefault(x => x.Key <= failures).Value;
        }

        private void SendMessage(BasePlayer player, string message)
        {
            if (configData.chatS.usePop && PopupNotifications != null) CreatePopupNotification(message, player);
            else Print(player, message);
        }

        private void CreatePopupNotification(string message, BasePlayer player = null, float duration = 0f) => PopupNotifications?.Call("CreatePopupNotification", message, player, duration);

        #endregion Methods

        #region UI

        private const string UINAME_COST = "ResearchUI_Cost";
        private const string UINAME_RESEARCH = "ResearchUI_Research";

        public class UI
        {
            public static CuiElementContainer CreateElementContainer(string parent, string panelName, string backgroundColor, string anchorMin, string anchorMax, string offsetMin, string offsetMax, bool cursor = false)
            {
                return new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = { Color = backgroundColor },
                            RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax ,OffsetMin = offsetMin,OffsetMax = offsetMax},
                            CursorEnabled = cursor
                        },  parent,  panelName
                    }
                };
            }

            public static void CreateLabel(ref CuiElementContainer container, string panelName, string textColor, string text, int fontSize, string anchorMin, string anchorMax, TextAnchor align = TextAnchor.MiddleCenter, float fadeIn = 0f)
            {
                container.Add(new CuiLabel
                {
                    Text = { Color = textColor, FontSize = fontSize, Align = align, Text = text, FadeIn = fadeIn },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
                }, panelName, CuiHelper.GetGuid());
            }

            public static void CreateButton(ref CuiElementContainer container, string panelName, string buttonColor, string command, string textColor, string text, int fontSize, string anchorMin, string anchorMax, TextAnchor align = TextAnchor.MiddleCenter, float fadeIn = 0f)
            {
                container.Add(new CuiButton
                {
                    Button = { Color = buttonColor, Command = command },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
                    Text = { Color = textColor, Text = text, FontSize = fontSize, Align = align, FadeIn = fadeIn }
                }, panelName, CuiHelper.GetGuid());
            }
        }

        private static void CreateResearchUI(BasePlayer player, string message = "", int fontSize = 0)
        {
            if (player == null) return;
            var container = UI.CreateElementContainer("Overlay", UINAME_RESEARCH, instance.configData.uiS.researchBackgroundColor, "0.5 0", "0.5 0", "436 116", "565 148");
            if (string.IsNullOrEmpty(message)) UI.CreateButton(ref container, UINAME_RESEARCH, "0 0 0 0", "ResearchUI_DoResearch", instance.configData.uiS.researchTextColor, instance.Lang("CanResearch", player.UserIDString), instance.configData.uiS.researchTextSize, "0 0", "1 1");
            else UI.CreateLabel(ref container, UINAME_RESEARCH, instance.configData.uiS.researchTextColor, message, fontSize == 0 ? instance.configData.uiS.researchTextSize : fontSize, "0 0", "1 1");
            CuiHelper.DestroyUi(player, UINAME_RESEARCH);
            CuiHelper.AddUi(player, container);
        }

        private static void CreateCostUI(BasePlayer player, int scrapAmount)
        {
            if (player == null) return;
            string message = scrapAmount > 0 ? scrapAmount.ToString() : "N/A";
            var container = UI.CreateElementContainer("Overlay", UINAME_COST, instance.configData.uiS.researchCostBackgroundColor, "0.5 0", "0.5 0", "445 292", "572 372");
            UI.CreateLabel(ref container, UINAME_COST, instance.configData.uiS.researchCostTextColor, message, instance.configData.uiS.researchCostTextSize, "0 0", "1 1");
            CuiHelper.DestroyUi(player, UINAME_COST);
            CuiHelper.AddUi(player, container);
        }

        private static void DestroyAllUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UINAME_COST);
            CuiHelper.DestroyUi(player, UINAME_RESEARCH);
        }

        #endregion UI

        #region Commands

        [ConsoleCommand("ResearchUI_DoResearch")]
        private void CCmdDoResearch(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            var researchTable = player.inventory?.loot?.entitySource as ResearchTable;
            int scrapAmount = 0;
            if (researchTable != null && CanResearch(researchTable, ref scrapAmount))
            {
                DoResearch(researchTable, player);
            }
        }

        [ConsoleCommand("br.lvl")]
        private void CCmdResearchS(ConsoleSystem.Arg arg)
        {
            if (arg != null && arg.HasArgs(3) && arg.IsAdmin)
            {
                int level;
                if (int.TryParse(arg.Args[0], out level))
                {
                    var blueprintLevels = ItemManager.GetBlueprints().GroupBy(x => x.workbenchLevelRequired).ToDictionary(x => x.Key, y => y.Select(x => x.targetItem.shortname));
                    IEnumerable<string> itemBlueprints;
                    if (blueprintLevels.TryGetValue(level, out itemBlueprints))
                    {
                        switch (arg.Args[1].ToLower())
                        {
                            case "cost":
                                int cost;
                                if (!int.TryParse(arg.Args[2], out cost) || cost <= 0) break;
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                        entry.Value.scrapAmount = cost;
                                }
                                Print(arg, $"Successfully modified the research cost of level {level} workbench items.");
                                SaveConfig();
                                return;

                            case "time":
                                float time;
                                if (!float.TryParse(arg.Args[2], out time) || time < 0) break;
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                        entry.Value.researchTime = time;
                                }
                                Print(arg, $"Successfully modified the research time of level {level} workbench items.");
                                SaveConfig();
                                return;

                            case "successchance":
                                float chance;
                                if (!float.TryParse(arg.Args[2], out chance) || chance < 0) break;
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                        entry.Value.successChance = chance;
                                }
                                Print(arg, $"Successfully modified the chance of research success of level {level} workbench items.");
                                SaveConfig();
                                return;

                            case "scrapchance":
                                var scrapChances = new Dictionary<int, float>();
                                for (int i = 2; i < arg.Args.Length; i++)
                                {
                                    float value;
                                    if (float.TryParse(arg.Args[i], out value) && value >= 0)
                                    {
                                        scrapChances.Add(i - 1, value);
                                    }
                                }
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                    {
                                        foreach (var entry1 in scrapChances)
                                        {
                                            ConfigData.ConsumeSettings consumeS;
                                            if (entry.Value.itemConsumedSettings.TryGetValue(entry1.Key, out consumeS))
                                            {
                                                consumeS.scrapChance = entry1.Value;
                                            }
                                            else
                                            {
                                                consumeS = new ConfigData.ConsumeSettings { scrapChance = entry1.Value };
                                                entry.Value.itemConsumedSettings.Add(entry1.Key, consumeS);
                                            }
                                        }
                                    }
                                }
                                Print(arg, $"Successfully modified the chance of scrap consumed when research fails in level {level} workbench items.");
                                SaveConfig();
                                return;

                            case "scrappercentage":
                                var scrapPercentages = new Dictionary<int, float>();
                                for (int i = 2; i < arg.Args.Length; i++)
                                {
                                    float value;
                                    if (float.TryParse(arg.Args[i], out value) && value >= 0)
                                    {
                                        scrapPercentages.Add(i - 1, value);
                                    }
                                }
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                    {
                                        foreach (var entry1 in scrapPercentages)
                                        {
                                            ConfigData.ConsumeSettings consumeS;
                                            if (entry.Value.itemConsumedSettings.TryGetValue(entry1.Key, out consumeS))
                                            {
                                                consumeS.scrapPercentage = entry1.Value;
                                            }
                                            else
                                            {
                                                consumeS = new ConfigData.ConsumeSettings { scrapPercentage = entry1.Value };
                                                entry.Value.itemConsumedSettings.Add(entry1.Key, consumeS);
                                            }
                                        }
                                    }
                                }
                                Print(arg, $"Successfully modified the percentage of scrap amount consumed when research fails in level {level} workbench items.");
                                SaveConfig();
                                return;

                            case "targetitemchance":
                                var targetItemChances = new Dictionary<int, float>();
                                for (int i = 2; i < arg.Args.Length; i++)
                                {
                                    float value;
                                    if (float.TryParse(arg.Args[i], out value) && value >= 0)
                                    {
                                        targetItemChances.Add(i - 1, value);
                                    }
                                }
                                foreach (var entry in configData.researchS)
                                {
                                    if (itemBlueprints.Contains(entry.Key))
                                    {
                                        foreach (var entry1 in targetItemChances)
                                        {
                                            ConfigData.ConsumeSettings consumeS;
                                            if (entry.Value.itemConsumedSettings.TryGetValue(entry1.Key, out consumeS))
                                            {
                                                consumeS.targetItemChance = entry1.Value;
                                            }
                                            else
                                            {
                                                consumeS = new ConfigData.ConsumeSettings { targetItemChance = entry1.Value };
                                                entry.Value.itemConsumedSettings.Add(entry1.Key, consumeS);
                                            }
                                        }
                                    }
                                }
                                Print(arg, $"Successfully modified the chance of target item consumed when research fails in level {level} workbench items.");
                                SaveConfig();
                                return;
                        }
                    }
                }
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Syntax error");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <cost> <(cost amount)> - Modify the research cost of level N workbench items. e.g.: 'br.lvl 3 cost 750'");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <time> <(research time)> - Modify the research time of level N workbench items. e.g.: 'br.lvl 3 time 2'");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <successChance> <(success chance)> - Modify the chance of research success of level N workbench items. e.g.: 'br.lvl 3 successChance 50'");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <scrapChance> [Chance of first failure] [Chance of second failure]... - Modify the chance of scrap consumed when research fails of level N workbench items. e.g.: 'br.lvl 3 scrapChance 50' or 'br.lvl 3 scrapChance 50 20'");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <scrapPercentage> [Percentage of first failure] [Percentage of second failure]... - Modify the percentage of scrap amount consumed when research fails of level N workbench items. e.g.: 'br.lvl 3 scrapPercentage 50' or 'br.lvl 3 scrapPercentage 50 20'");
            stringBuilder.AppendLine("br.lvl <WorkBenchLevel (0~3)> <targetItemChance> [Chance of first failure] [Chance of second failure]... - Modify the chance of target item consumed when research fails of level N workbench items. e.g.: 'br.lvl 3 targetItemChance 50' or 'br.lvl 3 targetItemChance 50 20'");
            Print(arg, stringBuilder.ToString());
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Research Resource (Item Short Name)")]
            public string researchResource = "scrap";

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            [JsonProperty(PropertyName = "Research UI Settings")]
            public UISettings uiS = new UISettings();

            [JsonProperty(PropertyName = "Research Settings")]
            public Dictionary<string, ResearchSettings> researchS = new Dictionary<string, ResearchSettings>();

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version = new VersionNumber(1, 1, 1);

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Use PopupNotifications")]
                public bool usePop;

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "<color=#00FFFF>[BetterResearching]</color>: ";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon;
            }

            public class UISettings
            {
                [JsonProperty(PropertyName = "Research - Box - Background Color")]
                public string researchBackgroundColor = "0.42 0.52 0.25 0.98";

                [JsonProperty(PropertyName = "Research - Text - Text Color")]
                public string researchTextColor = "0 0 0 1";

                [JsonProperty(PropertyName = "Research - Text - Text Size")]
                public int researchTextSize = 16;

                [JsonProperty(PropertyName = "Research - Text - Timeleft Text Size")]
                public int researchTimeleftTextSize = 26;

                [JsonProperty(PropertyName = "ResearchCost - Box - Background Color")]
                public string researchCostBackgroundColor = "0 0 0 0.98";

                [JsonProperty(PropertyName = "ResearchCost - Text - Text Color")]
                public string researchCostTextColor = "1 0 0 1";

                [JsonProperty(PropertyName = "ResearchCost - Text - Text Size")]
                public int researchCostTextSize = 50;
            }

            public class ResearchSettings
            {
                [JsonProperty(PropertyName = "Can Research")]
                public bool canResearch = true;

                [JsonProperty(PropertyName = "Display Name")]
                public string displayName = string.Empty;

                [JsonProperty(PropertyName = "Research Cost")]
                public int scrapAmount;

                [JsonProperty(PropertyName = "Research Time")]
                public float researchTime = 10f;

                [JsonProperty(PropertyName = "Research Success Chance")]
                public float successChance = 100f;

                [JsonProperty(PropertyName = "Item Consumed When Research Fails")]
                public Dictionary<int, ConsumeSettings> itemConsumedSettings = new Dictionary<int, ConsumeSettings>();
            }

            public class ConsumeSettings
            {
                [JsonProperty(PropertyName = "Scrap Consumed Chance")]
                public float scrapChance = 100f;

                [JsonProperty(PropertyName = "Percentage Of Scrap Amount Consumed")]
                public float scrapPercentage = 100f;

                [JsonProperty(PropertyName = "Target Item Consumed Chance")]
                public float targetItemChance = 0f;
            }
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

        protected override void SaveConfig() => Config.WriteObject(configData);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= new VersionNumber(1, 1, 1))
                {
                    if (configData.chatS.prefix == "[BetterResearching]: ")
                    {
                        configData.chatS.prefix = "<color=#00FFFF>[BetterResearching]</color>: ";
                    }
                }
                configData.version = Version;
            }
        }

        #endregion ConfigurationFile

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public readonly Dictionary<ulong, Dictionary<string, int>> playerResearchFailures = new Dictionary<ulong, Dictionary<string, int>>();
        }

        private void LoadData()
        {
            try
            {
                storedData = Interface.Oxide.DataFileSystem.ReadObject<StoredData>(Name);
            }
            catch
            {
                ClearData();
            }
        }

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        private void OnNewSave(string filename) => ClearData();

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message) => Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);

        private void Print(ConsoleSystem.Arg arg, string message)
        {
            var player = arg?.Player();
            if (player == null) Puts(message);
            else PrintToConsole(player, message);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ResearchFailed"] = "Researching <color=#FF8C00>{0}</color> failed",
                ["ResearchFailedScrap"] = "Researching <color=#FF8C00>{0}</color> failed，Consumed {1} Scrap",
                ["ResearchFailedTargetItem"] = "Researching <color=#FF8C00>{0}</color> failed，target item is consumed",
                ["ResearchSuccess"] = "Researched <color=#7FFF00>{0}</color> successfully ",
                ["ResearchCantAfford"] = "You don't have enough Scrap to research {0}. This item's research cost is {1} Scrap",
                ["IsBlocked"] = "<color=#1E90FF>{0}</color> is blocked, cannot research",
                ["CantResearch"] = "CANT RESEARCH",
                ["CanResearch"] = "BEGIN RESEARCH",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["ResearchFailed"] = "<color=#FF8C00>{0}</color> 研究失败",
                ["ResearchFailedAmount"] = "<color=#FF8C00>{0}</color> 研究失败，浪费了 {1} 个废料",
                ["ResearchFailedTargetItem"] = "<color=#FF8C00>{0}</color> 研究失败，目标物品已经破碎了",
                ["ResearchSuccess"] = "<color=#7FFF00>{0}</color> 研究成功",
                ["ResearchCantAfford"] = "您需要 {1} 个废料来研究 <color=#7FFF00>{0}</color>",
                ["IsBlocked"] = "服务器禁止您研究 <color=#1E90FF>{0}</color>",
                ["CantResearch"] = "无法研究",
                ["CanResearch"] = "开始研究",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}