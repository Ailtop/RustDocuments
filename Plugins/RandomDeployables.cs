using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

//using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("Random Deployables", "Norn/Arainrr", "1.0.4", ResourceId = 2187)]
    [Description("Randomize deployable skins")]
    public class RandomDeployables : RustPlugin
    {
        private const string PERMISSION_USE = "randomdeployables.use";
        private readonly Dictionary<string, string> deployedToItem = new Dictionary<string, string>();
        private readonly Hash<string, List<ulong>> itemSkins = new Hash<string, List<ulong>>();

        private void Init() => permission.RegisterPermission(PERMISSION_USE, this);

        private void OnServerInitialized()
        {
            var approvedSkins = new Dictionary<string, List<ulong>>();
            foreach (var skinInfo in Rust.Workshop.Approved.All.Values)
            {
                List<ulong> skins;
                if (!approvedSkins.TryGetValue(skinInfo.Skinnable.ItemName, out skins))
                {
                    skins = new List<ulong>();
                    approvedSkins.Add(skinInfo.Skinnable.ItemName, skins);
                }
                skins.Add(skinInfo.WorkshopdId);
            }

            foreach (var definition in ItemManager.GetItemDefinitions())
            {
                if (!configData.blockItem.Contains(definition.shortname))
                {
                    var skins = new List<ulong>();
                    skins.AddRange(GetSkins(definition));
                    List<ulong> list;
                    if (approvedSkins.TryGetValue(definition.shortname, out list)) skins.AddRange(list);
                    if (skins.Count > 0)
                    {
                        skins = skins.Where(skin => !configData.blockSkin.Contains(skin) && skin != 0).ToList();
                        if (configData.defaultSkin) skins.Add(0);
                        itemSkins[definition.shortname] = skins;
                    }
                }
                var deployablePrefab = definition.GetComponent<ItemModDeployable>()?.entityPrefab?.resourcePath;
                if (!string.IsNullOrEmpty(deployablePrefab) && !deployedToItem.ContainsKey(deployablePrefab))
                    deployedToItem.Add(deployablePrefab, definition.shortname);
            }
            //Interface.Oxide.DataFileSystem.WriteObject(Name, itemSkins);
        }

        private static IEnumerable<ulong> GetSkins(ItemDefinition itemDefinition)
        {
            if (itemDefinition.skins != null && itemDefinition.skins.Length > 0)
            {
                foreach (var itemDefinitionSkin in itemDefinition.skins)
                {
                    yield return (ulong)itemDefinitionSkin.id;
                }
            }
        }

        private void OnEntityBuilt(Planner planner, GameObject obj)
        {
            var entity = obj?.ToBaseEntity();
            var player = planner?.GetOwnerPlayer();
            if (entity == null || player == null) return;
            if (permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                string shortName;
                if (deployedToItem.TryGetValue(entity.PrefabName, out shortName))
                {
                    if (configData.blockRandom && entity.skinID != 0) return;
                    List<ulong> skins;
                    if (itemSkins.TryGetValue(shortName, out skins))
                    {
                        entity.skinID = skins.GetRandom();
                        entity.SendNetworkUpdate();
                    }
                }
            }
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Allow default skin")]
            public bool defaultSkin = false;

            [JsonProperty(PropertyName = "If the item has skin, block random skin")]
            public bool blockRandom = true;

            [JsonProperty(PropertyName = "Block item list (item short name)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<string> blockItem = new List<string> { "item short name" };

            [JsonProperty(PropertyName = "Block skin list (item skin id)", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<ulong> blockSkin = new List<ulong> { 492800372 };
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

        protected override void SaveConfig() => Config.WriteObject(configData);

        #endregion ConfigurationFile
    }
}