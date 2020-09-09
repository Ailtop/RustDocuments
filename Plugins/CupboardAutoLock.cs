using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Cupboard Auto Lock", "BuzZ/Arainrr", "0.0.5")]
    [Description("Automatically add a codelock on cupboards.")]
    public class CupboardAutoLock : RustPlugin
    {
        private const string PERMISSION_USE = "cupboardautolock.code";
        private const string PREFAB_CODE_LOCK = "assets/prefabs/locks/keypad/lock.code.prefab";

        private void Init() => permission.RegisterPermission(PERMISSION_USE, this);

        private void OnEntityBuilt(Planner planner, GameObject obj)
        {
            var buildingPrivlidge = obj.ToBaseEntity() as BuildingPrivlidge;
            if (buildingPrivlidge == null || !buildingPrivlidge.OwnerID.IsSteamId()) return;
            var player = planner.GetOwnerPlayer();
            NextTick(() =>
            {
                if (buildingPrivlidge == null || buildingPrivlidge.IsDestroyed) return;
                if (buildingPrivlidge.GetSlot(BaseEntity.Slot.Lock) != null) return;
                if (!permission.UserHasPermission(buildingPrivlidge.OwnerID.ToString(), PERMISSION_USE)) return;
                var codeLock = GameManager.server.CreateEntity(PREFAB_CODE_LOCK) as CodeLock;
                if (codeLock == null) return;
                codeLock.SetParent(buildingPrivlidge, buildingPrivlidge.GetSlotAnchorName(BaseEntity.Slot.Lock));
                codeLock.OwnerID = buildingPrivlidge.OwnerID;
                codeLock.OnDeployed(buildingPrivlidge, player);
                codeLock.Spawn();
                if (configData.cupboardNoRefill) codeLock.SetFlag(BaseEntity.Flags.Locked, true, false);
                else
                {
                    codeLock.code = Random.Range(1000, 10000).ToString();
                    codeLock.whitelistPlayers.Add(buildingPrivlidge.OwnerID);
                    codeLock.SetFlag(BaseEntity.Flags.Locked, true);
                }
                buildingPrivlidge.SetSlot(BaseEntity.Slot.Lock, codeLock);
                Effect.server.Run(codeLock.effectLocked.resourcePath, codeLock.transform.position);
                if (player != null) player.ChatMessage(Lang("AutoLockMsg", player.UserIDString, codeLock.code));
            });
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Prevent inventory from being accessed")]
            public bool cupboardNoRefill;
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

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AutoLockMsg"] = "Your cupboard is automatically locked. Password is {0}",
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["AutoLockMsg"] = "您的领地柜已自动上锁，密码为 {0}",
            }, this, "zh-CN");
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        #endregion LanguageFile
    }
}