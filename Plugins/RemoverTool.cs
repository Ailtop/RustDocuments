using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using VLB;

namespace Oxide.Plugins
{
    [Info("Remover Tool", "Reneb/Fuji/Arainrr", "4.3.25", ResourceId = 651)]
    [Description("Building and entity removal tool")]
    public class RemoverTool : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Friends, ServerRewards, Clans, Economics, ImageLibrary, BuildingOwners;
        private const string PERMISSION_ALL = "removertool.all";
        private const string PERMISSION_ADMIN = "removertool.admin";
        private const string PERMISSION_NORMAL = "removertool.normal";
        private const string PERMISSION_TARGET = "removertool.target";
        private const string PERMISSION_EXTERNAL = "removertool.external";
        private const string PERMISSION_OVERRIDE = "removertool.override";
        private const string PERMISSION_STRUCTURE = "removertool.structure";
        private const string PREFAB_ITEM_DROP = "assets/prefabs/misc/item drop/item_drop.prefab";

        private const int LAYER_ALL = 1 << 0 | 1 << 8 | 1 << 21;
        private const int LAYER_TARGET = ~(1 << 2 | 1 << 3 | 1 << 4 | 1 << 10 | 1 << 18 | 1 << 28 | 1 << 29);

        private static RemoverTool rt;
        private static BUTTON removeButton;
        private static RemoveMode removeMode;
        private bool removeOverride;
        private Coroutine removeAllCoroutine;
        private Coroutine removeStructureCoroutine;
        private Coroutine removeExternalCoroutine;
        private Coroutine removePlayerEntityCoroutine;

        private Hash<uint, float> entitySpawnedTimes;
        private Hash<ulong, float> lastBlockedPlayers;
        private Hash<uint, float> lastAttackedBuildings;
        private readonly Hash<ulong, float> cooldownTimes = new Hash<ulong, float>();

        //Reduce boxing
        private static readonly object True = true, False = false, Null = null;

        private enum RemoveMode
        {
            None,
            NoHeld,
            MeleeHit,
            SpecificTool
        }

        private enum RemoveType
        {
            None,
            All,
            Admin,
            Normal,
            External,
            Structure
        }

