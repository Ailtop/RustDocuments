//#define DEBUG

using System;
using System.Collections.Generic;
using ConVar;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Pickup", "Arainrr", "1.2.7")]
    [Description("Automatically pickup hemp, pumpkin, ore, pickable items, corpse, etc.")]
    public class AutoPickup : RustPlugin
    {
        #region Fields

        private static AutoPickup instance;
        private const string PERMISSION_USE = "autopickup.use";
        private static PickupType enabledPickupTypes;
        private readonly HashSet<ulong> autoClonePlayers = new HashSet<ulong>();

        [Flags]
        private enum PickupType
        {
            None = 0,
            PlantEntity = 1,
            CollectibleEntity = 1 << 1,
            MurdererCorpse = 1 << 2,
            ScientistCorpse = 1 << 3,
            PlayerCorpse = 1 << 4,
            ItemDropBackpack = 1 << 5,
            ItemDrop = 1 << 6,
            WorldItem = 1 << 7,
            LootContainer = 1 << 8,
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            UpdateConfig();
            instance = this;
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_USE, this);
            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdAutoPickup));

            foreach (var entry in configData.autoPickupS)
            {
                if (entry.Value.enabled)
                {
                    enabledPickupTypes |= entry.Key;
                }
            }
            if (!enabledPickupTypes.HasFlag(PickupType.LootContainer))
            {
                Unsubscribe(nameof(CanLootEntity));
            }
        }

        private void UpdateConfig()
        {
            foreach (PickupType pickupType in Enum.GetValues(typeof(PickupType)))
            {
                if (pickupType == PickupType.None) continue;
                if (!configData.autoPickupS.ContainsKey(pickupType))
                {
                    configData.autoPickupS.Add(pickupType, new ConfigData.PickupTypeS { enabled = true, radius = 0.5f });
                }
            }
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                if (!configData.worldItemS.itemCategoryS.ContainsKey(itemDefinition.category))
                {
                    configData.worldItemS.itemCategoryS.Add(itemDefinition.category, true);
                }
            }
            foreach (var prefab in GameManifest.Current.entities)
            {
                var entity = GameManager.server.FindPrefab(prefab.ToLower())?.GetComponent<BaseEntity>();
                if (entity == null || string.IsNullOrEmpty(entity.ShortPrefabName)) continue;
                var lootContainer = entity.GetComponent<LootContainer>();
                if (lootContainer != null)
                {
                    if (!configData.lootContainerS.ContainsKey(lootContainer.ShortPrefabName))
                    {
                        configData.lootContainerS.Add(lootContainer.ShortPrefabName, true);
                    }
                    continue;
                }
                var collectibleEntity = entity.GetComponent<CollectibleEntity>();
                if (collectibleEntity != null)
                {
                    if (!configData.collectibleEntityS.ContainsKey(collectibleEntity.ShortPrefabName))
                    {
                        configData.collectibleEntityS.Add(collectibleEntity.ShortPrefabName, true);
                    }
                    continue;
                }
                var plantEntity = entity.GetComponent<GrowableEntity>();
                if (plantEntity != null)
                {
                    if (!configData.plantEntityS.ContainsKey(plantEntity.ShortPrefabName))
                    {
                        configData.plantEntityS.Add(plantEntity.ShortPrefabName, true);
                    }
                    continue;
                }
            }
            SaveConfig();
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                CheckEntity(baseNetworkable);
            }
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void Unload()
        {
            if (AutoPickupHelper.autoPickupHelpers != null)
            {
                foreach (var autoPickupEntity in AutoPickupHelper.autoPickupHelpers.ToArray())
                {
                    UnityEngine.Object.Destroy(autoPickupEntity);
                }
                AutoPickupHelper.autoPickupHelpers.Clear();
            }

            foreach (var player in BasePlayer.activePlayerList)
            {
                DestroyUI(player);
            }
            SaveData();
            instance = null;
            configData = null;
        }

        private object CanLootEntity(BasePlayer player, LootContainer lootContainer)
        {
            if (player == null || lootContainer == null || lootContainer.OwnerID.IsSteamId()) return null;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                bool enabled;
                if (configData.lootContainerS.TryGetValue(lootContainer.ShortPrefabName, out enabled) && !enabled)
                {
                    return null;
                }
                if (configData.settings.preventPickupLoot && lootContainer.OwnerID.IsSteamId() &&
                    lootContainer.OwnerID != player.userID)
                {
                    return null;
                }
                var autoPickData = GetAutoPickupData(player.userID);
                if (autoPickData.enabled && !autoPickData.blockPickupTypes.HasFlag(PickupType.LootContainer))
                {
                    if (CanAutoPickup(player, lootContainer) && PickupLootContainer(player, lootContainer))
                    {
                        return false;
                    }
                }
            }
            return null;
        }

        private void OnEntitySpawned(BaseNetworkable baseNetworkable) => CheckEntity(baseNetworkable, true);

        #endregion Oxide Hooks

        #region Methods

        private static void CheckEntity(BaseNetworkable baseNetworkable, bool justCreated = false)
        {
            if (baseNetworkable == null) return;
            PickupType pickupType;
            switch (baseNetworkable.ShortPrefabName)
            {
                case "murderer_corpse": pickupType = PickupType.MurdererCorpse; break;
                case "scientist_corpse": pickupType = PickupType.ScientistCorpse; break;
                case "player_corpse": pickupType = PickupType.PlayerCorpse; break;
                case "item_drop": pickupType = PickupType.ItemDrop; break;
                case "item_drop_backpack": pickupType = PickupType.ItemDropBackpack; break;
                default:
                    if (baseNetworkable is GrowableEntity)
                    {
                        pickupType = PickupType.PlantEntity;
                        break;
                    }
                    if (baseNetworkable is CollectibleEntity)
                    {
                        pickupType = PickupType.CollectibleEntity;
                        break;
                    }
                    if (baseNetworkable is WorldItem)
                    {
                        pickupType = PickupType.WorldItem;
                        break;
                    }
                    return;
            }
            if (!enabledPickupTypes.HasFlag(pickupType))
            {
                return;
            }
            var autoPickupEntity = baseNetworkable.gameObject.GetComponent<AutoPickupHelper>();
            if (autoPickupEntity != null)
            {
                UnityEngine.Object.Destroy(autoPickupEntity);
            }

            switch (pickupType)
            {
                case PickupType.CollectibleEntity:
                    {
                        bool enabled;
                        if (configData.collectibleEntityS.TryGetValue(baseNetworkable.ShortPrefabName, out enabled) && !enabled)
                        {
                            return;
                        }
                    }
                    break;

                case PickupType.PlantEntity:
                    {
                        bool enabled;
                        if (configData.plantEntityS.TryGetValue(baseNetworkable.ShortPrefabName, out enabled) && !enabled)
                        {
                            return;
                        }
                    }
                    break;

                case PickupType.MurdererCorpse:
                case PickupType.ScientistCorpse:
                case PickupType.PlayerCorpse:
                case PickupType.ItemDrop:
                case PickupType.ItemDropBackpack:
                    break;

                case PickupType.WorldItem:
                    {
                        var worldItem = baseNetworkable as WorldItem;
                        if (worldItem != null)
                        {
                            var item = worldItem.GetItem();
                            if (item != null)
                            {
                                if (configData.worldItemS.itemBlockList.Contains(item.info.shortname)) return;
                                bool enabled;
                                if (configData.worldItemS.itemCategoryS.TryGetValue(item.info.category, out enabled) && !enabled)
                                {
                                    return;
                                }
                            }
                        }
                        if (justCreated)
                        {
                            var collisionDetection = baseNetworkable.gameObject.GetComponent<WorldItemCollisionDetection>();
                            if (collisionDetection != null)
                            {
                                UnityEngine.Object.Destroy(collisionDetection);
                            }
                            baseNetworkable.gameObject.AddComponent<WorldItemCollisionDetection>();
                            return;
                        }
                    }
                    break;
            }
            CreateAutoPickupHelper(baseNetworkable.gameObject, pickupType);
        }

        private static void CreateAutoPickupHelper(GameObject entity, PickupType pickupType)
        {
            var newObject = new GameObject("AutoPickHelper");
            newObject.transform.SetParent(entity.transform);
            newObject.transform.position = entity.transform.position;
            newObject.AddComponent<AutoPickupHelper>().SetPickupType(pickupType);
        }

        private static bool PickupLootContainer(BasePlayer player, LootContainer lootContainer)
        {
            var itemContainer = lootContainer?.inventory;
            if (itemContainer != null)
            {
                for (int i = itemContainer.itemList.Count - 1; i >= 0; i--)
                {
                    player.GiveItem(itemContainer.itemList[i], BaseEntity.GiveItemReason.PickedUp);
                }
                if (itemContainer.itemList == null || itemContainer.itemList.Count <= 0)
                {
                    lootContainer.Kill();
                }
                return true;
            }
            return false;
        }

        private static bool InventoryExistItem(BasePlayer player, Item item)
        {
            return player.inventory.containerMain.FindItemByItemID(item.info.itemid) != null
                   || player.inventory.containerBelt.FindItemByItemID(item.info.itemid) != null;
        }

        private static bool InventoryIsFull(BasePlayer player, Item item)
        {
            if (player.inventory.containerMain.IsFull() && player.inventory.containerBelt.IsFull())
            {
                var item1 = player.inventory.containerMain.FindItemByItemID(item.info.itemid);
                var item2 = player.inventory.containerBelt.FindItemByItemID(item.info.itemid);
                return (item1 == null || !item.CanStack(item1)) && (item2 == null || !item.CanStack(item2));
            }
            return false;
        }

        private static bool PickupDroppedItemContainer(BasePlayer player, DroppedItemContainer droppedItemContainer)
        {
            var itemContainer = droppedItemContainer?.inventory;
            if (itemContainer != null)
            {
                for (int i = itemContainer.itemList.Count - 1; i >= 0; i--)
                {
                    player.GiveItem(itemContainer.itemList[i], BaseEntity.GiveItemReason.PickedUp);
                }
                if (itemContainer.itemList == null || itemContainer.itemList.Count <= 0)
                {
                    droppedItemContainer.Kill();
                }
                return true;
            }
            return false;
        }

        private static bool PickupPlayerCorpse(BasePlayer player, PlayerCorpse playerCorpse)
        {
            var itemContainers = playerCorpse?.containers;
            if (itemContainers != null)
            {
                for (int i = itemContainers.Length - 1; i >= 0; i--)
                {
                    var itemContainer = itemContainers[i];
                    if (itemContainer != null)
                    {
                        for (int j = itemContainer.itemList.Count - 1; j >= 0; j--)
                        {
                            player.GiveItem(itemContainer.itemList[j], BaseEntity.GiveItemReason.PickedUp);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private static bool CanAutoPickup(BasePlayer player, BaseEntity entity)
        {
            return entity != null && player.CanInteract() && Interface.CallHook("OnAutoPickupEntity", player, entity) == null;
        }

        #endregion Methods

        private class WorldItemCollisionDetection : MonoBehaviour
        {
            private bool waiting;

            private void OnCollisionEnter(Collision collision)
            {
                if (waiting || collision?.gameObject?.layer == null) return;
                waiting = true;
                Invoke(nameof(AddAutoPickupComponent), configData.worldItemS.pickupDelay);
            }

            private void AddAutoPickupComponent()
            {
                CreateAutoPickupHelper(gameObject, PickupType.WorldItem);
                DestroyImmediate(this);
            }
        }

        private class AutoPickupHelper : FacepunchBehaviour
        {
            public static List<AutoPickupHelper> autoPickupHelpers;

            private BaseEntity entity;
            private PickupType pickupType;
            private SphereCollider sphereCollider;

            private void Awake()
            {
                if (autoPickupHelpers == null)
                {
                    autoPickupHelpers = new List<AutoPickupHelper>();
                }
                autoPickupHelpers.Add(this);
                entity = GetComponentInParent<BaseEntity>();
            }

            public void SetPickupType(PickupType pickupType)
            {
                this.pickupType = pickupType;
                transform.position = entity.CenterPoint();
                CreateCollider();
            }

            private void CreateCollider()
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.gameObject.layer = (int)Rust.Layer.Reserved1;
                sphereCollider.radius = configData.autoPickupS[pickupType].radius;
                sphereCollider.isTrigger = true;
#if DEBUG
                InvokeRepeating(Tick, 0f, 1f);
#endif
            }

#if DEBUG

            private void Tick()
            {
                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (player.IsAdmin && Vector3.Distance(transform.position, player.transform.position) < 50f)
                    {
                        player.SendConsoleCommand("ddraw.sphere", 1, Color.cyan, sphereCollider.transform.position, sphereCollider.radius);
                    }
                }
            }

#endif

            private void OnTriggerEnter(Collider collider)
            {
                if (collider?.gameObject?.layer != (int)Rust.Layer.Player_Server) return;
                var player = collider.gameObject.GetComponentInParent<BasePlayer>();
                if (player == null || !player.userID.IsSteamId()) return;
                if (instance.permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
                {
                    var autoPickData = instance.GetAutoPickupData(player.userID);
                    if (autoPickData.enabled && !autoPickData.blockPickupTypes.HasFlag(pickupType))
                    {
                        switch (pickupType)
                        {
                            case PickupType.PlantEntity:
                                var plantEntity = entity as GrowableEntity;
                                if (configData.settings.preventPickupPlant && plantEntity.OwnerID.IsSteamId() && plantEntity.OwnerID != player.userID) return;
                                if (CanAutoPickup(player, plantEntity))
                                {
                                    if (instance.autoClonePlayers.Contains(player.userID))
                                    {
                                        var rPCMessage = default(BaseEntity.RPCMessage);
                                        rPCMessage.player = player;
                                        plantEntity.RPC_TakeClone(rPCMessage);
                                    }
                                    else
                                    {
                                        plantEntity.PickFruit(player);
                                    }
                                    plantEntity.RemoveDying(player);
                                }
                                return;

                            case PickupType.CollectibleEntity:
                                var collectibleEntity = entity as CollectibleEntity;
                                if (CanAutoPickup(player, collectibleEntity))
                                {
                                    collectibleEntity.DoPickup(player);
                                    Destroy(this);
                                }
                                return;

                            case PickupType.ItemDrop:
                            case PickupType.ItemDropBackpack:
                                var droppedItemContainer = entity as DroppedItemContainer;
                                if (configData.settings.preventPickupBackpack && droppedItemContainer.playerSteamID.IsSteamId() && droppedItemContainer.playerSteamID != player.userID) return;
                                if (CanAutoPickup(player, droppedItemContainer))
                                {
                                    if (PickupDroppedItemContainer(player, droppedItemContainer))
                                    {
                                        Destroy(this);
                                    }
                                }
                                return;

                            case PickupType.MurdererCorpse:
                            case PickupType.ScientistCorpse:
                            case PickupType.PlayerCorpse:
                                var playerCorpse = entity as PlayerCorpse;
                                if (!playerCorpse.CanLoot()) return;
                                if (configData.settings.preventPickupCorpse && playerCorpse.playerSteamID.IsSteamId() && playerCorpse.playerSteamID != player.userID) return;
                                if (CanAutoPickup(player, playerCorpse))
                                {
                                    if (PickupPlayerCorpse(player, playerCorpse))
                                    {
                                        Destroy(this);
                                    }
                                }
                                return;

                            case PickupType.WorldItem:
                                var worldItem = entity as WorldItem;
                                if (CanAutoPickup(player, worldItem))
                                {
                                    var item = worldItem.GetItem();
                                    if (item != null)
                                    {
                                        if (configData.worldItemS.onlyPickupExistItem && !InventoryExistItem(player, item)) return;
                                        if (configData.worldItemS.checkInventoryFull && InventoryIsFull(player, item)) return;
                                        if (worldItem.allowPickup && Interface.CallHook("OnItemPickup", item, player) == null)
                                        {
                                            worldItem.ClientRPC(null, "PickupSound");
                                            player.GiveItem(item, BaseEntity.GiveItemReason.PickedUp);
                                            player.SignalBroadcast(BaseEntity.Signal.Gesture, "pickup_item");
                                            Destroy(this);
                                        }
                                    }
                                }
                                return;

                            default:
                                return;
                        }
                    }
                }
            }

            private void OnDestroy()
            {
#if DEBUG
                CancelInvoke(Tick);
#endif
                Destroy(gameObject);
                autoPickupHelpers?.Remove(this);
            }
        }

        private StoredData.AutoPickData GetAutoPickupData(ulong playerID)
        {
            StoredData.AutoPickData autoPickData;
            if (!storedData.playerAutoPickupData.TryGetValue(playerID, out autoPickData))
            {
                autoPickData = new StoredData.AutoPickData
                {
                    enabled = configData.settings.defaultEnabled
                };
                storedData.playerAutoPickupData.Add(playerID, autoPickData);
            }

            return autoPickData;
        }

        #region UI

        public class UI
        {
            public static CuiElementContainer CreateElementContainer(string parent, string panelName, string backgroundColor, string anchorMin, string anchorMax, bool cursor = false)
            {
                return new CuiElementContainer
                {
                    {
                        new CuiPanel
                        {
                            Image = { Color = backgroundColor },
                            RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
                            CursorEnabled = cursor
                        },
                        new CuiElement().Parent = parent,
                        panelName
                    }
                };
            }

            public static void CreateLabel(ref CuiElementContainer container, string panelName, string textColor, string text, int fontSize, string anchorMin, string anchorMax, TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiLabel
                {
                    Text = { Color = textColor, FontSize = fontSize, Align = align, Text = text },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
                }, panelName, CuiHelper.GetGuid());
            }

            public static void CreateButton(ref CuiElementContainer container, string panelName, string buttonColor, string command, string textColor, string text, int fontSize, string anchorMin, string anchorMax, string close = "", TextAnchor align = TextAnchor.MiddleCenter)
            {
                container.Add(new CuiButton
                {
                    Button = { Color = buttonColor, Command = command, Close = close },
                    RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
                    Text = { Color = textColor, Text = text, FontSize = fontSize, Align = align }
                }, panelName, CuiHelper.GetGuid());
            }
        }

        private const string UINAME_AUTO_PICKUP = "AutoPickupUI";

        private static void CreateUI(BasePlayer player, PickupType blockTypes)
        {
            if (player == null) return;
            var container = UI.CreateElementContainer("Hud", UINAME_AUTO_PICKUP, "0 0 0 0.6", "0.38 0.25", "0.62 0.7", true);
            UI.CreateLabel(ref container, UINAME_AUTO_PICKUP, "1 1 1 1", instance.Lang("Title", player.UserIDString), 20, "0.2 0.9", "0.8 1");
            UI.CreateButton(ref container, UINAME_AUTO_PICKUP, "1 0 0 0.9", "", "0 0 0 1", "X", 18, "0.90 0.93", "1 1", UINAME_AUTO_PICKUP);

            int i = 0;
            var spacing = 0.9f / 10;
            foreach (PickupType pickupType in Enum.GetValues(typeof(PickupType)))
            {
                if (pickupType == PickupType.None || !enabledPickupTypes.HasFlag(pickupType)) continue;
                var anchors = GetAnchors(i, spacing);
                UI.CreateLabel(ref container, UINAME_AUTO_PICKUP, "0 1 1 1",
                    instance.Lang(pickupType.ToString(), player.UserIDString), 12, $"0.1 {anchors[0]}",
                    $"0.8 {anchors[1]}", TextAnchor.MiddleLeft);
                UI.CreateButton(ref container, UINAME_AUTO_PICKUP, "0 0 0 0.7", $"AutoPickupUI {pickupType}",
                    "0 0 0 0.5",
                    blockTypes.HasFlag(pickupType)
                        ? instance.Lang("Disabled", player.UserIDString)
                        : instance.Lang("Enabled", player.UserIDString), 12, $"0.8 {anchors[0]}",
                    $"1 {anchors[1]}");
                if (pickupType == PickupType.PlantEntity)
                {
                    i++;
                    anchors = GetAnchors(i, spacing);
                    UI.CreateLabel(ref container, UINAME_AUTO_PICKUP, "0 1 1 1",
                        instance.Lang("AutoClonePlants", player.UserIDString), 12, $"0.1 {anchors[0]}",
                        $"0.8 {anchors[1]}", TextAnchor.MiddleLeft);
                    UI.CreateButton(ref container, UINAME_AUTO_PICKUP, "0 0 0 0.7", $"AutoPickupUI ClonePlants",
                        "0 0 0 0.5",
                        instance.autoClonePlayers.Contains(player.userID)
                            ? instance.Lang("Enabled", player.UserIDString)
                            : instance.Lang("Disabled", player.UserIDString), 12, $"0.8 {anchors[0]}",
                        $"1 {anchors[1]}");
                }
                i++;
            }

            CuiHelper.DestroyUi(player, UINAME_AUTO_PICKUP);
            CuiHelper.AddUi(player, container);
        }

        private static float[] GetAnchors(int i, float spacing)
        {
            return new[] { 0.9f - (i + 1) * spacing, 0.9f - i * spacing };
        }

        private static void DestroyUI(BasePlayer player) => CuiHelper.DestroyUi(player, UINAME_AUTO_PICKUP);

        #endregion UI

        #region Commands

        [ConsoleCommand("AutoPickupUI")]
        private void CCmdAutoPickupUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            var autoPickData = GetAutoPickupData(player.userID);
            PickupType pickupType;
            if (Enum.TryParse(arg.Args[0], true, out pickupType))
            {
                if (autoPickData.blockPickupTypes.HasFlag(pickupType))
                {
                    autoPickData.blockPickupTypes &= ~pickupType;
                }
                else
                {
                    autoPickData.blockPickupTypes |= pickupType;
                }
            }
            else
            {
                if (autoClonePlayers.Contains(player.userID))
                {
                    autoClonePlayers.Remove(player.userID);
                }
                else
                {
                    autoClonePlayers.Add(player.userID);
                }
            }
            CreateUI(player, autoPickData.blockPickupTypes);
        }

        private void CmdAutoPickup(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            var autoPickData = GetAutoPickupData(player.userID);
            if (args == null || args.Length == 0)
            {
                autoPickData.enabled = !autoPickData.enabled;
                Print(player, Lang("AutoPickup", player.UserIDString, autoPickData.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                return;
            }
            CreateUI(player, autoPickData.blockPickupTypes);
        }

        #endregion Commands

        #region ConfigurationFile

        private static ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public Settings settings = new Settings();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatS chatS = new ChatS();

            [JsonProperty(PropertyName = "Auto Pickup Settings")]
            public Dictionary<PickupType, PickupTypeS> autoPickupS = new Dictionary<PickupType, PickupTypeS>();

            [JsonProperty(PropertyName = "World Item Pickup Settings")]
            public WorldItemPickupS worldItemS = new WorldItemPickupS();

            [JsonProperty(PropertyName = "Loot Container Pickup Settings")]
            public Dictionary<string, bool> lootContainerS = new Dictionary<string, bool>();

            [JsonProperty(PropertyName = "Collectible Entity Pickup Settings")]
            public Dictionary<string, bool> collectibleEntityS = new Dictionary<string, bool>();

            [JsonProperty(PropertyName = "Plant Entity Pickup Settings")]
            public Dictionary<string, bool> plantEntityS = new Dictionary<string, bool>();

            public class Settings
            {
                [JsonProperty(PropertyName = "Auto pickup is enabled by default")]
                public bool defaultEnabled = true;

                [JsonProperty(PropertyName = "Prevent pickup other player's backpack")]
                public bool preventPickupBackpack;

                [JsonProperty(PropertyName = "Prevent pickup other player's corpse")]
                public bool preventPickupCorpse;

                [JsonProperty(PropertyName = "Prevent pickup other player's plant entity")]
                public bool preventPickupPlant;

                [JsonProperty(PropertyName = "Prevent pickup other player's loot container")]
                public bool preventPickupLoot = true;
            }

            public class ChatS
            {
                [JsonProperty(PropertyName = "Chat Command")]
                public string command = "ap";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "[AutoPickup]:";

                [JsonProperty(PropertyName = "Chat Prefix Color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            public class PickupTypeS
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled = true;

                [JsonProperty(PropertyName = "Check Radius")]
                public float radius = 0.5f;
            }

            public class WorldItemPickupS
            {
                [JsonProperty(PropertyName = "Auto Pickup Delay")]
                public float pickupDelay = 0.5f;

                [JsonProperty(PropertyName = "Check that player's inventory is full")]
                public bool checkInventoryFull = true;

                [JsonProperty(PropertyName = "Only pickup items that exist in player's inventory")]
                public bool onlyPickupExistItem;

                [JsonProperty(PropertyName = "Item Block List (Item shortname)")]
                public HashSet<string> itemBlockList = new HashSet<string>();

                [JsonProperty(PropertyName = "Allow Pickup Item Category")]
                public Dictionary<ItemCategory, bool> itemCategoryS = new Dictionary<ItemCategory, bool>();
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

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public readonly Dictionary<ulong, AutoPickData> playerAutoPickupData = new Dictionary<ulong, AutoPickData>();

            public class AutoPickData
            {
                public bool enabled;

                //[JsonConverter(typeof(StringEnumConverter))]
                public PickupType blockPickupTypes;
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

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message,
                string.IsNullOrEmpty(configData.chatS.prefix)
                    ? string.Empty
                    : $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>",
                configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command",
                ["Enabled"] = "<color=#8ee700>Enabled</color>",
                ["Disabled"] = "<color=#ce422b>Disabled</color>",
                ["AutoPickup"] = "Automatically pickup is now {0}",
                ["Title"] = "Auto Pickup UI",
                ["PlantEntity"] = "Auto Pickup Plant Entity",
                ["CollectibleEntity"] = "Auto Pickup Collectible Entity",
                ["MurdererCorpse"] = "Auto Pickup Murderer Corpse",
                ["ScientistCorpse"] = "Auto Pickup Scientist Corpse",
                ["PlayerCorpse"] = "Auto Pickup Player Corpse",
                ["ItemDropBackpack"] = "Auto Pickup Item Drop Backpack",
                ["ItemDrop"] = "Auto Pickup Item Drop",
                ["WorldItem"] = "Auto Pickup World Item",
                ["LootContainer"] = "Auto Pickup Loot Container",
                ["AutoClonePlants"] = "Auto Clone Plants",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有使用该命令的权限",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>",
                ["AutoPickup"] = "自动拾取当前状态为 {0}",
                ["Title"] = "自动拾取设置",
                ["PlantEntity"] = "自动拾取农作物",
                ["CollectibleEntity"] = "自动拾取收藏品",
                ["MurdererCorpse"] = "自动拾取僵尸尸体",
                ["ScientistCorpse"] = "自动拾取科学家尸体",
                ["PlayerCorpse"] = "自动拾取玩家尸体",
                ["ItemDropBackpack"] = "自动拾取尸体背包",
                ["ItemDrop"] = "自动拾取掉落容器",
                ["WorldItem"] = "自动拾取掉落物品",
                ["LootContainer"] = "自动拾取战利品容器",
                ["AutoClonePlants"] = "自动克隆植物",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}