#define UNITY_ASSERTIONS
using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class Workbench : StorageContainer
{
	public const int blueprintSlot = 0;

	public const int experimentSlot = 1;

	public bool Static;

	public int Workbenchlevel;

	public LootSpawn experimentalItems;

	public GameObjectRef experimentStartEffect;

	public GameObjectRef experimentSuccessEffect;

	public ItemDefinition experimentResource;

	public TechTreeData techTree;

	public bool supportsIndustrialCrafter;

	public static ItemDefinition blueprintBaseDef;

	private ItemDefinition pendingBlueprint;

	private bool creatingBlueprint;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Workbench.OnRpcMessage"))
		{
			if (rpc == 2308794761u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_BeginExperiment ");
				}
				using (TimeWarning.New("RPC_BeginExperiment"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(2308794761u, "RPC_BeginExperiment", this, player, 3f))
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
							RPC_BeginExperiment(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_BeginExperiment");
					}
				}
				return true;
			}
			if (rpc == 4127240744u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_TechTreeUnlock ");
				}
				using (TimeWarning.New("RPC_TechTreeUnlock"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4127240744u, "RPC_TechTreeUnlock", this, player, 3f))
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
							RPC_TechTreeUnlock(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RPC_TechTreeUnlock");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public int GetScrapForExperiment()
	{
		if (Workbenchlevel == 1)
		{
			return 75;
		}
		if (Workbenchlevel == 2)
		{
			return 300;
		}
		if (Workbenchlevel == 3)
		{
			return 1000;
		}
		Debug.LogWarning("GetScrapForExperiment fucked up big time.");
		return 0;
	}

	public bool IsWorking()
	{
		return HasFlag(Flags.On);
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (children.Count == 0)
		{
			return base.CanPickup(player);
		}
		return false;
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_TechTreeUnlock(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		int id = msg.read.Int32();
		TechTreeData.NodeInstance byID = techTree.GetByID(id);
		if (byID == null)
		{
			Debug.Log("Node for unlock not found :" + id);
		}
		else
		{
			if (!techTree.PlayerCanUnlock(player, byID))
			{
				return;
			}
			if (byID.IsGroup())
			{
				foreach (int output in byID.outputs)
				{
					TechTreeData.NodeInstance byID2 = techTree.GetByID(output);
					if (byID2 != null && byID2.itemDef != null)
					{
						player.blueprints.Unlock(byID2.itemDef);
						Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID2.itemDef, "techtree", 0, this);
					}
				}
				Debug.Log("Player unlocked group :" + byID.groupName);
			}
			else if (byID.itemDef != null && Interface.CallHook("OnTechTreeNodeUnlock", this, byID, player) == null)
			{
				int num = ResearchTable.ScrapForResearch(byID.itemDef, ResearchTable.ResearchType.TechTree);
				int itemid = ItemManager.FindItemDefinition("scrap").itemid;
				if (player.inventory.GetAmount(itemid) >= num)
				{
					player.inventory.Take(null, itemid, num);
					player.blueprints.Unlock(byID.itemDef);
					Interface.CallHook("OnTechTreeNodeUnlocked", this, byID, player);
					Facepunch.Rust.Analytics.Azure.OnBlueprintLearned(player, byID.itemDef, "techtree", num, this);
				}
			}
		}
	}

	public static ItemDefinition GetBlueprintTemplate()
	{
		if (blueprintBaseDef == null)
		{
			blueprintBaseDef = ItemManager.FindItemDefinition("blueprintbase");
		}
		return blueprintBaseDef;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_BeginExperiment(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (player == null || IsWorking())
		{
			return;
		}
		PersistantPlayer persistantPlayerInfo = player.PersistantPlayerInfo;
		int num = UnityEngine.Random.Range(0, experimentalItems.subSpawn.Length);
		for (int i = 0; i < experimentalItems.subSpawn.Length; i++)
		{
			int num2 = i + num;
			if (num2 >= experimentalItems.subSpawn.Length)
			{
				num2 -= experimentalItems.subSpawn.Length;
			}
			ItemDefinition itemDef = experimentalItems.subSpawn[num2].category.items[0].itemDef;
			if ((bool)itemDef.Blueprint && !itemDef.Blueprint.defaultBlueprint && itemDef.Blueprint.userCraftable && itemDef.Blueprint.isResearchable && !itemDef.Blueprint.NeedsSteamItem && !itemDef.Blueprint.NeedsSteamDLC && !persistantPlayerInfo.unlockedItems.Contains(itemDef.itemid))
			{
				pendingBlueprint = itemDef;
				break;
			}
		}
		if (pendingBlueprint == null)
		{
			player.ChatMessage("You have already unlocked everything for this workbench tier.");
		}
		else
		{
			if (Interface.CallHook("OnExperimentStart", this, player) != null)
			{
				return;
			}
			Item slot = base.inventory.GetSlot(0);
			if (slot != null)
			{
				if (!slot.MoveToContainer(player.inventory.containerMain))
				{
					slot.Drop(GetDropPosition(), GetDropVelocity());
				}
				player.inventory.loot.SendImmediate();
			}
			if (experimentStartEffect.isValid)
			{
				Effect.server.Run(experimentStartEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			SetFlag(Flags.On, b: true);
			base.inventory.SetLocked(isLocked: true);
			CancelInvoke(ExperimentComplete);
			Invoke(ExperimentComplete, 5f);
			SendNetworkUpdate();
			Interface.CallHook("OnExperimentStarted", this, player);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
	}

	public override void OnKilled(HitInfo info)
	{
		base.OnKilled(info);
		CancelInvoke(ExperimentComplete);
	}

	public int GetAvailableExperimentResources()
	{
		Item experimentResourceItem = GetExperimentResourceItem();
		if (experimentResourceItem == null || experimentResourceItem.info != experimentResource)
		{
			return 0;
		}
		return experimentResourceItem.amount;
	}

	public Item GetExperimentResourceItem()
	{
		return base.inventory.GetSlot(1);
	}

	public void ExperimentComplete()
	{
		Item experimentResourceItem = GetExperimentResourceItem();
		int scrapForExperiment = GetScrapForExperiment();
		if (pendingBlueprint == null)
		{
			Debug.LogWarning("Pending blueprint was null!");
		}
		if (Interface.CallHook("OnExperimentEnd", this) != null)
		{
			return;
		}
		if (experimentResourceItem != null && experimentResourceItem.amount >= scrapForExperiment && pendingBlueprint != null)
		{
			experimentResourceItem.UseItem(scrapForExperiment);
			Item item = ItemManager.Create(GetBlueprintTemplate(), 1, 0uL);
			item.blueprintTarget = pendingBlueprint.itemid;
			creatingBlueprint = true;
			if (!item.MoveToContainer(base.inventory, 0))
			{
				item.Drop(GetDropPosition(), GetDropVelocity());
			}
			creatingBlueprint = false;
			if (experimentSuccessEffect.isValid)
			{
				Effect.server.Run(experimentSuccessEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
		}
		SetFlag(Flags.On, b: false);
		pendingBlueprint = null;
		base.inventory.SetLocked(isLocked: false);
		SendNetworkUpdate();
		Interface.CallHook("OnExperimentEnded", this);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetFlag(Flags.On, b: false);
		if (base.inventory != null)
		{
			base.inventory.SetLocked(isLocked: false);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = ItemFilter;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if ((targetSlot == 1 && item.info == experimentResource) || (targetSlot == 0 && creatingBlueprint))
		{
			return true;
		}
		return false;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
