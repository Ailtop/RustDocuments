using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Weapon Changer", "Orange", "1.1.2")]
    [Description("")]
    public class WeaponChanger : RustPlugin
    {
        #region Oxide Hooks

        private void OnServerInitialized()
        {
            if (config.weapons.Count == 0)
            {
                GetWeapons();
            }

            foreach (var perm in config.weapons.Values.Select(x => x.clip.permission))
            {
                permission.RegisterPermission(perm, this);
            }
        }

        private void OnItemCraftFinished(ItemCraftTask task, Item item)
        {
            CheckItem(item, task.owner?.UserIDString ?? "0");
        }
        
        private void OnItemAddedToContainer(ItemContainer container, Item item)
        {
            CheckItem(item, container.playerOwner?.UserIDString ?? "0");
        }
        
        private void OnReloadWeapon(BasePlayer player, BaseProjectile projectile)
        {
            CheckItem(projectile.GetItem(), player.UserIDString);
        }

        #endregion

        #region Core

        private void GetWeapons()
        {
            foreach (var item in ItemManager.itemList)
            {
                if (item.category != ItemCategory.Weapon)
                {
                    continue;
                }

                var weapon = item.GetComponent<ItemModEntity>()?.entityPrefab?.Get()?.GetComponent<BaseProjectile>();
                if (weapon == null)
                {
                    continue;
                }

                var size = weapon.primaryMagazine.definition.builtInSize;
                
                config.weapons.Add(item.shortname, new WeaponConfig
                {
                    clip = new Clip
                    {
                        size = size,
                        permission = $"weaponchanger.{item.shortname}",
                        permSize = size + 10
                    }
                });
            }
                
            SaveConfig();
        }

        private void CheckItem(Item item, string playerID)
        {
            var weapon = item?.GetHeldEntity()?.GetComponent<BaseProjectile>();
            if (weapon == null) {return;}
            var name = item.info.shortname;
            var data = (WeaponConfig) null;
            if (!config.weapons.TryGetValue(name, out data))
            {
                return;
            }
            var size = permission.UserHasPermission(playerID, data.clip.permission) ? data.clip.permSize : data.clip.size;
            weapon.primaryMagazine.capacity = size;
            weapon.SendNetworkUpdate();
        }

        #endregion
        
        #region Configuration 1.0.0
        
        private static ConfigData config;
        
        private class ConfigData
        {
            [JsonProperty(PropertyName = "Shortname -> Settings")]
            public Dictionary<string, WeaponConfig> weapons;
        }
        
        private ConfigData GetDefaultConfig()
        {
            return new ConfigData 
            {
                weapons = new Dictionary<string, WeaponConfig>()
            };
        }
        
        protected override void LoadConfig()
        {
            base.LoadConfig();
   
            try
            {
                config = Config.ReadObject<ConfigData>();
        
                if (config == null)
                {
                    LoadDefaultConfig();
                }
            }
            catch
            {
                LoadDefaultConfig();
            }

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintError("Configuration file is corrupt(or not exists), creating new one!");
            config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }
        
        #endregion

        #region Classes

        private class WeaponConfig
        {
            [JsonProperty(PropertyName = "Magazine settings")]
            public Clip clip;
        }

        private class Clip
        {
            [JsonProperty(PropertyName = "Size")]
            public int size;

            [JsonProperty(PropertyName = "Permission")]
            public string permission;

            [JsonProperty(PropertyName = "Size with permission")]
            public int permSize;
        }

        #endregion
    }
}