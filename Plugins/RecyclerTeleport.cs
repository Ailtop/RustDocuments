using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Recycler Teleport", "Norn/Arainrr", "1.1.1")]
    [Description("Teleport to recyclers via command.")]
    public class RecyclerTeleport : RustPlugin
    {
        private const string PERMISSION_USE = "recyclerteleport.use";
        private readonly List<Vector3> recyclerPositions = new List<Vector3>();

        private void Init()
        {
            permission.RegisterPermission(PERMISSION_USE, this);
            cmd.AddChatCommand(configData.command, this, nameof(RecyclerCommand));
        }

        private void OnServerInitialized() => Findrecycler();

        private void Findrecycler()
        {
            var monumentRecyclers = new Hash<string, List<Vector3>>();
            var monumentInfos = TerrainMeta.Path.Monuments.Where(x => !x.name.Contains("power substations") && !x.name.Contains("cave") && !x.name.Contains("tiny")).GroupBy(x => x.name).ToDictionary(x => x.Key, y => y.ToList());
            foreach (var entry in monumentInfos)
            {
                var monument = GameManager.server.FindPrefab(entry.Key);
                if (monument == null) continue;
                var monumentName = string.Empty;
                List<Vector3> recyclers = new List<Vector3>();
                foreach (var monumentInfo in entry.Value)
                {
                    monumentName = monumentInfo.displayPhrase.english.Replace("\n", "");
                    recyclers.AddRange(monument.gameObject.GetComponentsInChildren<Recycler>()?.Select(x => monumentInfo.transform.TransformPoint(x.transform.position)));
                }
                if (!string.IsNullOrEmpty(monumentName) && recyclers.Count > 0)
                {
                    if (!monumentRecyclers.ContainsKey(monumentName))
                        monumentRecyclers.Add(monumentName, recyclers);
                    else monumentRecyclers[monumentName].AddRange(recyclers);
                }
            }
            foreach (var entry in monumentRecyclers)
            {
                if (!configData.monumentBlockList.ContainsKey(entry.Key))
                    configData.monumentBlockList.Add(entry.Key, false);
                else if (configData.monumentBlockList[entry.Key]) continue;
                recyclerPositions.AddRange(entry.Value);
            }
            SaveConfig();
            Puts($"{recyclerPositions.Count} recyclers found.");
        }

        private void TeleportToRecycler(BasePlayer player)
        {
            Vector3 position = recyclerPositions.GetRandom() + new Vector3(0f, 1.5f, 0f);
            timer.Once(configData.teleportSeconds, () =>
            {
                if (!player.IsConnected) return;
                player.EnsureDismounted();
                player.SetParent(null, true, true);
                player.ClientRPCPlayer(null, player, "StartLoading");
                player.SetPlayerFlag(BasePlayer.PlayerFlags.Sleeping, true);
                player.Teleport(position);
                player.SetPlayerFlag(BasePlayer.PlayerFlags.ReceivingSnapshot, true);
                player.UpdateNetworkGroup();
                player.SendNetworkUpdateImmediate();
                player.SendFullSnapshot();
            });
            Print(player, Lang("Teleporting", player.UserIDString, configData.teleportSeconds.ToString()));
        }

        private void RecyclerCommand(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NoPermission", player.UserIDString));
                return;
            }
            if (recyclerPositions.Count == 0)
            {
                Print(player, Lang("NoRecyclers", player.UserIDString));
                return;
            }
            object canTeleport = Interface.CallHook("CanTeleport", player);
            if (canTeleport is string)
            {
                Print(player, (string)canTeleport);
                return;
            }
            TeleportToRecycler(player);
        }

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Chat command")]
            public string command = "recycler";

            [JsonProperty(PropertyName = "Chat prefix")]
            public string prefix = "[RecyclerTeleport]:";

            [JsonProperty(PropertyName = "Chat prefix color")]
            public string prefixColor = "#00FFFF";

            [JsonProperty(PropertyName = "Chat steamID icon")]
            public ulong steamIDIcon = 0;

            [JsonProperty(PropertyName = "Teleport seconds")]
            public float teleportSeconds = 10f;

            [JsonProperty(PropertyName = "Block list")]
            public Dictionary<string, bool> monumentBlockList = new Dictionary<string, bool>();
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
                ["NoPermission"] = "<color=#FFFF00>You don't have permission to use this command.</color>",
                ["Teleporting"] = "Teleporting to recycler in <color=#FFFF00>{0}</color> seconds.",
                ["NoRecyclers"] = "No recyclers found."
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NoPermission"] = "<color=#FFFF00>您没有使用该命令的权限</color>",
                ["Teleporting"] = "<color=#FFFF00>{0}</color> 秒后，您将传送到分解机",
                ["NoRecyclers"] = "服务器上没有找到任何分解机"
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}