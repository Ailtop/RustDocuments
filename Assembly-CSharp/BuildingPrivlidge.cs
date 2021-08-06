#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class BuildingPrivlidge : StorageContainer
{
	public class UpkeepBracket
	{
		public int objectsUpTo;

		public float fraction;

		public float blocksTaxPaid;

		public UpkeepBracket(int numObjs, float frac)
		{
			objectsUpTo = numObjs;
			fraction = frac;
			blocksTaxPaid = 0f;
		}
	}

	public float cachedProtectedMinutes;

	public float nextProtectedCalcTime;

	public static UpkeepBracket[] upkeepBrackets = new UpkeepBracket[4]
	{
		new UpkeepBracket(ConVar.Decay.bracket_0_blockcount, ConVar.Decay.bracket_0_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_1_blockcount, ConVar.Decay.bracket_1_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_2_blockcount, ConVar.Decay.bracket_2_costfraction),
		new UpkeepBracket(ConVar.Decay.bracket_3_blockcount, ConVar.Decay.bracket_3_costfraction)
	};

	public List<ItemAmount> upkeepBuffer = new List<ItemAmount>();

	public List<PlayerNameID> authorizedPlayers = new List<PlayerNameID>();

	public const Flags Flag_MaxAuths = Flags.Reserved5;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BuildingPrivlidge.OnRpcMessage"))
		{
			RPCMessage rPCMessage;
			if (rpc == 1092560690 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - AddSelfAuthorize "));
				}
				using (TimeWarning.New("AddSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1092560690u, "AddSelfAuthorize", this, player, 3f))
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
							AddSelfAuthorize(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in AddSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 253307592 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ClearList "));
				}
				using (TimeWarning.New("ClearList"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(253307592u, "ClearList", this, player, 3f))
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
							ClearList(rpc3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in ClearList");
					}
				}
				return true;
			}
			if (rpc == 3617985969u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RemoveSelfAuthorize "));
				}
				using (TimeWarning.New("RemoveSelfAuthorize"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3617985969u, "RemoveSelfAuthorize", this, player, 3f))
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
							RemoveSelfAuthorize(rpc4);
						}
					}
					catch (Exception exception3)
					{
						Debug.LogException(exception3);
						player.Kick("RPC Error in RemoveSelfAuthorize");
					}
				}
				return true;
			}
			if (rpc == 2051750736 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_Rotate "));
				}
				using (TimeWarning.New("RPC_Rotate"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2051750736u, "RPC_Rotate", this, player, 3f))
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
							RPC_Rotate(msg2);
						}
					}
					catch (Exception exception4)
					{
						Debug.LogException(exception4);
						player.Kick("RPC Error in RPC_Rotate");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public float CalculateUpkeepPeriodMinutes()
	{
		if (base.isServer)
		{
			return ConVar.Decay.upkeep_period_minutes;
		}
		return 0f;
	}

	public float CalculateUpkeepCostFraction()
	{
		if (base.isServer)
		{
			return CalculateBuildingTaxRate();
		}
		return 0f;
	}

	public void CalculateUpkeepCostAmounts(List<ItemAmount> itemAmounts)
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float multiplier = CalculateUpkeepCostFraction();
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			decayEntity.CalculateUpkeepCostAmounts(itemAmounts, multiplier);
		}
	}

	public float GetProtectedMinutes(bool force = false)
	{
		if (base.isServer)
		{
			if (!force && UnityEngine.Time.realtimeSinceStartup < nextProtectedCalcTime)
			{
				return cachedProtectedMinutes;
			}
			nextProtectedCalcTime = UnityEngine.Time.realtimeSinceStartup + 60f;
			List<ItemAmount> obj = Facepunch.Pool.GetList<ItemAmount>();
			CalculateUpkeepCostAmounts(obj);
			float num = CalculateUpkeepPeriodMinutes();
			float num2 = -1f;
			if (base.inventory != null)
			{
				foreach (ItemAmount item in obj)
				{
					int num3 = base.inventory.FindItemsByItemID(item.itemid).Sum((Item x) => x.amount);
					if (num3 > 0 && item.amount > 0f)
					{
						float num4 = (float)num3 / item.amount * num;
						if (num2 == -1f || num4 < num2)
						{
							num2 = num4;
						}
					}
					else
					{
						num2 = 0f;
					}
				}
				if (num2 == -1f)
				{
					num2 = 0f;
				}
			}
			Facepunch.Pool.FreeList(ref obj);
			cachedProtectedMinutes = num2;
			Interface.CallHook("OnCupboardProtectionCalculated", this, cachedProtectedMinutes);
			return cachedProtectedMinutes;
		}
		return 0f;
	}

	public override void OnKilled(HitInfo info)
	{
		if (ConVar.Decay.upkeep_grief_protection > 0f)
		{
			PurchaseUpkeepTime(ConVar.Decay.upkeep_grief_protection * 60f);
		}
		base.OnKilled(info);
	}

	public override void DecayTick()
	{
		if (EnsurePrimary())
		{
			base.DecayTick();
		}
	}

	public bool EnsurePrimary()
	{
		BuildingManager.Building building = GetBuilding();
		if (building != null)
		{
			BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
			if (dominatingBuildingPrivilege != null && dominatingBuildingPrivilege != this)
			{
				Kill(DestroyMode.Gib);
				return false;
			}
		}
		return true;
	}

	public void MarkProtectedMinutesDirty(float delay = 0f)
	{
		nextProtectedCalcTime = UnityEngine.Time.realtimeSinceStartup + delay;
	}

	public float CalculateBuildingTaxRate()
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null)
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		if (!building.HasBuildingBlocks())
		{
			return ConVar.Decay.bracket_0_costfraction;
		}
		int count = building.buildingBlocks.Count;
		int num = count;
		for (int i = 0; i < upkeepBrackets.Length; i++)
		{
			UpkeepBracket upkeepBracket = upkeepBrackets[i];
			upkeepBracket.blocksTaxPaid = 0f;
			if (num > 0)
			{
				int num2 = 0;
				num2 = ((i != upkeepBrackets.Length - 1) ? Mathf.Min(num, upkeepBrackets[i].objectsUpTo) : num);
				num -= num2;
				upkeepBracket.blocksTaxPaid = (float)num2 * upkeepBracket.fraction;
			}
		}
		float num3 = 0f;
		for (int j = 0; j < upkeepBrackets.Length; j++)
		{
			UpkeepBracket upkeepBracket2 = upkeepBrackets[j];
			if (!(upkeepBracket2.blocksTaxPaid > 0f))
			{
				break;
			}
			num3 += upkeepBracket2.blocksTaxPaid;
		}
		return num3 / (float)count;
	}

	public void ApplyUpkeepPayment()
	{
		List<Item> obj = Facepunch.Pool.GetList<Item>();
		for (int i = 0; i < upkeepBuffer.Count; i++)
		{
			ItemAmount itemAmount = upkeepBuffer[i];
			int num = (int)itemAmount.amount;
			if (num < 1)
			{
				continue;
			}
			base.inventory.Take(obj, itemAmount.itemid, num);
			foreach (Item item in obj)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Using " + item.amount + " of " + item.info.shortname);
				}
				item.UseItem(item.amount);
			}
			obj.Clear();
			itemAmount.amount -= num;
			upkeepBuffer[i] = itemAmount;
		}
		Facepunch.Pool.FreeList(ref obj);
	}

	public void QueueUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			bool flag = false;
			foreach (ItemAmount item in upkeepBuffer)
			{
				if (item.itemDef == itemAmount.itemDef)
				{
					item.amount += itemAmount.amount;
					if (IsDebugging())
					{
						Debug.Log(ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname + " to " + item.amount);
					}
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Adding " + itemAmount.amount + " of " + itemAmount.itemDef.shortname);
				}
				upkeepBuffer.Add(new ItemAmount(itemAmount.itemDef, itemAmount.amount));
			}
		}
	}

	public bool CanAffordUpkeepPayment(List<ItemAmount> itemAmounts)
	{
		for (int i = 0; i < itemAmounts.Count; i++)
		{
			ItemAmount itemAmount = itemAmounts[i];
			if ((float)base.inventory.GetAmount(itemAmount.itemid, true) < itemAmount.amount)
			{
				if (IsDebugging())
				{
					Debug.Log(ToString() + ": Can't afford " + itemAmount.amount + " of " + itemAmount.itemDef.shortname);
				}
				return false;
			}
		}
		return true;
	}

	public float PurchaseUpkeepTime(DecayEntity entity, float deltaTime)
	{
		float num = CalculateUpkeepCostFraction();
		float num2 = CalculateUpkeepPeriodMinutes() * 60f;
		float multiplier = num * deltaTime / num2;
		List<ItemAmount> obj = Facepunch.Pool.GetList<ItemAmount>();
		entity.CalculateUpkeepCostAmounts(obj, multiplier);
		bool num3 = CanAffordUpkeepPayment(obj);
		QueueUpkeepPayment(obj);
		Facepunch.Pool.FreeList(ref obj);
		ApplyUpkeepPayment();
		if (!num3)
		{
			return 0f;
		}
		return deltaTime;
	}

	public void PurchaseUpkeepTime(float deltaTime)
	{
		BuildingManager.Building building = GetBuilding();
		if (building == null || !building.HasDecayEntities())
		{
			return;
		}
		float num = Mathf.Min(GetProtectedMinutes(true) * 60f, deltaTime);
		if (!(num > 0f))
		{
			return;
		}
		foreach (DecayEntity decayEntity in building.decayEntities)
		{
			float protectedSeconds = decayEntity.GetProtectedSeconds();
			if (num > protectedSeconds)
			{
				float num2 = PurchaseUpkeepTime(decayEntity, num - protectedSeconds);
				decayEntity.AddUpkeepTime(num2);
				if (IsDebugging())
				{
					Debug.Log(ToString() + " purchased upkeep time for " + decayEntity.ToString() + ": " + protectedSeconds + " + " + num2 + " = " + decayEntity.GetProtectedSeconds());
				}
			}
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		authorizedPlayers.Clear();
	}

	public bool IsAuthed(BasePlayer player)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == player.userID);
	}

	public bool IsAuthed(ulong userID)
	{
		return authorizedPlayers.Any((PlayerNameID x) => x.userid == userID);
	}

	public bool AnyAuthed()
	{
		return authorizedPlayers.Count > 0;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		return base.ItemFilter(item, targetSlot);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.buildingPrivilege = Facepunch.Pool.Get<BuildingPrivilege>();
		info.msg.buildingPrivilege.users = authorizedPlayers;
		if (!info.forDisk)
		{
			info.msg.buildingPrivilege.upkeepPeriodMinutes = CalculateUpkeepPeriodMinutes();
			info.msg.buildingPrivilege.costFraction = CalculateUpkeepCostFraction();
			info.msg.buildingPrivilege.protectedMinutes = GetProtectedMinutes();
		}
	}

	public override void PostSave(SaveInfo info)
	{
		info.msg.buildingPrivilege.users = null;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		authorizedPlayers.Clear();
		if (info.msg.buildingPrivilege != null && info.msg.buildingPrivilege.users != null)
		{
			authorizedPlayers = info.msg.buildingPrivilege.users;
			if (!info.fromDisk)
			{
				cachedProtectedMinutes = info.msg.buildingPrivilege.protectedMinutes;
			}
			info.msg.buildingPrivilege.users = null;
		}
	}

	public void BuildingDirty()
	{
		if (base.isServer)
		{
			AddDelayedUpdate();
		}
	}

	public bool AtMaxAuthCapacity()
	{
		return HasFlag(Flags.Reserved5);
	}

	public void UpdateMaxAuthCapacity()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(true);
		if ((bool)activeGameMode && activeGameMode.limitTeamAuths)
		{
			SetFlag(Flags.Reserved5, authorizedPlayers.Count >= activeGameMode.GetMaxRelationshipTeamSize());
		}
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		AddDelayedUpdate();
	}

	public override void OnItemAddedOrRemoved(Item item, bool bAdded)
	{
		base.OnItemAddedOrRemoved(item, bAdded);
		AddDelayedUpdate();
	}

	public void AddDelayedUpdate()
	{
		if (IsInvoking(DelayedUpdate))
		{
			CancelInvoke(DelayedUpdate);
		}
		Invoke(DelayedUpdate, 1f);
	}

	public void DelayedUpdate()
	{
		MarkProtectedMinutesDirty();
		SendNetworkUpdate();
	}

	public bool CanAdministrate(BasePlayer player)
	{
		BaseLock baseLock = GetSlot(Slot.Lock) as BaseLock;
		if (baseLock == null)
		{
			return true;
		}
		return baseLock.OnTryToOpen(player);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void AddSelfAuthorize(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardAuthorize", this, rpc.player) == null)
		{
			AddPlayer(rpc.player);
			SendNetworkUpdate();
		}
	}

	public void AddPlayer(BasePlayer player)
	{
		if (!AtMaxAuthCapacity())
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == player.userID);
			PlayerNameID playerNameID = new PlayerNameID();
			playerNameID.userid = player.userID;
			playerNameID.username = player.displayName;
			authorizedPlayers.Add(playerNameID);
			UpdateMaxAuthCapacity();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RemoveSelfAuthorize(RPCMessage rpc)
	{
		RPCMessage rpc2 = rpc;
		if (rpc2.player.CanInteract() && CanAdministrate(rpc2.player) && Interface.CallHook("OnCupboardDeauthorize", this, rpc.player) == null)
		{
			authorizedPlayers.RemoveAll((PlayerNameID x) => x.userid == rpc2.player.userID);
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ClearList(RPCMessage rpc)
	{
		if (rpc.player.CanInteract() && CanAdministrate(rpc.player) && Interface.CallHook("OnCupboardClearList", this, rpc.player) == null)
		{
			authorizedPlayers.Clear();
			UpdateMaxAuthCapacity();
			SendNetworkUpdate();
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Rotate(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player.CanBuild() && (bool)player.GetHeldEntity() && player.GetHeldEntity().GetComponent<Hammer>() != null && (GetSlot(Slot.Lock) == null || !GetSlot(Slot.Lock).IsLocked()))
		{
			base.transform.rotation = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			SendNetworkUpdate();
			Deployable component = GetComponent<Deployable>();
			if (component != null && component.placeEffect.isValid)
			{
				Effect.server.Run(component.placeEffect.resourcePath, base.transform.position, Vector3.up);
			}
		}
		BaseEntity slot = GetSlot(Slot.Lock);
		if (slot != null)
		{
			slot.SendNetworkUpdate();
		}
	}

	public override bool HasSlot(Slot slot)
	{
		if (slot == Slot.Lock)
		{
			return true;
		}
		return base.HasSlot(slot);
	}
}
