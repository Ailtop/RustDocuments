using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("SAM Site Range", "gsuberland/Arainrr", "1.2.4")]
    [Description("Modifies SAM site range.")]
    internal class SAMSiteRange : RustPlugin
    {
        private void Init()
        {
            Unsubscribe(nameof(OnEntitySpawned));
            foreach (var permissionRange in configData.permissionList)
                if (!permission.PermissionExists(permissionRange.permission))
                    permission.RegisterPermission(permissionRange.permission, this);
        }

        private void OnServerInitialized()
        {
            Subscribe(nameof(OnEntitySpawned));
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
                if (baseNetworkable is SamSite)
                    OnEntitySpawned(baseNetworkable as SamSite);
        }

        private void OnEntitySpawned(SamSite samSite) => ApplySettings(samSite);

        private void ApplySettings(SamSite samSite)
        {
            if (samSite == null) return;
            if (samSite.OwnerID == 0) samSite.scanRadius = configData.staticRange;
            else samSite.scanRadius = GetRange(samSite.OwnerID.ToString());
            samSite.SendNetworkUpdateImmediate();
        }

        private float GetRange(string ownerID)
        {
            float range = 0f;
            foreach (var permissionRange in configData.permissionList)
            {
                if (permission.UserHasPermission(ownerID, permissionRange.permission) && permissionRange.range > range)
                    range = permissionRange.range;
            }
            return range == 0f ? 150f : range;
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Static sam site range")]
            public float staticRange = 150f;

            [JsonProperty(PropertyName = "Permission list", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<PermissionRange> permissionList = new List<PermissionRange>
            {
                new PermissionRange
                {
                    permission ="samsiterange.use",
                    range = 200f,
                },
                new PermissionRange
                {
                    permission = "samsiterange.vip",
                    range = 250f,
                }
            };

            public class PermissionRange
            {
                public string permission;
                public float range;
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