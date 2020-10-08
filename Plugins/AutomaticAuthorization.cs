using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust;
using Oxide.Game.Rust.Cui;
using ProtoBuf;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("Automatic Authorization", "k1lly0u/Arainrr", "1.2.2", ResourceId = 2063)]
    public class AutomaticAuthorization : RustPlugin
    {
        #region Fields

        [PluginReference] private readonly Plugin Clans, Friends;
        private const string PERMISSION_USE = "automaticauthorization.use";
        private readonly Dictionary<ulong, EntityCache> playerEntities = new Dictionary<ulong, EntityCache>();

        private enum ShareType
        {
            None,
            Teams,
            Friends,
            Clans,
        }

        private class EntityCache
        {
            public HashSet<AutoTurret> autoTurrets = new HashSet<AutoTurret>();
            public HashSet<BuildingPrivlidge> buildingPrivlidges = new HashSet<BuildingPrivlidge>();
        }

        #endregion Fields

        #region Oxide Hooks

        private void Init()
        {
            LoadData();
            UpdateData();
            Unsubscribe(nameof(OnEntitySpawned));
            permission.RegisterPermission(PERMISSION_USE, this);
            cmd.AddChatCommand(configData.chatS.chatCommand, this, nameof(CmdAutoAuth));
            cmd.AddChatCommand(configData.chatS.uiCommand, this, nameof(CmdAutoAuthUI));
        }

        private void OnServerInitialized()
        {
            if (!configData.teamShareS.enabled)
            {
                Unsubscribe(nameof(OnTeamLeave));
                Unsubscribe(nameof(OnTeamKick));
                Unsubscribe(nameof(OnTeamAcceptInvite));
            }
            if (!configData.friendsShareS.enabled)
            {
                Unsubscribe(nameof(OnFriendAdded));
                Unsubscribe(nameof(OnFriendRemoved));
            }
            if (!configData.clanShareS.enabled)
            {
                Unsubscribe(nameof(OnClanUpdate));
                Unsubscribe(nameof(OnClanDestroy));
            }
            Subscribe(nameof(OnEntitySpawned));
            foreach (var entity in BaseNetworkable.serverEntities.OfType<BaseEntity>())
            {
                CheckEntity(entity);
            }
        }

        private void OnServerSave() => timer.Once(UnityEngine.Random.Range(0f, 60f), SaveData);

        private void Unload()
        {
            foreach (var player in BasePlayer.activePlayerList)
                DestroyUI(player);
            SaveData();
        }

        private void OnEntitySpawned(BaseEntity entity) => CheckEntity(entity, true);

        private void CheckEntity(BaseEntity entity, bool justCreated = false)
        {
            if (entity == null || !entity.OwnerID.IsSteamId()) return;
            var buildingPrivlidge = entity as BuildingPrivlidge;
            if (buildingPrivlidge != null)
            {
                EntityCache entityCache;
                if (playerEntities.TryGetValue(buildingPrivlidge.OwnerID, out entityCache)) entityCache.buildingPrivlidges.Add(buildingPrivlidge);
                else playerEntities.Add(buildingPrivlidge.OwnerID, new EntityCache { buildingPrivlidges = new HashSet<BuildingPrivlidge> { buildingPrivlidge } });
                if (justCreated && permission.UserHasPermission(buildingPrivlidge.OwnerID.ToString(), PERMISSION_USE))
                    AuthToCupboard(new HashSet<BuildingPrivlidge> { buildingPrivlidge }, buildingPrivlidge.OwnerID, true);
                return;
            }

            var autoTurret = entity as AutoTurret;
            if (autoTurret != null)
            {
                EntityCache entityCache;
                if (playerEntities.TryGetValue(autoTurret.OwnerID, out entityCache)) entityCache.autoTurrets.Add(autoTurret);
                else playerEntities.Add(autoTurret.OwnerID, new EntityCache { autoTurrets = new HashSet<AutoTurret> { autoTurret } });
                if (justCreated && permission.UserHasPermission(autoTurret.OwnerID.ToString(), PERMISSION_USE))
                    AuthToTurret(new HashSet<AutoTurret> { autoTurret }, autoTurret.OwnerID, true);
            }
        }

        private void OnEntityKill(BaseCombatEntity entity)
        {
            if (entity == null || !entity.OwnerID.IsSteamId()) return;
            var buildingPrivlidge = entity as BuildingPrivlidge;
            if (buildingPrivlidge != null)
            {
                foreach (var entry in playerEntities)
                {
                    if (entry.Value.buildingPrivlidges.Remove(buildingPrivlidge))
                    {
                        return; ;
                    }
                }
                return;
            }

            var autoTurret = entity as AutoTurret;
            if (autoTurret != null)
            {
                foreach (var entry in playerEntities)
                {
                    if (entry.Value.autoTurrets.Remove(autoTurret))
                    {
                        return;
                    }
                }
            }
        }

        private object CanUseLockedEntity(BasePlayer player, BaseLock baseLock)
        {
            var parentEntity = baseLock?.GetParentEntity();
            if (player == null || parentEntity == null || !parentEntity.OwnerID.IsSteamId() || !baseLock.IsLocked()) return null;
            if (!permission.UserHasPermission(parentEntity.OwnerID.ToString(), PERMISSION_USE)) return null;
            var shareData = GetShareData(parentEntity.OwnerID, true);
            if (shareData.friendsShare.enabled && HasFriend(parentEntity.OwnerID, player.userID))
            {
                if (baseLock is KeyLock && shareData.friendsShare.keyLock && CanUnlockEntity(parentEntity, configData.friendsShareS.keyLockS))
                    return true;
                var codeLock = baseLock as CodeLock;
                if (codeLock != null && shareData.friendsShare.codeLock && CanUnlockEntity(parentEntity, configData.friendsShareS.codeLockS))
                    return SendUnlockedEffect(codeLock);
            }
            if (shareData.clanShare.enabled && SameClan(parentEntity.OwnerID, player.userID))
            {
                if (baseLock is KeyLock && shareData.clanShare.keyLock && CanUnlockEntity(parentEntity, configData.clanShareS.keyLockS))
                    return true;
                var codeLock = baseLock as CodeLock;
                if (codeLock != null && shareData.clanShare.codeLock && CanUnlockEntity(parentEntity, configData.clanShareS.codeLockS))
                    return SendUnlockedEffect(codeLock);
            }
            if (shareData.teamShare.enabled && SameTeam(parentEntity.OwnerID, player.userID))
            {
                if (baseLock is KeyLock && shareData.teamShare.keyLock && CanUnlockEntity(parentEntity, configData.teamShareS.keyLockS))
                    return true;
                var codeLock = baseLock as CodeLock;
                if (codeLock != null && shareData.teamShare.codeLock && CanUnlockEntity(parentEntity, configData.teamShareS.codeLockS))
                    return SendUnlockedEffect(codeLock);
            }
            return null;
        }

        private static bool CanUnlockEntity(BaseEntity parentEntity, ConfigData.LockSettings lockSettings)
        {
            if (parentEntity is Door)
            {
                return lockSettings.shareDoor;
            }
            if (parentEntity is BoxStorage)
            {
                return lockSettings.shareBox;
            }
            return lockSettings.shareOtherEntity;
        }

        private static bool SendUnlockedEffect(CodeLock codeLock)
        {
            Effect.server.Run(codeLock.effectUnlocked.resourcePath, codeLock.transform.position);
            return true;
        }

        #endregion Oxide Hooks

        #region Methods

        private enum AutoAuthType
        {
            All,
            Turret,
            Cupboard,
        }

        private void UpdateAuthList(ulong playerID, AutoAuthType autoAuthType)
        {
            if (!permission.UserHasPermission(playerID.ToString(), PERMISSION_USE)) return;
            EntityCache entityCache;
            if (!playerEntities.TryGetValue(playerID, out entityCache)) return;
            switch (autoAuthType)
            {
                case AutoAuthType.All:
                    AuthToCupboard(entityCache.buildingPrivlidges, playerID);
                    AuthToTurret(entityCache.autoTurrets, playerID);
                    return;

                case AutoAuthType.Turret:
                    AuthToTurret(entityCache.autoTurrets, playerID);
                    return;

                case AutoAuthType.Cupboard:
                    AuthToCupboard(entityCache.buildingPrivlidges, playerID);
                    return;
            }
        }

        private void AuthToCupboard(HashSet<BuildingPrivlidge> buildingPrivlidges, ulong playerID, bool justCreated = false)
        {
            if (buildingPrivlidges.Count <= 0) return;
            var authList = GetPlayerNameIDs(playerID, AutoAuthType.Cupboard);
            foreach (var buildingPrivlidge in buildingPrivlidges)
            {
                if (buildingPrivlidge == null || buildingPrivlidge.IsDestroyed) continue;
                buildingPrivlidge.authorizedPlayers.Clear();
                foreach (var friend in authList)
                {
                    buildingPrivlidge.authorizedPlayers.Add(friend);
                }
                buildingPrivlidge.SendNetworkUpdateImmediate();
            }
            var player = RustCore.FindPlayerById(playerID);
            if (player == null) return;
            if (justCreated && configData.chatS.sendMessage && authList.Count > 1)
                Print(player, Lang("CupboardSuccess", player.UserIDString, authList.Count - 1, buildingPrivlidges.Count));
        }

        private void AuthToTurret(HashSet<AutoTurret> autoTurrets, ulong playerID, bool justCreated = false)
        {
            if (autoTurrets.Count <= 0) return;
            var authList = GetPlayerNameIDs(playerID, AutoAuthType.Turret);
            foreach (var autoTurret in autoTurrets)
            {
                if (autoTurret == null || autoTurret.IsDestroyed) continue;
                bool isOnline = false;
                if (autoTurret.IsOnline())
                {
                    autoTurret.SetIsOnline(false);
                    isOnline = true;
                }
                autoTurret.authorizedPlayers.Clear();
                foreach (var friend in authList)
                {
                    autoTurret.authorizedPlayers.Add(friend);
                }
                if (isOnline) autoTurret.SetIsOnline(true);
                autoTurret.SendNetworkUpdateImmediate();
            }
            var player = RustCore.FindPlayerById(playerID);
            if (player == null) return;
            if (justCreated && configData.chatS.sendMessage && authList.Count > 1)
                Print(player, Lang("TurretSuccess", player.UserIDString, authList.Count - 1, autoTurrets.Count));
        }

        private List<PlayerNameID> GetPlayerNameIDs(ulong playerID, AutoAuthType autoAuthType)
        {
            var authList = GetAuthList(playerID, autoAuthType);
            return authList.Select(auth => new PlayerNameID { userid = auth, username = RustCore.FindPlayerById(auth)?.displayName ?? string.Empty, ShouldPool = true }).ToList();
        }

        private IEnumerable<ulong> GetAuthList(ulong playerID, AutoAuthType autoAuthType)
        {
            var shareData = GetShareData(playerID, true);
            var sharePlayers = new HashSet<ulong> { playerID };
            if (shareData.friendsShare.enabled && (autoAuthType == AutoAuthType.Turret ? shareData.friendsShare.turret : shareData.friendsShare.cupboard))
            {
                var friends = GetFriends(playerID);
                foreach (var friend in friends)
                    sharePlayers.Add(friend);
            }
            if (shareData.clanShare.enabled && (autoAuthType == AutoAuthType.Turret ? shareData.clanShare.turret : shareData.clanShare.cupboard))
            {
                var clanMembers = GetClanMembers(playerID);
                foreach (var member in clanMembers)
                    sharePlayers.Add(member);
            }
            if (shareData.teamShare.enabled && (autoAuthType == AutoAuthType.Turret ? shareData.teamShare.turret : shareData.teamShare.cupboard))
            {
                var teamMembers = GetTeamMembers(playerID);
                foreach (var member in teamMembers)
                    sharePlayers.Add(member);
            }
            return sharePlayers;
        }

        private StoredData.ShareData GetShareData(ulong playerID, bool readOnly = false)
        {
            StoredData.ShareData shareData;
            if (!storedData.playerShareData.TryGetValue(playerID, out shareData))
            {
                shareData = new StoredData.ShareData
                {
                    friendsShare = new StoredData.ShareEntry
                    {
                        enabled = configData.friendsShareS.enabled,
                        turret = configData.friendsShareS.shareTurret,
                        cupboard = configData.friendsShareS.shareCupboard,
                        keyLock = configData.friendsShareS.keyLockS.enabled,
                        codeLock = configData.friendsShareS.codeLockS.enabled,
                    },
                    clanShare = new StoredData.ShareEntry
                    {
                        enabled = configData.clanShareS.enabled,
                        turret = configData.clanShareS.shareTurret,
                        cupboard = configData.clanShareS.shareCupboard,
                        keyLock = configData.clanShareS.keyLockS.enabled,
                        codeLock = configData.clanShareS.codeLockS.enabled,
                    },
                    teamShare = new StoredData.ShareEntry
                    {
                        enabled = configData.teamShareS.enabled,
                        turret = configData.teamShareS.shareTurret,
                        cupboard = configData.teamShareS.shareCupboard,
                        keyLock = configData.teamShareS.keyLockS.enabled,
                        codeLock = configData.teamShareS.codeLockS.enabled,
                    }
                };
                if (readOnly)
                {
                    return shareData;
                }
                storedData.playerShareData.Add(playerID, shareData);
            }
            return shareData;
        }

        private void UpdateData()
        {
            foreach (var entry in storedData.playerShareData)
            {
                if (!configData.friendsShareS.enabled) entry.Value.friendsShare.enabled = false;
                if (!configData.friendsShareS.shareCupboard) entry.Value.friendsShare.cupboard = false;
                if (!configData.friendsShareS.shareTurret) entry.Value.friendsShare.turret = false;
                if (!configData.friendsShareS.keyLockS.enabled) entry.Value.friendsShare.keyLock = false;
                if (!configData.friendsShareS.codeLockS.enabled) entry.Value.friendsShare.codeLock = false;

                if (!configData.clanShareS.enabled) entry.Value.clanShare.enabled = false;
                if (!configData.clanShareS.shareCupboard) entry.Value.clanShare.cupboard = false;
                if (!configData.clanShareS.shareTurret) entry.Value.clanShare.turret = false;
                if (!configData.clanShareS.keyLockS.enabled) entry.Value.clanShare.keyLock = false;
                if (!configData.clanShareS.codeLockS.enabled) entry.Value.clanShare.codeLock = false;

                if (!configData.teamShareS.enabled) entry.Value.teamShare.enabled = false;
                if (!configData.teamShareS.shareCupboard) entry.Value.teamShare.cupboard = false;
                if (!configData.teamShareS.shareTurret) entry.Value.teamShare.turret = false;
                if (!configData.teamShareS.keyLockS.enabled) entry.Value.teamShare.keyLock = false;
                if (!configData.teamShareS.codeLockS.enabled) entry.Value.teamShare.codeLock = false;
            }
            SaveData();
        }

        private IEnumerable<ShareType> GetAvailableTypes()
        {
            if (configData.teamShareS.enabled && RelationshipManager.TeamsEnabled()) yield return ShareType.Teams;
            if (configData.friendsShareS.enabled && Friends != null) yield return ShareType.Friends;
            if (configData.clanShareS.enabled && Clans != null) yield return ShareType.Clans;
        }

        #endregion Methods

        #region External Plugins

        #region Teams

        private void OnTeamLeave(RelationshipManager.PlayerTeam playerTeam, BasePlayer player)
        {
            NextTick(() =>
            {
                if (playerTeam == null || player == null) return;
                if (!playerTeam.members.Contains(player.userID))
                    UpdateTeamAuthList(playerTeam.members);
            });
        }

        private void OnTeamAcceptInvite(RelationshipManager.PlayerTeam playerTeam, BasePlayer player)
        {
            NextTick(() =>
            {
                if (playerTeam == null || player == null) return;
                if (playerTeam.members.Contains(player.userID))
                    UpdateTeamAuthList(playerTeam.members);
            });
        }

        private void OnTeamKick(RelationshipManager.PlayerTeam playerTeam, BasePlayer player, ulong target)
        {
            NextTick(() =>
            {
                if (playerTeam == null) return;
                if (!playerTeam.members.Contains(target))
                    UpdateTeamAuthList(playerTeam.members);
            });
        }

        private void UpdateTeamAuthList(List<ulong> teamMembers)
        {
            if (teamMembers.Count <= 0) return;
            foreach (var member in teamMembers)
                UpdateAuthList(member, AutoAuthType.All);
        }

        private static List<ulong> GetTeamMembers(ulong playerID)
        {
            if (!RelationshipManager.TeamsEnabled()) return new List<ulong>();
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam != null) return playerTeam.members;
            return new List<ulong>();
        }

        private static bool SameTeam(ulong playerID, ulong friendID)
        {
            if (!RelationshipManager.TeamsEnabled()) return false;
            var playerTeam = RelationshipManager.Instance.FindPlayersTeam(playerID);
            if (playerTeam == null) return false;
            var friendTeam = RelationshipManager.Instance.FindPlayersTeam(friendID);
            if (friendTeam == null) return false;
            return playerTeam == friendTeam;
        }

        #endregion Teams

        #region Friends

        private void OnFriendAdded(string playerID, string friendID) => UpdateFriendAuthList(playerID);

        private void OnFriendRemoved(string playerID, string friendID) => UpdateFriendAuthList(playerID);

        private void UpdateFriendAuthList(string playerID) => UpdateAuthList(ulong.Parse(playerID), AutoAuthType.All);

        private List<ulong> GetFriends(ulong playerID)
        {
            if (Friends == null) return new List<ulong>();
            var friends = Friends.Call("GetFriends", playerID);
            if (friends != null && friends is ulong[])
                return (friends as ulong[]).ToList();
            return new List<ulong>();
        }

        private bool HasFriend(ulong playerID, ulong friendID)
        {
            if (Friends == null) return false;
            var hasFriend = Friends.Call("HasFriend", playerID, friendID);
            if (hasFriend != null && (bool)hasFriend) return true;
            return false;
        }

        #endregion Friends

        #region Clans

        private void OnClanDestroy(string clanName) => UpdateClanAuthList(clanName);

        private void OnClanUpdate(string clanName) => UpdateClanAuthList(clanName);

        private void UpdateClanAuthList(string clanName)
        {
            var clanMembers = GetClanMembers(clanName);
            foreach (var member in clanMembers)
                UpdateAuthList(member, AutoAuthType.All);
        }

        private List<ulong> GetClanMembers(ulong playerID)
        {
            if (Clans == null) return new List<ulong>();
            var clanName = Clans.Call("GetClanOf", playerID);
            if (clanName != null && clanName is string)
                return GetClanMembers((string)clanName);
            return new List<ulong>();
        }

        private List<ulong> GetClanMembers(string clanName)
        {
            var clan = Clans.Call("GetClan", clanName);
            if (clan != null && clan is JObject)
            {
                var members = (clan as JObject).GetValue("members");
                if (members != null && members is JArray)
                    return ((JArray)members).Select(x => ulong.Parse(x.ToString())).ToList();
            }
            return new List<ulong>();
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

        #endregion Clans

        #endregion External Plugins

        #region UI

        private const string UINAME_MAIN = "AutoAuthUI_Main";
        private const string UINAME_MENU = "AutoAuthUI_Menu";

        private void CreateMainUI(BasePlayer player)
        {
            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.6" },
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-380 -200", OffsetMax = "380 260" },
                CursorEnabled = true
            }, "Hud", UINAME_MAIN);
            var titlePanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.42 0.88 0.88 1" },
                RectTransform = { AnchorMin = "0 0.902", AnchorMax = "1 1" },
            }, UINAME_MAIN);
            container.Add(new CuiElement
            {
                Parent = titlePanel,
                Components =
                {
                    new CuiTextComponent { Text = Lang("UI_Title", player.UserIDString), FontSize = 20, Align = TextAnchor.MiddleCenter, Color ="1 0 0 1" },
                    new CuiOutlineComponent { Distance = "0.5 0.5", Color = "1 1 1 1" },
                    new CuiRectTransformComponent { AnchorMin = "0.2 0",  AnchorMax = "0.8 1" }
                }
            });
            container.Add(new CuiButton
            {
                Button = { Color = "0.95 0.1 0.1 0.95", Close = UINAME_MAIN },
                Text = { Text = "X", Align = TextAnchor.MiddleCenter, Color = "0 0 0 1", FontSize = 22 },
                RectTransform = { AnchorMin = "0.885 0.05", AnchorMax = "0.995 0.95" }
            }, titlePanel);
            container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.4" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.898" },
            }, UINAME_MAIN, UINAME_MENU);
            CuiHelper.DestroyUi(player, UINAME_MAIN);
            CuiHelper.AddUi(player, container);
            var shareData = GetShareData(player.userID, true);
            UpdateMenuUI(player, shareData);
        }

        private void UpdateMenuUI(BasePlayer player, StoredData.ShareData shareData, ShareType type = ShareType.None)
        {
            if (player == null) return;
            var container = new CuiElementContainer();
            var availableTypes = GetAvailableTypes();
            var total = availableTypes.Count();
            if (total <= 0) return;
            int i = 0;

            #region Teams UI

            if (availableTypes.Contains(ShareType.Teams))
            {
                if ((type == ShareType.None || type == ShareType.Teams))
                {
                    var anchors = GetMenuSubAnchors(i, total);
                    CuiHelper.DestroyUi(player, UINAME_MENU + ShareType.Teams);
                    CreateMenuSubUI(ref container, shareData.teamShare, player.UserIDString, ShareType.Teams,
                        $"{anchors[0]} 0.05", $"{anchors[1]} 0.95");
                }
                i++;
            }

            #endregion Teams UI

            #region Friends UI

            if (availableTypes.Contains(ShareType.Friends))
            {
                if ((type == ShareType.None || type == ShareType.Friends))
                {
                    var anchors = GetMenuSubAnchors(i, total);
                    CuiHelper.DestroyUi(player, UINAME_MENU + ShareType.Friends);
                    CreateMenuSubUI(ref container, shareData.friendsShare, player.UserIDString, ShareType.Friends,
                        $"{anchors[0]} 0.05", $"{anchors[1]} 0.95");
                }
                i++;
            }

            #endregion Friends UI

            #region Clans UI

            if (availableTypes.Contains(ShareType.Clans))
            {
                if ((type == ShareType.None || type == ShareType.Clans))
                {
                    var anchors = GetMenuSubAnchors(i, total);
                    CuiHelper.DestroyUi(player, UINAME_MENU + ShareType.Clans);
                    CreateMenuSubUI(ref container, shareData.clanShare, player.UserIDString, ShareType.Clans,
                        $"{anchors[0]} 0.05", $"{anchors[1]} 0.95");
                }
            }

            #endregion Clans UI

            CuiHelper.AddUi(player, container);
        }

        private void CreateMenuSubUI(ref CuiElementContainer container, StoredData.ShareEntry shareEntry, string playerID, ShareType type, string anchorMin, string anchorMax)
        {
            var panelName = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
            }, UINAME_MENU, UINAME_MENU + type);
            var titlePanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = "0 0.85", AnchorMax = "1 1" },
            }, panelName);
            container.Add(new CuiLabel
            {
                Text = { Color = "0 1 1 1", FontSize = 18, Align = TextAnchor.MiddleCenter, Text = Lang($"UI_{type}Title", playerID) },
                RectTransform = { AnchorMin = "0.1 0", AnchorMax = "0.795 1" }
            }, titlePanel);
            var contentPanel = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "0.995 0.845" },
            }, panelName);
            int i = 0;
            var spacing = 1f / 5;
            var anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, contentPanel, $"AutoAuthUI {type}", Lang($"UI_{type}Share", playerID),
                shareEntry.enabled ? Lang("Enabled", playerID) : Lang("Disabled", playerID), $"0 {anchors[0]}",
                $"0.995 {anchors[1]}");
            anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, contentPanel, $"AutoAuthUI {type} Cupboard", Lang($"UI_{type}Cupboard", playerID),
                shareEntry.cupboard ? Lang("Enabled", playerID) : Lang("Disabled", playerID), $"0 {anchors[0]}",
                $"0.995 {anchors[1]}");
            anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, contentPanel, $"AutoAuthUI {type} Turret", Lang($"UI_{type}Turret", playerID),
                shareEntry.turret ? Lang("Enabled", playerID) : Lang("Disabled", playerID), $"0 {anchors[0]}",
                $"0.995 {anchors[1]}");
            anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, contentPanel, $"AutoAuthUI {type} KeyLock", Lang($"UI_{type}KeyLock", playerID),
                shareEntry.keyLock ? Lang("Enabled", playerID) : Lang("Disabled", playerID), $"0 {anchors[0]}",
                $"0.995 {anchors[1]}");
            anchors = GetEntryAnchors(i++, spacing);
            CreateEntry(ref container, contentPanel, $"AutoAuthUI {type} CodeLock", Lang($"UI_{type}CodeLock", playerID),
                shareEntry.codeLock ? Lang("Enabled", playerID) : Lang("Disabled", playerID), $"0 {anchors[0]}",
                $"0.995 {anchors[1]}");
        }

        private static void CreateEntry(ref CuiElementContainer container, string parentName, string command, string leftText, string rightText, string anchorMin, string anchorMax)
        {
            var panelName = container.Add(new CuiPanel
            {
                Image = { Color = "0.1 0.1 0.1 0.6" },
                RectTransform = { AnchorMin = anchorMin, AnchorMax = anchorMax },
            }, parentName);
            container.Add(new CuiLabel
            {
                Text = { Color = "0 1 1 1", FontSize = 14, Align = TextAnchor.MiddleLeft, Text = leftText },
                RectTransform = { AnchorMin = "0.1 0", AnchorMax = "0.695 1" }
            }, panelName);
            container.Add(new CuiButton
            {
                Button = { Color = "0 0 0 0.7", Command = command },
                Text = { Text = rightText, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1", FontSize = 14 },
                RectTransform = { AnchorMin = "0.7 0.01", AnchorMax = "0.995 0.99" },
            }, panelName);
        }

        private static float[] GetEntryAnchors(int i, float spacing)
        {
            return new[] { 1f - (i + 1) * spacing, 1f - i * spacing };
        }

        private static float[] GetMenuSubAnchors(int i, int total)
        {
            switch (total)
            {
                case 1:
                    return new[] { 0.3f, 0.7f };

                case 2:
                    return i == 0 ? new[] { 0.15f, 0.48f } : new[] { 0.52f, 0.85f };

                case 3:
                    switch (i)
                    {
                        case 0:
                            return new[] { 0.02f, 0.32f };

                        case 1:
                            return new[] { 0.335f, 0.665f };

                        default:
                            return new[] { 0.68f, 0.98f };
                    }

                default:
                    return null;
            }
        }

        private static void DestroyUI(BasePlayer player) => CuiHelper.DestroyUi(player, UINAME_MAIN);

        #endregion UI

        #region Chat Commands

        private void CmdAutoAuth(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            var shareData = GetShareData(player.userID);
            var availableTypes = GetAvailableTypes();
            if (args == null || args.Length == 0)
            {
                if (!availableTypes.Any())
                {
                    Print(player, Lang("UnableAutoAuth", player.UserIDString));
                    return;
                }
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine();
                if (availableTypes.Contains(ShareType.Teams))
                {
                    stringBuilder.AppendLine(Lang("AutoShareTeamsStatus", player.UserIDString));
                    stringBuilder.AppendLine(Lang("AutoShareTeams", player.UserIDString, shareData.teamShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareTeamsCupboard", player.UserIDString, shareData.teamShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareTeamsTurret", player.UserIDString, shareData.teamShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareTeamsKeyLock", player.UserIDString, shareData.teamShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareTeamsCodeLock", player.UserIDString, shareData.teamShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                }
                if (availableTypes.Contains(ShareType.Friends))
                {
                    stringBuilder.AppendLine(Lang("AutoShareFriendsStatus", player.UserIDString));
                    stringBuilder.AppendLine(Lang("AutoShareFriends", player.UserIDString, shareData.friendsShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareFriendsCupboard", player.UserIDString, shareData.friendsShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareFriendsTurret", player.UserIDString, shareData.friendsShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareFriendsKeyLock", player.UserIDString, shareData.friendsShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareFriendsCodeLock", player.UserIDString, shareData.friendsShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                }
                if (availableTypes.Contains(ShareType.Clans))
                {
                    stringBuilder.AppendLine(Lang("AutoShareClansStatus", player.UserIDString));
                    stringBuilder.AppendLine(Lang("AutoShareClans", player.UserIDString, shareData.clanShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareClansCupboard", player.UserIDString, shareData.clanShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareClansTurret", player.UserIDString, shareData.clanShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareClansKeyLock", player.UserIDString, shareData.clanShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                    stringBuilder.AppendLine(Lang("AutoShareClansCodeLock", player.UserIDString, shareData.clanShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                }
                Print(player, stringBuilder.ToString());
                return;
            }
            switch (args[0].ToLower())
            {
                case "at":
                case "autoteam":
                    if (!availableTypes.Contains(ShareType.Teams))
                    {
                        Print(player, Lang("TeamsDisabled", player.UserIDString));
                        return;
                    }
                    if (args.Length <= 1)
                    {
                        shareData.teamShare.enabled = !shareData.teamShare.enabled;
                        Print(player, Lang("Teams", player.UserIDString, shareData.teamShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        return;
                    }
                    switch (args[1].ToLower())
                    {
                        case "c":
                        case "cupboard":
                            if (!configData.clanShareS.shareCupboard)
                            {
                                Print(player, Lang("TeamsCupboardDisable", player.UserIDString));
                                return;
                            }
                            shareData.teamShare.cupboard = !shareData.teamShare.cupboard;
                            Print(player, Lang("TeamsCupboard", player.UserIDString, shareData.teamShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            return;

                        case "t":
                        case "turret":
                            if (!configData.clanShareS.shareTurret)
                            {
                                Print(player, Lang("TeamsTurretDisable", player.UserIDString));
                                return;
                            }
                            shareData.teamShare.turret = !shareData.teamShare.turret;
                            Print(player, Lang("TeamsTurret", player.UserIDString, shareData.teamShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            return;

                        case "kl":
                        case "keylock":
                            if (!configData.clanShareS.keyLockS.enabled)
                            {
                                Print(player, Lang("TeamsKeyLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.teamShare.keyLock = !shareData.teamShare.keyLock;
                            Print(player, Lang("TeamsKeyLock", player.UserIDString, shareData.teamShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            return;

                        case "cl":
                        case "codelock":
                            if (!configData.clanShareS.codeLockS.enabled)
                            {
                                Print(player, Lang("TeamsCodeLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.teamShare.codeLock = !shareData.teamShare.codeLock;
                            Print(player, Lang("TeamsCodeLock", player.UserIDString, shareData.teamShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));

                            return;

                        case "h":
                        case "help":
                            StringBuilder stringBuilder1 = new StringBuilder();
                            stringBuilder1.AppendLine();
                            stringBuilder1.AppendLine(Lang("TeamsSyntax", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("TeamsSyntax1", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("TeamsSyntax2", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("TeamsSyntax3", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("TeamsSyntax4", player.UserIDString, configData.chatS.chatCommand));
                            Print(player, stringBuilder1.ToString());
                            return;
                    }
                    Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.chatCommand));
                    return;

                case "af":
                case "autofriends":
                    if (!availableTypes.Contains(ShareType.Friends))
                    {
                        Print(player, Lang("FriendsDisabled", player.UserIDString));
                        return;
                    }
                    if (args.Length <= 1)
                    {
                        shareData.friendsShare.enabled = !shareData.friendsShare.enabled;
                        Print(player, Lang("Friends", player.UserIDString, shareData.friendsShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        return;
                    }
                    switch (args[1].ToLower())
                    {
                        case "c":
                        case "cupboard":
                            if (!configData.friendsShareS.shareCupboard)
                            {
                                Print(player, Lang("FriendsCupboardDisabled", player.UserIDString));
                                return;
                            }
                            shareData.friendsShare.cupboard = !shareData.friendsShare.cupboard;
                            Print(player, Lang("FriendsCupboard", player.UserIDString, shareData.friendsShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            return;

                        case "t":
                        case "turret":
                            if (!configData.friendsShareS.shareTurret)
                            {
                                Print(player, Lang("FriendsTurretDisable", player.UserIDString));
                                return;
                            }
                            shareData.friendsShare.turret = !shareData.friendsShare.turret;
                            Print(player, Lang("FriendsTurret", player.UserIDString, shareData.friendsShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            return;

                        case "kl":
                        case "keylock":
                            if (!configData.friendsShareS.keyLockS.enabled)
                            {
                                Print(player, Lang("FriendsKeyLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.friendsShare.keyLock = !shareData.friendsShare.keyLock;
                            Print(player, Lang("FriendsKeyLock", player.UserIDString, shareData.friendsShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            return;

                        case "cl":
                        case "codelock":
                            if (!configData.friendsShareS.codeLockS.enabled)
                            {
                                Print(player, Lang("FriendsCodeLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.friendsShare.codeLock = !shareData.friendsShare.codeLock;
                            Print(player, Lang("FriendsCodeLock", player.UserIDString, shareData.friendsShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            return;

                        case "h":
                        case "help":
                            StringBuilder stringBuilder1 = new StringBuilder();
                            stringBuilder1.AppendLine();
                            stringBuilder1.AppendLine(Lang("FriendsSyntax", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("FriendsSyntax1", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("FriendsSyntax2", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("FriendsSyntax3", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("FriendsSyntax4", player.UserIDString, configData.chatS.chatCommand));
                            Print(player, stringBuilder1.ToString());
                            return;
                    }
                    Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.chatCommand));
                    return;

                case "ac":
                case "autoclan":
                    if (!availableTypes.Contains(ShareType.Clans))
                    {
                        Print(player, Lang("ClansDisabled", player.UserIDString));
                        return;
                    }
                    if (args.Length <= 1)
                    {
                        shareData.clanShare.enabled = !shareData.clanShare.enabled;
                        Print(player, Lang("Clans", player.UserIDString, shareData.clanShare.enabled ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        return;
                    }
                    switch (args[1].ToLower())
                    {
                        case "c":
                        case "cupboard":
                            if (!configData.clanShareS.shareCupboard)
                            {
                                Print(player, Lang("ClansCupboardDisable", player.UserIDString));
                                return;
                            }
                            shareData.clanShare.cupboard = !shareData.clanShare.cupboard;
                            Print(player, Lang("ClansCupboard", player.UserIDString, shareData.clanShare.cupboard ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            return;

                        case "t":
                        case "turret":
                            if (!configData.clanShareS.shareTurret)
                            {
                                Print(player, Lang("ClansTurretDisable", player.UserIDString));
                                return;
                            }
                            shareData.clanShare.turret = !shareData.clanShare.turret;
                            Print(player, Lang("ClansTurret", player.UserIDString, shareData.clanShare.turret ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            return;

                        case "kl":
                        case "keylock":
                            if (!configData.clanShareS.keyLockS.enabled)
                            {
                                Print(player, Lang("ClansKeyLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.clanShare.keyLock = !shareData.clanShare.keyLock;
                            Print(player, Lang("ClansKeyLock", player.UserIDString, shareData.clanShare.keyLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            return;

                        case "cl":
                        case "codelock":
                            if (!configData.clanShareS.codeLockS.enabled)
                            {
                                Print(player, Lang("ClansCodeLockDisable", player.UserIDString));
                                return;
                            }
                            shareData.clanShare.codeLock = !shareData.clanShare.codeLock;
                            Print(player, Lang("ClansCodeLock", player.UserIDString, shareData.clanShare.codeLock ? Lang("Enabled", player.UserIDString) : Lang("Disabled", player.UserIDString)));
                            return;

                        case "h":
                        case "help":
                            StringBuilder stringBuilder1 = new StringBuilder();
                            stringBuilder1.AppendLine();
                            stringBuilder1.AppendLine(Lang("ClansSyntax", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("ClansSyntax1", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("ClansSyntax2", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("ClansSyntax3", player.UserIDString, configData.chatS.chatCommand));
                            stringBuilder1.AppendLine(Lang("ClansSyntax4", player.UserIDString, configData.chatS.chatCommand));
                            Print(player, stringBuilder1.ToString());
                            return;
                    }
                    Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.chatCommand));
                    return;

                case "h":
                case "help":
                    if (!availableTypes.Any())
                    {
                        Print(player, Lang("UnableAutoAuth", player.UserIDString));
                        return;
                    }
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine();
                    if (availableTypes.Contains(ShareType.Teams))
                    {
                        stringBuilder.AppendLine(Lang("TeamsSyntax", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("TeamsSyntax1", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("TeamsSyntax2", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("TeamsSyntax3", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("TeamsSyntax4", player.UserIDString, configData.chatS.chatCommand));
                    }
                    if (availableTypes.Contains(ShareType.Friends))
                    {
                        stringBuilder.AppendLine(Lang("FriendsSyntax", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("FriendsSyntax1", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("FriendsSyntax2", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("FriendsSyntax3", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("FriendsSyntax4", player.UserIDString, configData.chatS.chatCommand));
                    }
                    if (availableTypes.Contains(ShareType.Clans))
                    {
                        stringBuilder.AppendLine(Lang("ClansSyntax", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("ClansSyntax1", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("ClansSyntax2", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("ClansSyntax3", player.UserIDString, configData.chatS.chatCommand));
                        stringBuilder.AppendLine(Lang("ClansSyntax4", player.UserIDString, configData.chatS.chatCommand));
                    }
                    stringBuilder.AppendLine(Lang("UISyntax", player.UserIDString, configData.chatS.uiCommand));
                    Print(player, stringBuilder.ToString());
                    return;

                default:
                    Print(player, Lang("SyntaxError", player.UserIDString, configData.chatS.chatCommand));
                    return;
            }
        }

        private void CmdAutoAuthUI(BasePlayer player, string command, string[] args)
        {
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE))
            {
                Print(player, Lang("NotAllowed", player.UserIDString));
                return;
            }
            CreateMainUI(player);
        }

        [ConsoleCommand("AutoAuthUI")]
        private void CCmdAutoAuthUI(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            if (player == null) return;
            if (!permission.UserHasPermission(player.UserIDString, PERMISSION_USE)) return;
            var shareData = GetShareData(player.userID);
            switch (arg.Args[0].ToLower())
            {
                case "teams":
                    if (!configData.teamShareS.enabled) return;
                    if (arg.Args.Length <= 1)
                    {
                        shareData.teamShare.enabled = !shareData.teamShare.enabled;
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        UpdateMenuUI(player, shareData, ShareType.Teams);
                        return;
                    }
                    switch (arg.Args[1].ToLower())
                    {
                        case "cupboard":
                            shareData.teamShare.cupboard = !shareData.teamShare.cupboard;
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            break;

                        case "turret":
                            shareData.teamShare.turret = !shareData.teamShare.turret;
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            break;

                        case "keylock":
                            shareData.teamShare.keyLock = !shareData.teamShare.keyLock;
                            break;

                        case "codelock":
                            shareData.teamShare.codeLock = !shareData.teamShare.codeLock;
                            break;

                        default: return;
                    }
                    UpdateMenuUI(player, shareData, ShareType.Teams);
                    return;

                case "friends":
                    if (!configData.friendsShareS.enabled) return;
                    if (arg.Args.Length <= 1)
                    {
                        shareData.friendsShare.enabled = !shareData.friendsShare.enabled;
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        UpdateMenuUI(player, shareData, ShareType.Friends);
                        return;
                    }
                    switch (arg.Args[1].ToLower())
                    {
                        case "cupboard":
                            shareData.friendsShare.cupboard = !shareData.friendsShare.cupboard;
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            break;

                        case "turret":
                            shareData.friendsShare.turret = !shareData.friendsShare.turret;
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            break;

                        case "keylock":
                            shareData.friendsShare.keyLock = !shareData.friendsShare.keyLock;
                            break;

                        case "codelock":
                            shareData.friendsShare.codeLock = !shareData.friendsShare.codeLock;
                            break;

                        default: return;
                    }
                    UpdateMenuUI(player, shareData, ShareType.Friends);
                    return;

                case "clans":
                    if (!configData.clanShareS.enabled) return;
                    if (arg.Args.Length <= 1)
                    {
                        shareData.clanShare.enabled = !shareData.clanShare.enabled;
                        UpdateAuthList(player.userID, AutoAuthType.All);
                        UpdateMenuUI(player, shareData, ShareType.Clans);
                        return;
                    }
                    switch (arg.Args[1].ToLower())
                    {
                        case "cupboard":
                            shareData.clanShare.cupboard = !shareData.clanShare.cupboard;
                            UpdateAuthList(player.userID, AutoAuthType.Cupboard);
                            break;

                        case "turret":
                            shareData.clanShare.turret = !shareData.clanShare.turret;
                            UpdateAuthList(player.userID, AutoAuthType.Turret);
                            break;

                        case "keylock":
                            shareData.clanShare.keyLock = !shareData.clanShare.keyLock;
                            break;

                        case "codelock":
                            shareData.clanShare.codeLock = !shareData.clanShare.codeLock;
                            break;

                        default: return;
                    }
                    UpdateMenuUI(player, shareData, ShareType.Clans);
                    return;
            }
        }

        #endregion Chat Commands

        #region ConfigurationFile

        private ConfigData configData;

        private class ConfigData
        {
            [JsonProperty(PropertyName = "Clear Share Data On Map Wipe")]
            public bool clearDataOnWipe = false;

            [JsonProperty(PropertyName = "Team Share Settings")]
            public ShareSettings teamShareS = new ShareSettings();

            [JsonProperty(PropertyName = "Friends Share Settings")]
            public ShareSettings friendsShareS = new ShareSettings();

            [JsonProperty(PropertyName = "Clan Share Settings")]
            public ShareSettings clanShareS = new ShareSettings();

            [JsonProperty(PropertyName = "Chat Settings")]
            public ChatSettings chatS = new ChatSettings();

            [JsonProperty(PropertyName = "Version")]
            public VersionNumber version = new VersionNumber(1, 2, 0);

            public class ChatSettings
            {
                [JsonProperty(PropertyName = "Send Authorization Success Message")]
                public bool sendMessage = true;

                [JsonProperty(PropertyName = "Chat Command")]
                public string chatCommand = "autoauth";

                [JsonProperty(PropertyName = "Chat UI Command")]
                public string uiCommand = "autoauthui";

                [JsonProperty(PropertyName = "Chat Prefix")]
                public string prefix = "<color=#00FFFF>[AutoAuth]</color>: ";

                [JsonProperty(PropertyName = "Chat SteamID Icon")]
                public ulong steamIDIcon = 0;
            }

            public class ShareSettings
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled = true;

                [JsonProperty(PropertyName = "Share Cupboard")]
                public bool shareCupboard = true;

                [JsonProperty(PropertyName = "Share Turret")]
                public bool shareTurret = true;

                [JsonProperty(PropertyName = "Key Lock Settings")]
                public LockSettings keyLockS = new LockSettings();

                [JsonProperty(PropertyName = "Code Lock Settings")]
                public LockSettings codeLockS = new LockSettings();
            }

            public class LockSettings
            {
                [JsonProperty(PropertyName = "Enabled")]
                public bool enabled = true;

                [JsonProperty(PropertyName = "Share Door")]
                public bool shareDoor = true;

                [JsonProperty(PropertyName = "Share Box")]
                public bool shareBox = true;

                [JsonProperty(PropertyName = "Share Other Locked Entities")]
                public bool shareOtherEntity = true;
            }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                configData = Config.ReadObject<ConfigData>();
                if (configData == null)
                {
                    LoadDefaultConfig();
                }
                else
                {
                    UpdateConfigValues();
                }
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

        protected override void SaveConfig() => Config.WriteObject(configData, true);

        private void UpdateConfigValues()
        {
            if (configData.version < Version)
            {
                if (configData.version <= new VersionNumber(1, 2, 0))
                {
                    if (configData.chatS.prefix == "[AutoAuth]: ")
                    {
                        configData.chatS.prefix = "<color=#00FFFF>[AutoAuth]</color>: ";
                    }
                }
                configData.version = Version;
            }
        }

        #endregion ConfigurationFile

        #region DataFile

        private StoredData storedData;

        private class StoredData
        {
            public readonly Dictionary<ulong, ShareData> playerShareData = new Dictionary<ulong, ShareData>();

            public class ShareData
            {
                public ShareEntry friendsShare = new ShareEntry();
                public ShareEntry clanShare = new ShareEntry();
                public ShareEntry teamShare = new ShareEntry();
            }

            public class ShareEntry
            {
                public bool enabled;
                public bool cupboard;
                public bool turret;
                public bool keyLock;
                public bool codeLock;
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

        private void OnNewSave(string filename)
        {
            if (configData.clearDataOnWipe)
            {
                ClearData();
            }
        }

        #endregion DataFile

        #region LanguageFile

        private void Print(BasePlayer player, string message)
        {
            Player.Message(player, message, configData.chatS.prefix, configData.chatS.steamIDIcon);
        }

        private string Lang(string key, string id = null, params object[] args) => string.Format(lang.GetMessage(key, this, id), args);

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "You do not have permission to use this command",
                ["Enabled"] = "<color=#8ee700>Enabled</color>",
                ["Disabled"] = "<color=#ce422b>Disabled</color>",
                ["UnableAutoAuth"] = "Unable to automatically authorize other players",
                ["SyntaxError"] = "Syntax error, please type '<color=#ce422b>/{0} <help | h></color>' to view help",
                ["TurretSuccess"] = "Successfully added <color=#ce422b>{0}</color> friends/clan members/team members to <color=#ce422b>{1}</color> turrets auth list",
                ["CupboardSuccess"] = "Successfully added <color=#ce422b>{0}</color> friends/clan members/team members  to <color=#ce422b>{1}</color> cupboards auth list",

                ["FriendsSyntax"] = "<color=#ce422b>/{0} <autofriends | af></color> - Enable/Disable automatic authorization for your friends",
                ["FriendsSyntax1"] = "<color=#ce422b>/{0} <autofriends | af> <cupboard | c></color> - Sharing cupboard with your friends",
                ["FriendsSyntax2"] = "<color=#ce422b>/{0} <autofriends | af> <turret | t></color> - Sharing turret with your friends",
                ["FriendsSyntax3"] = "<color=#ce422b>/{0} <autofriends | af> <keylock | kl></color> - Sharing key lock with your friends",
                ["FriendsSyntax4"] = "<color=#ce422b>/{0} <autofriends | af> <codelock | cl></color> - Sharing code lock with your friends",

                ["ClansSyntax"] = "<color=#ce422b>/{0} <autoclan | ac></color> - Enable/Disable automatic authorization for your clan members",
                ["ClansSyntax1"] = "<color=#ce422b>/{0} <autoclan | ac> <cupboard | c></color> - Sharing cupboard with your clan members",
                ["ClansSyntax2"] = "<color=#ce422b>/{0} <autoclan | ac> <turret | t></color> - Sharing turret with your clan members",
                ["ClansSyntax3"] = "<color=#ce422b>/{0} <autoclan | ac> <keylock | kl></color> - Sharing key lock with your clan members",
                ["ClansSyntax4"] = "<color=#ce422b>/{0} <autoclan | ac> <codelock | cl></color> - Sharing code lock with your clan members",

                ["TeamsSyntax"] = "<color=#ce422b>/{0} <autoteam | at></color> - Enable/Disable automatic authorization for your team members",
                ["TeamsSyntax1"] = "<color=#ce422b>/{0} <autoteam | at> <cupboard | c></color> - Sharing cupboard with your team members",
                ["TeamsSyntax2"] = "<color=#ce422b>/{0} <autoteam | at> <turret | t></color> - Sharing turret with your team members",
                ["TeamsSyntax3"] = "<color=#ce422b>/{0} <autoteam | at> <keylock | kl></color> - Sharing key lock with your team members",
                ["TeamsSyntax4"] = "<color=#ce422b>/{0} <autoteam | at> <codelock | cl></color> - Sharing code lock with your team members",

                ["UISyntax"] = "<color=#ce422b>/{0}</color> - Open Automatic Authorization UI",

                ["AutoShareFriendsStatus"] = "<color=#ffa500>Current friends sharing status: </color>",
                ["AutoShareFriends"] = "Automatically sharing with friends: {0}",
                ["AutoShareFriendsCupboard"] = "Automatically sharing cupboard with friends: {0}",
                ["AutoShareFriendsTurret"] = "Automatically sharing turret with friends: {0}",
                ["AutoShareFriendsKeyLock"] = "Automatically sharing key lock with friends: {0}",
                ["AutoShareFriendsCodeLock"] = "Automatically sharing code lock with friends: {0}",

                ["AutoShareClansStatus"] = "<color=#ffa500>Current clan sharing status: </color>",
                ["AutoShareClans"] = "Automatically sharing with clan: {0}",
                ["AutoShareClansCupboard"] = "Automatically sharing cupboard with clan: {0}",
                ["AutoShareClansTurret"] = "Automatically sharing turret with clan: {0}",
                ["AutoShareClansKeyLock"] = "Automatically sharing key lock with clan: {0}",
                ["AutoShareClansCodeLock"] = "Automatically sharing code lock with clan: {0}",

                ["AutoShareTeamsStatus"] = "<color=#ffa500>Current Team sharing status: </color>",
                ["AutoShareTeams"] = "Automatically sharing with Team: {0}",
                ["AutoShareTeamsCupboard"] = "Automatically sharing cupboard with Team: {0}",
                ["AutoShareTeamsTurret"] = "Automatically sharing turret with Team: {0}",
                ["AutoShareTeamsKeyLock"] = "Automatically sharing key lock with Team: {0}",
                ["AutoShareTeamsCodeLock"] = "Automatically sharing code lock with Team: {0}",

                ["Friends"] = "Friends automatic authorization {0}",
                ["FriendsCupboard"] = "Sharing cupboard with friends is {0}",
                ["FriendsTurret"] = "Sharing turret with friends is {0}",
                ["FriendsKeyLock"] = "Sharing key lock with friends is {0}",
                ["FriendsCodeLock"] = "Sharing code lock with friends is {0}",

                ["Clans"] = "Clan automatic authorization {0}",
                ["ClansCupboard"] = "Sharing cupboard with clan is {0}",
                ["ClansTurret"] = "Sharing turret with clan is {0}",
                ["ClansKeyLock"] = "Sharing key lock with clan is {0}",
                ["ClansCodeLock"] = "Sharing code lock with clan is {0}",

                ["Teams"] = "Team automatic authorization {0}",
                ["TeamsCupboard"] = "Sharing cupboard with team is {0}",
                ["TeamsTurret"] = "Sharing turret with team is {0}",
                ["TeamsKeyLock"] = "Sharing key lock with team is {0}",
                ["TeamsCodeLock"] = "Sharing code lock with team is {0}",

                ["FriendsDisabled"] = "Server has disabled friends sharing",
                ["FriendsCupboardDisabled"] = "Server has disabled sharing cupboard with friends",
                ["FriendsTurretDisable"] = "Server has disabled sharing turret with friends",
                ["FriendsKeyLockDisable"] = "Server has disabled sharing key lock with friends",
                ["FriendsCodeLockDisable"] = "Server has disabled sharing code lock with friends",

                ["ClansDisabled"] = "Server has disabled clan sharing",
                ["ClansCupboardDisable"] = "Server has disabled sharing cupboard with clan",
                ["ClansTurretDisable"] = "Server has disabled sharing turret with clan",
                ["ClansKeyLockDisable"] = "Server has disabled sharing key lock with clan",
                ["ClansCodeLockDisable"] = "Server has disabled sharing code lock with clan",

                ["TeamsDisabled"] = "Server has disabled team sharing",
                ["TeamsCupboardDisable"] = "Server has disabled sharing cupboard with team",
                ["TeamsTurretDisable"] = "Server has disabled sharing turret with team",
                ["TeamsKeyLockDisable"] = "Server has disabled sharing key lock with team",
                ["TeamsCodeLockDisable"] = "Server has disabled sharing code lock with team",

                ["UI_Title"] = "Automatic Authorization UI",

                ["UI_TeamsTitle"] = "Team Share Settings",
                ["UI_TeamsShare"] = "Team Share",
                ["UI_TeamsCupboard"] = "Team Cupboard Share",
                ["UI_TeamsTurret"] = "Team Turret Share",
                ["UI_TeamsKeyLock"] = "Team Key Lock Share",
                ["UI_TeamsCodeLock"] = "Team Code Lock Share",

                ["UI_FriendsTitle"] = "Friends Share Settings",
                ["UI_FriendsShare"] = "Friends Share",
                ["UI_FriendsCupboard"] = "Friends Cupboard Share",
                ["UI_FriendsTurret"] = "Friends Turret Share",
                ["UI_FriendsKeyLock"] = "Friends Key Lock Share",
                ["UI_FriendsCodeLock"] = "Friends Code Lock Share",

                ["UI_ClansTitle"] = "Clan Share Settings",
                ["UI_ClansShare"] = "Clan Share",
                ["UI_ClansCupboard"] = "Clan Cupboard Share",
                ["UI_ClansTurret"] = "Clan Turret Share",
                ["UI_ClansKeyLock"] = "Clan Key Lock Share",
                ["UI_ClansCodeLock"] = "Clan Code Lock Share",
            }, this);

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["NotAllowed"] = "您没有权限使用该命令",
                ["Enabled"] = "<color=#8ee700>已启用</color>",
                ["Disabled"] = "<color=#ce422b>已禁用</color>",
                ["UnableAutoAuth"] = "服务器无法使用自动授权",
                ["SyntaxError"] = "语法错误, 输入 '<color=#ce422b>/{0} <help | h></color>' 查看帮助",
                ["TurretSuccess"] = "自动添加了 <color=#ce422b>{0}</color> 个朋友/战队成员/队友到您的 <color=#ce422b>{1}</color> 个炮台授权列表中",
                ["CupboardSuccess"] = "自动添加了 <color=#ce422b>{0}</color> 个朋友/战队成员/队友到您的 <color=#ce422b>{1}</color> 个领地柜授权列表中",

                ["FriendsSyntax"] = "<color=#ce422b>/{0} <autofriends | af></color> - 启用/禁用朋友自动授权",
                ["FriendsSyntax1"] = "<color=#ce422b>/{0} <autofriends | af> <cupboard | c></color> - 自动与朋友共享领地柜",
                ["FriendsSyntax2"] = "<color=#ce422b>/{0} <autofriends | af> <turret | t></color> - 自动与朋友共享炮台",
                ["FriendsSyntax3"] = "<color=#ce422b>/{0} <autofriends | af> <keylock | kl></color> - 自动与朋友共享钥匙锁",
                ["FriendsSyntax4"] = "<color=#ce422b>/{0} <autofriends | af> <codelock | cl></color> - 自动与朋友共享密码锁",

                ["ClansSyntax"] = "<color=#ce422b>/{0} <autoclan | ac></color> - 启用/禁用战队自动授权",
                ["ClansSyntax1"] = "<color=#ce422b>/{0} <autoclan | ac> <cupboard | c></color> - 自动与战队共享领地柜",
                ["ClansSyntax2"] = "<color=#ce422b>/{0} <autoclan | ac> <turret | t></color> - 自动与战队共享炮台",
                ["ClansSyntax3"] = "<color=#ce422b>/{0} <autoclan | ac> <keylock | kl></color> - 自动与战队共享钥匙锁",
                ["ClansSyntax4"] = "<color=#ce422b>/{0} <autoclan | ac> <codelock | cl></color> - 自动与战队共享密码锁",

                ["TeamsSyntax"] = "<color=#ce422b>/{0} <autoclan | ac></color> - 启用/禁用团队自动授权",
                ["TeamsSyntax1"] = "<color=#ce422b>/{0} <autoclan | ac> <cupboard | c></color> - 自动与团队共享领地柜",
                ["TeamsSyntax2"] = "<color=#ce422b>/{0} <autoclan | ac> <turret | t></color> - 自动与团队共享炮台",
                ["TeamsSyntax3"] = "<color=#ce422b>/{0} <autoclan | ac> <keylock | kl></color> - 自动与团队共享钥匙锁",
                ["TeamsSyntax4"] = "<color=#ce422b>/{0} <autoclan | ac> <codelock | cl></color> - 自动与团队共享密码锁",

                ["UISyntax"] = "<color=#ce422b>/{0}</color> - 打开自动共享UI",

                ["AutoShareFriendsStatus"] = "<color=#ffa500>当前朋友自动授权状态: </color>",
                ["AutoShareFriends"] = "自动与朋友共享: {0}",
                ["AutoShareFriendsCupboard"] = "自动与朋友共享领地柜: {0}",
                ["AutoShareFriendsTurret"] = "自动与朋友共享炮台: {0}",
                ["AutoShareFriendsKeyLock"] = "自动与朋友共享钥匙锁: {0}",
                ["AutoShareFriendsCodeLock"] = "自动与朋友共享密码锁: {0}",

                ["AutoShareClansStatus"] = "<color=#ffa500>当前战队自动授权状态: </color>",
                ["AutoShareClans"] = "自动与战队共享: {0}",
                ["AutoShareClansCupboard"] = "自动与战队共享领地柜: {0}",
                ["AutoShareClansTurret"] = "自动与战队共享炮台: {0}",
                ["AutoShareClansKeyLock"] = "自动与战队共享钥匙锁: {0}",
                ["AutoShareClansCodeLock"] = "自动与战队共享密码锁: {0}",

                ["AutoShareTeamsStatus"] = "<color=#ffa500>当前团队自动授权状态: </color>",
                ["AutoShareTeams"] = "自动与团队共享: {0}",
                ["AutoShareTeamsCupboard"] = "自动与团队共享领地柜: {0}",
                ["AutoShareTeamsTurret"] = "自动与团队共享炮台: {0}",
                ["AutoShareTeamsKeyLock"] = "自动与团队共享钥匙锁: {0}",
                ["AutoShareTeamsCodeLock"] = "自动与团队共享密码锁: {0}",

                ["Friends"] = "朋友自动授权 {0}",
                ["FriendsCupboard"] = "自动与朋友共享领地柜 {0}",
                ["FriendsTurret"] = "自动与朋友共享炮台 {0}",
                ["FriendsKeyLock"] = "自动与朋友共享钥匙锁 {0}",
                ["FriendsCodeLock"] = "自动与朋友共享密码锁 {0}",

                ["Clans"] = "战队自动授权 {0}",
                ["ClansCupboard"] = "自动与战队共享领地柜 {0}",
                ["ClansTurret"] = "自动与战队共享炮台 {0}",
                ["ClansKeyLock"] = "自动与战队共享钥匙锁 {0}",
                ["ClansCodeLock"] = "自动与战队共享密码锁 {0}",

                ["Teams"] = "团队自动授权 {0}",
                ["TeamsCupboard"] = "自动与团队共享领地柜 {0}",
                ["TeamsTurret"] = "自动与团队共享炮台 {0}",
                ["TeamsKeyLock"] = "自动与团队共享钥匙锁 {0}",
                ["TeamsCodeLock"] = "自动与团队共享密码锁 {0}",

                ["FriendsDisabled"] = "服务器已禁用朋友自动授权",
                ["FriendsCupboardDisabled"] = "服务器已禁用自动与朋友共享领地柜",
                ["FriendsTurretDisable"] = "服务器已禁用自动与朋友共享炮台",
                ["FriendsKeyLockDisable"] = "服务器已禁用自动与朋友共享钥匙锁",
                ["FriendsCodeLockDisable"] = "服务器已禁用自动与朋友共享密码锁",

                ["ClansDisabled"] = "服务器已禁用战队自动授权",
                ["ClansCupboardDisable"] = "服务器已禁用自动与战队共享领地柜",
                ["ClansTurretDisable"] = "服务器已禁用自动与战队共享炮台",
                ["ClansKeyLockDisable"] = "服务器已禁用自动与战队共享钥匙锁",
                ["ClansCodeLockDisable"] = "服务器已禁用自动与战队共享密码锁",

                ["TeamsDisabled"] = "服务器已禁用团队自动授权",
                ["TeamsCupboardDisable"] = "服务器已禁用自动与团队共享领地柜",
                ["TeamsTurretDisable"] = "服务器已禁用自动与团队共享炮台",
                ["TeamsKeyLockDisable"] = "服务器已禁用自动与团队共享钥匙锁",
                ["TeamsCodeLockDisable"] = "服务器已禁用自动与团队共享密码锁",

                ["UI_Title"] = "自动共享UI",

                ["UI_TeamsTitle"] = "团队共享设置",
                ["UI_TeamsShare"] = "团队共享",
                ["UI_TeamsCupboard"] = "团队领地柜共享",
                ["UI_TeamsTurret"] = "团队炮台共享",
                ["UI_TeamsKeyLock"] = "团队钥匙锁共享",
                ["UI_TeamsCodeLock"] = "团队密码锁共享",

                ["UI_FriendsTitle"] = "朋友共享设置",
                ["UI_FriendsShare"] = "朋友共享",
                ["UI_FriendsCupboard"] = "朋友领地柜共享",
                ["UI_FriendsTurret"] = "朋友炮台共享",
                ["UI_FriendsKeyLock"] = "朋友钥匙锁共享",
                ["UI_FriendsCodeLock"] = "朋友密码锁共享",

                ["UI_ClansTitle"] = "战队共享设置",
                ["UI_ClansShare"] = "战队共享",
                ["UI_ClansCupboard"] = "战队领地柜共享",
                ["UI_ClansTurret"] = "战队炮台共享",
                ["UI_ClansKeyLock"] = "战队钥匙锁共享",
                ["UI_ClansCodeLock"] = "战队密码锁共享",
            }, this, "zh-CN");
        }

        #endregion LanguageFile
    }
}