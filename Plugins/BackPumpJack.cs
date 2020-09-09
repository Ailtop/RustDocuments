using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Back PumpJack", "Arainrr", "1.4.9")]
    [Description("Obtain oil crater using survey charge.")]
    internal class BackPumpJack : RustPlugin
    {
        [PluginReference] private readonly Plugin Friends, Clans;
        private const string PREFAB_CRATER_OIL = "assets/prefabs/tools/surveycharge/survey_crater_oil.prefab";
        private readonly List<QuarryData> activeCraters = new List<QuarryData>();
        private readonly HashSet<SurveyCrater> checkedCraters = new HashSet<SurveyCrater>();
        private readonly Dictionary<uint, ConfigData.PermissionS> activeSurveyCharges = new Dictionary<uint, ConfigData.PermissionS>();

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            foreach (var permissionS in configData.permissionList)
            {
                if (!permission.PermissionExists(permissionS.permission, this))
                {
                    permission.RegisterPermission(permissionS.permission, this);
                }
            }
            if (!configData.settings.cantDeploy) Unsubscribe(nameof(CanBuild));
            if (!configData.settings.cantDamage) Unsubscribe(nameof(OnEntityTakeDamage));
        }

        private void OnServerInitialized()
        {
            List<MiningQuarry> miningQuarries = new List<MiningQuarry>();
            foreach (var baseNetworkable in BaseNetworkable.serverEntities)
            {
                var surveyCrater = baseNetworkable as SurveyCrater;
                if (surveyCrater != null)
                {
                    if (!surveyCrater.OwnerID.IsSteamId()) continue;
                    var deposit = ResourceDepositManager.GetOrCreate(surveyCrater.transform.position);
                    if (deposit?._resources == null || deposit._resources.Count <= 0) continue;
                    var mineralItemDataList = deposit._resources.Select(depositEntry => new QuarryData.MineralItemData { amount = depositEntry.amount, shortname = depositEntry.type.shortname, workNeeded = depositEntry.workNeeded }).ToList();
                    activeCraters.Add(new QuarryData { position = surveyCrater.transform.position, isLiquid = surveyCrater.ShortPrefabName == "survey_crater_oil", mineralItems = mineralItemDataList });
                    continue;
                }

                var miningQuarry = baseNetworkable as MiningQuarry;
                if (miningQuarry != null)
                {
                    if (miningQuarry.OwnerID.IsSteamId())
                    {
                        miningQuarries.Add(miningQuarry);
                    }
                }
            }
            CheckValidData(miningQuarries);
        }

        private void OnExplosiveThrown(BasePlayer player, SurveyCharge surveyCharge)
        {
            if (surveyCharge == null || surveyCharge.net == null) return;
            var permissionS = GetPermissionS(player);
            if (permissionS == null) return;
            surveyCharge.OwnerID = player.userID;
            activeSurveyCharges.Add(surveyCharge.net.ID, permissionS);
        }

        private void OnEntityKill(SurveyCharge surveyCharge)
        {
            if (surveyCharge == null || surveyCharge.net == null) return;
            ConfigData.PermissionS permissionS;
            if (activeSurveyCharges.TryGetValue(surveyCharge.net.ID, out permissionS))
            {
                activeSurveyCharges.Remove(surveyCharge.net.ID);
                ModifyResourceDeposit(permissionS, surveyCharge.transform.position, surveyCharge.OwnerID);
            }
        }

        private void OnEntityBuilt(Planner planner, GameObject obj)
        {
            var miningQuarry = obj?.ToBaseEntity() as MiningQuarry;
            if (miningQuarry == null || !miningQuarry.OwnerID.IsSteamId()) return;
            foreach (var quarryData in activeCraters.ToArray())
            {
                if (Vector3.Distance(quarryData.position, miningQuarry.transform.position) < 2f)
                {
                    storedData.quarryDataList.Add(quarryData);
                    CreateResourceDeposit(miningQuarry, quarryData);
                    activeCraters.Remove(quarryData);
                    SaveData();
                }
            }
        }

        private object OnEntityTakeDamage(SurveyCrater surveyCrater, HitInfo info)
        {
            if (surveyCrater == null || !surveyCrater.OwnerID.IsSteamId()) return null;
            var player = info?.InitiatorPlayer;
            if (player != null && player.userID.IsSteamId())
            {
                if (!AreFriends(surveyCrater.OwnerID, player.userID))
                {
                    Print(player, Lang("NoDamage", player.UserIDString));
                    return true;
                }
                return null;
            }
            return true;
        }

        private object CanBuild(Planner planner, Construction prefab, Construction.Target target)
        {
            var surveyCrater = target.entity as SurveyCrater;
            if (surveyCrater == null || !surveyCrater.OwnerID.IsSteamId()) return null;
            var player = planner?.GetOwnerPlayer();
            if (player == null) return null;
            if (!AreFriends(surveyCrater.OwnerID, player.userID))
            {
                Print(player, Lang("NoDeploy", player.UserIDString));
                return false;
            }
            return null;
        }

        #endregion Oxide Hooks

        #region Methods

        private void CheckValidData(List<MiningQuarry> miningQuarries)
        {
            if (miningQuarries.Count <= 0) return;
            foreach (var quarryData in storedData.quarryDataList.ToArray())
            {
                var validData = false;
                foreach (var miningQuarry in miningQuarries)
                {
                    if (Vector3.Distance(quarryData.position, miningQuarry.transform.position) < 2f)
                    {
                        CreateResourceDeposit(miningQuarry, quarryData);
                        validData = true;
                        break;
                    }
                }
                if (!validData) storedData.quarryDataList.Remove(quarryData);
            }
            SaveData();
        }
 
        private static void CreateResourceDeposit(MiningQuarry miningQuarry, QuarryData quarryData)
        {
            if (quarryData.isLiquid) miningQuarry.canExtractLiquid = true;
            else miningQuarry.canExtractSolid = true;
 
            miningQuarry._linkedDeposit._resources.Clear();
            foreach (var mineralItem in quarryData.mineralItems)
            {
                var itemDefinition = ItemManager.FindItemDefinition(mineralItem.shortname);
                if (itemDefinition == null) continue;
				miningQuarry._linkedDeposit.Add(itemDefinition, 1f, mineralItem.amount, mineralItem.workNeeded, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM, quarryData.isLiquid);
            }
            miningQuarry.SendNetworkUpdateImmediate();
        }

        private ConfigData.PermissionS GetPermissionS(BasePlayer player)
        {
            ConfigData.PermissionS permissionS = null;
            int priority = 0;
            foreach (var p in configData.permissionList)
            {
                if (permission.UserHasPermission(player.UserIDString, p.permission) && p.priority >= priority)
                {
                    priority = p.priority;
                    permissionS = p;
                }
            }
            return permissionS;
        }

        private void ModifyResourceDeposit(ConfigData.PermissionS permissionS, Vector3 checkPosition, ulong playerID)
        {
            NextTick(() =>
            {
                var surveyCraterList = Pool.GetList<SurveyCrater>();
                Vis.Entities(checkPosition, 1f, surveyCraterList, Rust.Layers.Mask.Default);
                foreach (var surveyCrater in surveyCraterList)
                {
                    if (checkedCraters.Contains(surveyCrater)) continue;
                    if (UnityEngine.Random.Range(0f, 100f) < permissionS.oilCraterChance)
                    {
                        var oilCrater = GameManager.server.CreateEntity(PREFAB_CRATER_OIL, surveyCrater.transform.position) as SurveyCrater;
                        if (oilCrater == null) continue;
                        surveyCrater.Kill();
                        oilCrater.OwnerID = playerID;
                        oilCrater.Spawn();
                        checkedCraters.Add(oilCrater);
                        var deposit = ResourceDepositManager.GetOrCreate(oilCrater.transform.position);
                        if (deposit != null)
                        {
                            deposit._resources.Clear();
                            int amount = UnityEngine.Random.Range(10000, 100000);
                            float workNeeded = 45f / UnityEngine.Random.Range(permissionS.pumpJackS.pMMin, permissionS.pumpJackS.pMMax);
                            var crudeItemDef = ItemManager.FindItemDefinition("crude.oil");
                            if (crudeItemDef != null)
                            {
                                deposit.Add(crudeItemDef, 1, amount, workNeeded, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM, true);
                                List<QuarryData.MineralItemData> mineralItemDatas = new List<QuarryData.MineralItemData> { new QuarryData.MineralItemData { amount = amount, shortname = crudeItemDef.shortname, workNeeded = workNeeded } };
                                activeCraters.Add(new QuarryData { position = oilCrater.transform.position, isLiquid = true, mineralItems = mineralItemDatas });
                            }
                        }
                    }
                    else if (UnityEngine.Random.Range(0f, 100f) < permissionS.quarryS.modifyChance)
                    {
                        var deposit = ResourceDepositManager.GetOrCreate(surveyCrater.transform.position);
                        if (deposit != null)
                        {
                            deposit._resources.Clear();
                            surveyCrater.OwnerID = playerID;
                            int amountsRemaining = UnityEngine.Random.Range(permissionS.quarryS.amountMin, permissionS.quarryS.amountMax + 1);
                            var mineralItemDataList = new List<QuarryData.MineralItemData>();

                            for (int i = 0; i < 200; i++)
                            {
                                if (amountsRemaining <= 0) break;
                                var mineralItem = permissionS.quarryS.mineralItems.GetRandom();
                                if (!permissionS.quarryS.allowDuplication && deposit._resources.Any(x => x.type.shortname == mineralItem.shortName)) continue;
                                if (UnityEngine.Random.Range(0f, 100f) < mineralItem.chance)
                                {
                                    var itemDef = ItemManager.FindItemDefinition(mineralItem.shortName);
                                    if (itemDef != null)
                                    {
                                        int amount = UnityEngine.Random.Range(10000, 100000);
                                        float workNeeded = 45f / UnityEngine.Random.Range(mineralItem.pMMin, mineralItem.pMMax);
                                        deposit.Add(itemDef, 1, amount, workNeeded, ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM);
                                        mineralItemDataList.Add(new QuarryData.MineralItemData { amount = amount, shortname = itemDef.shortname, workNeeded = workNeeded });
                                    }
                                    amountsRemaining--;
                                }
                            }
                            activeCraters.Add(new QuarryData { position = surveyCrater.transform.position, isLiquid = false, mineralItems = mineralItemDataList });
                        }
                    }
                    if (surveyCrater != null && !surveyCrater.IsDestroyed)
                        checkedCraters.Add(surveyCrater);
                }
                Pool.FreeList(ref surveyCraterList);
            });
        }

        #endregion Methods

        #region Helper

        private bool AreFriends(ulong playerID, ulong friendID)
        {
            if (playerID == friendID) return true;
            if (configData.settings.useTeams && SameTeam(playerID, friendID)) return true;
            if (configData.settings.useFriends && HasFriend(playerID, friendID)) return true;
            if (configData.settings.useClans && SameClan(playerID, friendID)) return true;
            return false;
        }

        private bool HasFriend(ulong playerID, ulong friendID)
        {
            if (Friends == null) return false;
            return (bool)Friends.Call("HasFriend", playerID, friendID);
        }

        private bool SameTeam(ulong playerID, ulong friendID)
        {
            if (!RelationshipManager.TeamsEnabled()) return false;
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam == null) return false;
            var friendTeam = RelationshipManager.Instance.FindPlayersTeam(friendID);
            if (friendTeam == null) return false;
            return playerTeam == friendTeam;
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

        #endregion Helper

        #region Commands

        [ConsoleCommand("backpumpjack.refill")]
        private void CCmdRefresh(ConsoleSystem.Arg arg)
        {
            int count = 0;
            foreach (var miningQuarry in BaseNetworkable.serverEntities.OfType<MiningQuarry>())
            {
                if (!miningQuarry.OwnerID.IsSteamId()) continue;
                foreach (var quarryData in storedData.quarryDataList)
                {
                    if (Vector3.Distance(quarryData.position, miningQuarry.transform.position) < 2f)
                    {
                        count++;
                        CreateResourceDeposit(miningQuarry, quarryData);
                        break;
                    }
                }
            }
            SendReply(arg, $"Refreshed {count} quarry resources.");
        }

        #endregion Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Settings")]
            public Settings settings = new Settings();

            public class Settings
            {
                [JsonProperty(PropertyName = "Use Teams")]
                public bool useTeams = true;

                [JsonProperty(PropertyName = "Use Friends")]
                public bool useFriends = true;

                [JsonProperty(PropertyName = "Use Clans")]
                public bool useClans = false;

                [JsonProperty(PropertyName = "Block damage another player's survey crater")]
                public bool cantDamage = true;

                [JsonProperty(PropertyName = "Block deploy a quarry on another player's survey crater")]
                public bool cantDeploy = true;
            }

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "[BackPumpJack]: ";

                [JsonProperty(PropertyName = "Chat Prefix Color")]
                public string prefixColor = "#00FFFF";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            [JsonProperty(PropertyName = "Permission List", ObjectCreationHandling = ObjectCreationHandling.Replace)]
            public List<PermissionS> permissionList = new List<PermissionS>
            {
                new PermissionS
                {
                    permission = "backpumpjack.use",
                    priority = 0,
                    oilCraterChance = 20f,
                    pumpJackS = new PermissionS.PumpJackS
                    {
                        pMMin = 5f,
                        pMMax = 10f
                    },
                    quarryS = new PermissionS.QuarryS()
                },
                new PermissionS
                {
                    permission = "backpumpjack.vip",
                    priority = 1,
                    oilCraterChance = 40f,
                    pumpJackS = new PermissionS.PumpJackS
                    {
                        pMMin = 10f,
                        pMMax = 20f
                    },
                    quarryS = new PermissionS.QuarryS
                    {
                        amountMin = 1,
                        amountMax = 3,
                        modifyChance = 50,
                        allowDuplication = false,
                        mineralItems = new List<PermissionS.QuarryS.MineralItem>
                        {
                            new PermissionS.QuarryS.MineralItem
                            {
                                shortName = "stones",
                                chance = 60f,
                                pMMin = 120f,
                                pMMax = 180f
                            },
                            new PermissionS.QuarryS.MineralItem
                            {
                                shortName = "metal.ore",
                                chance = 50f,
                                pMMin = 15f,
                                pMMax = 25f
                            },
                            new PermissionS.QuarryS.MineralItem
                            {
                                shortName = "sulfur.ore",
                                chance = 50f,
                                pMMin = 15f,
                                pMMax = 15f
                            },
                            new PermissionS.QuarryS.MineralItem
                            {
                                shortName = "hq.metal.ore",
                                chance = 50f,
                                pMMin = 1.5f,
                                pMMax = 2f
                            }
                        }
                    }
                }
            };

            public class PermissionS
            {
                [JsonProperty(PropertyName = "Permission")]
                public string permission;

                [JsonProperty(PropertyName = "Priority")]
                public int priority;

                [JsonProperty(PropertyName = "Oil Crater Chance")]
                public float oilCraterChance;

                [JsonProperty(PropertyName = "Oil Crater Settings")]
                public PumpJackS pumpJackS = new PumpJackS();

                public class PumpJackS
                {
                    [JsonProperty(PropertyName = "Minimum pM")]
                    public float pMMin;

                    [JsonProperty(PropertyName = "Maximum pM")]
                    public float pMMax;
                }

                [JsonProperty(PropertyName = "Normal Crater Settings")]
                public QuarryS quarryS = new QuarryS();

                public class QuarryS
                {
                    [JsonProperty(PropertyName = "Modify Chance (If not modified, use default mineral)")]
                    public float modifyChance;

                    [JsonProperty(PropertyName = "Minimum Mineral Amount")]
                    public int amountMin;

                    [JsonProperty(PropertyName = "Maximum Mineral Amount")]
                    public int amountMax;

                    [JsonProperty(PropertyName = "Allow Duplication Of Mineral Item")]
                    public bool allowDuplication = true;

                    [JsonProperty(PropertyName = "Mineral Items")]
                    public List<MineralItem> mineralItems = new List<MineralItem>();

                    public class MineralItem
                    {
                        [JsonProperty(PropertyName = "Mineral Item Short Name")]
                        public string shortName;

                        [JsonProperty(PropertyName = "Chance")]
                        public float chance;

                        [JsonProperty(PropertyName = "Minimum pM")]
                        public float pMMin;

                        [JsonProperty(PropertyName = "Maximum pM")]
                        public float pMMax;
                    }
                }
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
            public readonly List<QuarryData> quarryDataList = new List<QuarryData>();
        }

        public class QuarryData
        {
            public Vector3 position;
            public bool isLiquid;
            public List<MineralItemData> mineralItems = new List<MineralItemData>();

            public class MineralItemData
            {
                public string shortname;
                public int amount;
                public float workNeeded;
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

        private void ClearData()
        {
            storedData = new StoredData();
            SaveData();
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, storedData);

        private void OnNewSave() => ClearData();

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
                ["NoDamage"] = "You can't damage another player's survey crater.",
                ["NoDeploy"] = "You can't deploy a quarry on another player's survey crater.",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoDamage"] = "您不能伤害别人的矿坑",
                ["NoDeploy"] = "您不能放置挖矿机到别人的矿坑上",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}