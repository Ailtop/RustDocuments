#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
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
			RPCMessage rPCMessage;
			if (rpc == 1942825351 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - ChangeSkin "));
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
							rPCMessage = default(RPCMessage);
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
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RepairItem "));
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
							rPCMessage = default(RPCMessage);
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
		foreach (ItemAmount ingredient in bp.ingredients)
		{
			allIngredients.Add(new ItemAmount(ingredient.itemDef, ingredient.amount));
		}
		foreach (ItemAmount ingredient2 in bp.ingredients)
		{
			if (ingredient2.itemDef.category != ItemCategory.Component || !(ingredient2.itemDef.Blueprint != null))
			{
				continue;
			}
			bool flag = false;
			ItemAmount itemAmount = ingredient2.itemDef.Blueprint.ingredients[0];
			foreach (ItemAmount allIngredient in allIngredients)
			{
				if (allIngredient.itemDef == itemAmount.itemDef)
				{
					allIngredient.amount += itemAmount.amount * ingredient2.amount;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				allIngredients.Add(new ItemAmount(itemAmount.itemDef, itemAmount.amount * ingredient2.amount));
			}
		}
	}

	public void debugprint(string toPrint)
	{
		if (Global.developer > 0)
		{
			Debug.LogWarning(toPrint);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void ChangeSkin(RPCMessage msg)
	{
		if (UnityEngine.Time.realtimeSinceStartup < nextSkinChangeTime)
		{
			return;
		}
		BasePlayer player = msg.player;
		int num = msg.read.Int32();
		Item slot = base.inventory.GetSlot(0);
		if (slot == null || Interface.CallHook("OnItemSkinChange", num, slot, this, msg.player) != null)
		{
			return;
		}
		bool flag = false;
		if (num != 0 && !flag && !player.blueprints.CheckSkinOwnership(num, player.userID))
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
			bool flag2 = false;
			if (itemSkin != null && itemSkin.Redirect == null && slot.info.isRedirectOf != null)
			{
				template = slot.info.isRedirectOf;
				flag2 = num != 0;
			}
			float condition = slot.condition;
			float maxCondition = slot.maxCondition;
			int amount = slot.amount;
			int contents = 0;
			ItemDefinition ammoType = null;
			BaseProjectile baseProjectile;
			if (slot.GetHeldEntity() != null && (object)(baseProjectile = slot.GetHeldEntity() as BaseProjectile) != null && baseProjectile.primaryMagazine != null)
			{
				contents = baseProjectile.primaryMagazine.contents;
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
			item.MoveToContainer(base.inventory, 0, false);
			item.maxCondition = maxCondition;
			item.condition = condition;
			item.amount = amount;
			BaseProjectile baseProjectile2;
			if (item.GetHeldEntity() != null && (object)(baseProjectile2 = item.GetHeldEntity() as BaseProjectile) != null && baseProjectile2.primaryMagazine != null)
			{
				baseProjectile2.primaryMagazine.contents = contents;
				baseProjectile2.primaryMagazine.ammoType = ammoType;
			}
			if (obj.Count > 0 && item.contents != null)
			{
				foreach (Item item4 in obj)
				{
					item4.MoveToContainer(item.contents);
				}
			}
			Facepunch.Pool.FreeList(ref obj);
			if (flag2)
			{
				ApplySkinToItem(item, Skin);
			}
		}
		else
		{
			ApplySkinToItem(slot, Skin);
		}
		if (skinchangeEffect.isValid)
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

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RepairItem(RPCMessage msg)
	{
		Item slot = base.inventory.GetSlot(0);
		BasePlayer player = msg.player;
		RepairAnItem(slot, player, this, maxConditionLostOnRepair, true);
	}

	public static void RepairAnItem(Item itemToRepair, BasePlayer player, BaseEntity repairBenchEntity, float maxConditionLostOnRepair, bool mustKnowBlueprint)
	{
		if (itemToRepair == null)
		{
			return;
		}
		ItemDefinition info = itemToRepair.info;
		ItemBlueprint component = info.GetComponent<ItemBlueprint>();
		if (!component || !info.condition.repairable || itemToRepair.condition == itemToRepair.maxCondition)
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
			}
		}
		Facepunch.Pool.FreeList(ref obj);
		itemToRepair.DoRepair(maxConditionLostOnRepair);
		if (Global.developer > 0)
		{
			Debug.Log("Item repaired! condition : " + itemToRepair.condition + "/" + itemToRepair.maxCondition);
		}
		Effect.server.Run("assets/bundled/prefabs/fx/repairbench/itemrepair.prefab", repairBenchEntity, 0u, Vector3.zero, Vector3.zero);
	}
}
