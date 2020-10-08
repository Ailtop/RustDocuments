//Requires: Kits

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Custom Auto Kits", "Absolut/Arainrr", "1.2.5", ResourceId = 41234154)]
    [Description("Automatic kits by permission")]
    public class CustomAutoKits : RustPlugin
    {
        [PluginReference] private readonly Plugin EventManager, Kits;

        private readonly Dictionary<ulong, Hash<string, float>> kitCooldown =
            new Dictionary<ulong, Hash<string, float>>();

        private void Init()
        {
            LoadData();
            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdChooseKit));
        }

        private void OnServerInitialized()
        {
            foreach (var autoKit in configData.autoKits)
            {
                if (!permission.PermissionExists(autoKit.permission, this))
                    permission.RegisterPermission(autoKit.permission, this);
                foreach (var kitS in autoKit.kits)
                    if (!IsKit(kitS.kitName))
                        PrintError($"'{kitS.kitName}' kit does not exist");
            }

            foreach (var player in BasePlayer.allPlayerList)
                storedData.players.Add(player.userID);
        }

        private void OnServerSave()
        {
            timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
                DestroyUI(player);
            SaveData();
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (player == null || !player.userID.IsSteamId()) return;
            if (!storedData.players.Add(player.userID)) return;
            OnPlayerRespawned(player);
        }

        private void OnPlayerRespawned(BasePlayer player)
        {
            var isPlaying = EventManager?.CallHook("isPlaying", player);
            if (isPlaying is bool && (bool)isPlaying) return;
            var playerData = GetPlayerData(player.userID, true);
            if (!playerData.enabled) return;
            var kitName = GetSelectedKit(player, playerData);
            if (string.IsNullOrEmpty(kitName)) return;
            if (configData.emptyInventory) player.inventory.Strip();
            GiveKit(player, kitName);

            Hash<string, float> cooldowns;
            if (kitCooldown.TryGetValue(player.userID, out cooldowns)) cooldowns[kitName] = Time.realtimeSinceStartup;
            else kitCooldown.Add(player.userID, new Hash<string, float> { { kitName, Time.realtimeSinceStartup } });
        }

        #region Methods

        private string GetSelectedKit(BasePlayer player, StoredData.PlayerData playerData)
        {
            string kitName;
            ConfigData.KitS kitS;
            var availableKits = GetAvailableKits(player);
            if (!availableKits.Any()) return null;
            if (!string.IsNullOrEmpty(playerData.selectedKit))
            {
                var found = availableKits.FirstOrDefault(x => x.kitName == playerData.selectedKit);
                if (found != null)
                {
                    kitS = found;
                }
                else
                {
                    playerData.selectedKit = null;
                    kitS = GetDefaultKit(availableKits);
                }
            }
            else
            {
                kitS = GetDefaultKit(availableKits);
            }

            if (kitS == null) return null;
            kitName = kitS.kitName;
            Hash<string, float> cooldowns;
            if (kitS.cooldown > 0 && kitCooldown.TryGetValue(player.userID, out cooldowns))
            {
                float lastUse;
                if (cooldowns.TryGetValue(kitName, out lastUse) && Time.realtimeSinceStartup - lastUse < kitS.cooldown)
                    return kitS.cooldownKit;
            }

            return kitName;
        }

        private static ConfigData.KitS GetDefaultKit(IEnumerable<ConfigData.KitS> availableKits)
        {
            return availableKits.OrderByDescending(x => x.priority).First();
        }

        private IEnumerable<ConfigData.KitS> GetAvailableKits(BasePlayer player)
        {
            return from entry in configData.autoKits
                   where permission.UserHasPermission(player.UserIDString, entry.permission)
                   from kitS in entry.kits
                   where IsKit(kitS.kitName)
                   select kitS;
        }

        private bool IsKit(string kitName)
        {
            return (bool)(Kits.Call("isKit", kitName) ?? true);
        }

        private void GiveKit(BasePlayer player, string kitName)
        {
            Kits.Call("GiveKit", player, kitName);
        }

        private StoredData.PlayerData GetPlayerData(ulong playerID, bool readOnly = false)
        {
            StoredData.PlayerData playerData;
            if (!storedData.playerPrefs.TryGetValue(playerID, out playerData))
            {
                playerData = new StoredData.PlayerData { enabled = true };
                if (readOnly) return playerData;
                storedData.playerPrefs.Add(playerID, playerData);
            }

            return playerData;
        }

        #endregion Methods

        #region Commands

        private void CmdChooseKit(BasePlayer player, string command, string[] args)
        {
            var availableKits = GetAvailableKits(player);
            if (!availableKits.Any())
            {
                Print(player, Lang("NoAvailableKits", player.UserIDString));
                return;
            }

            CreateMainUI(player);
        }

        [ConsoleCommand("CustomAutoKitsUI")]
        private void CCmdCustomAutoKitsUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            var playerData = GetPlayerData(player.userID);
            switch (arg.Args[0].ToLower())
            {
                case "toggle":
                    playerData.enabled = !playerData.enabled;
                    UpdateMenuUI(player, playerData);
                    return;

                case "choose":
                    var kitName = arg.Args[1];
                    playerData.selectedKit = playerData.selectedKit == kitName ? null : kitName;
                    UpdateMenuUI(player, playerData);
                    return;
            }
        }

        #endregion Commands

        #region UI

        private const string UINAME_MAIN = "CustomAutoKitsUI_Main";
        private const string UINAME_MENU = "CustomAutoKitsUI_Menu";

        private void CreateMainUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0" },
                RectTransform =
                    {AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-210 -180", OffsetMax = "210 220"},
                CursorEnabled = true
            }, "Hud", UINAME_MAIN);
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.6" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1" }
            }, UINAME_MAIN);
            var titlePanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.42 0.88 0.88 1" },
                RectTransform = { AnchorMin = "0 0.902", AnchorMax = "0.995 1" }
            }, UINAME_MAIN);
            container.Add(new CuiElement
            {
                Parent = titlePanel,
                Components =
                {
                    new CuiTextComponent
                    {
                        Text = Lang("Title", player.UserIDString), FontSize = 20, Align = TextAnchor.MiddleCenter,
                        Color = "1 0 0 1"
                    },
                    new CuiOutlineComponent {Distance = "0.5 0.5", Color = "1 1 1 1"},
                    new CuiRectTransformComponent {AnchorMin = "0.2 0", AnchorMax = "0.8 1"}
                }
            });
            container.Add(new CuiButton
            {
                Button = { Color = "0.95 0.1 0.1 0.95", Close = UINAME_MAIN },
                Text = { Text = "X", Align = TextAnchor.MiddleCenter, Color = "0 0 0 1", FontSize = 22 },
                RectTransform = { AnchorMin = "0.885 0.05", AnchorMax = "0.995 0.95" }
            }, titlePanel);
            CuiHelper.DestroyUi(player, UINAME_MAIN);
            CuiHelper.AddUi(player, container);
            var playerData = GetPlayerData(player.userID, true);
            UpdateMenuUI(player, playerData);
        }

        private void UpdateMenuUI(BasePlayer player, StoredData.PlayerData playerData)
        {
            if (player == null) return;
            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.4" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.898" }
            }, UINAME_MAIN, UINAME_MENU);
            var selectedKitName = GetSelectedKit(player, playerData);
            var availableKits = GetAvailableKits(player);
            var i = 0;
            var spacing = 1f / 10;
            var anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, $"CustomAutoKitsUI Toggle", Lang("Status", player.UserIDString),
                playerData.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString),
                $"0 {anchors[0]}", $"1 {anchors[1]}");
            foreach (var kitS in availableKits)
            {
                var kitName = kitS.kitName;
                anchors = GetEntryAnchors(i++, spacing);
                CreateEntry(ref container, $"CustomAutoKitsUI Choose {kitName}", kitName,
                    selectedKitName == kitName
                        ? Lang("Selected", player.UserIDString)
                        : Lang("Unselected", player.UserIDString), $"0 {anchors[0]}", $"1 {anchors[1]}");
            }

            CuiHelper.DestroyUi(player, UINAME_MENU);
            CuiHelper.AddUi(player, container);
        }

        private static void CreateEntry(ref CuiElementContainer container, string command, string leftText,
            string rightText, string anchorMin, string anchorMax)
        {
            var panelName = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax }
            }, UINAME_MENU);
            container.Add(new CuiLabel
            {
                Text = { Color = "0 1 1 1", FontSize = 14, Align = TextAnchor.MiddleLeft, Text = leftText },
                RectTransform = { AnchorMin = "0.1 0", AnchorMax = "0.795 1" }
            }, panelName);
            container.Add(new CuiButton
            {
                Button = { Color = "0 0 0 0.7", Command = command },
                Text = { Text = rightText, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 14 },
                RectTransform = { AnchorMin = "0.8 0.01", AnchorMax = "0.995 0.99" }
            }, panelName);
        }

        private static float[] GetEntryAnchors(int i, float spacing)
        {
            return new[] { 1f - (i + 1) * spacing, 1f - i * spacing };
        }

        private static void DestroyUI(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, UINAME_MAIN);
        }

        #endregion UI

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Empty default items before give kits")]
            public bool emptyInventory = true;

            [JsonProperty(PropertyName = "Auto Kits", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<AutoKit> autoKits = new List<AutoKit>
            {
                new AutoKit
                {
                    permission = "customautokits.vip1",
                    kits = new List<KitS>
                    {
                        new KitS
                        {
                            priority = 0,
                            cooldown = 0,
                            kitName = "KitName1",
                            cooldownKit = "Cooldown Kit"
                        }
                    }
                },
                new AutoKit
                {
                    permission = "customautokits.vip2",
                    kits = new List<KitS>
                    {
                        new KitS
                        {
                            priority = 1,
                            cooldown = 0,
                            kitName = "KitName2",
                            cooldownKit = "Cooldown Kit"
                        },
                        new KitS
                        {
                            priority = 2,
                            cooldown = 0,
                            kitName = "KitName21",
                            cooldownKit = "Cooldown Kit"
                        }
                    }
                }
            };

            public class AutoKit
            {
                public string permission = string.Empty;
                public List<KitS> kits = new List<KitS>();
            }

            public class KitS
            {
                public int priority;
                public float cooldown;
                public string kitName = string.Empty;
                public string cooldownKit = string.Empty;
            }

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat Command")]
                public string command = "autokit";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "<color=#00FFFF>[CustomAutoKits]</color>: ";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version = new VersionNumber(1, 2, 4);
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                    LoadDefaultConfig();
                else
                    UpdateConfigValues();
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
            Config.WriteObject(configData);
        }

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= new VersionNumber(1, 2, 4))
                {
                    if (configData.chatS.prefix == "[CustomAutoKits]: ")
                    {
                        configData.chatS.prefix = "<color=#00FFFF>[CustomAutoKits]</color>: ";
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
            public readonly Dictionary<ulong, PlayerData> playerPrefs = new Dictionary<ulong, PlayerData>();
            public readonly HashSet<ulong> players = new HashSet<ulong>();

            public class PlayerData
            {
                public bool enabled;
                public string selectedKit;
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

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);
        }

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        private void OnNewSave(string filename)
        {
            storedData.players.Clear();
            SaveData();
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args)
        {
            return string.Format(lang.GetMessage(key, this, id), args);
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoAvailableKits"] = "You don't have any available kits",
                ["Title"] = "Custom Auto Kits UI",
                ["Status"] = "Respawn kit status",
                ["Selected"] = "<color=#8ee700>Selected</color>",
                ["Unselected"] = "<color=#ce422b>X</color>",
                ["Enabled"] = "<color=#8ee700>Enabled</color>",
                ["Disabled"] = "<color=#ce422b>Disabled</color>"
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoAvailableKits"] = "您没有可用的复活礼包",
                ["Title"] = "自定义复活礼包",
                ["Status"] = "复活礼包状态",
                ["Selected"] = "<color=#8ee700>已选择</color>",
                ["Unselected"] = "<color=#ce422b>未选择</color>",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}