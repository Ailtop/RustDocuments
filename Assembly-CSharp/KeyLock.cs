#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class KeyLock : BaseLock
{
	[ItemSelector(ItemCategory.All)]
	public ItemDefinition keyItemType;

	public int keyCode;

	public bool firstKeyCreated;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("KeyLock.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 4135414453u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_CreateKey "));
				}
				using (TimeWarning.New("RPC_CreateKey"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4135414453u, "RPC_CreateKey", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							RPC_CreateKey(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_CreateKey");
					}
				}
				return true;
			}
			if (rpc == 954115386 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Lock "));
				}
				using (TimeWarning.New("RPC_Lock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(954115386u, "RPC_Lock", this, player, 3f))
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
							RPCMessage rpc3 = rPCMessage;
							RPC_Lock(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_Lock");
					}
				}
				return true;
			}
			if (rpc == 1663222372 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Unlock "));
				}
				using (TimeWarning.New("RPC_Unlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(1663222372u, "RPC_Unlock", this, player, 3f))
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
							RPCMessage rpc4 = rPCMessage;
							RPC_Unlock(rpc4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RPC_Unlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool HasLockPermission(BasePlayer player)
	{
		if (player.IsDead())
		{
			return false;
		}
		if (player.userID == base.OwnerID)
		{
			return true;
		}
		foreach (Item item in player.inventory.FindItemIDs(keyItemType.itemid))
		{
			if (CanKeyUnlockUs(item))
			{
				return true;
			}
		}
		return false;
	}

	private bool CanKeyUnlockUs(Item key)
	{
		if (key.instanceData == null)
		{
			return false;
		}
		if (key.instanceData.dataInt != keyCode)
		{
			return false;
		}
		return true;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.keyLock != null)
		{
			keyCode = info.msg.keyLock.code;
		}
	}

	public override bool ShouldNetworkOwnerInfo()
	{
		return true;
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		if (base.OwnerID == 0L && (bool)GetParentEntity())
		{
			base.OwnerID = GetParentEntity().OwnerID;
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.keyLock = Facepunch.Pool.Get<ProtoBuf.KeyLock>();
			info.msg.keyLock.code = keyCode;
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy)
	{
		base.OnDeployed(parent, deployedBy);
		keyCode = UnityEngine.Random.Range(1, 100000);
		Lock(deployedBy);
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasLockPermission(player))
		{
			return true;
		}
		return !IsLocked();
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		object obj = Interface.CallHook("CanUseLockedEntity", player, this);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (HasLockPermission(player))
		{
			return true;
		}
		return !IsLocked();
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Unlock(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && IsLocked() && Interface.CallHook("CanUnlock", rpc.player, this) == null && HasLockPermission(rpc.player))
		{
			SetFlag(Flags.Locked, false);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_Lock(RPCMessage rpc)
	{
		Lock(rpc.player);
	}

	private void Lock(BasePlayer player)
	{
		if (!(player == null) && player.CanInteract() && !IsLocked() && Interface.CallHook("CanLock", player, this) == null && HasLockPermission(player))
		{
			LockLock(player);
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void RPC_CreateKey(RPCMessage rpc)
	{
		if (!rpc.player.CanInteract() || (IsLocked() && !HasLockPermission(rpc.player)))
		{
			return;
		}
		ItemDefinition itemDefinition = ItemManager.FindItemDefinition(keyItemType.itemid);
		if (itemDefinition == null)
		{
			Debug.LogWarning("RPC_CreateKey: Itemdef is missing! " + keyItemType);
			return;
		}
		ItemBlueprint bp = ItemManager.FindBlueprint(itemDefinition);
		if (rpc.player.inventory.crafting.CanCraft(bp))
		{
			ProtoBuf.Item.InstanceData instanceData = Facepunch.Pool.Get<ProtoBuf.Item.InstanceData>();
			instanceData.dataInt = keyCode;
			rpc.player.inventory.crafting.CraftItem(bp, rpc.player, instanceData);
			if (!firstKeyCreated)
			{
				LockLock(rpc.player);
				SendNetworkUpdate();
				firstKeyCreated = true;
			}
		}
	}

	public void LockLock(BasePlayer player)
	{
		SetFlag(Flags.Locked, true);
		if (BaseEntityEx.IsValid(player))
		{
			player.GiveAchievement("LOCK_LOCK");
		}
	}
}
