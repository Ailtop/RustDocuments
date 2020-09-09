using System.Collections.Generic;
using System.Text;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;

namespace Oxide.Plugins
{
    [Info("Player Names", "Arainrr", "1.1.2")]
    [Description("Logs and stores all names used by players.")]
    public class PlayerNames : CovalencePlugin
    {
        private const string PERMISSION_USE = "playernames.use";
        private bool changed = false;

        private void Init()
        {
            LoadData();
            permission.RegisterPermission(PERMISSION_USE, this);
        }

        private void Unload() => SaveData();

        private void OnServerSave()
        {
            if (changed)
            {
                SaveData();
                changed = false;
            }
        }

        private void OnUserConnected(IPlayer iPlayer)
        {
            if (playerData.ContainsKey(iPlayer.Id))
            {
                if (!playerData[iPlayer.Id].Contains(iPlayer.Name))
                {
                    playerData[iPlayer.Id].Add(iPlayer.Name);
                    changed = true;
                }
            }
            else
            {
                playerData.Add(iPlayer.Id, new HashSet<string>() { iPlayer.Name });
                changed = true;
            }
        }

        private string GetPlayerNames(string playerID)
        {
            if (playerData.ContainsKey(playerID))
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(Lang("Prefix", playerID, playerID));
                foreach (var name in playerData[playerID])
                    stringBuilder.AppendLine(Lang("Name", playerID, name));
                return stringBuilder.ToString();
            }
            return string.Empty;
        }

        #region Command

        [Command("name")]
        private void CmdName(IPlayer iPlayer, string command, string[] args)
        {
            if (!iPlayer.IsAdmin && !iPlayer.HasPermission(PERMISSION_USE))
            {
                iPlayer.Message(Lang("NotAllowed", iPlayer.Id));
                return;
            }
            var target = args.Length == 0 ? iPlayer : players.FindPlayer(args[0]);
            if (target == null)
            {
                iPlayer.Message(Lang("PlayerNotFound", iPlayer.Id, args[0]));
                return;
            }
            var names = GetPlayerNames(target.Id);
            if (string.IsNullOrEmpty(names)) iPlayer.Message(Lang("DataNotFound", target.Id, target.Name));
            else iPlayer.Message(names);
        }

        #endregion Command

        #region DataFile

        private Dictionary<string, HashSet<string>> playerData = new Dictionary<string, HashSet<string>>();

        private void LoadData()
        {
            try
            {
                playerData = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, HashSet<string>>>(Name);
            }
            catch
            {
                playerData = new Dictionary<string, HashSet<string>>();
            }
        }

        private void SaveData() => Interface.Oxide.DataFileSystem.WriteObject(Name, playerData);

        #endregion DataFile

        #region LanguageFile

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command.",
                ["Name"] = "{0}",
                ["Prefix"] = "'{0}' used names: ",
                ["DataNotFound"] = "Name data of player '{0}' was not found.",
                ["PlayerNotFound"] = "Player '{0}' not found."
            }, this);
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["Name"] = "{0}",
                ["Prefix"] = "'{0}' 使用过的名字:",
                ["DataNotFound"] = "没有找到玩家 '{0}' 的名字数据",
                ["PlayerNotFound"] = "玩家 '{0}' 没有找到"
            }, this, "zh-CN");
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        #endregion LanguageFile
    }
}