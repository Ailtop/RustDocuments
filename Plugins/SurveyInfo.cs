using System.Collections.Generic;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Survey Info", "Diesel_42o/Arainrr", "1.0.1", ResourceId = 2463)]
    [Description("Displays loot from survey charges")]
    internal class SurveyInfo : RustPlugin
    {
        private static SurveyInfo instance;
        private const string PERMISSION_USE = "surveyinfo.use";
        private const string PERMISSION_CHECK = "surveyinfo.check";
        private readonly HashSet<SurveyCrater> checkedCraters = new HashSet<SurveyCrater>();
        private readonly Dictionary<uint, List<SurveyItem>> activeSurveyCharges = new Dictionary<uint, List<SurveyItem>>();

        #region Oxide Hooks

        private void Init()
        {
            instance = this;
            permission.RegisterPermission(PERMISSION_USE, this);
            permission.RegisterPermission(PERMISSION_CHECK, this);
            cmd.AddChatCommand(configData.command, this, nameof(CmdCraterInfo));
        }

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
                UnityEngine.Object.Destroy(player.GetComponent<SurveyerComponent>());
            instance = null;
        }

        private void OnExplosiveThrown(BasePlayer player, SurveyCharge surveyCharge)
        {
            if (surveyCharge == null || surveyCharge.net == null || player == null) return;
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return;
            var entityID = surveyCharge.net.ID;
            activeSurveyCharges.Add(entityID, new List<SurveyItem>());
            timer.Once(5.5f, () =>
            {
                List<SurveyItem> surveyItems;
                if (activeSurveyCharges.TryGetValue(entityID, out surveyItems))
                {
                    activeSurveyCharges.Remove(entityID);
                    if (surveyItems.Count > 0) SendMineralAnalysis(player, surveyItems);
                }
            });
        }

        private void OnEntityKill(SurveyCharge surveyCharge)
        {
            if (surveyCharge == null || surveyCharge.net == null) return;
            var entityID = surveyCharge.net.ID;
            if (!activeSurveyCharges.ContainsKey(entityID)) return;
            Vector3 checkPosition = surveyCharge.transform.position;
            timer.Once(0.3f, () =>
            {
                List<SurveyCrater> surveyCraterList = Pool.GetList<SurveyCrater>();
                Vis.Entities(checkPosition, 1f, surveyCraterList, Rust.Layers.Mask.Default);
                List<SurveyItem> surveyItems = new List<SurveyItem>();
                foreach (var surveyCrater in surveyCraterList)
                {
                    if (checkedCraters.Contains(surveyCrater)) continue;
                    var deposit = ResourceDepositManager.GetOrCreate(surveyCrater.transform.position);
                    if (deposit == null) continue;
                    foreach (var resource in deposit._resources)
                    {
                        string displayName;
                        if (!configData.displayNames.TryGetValue(resource.type.displayName.english, out displayName))
                        {
                            displayName = resource.type.displayName.english;
                            configData.displayNames.Add(displayName, displayName);
                            SaveConfig();
                        }
                        surveyItems.Add(new SurveyItem { displayName = displayName, amount = resource.amount, workNeeded = resource.workNeeded });
                    }
                    checkedCraters.Add(surveyCrater);
                }
                activeSurveyCharges[entityID] = surveyItems;
                Pool.FreeList(ref surveyCraterList);
            });
        }

        private void OnActiveItemChanged(BasePlayer player, Item oldItem, Item newItem)
        {
            if (player == null || newItem == null) return;
            if (oldItem?.info?.shortname != "surveycharge" && newItem.info.shortname == "surveycharge")
            {
                if (!permission.UserHasPermission(player.UserIDString, PERMISSION_CHECK)) return;
                UnityEngine.Object.Destroy(player.GetComponent<SurveyerComponent>());
                player.gameObject.AddComponent<SurveyerComponent>();
            }
        }

        #endregion Oxide Hooks

        private class SurveyerComponent : MonoBehaviour
        {
            private BasePlayer player;
            private Item heldItem;
            private float lastCheck;
            private uint currentItemID;

            private void Awake()
            {
                player = GetComponent<BasePlayer>();
                heldItem = player.GetActiveItem();
                currentItemID = player.svActiveItemID;
            }

            private void FixedUpdate()
            {
                if (player == null || !player.IsConnected || !player.CanInteract())
                {
                    Destroy(this);
                    return;
                }
                if (player.svActiveItemID != currentItemID)
                {
                    heldItem = player.GetActiveItem();
                    if (heldItem != null && heldItem.info.shortname != "surveycharge")
                    {
                        Destroy(this);
                        return;
                    }
                    currentItemID = player.svActiveItemID;
                }
                if (Time.realtimeSinceStartup - lastCheck >= 0.5f)
                {
                    if (!player.serverInput.IsDown(BUTTON.FIRE_SECONDARY)) return;
                    Vector3 surveyPosition = GetSurveyPosition();
                    instance.Print(player, CanSpawnCrater(surveyPosition) ? instance.Lang("CanSpawnCrater", player.UserIDString) : instance.Lang("CantSpawnCrater", player.UserIDString));
                    lastCheck = Time.realtimeSinceStartup;
                }
            }

            private Vector3 GetSurveyPosition()
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(player.eyes.HeadRay(), out hitInfo, 100f, Rust.Layers.Solid))
                    return hitInfo.point;
                return player.transform.position;
            }

            private bool CanSpawnCrater(Vector3 position)
            {
                if (WaterLevel.Test(position)) return false;
                var deposit = ResourceDepositManager.GetOrCreate(position);
                if (deposit == null || Time.realtimeSinceStartup - deposit.lastSurveyTime < 10f) return false;
                RaycastHit raycastHit;
                if (!TransformUtil.GetGroundInfo(position, out raycastHit, 0.3f, 8388608)) return false;
                List<SurveyCrater> list = Pool.GetList<SurveyCrater>();
                Vis.Entities(position, 10f, list, 1);
                bool flag = list.Count > 0;
                Pool.FreeList(ref list);
                if (flag) return false;
                foreach (ResourceDepositManager.ResourceDeposit.ResourceDepositEntry resource in deposit._resources)
                {
                    if (resource.spawnType == ResourceDepositManager.ResourceDeposit.surveySpawnType.ITEM && !resource.isLiquid && resource.amount >= 1000)
                        return true;
                }
                return false;
            }
        }

        private void CmdCraterInfo(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            RaycastHit hitInfo;
            if (!Physics.Raycast(player.eyes.HeadRay(), out hitInfo, 10f, Rust.Layers.Mask.Default) || !(hitInfo.GetEntity() is SurveyCrater))
            {
                Print(player, Lang("NotLookingAtCrater", player.UserIDString));
                return;
            }
            var surveyCrater = hitInfo.GetEntity() as SurveyCrater;
            List<SurveyItem> surveyItems = new List<SurveyItem>();
            var deposit = ResourceDepositManager.GetOrCreate(surveyCrater.transform.position);
            if (deposit != null)
            {
                foreach (var resource in deposit._resources)
                {
                    string displayName;
                    if (!configData.displayNames.TryGetValue(resource.type.displayName.english, out displayName))
                    {
                        displayName = resource.type.displayName.english;
                        configData.displayNames.Add(displayName, displayName);
                        SaveConfig();
                    }
                    surveyItems.Add(new SurveyItem { displayName = displayName, amount = resource.amount, workNeeded = resource.workNeeded });
                }
            }
            if (surveyItems.Count <= 0)
            {
                Print(player, Lang("NoMinerals", player.UserIDString));
                return;
            }
            SendMineralAnalysis(player, surveyItems);
        }

        #region Methods

        private void SendMineralAnalysis(BasePlayer player, List<SurveyItem> surveyItems)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Lang("MineralAnalysis", player.UserIDString));
            foreach (var surveyItem in surveyItems)
            {
                float pM = 45f / surveyItem.workNeeded;
                stringBuilder.AppendLine(Lang("MineralInfo", player.UserIDString, surveyItem.displayName, surveyItem.amount, pM.ToString("0.0")));
            }
            Print(player, stringBuilder.ToString());
        }

        private class SurveyItem
        {
            public string displayName;
            public int amount;
            public float workNeeded;
        }

        #endregion Methods

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Chat Command")]
            public string command = "craterinfo";

            [JsonProperty(PropertyName = "Chat Prefix")]
            public string prefix = "[SurveyInfo]: ";

            [JsonProperty(PropertyName = "Chat Prefix Color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat SteamID Icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Display Names")]
            public Dictionary<string, string> displayNames = new Dictionary<string, string>();
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

        #region LanguageFile

        private void Print(BasePlayer player, string message) => Player.Message(player, message, $"<color={configData.prefixColor}>{configData.prefix}</color>", configData.steamIDIcon);

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You don't have permission to use this command.",
                ["MineralAnalysis"] = "- Mineral Analysis -",
                ["MineralInfo"] = "<color=#05EB59>{0}</color> x<color=#FFA500>{1}</color> -- <color=#FF4500> pM: {2} </color>",
                ["CanSpawnCrater"] = "A crater <color=#8ee700>can</color> be spawned at the position you are looking at.",
                ["CantSpawnCrater"] = "A crater <color=#ce422b>cannot</color> be spawned at the position you are looking at.",
                ["NotLookingAtCrater"] = "You must be looking at a crater.",
                ["NoMinerals"] = "There are no minerals in this crater",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["MineralAnalysis"] = "- 矿物分析 -",
                ["MineralInfo"] = "<color=#05EB59>{0}</color> x<color=#FFA500>{1}</color> -- <color=#FF4500> pM: {2} </color>",
                ["CanSpawnCrater"] = "您看着的位置 <color=#8ee700>可以</color> 勘探出矿物",
                ["CantSpawnCrater"] = "您看着的位置 <color=#ce422b>不能</color> 勘探出矿物",
                ["NotLookingAtCrater"] = "您必须看着一个矿坑",
                ["NoMinerals"] = "这个矿坑内没有任何矿物",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}