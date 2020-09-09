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
    [Info("Custom Auto Kits", "Absolut/Arainrr", "1.2.4", ResourceId = 41234154)]
    [Description("Automatic kits by permission")]
    public class CustomAutoKits : RustPlugin
    {
        [PluginReference] private readonly Plugin EventManager, Kits;
        private readonly Dictionary<ulong, Hash<string, float>> kitCooldown = new Dictionary<ulong, Hash<string, float>>();

        private void Init()
        {
            LoadData();
            cmd.AddChatCommand(configData.chatS.command, this, nameof(CmdChooseKit));
        }

        private void OnServerInitialized()
        {
            foreach (var entry in configData.autoKits)
            {
                if (!permission.PermissionExists(entry.permission, this))
                    permission.RegisterPermission(entry.permission, this);
                foreach (var kitS in entry.kits)
                {
                    if (!IsKit(kitS.kitName))
                    {
                        PrintError($"'{kitS.kitName}' kit does not exist");
                    }
                }
            }
            foreach (var player in BasePlayer.allPlayerList)
                storedData.players.Add(player.userID);
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

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

            var kitName = GetSelectedKit(player);
            if (string.IsNullOrEmpty(kitName)) return;
            if (configData.emptyItem) player.inventory.Strip();
            GiveKit(player, kitName);
            if (!kitCooldown.ContainsKey(player.userID)) kitCooldown.Add(player.userID, new Hash<string, float>());
            kitCooldown[player.userID][kitName] = Time.realtimeSinceStartup;
        }

        #region Methods

        private string GetSelectedKit(BasePlayer player)
        {
            string kitName;
            ConfigData.KitS kitS = null;
            if (storedData.playerPrefs.TryGetValue(player.userID, out kitName))
            {
                if (!string.IsNullOrEmpty(kitName))
                {
                    var availableKits = GetAvailableKits(player);
                    var found = availableKits.FirstOrDefault(x => x.kitName == kitName);
                    kitS = found ?? GetDefaultKit(player);
                }
            }
            else kitS = GetDefaultKit(player);

            if (kitS == null) return string.Empty;
            kitName = kitS.kitName;
            Hash<string, float> playerCooldown;
            if (kitS.cooldown > 0 && kitCooldown.TryGetValue(player.userID, out playerCooldown))
            {
                float lastUse;
                if (playerCooldown.TryGetValue(kitName, out lastUse) && Time.realtimeSinceStartup - lastUse < kitS.cooldown)
                    return kitS.cooldownKit;
            }
            return kitName;
        }

        private ConfigData.KitS GetDefaultKit(BasePlayer player)
        {
            var kits = GetAvailableKits(player);
            return kits.Count > 0 ? kits.OrderByDescending(x => x.priority).FirstOrDefault() : null;
        }

        private List<ConfigData.KitS> GetAvailableKits(BasePlayer player)
        {
            var kits = new List<ConfigData.KitS>();
            foreach (var entry in configData.autoKits)
            {
                if (permission.UserHasPermission(player.UserIDString, entry.permission))
                {
                    kits.AddRange(entry.kits.Where(x => IsKit(x.kitName)));
                }
            }
            return kits;
        }

        private bool IsKit(string kitName) => (bool)(Kits.Call("isKit", kitName) ?? true);

        private object GiveKit(BasePlayer player, string kitName) => Kits.Call("GiveKit", player, kitName);

        #endregion Methods

        #region Commands

        private void CmdChooseKit(BasePlayer player, string command, string[] args)
        {
            var availableKits = GetAvailableKits(player);
            if (availableKits.Count <= 0)
            {
                Print(player, Lang("NoAvailableKits", player.UserIDString));
                return;
            }
            CreateUI(player);
        }

        [ConsoleCommand("CustomAutoKitsUI")]
        private void CCmdCustomAutoKitsUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            switch (arg.Args[0].ToLower())
            {
                case "close":
                    DestroyUI(player);
                    return;

                case "choose":
                    string pref;
                    if (storedData.playerPrefs.TryGetValue(player.userID, out pref))
                    {
                        var kitName = arg.Args[1];
                        if (pref == kitName)
                        {
                            storedData.playerPrefs[player.userID] = string.Empty;
                        }
                        else
                        {
                            storedData.playerPrefs[player.userID] = kitName;
                        }
                    }
                    CreateUI(player);
                    return;
            }
        }

        #endregion Commands

        #region UI

        public class UI
        {
            public static CuiElementContainer CreateElementContainer(string parent, string panelName, string backgroundColor, string anchorMin, string anchorMax, bool cursor = false)
            {
                return new CuiElementContainer()
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

        private const string UINAME_AUTO_KITS = "CustomAutoKitsUI";

        private void CreateUI(BasePlayer player)
        {
            if (player == null) return;
            CuiHelper.DestroyUi(player, UINAME_AUTO_KITS);
            var selectedKitName = GetSelectedKit(player);
            var availableKits = GetAvailableKits(player).Select(x => x.kitName).ToList();
            var container = UI.CreateElementContainer("Hud", UINAME_AUTO_KITS, "0 0 0 0.6", "0.38 0.25", "0.62 0.7", true);
            UI.CreateLabel(ref container, UINAME_AUTO_KITS, "1 1 1 1", Lang("Title", player.UserIDString), 16, "0 0.9", "1 1");
            UI.CreateButton(ref container, UINAME_AUTO_KITS, "1 0 0 0.9", "CustomAutoKitsUI Close", "0 0 0 1", "X", 20, "0.90 0.93", "1 1");

            var spacing = 0.9f / 9;
            for (int i = 0; i < availableKits.Count; i++)
            {
                var kitName = availableKits[i];
                UI.CreateLabel(ref container, UINAME_AUTO_KITS, "0 1 1 1", kitName, 18, $"0.1 {0.9f - (i + 1) * spacing}", $"0.7 {0.9f - i * spacing}", TextAnchor.MiddleLeft);
                UI.CreateButton(ref container, UINAME_AUTO_KITS, "0 0 0 0.7", $"CustomAutoKitsUI Choose {kitName}", "0 0 0 0.5", selectedKitName == kitName ? Lang("Selected", player.UserIDString) : Lang("Unselected", player.UserIDString), 14, $"0.7 {0.9f - (i + 1) * spacing}", $"1 {0.9f - i * spacing}");
            }
            CuiHelper.AddUi(player, container);
        }

        private static void DestroyUI(BasePlayer player) => CuiHelper.DestroyUi(player, UINAME_AUTO_KITS);

        #endregion UI

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Empty default items before give kits")]
            public bool emptyItem = true;

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
                            cooldownKit = "Cooldown Kit",
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
                            cooldownKit = "Cooldown Kit",
                        },
                        new KitS
                        {
                            priority = 2,
                            cooldown = 0,
                            kitName = "KitName21",
                            cooldownKit = "Cooldown Kit",
                        }
                    }
                },
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
                public string prefix = "[CustomAutoKits]: ";

                [JsonProperty(PropertyName = "Chat Prefix Color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
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
            public readonly Hash<ulong, string> playerPrefs = new Hash<ulong, string>();
            public readonly HashSet<ulong> players = new HashSet<ulong>();
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

        private void OnNewSave(string filename)
        {
            storedData.players.Clear();
            SaveData();
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, string.IsNullOrEmpty(configData.chatS.prefix) ? string.Empty : $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoAvailableKits"] = "You don't have any available kits",
                ["Title"] = "Please choose your respawn kit",
                ["Selected"] = "<color=#8ee700>Selected</color>",
                ["Unselected"] = "<color=#ce422b>X</color>",
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoAvailableKits"] = "您没有可用的复活礼包",
                ["Title"] = "请选择您的复活礼包",
                ["Selected"] = "<color=#8ee700>已选择</color>",
                ["Unselected"] = "<color=#ce422b>未选择</color>",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}