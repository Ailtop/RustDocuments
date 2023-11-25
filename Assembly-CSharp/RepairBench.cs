#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class RepairBench : StorageContainer
{
	public float maxConditionLostOnRepair = 0.2f;

	public GameObjectRef skinchangeEffect;

	public const float REPAIR_COST_FRACTION = 0.2f;

	private float nextSkinChangeTime;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RepairBench.OnRpcMessage"))
		{
			if (rpc == 1942825351 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - ChangeSkin ");
				}
				using (TimeWarning.New("ChangeSkin"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1942825351u, "ChangeSkin", this, player, 3f))
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
							ChangeSkin(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in ChangeSkin");
					}
				}
				return true;
			}
			if (rpc == 1178348163 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RepairItem ");
				}
				using (TimeWarning.New("RepairItem"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1178348163u, "RepairItem", this, player, 3f))
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
							RepairItem(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in RepairItem");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static float GetRepairFraction(Item itemToRepair)
	{
		return 1f - itemToRepair.condition / itemToRepair.maxCondition;
	}

	public static float RepairCostFraction(Item itemToRepair)
	{
		return GetRepairFraction(itemToRepair) * 0.2f;
	}

	public static void GetRepairCostList(ItemBlueprint bp, List<ItemAmount> allIngredients)
	{
		ItemModRepair itemModRepair = bp.targetItem?.GetComponent<ItemModRepair>();
		if (itemModRepair != null && itemModRepair.canUseRepairBench)
		{
			return;
		}
		foreach (ItemAmount ingredient in bp.ingredients)
		{
			allIngredients.Add(new ItemAmount(ingredient.itemDef, ingredient.amount));
		}
		StripComponentRepairCost(allIngredients);
	}

	public static void StripComponentRepairCost(List<ItemAmount> allIngredients)
	{
		if (allIngredients == null)
		{
			return;
		}
		for (int i = 0; i < allIngredients.Count; i++)
		{
			ItemAmount itemAmount = allIngredients[i];
			if (itemAmount.itemDef.category != ItemCategory.Component)
			{
				continue;
			}
			if (itemAmount.itemDef.Blueprint != null)
			{
				bool flag = false;
				ItemAmount itemAmount2 = itemAmount.itemDef.Blueprint.ingredients[0];
				foreach (ItemAmount allIngredient in allIngredients)
				{
					if (allIngredient.itemDef == itemAmount2.itemDef)
					{
						allIngredient.amount += itemAmount2.amount * itemAmount.amount;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					allIngredients.Add(new ItemAmount(itemAmount2.itemDef, itemAmount2.amount * itemAmount.amount));
				}
			}
			allIngredients.RemoveAt(i);
			i--;
		}
	}

	public void debugprint(string toPrint)
	{
		if (Global.developer > 0)
		{
			Debug.LogWarning(toPrint);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void ChangeSkin(RPCMessage msg)
	{
		bool flag = UnityEngine.Time.realtimeSinceStartup < nextSkinChangeTime;
		BasePlayer player = msg.player;
		int num = msg.read.Int32();
		ItemId itemId = new ItemId(msg.read.UInt64());
		bool isValid = itemId.IsValid;
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || Interface.CallHook("OnItemSkinChange", num, slot, this, player) != null || (isValid && slot.uid != itemId))
		{
			return;
		}
		bool flag2 = false;
		if (msg.player.UnlockAllSkins)
		{
			flag2 = true;
		}
		if (num != 0 && !flag2 && !player.blueprints.CheckSkinOwnership(num, player.userID))
		{
			debugprint("RepairBench.ChangeSkin player does not have item :" + num + ":");
			return;
		}
		ulong Skin = ItemDefinition.FindSkin(slot.info.itemid, num);
		if (Skin == slot.skin && slot.info.isRedirectOf == null)
		{
			debugprint("RepairBench.ChangeSkin cannot apply same skin twice : " + Skin + ": " + slot.skin);
			return;
		}
		nextSkinChangeTime = UnityEngine.Time.realtimeSinceStartup + 0.75f;
		ItemSkinDirectory.Skin skin = slot.info.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
		if (slot.info.isRedirectOf != null)
		{
			Skin = ItemDefinition.FindSkin(slot.info.isRedirectOf.itemid, num);
			skin = slot.info.isRedirectOf.skins.FirstOrDefault((ItemSkinDirectory.Skin x) => (ulong)x.id == Skin);
		}
		ItemSkin itemSkin = ((skin.id == 0) ? null : (skin.invItem as ItemSkin));
		if (((bool)itemSkin && (itemSkin.Redirect != null || slot.info.isRedirectOf != null)) || (!itemSkin && slot.info.isRedirectOf != null))
		{
			ItemDefinition template = ((itemSkin != null) ? itemSkin.Redirect : slot.info.isRedirectOf);
			bool flag3 = false;
			if (itemSkin != null && itemSkin.Redirect == null && slot.info.isRedirectOf != null)
			{
				template = slot.info.isRedirectOf;
				flag3 = num != 0;
			}
			float condition = slot.condition;
			float maxCondition = slot.maxCondition;
			int amount = slot.amount;
			int ammoCount = 0;
			ItemDefinition ammoType = null;
			if (slot.GetHeldEntity() != null && slot.GetHeldEntity() is BaseProjectile { primaryMagazine: not null } baseProjectile)
			{
				ammoCount = baseProjectile.primaryMagazine.contents;
				ammoType = baseProjectile.primaryMagazine.ammoType;
			}
			List<Item> obj = Facepunch.Pool.GetList<Item>();
			if (slot.contents != null && slot.contents.itemList != null && slot.contents.itemList.Count > 0)
			{
				foreach (Item item2 in slot.contents.itemList)
				{
					obj.Add(item2);
				}
				foreach (Item item3 in obj)
				{
					item3.RemoveFromContainer();
				}
			}
			slot.Remove();
			ItemManager.DoRemoves();
			Item item = ItemManager.Create(template, 1, 0uL);
			item.MoveToContainer(base.inventory, 0, allowStack: false);
			item.maxCondition = maxCondition;
			item.condition = condition;
			item.amount = amount;
			if (item.GetHeldEntity() != null && item.GetHeldEntity() is BaseProjectile baseProjectile2)
			{
				if (baseProjectile2.primaryMagazine != null)
				{
					baseProjectile2.SetAmmoCount(ammoCount);
					baseProjectile2.primaryMagazine.ammoType = ammoType;
				}
				baseProjectile2.ForceModsChanged();
			}
			if (obj.Count > 0 && item.contents != null)
			{
				foreach (Item item4 in obj)
				{
					item4.MoveToContainer(item.contents);
				}
			}
			Facepunch.Pool.FreeList(ref obj);
			if (flag3)
			{
				ApplySkinToItem(item, Skin);
			}
			Facepunch.Rust.Analytics.Server.SkinUsed(item.info.shortname, num);
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, item, Skin);
		}
		else
		{
			ApplySkinToItem(slot, Skin);
			Facepunch.Rust.Analytics.Server.SkinUsed(slot.info.shortname, num);
			Facepunch.Rust.Analytics.Azure.OnSkinChanged(player, this, slot, Skin);
		}
		if (flag && skinchangeEffect.isValid)
		{
			Effect.server.Run(skinchangeEffect.resourcePath, this, 0u, new Vector3(0f, 1.5f, 0f), Vector3.zero);
		}
	}

	private void ApplySkinToItem(Item item, ulong Skin)
	{
		item.skin = Skin;
		item.MarkDirty();
		BaseEntity heldEntity = item.GetHeldEntity();
		if (heldEntity != null)
		{
			heldEntity.skinID = Skin;
			heldEntity.SendNetworkUpdate();
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RepairItem(RPCMessage msg)
	{
		Item slot = base.inventory.GetSlot(0);
		BasePlayer player = msg.player;
		float conditionLost = maxConditionLostOnRepair;
		ItemModRepair component = slot.info.GetComponent<ItemModRepair>();
		if (component != null)
		{
			conditionLost = component.conditionLost;
		}
		RepairAnItem(slot, player, this, conditionLost, mustKnowBlueprint: true);
	}

	public override int GetIdealSlot(BasePlayer player, Item item)
	{
		return 0;
	}

	public static void RepairAnItem(Item itemToRepair, BasePlayer player, BaseEntity repairBenchEntity, float maxConditionLostOnRepair, bool mustKnowBlueprint)
	{
		if (itemToRepair == null)
		{
			return;
		}
		ItemDefinition info = itemToRepair.info;
		ItemBlueprint component = info.GetComponent<ItemBlueprint>();
		if (!component)
		{
			return;
		}
		ItemModRepair component2 = itemToRepair.info.GetComponent<ItemModRepair>();
		if (!info.condition.repairable || itemToRepair.condition == itemToRepair.maxCondition)
		{
			return;
		}
		if (mustKnowBlueprint)
		{
			ItemDefinition itemDefinition = ((info.isRedirectOf != null) ? info.isRedirectOf : info);
			if (!player.blueprints.HasUnlocked(itemDefinition) && (!(itemDefinition.Blueprint != null) || itemDefinition.Blueprint.isResearchable))
			{
				return;
			}
		}
		if (Interface.CallHook("OnItemRepair", player, itemToRepair) != null)
		{
			return;
		}
		float num = RepairCostFraction(itemToRepair);
		bool flag = false;
		List<ItemAmount> obj = Facepunch.Pool.GetList<ItemAmount>();
		GetRepairCostList(component, obj);
		foreach (ItemAmount item in obj)
		{
			if (item.itemDef.category != ItemCategory.Component)
			{
				int amount = player.inventory.GetAmount(item.itemDef.itemid);
				if (Mathf.CeilToInt(item.amount * num) > amount)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Facepunch.Pool.FreeList(ref obj);
			return;
		}
		foreach (ItemAmount item2 in obj)
		{
			if (item2.itemDef.category != ItemCategory.Component)
			{
				int amount2 = Mathf.CeilToInt(item2.amount * num);
				player.inventory.Take(null, item2.itemid, amount2);
				Facepunch.Rust.Analytics.Azure.LogResource(Facepunch.Rust.Analytics.Azure.ResourceMode.Consumed, "repair", item2.itemDef.shortname, amount2, repairBenchEntity, null, safezone: false, null, 0uL, null, itemToRepair);
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		float conditionNormalized = itemToRepair.conditionNormalized;
		float maxConditionNormalized = itemToRepair.maxConditionNormalized;
		itemToRepair.DoRepair(maxConditionLostOnRepair);
		Facepunch.Rust.Analytics.Azure.OnItemRepaired(player, repairBenchEntity, itemToRepair, conditionNormalized, maxConditionNormalized);
		if (Global.developer > 0)
		{
			Debug.Log("Item repaired! condition : " + itemToRepair.condition + "/" + itemToRepair.maxCondition);
		}
		string strName = "assets/bundled/prefabs/fx/repairbench/itemrepair.prefab";
		if (component2 != null && component2.successEffect?.Get() != null)
		{
			strName = component2.successEffect.resourcePath;
		}
		Effect.server.Run(strName, repairBenchEntity, 0u, Vector3.zero, Vector3.zero);
	}
}