        private enum RemovePlayerEntityType
        {
            All,
            Cupboard,
            Building,
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            rt = this;
            LoadDefaultMessages();
            permission.RegisterPermission(PERMISSION_ALL, this);
            permission.RegisterPermission(PERMISSION_ADMIN, this);
            permission.RegisterPermission(PERMISSION_NORMAL, this);
            permission.RegisterPermission(PERMISSION_TARGET, this);
            permission.RegisterPermission(PERMISSION_OVERRIDE, this);
            permission.RegisterPermission(PERMISSION_EXTERNAL, this);
            permission.RegisterPermission(PERMISSION_STRUCTURE, this);
            foreach (var perm in configData.permS.Keys)
            {
                if (!permission.PermissionExists(perm, this))
                    permission.RegisterPermission(perm, this);
            }
            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdRemove));
            Unsubscribe(nameof(OnEntityDeath));
            Unsubscribe(nameof(OnHammerHit));
            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnEntityKill));
            Unsubscribe(nameof(OnPlayerAttack));
            Unsubscribe(nameof(OnActiveItemChanged));
            Unsubscribe(nameof(OnServerSave));
        }

        private void OnServerInitialized()
        {
            Initialize();
            UpdateConfig();
            removeMode = RemoveMode.None;
            if (configData.removerModeS.noHeldMode) removeMode = RemoveMode.NoHeld;
            if (configData.removerModeS.meleeHitMode) removeMode = RemoveMode.MeleeHit;
            if (configData.removerModeS.specificToolMode) removeMode = RemoveMode.SpecificTool;
            if (removeMode == RemoveMode.MeleeHit)
            {
                BaseMelee beseMelee;
                ItemDefinition itemDefinition;
                if (string.IsNullOrEmpty(configData.removerModeS.meleeHitItemShortname) ||
                    (itemDefinition = ItemManager.FindItemDefinition(configData.removerModeS.meleeHitItemShortname)) == null ||
                    (beseMelee = itemDefinition.GetComponent<ItemModEntity>()?.entityPrefab.Get()?.GetComponent<BaseMelee>()) == null)
                {
                    PrintError($"{configData.removerModeS.meleeHitItemShortname} is not an item shortname for a melee tool");
                    removeMode = RemoveMode.None;
                }
                else
                {
                    Subscribe(beseMelee is Hammer ? nameof(OnHammerHit) : nameof(OnPlayerAttack));
                }
            }

            if (configData.globalS.logToFile)
            {
                debugStringBuilder = new StringBuilder();
                Subscribe(nameof(OnServerSave));
            }
            if (configData.raidS.enabled)
            {
                lastBlockedPlayers = new Hash<ulong, float>();
                lastAttackedBuildings = new Hash<uint, float>();
                Subscribe(nameof(OnEntityDeath));
            }
            if (configData.globalS.entityTimeLimit)
            {
                entitySpawnedTimes = new Hash<uint, float>();
                Subscribe(nameof(OnEntitySpawned));
                Subscribe(nameof(OnEntityKill));
            }

            if (removeMode == RemoveMode.MeleeHit && configData.removerModeS.meleeHitEnableInHand ||
                removeMode == RemoveMode.SpecificTool && configData.removerModeS.specificToolEnableInHand)
            {
                Subscribe(nameof(OnActiveItemChanged));
            }

            if (!Enum.TryParse(configData.globalS.removeButton, true, out removeButton))
            {
                PrintError($"{configData.globalS.removeButton} is an invalid button. The remove button has been changed to 'FIRE_PRIMARY'.");
                removeButton = BUTTON.FIRE_PRIMARY;
                configData.globalS.removeButton = removeButton.ToString();
                SaveConfig();
            }
            if (ImageLibrary != null)
            {
                foreach (var image in configData.imageUrls)
                {
                    AddImageToLibrary(image.Value, image.Key);
                }
                if (configData.removerModeS.showCrosshair)
                {
                    AddImageToLibrary(configData.removerModeS.crosshairImageUrl, UINAME_CROSSHAIR);
                }
            }
        }

        private void Unload()
        {
            if (removeAllCoroutine != null) ServerMgr.Instance.StopCoroutine(removeAllCoroutine);
            if (removeStructureCoroutine != null) ServerMgr.Instance.StopCoroutine(removeStructureCoroutine);
            if (removeExternalCoroutine != null) ServerMgr.Instance.StopCoroutine(removeExternalCoroutine);
            if (removePlayerEntityCoroutine != null) ServerMgr.Instance.StopCoroutine(removePlayerEntityCoroutine);
            foreach (var player in BasePlayer.activePlayerList)
            {
                player.GetComponent<ToolRemover>()?.DisableTool();
            }

            SaveDebug();
            rt = null;
            configData = null;
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveDebug);

        private void OnEntityDeath(BuildingBlock buildingBlock, HitInfo info)
        {
            if (buildingBlock == null || info == null) return;
            var attacker = info.InitiatorPlayer;
            if (attacker != null && attacker.userID.IsSteamId() && HasAccess(attacker, buildingBlock)) return;
            BlockRemove(buildingBlock);
        }

        private void OnEntitySpawned(BaseEntity entity)
        {
            if (entity == null || entity.net == null) return;
            if (!CanEntityBeSaved(entity)) return;
            entitySpawnedTimes[entity.net.ID] = Time.realtimeSinceStartup;
        }

        private void OnEntityKill(BaseEntity entity)
        {
            if (entity == null || entity.net == null) return;
            entitySpawnedTimes.Remove(entity.net.ID);
        }

        private object OnPlayerAttack(BasePlayer player, HitInfo info) => OnHammerHit(player, info);

        private object OnHammerHit(BasePlayer player, HitInfo info)
        {
            if (player == null || info.HitEntity == null) return Null;
            var toolRemover = player.GetComponent<ToolRemover>();
            if (toolRemover == null) return Null;
            if (!IsMeleeTool(player)) return Null;
            toolRemover.hitEntity = info.HitEntity;
            return False;
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item newItem)
        {
            if (newItem == null) return;
            if (player == null || !player.userID.IsSteamId()) return;
            if (IsToolRemover(player)) return;
            if (removeMode == RemoveMode.MeleeHit && IsMeleeTool(newItem))
            {
                ToggleRemove(player, RemoveType.Normal);
                return;
            }
            if (removeMode == RemoveMode.SpecificTool && IsSpecificTool(newItem))
            {
                ToggleRemove(player, RemoveType.Normal);
                return;
            }
        }

        #endregion Oxide Hooks

        #region Initializing

        private readonly Dictionary<string, string> shorPrefabNameToDeployable = new Dictionary<string, string>();
        private readonly Dictionary<string, string> prefabNameToStructure = new Dictionary<string, string>();
        private readonly Dictionary<string, int> itemShortNameToItemID = new Dictionary<string, int>();
        private readonly HashSet<Construction> constructions = new HashSet<Construction>();

        private void Initialize()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (!itemShortNameToItemID.ContainsKey(itemDefinition.shortname))
                    itemShortNameToItemID.Add(itemDefinition.shortname, itemDefinition.itemid);
                var deployablePrefab = itemDefinition.GetComponent<ItemModDeployable>()?.entityPrefab?.resourcePath;
                if (string.IsNullOrEmpty(deployablePrefab)) continue;
                var shortPrefabName = GameManager.server.FindPrefab(deployablePrefab)?.GetComponent<BaseEntity>()?.ShortPrefabName;
                if (!string.IsNullOrEmpty(shortPrefabName) && !shorPrefabNameToDeployable.ContainsKey(shortPrefabName))
                    shorPrefabNameToDeployable.Add(shortPrefabName, itemDefinition.shortname);
            }
            foreach (var entry in PrefabAttribute.server.prefabs)
            {
                var construction = entry.Value.Find<Construction>().FirstOrDefault();
                if (construction != null && construction.deployable == null && !string.IsNullOrEmpty(construction.info.name.english))
                {
                    constructions.Add(construction);
                    if (!prefabNameToStructure.ContainsKey(construction.fullName))
                        prefabNameToStructure.Add(construction.fullName, construction.info.name.english);
                }
            }
        }

        #endregion Initializing

        #region Methods

        private static string GetRemoveTypeName(RemoveType removeType) => configData.removeTypeS[removeType].displayName;

        private static void DropItemContainer(ItemContainer itemContainer, Vector3 position, Quaternion rotation) => itemContainer?.Drop(PREFAB_ITEM_DROP, position, rotation);

        private static bool IsExternalWall(StabilityEntity stabilityEntity) => stabilityEntity.ShortPrefabName.Contains("external");

        private static bool IsRemovableEntity(BaseEntity entity) => rt.shorPrefabNameToDeployable.ContainsKey(entity.ShortPrefabName) || rt.prefabNameToStructure.ContainsKey(entity.PrefabName) || configData.removeS.entityS.ContainsKey(entity.ShortPrefabName);

        private static bool CanEntityBeDisplayed(BaseEntity entity, BasePlayer player)
        {
            if (entity == null) return false;
            var stash = entity as StashContainer;
            return stash == null || !stash.IsHidden() || stash.PlayerInRange(player);
        }

        private static bool CanEntityBeSaved(BaseEntity entity)
        {
            if (entity is BuildingBlock) return true;
            EntitySettings entitySettings;
            return configData.removeS.entityS.TryGetValue(entity.ShortPrefabName, out entitySettings) && entitySettings.enabled;
        }

        private static bool HasEntityEnabled(BaseEntity entity)
        {
            bool valid;
            var buildingBlock = entity as BuildingBlock;
            if (buildingBlock != null && configData.removeS.validConstruction.TryGetValue(buildingBlock.grade, out valid) && valid) return true;
            EntitySettings entitySettings;
            return configData.removeS.entityS.TryGetValue(entity.ShortPrefabName, out entitySettings) && entitySettings.enabled;
        }

        private static string GetEntityName(BaseEntity entity)
        {
            string entityName;
            if (rt.shorPrefabNameToDeployable.TryGetValue(entity.ShortPrefabName, out entityName)) return entityName;
            if (rt.prefabNameToStructure.TryGetValue(entity.PrefabName, out entityName)) return entityName;
            if (configData.removeS.entityS.ContainsKey(entity.ShortPrefabName)) return entity.ShortPrefabName;
            return string.Empty;
        }

        private static string GetEntityImage(string name)
        {
            if (configData.imageUrls.ContainsKey(name))
                return GetImageFromLibrary(name);
            if (rt.itemShortNameToItemID.ContainsKey(name))
                return GetImageFromLibrary(name);
            return string.Empty;
        }

        private static string GetItemImage(string shortname)
        {
            switch (shortname.ToLower())
            {
                case "economics": return GetImageFromLibrary("Economics");
                case "serverrewards": return GetImageFromLibrary("ServerRewards");
            }
            return GetEntityImage(shortname);
        }

        private static string GetDisplayName(string name)
        {
            var shortPrefabName = rt.shorPrefabNameToDeployable.FirstOrDefault(x => x.Value == name).Key;
            if (string.IsNullOrEmpty(shortPrefabName)) shortPrefabName = name;

            EntitySettings entitySettings;
            if (configData.removeS.entityS.TryGetValue(shortPrefabName, out entitySettings)) return entitySettings.displayName;
            BuildingBlocksSettings buildingBlockSettings;
            if (configData.removeS.buildingBlockS.TryGetValue(name, out buildingBlockSettings)) return buildingBlockSettings.displayName;
            if (configData.displayNames.TryGetValue(name, out shortPrefabName)) return shortPrefabName;
            var itemDefinition = ItemManager.FindItemDefinition(name);
            if (itemDefinition != null)
            {
                configData.displayNames.Add(name, itemDefinition.displayName.english);
                name = itemDefinition.displayName.english;
            }
            else configData.displayNames.Add(name, name);
            rt.SaveConfig();
            return name;
        }

        private static PermissionSettings GetPermissionS(BasePlayer player)
        {
            int priority = 0;
            PermissionSettings permissionSettings = null;
            foreach (var entry in configData.permS)
            {
                if (entry.Value.priority >= priority && rt.permission.UserHasPermission(player.UserIDString, entry.Key))
                {
                    priority = entry.Value.priority;
                    permissionSettings = entry.Value;
                }
            }
            return permissionSettings ?? new PermissionSettings();
        }

        private static Vector2 GetAnchor(string anchor)
        {
            var array = anchor.Split(' ');
            return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
        }

        private static bool AddImageToLibrary(string url, string shortname, ulong skin = 0) => (bool)rt.ImageLibrary.Call("AddImage", url, shortname.ToLower(), skin);

        private static string GetImageFromLibrary(string shortname, ulong skin = 0, bool returnUrl = false) => string.IsNullOrEmpty(shortname) ? string.Empty : (string)rt.ImageLibrary.Call("GetImage", shortname.ToLower(), skin, returnUrl);

        #endregion Methods

        #region UI

        private class UI
        {
            public static CuiElementContainer CreateElementContainer(string parent, string panelName, string backgroundColor, string anchorMin, string anchorMax, string offsetMin = "", string offsetMax = "", bool cursor = false)
            {
                return new CuiElementContainer()
                {
                    {
                        new CuiPanel
                        {
                            Image = { Color = backgroundColor },
                            RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax ,OffsetMin = offsetMin,OffsetMax = offsetMax},
                            CursorEnabled = cursor
                        }, parent, panelName
                    }
                };
            }

            public static void CreatePanel(ref CuiElementContainer container, string panelName, string backgroundColor, string anchorMin, string anchorMax, bool cursor = false)
            {
                container.Add(new CuiPanel
                {
                    Image = { Color = backgroundColor },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
                    CursorEnabled = cursor
                }, panelName, CuiHelper.GetGuid());
            }

            public static void CreateLabel(ref CuiElementContainer container, string panelName, string textColor, string text, int fontSize, string anchorMin, string anchorMax, TextAnchor align = TextAnchor.MiddleCenter, float fadeIn = 0f)
            {
                container.Add(new CuiLabel
                {
                    Text = { Color = textColor, FontSize = fontSize, Align = align, Text = text, FadeIn = fadeIn },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
                }, panelName, CuiHelper.GetGuid());
            }

            public static void CreateImage(ref CuiElementContainer container, string panelName, string image, string anchorMin, string anchorMax, string color = "1 1 1 1")
            {
                container.Add(new CuiElement
                {
                    Name = CuiHelper.GetGuid(),
                    Parent = panelName,
                    Components =
                    {
                        new CuiRawImageComponent { Sprite = "assets/content/textures/generic/fulltransparent.tga", Color = color, Png = image},
                        new CuiRectTransformComponent { AnchorMin = anchorMin, AnchorMax = anchorMax }
                    }
                });
            }
        }

        private const string UINAME_MAIN = "RemoverToolUI_Main";
        private const string UINAME_TIMELEFT = "RemoverToolUI_TimeLeft";
        private const string UINAME_ENTITY = "RemoverToolUI_Entity";
        private const string UINAME_PRICE = "RemoverToolUI_Price";
        private const string UINAME_REFUND = "RemoverToolUI_Refund";
        private const string UINAME_AUTH = "RemoverToolUI_Auth";
        private const string UINAME_CROSSHAIR = "RemoverToolUI_Crosshair";

        private static void CreateCrosshairUI(BasePlayer player)
        {
            if (rt.ImageLibrary == null) return;
            var image = GetImageFromLibrary(UINAME_CROSSHAIR);
            if (string.IsNullOrEmpty(image)) return;
            var container = UI.CreateElementContainer("Hud", UINAME_CROSSHAIR, "0 0 0 0", configData.removerModeS.crosshairAnchorMin, configData.removerModeS.crosshairAnchorMax, configData.removerModeS.crosshairOffsetMin, configData.removerModeS.crosshairOffsetMax);
            UI.CreateImage(ref container, UINAME_CROSSHAIR, image, "0 0", "1 1", configData.removerModeS.crosshairColor);
            CuiHelper.DestroyUi(player, UINAME_CROSSHAIR);
            CuiHelper.AddUi(player, container);
        }

        private static void CreateToolUI(BasePlayer player, RemoveType removeType)
        {
            var container = UI.CreateElementContainer("Hud", UINAME_MAIN, configData.uiS.removerToolBackgroundColor, configData.uiS.removerToolAnchorMin, configData.uiS.removerToolAnchorMax, configData.uiS.removerToolOffsetMin, configData.uiS.removerToolOffsetMax);
            UI.CreatePanel(ref container, UINAME_MAIN, configData.uiS.removeBackgroundColor, configData.uiS.removeAnchorMin, configData.uiS.removeAnchorMax);
            UI.CreateLabel(ref container, UINAME_MAIN, configData.uiS.removeTextColor, rt.Lang("RemoverToolType", player.UserIDString, GetRemoveTypeName(removeType)), configData.uiS.removeTextSize, configData.uiS.removeTextAnchorMin, configData.uiS.removeTextAnchorMax, TextAnchor.MiddleLeft);
            CuiHelper.DestroyUi(player, UINAME_MAIN);
            CuiHelper.AddUi(player, container);
        }

        private static void UpdateTimeLeftUI(BasePlayer player, RemoveType removeType, int timeLeft, int currentRemoved, int maxRemovable)
        {
            var container = UI.CreateElementContainer(UINAME_MAIN, UINAME_TIMELEFT, configData.uiS.timeLeftBackgroundColor, configData.uiS.timeLeftAnchorMin, configData.uiS.timeLeftAnchorMax);
            UI.CreateLabel(ref container, UINAME_TIMELEFT, configData.uiS.timeLeftTextColor, rt.Lang("TimeLeft", player.UserIDString, timeLeft, removeType == RemoveType.Normal || removeType == RemoveType.Admin ? maxRemovable == 0 ? $"{currentRemoved} / {rt.Lang("Unlimit", player.UserIDString)}" : $"{currentRemoved} / {maxRemovable}" : currentRemoved.ToString()), configData.uiS.timeLeftTextSize, configData.uiS.timeLeftTextAnchorMin, configData.uiS.timeLeftTextAnchorMax, TextAnchor.MiddleLeft);
            CuiHelper.DestroyUi(player, UINAME_TIMELEFT);
            CuiHelper.AddUi(player, container);
        }

        private static void UpdateEntityUI(BasePlayer player, BaseEntity targetEntity)
        {
            CuiHelper.DestroyUi(player, UINAME_ENTITY);
            if (!CanEntityBeDisplayed(targetEntity, player)) return;
            var container = UI.CreateElementContainer(UINAME_MAIN, UINAME_ENTITY, configData.uiS.entityBackgroundColor, configData.uiS.entityAnchorMin, configData.uiS.entityAnchorMax);
            string name;
            var entityName = GetEntityName(targetEntity);
            if (string.IsNullOrEmpty(entityName))
            {
                var target = targetEntity as BasePlayer;
                name = target != null ? $"{target.displayName} ({GetDisplayName(target.ShortPrefabName)})" : targetEntity.ShortPrefabName;
            }
            else name = GetDisplayName(entityName);
            UI.CreateLabel(ref container, UINAME_ENTITY, configData.uiS.entityTextColor, name, configData.uiS.entityTextSize, configData.uiS.entityTextAnchorMin, configData.uiS.entityTextAnchorMax, TextAnchor.MiddleLeft);
            if (configData.uiS.entityImageEnabled && !string.IsNullOrEmpty(entityName) && rt.ImageLibrary != null)
            {
                var image = GetEntityImage(entityName);
                if (!string.IsNullOrEmpty(image))
                    UI.CreateImage(ref container, UINAME_ENTITY, image, configData.uiS.entityImageAnchorMin, configData.uiS.entityImageAnchorMax);
            }
            CuiHelper.AddUi(player, container);
        }

        private static void UpdatePricesUI(BasePlayer player, bool usePrice, BaseEntity targetEntity)
        {
            CuiHelper.DestroyUi(player, UINAME_PRICE);
            if (!CanEntityBeDisplayed(targetEntity, player) || !HasEntityEnabled(targetEntity)) return;
            Dictionary<string, int> price = null;
            if (usePrice) price = rt.GetPrice(targetEntity);
            var container = UI.CreateElementContainer(UINAME_MAIN, UINAME_PRICE, configData.uiS.priceBackgroundColor, configData.uiS.priceAnchorMin, configData.uiS.priceAnchorMax);
            UI.CreateLabel(ref container, UINAME_PRICE, configData.uiS.priceTextColor, rt.Lang("Price", player.UserIDString), configData.uiS.priceTextSize, configData.uiS.priceTextAnchorMin, configData.uiS.priceTextAnchorMax, TextAnchor.MiddleLeft);
            if (price == null || price.Count == 0) UI.CreateLabel(ref container, UINAME_PRICE, configData.uiS.price2TextColor, rt.Lang("Free", player.UserIDString), configData.uiS.price2TextSize, configData.uiS.price2TextAnchorMin, configData.uiS.price2TextAnchorMax, TextAnchor.MiddleLeft);
            else
            {
                var anchorMin = configData.uiS.Price2TextAnchorMin;
                var anchorMax = configData.uiS.Price2TextAnchorMax;
                float x = (anchorMax.y - anchorMin.y) / price.Count;
                int textSize = configData.uiS.price2TextSize - price.Count;
                int i = 0;
                foreach (var p in price)
                {
                    UI.CreateLabel(ref container, UINAME_PRICE, configData.uiS.price2TextColor, $"{GetDisplayName(p.Key)} x{p.Value}", textSize, $"{anchorMin.x} {anchorMin.y + i * x}", $"{anchorMax.x} {anchorMin.y + (i + 1) * x}", TextAnchor.MiddleLeft);
                    if (configData.uiS.imageEnabled && rt.ImageLibrary != null)
                    {
                        var image = GetItemImage(p.Key);
                        if (!string.IsNullOrEmpty(image))
                            UI.CreateImage(ref container, UINAME_PRICE, image, $"{anchorMax.x - configData.uiS.rightDistance - x * configData.uiS.imageScale} {anchorMin.y + i * x}", $"{anchorMax.x - configData.uiS.rightDistance} {anchorMin.y + (i + 1) * x}");
                    }
                    i++;
                }
            }
            CuiHelper.AddUi(player, container);
        }

        private static void UpdateRefundUI(BasePlayer player, bool useRefund, BaseEntity targetEntity)
        {
            CuiHelper.DestroyUi(player, UINAME_REFUND);
            if (!CanEntityBeDisplayed(targetEntity, player) || !HasEntityEnabled(targetEntity)) return;
            Dictionary<string, int> refund = null;
            if (useRefund) refund = rt.GetRefund(targetEntity);
            var container = UI.CreateElementContainer(UINAME_MAIN, UINAME_REFUND, configData.uiS.refundBackgroundColor, configData.uiS.refundAnchorMin, configData.uiS.refundAnchorMax);
            UI.CreateLabel(ref container, UINAME_REFUND, configData.uiS.refundTextColor, rt.Lang("Refund", player.UserIDString), configData.uiS.refundTextSize, configData.uiS.refundTextAnchorMin, configData.uiS.refundTextAnchorMax, TextAnchor.MiddleLeft);

            if (refund == null || refund.Count == 0)
            {
                UI.CreateLabel(ref container, UINAME_REFUND, configData.uiS.refund2TextColor, rt.Lang("Nothing", player.UserIDString), configData.uiS.refund2TextSize, configData.uiS.refund2TextAnchorMin, configData.uiS.refund2TextAnchorMax, TextAnchor.MiddleLeft);
            }
            else
            {
                var anchorMin = configData.uiS.Refund2TextAnchorMin;
                var anchorMax = configData.uiS.Refund2TextAnchorMax;
                float x = (anchorMax.y - anchorMin.y) / refund.Count;
                int textSize = configData.uiS.refund2TextSize - refund.Count;
                int i = 0;
                foreach (var p in refund)
                {
                    UI.CreateLabel(ref container, UINAME_REFUND, configData.uiS.refund2TextColor, $"{GetDisplayName(p.Key)} x{p.Value}", textSize, $"{anchorMin.x} {anchorMin.y + i * x}", $"{anchorMax.x} {anchorMin.y + (i + 1) * x}", TextAnchor.MiddleLeft);
                    if (configData.uiS.imageEnabled && rt.ImageLibrary != null)
                    {
                        var image = GetItemImage(p.Key);
                        if (!string.IsNullOrEmpty(image))
                            UI.CreateImage(ref container, UINAME_REFUND, image, $"{anchorMax.x - configData.uiS.rightDistance - x * configData.uiS.imageScale} {anchorMin.y + i * x}", $"{anchorMax.x - configData.uiS.rightDistance} {anchorMin.y + (i + 1) * x}");
                    }
                    i++;
                }
            }
            CuiHelper.AddUi(player, container);
        }

        private static void UpdateAuthorizationUI(BasePlayer player, RemoveType removeType, BaseEntity targetEntity, bool shouldPay)
        {
            CuiHelper.DestroyUi(player, UINAME_AUTH);
            if (!CanEntityBeDisplayed(targetEntity, player)) return;
            string reason;
            string color = rt.CanRemoveEntity(player, removeType, targetEntity, shouldPay, out reason) ? configData.uiS.allowedBackgroundColor : configData.uiS.refusedBackgroundColor;
            var container = UI.CreateElementContainer(UINAME_MAIN, UINAME_AUTH, color, configData.uiS.authorizationsAnchorMin, configData.uiS.authorizationsAnchorMax);
            UI.CreateLabel(ref container, UINAME_AUTH, configData.uiS.authorizationsTextColor, reason, configData.uiS.authorizationsTextSize, configData.uiS.authorizationsTextAnchorMin, configData.uiS.authorizationsTextAnchorMax, TextAnchor.MiddleLeft);
            CuiHelper.AddUi(player, container);
        }

        private static void DestroyAllUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UINAME_CROSSHAIR);
            CuiHelper.DestroyUi(player, UINAME_MAIN);
        }

        #endregion UI

        #region ToolRemover Component

        #region Tool Helpers

        private static bool IsSpecificTool(BasePlayer player)
        {
            var heldItem = player.GetActiveItem();
            return IsSpecificTool(heldItem);
        }

        private static bool IsSpecificTool(Item heldItem)
        {
            if (heldItem != null && heldItem.info.shortname == configData.removerModeS.specificToolShortname)
            {
                if (configData.removerModeS.specificToolSkin < 0) return true;
                return heldItem.skin == (ulong)configData.removerModeS.specificToolSkin;
            }
            return false;
        }

        private static bool IsMeleeTool(BasePlayer player)
        {
            var heldItem = player.GetActiveItem();
            return IsMeleeTool(heldItem);
        }

        private static bool IsMeleeTool(Item heldItem)
        {
            if (heldItem != null && heldItem.info.shortname == configData.removerModeS.meleeHitItemShortname)
            {
                if (configData.removerModeS.meleeHitModeSkin < 0) return true;
                return heldItem.skin == (ulong)configData.removerModeS.meleeHitModeSkin;
            }
            return false;
        }

        #endregion Tool Helpers

        private class ToolRemover : FacepunchBehaviour
        {
            public BasePlayer player;
            public RemoveType removeType;
            public bool canOverride;
            public BaseEntity hitEntity;
            public int currentRemoved;

            private int timeLeft;
            private float distance;
            private float lastRemove;
            private float removeInterval;
            private bool pay;
            private bool refund;
            private int maxRemovable;
            private RaycastHit raycastHit;
            private BaseEntity targetEntity;
            private uint currentItemID;
            private int removeTime;
            private bool resetTime;
            private Item lastHeldItem;

            private bool checkHeldItem;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                currentItemID = player.svActiveItemID;
                checkHeldItem = removeMode == RemoveMode.MeleeHit && configData.removerModeS.meleeHitDisableInHand ||
                                removeMode == RemoveMode.SpecificTool && configData.removerModeS.specificToolDisableInHand;
                if (checkHeldItem)
                {
                    lastHeldItem = player.GetActiveItem();
                }
                if (removeMode == RemoveMode.NoHeld)
                {
                    UnEquip();
                }
            }

            public void Init(RemoveType type, int time, int max, float dis, float interval, bool p, bool r, bool reset, bool c)
            {
                canOverride = c;
                removeTime = timeLeft = time;
                removeType = type;
                distance = dis;
                resetTime = reset;
                removeInterval = Mathf.Max(0.2f, interval);
                if (removeType == RemoveType.Normal)
                {
                    maxRemovable = max;
                    pay = p && configData.removeS.priceEnabled;
                    refund = r && configData.removeS.refundEnabled;
                    rt.PrintDebug($"{player.displayName}({player.userID}) have Enabled the remover tool.");
                }
                else
                {
                    maxRemovable = currentRemoved = 0;
                    pay = refund = false;
                }
                DestroyAllUI(player);
                if (removeMode == RemoveMode.NoHeld && configData.removerModeS.showCrosshair)
                {
                    CreateCrosshairUI(player);
                }

                if (configData.uiS.enabled)
                {
                    CreateToolUI(player, removeType);
                }
                CancelInvoke(RemoveUpdate);
                InvokeRepeating(RemoveUpdate, 0f, 1f);
            }

            private void RemoveUpdate()
            {
                if (configData.uiS.enabled)
                {
                    GetTargetEntity();
                    UpdateTimeLeftUI(player, removeType, timeLeft, currentRemoved, maxRemovable);
                    UpdateEntityUI(player, targetEntity);
                    if (removeType == RemoveType.Normal)
                    {
                        if (configData.uiS.authorizationEnabled) UpdateAuthorizationUI(player, removeType, targetEntity, pay);
                        if (configData.uiS.priceEnabled) UpdatePricesUI(player, pay, targetEntity);
                        if (configData.uiS.refundEnabled) UpdateRefundUI(player, refund, targetEntity);
                    }
                }

                if (timeLeft-- <= 0)
                {
                    DisableTool();
                }
            }

            private void GetTargetEntity()
            {
                bool flag = Physics.Raycast(player.eyes.HeadRay(), out raycastHit, distance, LAYER_TARGET);
                targetEntity = flag ? raycastHit.GetEntity() : null;
            }

            private void FixedUpdate()
            {
                if (player == null || !player.IsConnected || !player.CanInteract())
                {
                    DisableTool();
                    return;
                }
                if (player.svActiveItemID != currentItemID)
                {
                    if (checkHeldItem)
                    {
                        var heldItem = player.GetActiveItem();
                        if (removeMode == RemoveMode.MeleeHit && IsMeleeTool(lastHeldItem) && !IsMeleeTool(heldItem) ||
                            removeMode == RemoveMode.SpecificTool && IsSpecificTool(lastHeldItem) && !IsSpecificTool(heldItem))
                        {
                            DisableTool();
                            return;
                        }
                        lastHeldItem = heldItem;
                    }
                    if (removeMode == RemoveMode.NoHeld)
                    {
                        if (player.svActiveItemID != 0)
                        {
                            if (configData.removerModeS.noHeldDisableInHand)
                            {
                                DisableTool();
                                return;
                            }
                            UnEquip();
                        }
                    }
                    currentItemID = player.svActiveItemID;
                }
                if (Time.realtimeSinceStartup - lastRemove >= removeInterval)
                {
                    if (removeMode == RemoveMode.MeleeHit)
                    {
                        if (hitEntity == null) return;
                        targetEntity = hitEntity;
                        hitEntity = null;
                    }
                    else
                    {
                        if (!player.serverInput.IsDown(removeButton)) return;
                        if (removeMode == RemoveMode.SpecificTool && !IsSpecificTool(player)) return;
                        GetTargetEntity();
                    }
                    if (rt.TryRemove(player, targetEntity, removeType, pay, refund))
                    {
                        if (resetTime) timeLeft = removeTime;
                        if (removeType == RemoveType.Normal || removeType == RemoveType.Admin)
                            currentRemoved++;
                        if (configData.globalS.startCooldownOnRemoved && removeType == RemoveType.Normal)
                        {
                            rt.cooldownTimes[player.userID] = Time.realtimeSinceStartup;
                        }
                    }
                    lastRemove = Time.realtimeSinceStartup;
                }
                if (removeType == RemoveType.Normal && maxRemovable > 0 && currentRemoved >= maxRemovable)
                {
                    rt.Print(player, rt.Lang("EntityLimit", player.UserIDString, maxRemovable));
                    DisableTool(false);
                };
            }

            private void UnEquip()
            {
                //player.lastReceivedTick.activeItem = 0;
                var activeItem = player.GetActiveItem();
                if (activeItem?.GetHeldEntity() is HeldEntity)
                {
                    var slot = activeItem.position;
                    activeItem.SetParent(null);
                    player.Invoke(() =>
                    {
                        if (activeItem == null || !activeItem.IsValid()) return;
                        if (player.inventory.containerBelt.GetSlot(slot) == null)
                        {
                            activeItem.position = slot;
                            activeItem.SetParent(player.inventory.containerBelt);
                        }
                        else player.GiveItem(activeItem);
                    }, 0.2f);
                }
            }

            public void DisableTool(bool showMessage = true)
            {
                if (showMessage)
                {
                    if (rt != null && player != null && player.IsConnected)
                    {
                        rt.Print(player, rt.Lang("ToolDisabled", player.UserIDString));
                    }
                }

                if (removeType == RemoveType.Normal)
                {
                    if (rt != null && player != null)
                    {
                        rt.PrintDebug($"{player.displayName}({player.userID}) have Disabled the remover tool.");
                    }
                }
                DestroyAllUI(player);
                Destroy(this);
            }

            private void OnDestroy()
            {
                if (rt != null && removeType == RemoveType.Normal)
                {
                    if (configData != null && !configData.globalS.startCooldownOnRemoved)
                    {
                        rt.cooldownTimes[player.userID] = Time.realtimeSinceStartup;
                    }
                }
            }
        }

        #endregion ToolRemover Component

        #region Pay

        private bool Pay(BasePlayer player, BaseEntity targetEntity)
        {
            var price = GetPrice(targetEntity);
            if (price == null) return true;
            var collect = Pool.GetList<Item>();
            try
            {
                foreach (var entry in price)
                {
                    if (entry.Value <= 0) continue;
                    int itemID;
                    if (itemShortNameToItemID.TryGetValue(entry.Key, out itemID))
                    {
                        player.inventory.Take(collect, itemID, entry.Value);
                        player.Command("note.inv", itemID, -entry.Value);
                    }
                    else if (!CheckOrPay(entry.Key, entry.Value, player.userID))
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                PrintError($"{player} couldn't pay to remove entity. Error Message: {e.Message}");
                return false;
            }
            finally
            {
                foreach (Item item in collect)
                {
                    item.Remove();
                }
                Pool.FreeList(ref collect);
            }
            return true;
        }

        private Dictionary<string, int> GetPrice(BaseEntity targetEntity)
        {
            var buildingBlock = targetEntity as BuildingBlock;
            if (buildingBlock != null)
            {
                var entityName = prefabNameToStructure[buildingBlock.PrefabName];
                BuildingBlocksSettings buildingBlockSettings;
                if (configData.removeS.buildingBlockS.TryGetValue(entityName, out buildingBlockSettings))
                {
                    BuildingGradeSettings buildingGradeSettings;
                    if (buildingBlockSettings.buildingGradeS.TryGetValue(buildingBlock.grade, out buildingGradeSettings))
                    {
                        if (buildingGradeSettings.priceDic != null)
                        {
                            return buildingGradeSettings.priceDic;
                        }
                        if (buildingGradeSettings.pricePercentage > 0f)
                        {
                            var currentGrade = buildingBlock.currentGrade;
                            if (currentGrade != null)
                            {
                                var price = new Dictionary<string, int>();
                                foreach (var itemAmount in currentGrade.costToBuild)
                                {
                                    var amount = Mathf.RoundToInt(itemAmount.amount * buildingGradeSettings.pricePercentage / 100);
                                    if (amount <= 0) continue;
                                    price.Add(itemAmount.itemDef.shortname, amount);
                                }

                                return price;
                            }
                        }
                        else if (buildingGradeSettings.pricePercentage < 0f)
                        {
                            var currentGrade = buildingBlock.currentGrade;
                            if (currentGrade != null)
                            {
                                return currentGrade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => Mathf.RoundToInt(y.amount));
                            }
                        }
                    }
                }
            }
            else
            {
                EntitySettings entitySettings;
                if (configData.removeS.entityS.TryGetValue(targetEntity.ShortPrefabName, out entitySettings))
                {
                    return entitySettings.price;
                }
            }
            return null;
        }

        private bool CanPay(BasePlayer player, BaseEntity targetEntity)
        {
            var price = GetPrice(targetEntity);
            if (price.Count <= 0) return true;
            foreach (var p in price)
            {
                if (p.Value <= 0) continue;
                int itemID;
                if (itemShortNameToItemID.TryGetValue(p.Key, out itemID))
                {
                    int c = player.inventory.GetAmount(itemID);
                    if (c < p.Value) return false;
                }
                else if (!CheckOrPay(p.Key, p.Value, player.userID, true)) return false;
            }
            return true;
        }

        private bool CheckOrPay(string key, int price, ulong playerID, bool check = false)
        {
            if (price <= 0) return true;
            switch (key.ToLower())
            {
                case "economics":
                    if (Economics == null) return false;
                    if (check)
                    {
                        var b = Economics.Call("Balance", playerID);
                        if (b == null) return false;
                        if ((double)b < price) return false;
                    }
                    else
                    {
                        var w = Economics.Call("Withdraw", playerID, (double)price);
                        if (w == null || !(bool)w) return false;
                    }
                    return true;

                case "serverrewards":
                    if (ServerRewards == null) return false;
                    if (check)
                    {
                        var c = ServerRewards.Call("CheckPoints", playerID);
                        if (c == null) return false;
                        if ((int)c < price) return false;
                    }
                    else
                    {
                        var t = ServerRewards.Call("TakePoints", playerID, price);
                        if (t == null || !(bool)t) return false;
                    }
                    return true;
            }
            return true;
        }

        #endregion Pay

        #region Refund

        private void GiveRefund(BasePlayer player, BaseEntity targetEntity)
        {
            var refund = GetRefund(targetEntity);
            if (refund == null) return;
            foreach (var entry in refund)
            {
                if (entry.Value <= 0) continue;
                int itemID; string shortname;
                shorPrefabNameToDeployable.TryGetValue(targetEntity.ShortPrefabName, out shortname);
                if (itemShortNameToItemID.TryGetValue(entry.Key, out itemID))
                {
                    var item = ItemManager.CreateByItemID(itemID, entry.Value, entry.Key == shortname ? targetEntity.skinID : 0);
                    player.GiveItem(item);
                }
                else
                {
                    switch (entry.Key.ToLower())
                    {
                        case "economics":
                            if (Economics == null) continue;
                            Economics.Call("Deposit", player.userID, (double)entry.Value);
                            continue;

                        case "serverrewards":
                            if (ServerRewards == null) continue;
                            ServerRewards.Call("AddPoints", player.userID, entry.Value);
                            continue;
                        default:
                            PrintError($"{player} didn't receive refund because {entry.Key} doesn't seem to be a valid item name");
                            continue;
                    }
                }
            }
        }

        private Dictionary<string, int> GetRefund(BaseEntity targetEntity)
        {
            var buildingBlock = targetEntity.GetComponent<BuildingBlock>();
            if (buildingBlock != null)
            {
                var entityName = prefabNameToStructure[buildingBlock.PrefabName];
                BuildingBlocksSettings buildingBlockSettings;
                if (configData.removeS.buildingBlockS.TryGetValue(entityName, out buildingBlockSettings))
                {
                    BuildingGradeSettings buildingGradeSettings;
                    if (buildingBlockSettings.buildingGradeS.TryGetValue(buildingBlock.grade, out buildingGradeSettings))
                    {
                        if (buildingGradeSettings.refundDic != null)
                        {
                            return buildingGradeSettings.refundDic;
                        }
                        if (buildingGradeSettings.refundPercentage > 0f)
                        {
                            var currentGrade = buildingBlock.currentGrade;
                            if (currentGrade != null)
                            {
                                var refund = new Dictionary<string, int>();
                                foreach (var itemAmount in currentGrade.costToBuild)
                                {
                                    var amount = Mathf.RoundToInt(itemAmount.amount * buildingGradeSettings.refundPercentage / 100);
                                    if (amount <= 0) continue;
                                    refund.Add(itemAmount.itemDef.shortname, amount);
                                }
                                return refund;
                            }
                        }
                        else if (buildingGradeSettings.refundPercentage < 0f)
                        {
                            var currentGrade = buildingBlock.currentGrade;
                            if (currentGrade != null)
                            {
                                return currentGrade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => Mathf.RoundToInt(y.amount));
                            }
                        }
                    }
                }
            }
            else
            {
                EntitySettings entitySettings;
                if (configData.removeS.entityS.TryGetValue(targetEntity.ShortPrefabName, out entitySettings))
                {
                    if (configData.removeS.refundSlot)
                    {
                        var slots = GetSlots(targetEntity);
                        if (slots.Any())
                        {
                            var refund = new Dictionary<string, int>(entitySettings.refund);
                            foreach (var slotName in slots)
                            {
                                if (!refund.ContainsKey(slotName)) refund.Add(slotName, 0);
                                refund[slotName]++;
                            }
                            return refund;
                        }
                    }
                    return entitySettings.refund;
                }
            }
            return null;
        }

        private IEnumerable<string> GetSlots(BaseEntity targetEntity)
        {
            foreach (BaseEntity.Slot slot in Enum.GetValues(typeof(BaseEntity.Slot)))
            {
                if (targetEntity.HasSlot(slot))
                {
                    var entity = targetEntity.GetSlot(slot);
                    if (entity != null)
                    {
                        string slotName;
                        if (shorPrefabNameToDeployable.TryGetValue(entity.ShortPrefabName, out slotName))
                        {
                            yield return slotName;
                        }
                    }
                }
            }
        }

        #endregion Refund

        #region RaidBlocker

        private void BlockRemove(BuildingBlock buildingBlock)
        {
            if (configData.raidS.blockBuildingID)
            {
                var buildingID = buildingBlock.buildingID;
                lastAttackedBuildings[buildingID] = Time.realtimeSinceStartup;
            }
            if (configData.raidS.blockPlayers)
            {
                var players = Pool.GetList<BasePlayer>();
                Vis.Entities(buildingBlock.transform.position, configData.raidS.blockRadius, players, Rust.Layers.Mask.Player_Server);
                foreach (var player in players)
                {
                    if (player.userID.IsSteamId())
                        lastBlockedPlayers[player.userID] = Time.realtimeSinceStartup;
                }
                Pool.FreeList(ref players);
            }
        }

        private bool IsRaidBlocked(BasePlayer player, BaseEntity targetEntity, out float timeLeft)
        {
            if (configData.raidS.blockBuildingID)
            {
                var buildingBlock = targetEntity as BuildingBlock;
                if (buildingBlock != null)
                {
                    float blockTime;
                    if (lastAttackedBuildings.TryGetValue(buildingBlock.buildingID, out blockTime))
                    {
                        timeLeft = configData.raidS.blockTime - (Time.realtimeSinceStartup - blockTime);
                        if (timeLeft > 0) return true;
                    }
                }
            }
            if (configData.raidS.blockPlayers)
            {
                float blockTime;
                if (lastBlockedPlayers.TryGetValue(player.userID, out blockTime))
                {
                    timeLeft = configData.raidS.blockTime - (Time.realtimeSinceStartup - blockTime);
                    if (timeLeft > 0) return true;
                }
            }
            timeLeft = 0;
            return false;
        }

        #endregion RaidBlocker

        #region TryRemove

        private bool TryRemove(BasePlayer player, BaseEntity targetEntity, RemoveType removeType, bool shouldPay, bool shouldRefund)
        {
            if (!CanEntityBeDisplayed(targetEntity, player))
            {
                Print(player, Lang("NotFoundOrFar", player.UserIDString));
                return false;
            }
            if (removeType == RemoveType.Admin)
            {
                var target = targetEntity as BasePlayer;
                if (target != null)
                {
                    if (target.userID.IsSteamId() && target.IsConnected)
                    {
                        target.Kick("From RemoverTool Plugin");
                        return true;
                    }
                }
                DoRemove(targetEntity, configData.removeTypeS[RemoveType.Admin].gibs ? BaseNetworkable.DestroyMode.Gib : BaseNetworkable.DestroyMode.None);
                return true;
            }

            string reason;
            if (!CanRemoveEntity(player, removeType, targetEntity, shouldPay, out reason))
            {
                Print(player, reason);
                return false;
            }
            switch (removeType)
            {
                case RemoveType.All:
                    {
                        if (removeAllCoroutine != null)
                        {
                            Print(player, Lang("AlreadyRemoveAll", player.UserIDString));
                            return false;
                        }
                        removeAllCoroutine = ServerMgr.Instance.StartCoroutine(RemoveAll(targetEntity, player));
                        Print(player, Lang("StartRemoveAll", player.UserIDString));
                        return true;
                    }
                case RemoveType.External:
                    {
                        var stabilityEntity = targetEntity as StabilityEntity;
                        if (stabilityEntity == null || !IsExternalWall(stabilityEntity))
                        {
                            Print(player, Lang("NotExternalWall", player.UserIDString));
                            return false;
                        }
                        if (removeExternalCoroutine != null)
                        {
                            Print(player, Lang("AlreadyRemoveExternal", player.UserIDString));
                            return false;
                        }
                        removeExternalCoroutine = ServerMgr.Instance.StartCoroutine(RemoveExternal(stabilityEntity, player));
                        Print(player, Lang("StartRemoveExternal", player.UserIDString));
                        return true;
                    }
                case RemoveType.Structure:
                    {
                        var decayEntity = targetEntity as DecayEntity;
                        if (decayEntity == null)
                        {
                            Print(player, Lang("NotStructure", player.UserIDString));
                            return false;
                        }
                        if (removeStructureCoroutine != null)
                        {
                            Print(player, Lang("AlreadyRemoveStructure", player.UserIDString));
                            return false;
                        }
                        removeStructureCoroutine = ServerMgr.Instance.StartCoroutine(RemoveStructure(decayEntity, player));
                        Print(player, Lang("StartRemoveStructure", player.UserIDString));
                        return true;
                    }
            }

            var storageContainer = targetEntity as StorageContainer;
            if (storageContainer != null && storageContainer.inventory?.itemList?.Count > 0)
            {
                if (configData.containerS.dropContainerStorage || configData.containerS.dropItemsStorage)
                {
                    if (Interface.CallHook("OnDropContainerEntity", storageContainer) == null)
                    {
                        if (configData.containerS.dropContainerStorage)
                        {
                            DropItemContainer(storageContainer.inventory, storageContainer.GetDropPosition(),
                                storageContainer.transform.rotation);
                        }
                        else if (configData.containerS.dropItemsStorage)
                        {
                            storageContainer.DropItems();
                            //DropUtil.DropItems(storageContainer.inventory, storageContainer.GetDropPosition());
                        }
                    }
                }
            }
            else
            {
                var containerIoEntity = targetEntity as ContainerIOEntity;
                if (containerIoEntity != null && containerIoEntity.inventory?.itemList?.Count > 0)
                {
                    if (configData.containerS.dropContainerIoEntity || configData.containerS.dropItemsIoEntity)
                    {
                        if (Interface.CallHook("OnDropContainerEntity", containerIoEntity) == null)
                        {
                            if (configData.containerS.dropContainerIoEntity)
                            {
                                DropItemContainer(containerIoEntity.inventory, containerIoEntity.GetDropPosition(), containerIoEntity.transform.rotation);
                            }
                            else if (configData.containerS.dropItemsIoEntity)
                            {
                                containerIoEntity.DropItems();
                                //DropUtil.DropItems(containerIoEntity.inventory, containerIoEntity.GetDropPosition());
                            }
                        }
                    }
                }
            }
            if (shouldPay)
            {
                bool flag = Pay(player, targetEntity);
                if (!flag)
                {
                    Print(player, Lang("CantPay", player.UserIDString));
                    return false;
                }
            }
            if (shouldRefund) GiveRefund(player, targetEntity);
            DoNormalRemove(player, targetEntity, configData.removeTypeS[RemoveType.Normal].gibs);
            return true;
        }

        private bool CanRemoveEntity(BasePlayer player, RemoveType removeType, BaseEntity targetEntity, bool shouldPay, out string reason)
        {
            if (targetEntity.IsDestroyed || !IsRemovableEntity(targetEntity) ||
                (removeType == RemoveType.Normal && ((targetEntity as BaseCombatEntity)?.IsDead() ?? false)))
            {
                reason = Lang("InvalidEntity", player.UserIDString);
                return false;
            }
            if (removeType != RemoveType.Normal)
            {
                reason = string.Empty;
                return true;
            }
            if (!HasEntityEnabled(targetEntity))
            {
                reason = Lang("EntityDisabled", player.UserIDString);
                return false;
            }
            var result = Interface.CallHook("canRemove", player, targetEntity);
            if (result != null)
            {
                reason = result is string ? (string)result : Lang("BeBlocked", player.UserIDString);
                return false;
            }
            if (!configData.damagedEntityS.enabled && IsDamagedEntity(targetEntity))
            {
                reason = Lang("DamagedEntity", player.UserIDString);
                return false;
            }
            float timeLeft;
            if (configData.raidS.enabled && IsRaidBlocked(player, targetEntity, out timeLeft))
            {
                reason = Lang("RaidBlocked", player.UserIDString, Math.Ceiling(timeLeft));
                return false;
            }
            if (configData.globalS.entityTimeLimit && IsEntityTimeLimit(targetEntity))
            {
                reason = Lang("EntityTimeLimit", player.UserIDString, configData.globalS.limitTime);
                return false;
            }
            if (shouldPay && !CanPay(player, targetEntity))
            {
                reason = Lang("NotEnoughCost", player.UserIDString);
                return false;
            }
            if (!configData.containerS.removeNotEmptyStorage && targetEntity is StorageContainer)
            {
                if (((StorageContainer)targetEntity).inventory?.itemList?.Count > 0)
                {
                    reason = Lang("StorageNotEmpty", player.UserIDString);
                    return false;
                }
            }
            if (!configData.containerS.removeNotEmptyIoEntity && targetEntity is ContainerIOEntity)
            {
                if (((ContainerIOEntity)targetEntity).inventory?.itemList?.Count > 0)
                {
                    reason = Lang("StorageNotEmpty", player.UserIDString);
                    return false;
                }
            }
            if (!HasAccess(player, targetEntity))
            {
                reason = Lang("NotRemoveAccess", player.UserIDString);
                return false;
            }
            if (configData.globalS.checkStash && HasStashUnderFoundation(targetEntity as BuildingBlock))//Prevent not access players from knowing that there has a stash
            {
                reason = Lang("HasStash", player.UserIDString);
                return false;
            }
            reason = Lang("CanRemove", player.UserIDString);
            return true;
        }

        private bool HasAccess(BasePlayer player, BaseEntity targetEntity)
        {
            if (configData.globalS.useBuildingOwners && BuildingOwners != null)
            {
                var buildingBlock = targetEntity as BuildingBlock;
                if (buildingBlock != null)
                {
                    var result = BuildingOwners?.Call("FindBlockData", buildingBlock) as string;
                    if (result != null)
                    {
                        ulong ownerID = ulong.Parse(result);
                        if (AreFriends(ownerID, player.userID))
                        {
                            return true;
                        }
                    }
                }
            }
            //var excludeTwigs = configData.globalS.excludeTwigs && (targetEntity as BuildingBlock)?.grade == BuildingGrade.Enum.Twigs;
            if (configData.globalS.useEntityOwners)
            {
                if (AreFriends(targetEntity.OwnerID, player.userID))
                {
                    if (!configData.globalS.useToolCupboards)
                    {
                        return true;
                    }
                    if (HasTotalAccess(player, targetEntity))
                    {
                        return true;
                    }
                }

                return false;
            }
            if (configData.globalS.useToolCupboards)
            {
                if (HasTotalAccess(player, targetEntity))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool HasTotalAccess(BasePlayer player, BaseEntity targetEntity)
        {
            if (player.IsBuildingBlocked(targetEntity.WorldSpaceBounds()))
            {
                return false;
            }
            if (configData.globalS.useBuildingLocks && !CanOpenAllLocks(player, targetEntity))
            {
                //reason = Lang("Can'tOpenAllLocks", player.UserIDString);
                return false;
            }
            return true;
        }

        private static bool CanOpenAllLocks(BasePlayer player, BaseEntity targetEntity)
        {
            var decayEntities = Pool.GetList<DecayEntity>();
            var building = targetEntity.GetBuildingPrivilege()?.GetBuilding() ?? (targetEntity as DecayEntity)?.GetBuilding();
            if (building != null)
            {
                decayEntities.AddRange(building.decayEntities);
            }
            /*else//An entity placed outside
            {
                Vis.Entities(targetEntity.transform.position, 9f, decayEntities, Layers.Mask.Construction | Layers.Mask.Deployed);
            }*/
            foreach (var decayEntity in decayEntities)
            {
                if ((decayEntity is Door || decayEntity is BoxStorage) && decayEntity.OwnerID.IsSteamId())
                {
                    var lockEntity = decayEntity.GetSlot(BaseEntity.Slot.Lock) as BaseLock;
                    if (lockEntity != null && !OnTryToOpen(player, lockEntity))
                    {
                        Pool.FreeList(ref decayEntities);
                        return false;
                    }
                }
            }
            Pool.FreeList(ref decayEntities);
            return true;
        }

        private static bool OnTryToOpen(BasePlayer player, BaseLock baseLock)
        {
            var codeLock = baseLock as CodeLock;
            if (codeLock != null)
            {
                var obj = Interface.CallHook("CanUseLockedEntity", player, codeLock);
                if (obj is bool)
                {
                    return (bool)obj;
                }
                if (!codeLock.IsLocked())
                {
                    return true;
                }
                if (codeLock.whitelistPlayers.Contains(player.userID) || codeLock.guestPlayers.Contains(player.userID))
                {
                    return true;
                }
                return false;
            }
            var keyLock = baseLock as KeyLock;
            if (keyLock != null)
            {
                return keyLock.OnTryToOpen(player);
            }

            return false;
        }

        private static bool HasStashUnderFoundation(BuildingBlock buildingBlock)
        {
            if (buildingBlock == null) return false;
            if (buildingBlock.ShortPrefabName.Contains("foundation"))
            {
                return GamePhysics.CheckOBB<StashContainer>(buildingBlock.WorldSpaceBounds());
            }
            return false;
        }

        private static bool IsDamagedEntity(BaseEntity entity)
        {
            if (configData.damagedEntityS.excludeBuildingBlocks && (entity is BuildingBlock || entity is SimpleBuildingBlock)) return false;
            var baseCombatEntity = entity as BaseCombatEntity;
            if (baseCombatEntity == null || !baseCombatEntity.repair.enabled) return false;
            if (!(entity is BuildingBlock) && (baseCombatEntity.repair.itemTarget == null || baseCombatEntity.repair.itemTarget.Blueprint == null))//Quarry
                return false;
            if (baseCombatEntity.Health() / baseCombatEntity.MaxHealth() * 100f >= configData.damagedEntityS.percentage) return false;
            return true;
        }

        private static bool IsEntityTimeLimit(BaseEntity entity)
        {
            if (entity.net == null) return true;
            float spawnedTime;
            if (rt.entitySpawnedTimes.TryGetValue(entity.net.ID, out spawnedTime))
                return Time.realtimeSinceStartup - spawnedTime > configData.globalS.limitTime;
            return true;
        }

        #region AreFriends

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (!playerID.IsSteamId()) return false;
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

        #endregion TryRemove

        #region RemoveEntity

        private IEnumerator RemoveAll(BaseEntity sourceEntity, BasePlayer player)
        {
            var removeList = Pool.Get<HashSet<BaseEntity>>();
            yield return GetNearbyEntities(sourceEntity, removeList, LAYER_ALL);
            yield return ProcessContainer(removeList);
            yield return DelayRemove(removeList, player, RemoveType.All);
            Pool.Free(ref removeList);
            removeAllCoroutine = null;
        }

        private IEnumerator RemoveExternal(StabilityEntity sourceEntity, BasePlayer player)
        {
            var removeList = Pool.Get<HashSet<StabilityEntity>>();
            yield return GetNearbyEntities(sourceEntity, removeList, Rust.Layers.Mask.Construction, IsExternalWall);
            yield return DelayRemove(removeList, player, RemoveType.External);
            Pool.Free(ref removeList);
            removeExternalCoroutine = null;
        }

        private IEnumerator RemoveStructure(DecayEntity sourceEntity, BasePlayer player)
        {
            var removeList = Pool.Get<HashSet<BaseEntity>>();
            yield return ProcessBuilding(sourceEntity, removeList);
            yield return DelayRemove(removeList, player, RemoveType.Structure);
            Pool.Free(ref removeList);
            removeStructureCoroutine = null;
        }

        private IEnumerator RemovePlayerEntity(ConsoleSystem.Arg arg, ulong targetID, RemovePlayerEntityType removePlayerEntityType)
        {
            int current = 0;
            var removeList = Pool.Get<HashSet<BaseEntity>>();
            switch (removePlayerEntityType)
            {
                case RemovePlayerEntityType.All:
                case RemovePlayerEntityType.Building:
                    bool onlyBuilding = removePlayerEntityType == RemovePlayerEntityType.Building;
                    foreach (var serverEntity in BaseNetworkable.serverEntities)
                    {
                        if (++current % 500 == 0) yield return CoroutineEx.waitForEndOfFrame;
                        var entity = serverEntity as BaseEntity;
                        if (entity == null || entity.OwnerID != targetID) continue;
                        if (!onlyBuilding || entity is BuildingBlock)
                        {
                            removeList.Add(entity);
                        }
                    }
                    foreach (var player in BasePlayer.allPlayerList)
                    {
                        if (player.userID == targetID)
                        {
                            if (player.IsConnected) player.Kick("From RemoverTool Plugin");
                            removeList.Add(player);
                            break;
                        }
                    }
                    break;

                case RemovePlayerEntityType.Cupboard:
                    foreach (var serverEntity in BaseNetworkable.serverEntities)
                    {
                        if (++current % 500 == 0) yield return CoroutineEx.waitForEndOfFrame;
                        var entity = serverEntity as BuildingPrivlidge;
                        if (entity == null || entity.OwnerID != targetID) continue;
                        yield return ProcessBuilding(entity, removeList);
                    }
                    break;
            }
            int removed = removeList.Count(x => x != null && !x.IsDestroyed);
            yield return DelayRemove(removeList);
            Pool.Free(ref removeList);
            Print(arg, $"You have successfully removed {removed} entities of player {targetID}.");
            removePlayerEntityCoroutine = null;
        }

        private IEnumerator DelayRemove(IEnumerable<BaseEntity> entities, BasePlayer player = null, RemoveType removeType = RemoveType.None)
        {
            int removed = 0;
            var destroyMode = removeType == RemoveType.None ? BaseNetworkable.DestroyMode.None : configData.removeTypeS[removeType].gibs ? BaseNetworkable.DestroyMode.Gib : BaseNetworkable.DestroyMode.None;
            foreach (var entity in entities)
            {
                if (DoRemove(entity, destroyMode) && ++removed % configData.globalS.removePerFrame == 0)
                    yield return CoroutineEx.waitForEndOfFrame;
            }
            if (removeType == RemoveType.None) yield break;
            var toolRemover = player?.GetComponent<ToolRemover>();
            switch (removeType)
            {
                case RemoveType.All:
                    if (toolRemover != null && toolRemover.removeType == removeType) toolRemover.currentRemoved += removed;
                    if (player != null) Print(player, Lang("CompletedRemoveAll", player.UserIDString, removed));
                    yield break;

                case RemoveType.Structure:
                    if (toolRemover != null && toolRemover.removeType == removeType) toolRemover.currentRemoved += removed;
                    if (player != null) Print(player, Lang("CompletedRemoveStructure", player.UserIDString, removed));
                    yield break;

                case RemoveType.External:
                    if (toolRemover != null && toolRemover.removeType == removeType) toolRemover.currentRemoved += removed;
                    if (player != null) Print(player, Lang("CompletedRemoveExternal", player.UserIDString, removed));
                    yield break;
            }
        }

        #region RemoveEntity Helpers

        private static IEnumerator GetNearbyEntities<T>(T sourceEntity, HashSet<T> removeList, int layers, Func<T, bool> filter = null) where T : BaseEntity
        {
            int current = 0;
            var checkFrom = Pool.Get<Queue<Vector3>>();
            var nearbyEntities = Pool.GetList<T>();
            checkFrom.Enqueue(sourceEntity.transform.position);
            while (checkFrom.Count > 0)
            {
                nearbyEntities.Clear();
                var position = checkFrom.Dequeue();
                Vis.Entities(position, 3f, nearbyEntities, layers);
                for (var i = 0; i < nearbyEntities.Count; i++)
                {
                    var entity = nearbyEntities[i];
                    if (filter != null && !filter(entity)) continue;
                    if (!removeList.Add(entity)) continue;
                    checkFrom.Enqueue(entity.transform.position);
                }
                if (++current % configData.globalS.removePerFrame == 0) yield return CoroutineEx.waitForEndOfFrame;
            }
            Pool.Free(ref checkFrom);
            Pool.FreeList(ref nearbyEntities);
        }

        private static IEnumerator ProcessContainer(HashSet<BaseEntity> removeList)
        {
            foreach (var entity in removeList)
            {
                var storageContainer = entity as StorageContainer;
                if (storageContainer != null && storageContainer.inventory?.itemList?.Count > 0)
                {
                    if (configData.globalS.noItemContainerDrop) storageContainer.inventory.Clear();
                    else DropItemContainer(storageContainer.inventory, storageContainer.GetDropPosition(), storageContainer.transform.rotation);
                    continue;
                }
                var containerIoEntity = entity as ContainerIOEntity;
                if (containerIoEntity != null && containerIoEntity.inventory?.itemList?.Count > 0)
                {
                    if (configData.globalS.noItemContainerDrop) containerIoEntity.inventory.Clear();
                    else DropItemContainer(containerIoEntity.inventory, containerIoEntity.GetDropPosition(), containerIoEntity.transform.rotation);
                }
            }
            if (configData.globalS.noItemContainerDrop) ItemManager.DoRemoves();
            yield break;
        }

        private static IEnumerator ProcessBuilding(DecayEntity sourceEntity, HashSet<BaseEntity> removeList)
        {
            var building = sourceEntity.GetBuilding();
            if (building != null)
            {
                foreach (var entity in building.decayEntities)
                {
                    if (!removeList.Add(entity)) continue;
                    var storageContainer = entity as StorageContainer;
                    if (storageContainer != null && storageContainer.inventory?.itemList?.Count > 0)
                    {
                        if (configData.globalS.noItemContainerDrop) storageContainer.inventory.Clear();
                        else DropItemContainer(storageContainer.inventory, storageContainer.GetDropPosition(), storageContainer.transform.rotation);
                    }
                }
            }
            else removeList.Add(sourceEntity);
            if (configData.globalS.noItemContainerDrop) ItemManager.DoRemoves();
            yield break;
        }

        private static bool DoRemove(BaseEntity entity, BaseNetworkable.DestroyMode destroyMode)
        {
            if (entity != null && !entity.IsDestroyed)
            {
                entity.Kill(destroyMode);
                return true;
            }
            return false;
        }

        private static void DoNormalRemove(BasePlayer player, BaseEntity entity, bool gibs = true)
        {
            if (entity != null && !entity.IsDestroyed)
            {
                rt.PrintDebug($"{player.displayName}({player.userID}) has removed {entity.ShortPrefabName}({entity.OwnerID})");
                Interface.CallHook("OnNormalRemovedEntity", player, entity);
                entity.Kill(gibs ? BaseNetworkable.DestroyMode.Gib : BaseNetworkable.DestroyMode.None);
            }
        }

        #endregion RemoveEntity Helpers

        #endregion RemoveEntity

        #region API

        private bool IsToolRemover(BasePlayer player) => player?.GetComponent<ToolRemover>() != null;

        private string GetPlayerRemoveType(BasePlayer player) => player?.GetComponent<ToolRemover>()?.removeType.ToString();

        #endregion API

        #region Commands

        private void CmdRemove(BasePlayer player, string command, string[] args)
        {
            if (args == null || args.Length == 0)
            {
                var sourceRemover = player.GetComponent<ToolRemover>();
                if (sourceRemover != null)
                {
                    sourceRemover.DisableTool();
                    return;
                }
            }
            if (removeOverride && !permission.UserHasPermission(player.UserIDString, PERMISSION_OVERRIDE))
            {
                Print(player, Lang("CurrentlyDisabled", player.UserIDString));
                return;
            }
            RemoveType removeType = RemoveType.Normal;
            int time = configData.removeTypeS[removeType].defaultTime;
            if (args != null && args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "n":
                    case "normal":
                        break;

                    case "a":
                    case "admin":
                        removeType = RemoveType.Admin;
                        time = configData.removeTypeS[removeType].defaultTime;
                        if (!permission.UserHasPermission(player.UserIDString, PERMISSION_ADMIN))
                        {
                            Print(player, Lang("NotAllowed", player.UserIDString, PERMISSION_ADMIN));
                            return;
                        }
                        break;

                    case "all":
                        removeType = RemoveType.All;
                        time = configData.removeTypeS[removeType].defaultTime;
                        if (!permission.UserHasPermission(player.UserIDString, PERMISSION_ALL))
                        {
                            Print(player, Lang("NotAllowed", player.UserIDString, PERMISSION_ALL));
                            return;
                        }
                        break;

                    case "s":
                    case "structure":
                        removeType = RemoveType.Structure;
                        time = configData.removeTypeS[removeType].defaultTime;
                        if (!permission.UserHasPermission(player.UserIDString, PERMISSION_STRUCTURE))
                        {
                            Print(player, Lang("NotAllowed", player.UserIDString, PERMISSION_STRUCTURE));
                            return;
                        }
                        break;

                    case "e":
                    case "external":
                        removeType = RemoveType.External;
                        time = configData.removeTypeS[removeType].defaultTime;
                        if (!permission.UserHasPermission(player.UserIDString, PERMISSION_EXTERNAL))
                        {
                            Print(player, Lang("NotAllowed", player.UserIDString, PERMISSION_EXTERNAL));
                            return;
                        }
                        break;

                    case "h":
                    case "help":
                        StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                        stringBuilder.AppendLine(Lang("Syntax", player.UserIDString, configData.chatS.command, GetRemoveTypeName(RemoveType.Normal)));
                        stringBuilder.AppendLine(Lang("Syntax1", player.UserIDString, configData.chatS.command, GetRemoveTypeName(RemoveType.Admin)));
                        stringBuilder.AppendLine(Lang("Syntax2", player.UserIDString, configData.chatS.command, GetRemoveTypeName(RemoveType.All)));
                        stringBuilder.AppendLine(Lang("Syntax3", player.UserIDString, configData.chatS.command, GetRemoveTypeName(RemoveType.Structure)));
                        stringBuilder.AppendLine(Lang("Syntax4", player.UserIDString, configData.chatS.command, GetRemoveTypeName(RemoveType.External)));
                        Print(player, stringBuilder.ToString());
                        stringBuilder.Clear();
                        Pool.Free(ref stringBuilder);
                        return;

                    default:
                        if (int.TryParse(args[0], out time)) break;
                        Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.command));
                        return;
                }
            }
            if (args != null && args.Length > 1) int.TryParse(args[1], out time);
            ToggleRemove(player, removeType, time);
        }

        private bool ToggleRemove(BasePlayer player, RemoveType removeType, int time = 0)
        {
            if (removeType == RemoveType.Normal && !permission.UserHasPermission(player.UserIDString, PERMISSION_NORMAL))
            {
                Print(player, Lang("NotAllowed", player.UserIDString, PERMISSION_NORMAL));
                return false;
            }

            int maxRemovable = 0;
            bool pay = false, refund = false;
            var removeTypeS = configData.removeTypeS[removeType];
            float distance = removeTypeS.distance;
            int maxTime = removeTypeS.maxTime;
            bool resetTime = removeTypeS.resetTime;
            float interval = configData.globalS.removeInterval;
            if (removeType == RemoveType.Normal)
            {
                var permissionS = GetPermissionS(player);
                var cooldown = permissionS.cooldown;
                if (cooldown > 0 && !(configData.globalS.cooldownExclude && player.IsAdmin))
                {
                    float lastUse;
                    if (cooldownTimes.TryGetValue(player.userID, out lastUse))
                    {
                        var timeLeft = cooldown - (Time.realtimeSinceStartup - lastUse);
                        if (timeLeft > 0)
                        {
                            Print(player, Lang("Cooldown", player.UserIDString, Math.Ceiling(timeLeft)));
                            return false;
                        }
                    }
                }
                if (removeMode == RemoveMode.MeleeHit && configData.removerModeS.meleeHitRequires)
                {
                    if (!IsMeleeTool(player))
                    {
                        Print(player, Lang("MeleeToolNotHeld", player.UserIDString));
                        return false;
                    }
                }
                if (removeMode == RemoveMode.SpecificTool && configData.removerModeS.specificToolRequires)
                {
                    if (!IsSpecificTool(player))
                    {
                        Print(player, Lang("SpecificToolNotHeld", player.UserIDString));
                        return false;
                    }
                }

                interval = permissionS.removeInterval;
                resetTime = permissionS.resetTime;
                maxTime = permissionS.maxTime;
                maxRemovable = permissionS.maxRemovable;
                if (configData.globalS.maxRemovableExclude && player.IsAdmin) maxRemovable = 0;
                distance = permissionS.distance;
                pay = permissionS.pay;
                refund = permissionS.refund;
            }
            if (time == 0) time = configData.removeTypeS[removeType].defaultTime;
            if (time > maxTime) time = maxTime;
            var toolRemover = player.GetOrAddComponent<ToolRemover>();
            if (toolRemover.removeType == RemoveType.Normal)
            {
                if (!configData.globalS.startCooldownOnRemoved)
                {
                    cooldownTimes[player.userID] = Time.realtimeSinceStartup;
                }
            }
            toolRemover.Init(removeType, time, maxRemovable, distance, interval, pay, refund, resetTime, true);
            Print(player, Lang("ToolEnabled", player.UserIDString, time, maxRemovable == 0 ? Lang("Unlimit", player.UserIDString) : maxRemovable.ToString(), GetRemoveTypeName(removeType)));
            return true;
        }

        [ConsoleCommand("remove.toggle")]
        private void CCmdRemoveToggle(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null)
            {
                Print(arg, "Syntax error!!! Please type the commands in the F1 console");
                return;
            }
            CmdRemove(player, string.Empty, arg.Args);
        }

        [ConsoleCommand("remove.target")]
        private void CCmdRemoveTarget(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length <= 1)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine("Syntax error of target command");
                stringBuilder.AppendLine("remove.target <disable | d> <player (name or id)> - Disable remover tool for player");
                stringBuilder.AppendLine("remove.target <normal | n> <player (name or id)> [time (seconds)] [max removable objects (integer)] - Enable remover tool for player (Normal)");
                stringBuilder.AppendLine("remove.target <admin | a> <player (name or id)> [time (seconds)] - Enable remover tool for player (Admin)");
                stringBuilder.AppendLine("remove.target <all> <player (name or id)> [time (seconds)] - Enable remover tool for player (All)");
                stringBuilder.AppendLine("remove.target <structure | s> <player (name or id)> [time (seconds)] - Enable remover tool for player (Structure)");
                stringBuilder.AppendLine("remove.target <external | e> <player (name or id)> [time (seconds)] - Enable remover tool for player (External)");
                Print(arg, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }
            var player = arg.Player();
            if (player != null && !permission.UserHasPermission(player.UserIDString, PERMISSION_TARGET))
            {
                Print(arg, Lang("NotAllowed", player.UserIDString, PERMISSION_TARGET));
                return;
            }
            var target = RustCore.FindPlayer(arg.Args[1]);
            if (target == null || !target.IsConnected)
            {
                Print(arg, target == null ? $"'{arg.Args[0]}' cannot be found." : $"'{target}' is offline.");
                return;
            }
            RemoveType removeType = RemoveType.Normal;
            switch (arg.Args[0].ToLower())
            {
                case "n":
                case "normal":
                    break;

                case "a":
                case "admin":
                    removeType = RemoveType.Admin;
                    break;

                case "all":
                    removeType = RemoveType.All;
                    break;

                case "s":
                case "structure":
                    removeType = RemoveType.Structure;
                    break;

                case "e":
                case "external":
                    removeType = RemoveType.External;
                    break;

                case "d":
                case "disable":
                    var toolRemover = target.GetComponent<ToolRemover>();
                    if (toolRemover != null)
                    {
                        toolRemover.DisableTool();
                        Print(arg, $"{target}'s remover tool is disabled");
                    }
                    else Print(arg, $"{target} did not enable the remover tool");
                    return;

                default:
                    StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                    stringBuilder.AppendLine("Syntax error of target command");
                    stringBuilder.AppendLine("remove.target <disable | d> <player (name or id)> - Disable remover tool for player");
                    stringBuilder.AppendLine("remove.target <normal | n> <player (name or id)> [time (seconds)] [max removable objects (integer)] - Enable remover tool for player (Normal)");
                    stringBuilder.AppendLine("remove.target <admin | a> <player (name or id)> [time (seconds)] - Enable remover tool for player (Admin)");
                    stringBuilder.AppendLine("remove.target <all> <player (name or id)> [time (seconds)] - Enable remover tool for player (All)");
                    stringBuilder.AppendLine("remove.target <structure | s> <player (name or id)> [time (seconds)] - Enable remover tool for player (Structure)");
                    stringBuilder.AppendLine("remove.target <external | e> <player (name or id)> [time (seconds)] - Enable remover tool for player (External)");
                    Print(arg, stringBuilder.ToString());
                    stringBuilder.Clear();
                    Pool.Free(ref stringBuilder);
                    return;
            }
            int maxRemovable = 0;
            int time = configData.removeTypeS[removeType].defaultTime;
            if (arg.Args.Length > 2) int.TryParse(arg.Args[2], out time);
            if (arg.Args.Length > 3 && removeType == RemoveType.Normal) int.TryParse(arg.Args[3], out maxRemovable);
            var permissionS = configData.permS[PERMISSION_NORMAL];
            var targetRemover = target.GetOrAddComponent<ToolRemover>();
            targetRemover.Init(removeType, time, maxRemovable, configData.removeTypeS[removeType].distance, permissionS.removeInterval, permissionS.pay, permissionS.refund, permissionS.resetTime, false);
            Print(arg, Lang("TargetEnabled", player?.UserIDString, target, time, maxRemovable, GetRemoveTypeName(removeType)));
        }

        [ConsoleCommand("remove.building")]
        private void CCmdConstruction(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length <= 1 || !arg.IsAdmin)
            {
                Print(arg, $"Syntax error, Please type 'remove.building <price / refund / priceP / refundP> <percentage>', e.g.'remove.building price 60'");
                return;
            }
            float value;
            switch (arg.Args[0].ToLower())
            {
                case "price":
                    if (!float.TryParse(arg.Args[1], out value)) value = 50f;
                    foreach (var construction in constructions)
                    {
                        BuildingBlocksSettings buildingBlocksSettings;
                        if (configData.removeS.buildingBlockS.TryGetValue(construction.info.name.english, out buildingBlocksSettings))
                        {
                            foreach (var entry in buildingBlocksSettings.buildingGradeS)
                            {
                                var grade = construction.grades[(int)entry.Key];
                                entry.Value.price = grade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => value <= 0 ? 0 : Mathf.RoundToInt(y.amount * value / 100));
                            }
                        }
                    }
                    Print(arg, $"Successfully modified all building prices to {value}% of the initial cost.");
                    SaveConfig();
                    return;

                case "refund":
                    if (!float.TryParse(arg.Args[1], out value)) value = 40f;
                    foreach (var construction in constructions)
                    {
                        BuildingBlocksSettings buildingBlocksSettings;
                        if (configData.removeS.buildingBlockS.TryGetValue(construction.info.name.english, out buildingBlocksSettings))
                        {
                            foreach (var entry in buildingBlocksSettings.buildingGradeS)
                            {
                                var grade = construction.grades[(int)entry.Key];
                                entry.Value.refund = grade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => value <= 0 ? 0 : Mathf.RoundToInt(y.amount * value / 100));
                            }
                        }
                    }
                    Print(arg, $"Successfully modified all building refunds to {value}% of the initial cost.");
                    SaveConfig();
                    return;

                case "pricep":
                    if (!float.TryParse(arg.Args[1], out value)) value = 40f;
                    foreach (var buildingBlockS in configData.removeS.buildingBlockS.Values)
                    {
                        foreach (var data in buildingBlockS.buildingGradeS.Values)
                            data.price = value <= 0 ? 0 : value;
                    }

                    Print(arg, $"Successfully modified all building prices to {value}% of the initial cost.");
                    SaveConfig();
                    return;

                case "refundp":
                    if (!float.TryParse(arg.Args[1], out value)) value = 50f;
                    foreach (var buildingBlockS in configData.removeS.buildingBlockS.Values)
                    {
                        foreach (var data in buildingBlockS.buildingGradeS.Values)
                            data.refund = value <= 0 ? 0 : value;
                    }

                    Print(arg, $"Successfully modified all building refunds to {value}% of the initial cost.");
                    SaveConfig();
                    return;

                default:
                    Print(arg, $"Syntax error, Please type 'remove.building <price / refund / priceP / refundP> <percentage>', e.g.'remove.building price 60'");
                    return;
            }
        }

        [ConsoleCommand("remove.allow")]
        private void CCmdRemoveAllow(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length == 0)
            {
                Print(arg, "Syntax error, Please type 'remove.allow <true | false>'");
                return;
            }
            var player = arg.Player();
            if (player != null && !permission.UserHasPermission(player.UserIDString, PERMISSION_OVERRIDE))
            {
                Print(arg, Lang("NotAllowed", player.UserIDString, PERMISSION_OVERRIDE));
                return;
            }
            switch (arg.Args[0].ToLower())
            {
                case "true":
                case "1":
                    removeOverride = false;
                    Print(arg, "Remove is now allowed depending on your settings.");
                    return;

                case "false":
                case "0":
                    removeOverride = true;
                    Print(arg, "Remove is now restricted for all players (exept admins)");
                    foreach (var p in BasePlayer.activePlayerList)
                    {
                        var toolRemover = p.GetComponent<ToolRemover>();
                        if (toolRemover == null) continue;
                        if (toolRemover.removeType == RemoveType.Normal && toolRemover.canOverride)
                        {
                            Print(toolRemover.player, "The remover tool has been disabled by the admin");
                            toolRemover.DisableTool(false);
                        }
                    }
                    return;

                default:
                    Print(arg, "This is not a valid argument");
                    return;
            }
        }

        [ConsoleCommand("remove.playerentity")]
        private void CCmdRemoveEntity(ConsoleSystem.Arg arg)
        {
            if (arg.Args == null || arg.Args.Length <= 1 || !arg.IsAdmin)
            {
                StringBuilder stringBuilder = Pool.Get<StringBuilder>();
                stringBuilder.AppendLine("Syntax error of remove.playerentity command");
                stringBuilder.AppendLine("remove.playerentity <all | a> <player id> - Remove all entities of the player");
                stringBuilder.AppendLine("remove.playerentity <building | b> <player id> - Remove all buildings of the player");
                stringBuilder.AppendLine("remove.playerentity <cupboard | c> <player id> - Remove buildings of the player owned cupboard");
                Print(arg, stringBuilder.ToString());
                stringBuilder.Clear();
                Pool.Free(ref stringBuilder);
                return;
            }
            if (removePlayerEntityCoroutine != null)
            {
                Print(arg, "There is already a RemovePlayerEntity running, please wait.");
                return;
            }
            ulong targetID;
            if (!ulong.TryParse(arg.Args[1], out targetID) || !targetID.IsSteamId())
            {
                Print(arg, "Please enter the player's steamID.");
                return;
            }
            RemovePlayerEntityType removePlayerEntityType;
            switch (arg.Args[0].ToLower())
            {
                case "a":
                case "all":
                    removePlayerEntityType = RemovePlayerEntityType.All;
                    break;

                case "b":
                case "building":
                    removePlayerEntityType = RemovePlayerEntityType.Building;
                    break;

                case "c":
                case "cupboard":
                    removePlayerEntityType = RemovePlayerEntityType.Cupboard;
                    break;

                default:
                    Print(arg, "This is not a valid argument");
                    return;
            }
            removePlayerEntityCoroutine = ServerMgr.Instance.StartCoroutine(RemovePlayerEntity(arg, targetID, removePlayerEntityType));
            Print(arg, "Start running RemovePlayerEntity, please wait.");
        }

        #endregion Commands

        #region Debug

        private StringBuilder debugStringBuilder;

        private void PrintDebug(string message)
        {
            if (configData.globalS.logToFile)
            {
                debugStringBuilder.AppendLine($"[{DateTime.Now.ToString(CultureInfo.InstalledUICulture)}] | {message}");
            }
        }

        private void SaveDebug()
        {
            if (!configData.globalS.logToFile) return;
            var debugText = debugStringBuilder.ToString().Trim();
            debugStringBuilder.Clear();
            if (!string.IsNullOrEmpty(debugText))
            {
                LogToFile("debug", debugText, this);
            }
        }

        #endregion Debug

        #region ConfigurationFile

        private void UpdateConfig()
        {
            var buildingGrades = new[] { BuildingGrade.Enum.Twigs, BuildingGrade.Enum.Wood, BuildingGrade.Enum.Stone, BuildingGrade.Enum.Metal, BuildingGrade.Enum.TopTier };
            foreach (var value in buildingGrades)
            {
                if (!configData.removeS.validConstruction.ContainsKey(value))
                {
                    configData.removeS.validConstruction.Add(value, true);
                }
            }

            var newBuildingBlocksS = new Dictionary<string, BuildingBlocksSettings>();
            foreach (var construction in constructions)
            {
                BuildingBlocksSettings buildingBlocksSettings;
                if (!configData.removeS.buildingBlockS.TryGetValue(construction.info.name.english, out buildingBlocksSettings))
                {
                    var buildingGrade = new Dictionary<BuildingGrade.Enum, BuildingGradeSettings>();
                    foreach (var value in buildingGrades)
                    {
                        var grade = construction.grades[(int)value];
                        buildingGrade.Add(value, new BuildingGradeSettings { refund = grade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => Mathf.RoundToInt(y.amount * 0.4f)), price = grade.costToBuild.ToDictionary(x => x.itemDef.shortname, y => Mathf.RoundToInt(y.amount * 0.6f)) });
                    }
                    buildingBlocksSettings = new BuildingBlocksSettings { displayName = construction.info.name.english, buildingGradeS = buildingGrade };
                }
                newBuildingBlocksS.Add(construction.info.name.english, buildingBlocksSettings);
            }
            configData.removeS.buildingBlockS = newBuildingBlocksS;

            foreach (var entry in shorPrefabNameToDeployable)
            {
                EntitySettings entitySettings;
                if (!configData.removeS.entityS.TryGetValue(entry.Key, out entitySettings))
                {
                    var itemDefinition = ItemManager.FindItemDefinition(entry.Value);
                    entitySettings = new EntitySettings
                    {
                        enabled = itemDefinition.category != ItemCategory.Food,
                        displayName = itemDefinition.displayName.english,
                        refund = new Dictionary<string, int> { [entry.Value] = 1 },
                        price = new Dictionary<string, int>()
                    };
                    configData.removeS.entityS.Add(entry.Key, entitySettings);
                }
            }
            SaveConfig();
        }

        private static ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public GlobalSettings globalS = new GlobalSettings();

            [JsonProperty(PropertyName = "Container Settings")]
            public ContainerSettings containerS = new ContainerSettings();

            [JsonProperty(PropertyName = "Remove Damaged Entities")]
            public DamagedEntitySettings damagedEntityS = new DamagedEntitySettings();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            [JsonProperty(PropertyName = "Permission Settings (Just for normal type)")]
            public Dictionary<string, PermissionSettings> permS = new Dictionary<string, PermissionSettings>
            {
                [PERMISSION_NORMAL] = new PermissionSettings { priority = 0, distance = 3, cooldown = 60, maxTime = 300, maxRemovable = 50, removeInterval = 0.8f, pay = true, refund = true, resetTime = false }
            };

            [JsonProperty(PropertyName = "Remove Type Settings")]
            public Dictionary<RemoveType, RemoveTypeSettings> removeTypeS = new Dictionary<RemoveType, RemoveTypeSettings>
            {
                [RemoveType.Normal] = new RemoveTypeSettings { displayName = RemoveType.Normal.ToString(), distance = 3, gibs = true, defaultTime = 60, maxTime = 300, resetTime = false },
                [RemoveType.Structure] = new RemoveTypeSettings { displayName = RemoveType.Structure.ToString(), distance = 100, gibs = false, defaultTime = 300, maxTime = 600, resetTime = true },
                [RemoveType.All] = new RemoveTypeSettings { displayName = RemoveType.All.ToString(), distance = 50, gibs = false, defaultTime = 300, maxTime = 600, resetTime = true },
                [RemoveType.Admin] = new RemoveTypeSettings { displayName = RemoveType.Admin.ToString(), distance = 20, gibs = true, defaultTime = 300, maxTime = 600, resetTime = true },
                [RemoveType.External] = new RemoveTypeSettings { displayName = RemoveType.External.ToString(), distance = 20, gibs = true, defaultTime = 300, maxTime = 600, resetTime = true }
            };

            [JsonProperty(PropertyName = "Remove Mode Settings (Only one model works)")]
            public RemoverModeSettings removerModeS = new RemoverModeSettings();

            [JsonProperty(PropertyName = "Raid Blocker Settings")]
            public RaidBlockerSettings raidS = new RaidBlockerSettings();

            [JsonProperty(PropertyName = "Image Urls (Used to UI image)")]
            public Dictionary<string, string> imageUrls = new Dictionary<string, string>
            {
                ["Economics"] = "https://i.imgur.com/znPwdcv.png",
                ["ServerRewards"] = "https://i.imgur.com/04rJsV3.png"
            };

            [JsonProperty(PropertyName = "Display Names Of Other Things")]
            public Dictionary<string, string> displayNames = new Dictionary<string, string>();

            [JsonProperty(PropertyName = "GUI")]
            public UiSettings uiS = new UiSettings();

            [JsonProperty(PropertyName = "Remove Info (Refund & Price)")]
            public RemoveSettings removeS = new RemoveSettings();

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version;
        }

        public class GlobalSettings
        {
            [JsonProperty(PropertyName = "Log Debug To File")]
            public bool logToFile = false;

            [JsonProperty(PropertyName = "Use Teams")]
            public bool useTeams = false;

            [JsonProperty(PropertyName = "Use Clans")]
            public bool useClans = true;

            [JsonProperty(PropertyName = "Use Friends")]
            public bool useFriends = true;

            [JsonProperty(PropertyName = "Use Entity Owners")]
            public bool useEntityOwners = true;

            [JsonProperty(PropertyName = "Use Building Locks")]
            public bool useBuildingLocks = false;

            [JsonProperty(PropertyName = "Use Tool Cupboards (Strongly unrecommended)")]
            public bool useToolCupboards = false;

            [JsonProperty(PropertyName = "Use Building Owners (You will need BuildingOwners plugin)")]
            public bool useBuildingOwners = false;

            //[JsonProperty(PropertyName = "Exclude Twigs (Used for \"Use Tool Cupboards\" and \"Use Entity Owners\")")]
            //public bool excludeTwigs;

            [JsonProperty(PropertyName = "Remove Button")]
            public string removeButton = BUTTON.FIRE_PRIMARY.ToString();

            [JsonProperty(PropertyName = "Remove Interval (Min = 0.2)")]
            public float removeInterval = 0.5f;

            [JsonProperty(PropertyName = "Only start cooldown when an entity is removed")]
            public bool startCooldownOnRemoved;

            [JsonProperty(PropertyName = "RemoveType - All/Structure - Remove per frame")]
            public int removePerFrame = 15;

            [JsonProperty(PropertyName = "RemoveType - All/Structure - No item container dropped")]
            public bool noItemContainerDrop = true;

            [JsonProperty(PropertyName = "RemoveType - Normal - Max Removable Objects - Exclude admins")]
            public bool maxRemovableExclude = true;

            [JsonProperty(PropertyName = "RemoveType - Normal - Cooldown - Exclude admins")]
            public bool cooldownExclude = true;

            [JsonProperty(PropertyName = "RemoveType - Normal - Check stash under the foundation")]
            public bool checkStash = false;

            [JsonProperty(PropertyName = "RemoveType - Normal - Entity Spawned Time Limit - Enabled")]
            public bool entityTimeLimit = false;

            [JsonProperty(PropertyName = "RemoveType - Normal - Entity Spawned Time Limit - Cannot be removed when entity spawned time more than it")]
            public float limitTime = 300f;
        }

        public class ContainerSettings
        {
            [JsonProperty(PropertyName = "Storage Container - Enable remove of not empty storages")]
            public bool removeNotEmptyStorage = true;

            [JsonProperty(PropertyName = "Storage Container - Drop items from container")]
            public bool dropItemsStorage = false;

            [JsonProperty(PropertyName = "Storage Container - Drop a item container from container")]
            public bool dropContainerStorage = true;

            [JsonProperty(PropertyName = "IOEntity Container - Enable remove of not empty storages")]
            public bool removeNotEmptyIoEntity = true;

            [JsonProperty(PropertyName = "IOEntity Container - Drop items from container")]
            public bool dropItemsIoEntity = false;

            [JsonProperty(PropertyName = "IOEntity Container - Drop a item container from container")]
            public bool dropContainerIoEntity = true;
        }

        public class DamagedEntitySettings
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Exclude Building Blocks")]
            public bool excludeBuildingBlocks = true;

            [JsonProperty(PropertyName = "Percentage (Can be removed when (health / max health * 100) is not less than it)")]
            public float percentage = 90f;
        }

        public class ChatSettings
        {
            [JsonProperty(PropertyName = "Chat Command")]
            public string command = "remove";

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "<color=#00FFFF>[RemoverTool]</color>: ";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;
        }

        public class PermissionSettings
        {
            [JsonProperty(PropertyName = "Priority")]
            public int priority;

            [JsonProperty(PropertyName = "Distance")]
            public float distance;

            [JsonProperty(PropertyName = "Cooldown")]
            public float cooldown;

            [JsonProperty(PropertyName = "Max Time")]
            public int maxTime;

            [JsonProperty(PropertyName = "Remove Interval (Min = 0.2)")]
            public float removeInterval;

            [JsonProperty(PropertyName = "Max Removable Objects (0 = Unlimit)")]
            public int maxRemovable;

            [JsonProperty(PropertyName = "Pay")]
            public bool pay;

            [JsonProperty(PropertyName = "Refund")]
            public bool refund;

            [JsonProperty(PropertyName = "Reset the time after removing an entity")]
            public bool resetTime;
        }

        public class RemoveTypeSettings
        {
            [JsonProperty(PropertyName = "Display Name")]
            public string displayName;

            [JsonProperty(PropertyName = "Distance")]
            public float distance;

            [JsonProperty(PropertyName = "Default Time")]
            public int defaultTime;

            [JsonProperty(PropertyName = "Max Time")]
            public int maxTime;

            [JsonProperty(PropertyName = "Gibs")]
            public bool gibs;

            [JsonProperty(PropertyName = "Reset the time after removing an entity")]
            public bool resetTime;
        }

        public class RemoverModeSettings
        {
            [JsonProperty(PropertyName = "No Held Item Mode")]
            public bool noHeldMode = true;

            [JsonProperty(PropertyName = "No Held Item Mode - Disable remover tool when you have any item in hand")]
            public bool noHeldDisableInHand = true;

            [JsonProperty(PropertyName = "No Held Item Mode - Show Crosshair")]
            public bool showCrosshair = true;

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Image Url")]
            public string crosshairImageUrl = "https://i.imgur.com/SqLCJaQ.png";

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Box - Min Anchor (in Rust Window)")]
            public string crosshairAnchorMin = "0.5 0.5";

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Box - Max Anchor (in Rust Window)")]
            public string crosshairAnchorMax = "0.5 0.5";

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Box - Min Offset (in Rust Window)")]
            public string crosshairOffsetMin = "-15 -15";

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Box - Max Offset (in Rust Window)")]
            public string crosshairOffsetMax = "15 15";

            [JsonProperty(PropertyName = "No Held Item Mode - Crosshair Box - Image Color")]
            public string crosshairColor = "1 0 0 1";

            [JsonProperty(PropertyName = "Melee Tool Hit Mode")]
            public bool meleeHitMode = false;

            [JsonProperty(PropertyName = "Melee Tool Hit Mode - Item shortname")]
            public string meleeHitItemShortname = "hammer";

            [JsonProperty(PropertyName = "Melee Tool Hit Mode - Item skin (-1 = All skins)")]
            public long meleeHitModeSkin = -1;

            [JsonProperty(PropertyName = "Melee Tool Hit Mode - Auto enable remover tool when you hold a melee tool")]
            public bool meleeHitEnableInHand = false;

            [JsonProperty(PropertyName = "Melee Tool Hit Mode - Requires a melee tool in your hand when remover tool is enabled")]
            public bool meleeHitRequires = false;

            [JsonProperty(PropertyName = "Melee Tool Hit Mode - Disable remover tool when you are not holding a melee tool")]
            public bool meleeHitDisableInHand = false;

            [JsonProperty(PropertyName = "Specific Tool Mode")]
            public bool specificToolMode = false;

            [JsonProperty(PropertyName = "Specific Tool Mode - Item shortname")]
            public string specificToolShortname = "hammer";

            [JsonProperty(PropertyName = "Specific Tool Mode - Item skin (-1 = All skins)")]
            public long specificToolSkin = -1;

            [JsonProperty(PropertyName = "Specific Tool Mode - Auto enable remover tool when you hold a specific tool")]
            public bool specificToolEnableInHand = false;

            [JsonProperty(PropertyName = "Specific Tool Mode - Requires a specific tool in your hand when remover tool is enabled")]
            public bool specificToolRequires = false;

            [JsonProperty(PropertyName = "Specific Tool Mode - Disable remover tool when you are not holding a specific tool")]
            public bool specificToolDisableInHand = false;
        }

        public class RaidBlockerSettings
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Block Time")]
            public float blockTime = 300;

            [JsonProperty(PropertyName = "By Buildings")]
            public bool blockBuildingID = true;

            [JsonProperty(PropertyName = "By Surrounding Players")]
            public bool blockPlayers = true;

            [JsonProperty(PropertyName = "By Surrounding Players - Radius")]
            public float blockRadius = 120;
        }

        public class UiSettings
        {
            [JsonProperty(PropertyName = "Enabled")]
            public bool enabled = true;

            [JsonProperty(PropertyName = "Main Box - Min Anchor (in Rust Window)")]
            public string removerToolAnchorMin = "0 1";

            [JsonProperty(PropertyName = "Main Box - Max Anchor (in Rust Window)")]
            public string removerToolAnchorMax = "0 1";

            [JsonProperty(PropertyName = "Main Box - Min Offset (in Rust Window)")]
            public string removerToolOffsetMin = "30 -330";

            [JsonProperty(PropertyName = "Main Box - Max Offset (in Rust Window)")]
            public string removerToolOffsetMax = "470 -40";

            [JsonProperty(PropertyName = "Main Box - Background Color")]
            public string removerToolBackgroundColor = "0 0 0 0";

            [JsonProperty(PropertyName = "Remove Title - Box - Min Anchor (in Main Box)")]
            public string removeAnchorMin = "0 0.84";

            [JsonProperty(PropertyName = "Remove Title - Box - Max Anchor (in Main Box)")]
            public string removeAnchorMax = "0.996 1";

            [JsonProperty(PropertyName = "Remove Title - Box - Background Color")]
            public string removeBackgroundColor = "0.31 0.88 0.71 1";

            [JsonProperty(PropertyName = "Remove Title - Text - Min Anchor (in Main Box)")]
            public string removeTextAnchorMin = "0.05 0.84";

            [JsonProperty(PropertyName = "Remove Title - Text - Max Anchor (in Main Box)")]
            public string removeTextAnchorMax = "0.6 1";

            [JsonProperty(PropertyName = "Remove Title - Text - Text Color")]
            public string removeTextColor = "1 0.1 0.1 1";

            [JsonProperty(PropertyName = "Remove Title - Text - Text Size")]
            public int removeTextSize = 18;

            [JsonProperty(PropertyName = "Timeleft - Box - Min Anchor (in Main Box)")]
            public string timeLeftAnchorMin = "0.6 0.84";

            [JsonProperty(PropertyName = "Timeleft - Box - Max Anchor (in Main Box)")]
            public string timeLeftAnchorMax = "1 1";

            [JsonProperty(PropertyName = "Timeleft - Box - Background Color")]
            public string timeLeftBackgroundColor = "0 0 0 0";

            [JsonProperty(PropertyName = "Timeleft - Text - Min Anchor (in Timeleft Box)")]
            public string timeLeftTextAnchorMin = "0 0";

            [JsonProperty(PropertyName = "Timeleft - Text - Max Anchor (in Timeleft Box)")]
            public string timeLeftTextAnchorMax = "0.9 1";

            [JsonProperty(PropertyName = "Timeleft - Text - Text Color")]
            public string timeLeftTextColor = "0 0 0 0.9";

            [JsonProperty(PropertyName = "Timeleft - Text - Text Size")]
            public int timeLeftTextSize = 15;

            [JsonProperty(PropertyName = "Entity - Box - Min Anchor (in Main Box)")]
            public string entityAnchorMin = "0 0.68";

            [JsonProperty(PropertyName = "Entity - Box - Max Anchor (in Main Box)")]
            public string entityAnchorMax = "1 0.84";

            [JsonProperty(PropertyName = "Entity - Box - Background Color")]
            public string entityBackgroundColor = "0.82 0.58 0.30 1";

            [JsonProperty(PropertyName = "Entity - Text - Min Anchor (in Entity Box)")]
            public string entityTextAnchorMin = "0.05 0";

            [JsonProperty(PropertyName = "Entity - Text - Max Anchor (in Entity Box)")]
            public string entityTextAnchorMax = "1 1";

            [JsonProperty(PropertyName = "Entity - Text - Text Color")]
            public string entityTextColor = "1 1 1 1";

            [JsonProperty(PropertyName = "Entity - Text - Text Size")]
            public int entityTextSize = 16;

            [JsonProperty(PropertyName = "Entity - Image - Enabled")]
            public bool entityImageEnabled = true;

            [JsonProperty(PropertyName = "Entity - Image - Min Anchor (in Entity Box)")]
            public string entityImageAnchorMin = "0.795 0.01";

            [JsonProperty(PropertyName = "Entity - Image - Max Anchor (in Entity Box)")]
            public string entityImageAnchorMax = "0.9 0.99";

            [JsonProperty(PropertyName = "Authorization Check Enabled")]
            public bool authorizationEnabled = true;

            [JsonProperty(PropertyName = "Authorization Check - Box - Min Anchor (in Main Box)")]
            public string authorizationsAnchorMin = "0 0.6";

            [JsonProperty(PropertyName = "Authorization Check - Box - Max Anchor (in Main Box)")]
            public string authorizationsAnchorMax = "1 0.68";

            [JsonProperty(PropertyName = "Authorization Check - Box - Allowed Background Color")]
            public string allowedBackgroundColor = "0.22 0.78 0.27 1";

            [JsonProperty(PropertyName = "Authorization Check - Box - Refused Background Color")]
            public string refusedBackgroundColor = "0.78 0.22 0.27 1";

            [JsonProperty(PropertyName = "Authorization Check - Text - Min Anchor (in Authorization Check Box)")]
            public string authorizationsTextAnchorMin = "0.05 0";

            [JsonProperty(PropertyName = "Authorization Check - Text - Max Anchor (in Authorization Check Box)")]
            public string authorizationsTextAnchorMax = "1 1";

            [JsonProperty(PropertyName = "Authorization Check - Text - Text Color")]
            public string authorizationsTextColor = "1 1 1 0.9";

            [JsonProperty(PropertyName = "Authorization Check Box - Text - Text Size")]
            public int authorizationsTextSize = 14;

            [JsonProperty(PropertyName = "Price & Refund - Image Enabled")]
            public bool imageEnabled = true;

            [JsonProperty(PropertyName = "Price & Refund - Image Scale")]
            public float imageScale = 0.18f;

            [JsonProperty(PropertyName = "Price & Refund - Distance of image from right border")]
            public float rightDistance = 0.05f;

            [JsonProperty(PropertyName = "Price Enabled")]
            public bool priceEnabled = true;

            [JsonProperty(PropertyName = "Price - Box - Min Anchor (in Main Box)")]
            public string priceAnchorMin = "0 0.3";

            [JsonProperty(PropertyName = "Price - Box - Max Anchor (in Main Box)")]
            public string priceAnchorMax = "1 0.6";

            [JsonProperty(PropertyName = "Price - Box - Background Color")]
            public string priceBackgroundColor = "0 0 0 0.8";

            [JsonProperty(PropertyName = "Price - Text - Min Anchor (in Price Box)")]
            public string priceTextAnchorMin = "0.05 0";

            [JsonProperty(PropertyName = "Price - Text - Max Anchor (in Price Box)")]
            public string priceTextAnchorMax = "0.25 1";

            [JsonProperty(PropertyName = "Price - Text - Text Color")]
            public string priceTextColor = "1 1 1 0.9";

            [JsonProperty(PropertyName = "Price - Text - Text Size")]
            public int priceTextSize = 18;

            [JsonProperty(PropertyName = "Price - Text2 - Min Anchor (in Price Box)")]
            public string price2TextAnchorMin = "0.3 0";

            [JsonProperty(PropertyName = "Price - Text2 - Max Anchor (in Price Box)")]
            public string price2TextAnchorMax = "1 1";

            [JsonProperty(PropertyName = "Price - Text2 - Text Color")]
            public string price2TextColor = "1 1 1 0.9";

            [JsonProperty(PropertyName = "Price - Text2 - Text Size")]
            public int price2TextSize = 16;

            [JsonProperty(PropertyName = "Refund Enabled")]
            public bool refundEnabled = true;

            [JsonProperty(PropertyName = "Refund - Box - Min Anchor (in Main Box)")]
            public string refundAnchorMin = "0 0";

            [JsonProperty(PropertyName = "Refund - Box - Max Anchor (in Main Box)")]
            public string refundAnchorMax = "1 0.3";

            [JsonProperty(PropertyName = "Refund - Box - Background Color")]
            public string refundBackgroundColor = "0 0 0 0.8";

            [JsonProperty(PropertyName = "Refund - Text - Min Anchor (in Refund Box)")]
            public string refundTextAnchorMin = "0.05 0";

            [JsonProperty(PropertyName = "Refund - Text - Max Anchor (in Refund Box)")]
            public string refundTextAnchorMax = "0.25 1";

            [JsonProperty(PropertyName = "Refund - Text - Text Color")]
            public string refundTextColor = "1 1 1 0.9";

            [JsonProperty(PropertyName = "Refund - Text - Text Size")]
            public int refundTextSize = 18;

            [JsonProperty(PropertyName = "Refund - Text2 - Min Anchor (in Refund Box)")]
            public string refund2TextAnchorMin = "0.3 0";

            [JsonProperty(PropertyName = "Refund - Text2 - Max Anchor (in Refund Box)")]
            public string refund2TextAnchorMax = "1 1";

            [JsonProperty(PropertyName = "Refund - Text2 - Text Color")]
            public string refund2TextColor = "1 1 1 0.9";

            [JsonProperty(PropertyName = "Refund - Text2 - Text Size")]
            public int refund2TextSize = 16;

            [JsonIgnore]
            public Vector2 Price2TextAnchorMin, Price2TextAnchorMax, Refund2TextAnchorMin, Refund2TextAnchorMax;
        }

        public class RemoveSettings
        {
            [JsonProperty(PropertyName = "Price Enabled")]
            public bool priceEnabled = true;

            [JsonProperty(PropertyName = "Refund Enabled")]
            public bool refundEnabled = true;

            [JsonProperty(PropertyName = "Refund Items In Entity Slot")]
            public bool refundSlot = true;

            [JsonProperty(PropertyName = "Allowed Building Grade")]
            public Dictionary<BuildingGrade.Enum, bool> validConstruction = new Dictionary<BuildingGrade.Enum, bool>();

            [JsonProperty(PropertyName = "Building Blocks Settings")]
            public Dictionary<string, BuildingBlocksSettings> buildingBlockS = new Dictionary<string, BuildingBlocksSettings>();

            [JsonProperty(PropertyName = "Other Entity Settings")]
            public Dictionary<string, EntitySettings> entityS = new Dictionary<string, EntitySettings>();
        }

        public class BuildingBlocksSettings
        {
            [JsonProperty(PropertyName = "Display Name")]
            public string displayName;

            [JsonProperty(PropertyName = "Building Grade")]
            public Dictionary<BuildingGrade.Enum, BuildingGradeSettings> buildingGradeS = new Dictionary<BuildingGrade.Enum, BuildingGradeSettings>();
        }

        public class BuildingGradeSettings
        {
            [JsonProperty(PropertyName = "Price")]
            public object price;

            [JsonProperty(PropertyName = "Refund")]
            public object refund;

            [JsonIgnore] public float pricePercentage = -1, refundPercentage = -1;
            [JsonIgnore] public Dictionary<string, int> priceDic, refundDic;
        }

        public class EntitySettings
        {
            [JsonProperty(PropertyName = "Remove Allowed")]
            public bool enabled = false;

            [JsonProperty(PropertyName = "Display Name")]
            public string displayName = string.Empty;

            [JsonProperty(PropertyName = "Price")]
            public Dictionary<string, int> price = new Dictionary<string, int>();

            [JsonProperty(PropertyName = "Refund")]
            public Dictionary<string, int> refund = new Dictionary<string, int>();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                PreprocessOldConfig();
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
            PreprocessConfigValues();
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file");
            configData = new ConfigData();
            configData.version = Version;
        }

        protected override void SaveConfig() => Config.WriteObject(configData);

        private void PreprocessConfigValues()
        {
            configData.uiS.Price2TextAnchorMin = GetAnchor(configData.uiS.price2TextAnchorMin);
            configData.uiS.Price2TextAnchorMax = GetAnchor(configData.uiS.price2TextAnchorMax);
            configData.uiS.Refund2TextAnchorMin = GetAnchor(configData.uiS.refund2TextAnchorMin);
            configData.uiS.Refund2TextAnchorMax = GetAnchor(configData.uiS.refund2TextAnchorMax);

            foreach (var entry in configData.removeS.buildingBlockS)
            {
                foreach (var gradeEntry in entry.Value.buildingGradeS)
                {
                    var price = gradeEntry.Value.price;
                    float pricePercentage;
                    if (float.TryParse(price.ToString(), out pricePercentage))
                    {
                        gradeEntry.Value.pricePercentage = pricePercentage;
                    }
                    else
                    {
                        var priceDic = price as Dictionary<string, int>;
                        if (priceDic != null)
                        {
                            gradeEntry.Value.priceDic = priceDic;
                        }
                        else
                        {
                            try { gradeEntry.Value.priceDic = JsonConvert.DeserializeObject<Dictionary<string, int>>(price.ToString()); }
                            catch (Exception e)
                            {
                                gradeEntry.Value.priceDic = null;
                                PrintError($"Wrong price format for '{gradeEntry.Key}' of '{entry.Key}' in 'Building Blocks Settings'. Error Message: {e.Message}");
                            }
                        }
                    }

                    var refund = gradeEntry.Value.refund;
                    float refundPercentage;
                    if (float.TryParse(refund.ToString(), out refundPercentage))
                    {
                        gradeEntry.Value.refundPercentage = refundPercentage;
                    }
                    else
                    {
                        var refundDic = refund as Dictionary<string, int>;
                        if (refundDic != null)
                        {
                            gradeEntry.Value.refundDic = refundDic;
                        }
                        else
                        {
                            try { gradeEntry.Value.refundDic = JsonConvert.DeserializeObject<Dictionary<string, int>>(refund.ToString()); }
                            catch (Exception e)
                            {
                                gradeEntry.Value.refundDic = null;
                                PrintError($"Wrong refund format for '{gradeEntry.Key}' of '{entry.Key}' in 'Building Blocks Settings'. Error Message: {e.Message}");
                            }
                        }
                    }
                }
            }
        }

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

                    if (configData.uiS.removerToolAnchorMin == "0.1 0.55")
                    {
                        configData.uiS.removerToolAnchorMin = "0.04 0.55";
                    }

                    if (configData.uiS.removerToolAnchorMax == "0.4 0.95")
                    {
                        configData.uiS.removerToolAnchorMax = "0.37 0.95";
                    }
                }

                if (configData.version <= new VersionNumber(4, 3, 18))
                {
                    configData.removerModeS.crosshairAnchorMin = "0.5 0.5";
                    configData.removerModeS.crosshairAnchorMax = "0.5 0.5";
                    configData.uiS.removerToolAnchorMin = "0 1";
                    configData.uiS.removerToolAnchorMax = "0 1";
                }

                if (configData.version <= new VersionNumber(4, 3, 21))
                {
                    if (configData.removerModeS.crosshairAnchorMin == "0.5 0")
                    {
                        configData.removerModeS.crosshairAnchorMin = "0.5 0.5";
                    }
                    if (configData.removerModeS.crosshairAnchorMax == "0.5 0")
                    {
                        configData.removerModeS.crosshairAnchorMax = "0.5 0.5";
                    }
                }

                if (configData.version <= new VersionNumber(4, 3, 22))
                {
                    bool enabled;
                    if (GetConfigValue(out enabled, "Remove Mode Settings (Only one model works)", "Hammer Hit Mode"))
                    {
                        configData.removerModeS.meleeHitMode = true;
                        configData.removerModeS.meleeHitItemShortname = "hammer";
                    }
                    if (GetConfigValue(out enabled, "Remove Mode Settings (Only one model works)", "Hammer Hit Mode - Requires a hammer in your hand when remover tool is enabled"))
                    {
                        configData.removerModeS.meleeHitRequires = true;
                    }
                    if (GetConfigValue(out enabled, "Remove Mode Settings (Only one model works)", "Hammer Hit Mode - Disable remover tool when you are not holding a hammer"))
                    {
                        configData.removerModeS.meleeHitDisableInHand = true;
                    }
                }

                if (configData.version <= new VersionNumber(4, 3, 23))
                {
                    string value;
                    if (GetConfigValue(out value, "GUI", "Authorization Check - Box - Allowed Background"))
                    {
                        configData.uiS.allowedBackgroundColor = value == "0 1 0 0.8" ? "0.22 0.78 0.27 1" : value;
                    }
                    if (GetConfigValue(out value, "GUI", "Authorization Check - Box - Refused Background"))
                    {
                        configData.uiS.refusedBackgroundColor = value == "1 0 0 0.8" ? "0.78 0.22 0.27 1" : value;
                    }
                    if (configData.uiS.removeBackgroundColor == "0.42 0.88 0.88 1")
                        configData.uiS.removeBackgroundColor = "0.31 0.88 0.71 1";
                    if (configData.uiS.entityBackgroundColor == "0 0 0 0.8")
                        configData.uiS.entityBackgroundColor = "0.82 0.58 0.30 1";
                }
                configData.version = Version;
            }
        }

        private bool GetConfigValue<T>(out T value, params string[] path)
        {
            var configValue = Config.Get(path);
            if (configValue != null)
            {
                if (configValue is T)
                {
                    value = (T)configValue;
                    return true;
                }
                try
                {
                    value = Config.ConvertValue<T>(configValue);
                    return true;
                }
                catch (Exception ex)
                {
                    PrintError($"GetConfigValue ERROR: path: {string.Join("\\", path)}\n{ex}");
                }
            }

            value = default(T);
            return false;
        }

        private void SetConfigValue(params object[] pathAndTrailingValue)
        {
            Config.Set(pathAndTrailingValue);
        }

        #region Preprocess Old Config

        private void PreprocessOldConfig()
        {
            var jObject = Config.ReadObject<JObject>();
            if (jObject == null) return;
            //Interface.Oxide.DataFileSystem.WriteObject(Name + "_old", jObject);
            VersionNumber oldVersion;
            if (GetConfigVersionPre(jObject, out oldVersion))
            {
                if (oldVersion < Version)
                {
                    //Fixed typos
                    if (oldVersion <= new VersionNumber(4, 3, 23))
                    {
                        foreach (RemoveType value in Enum.GetValues(typeof(RemoveType)))
                        {
                            if (value == RemoveType.None) continue;
                            bool enabled;
                            if (GetConfigValuePre(jObject, out enabled, "Remove Type Settings", value.ToString(), "Reset the time after removing a entity"))
                            {
                                SetConfigValuePre(jObject, enabled, "Remove Type Settings", value.ToString(), "Reset the time after removing an entity");
                            }
                        }
                        Dictionary<string, object> values;
                        if (GetConfigValuePre(jObject, out values, "Permission Settings (Just for normal type)"))
                        {
                            foreach (var entry in values)
                            {
                                object value;
                                if (GetConfigValuePre(jObject, out value, "Permission Settings (Just for normal type)", entry.Key, "Reset the time after removing a entity"))
                                {
                                    SetConfigValuePre(jObject, value, "Permission Settings (Just for normal type)", entry.Key, "Reset the time after removing an entity");
                                }
                            }
                        }
                    }
                    Config.WriteObject(jObject);
                    //Interface.Oxide.DataFileSystem.WriteObject(Name + "_new", jObject);
                }
            }
        }

        private bool GetConfigValuePre<T>(JObject config, out T value, params string[] path)
        {
            if (path.Length < 1)
            {
                throw new ArgumentException("path is empty");
            }

            try
            {
                JToken jToken;
                if (!config.TryGetValue(path[0], out jToken))
                {
                    value = default(T);
                    return false;
                }

                for (int i = 1; i < path.Length; i++)
                {
                    var jObject = jToken.ToObject<JObject>();
                    if (jObject == null || !jObject.TryGetValue(path[i], out jToken))
                    {
                        value = default(T);
                        return false;
                    }
                }
                value = jToken.ToObject<T>();
                return true;
            }
            catch (Exception ex)
            {
                PrintError($"GetConfigValuePre ERROR: path: {string.Join("\\", path)}\n{ex}");
            }
            value = default(T);
            return false;
        }

        private void SetConfigValuePre(JObject config, object value, params string[] path)
        {
            if (path.Length < 1)
            {
                throw new ArgumentException("path is empty");
            }

            try
            {
                JToken jToken;
                if (!config.TryGetValue(path[0], out jToken))
                {
                    if (path.Length == 1)
                    {
                        jToken = JToken.FromObject(value);
                        config.Add(path[0], jToken);
                        return;
                    }
                    jToken = new JObject();
                    config.Add(path[0], jToken);
                }

                for (int i = 1; i < path.Length - 1; i++)
                {
                    var jObject = jToken as JObject;
                    if (jObject == null || !jObject.TryGetValue(path[i], out jToken))
                    {
                        jToken = new JObject();
                        jObject?.Add(path[i], jToken);
                    }
                }
                (jToken as JObject)?.Add(path[path.Length - 1], JToken.FromObject(value));
            }
            catch (Exception ex)
            {
                PrintError($"SetConfigValuePre ERROR: value: {value} path: {string.Join("\\", path)}\n{ex}");
            }
        }

        private bool GetConfigVersionPre(JObject config, out VersionNumber version)
        {
            try
            {
                JToken jToken;
                if (config.TryGetValue("Version", out jToken))
                {
                    version = jToken.ToObject<VersionNumber>();
                    return true;
                }
            }
            catch
            {
                // ignored
            }
            version = default(VersionNumber);
            return false;
        }

        #endregion Preprocess Old Config

        #endregion ConfigurationFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private void Print(ConsoleSystem.Arg arg, string message)
        {
            //SendReply(arg, message);
            var player = arg.Player();
            if (player == null) Puts(message);
            else PrintToConsole(player, message);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You don't have '{0}' permission to use this command.",
                ["TargetDisabled"] = "{0}'s Remover Tool has been disabled.",
                ["TargetEnabled"] = "{0} is now using Remover Tool; Enabled for {1} seconds (Max Removable Objects: {2}, Remove Type: {3}).",
                ["ToolDisabled"] = "Remover Tool has been disabled.",
                ["ToolEnabled"] = "Remover Tool enabled for {0} seconds (Max Removable Objects: {1}, Remove Type: {2}).",
                ["Cooldown"] = "You need to wait {0} seconds before using Remover Tool again.",
                ["CurrentlyDisabled"] = "Remover Tool is currently disabled.",
                ["EntityLimit"] = "Entity limit reached, you have removed {0} entities, Remover Tool was automatically disabled.",
                ["MeleeToolNotHeld"] = "You need to be holding a melee tool in order to use the Remover Tool.",
                ["SpecificToolNotHeld"] = "You need to be holding a specific tool in order to use the Remover Tool.",

                ["StartRemoveAll"] = "Start running RemoveAll, please wait.",
                ["StartRemoveStructure"] = "Start running RemoveStructure, please wait.",
                ["StartRemoveExternal"] = "Start running RemoveExternal, please wait.",
                ["AlreadyRemoveAll"] = "There is already a RemoveAll running, please wait.",
                ["AlreadyRemoveStructure"] = "There is already a RemoveStructure running, please wait.",
                ["AlreadyRemoveExternal"] = "There is already a RemoveExternal running, please wait.",
                ["CompletedRemoveAll"] = "You've successfully removed {0} entities using RemoveAll.",
                ["CompletedRemoveStructure"] = "You've successfully removed {0} entities using RemoveStructure.",
                ["CompletedRemoveExternal"] = "You've successfully removed {0} entities using RemoveExternal.",

                ["CanRemove"] = "You can remove this entity.",
                ["NotEnoughCost"] = "Can't remove: You don't have enough resources.",
                ["EntityDisabled"] = "Can't remove: Server has disabled the entity from being removed.",
                ["DamagedEntity"] = "Can't remove: Server has disabled damaged objects from being removed.",
                ["BeBlocked"] = "Can't remove: An external plugin blocked the usage.",
                ["InvalidEntity"] = "Can't remove: No valid entity targeted.",
                ["NotFoundOrFar"] = "Can't remove: The entity is not found or too far away.",
                ["StorageNotEmpty"] = "Can't remove: The entity storage is not empty.",
                ["RaidBlocked"] = "Can't remove: Raid blocked for {0} seconds.",
                ["NotRemoveAccess"] = "Can't remove: You don't have any rights to remove this.",
                ["NotStructure"] = "Can't remove: The entity is not a structure.",
                ["NotExternalWall"] = "Can't remove: The entity is not an external wall.",
                ["HasStash"] = "Can't remove: There are stashes under the foundation.",
                ["EntityTimeLimit"] = "Can't remove: The entity was built more than {0} seconds ago.",
                //["Can'tOpenAllLocks"] = "Can't remove: There is a lock in the building that you cannot open.",
                ["CantPay"] = "Can't remove: Paying system crashed! Contact an administrator with the time and date to help him understand what happened.",

                ["Refund"] = "Refund:",
                ["Nothing"] = "Nothing",
                ["Price"] = "Price:",
                ["Free"] = "Free",
                ["TimeLeft"] = "Timeleft: {0}s\nRemoved: {1}",
                ["RemoverToolType"] = "Remover Tool ({0})",
                ["Unlimit"] = "∞",

                ["SyntaxError"] = "Syntax error, please type '<color=#ce422b>/{0} <help | h></color>' to view help",
                ["Syntax"] = "<color=#ce422b>/{0} [time (seconds)]</color> - Enable RemoverTool ({1})",
                ["Syntax1"] = "<color=#ce422b>/{0} <admin | a> [time (seconds)]</color> - Enable RemoverTool ({1})",
                ["Syntax2"] = "<color=#ce422b>/{0} <all> [time (seconds)]</color> - Enable RemoverTool ({1})",
                ["Syntax3"] = "<color=#ce422b>/{0} <structure | s> [time (seconds)]</color> - Enable RemoverTool ({1})",
                ["Syntax4"] = "<color=#ce422b>/{0} <external | e> [time (seconds)]</color> - Enable RemoverTool ({1})",
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有 '{0}' 权限来使用该命令",
                ["TargetDisabled"] = "'{0}' 的拆除工具已禁用",
                ["TargetEnabled"] = "'{0}' 的拆除工具已启用 {1} 秒 (可拆除数: {2}, 拆除模式: {3}).",
                ["ToolDisabled"] = "您的拆除工具已禁用",
                ["ToolEnabled"] = "您的拆除工具已启用 {0} 秒 (可拆除数: {1}, 拆除模式: {2}).",
                ["Cooldown"] = "您需要等待 {0} 秒才可以再次使用拆除工具",
                ["CurrentlyDisabled"] = "服务器当前已禁用了拆除工具",
                ["EntityLimit"] = "您已经拆除了 '{0}' 个实体，拆除工具已自动禁用",
                ["MeleeToolNotHeld"] = "您必须拿着近战工具才可以使用拆除工具",
                ["SpecificToolNotHeld"] = "您必须拿着指定工具才可以使用拆除工具",

                ["StartRemoveAll"] = "开始运行 '所有拆除'，请您等待",
                ["StartRemoveStructure"] = "开始运行 '建筑拆除'，请您等待",
                ["StartRemoveExternal"] = "开始运行 '外墙拆除'，请您等待",
                ["AlreadyRemoveAll"] = "已经有一个 '所有拆除' 正在运行，请您等待",
                ["AlreadyRemoveStructure"] = "已经有一个 '建筑拆除' 正在运行，请您等待",
                ["AlreadyRemoveExternal"] = "已经有一个 '外墙拆除' 正在运行，请您等待",
                ["CompletedRemoveAll"] = "您使用 '所有拆除' 成功拆除了 {0} 个实体",
                ["CompletedRemoveStructure"] = "您使用 '建筑拆除' 成功拆除了 {0} 个实体",
                ["CompletedRemoveExternal"] = "您使用 '外墙拆除' 成功拆除了 {0} 个实体",

                ["CanRemove"] = "您可以拆除该实体",
                ["NotEnoughCost"] = "无法拆除该实体: 拆除所需资源不足",
                ["EntityDisabled"] = "无法拆除该实体: 服务器已禁用拆除这种实体",
                ["DamagedEntity"] = "无法拆除该实体: 服务器已禁用拆除已损坏的实体",
                ["BeBlocked"] = "无法拆除该实体: 其他插件阻止您拆除该实体",
                ["InvalidEntity"] = "无法拆除该实体: 无效的实体",
                ["NotFoundOrFar"] = "无法拆除该实体: 没有找到实体或者距离太远",
                ["StorageNotEmpty"] = "无法拆除该实体: 该实体内含有物品",
                ["RaidBlocked"] = "无法拆除该实体: 拆除工具被突袭阻止了 {0} 秒",
                ["NotRemoveAccess"] = "无法拆除该实体: 您无权拆除该实体",
                ["NotStructure"] = "无法拆除该实体: 该实体不是建筑物",
                ["NotExternalWall"] = "无法拆除该实体: 该实体不是外高墙",
                ["HasStash"] = "无法拆除该实体: 地基下藏有小藏匿",
                ["EntityTimeLimit"] = "无法拆除该实体: 该实体的存活时间大于 {0} 秒",
                //["Can'tOpenAllLocks"] = "无法拆除该实体: 该建筑中有您无法打开的锁",
                ["CantPay"] = "无法拆除该实体: 支付失败，请联系管理员，告诉他详情",

                ["Refund"] = "退还:",
                ["Nothing"] = "没有",
                ["Price"] = "价格:",
                ["Free"] = "免费",
                ["TimeLeft"] = "剩余时间: {0}s\n已拆除数: {1} ",
                ["RemoverToolType"] = "拆除工具 ({0})",
                ["Unlimit"] = "∞",

                ["SyntaxError"] = "语法错误，输入 '<color=#ce422b>/{0} <help | h></color>' 查看帮助",
                ["Syntax"] = "<color=#ce422b>/{0} [time (seconds)]</color> - 启用拆除工具 ({1})",
                ["Syntax1"] = "<color=#ce422b>/{0} <admin | a> [time (seconds)]</color> - 启用拆除工具 ({1})",
                ["Syntax2"] = "<color=#ce422b>/{0} <all> [time (seconds)]</color> - 启用拆除工具 ({1})",
                ["Syntax3"] = "<color=#ce422b>/{0} <structure | s> [time (seconds)]</color> - 启用拆除工具 ({1})",
                ["Syntax4"] = "<color=#ce422b>/{0} <external | e> [time (seconds)]</color> - 启用拆除工具 ({1})",
            }, this, "zh-CN");
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "У вас нет разрешения '{0}' чтобы использовать эту команду.",
                ["TargetDisabled"] = "{0}'s Remover Tool отключен.",
                ["TargetEnabled"] = "{0} теперь использует Remover Tool; Включено на {1} секунд (Макс. объектов для удаления: {2}, Тип удаления: {3}).",
                ["ToolDisabled"] = "Remover Tool отключен.",
                ["ToolEnabled"] = "Remover Tool включен на {0} секунд (Макс. объектов для удаления: {1}, Тип удаления: {2}).",
                ["Cooldown"] = "Необходимо подождать {0} секунд, прежде чем использовать Remover Tool снова.",
                ["CurrentlyDisabled"] = "Remover Tool в данный момент отключен.",
                ["EntityLimit"] = "Достигнут предел, удалено {0} объектов, Remover Tool автоматически отключен.",
                ["MeleeToolNotHeld"] = "Вы должны держать Инструмент ближнего боя, чтобы использовать инструмент для удаления.",
                ["SpecificToolNotHeld"] = "Вы должны держать Инструмент определенный, чтобы использовать инструмент для удаления.",

                ["StartRemoveAll"] = "Запускается RemoveAll, пожалуйста, подождите.",
                ["StartRemoveStructure"] = "Запускается RemoveStructure, пожалуйста, подождите.",
                ["StartRemoveExternal"] = "Запускается RemoveExternal, пожалуйста, подождите.",
                ["AlreadyRemoveAll"] = "RemoveAll уже выполняется, пожалуйста, подождите.",
                ["AlreadyRemoveStructure"] = "RemoveStructure уже выполняется, пожалуйста, подождите.",
                ["AlreadyRemoveExternal"] = "RemoveExternal уже выполняется, пожалуйста, подождите.",
                ["CompletedRemoveAll"] = "Вы успешно удалили {0} объектов используя RemoveAll.",
                ["CompletedRemoveStructure"] = "Вы успешно удалили {0} объектов используя RemoveStructure.",
                ["CompletedRemoveExternal"] = "Вы успешно удалили {0} объектов используя RemoveExternal.",

                ["CanRemove"] = "Вы можете удалить этот объект.",
                ["NotEnoughCost"] = "Нельзя удалить: У вас не достаточно ресурсов.",
                ["EntityDisabled"] = "Нельзя удалить: Сервер отключил возможность удаления этого объекта.",
                ["DamagedEntity"] = "Нельзя удалить: Сервер отключил возможность удалять повреждённые объекты.",
                ["BeBlocked"] = "Нельзя удалить: Внешний plugin блокирует использование.",
                ["InvalidEntity"] = "Нельзя удалить: Неверный объект.",
                ["NotFoundOrFar"] = "Нельзя удалить: Объект не найден, либо слишком далеко.",
                ["StorageNotEmpty"] = "Нельзя удалить: Хранилище объекта не пусто.",
                ["RaidBlocked"] = "Нельзя удалить: Рэйд-блок {0} секунд.",
                ["NotRemoveAccess"] = "Нельзя удалить: У вас нет прав удалять это.",
                ["NotStructure"] = "Нельзя удалить: Объект не конструкция.",
                ["NotExternalWall"] = "Нельзя удалить: Объект не внешняя стена.",
                ["HasStash"] = "Нельзя удалить: Обнаружены тайники под фундаментом.",
                ["EntityTimeLimit"] = "Нельзя удалить: Объект был построен более {0} секунд назад.",
                //["Can'tOpenAllLocks"] = "Нельзя удалить: в здании есть замок, который вы не можете открыть",
                ["CantPay"] = "Нельзя удалить: Система оплаты дала сбой! Свяжитесь с админом указав дату и время, чтобы помочь ему понять что случилось.",

                ["Refund"] = "Возврат:",
                ["Nothing"] = "Ничего",
                ["Price"] = "Цена:",
                ["Free"] = "Бесплатно",
                ["TimeLeft"] = "Осталось времени: {0}s\nУдалено: {1}",
                ["RemoverToolType"] = "Remover Tool ({0})",
                ["Unlimit"] = "∞",

                ["SyntaxError"] = "Синтаксическая ошибка! Пожалуйста, введите '<color=#ce422b>/{0} <help | h></color>' для отображения помощи",
                ["Syntax"] = "<color=#ce422b>/{0} [время (секунд)]</color> - Включить RemoverTool ({1})",
                ["Syntax1"] = "<color=#ce422b>/{0} <admin | a> [время (секунд)]</color> - Включить RemoverTool ({1})",
                ["Syntax2"] = "<color=#ce422b>/{0} <all> [время (секунд)]</color> - Включить RemoverTool ({1})",
                ["Syntax3"] = "<color=#ce422b>/{0} <structure | s> [время (секунд)]</color> - Включить RemoverTool ({1})",
                ["Syntax4"] = "<color=#ce422b>/{0} <external | e> [время (секунд)]</color> - Включить RemoverTool ({1})",
            }, this, "ru");
        }

        #endregion LanguageFile
    }
}