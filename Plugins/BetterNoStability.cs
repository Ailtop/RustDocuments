using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Game.Rust;

namespace Oxide.Plugins
{
    [Info("Better No Stability", "Arainrr", "1.1.5")]
    [Description("Similar to 'server.stability false', but when an item loses its base, it does not levitate.")]
    public class BetterNoStability : RustPlugin
    {
        private const string PERMISSION_USE = "betternostability.use";

        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            Unsubscribe(nameof(OnUserGroupAdded));
            Unsubscribe(nameof(OnEntityGroundMissing));
            Unsubscribe(nameof(OnUserPermissionGranted));
            Unsubscribe(nameof(OnGroupPermissionGranted));
            permission.RegisterPermission(PERMISSION_USE, this);
        }

        private void OnServerInitialized()
        {
            UpdateConfig();
            if (configData.pluginEnabled)
            {
                Subscribe(nameof(OnEntitySpawned));
                if (configData.usePermission)
                {
                    Subscribe(nameof(OnUserGroupAdded));
                    Subscribe(nameof(OnUserPermissionGranted));
                    Subscribe(nameof(OnGroupPermissionGranted));
                }
                if (configData.floatingS.enabled)
                    Subscribe(nameof(OnEntityGroundMissing));
                ConVar.Server.stability = true;
                foreach (var stabilityEntity in BaseNetworkable.serverEntities.OfType<StabilityEntity>())
                    OnEntitySpawned(stabilityEntity);
            }
        }

        private void UpdateConfig()
        {
            foreach (var prefab in GameManifest.Current.entities)
            {
                var stabilityEntity = GameManager.server.FindPrefab(prefab.ToLower())?.GetComponent<StabilityEntity>();
                if (stabilityEntity != null && !string.IsNullOrEmpty(stabilityEntity.ShortPrefabName) && !configData.stabilityS.ContainsKey(stabilityEntity.ShortPrefabName))
                    configData.stabilityS.Add(stabilityEntity.ShortPrefabName, stabilityEntity is BuildingBlock);
            }
            SaveConfig();
        }

        private void OnEntitySpawned(StabilityEntity stabilityEntity)
        {
            if (stabilityEntity == null || stabilityEntity.OwnerID == 0) return;
            if (configData.usePermission && !permission.UserHasPermission(stabilityEntity.OwnerID.ToString(), PERMISSION_USE)) return;
            bool enabled;
            if (configData.stabilityS.TryGetValue(stabilityEntity.ShortPrefabName, out enabled) && !enabled) return;
            stabilityEntity.grounded = true;
        }

        private object OnEntityGroundMissing(BaseEntity entity)
        {
            if (entity == null || entity.OwnerID == 0) return null;
            if (configData.usePermission && !permission.UserHasPermission(entity.OwnerID.ToString(), PERMISSION_USE)) return null;
            if (configData.floatingS.floatingEntity.Contains(entity.ShortPrefabName)) return false;
            return null;
        }

        #region PermissionChanged

        private void OnUserPermissionGranted(string playerID, string permName)
        {
            if (permName != PERMISSION_USE) return;
            UserPermissionChanged(new string[] { playerID });
        }

        private void OnGroupPermissionGranted(string groupName, string permName)
        {
            if (permName != PERMISSION_USE) return;
            var users = permission.GetUsersInGroup(groupName);
            var playerIDs = users.Select(x => x.Substring(0, x.IndexOf(' '))).Where(x => RustCore.FindPlayerByIdString(x) != null).ToArray();
            UserPermissionChanged(playerIDs);
        }

        private void OnUserGroupAdded(string playerID, string groupName)
        {
            if (!permission.GroupHasPermission(groupName, PERMISSION_USE)) return;
            UserPermissionChanged(new string[] { playerID });
        }

        private void UserPermissionChanged(string[] playerIDs)
        {
            bool enabled;
            var stabilityEntities = BaseNetworkable.serverEntities.OfType<StabilityEntity>().GroupBy(x => x.ShortPrefabName).ToDictionary(x => x.Key, y => y.ToList());
            foreach (var entry in stabilityEntities)
            {
                if (configData.stabilityS.TryGetValue(entry.Key, out enabled) && !enabled) continue;
                foreach (var stabilityEntity in entry.Value)
                {
                    if (stabilityEntity.OwnerID == 0) continue;
                    if (playerIDs.Any(x => x == stabilityEntity.OwnerID.ToString()))
                        stabilityEntity.grounded = true;
                }
            }
        }

        #endregion PermissionChanged

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Enable Plugin")]
            public bool pluginEnabled = false;

            [JsonProperty(PropertyName = "Use Permission")]
            public bool usePermission = false;

            [JsonProperty(PropertyName = "Stability Entity Settings")]
            public Dictionary<string, bool> stabilityS = new Dictionary<string, bool>();

            [JsonProperty(PropertyName = "Floating Settings")]
            public FloatingS floatingS = new FloatingS();

            public class FloatingS
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled = false;

                [JsonProperty(PropertyName = "Floating Entity List (entity short prefab name)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
                public List<string> floatingEntity = new List<string>
                {
                    "rug.deployed",
                    "cupboard.tool.deployed"
                };
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
    }
}