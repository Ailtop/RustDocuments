#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class SleepingBag : DecayEntity
{
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

	public float unlockTime;

	public static List<SleepingBag> sleepingBags = new List<SleepingBag>();

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
			RPCMessage rPCMessage;
			if (rpc == 3057055788u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
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
							rPCMessage = default(RPCMessage);
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
				if (Global.developer > 2)
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
							rPCMessage = default(RPCMessage);
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
				if (Global.developer > 2)
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
							rPCMessage = default(RPCMessage);
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
				if (Global.developer > 2)
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
							rPCMessage = default(RPCMessage);
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

	public static SleepingBag[] FindForPlayer(ulong playerID, bool ignoreTimers)
	{
		return sleepingBags.Where((SleepingBag x) => x.ValidForPlayer(playerID, ignoreTimers)).ToArray();
	}

	public static SleepingBag FindForPlayer(ulong playerID, uint sleepingBagID, bool ignoreTimers)
	{
		return sleepingBags.FirstOrDefault((SleepingBag x) => x.deployerUserID == playerID && x.net.ID == sleepingBagID && (ignoreTimers || x.unlockTime < UnityEngine.Time.realtimeSinceStartup));
	}

	public static bool SpawnPlayer(BasePlayer player, uint sleepingBag)
	{
		BasePlayer player2 = player;
		SleepingBag[] array = FindForPlayer(player2.userID, true);
		SleepingBag sleepingBag2 = array.FirstOrDefault((SleepingBag x) => x.ValidForPlayer(player2.userID, false) && x.net.ID == sleepingBag && x.unlockTime < UnityEngine.Time.realtimeSinceStartup);
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
		Vector3 pos;
		Quaternion rot;
		sleepingBag2.GetSpawnPos(out pos, out rot);
		player2.RespawnAt(pos, rot);
		sleepingBag2.PostPlayerSpawn(player2);
		SleepingBag[] array2 = array;
		foreach (SleepingBag sleepingBag3 in array2)
		{
			if (Vector3.Distance(pos, sleepingBag3.transform.position) <= ConVar.Server.respawnresetrange)
			{
				sleepingBag3.SetUnlockTime(UnityEngine.Time.realtimeSinceStartup + sleepingBag3.secondsBetweenReuses);
			}
		}
		return true;
	}

	public virtual void SetUnlockTime(float newTime)
	{
		unlockTime = newTime;
	}

	public static bool DestroyBag(BasePlayer player, uint sleepingBag)
	{
		SleepingBag sleepingBag2 = FindForPlayer(player.userID, sleepingBag, false);
		if (sleepingBag2 == null)
		{
			return false;
		}
		if (Interface.CallHook("OnSleepingBagDestroy", sleepingBag2, player) != null)
		{
			return false;
		}
		if (sleepingBag2.canBePublic)
		{
			sleepingBag2.SetPublic(true);
			sleepingBag2.deployerUserID = 0uL;
		}
		else
		{
			sleepingBag2.Kill();
		}
		player.SendRespawnOptions();
		Interface.CallHook("OnSleepingBagDestroyed", sleepingBag2, player);
		return true;
	}

	public static void ResetTimersForPlayer(BasePlayer player)
	{
		SleepingBag[] array = FindForPlayer(player.userID, true);
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

	private void SetDeployedBy(BasePlayer player)
	{
		if (player == null)
		{
			return;
		}
		deployerUserID = player.userID;
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		SleepingBag[] array = sleepingBags.Where((SleepingBag x) => x.deployerUserID == player.userID && x.unlockTime > UnityEngine.Time.realtimeSinceStartup).ToArray();
		foreach (SleepingBag sleepingBag in array)
		{
			if (sleepingBag.unlockTime > realtimeSinceStartup && Vector3.Distance(sleepingBag.transform.position, base.transform.position) <= ConVar.Server.respawnresetrange)
			{
				realtimeSinceStartup = sleepingBag.unlockTime;
			}
		}
		unlockTime = Mathf.Max(realtimeSinceStartup, UnityEngine.Time.realtimeSinceStartup + secondsBetweenReuses);
		SendNetworkUpdate();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!sleepingBags.Contains(this))
		{
			sleepingBags.Add(this);
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		sleepingBags.RemoveAll((SleepingBag x) => x == this);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.sleepingBag = Facepunch.Pool.Get<ProtoBuf.SleepingBag>();
		info.msg.sleepingBag.name = niceName;
		info.msg.sleepingBag.deployerID = deployerUserID;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
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
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AssignToFriend(RPCMessage msg)
	{
		if (msg.player.CanInteract() && deployerUserID == msg.player.userID)
		{
			ulong num = msg.read.UInt64();
			if (num != 0L && Interface.CallHook("CanAssignBed", msg.player, this, num) == null)
			{
				deployerUserID = num;
				SendNetworkUpdate();
			}
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public virtual void RPC_MakePublic(RPCMessage msg)
	{
		if (!canBePublic || !msg.player.CanInteract() || (deployerUserID != msg.player.userID && !msg.player.CanBuild()))
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag != IsPublic() && Interface.CallHook("CanSetBedPublic", msg.player, this) == null)
		{
			SetPublic(flag);
			if (!IsPublic())
			{
				deployerUserID = msg.player.userID;
			}
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_MakeBed(RPCMessage msg)
	{
		if (canBePublic && IsPublic() && msg.player.CanInteract())
		{
			deployerUserID = msg.player.userID;
			SendNetworkUpdate();
		}
	}

	protected virtual void PostPlayerSpawn(BasePlayer p)
	{
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
