#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class SleepingBag : DecayEntity
{
	public enum BagAssignMode
	{
		Allowed = 0,
		TeamAndFriendlyContacts = 1,
		None = 2,
		LAST = 2
	}

	public enum BagResultType
	{
		Ok = 0,
		TooManyBags = 1,
		BagBlocked = 2
	}

	public struct CanAssignBedResult
	{
		public BagResultType Result;

		public int Count;

		public int Max;
	}

	public struct CanBuildResult
	{
		public bool Result;

		public Translate.Phrase Phrase;

		public string[] Arguments;
	}

	public enum SleepingBagResetReason
	{
		Respawned = 0,
		Placed = 1,
		Death = 2
	}

	[NonSerialized]
	public ulong deployerUserID;

	public GameObject renameDialog;

	public GameObject assignDialog;

	public float secondsBetweenReuses = 300f;

	public string niceName = "Unnamed Bag";

	public Vector3 spawnOffset = Vector3.zero;

	public RespawnInformation.SpawnOptions.RespawnType RespawnType = RespawnInformation.SpawnOptions.RespawnType.SleepingBag;

	public bool isStatic;

	public bool canBePublic;

	public const Flags IsPublicFlag = Flags.Reserved3;

	public static Translate.Phrase bagLimitPhrase = new Translate.Phrase("bag_limit_update", "You are now at {0}/{1} bags");

	public static Translate.Phrase bagLimitReachedPhrase = new Translate.Phrase("bag_limit_reached", "You have reached your bag limit!");

	public Translate.Phrase assignOtherBagPhrase = new Translate.Phrase("assigned_other_bag_limit", "You have assigned {0} a bag, they are now at {0}/{1} bags");

	public Translate.Phrase assignedBagPhrase = new Translate.Phrase("assigned_bag_limit", "You have been assigned a bag, you are now at {0}/{1} bags");

	public Translate.Phrase cannotAssignBedPhrase = new Translate.Phrase("cannot_assign_bag_limit", "You cannot assign {0} a bag, they have reached their bag limit!");

	public Translate.Phrase cannotMakeBedPhrase = new Translate.Phrase("cannot_make_bed_limit", "You cannot take ownership of the bed, you are at your bag limit");

	public Translate.Phrase bedAssigningBlocked = new Translate.Phrase("bag_assign_blocked", "That player has blocked bag assignment");

	public float unlockTime;

	public static List<SleepingBag> sleepingBags = new List<SleepingBag>();

	private bool notifyPlayerOnServerInit;

	private static Dictionary<ulong, List<SleepingBag>> bagsPerPlayer = new Dictionary<ulong, List<SleepingBag>>();

	public virtual float unlockSeconds
	{
		get
		{
			if (unlockTime < UnityEngine.Time.realtimeSinceStartup)
			{
				return 0f;
			}
			return unlockTime - UnityEngine.Time.realtimeSinceStartup;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SleepingBag.OnRpcMessage"))
		{
			if (rpc == 3057055788u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AssignToFriend "));
				}
				using (TimeWarning.New("AssignToFriend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3057055788u, "AssignToFriend", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg2 = rPCMessage;
							AssignToFriend(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AssignToFriend");
					}
				}
				return true;
			}
			if (rpc == 1335950295 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - Rename "));
				}
				using (TimeWarning.New("Rename"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1335950295u, "Rename", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg3 = rPCMessage;
							Rename(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in Rename");
					}
				}
				return true;
			}
			if (rpc == 42669546 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_MakeBed "));
				}
				using (TimeWarning.New("RPC_MakeBed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(42669546u, "RPC_MakeBed", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg4 = rPCMessage;
							RPC_MakeBed(msg4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_MakeBed");
					}
				}
				return true;
			}
			if (rpc == 393812086 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (ConVar.Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_MakePublic "));
				}
				using (TimeWarning.New("RPC_MakePublic"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(393812086u, "RPC_MakePublic", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rPCMessage = default(RPCMessage);
							rPCMessage.connection = msg.connection;
							rPCMessage.player = player;
							rPCMessage.read = msg.read;
							RPCMessage msg5 = rPCMessage;
							RPC_MakePublic(msg5);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_MakePublic");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsPublic()
	{
		return HasFlag(Flags.Reserved3);
	}

	public virtual float GetUnlockSeconds(ulong playerID)
	{
		return unlockSeconds;
	}

	public virtual bool ValidForPlayer(ulong playerID, bool ignoreTimers)
	{
		object obj = Interface.CallHook("OnSleepingBagValidCheck", this, playerID, ignoreTimers);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (deployerUserID == playerID)
		{
			if (!ignoreTimers)
			{
				return unlockTime < UnityEngine.Time.realtimeSinceStartup;
			}
			return true;
		}
		return false;
	}

	public static CanAssignBedResult? CanAssignBed(BasePlayer player, SleepingBag newBag, ulong targetPlayer, int countOffset = 1, int maxOffset = 0, SleepingBag ignore = null)
	{
		int num = ConVar.Server.max_sleeping_bags + maxOffset;
		if (num < 0)
		{
			return null;
		}
		int num2 = countOffset;
		BasePlayer basePlayer = BasePlayer.FindByID(targetPlayer);
		CanAssignBedResult value;
		if (player != basePlayer && basePlayer != null)
		{
			switch ((BagAssignMode)Mathf.Clamp(basePlayer.GetInfoInt("client.bagassignmode", 0), 0, 2))
			{
			case BagAssignMode.None:
				value = default(CanAssignBedResult);
				value.Result = BagResultType.BagBlocked;
				return value;
			case BagAssignMode.TeamAndFriendlyContacts:
			{
				bool flag = false;
				if (basePlayer.Team != null && basePlayer.Team.members.Contains(player.userID))
				{
					flag = true;
				}
				else
				{
					RelationshipManager.PlayerRelationshipInfo relations = RelationshipManager.ServerInstance.GetRelationships(targetPlayer).GetRelations(player.userID);
					if (relations != null && relations.type == RelationshipManager.RelationshipType.Friend)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					value = default(CanAssignBedResult);
					value.Result = BagResultType.BagBlocked;
					return value;
				}
				break;
			}
			}
		}
		foreach (SleepingBag sleepingBag in sleepingBags)
		{
			if (sleepingBag != ignore && sleepingBag.deployerUserID == targetPlayer)
			{
				num2++;
				if (num2 > num)
				{
					value = default(CanAssignBedResult);
					value.Count = num2;
					value.Max = num;
					value.Result = BagResultType.TooManyBags;
					return value;
				}
			}
		}
		value = default(CanAssignBedResult);
		value.Count = num2;
		value.Max = num;
		value.Result = BagResultType.Ok;
		return value;
	}

	public static CanBuildResult? CanBuildBed(BasePlayer player, Construction construction)
	{
		if (GameManager.server.FindPrefab(construction.prefabID)?.GetComponent<BaseEntity>() is SleepingBag)
		{
			CanAssignBedResult? canAssignBedResult = CanAssignBed(player, null, player.userID);
			if (canAssignBedResult.HasValue)
			{
				CanBuildResult value;
				if (canAssignBedResult.Value.Result == BagResultType.Ok)
				{
					value = default(CanBuildResult);
					value.Result = true;
					value.Phrase = bagLimitPhrase;
					value.Arguments = new string[2]
					{
						canAssignBedResult.Value.Count.ToString(),
						canAssignBedResult.Value.Max.ToString()
					};
					return value;
				}
				value = default(CanBuildResult);
				value.Result = false;
				value.Phrase = bagLimitReachedPhrase;
				return value;
			}
		}
		return null;
	}

	public static SleepingBag[] FindForPlayer(ulong playerID, bool ignoreTimers)
	{
		return sleepingBags.Where((SleepingBag x) => x.ValidForPlayer(playerID, ignoreTimers)).ToArray();
	}

	public static bool SpawnPlayer(BasePlayer player, NetworkableId sleepingBag)
	{
		BasePlayer player2 = player;
		SleepingBag[] array = FindForPlayer(player2.userID, ignoreTimers: true);
		SleepingBag sleepingBag2 = array.FirstOrDefault((SleepingBag x) => x.ValidForPlayer(player2.userID, ignoreTimers: false) && x.net.ID == sleepingBag && x.unlockTime < UnityEngine.Time.realtimeSinceStartup);
		if (sleepingBag2 == null)
		{
			return false;
		}
		object obj = Interface.CallHook("OnPlayerRespawn", player, sleepingBag2);
		if (obj is SleepingBag)
		{
			sleepingBag2 = (SleepingBag)obj;
		}
		if (sleepingBag2.IsOccupied())
		{
			return false;
		}
		sleepingBag2.GetSpawnPos(out var pos, out var rot);
		player2.RespawnAt(pos, rot, sleepingBag2);
		sleepingBag2.PostPlayerSpawn(player2);
		SleepingBag[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			SetBagTimer(array2[i], pos, SleepingBagResetReason.Respawned);
		}
		return true;
	}

	public static void AddBagForPlayer(SleepingBag bag, ulong user, bool networkUpdate = true)
	{
		if (user == 0L)
		{
			return;
		}
		if (!bagsPerPlayer.TryGetValue(user, out var value))
		{
			value = new List<SleepingBag>();
			bagsPerPlayer[user] = value;
		}
		if (!value.Contains(bag))
		{
			value.Add(bag);
			if (networkUpdate)
			{
				RelationshipManager.FindByID(user)?.SendNetworkUpdate();
			}
		}
	}

	public static void RemoveBagForPlayer(SleepingBag bag, ulong user)
	{
		if (user != 0L && bagsPerPlayer.TryGetValue(user, out var value))
		{
			if (value.Remove(bag))
			{
				RelationshipManager.FindByID(user)?.SendNetworkUpdate();
			}
			if (value.Count == 0)
			{
				bagsPerPlayer.Remove(user);
			}
		}
	}

	public static void OnBagChangedOwnership(SleepingBag bag, ulong oldUser)
	{
		if (bag.deployerUserID != oldUser)
		{
			RemoveBagForPlayer(bag, oldUser);
			AddBagForPlayer(bag, bag.deployerUserID);
		}
	}

	public static int GetSleepingBagCount(ulong userId)
	{
		if (userId == 0L)
		{
			return 0;
		}
		if (!bagsPerPlayer.TryGetValue(userId, out var value))
		{
			return 0;
		}
		return value.Count;
	}

	public virtual void SetUnlockTime(float newTime)
	{
		unlockTime = newTime;
	}

	public static bool DestroyBag(BasePlayer player, NetworkableId sleepingBag)
	{
		SleepingBag sleepingBag2 = FindForPlayer(player.userID, ignoreTimers: true).FirstOrDefault((SleepingBag x) => x.net.ID == sleepingBag);
		if (sleepingBag2 == null)
		{
			return false;
		}
		if (Interface.CallHook("OnSleepingBagDestroy", sleepingBag2, player) != null)
		{
			return false;
		}
		RemoveBagForPlayer(sleepingBag2, sleepingBag2.deployerUserID);
		sleepingBag2.deployerUserID = 0uL;
		sleepingBag2.SendNetworkUpdate();
		player.SendRespawnOptions();
		Facepunch.Rust.Analytics.Azure.OnBagUnclaimed(player, sleepingBag2);
		Interface.CallHook("OnSleepingBagDestroyed", sleepingBag2, player);
		return true;
	}

	public static void ResetTimersForPlayer(BasePlayer player)
	{
		SleepingBag[] array = FindForPlayer(player.userID, ignoreTimers: true);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].unlockTime = 0f;
		}
	}

	public virtual void GetSpawnPos(out Vector3 pos, out Quaternion rot)
	{
		pos = base.transform.position + spawnOffset;
		rot = Quaternion.Euler(0f, base.transform.rotation.eulerAngles.y, 0f);
	}

	public void SetPublic(bool isPublic)
	{
		SetFlag(Flags.Reserved3, isPublic);
	}

	public void SetDeployedBy(BasePlayer player)
	{
		if (!(player == null))
		{
			deployerUserID = player.userID;
			SetBagTimer(this, base.transform.position, SleepingBagResetReason.Placed);
			SendNetworkUpdate();
			notifyPlayerOnServerInit = true;
		}
	}

	public static void OnPlayerDeath(BasePlayer player)
	{
		SleepingBag[] array = FindForPlayer(player.userID, ignoreTimers: true);
		for (int i = 0; i < array.Length; i++)
		{
			SetBagTimer(array[i], player.transform.position, SleepingBagResetReason.Death);
		}
	}

	public static void SetBagTimer(SleepingBag bag, Vector3 position, SleepingBagResetReason reason)
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		float? num = null;
		if (activeGameMode != null)
		{
			num = activeGameMode.EvaluateSleepingBagReset(bag, position, reason);
		}
		if (num.HasValue)
		{
			bag.SetUnlockTime(UnityEngine.Time.realtimeSinceStartup + num.Value);
			return;
		}
		if (reason == SleepingBagResetReason.Respawned && Vector3.Distance(position, bag.transform.position) <= ConVar.Server.respawnresetrange)
		{
			bag.SetUnlockTime(UnityEngine.Time.realtimeSinceStartup + bag.secondsBetweenReuses);
			bag.SendNetworkUpdate();
		}
		if (reason != SleepingBagResetReason.Placed)
		{
			return;
		}
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		SleepingBag[] array = sleepingBags.Where((SleepingBag x) => x.deployerUserID != 0L && x.deployerUserID == bag.deployerUserID && x.unlockTime > UnityEngine.Time.realtimeSinceStartup).ToArray();
		foreach (SleepingBag sleepingBag in array)
		{
			if (bag.unlockTime > realtimeSinceStartup && Vector3.Distance(sleepingBag.transform.position, position) <= ConVar.Server.respawnresetrange)
			{
				realtimeSinceStartup = bag.unlockTime;
			}
		}
		bag.SetUnlockTime(Mathf.Max(realtimeSinceStartup, UnityEngine.Time.realtimeSinceStartup + bag.secondsBetweenReuses));
		bag.SendNetworkUpdate();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!sleepingBags.Contains(this))
		{
			sleepingBags.Add(this);
			if (deployerUserID != 0L)
			{
				AddBagForPlayer(this, deployerUserID, !Rust.Application.isLoadingSave);
			}
		}
		if (notifyPlayerOnServerInit)
		{
			notifyPlayerOnServerInit = false;
			NotifyPlayer(deployerUserID);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		AddBagForPlayer(this, deployerUserID, !Rust.Application.isLoadingSave);
	}

	private void NotifyPlayer(ulong id)
	{
		BasePlayer basePlayer = BasePlayer.FindByID(id);
		if (basePlayer != null && basePlayer.IsConnected)
		{
			basePlayer.SendRespawnOptions();
		}
	}

	public override void DoServerDestroy()
	{
		base.DoServerDestroy();
		sleepingBags.RemoveAll((SleepingBag x) => x == this);
		RemoveBagForPlayer(this, deployerUserID);
		NotifyPlayer(deployerUserID);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.sleepingBag = Facepunch.Pool.Get<ProtoBuf.SleepingBag>();
		info.msg.sleepingBag.name = niceName;
		info.msg.sleepingBag.deployerID = deployerUserID;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void Rename(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		string text = msg.read.String();
		if (Interface.CallHook("CanRenameBed", msg.player, this, text) == null)
		{
			text = WordFilter.Filter(text);
			if (string.IsNullOrEmpty(text))
			{
				text = "Unnamed Sleeping Bag";
			}
			if (text.Length > 24)
			{
				text = text.Substring(0, 22) + "..";
			}
			niceName = text;
			SendNetworkUpdate();
			NotifyPlayer(deployerUserID);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AssignToFriend(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || deployerUserID != msg.player.userID)
		{
			return;
		}
		ulong num = msg.read.UInt64();
		if (num == 0L || Interface.CallHook("CanAssignBed", msg.player, this, num) != null)
		{
			return;
		}
		if (ConVar.Server.max_sleeping_bags > 0)
		{
			CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, num);
			if (canAssignBedResult.HasValue)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(num);
				if (canAssignBedResult.Value.Result == BagResultType.TooManyBags)
				{
					msg.player.ShowToast(GameTip.Styles.Red_Normal, cannotAssignBedPhrase, basePlayer?.displayName ?? "other player");
				}
				else if (canAssignBedResult.Value.Result == BagResultType.BagBlocked)
				{
					msg.player.ShowToast(GameTip.Styles.Red_Normal, bedAssigningBlocked);
				}
				else
				{
					basePlayer?.ShowToast(GameTip.Styles.Blue_Long, assignedBagPhrase, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, (GetSleepingBagCount(msg.player.userID) - 1).ToString(), canAssignBedResult.Value.Max.ToString());
				}
				if (canAssignBedResult.Value.Result != 0)
				{
					return;
				}
			}
		}
		ulong num2 = deployerUserID;
		deployerUserID = num;
		NotifyPlayer(num2);
		NotifyPlayer(deployerUserID);
		OnBagChangedOwnership(this, num2);
		Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, num);
		SendNetworkUpdate();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public virtual void RPC_MakePublic(RPCMessage msg)
	{
		if (!canBePublic || !msg.player.CanInteract() || (deployerUserID != msg.player.userID && !msg.player.CanBuild()))
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag == IsPublic() || Interface.CallHook("CanSetBedPublic", msg.player, this) != null)
		{
			return;
		}
		SetPublic(flag);
		if (!IsPublic())
		{
			if (ConVar.Server.max_sleeping_bags > 0)
			{
				CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, msg.player.userID, 1, 0, this);
				if (canAssignBedResult.HasValue)
				{
					if (canAssignBedResult.Value.Result == BagResultType.Ok)
					{
						msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					}
					else
					{
						msg.player.ShowToast(GameTip.Styles.Blue_Long, cannotMakeBedPhrase, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					}
					if (canAssignBedResult.Value.Result != 0)
					{
						return;
					}
				}
			}
			ulong num = deployerUserID;
			deployerUserID = msg.player.userID;
			NotifyPlayer(num);
			NotifyPlayer(deployerUserID);
			OnBagChangedOwnership(this, num);
			Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, deployerUserID = msg.player.userID);
		}
		else
		{
			Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, 0uL);
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_MakeBed(RPCMessage msg)
	{
		if (!canBePublic || !IsPublic() || !msg.player.CanInteract())
		{
			return;
		}
		if (ConVar.Server.max_sleeping_bags > 0)
		{
			CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, msg.player.userID, 1, 0, this);
			if (canAssignBedResult.HasValue)
			{
				if (canAssignBedResult.Value.Result != 0)
				{
					msg.player.ShowToast(GameTip.Styles.Red_Normal, cannotMakeBedPhrase);
				}
				else
				{
					msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
				}
				if (canAssignBedResult.Value.Result != 0)
				{
					return;
				}
			}
		}
		ulong num = deployerUserID;
		deployerUserID = msg.player.userID;
		NotifyPlayer(num);
		NotifyPlayer(deployerUserID);
		OnBagChangedOwnership(this, num);
		SendNetworkUpdate();
	}

	protected virtual void PostPlayerSpawn(BasePlayer p)
	{
		p.SendRespawnOptions();
	}

	public virtual bool IsOccupied()
	{
		return false;
	}

	public override string Admin_Who()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.Admin_Who());
		stringBuilder.AppendLine($"Assigned bag ID: {deployerUserID}");
		stringBuilder.AppendLine("Assigned player name: " + Admin.GetPlayerName(deployerUserID));
		stringBuilder.AppendLine("Bag Name:" + niceName);
		return stringBuilder.ToString();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.sleepingBag != null)
		{
			niceName = info.msg.sleepingBag.name;
			deployerUserID = info.msg.sleepingBag.deployerID;
		}
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return player.userID == deployerUserID;
		}
		return false;
	}
}
