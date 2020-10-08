using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Auto Doors", "Wulf/lukespragg/Arainrr", "3.2.8", ResourceId = 1924)]
    [Description("Automatically closes doors behind players after X seconds")]
    public class AutoDoors : RustPlugin
    {
        #region Fields

        private const string PERMISSION_USE = "autodoors.use";
        private readonly Hash<uint, Timer> doorTimers = new Hash<uint, Timer>();
        private readonly Dictionary<string, string> supportedDoors = new Dictionary<string, string>();
        private HashSet<DoorManipulator> doorManipulators;

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_USE, this);
            if (configData.chatS.commands.Length == 0)
            {
                configData.chatS.commands = new[] { "ad" };
            }
            foreach (var command in configData.chatS.commands)
            {
                cmd.AddChatCommand(command, this, nameof(CmdAutoDoor));
            }
        }

        private void OnServerInitialized()
        {
            UpdateConfig();
            if (configData.globalS.excludeDoorController)
            {
                doorManipulators = new HashSet<DoorManipulator>();
                Subscribe(nameof(OnEntitySpawned));
                foreach (var doorManipulator in BaseNetworkable.serverEntities.OfType<DoorManipulator>())
                {
                    OnEntitySpawned(doorManipulator);
                }
            }
        }

        private void OnEntitySpawned(DoorManipulator doorManipulator)
        {
            if (doorManipulator == null || doorManipulator.OwnerID == 0) return;
            doorManipulators.Add(doorManipulator);
        }

        private void OnEntityKill(DoorManipulator doorManipulator)
        {
            if (doorManipulator == null || doorManipulators == null) return;
            doorManipulators.RemoveWhere(x => x == doorManipulator);
        }

        private void OnEntityKill(Door door)
        {
            if (door == null || door.net == null) return;
            var doorID = door.net.ID;
            Timer value;
            if (doorTimers.TryGetValue(doorID, out value))
            {
                value?.Destroy();
                doorTimers.Remove(doorID);
            }
            foreach (var playerData in storedData.playerData.Values)
            {
                playerData.theDoorS.Remove(doorID);
            }
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void Unload()
        {
            foreach (var value in doorTimers.Values)
            {
                value?.Destroy();
            }
            SaveData();
        }

        private void OnDoorOpened(Door door, BasePlayer player)
        {
            if (door == null || door.net == null || !door.IsOpen()) return;
            if (!supportedDoors.ContainsKey(door.ShortPrefabName)) return;
            if (!configData.globalS.useUnownedDoor && door.OwnerID == 0) return;
            if (configData.globalS.excludeDoorController && HasDoorController(door)) return;
            if (configData.usePermission && !permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return;

            var playerData = GetPlayerData(player.userID, true);
            if (!playerData.doorS.enabled) return;
            float autoCloseTime;
            var doorID = door.net.ID;
            StoredData.DoorData doorData;
            if (playerData.theDoorS.TryGetValue(doorID, out doorData))
            {
                if (!doorData.enabled) return;
                autoCloseTime = doorData.time;
            }
            else if (playerData.doorTypeS.TryGetValue(door.ShortPrefabName, out doorData))
            {
                if (!doorData.enabled) return;
                autoCloseTime = doorData.time;
            }
            else autoCloseTime = playerData.doorS.time;

            if (autoCloseTime <= 0) return;
            if (Interface.CallHook("OnDoorAutoClose", player, door) != null) return;

            Timer value;
            if (doorTimers.TryGetValue(doorID, out value))
            {
                value?.Destroy();
            }
            doorTimers[doorID] = timer.Once(autoCloseTime, () =>
            {
                doorTimers.Remove(doorID);
                if (door == null || !door.IsOpen()) return;
                if (configData.globalS.cancelOnKill && player != null && player.IsDead()) return;
                door.SetFlag(BaseEntity.Flags.Open, false);
                door.SendNetworkUpdateImmediate();
            });
        }

        private void OnDoorClosed(Door door, BasePlayer player)
        {
            if (door == null || door.net == null || door.IsOpen()) return;
            Timer value;
            if (doorTimers.TryGetValue(door.net.ID, out value))
            {
                value?.Destroy();
                doorTimers.Remove(door.net.ID);
            }
        }

        #endregion Oxide Hooks

        #region Methods

        private bool HasDoorController(Door door)
        {
            foreach (var doorManipulator in doorManipulators)
            {
                if (doorManipulator != null && doorManipulator.targetDoor == door)
                {
                    return true;
                }
            }
            return false;
        }

        private StoredData.PlayerData GetPlayerData(ulong playerID, bool readOnly = false)
        {
            StoredData.PlayerData playerData;
            if (!storedData.playerData.TryGetValue(playerID, out playerData))
            {
                playerData = new StoredData.PlayerData
                {
                    doorS = new StoredData.DoorData
                    {
                        enabled = configData.globalS.defaultEnabled,
                        time = configData.globalS.defaultDelay,
                    }
                };
                if (readOnly)
                {
                    return playerData;
                }
                storedData.playerData.Add(playerID, playerData);
            }

            return playerData;
        }

        private static Door GetLookingAtDoor(BasePlayer player)
        {
            RaycastHit rHit;
            if (Physics.Raycast(player.eyes.HeadRay(), out rHit, 10f, Rust.Layers.Mask.Construction))
            {
                return rHit.GetEntity() as Door;
            }
            return null;
        }

        private void UpdateConfig()
        {
            foreach (var itemDefinition in ItemManager.GetItemDefinitions())
            {
                var itemModDeployable = itemDefinition.GetComponent<ItemModDeployable>();
                if (itemModDeployable == null) continue;
                var door = GameManager.server.FindPrefab(itemModDeployable.entityPrefab.resourcePath)?.GetComponent<Door>();
                if (door == null || string.IsNullOrEmpty(door.ShortPrefabName)) continue;
                ConfigData.DoorSettings doorSettings;
                if (!configData.doorS.TryGetValue(itemDefinition.shortname, out doorSettings))
                {
                    doorSettings = new ConfigData.DoorSettings
                    {
                        enabled = true,
                        displayName = itemDefinition.displayName.english
                    };
                    configData.doorS.Add(itemDefinition.shortname, doorSettings);
                }
                if (doorSettings.enabled && !supportedDoors.ContainsKey(door.ShortPrefabName))
                {
                    supportedDoors.Add(door.ShortPrefabName, doorSettings.displayName);
                }
            }
            SaveConfig();
        }

        #endregion Methods

        #region ChatCommand

        private void CmdAutoDoor(BasePlayer player, string command, string[] args)
        {
            if (configData.usePermission && !permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            var playerData = GetPlayerData(player.userID);
            if (args == null || args.Length == 0)
            {
                playerData.doorS.enabled = !playerData.doorS.enabled;
                Print(player, Lang("AutoDoor", player.UserIDString, playerData.doorS.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                return;
            }
            float time;
            if (float.TryParse(args[0], out time))
            {
                if (time <= configData.globalS.maximumDelay && time >= configData.globalS.minimumDelay)
                {
                    playerData.doorS.time = time;
                    if (!playerData.doorS.enabled) playerData.doorS.enabled = true;
                    Print(player, Lang("AutoDoorDelay", player.UserIDString, time));
                    return;
                }
                Print(player, Lang("AutoDoorDelayLimit", player.UserIDString, configData.globalS.minimumDelay, configData.globalS.maximumDelay));
                return;
            }
            switch (args[0].ToLower())
            {
                case "a":
                case "all":
                    {
                        if (args.Length > 1)
                        {
                            if (float.TryParse(args[1], out time))
                            {
                                if (time <= configData.globalS.maximumDelay && time >= configData.globalS.minimumDelay)
                                {
                                    playerData.doorS.time = time;
                                    playerData.doorTypeS.Clear();
                                    playerData.theDoorS.Clear();
                                    Print(player, Lang("AutoDoorDelayAll", player.UserIDString, time));
                                    return;
                                }

                                Print(player,
                                    Lang("AutoDoorDelayLimit", player.UserIDString, configData.globalS.minimumDelay,
                                        configData.globalS.maximumDelay));
                                return;
                            }
                        }

                        break;
                    }
                case "s":
                case "single":
                    {
                        var door = GetLookingAtDoor(player);
                        if (door == null || door.net == null)
                        {
                            Print(player, Lang("DoorNotFound", player.UserIDString));
                            return;
                        }

                        string doorName;
                        if (!supportedDoors.TryGetValue(door.ShortPrefabName, out doorName))
                        {
                            Print(player, Lang("DoorNotSupported", player.UserIDString));
                            return;
                        }

                        StoredData.DoorData doorData;
                        if (!playerData.theDoorS.TryGetValue(door.net.ID, out doorData))
                        {
                            doorData = new StoredData.DoorData
                            { enabled = true, time = configData.globalS.defaultDelay };
                            playerData.theDoorS.Add(door.net.ID, doorData);
                        }

                        if (args.Length <= 1)
                        {
                            doorData.enabled = !doorData.enabled;
                            Print(player,
                                Lang("AutoDoorSingle", player.UserIDString, doorName,
                                    doorData.enabled
                                        ? Lang("Enabled", player.UserIDString)
                                        : Lang("Disabled", player.UserIDString)));
                            return;
                        }

                        if (float.TryParse(args[1], out time))
                        {
                            if (time <= configData.globalS.maximumDelay && time >= configData.globalS.minimumDelay)
                            {
                                doorData.time = time;
                                Print(player, Lang("AutoDoorSingleDelay", player.UserIDString, doorName, time));
                                return;
                            }

                            Print(player,
                                Lang("AutoDoorDelayLimit", player.UserIDString, configData.globalS.minimumDelay,
                                    configData.globalS.maximumDelay));
                            return;
                        }

                        break;
                    }

                case "t":
                case "type":
                    {
                        var door = GetLookingAtDoor(player);
                        if (door == null || door.net == null)
                        {
                            Print(player, Lang("DoorNotFound", player.UserIDString));
                            return;
                        }

                        string doorName;
                        if (!supportedDoors.TryGetValue(door.ShortPrefabName, out doorName))
                        {
                            Print(player, Lang("DoorNotSupported", player.UserIDString));
                            return;
                        }

                        StoredData.DoorData doorData;
                        if (!playerData.doorTypeS.TryGetValue(door.ShortPrefabName, out doorData))
                        {
                            doorData = new StoredData.DoorData
                            { enabled = true, time = configData.globalS.defaultDelay };
                            playerData.doorTypeS.Add(door.ShortPrefabName, doorData);
                        }

                        if (args.Length <= 1)
                        {
                            doorData.enabled = !doorData.enabled;
                            Print(player,
                                Lang("AutoDoorType", player.UserIDString, doorName,
                                    doorData.enabled
                                        ? Lang("Enabled", player.UserIDString)
                                        : Lang("Disabled", player.UserIDString)));
                            return;
                        }

                        if (float.TryParse(args[1], out time))
                        {
                            if (time <= configData.globalS.maximumDelay && time >= configData.globalS.minimumDelay)
                            {
                                doorData.time = time;
                                Print(player, Lang("AutoDoorTypeDelay", player.UserIDString, doorName, time));
                                return;
                            }

                            Print(player,
                                Lang("AutoDoorDelayLimit", player.UserIDString, configData.globalS.minimumDelay,
                                    configData.globalS.maximumDelay));
                            return;
                        }

                        break;
                    }

                case "h":
                case "help":
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        stringBuilder.AppendLine();
                        var firstCmd = configData.chatS.commands[0];
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax", player.UserIDString, firstCmd));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax1", player.UserIDString, firstCmd,
                            configData.globalS.minimumDelay, configData.globalS.maximumDelay));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax2", player.UserIDString, firstCmd));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax3", player.UserIDString, firstCmd,
                            configData.globalS.minimumDelay, configData.globalS.maximumDelay));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax4", player.UserIDString, firstCmd));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax5", player.UserIDString, firstCmd,
                            configData.globalS.minimumDelay, configData.globalS.maximumDelay));
                        stringBuilder.AppendLine(Lang("AutoDoorSyntax6", player.UserIDString, firstCmd,
                            configData.globalS.minimumDelay, configData.globalS.maximumDelay));
                        Print(player, stringBuilder.ToString());
                        return;
                    }
            }
            Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.commands[0]));
        }

        #endregion ChatCommand

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Use permissions")]
            public bool usePermission = false;

            [JsonProperty(PropertyName = "Clear data on map wipe")]
            public bool clearDataOnWipe = false;

            [JsonProperty(PropertyName = "Global settings")]
            public GlobalSettings globalS = new GlobalSettings();

            [JsonProperty(PropertyName = "Chat settings")]
            public ChatSettings chatS = new ChatSettings();

            [JsonProperty(PropertyName = "Door Settings")]
            public Dictionary<string, DoorSettings> doorS = new Dictionary<string, DoorSettings>();

            public class DoorSettings
            {
                public bool enabled;
                public string displayName;
            }

            public class GlobalSettings
            {
                [JsonProperty(PropertyName = "Allows automatic closing of unowned doors")]
                public bool useUnownedDoor = false;

                [JsonProperty(PropertyName = "Exclude door controller")]
                public bool excludeDoorController = true;

                [JsonProperty(PropertyName = "Cancel on player dead")]
                public bool cancelOnKill = false;

                [JsonProperty(PropertyName = "Default enabled")]
                public bool defaultEnabled = true;

                [JsonProperty(PropertyName = "Default delay")]
                public float defaultDelay = 5f;

                [JsonProperty(PropertyName = "Maximum delay")]
                public float maximumDelay = 10f;

                [JsonProperty(PropertyName = "Minimum delay")]
                public float minimumDelay = 5f;
            }

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat command")]
                public string[] commands = { "ad", "autodoor" };

                [JsonProperty(PropertyName = "Chat prefix")]
                public string prefix = "<color=#00FFFF>[AutoDoors]</color>: ";

                [JsonProperty(PropertyName = "Chat steamID icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version = new VersionNumber(3, 2, 6);
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
                if (configData.version <= new VersionNumber(3, 2, 6))
                {
                    if (configData.chatS.prefix == "[AutoDoors]: ")
                    {
                        configData.chatS.prefix = "<color=#00FFFF>[AutoDoors]</color>: ";
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
            public readonly Dictionary<ulong, PlayerData> playerData = new Dictionary<ulong, PlayerData>();

            public class PlayerData
            {
                public DoorData doorS = new DoorData();
                public readonly Dictionary<uint, DoorData> theDoorS = new Dictionary<uint, DoorData>();
                public readonly Dictionary<string, DoorData> doorTypeS = new Dictionary<string, DoorData>();
            }

            public class DoorData
            {
                public bool enabled;
                public float time;
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

        private void OnNewSave(string filename)
        {
            if (configData.clearDataOnWipe)
            {
                ClearData();
            }
            else
            {
                foreach (var value in storedData.playerData.Values)
                {
                    value.theDoorS.Clear();
                }
                SaveData();
            }
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command",
                ["Enabled"] = "<color=#8ee700>Enabled</color>",
                ["Disabled"] = "<color=#ce422b>Disabled</color>",
                ["AutoDoor"] = "Automatic door closing is now {0}",
                ["AutoDoorDelay"] = "Automatic door closing delay set to {0}s. (Doors set by 'single' and 'type' are not included)",
                ["AutoDoorDelayAll"] = "Automatic closing delay of all doors set to {0}s",
                ["DoorNotFound"] = "You need to look at a door",
                ["DoorNotSupported"] = "This type of door is not supported",
                ["AutoDoorDelayLimit"] = "Automatic door closing delay allowed is between {0}s and {1}s",
                ["AutoDoorSingle"] = "Automatic closing of this {0} is {1}",
                ["AutoDoorSingleDelay"] = "Automatic closing delay of this {0} is {1}s",
                ["AutoDoorType"] = "Automatic closing of {0} door is {1}",
                ["AutoDoorTypeDelay"] = "Automatic closing delay of {0} door is {1}s",
                ["SyntaxError"] = "Syntax error, type '<color=#ce422b>/{0} <help | h></color>' to view help",

                ["AutoDoorSyntax"] = "<color=#ce422b>/{0} </color> - Enable/Disable automatic door closing",
                ["AutoDoorSyntax1"] = "<color=#ce422b>/{0} [time (seconds)]</color> - Set automatic closing delay for doors, the allowed time is between {1}s and {2}s. (Doors set by 'single' and 'type' are not included)",
                ["AutoDoorSyntax2"] = "<color=#ce422b>/{0} <single | s></color> - Enable/Disable automatic closing of the door you are looking at",
                ["AutoDoorSyntax3"] = "<color=#ce422b>/{0} <single | s> [time (seconds)]</color> - Set automatic closing delay for the door you are looking at, the allowed time is between {1}s and {2}s",
                ["AutoDoorSyntax4"] = "<color=#ce422b>/{0} <type | t></color> - Enable/disable automatic door closing for the type of door you are looking at. ('type' is just a word, not the type of door)",
                ["AutoDoorSyntax5"] = "<color=#ce422b>/{0} <type | t> [time (seconds)]</color> - Set automatic closing delay for the type of door you are looking at, the allowed time is between {1}s and {2}s. ('type' is just a word, not the type of door)",
                ["AutoDoorSyntax6"] = "<color=#ce422b>/{0} <all | a> [time (seconds)]</color> - Set automatic closing delay for all doors, the allowed time is between {1}s and {2}s.",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>",
                ["AutoDoor"] = "自动关门现在的状态为 {0}",
                ["AutoDoorDelay"] = "自动关门延迟设置为 {0}秒",
                ["AutoDoorDelayAll"] = "全部门的自动关闭延迟设置为 {0}秒",
                ["DoorNotFound"] = "请您看着一条门再输入指令",
                ["DoorNotSupported"] = "不支持您看着的这种门",
                ["AutoDoorDelayLimit"] = "自动关门延迟应该在 {0}秒 和 {1}秒 之间",
                ["AutoDoorSingle"] = "这条 {0} 的自动关闭状态为 {1}",
                ["AutoDoorSingleDelay"] = "这条 {0} 的自动关闭延迟为 {1}秒",
                ["AutoDoorType"] = "这种 {0} 的自动关闭状态为 {1}",
                ["AutoDoorTypeDelay"] = "这种 {0} 的自动关闭延迟为 {1}秒",
                ["SyntaxError"] = "语法错误, 输入 '<color=#ce422b>/{0} <help | h></color>' 查看帮助",

                ["AutoDoorSyntax"] = "<color=#ce422b>/{0} </color> - 启用/禁用自动关门",
                ["AutoDoorSyntax1"] = "<color=#ce422b>/{0} [时间 (秒)]</color> - 设置自动关门延迟。(时间在 {1}秒 和 {2}秒 之间) (不包括'single'和'type'设置的门)",
                ["AutoDoorSyntax2"] = "<color=#ce422b>/{0} <single | s></color> - 为您看着的这条门，启用/禁用自动关门",
                ["AutoDoorSyntax3"] = "<color=#ce422b>/{0} <single | s> [时间 (秒)]</color> - 为您看着的这条门设置自动关闭延迟。(时间在 {1}秒 和 {2}秒 之间)",
                ["AutoDoorSyntax4"] = "<color=#ce422b>/{0} <type | t></color> - 为您看着的这种门，启用/禁用自动关门",
                ["AutoDoorSyntax5"] = "<color=#ce422b>/{0} <type | t> [时间 (秒)]</color> - 为您看着的这种门设置自动关闭延迟。(时间在 {1}秒 和 {2}秒 之间)",
                ["AutoDoorSyntax6"] = "<color=#ce422b>/{0} <all | a> [时间 (秒)]</color> - 为所有门设置自动关闭延迟。(时间在 {1}秒 和 {2}秒 之间)",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}