using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("QuarryFactory", "Masteroliw/Arainrr", "1.1.0", ResourceId = 1376)]
    [Description("Spawn items inside the quarry when it gathers resources")]
    public class QuarryFactory : RustPlugin
    {
        private const string PREFAB_CRATER = "assets/prefabs/tools/surveycharge/survey_crater.prefab";

        private void Init()
        {
            LoadConfig();
            foreach (var entry in configData.settings)
            {
                if (string.IsNullOrEmpty(entry.permission)) continue;
                if (permission.PermissionExists(entry.permission, this)) continue;
                permission.RegisterPermission(entry.permission, this);
            }
        }

        private void OnServerInitialized()
        {
        }

        private object OnQuarryGather(MiningQuarry miningQuarry, Item item)
        {
            if (item?.info?.shortname == null || miningQuarry == null) return null;
            PrintError($"{miningQuarry.OwnerID} - {miningQuarry.transform.position} - {item.info.shortname} - {item.amount}");

            bool hasChanged = false;
            QuarryType quarryType = QuarryType.MiningQuarry;
            if (miningQuarry.ShortPrefabName == "pumpjack-static" || miningQuarry.ShortPrefabName == "mining.pumpjack") quarryType = QuarryType.PumpJack;
            foreach (var entry in configData.settings)
            {
                if (!entry.qaurryItems.ContainsKey(quarryType)) continue;
                if (string.IsNullOrEmpty(entry.permission))
                {
                    if (miningQuarry.OwnerID != 0) continue;
                    goto createItem;
                }
                else
                {
                    if (miningQuarry.OwnerID == 0) continue;
                    if (!permission.UserHasPermission(miningQuarry.OwnerID.ToString(), entry.permission)) continue;
                    goto createItem;
                }
                createItem:
                {
                    hasChanged = true;
                    CreateNewItems(miningQuarry, entry.qaurryItems[quarryType]);
                    break;
                }
            }
            if (hasChanged == false)
            {
                PrintError($"hasChanged = false");
                return null;
            }

            if (!miningQuarry.FuelCheck())
            {
                miningQuarry.SetOn(false);
                PrintError($"SetOn false");
            }
            return false;
        }

        private void CreateNewItems(MiningQuarry miningQuarry, List<QaurryItem> qaurryItems)
        {
            foreach (var item in qaurryItems)
            {
                if (item.change >= UnityEngine.Random.Range(0f, 100f))
                {
                    int amount = UnityEngine.Random.Range(item.minAmount, item.maxAmount + 1);
                    if (amount <= 0) continue;
                    var gather = ItemManager.CreateByName(item.shortName, amount);
                    if (gather == null) continue;
                    if (!gather.MoveToContainer(miningQuarry.hopperPrefab.instance.GetComponent<StorageContainer>().inventory))
                    {
                        gather.Remove();
                        miningQuarry.SetOn(false);
                    }
                }
            }
        }

        private enum QuarryType
        {
            MiningQuarry,
            PumpJack,
        }

        private class Settings
        {
            public string permission = "quarryfactory.use";
            public Dictionary<QuarryType, List<QaurryItem>> qaurryItems = new Dictionary<QuarryType, List<QaurryItem>>();
        }

        private class QaurryItem
        {
            public string shortName = "rock";
            public float change = 60f;
            public int minAmount = 1;
            public int maxAmount = 2;
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Chat settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat command")]
                public string command = "";//

                [JsonProperty(PropertyName = "Chat prefix")]
                public string prefix = "[AutoAuth]: ";

                [JsonProperty(PropertyName = "Chat prefix color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat steamID icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Use Team")]
            public bool useTeam = false;

            [JsonProperty(PropertyName = "Use Friends")]
            public bool useFriends = false;

            [JsonProperty(PropertyName = "Use Clan")]
            public bool useClans = false;

            [JsonProperty(PropertyName = "Block hurt other player's oil crater")]
            public bool cantDamage = true;

            [JsonProperty(PropertyName = "Block deploy pumpjack on other player's oil crater")]
            public bool cantDeploy = true;

            [JsonProperty(PropertyName = "Permission List", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<PermissionS> permissionList = new List<PermissionS>
            {
                new PermissionS
                {
                    permission = "backpumpjack.use",
                    priority = 0,
                    chance = 20f,
                    pMMin = 5f,
                    pMMax = 10f
                },
                new PermissionS
                {
                    permission = "backpumpjack.vip",
                    priority = 1,
                    chance = 40f,
                    pMMin = 10f,
                    pMMax = 20f
                }
            };

            public class PermissionS
            {
                [JsonProperty(PropertyName = "Permission")]
                public string permission;

                [JsonProperty(PropertyName = "Priority of permission")]
                public int priority;

                [JsonProperty(PropertyName = "Oil crater chance")]
                public float chance;

                [JsonProperty(PropertyName = "Minimum PM size")]
                public float pMMin;

                [JsonProperty(PropertyName = "Maximum PM size")]
                public float pMMax;
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

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public Dictionary<ulong, ShareData> playerShareData = new Dictionary<ulong, ShareData>();

            public class ShareData
            {
                public ShareDataEntry friendsShareEntry = new ShareDataEntry();
                public ShareDataEntry clanShareEntry = new ShareDataEntry();
                public ShareDataEntry teamShareEntry = new ShareDataEntry();
            }

            public class ShareDataEntry
            {
                public bool enabled = false;
                public bool shareCupboard = false;
                public bool shareTurret = false;
                public bool shareKeyLock = false;
                public bool shareCodeLock = false;
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

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.chatS.prefixColor}>{configData.chatS.prefix}</color>", configData.chatS.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoDamage"] = "You can't hurt other player's oil crater.",
                ["NoDeploy"] = "You can't deploy pumpjack on other player's oil crater.",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoDamage"] = "您不能伤害别人的油坑",
                ["NoDeploy"] = "您不能放置挖油机到别人的油坑上",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}