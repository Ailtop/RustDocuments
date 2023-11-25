using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;

public class ItemCrafter : EntityComponent<BasePlayer>
{
	public List<ItemContainer> containers = new List<ItemContainer>();

	public LinkedList<ItemCraftTask> queue = new LinkedList<ItemCraftTask>();

	public int taskUID;

	[NonSerialized]
	public BasePlayer owner;

	public void AddContainer(ItemContainer container)
	{
		containers.Add(container);
	}

	public static float GetScaledDuration(ItemBlueprint bp, float workbenchLevel)
	{
		float num = workbenchLevel - (float)bp.workbenchLevelRequired;
		if (num == 1f)
		{
			return bp.time * 0.5f;
		}
		if (num >= 2f)
		{
			return bp.time * 0.25f;
		}
		return bp.time;
	}

	public void ServerUpdate(float delta)
	{
		if (queue.Count == 0)
		{
			return;
		}
		ItemCraftTask value = queue.First.Value;
		if (value.cancelled)
		{
			owner.Command("note.craft_done", value.taskUID, 0);
			queue.RemoveFirst();
			return;
		}
		float currentCraftLevel = owner.currentCraftLevel;
		if (value.endTime > UnityEngine.Time.realtimeSinceStartup)
		{
			return;
		}
		if (value.endTime == 0f)
		{
			float scaledDuration = GetScaledDuration(value.blueprint, currentCraftLevel);
			value.endTime = UnityEngine.Time.realtimeSinceStartup + scaledDuration;
			value.workbenchEntity = owner.GetCachedCraftLevelWorkbench();
			if (owner != null)
			{
				owner.Command("note.craft_start", value.taskUID, scaledDuration, value.amount);
				if (owner.IsAdmin && Craft.instant)
				{
					value.endTime = UnityEngine.Time.realtimeSinceStartup + 1f;
				}
			}
		}
		else
		{
			FinishCrafting(value);
			if (value.amount <= 0)
			{
				queue.RemoveFirst();
			}
			else
			{
				value.endTime = 0f;
			}
		}
	}

	public void CollectIngredient(int item, int amount, List<Item> collect)
	{
		foreach (ItemContainer container in containers)
		{
			amount -= container.Take(collect, item, amount);
			if (amount <= 0)
			{
				break;
			}
		}
	}

	public void CollectIngredients(ItemBlueprint bp, ItemCraftTask task, int amount = 1, BasePlayer player = null)
	{
		if (Interface.CallHook("OnIngredientsCollect", this, bp, task, amount, player) != null)
		{
			return;
		}
		List<Item> list = new List<Item>();
		foreach (ItemAmount ingredient in bp.ingredients)
		{
			CollectIngredient(ingredient.itemid, (int)ingredient.amount * amount, list);
		}
		foreach (Item item in list)
		{
			item.CollectedForCrafting(player);
		}
		task.takenItems = list;
	}

	public bool CraftItem(ItemBlueprint bp, BasePlayer owner, ProtoBuf.Item.InstanceData instanceData = null, int amount = 1, int skinID = 0, Item fromTempBlueprint = null, bool free = false)
	{
		if (owner != null && owner.IsTransferring())
		{
			return false;
		}
		if (!CanCraft(bp, amount, free))
		{
			return false;
		}
		taskUID++;
		ItemCraftTask itemCraftTask = Facepunch.Pool.Get<ItemCraftTask>();
		itemCraftTask.blueprint = bp;
		if (!free)
		{
			CollectIngredients(bp, itemCraftTask, amount, owner);
		}
		itemCraftTask.endTime = 0f;
		itemCraftTask.taskUID = taskUID;
		itemCraftTask.instanceData = instanceData;
		if (itemCraftTask.instanceData != null)
		{
			itemCraftTask.instanceData.ShouldPool = false;
		}
		itemCraftTask.amount = amount;
		itemCraftTask.skinID = skinID;
		if (fromTempBlueprint != null && itemCraftTask.takenItems != null)
		{
			fromTempBlueprint.RemoveFromContainer();
			itemCraftTask.takenItems.Add(fromTempBlueprint);
			itemCraftTask.conditionScale = 0.5f;
		}
		object obj = Interface.CallHook("OnItemCraft", itemCraftTask, owner, fromTempBlueprint);
		if (obj is bool)
		{
			if (fromTempBlueprint != null && itemCraftTask.instanceData != null)
			{
				fromTempBlueprint.instanceData = itemCraftTask.instanceData;
			}
			return (bool)obj;
		}
		queue.AddLast(itemCraftTask);
		if (owner != null)
		{
			owner.Command("note.craft_add", itemCraftTask.taskUID, itemCraftTask.blueprint.targetItem.itemid, amount, itemCraftTask.skinID);
		}
		return true;
	}

	public void FinishCrafting(ItemCraftTask task)
	{
		task.amount--;
		task.numCrafted++;
		ulong skin = ItemDefinition.FindSkin(task.blueprint.targetItem.itemid, task.skinID);
		Item item = ItemManager.CreateByItemID(task.blueprint.targetItem.itemid, 1, skin);
		item.amount = task.blueprint.amountToCreate;
		int amount = item.amount;
		_ = owner.currentCraftLevel;
		bool inSafezone = owner.InSafeZone();
		if (item.hasCondition && task.conditionScale != 1f)
		{
			item.maxCondition *= task.conditionScale;
			item.condition = item.maxCondition;
		}
		item.OnVirginSpawn();
		foreach (ItemAmount ingredient in task.blueprint.ingredients)
		{
			int num = (int)ingredient.amount;
			if (task.takenItems == null)
			{
				continue;
			}
			foreach (Item takenItem in task.takenItems)
			{
				if (takenItem.info == ingredient.itemDef)
				{
					int num2 = Mathf.Min(takenItem.amount, num);
					Facepunch.Rust.Analytics.Azure.OnCraftMaterialConsumed(takenItem.info.shortname, num, base.baseEntity, task.workbenchEntity, inSafezone, item.info.shortname);
					takenItem.UseItem(num);
					num -= num2;
				}
				_ = 0;
			}
		}
		Facepunch.Rust.Analytics.Server.Crafting(task.blueprint.targetItem.shortname, task.skinID);
		Facepunch.Rust.Analytics.Azure.OnCraftItem(item.info.shortname, item.amount, base.baseEntity, task.workbenchEntity, inSafezone);
		owner.Command("note.craft_done", task.taskUID, 1, task.amount);
		Interface.CallHook("OnItemCraftFinished", task, item, this);
		if (task.instanceData != null)
		{
			item.instanceData = task.instanceData;
		}
		if (!string.IsNullOrEmpty(task.blueprint.UnlockAchievment))
		{
			owner.GiveAchievement(task.blueprint.UnlockAchievment);
		}
		if (owner.inventory.GiveItem(item))
		{
			owner.Command("note.inv", item.info.itemid, amount);
			return;
		}
		ItemContainer itemContainer = containers.First();
		owner.Command("note.inv", item.info.itemid, amount);
		owner.Command("note.inv", item.info.itemid, -item.amount);
		item.Drop(itemContainer.dropPosition, itemContainer.dropVelocity);
	}

	public bool CancelTask(int iID, bool ReturnItems)
	{
		if (queue.Count == 0)
		{
			return false;
		}
		if (owner != null && owner.IsTransferring())
		{
			return false;
		}
		ItemCraftTask itemCraftTask = queue.FirstOrDefault((ItemCraftTask x) => x.taskUID == iID && !x.cancelled);
		if (itemCraftTask == null)
		{
			return false;
		}
		itemCraftTask.cancelled = true;
		if (owner == null)
		{
			return true;
		}
		Interface.CallHook("OnItemCraftCancelled", itemCraftTask, this);
		owner.Command("note.craft_done", itemCraftTask.taskUID, 0);
		if (itemCraftTask.takenItems != null && itemCraftTask.takenItems.Count > 0 && ReturnItems)
		{
			foreach (Item takenItem in itemCraftTask.takenItems)
			{
				if (takenItem != null && takenItem.amount > 0)
				{
					if (takenItem.IsBlueprint() && takenItem.blueprintTargetDef == itemCraftTask.blueprint.targetItem)
					{
						takenItem.UseItem(itemCraftTask.numCrafted);
					}
					if (takenItem.amount > 0 && !takenItem.MoveToContainer(owner.inventory.containerMain))
					{
						takenItem.Drop(owner.inventory.containerMain.dropPosition + UnityEngine.Random.value * Vector3.down + UnityEngine.Random.insideUnitSphere, owner.inventory.containerMain.dropVelocity);
						owner.Command("note.inv", takenItem.info.itemid, -takenItem.amount);
					}
				}
			}
		}
		return true;
	}

	public bool CancelBlueprint(int itemid)
	{
		if (queue.Count == 0)
		{
			return false;
		}
		if (owner != null && owner.IsTransferring())
		{
			return false;
		}
		ItemCraftTask itemCraftTask = queue.FirstOrDefault((ItemCraftTask x) => x.blueprint.targetItem.itemid == itemid && !x.cancelled);
		if (itemCraftTask == null)
		{
			return false;
		}
		return CancelTask(itemCraftTask.taskUID, ReturnItems: true);
	}

	public void CancelAll(bool returnItems)
	{
		foreach (ItemCraftTask item in queue)
		{
			CancelTask(item.taskUID, returnItems);
		}
	}

	public bool DoesHaveUsableItem(int item, int iAmount)
	{
		int num = 0;
		foreach (ItemContainer container in containers)
		{
			num += container.GetAmount(item, onlyUsableAmounts: true);
		}
		return num >= iAmount;
	}

	public bool CanCraft(ItemBlueprint bp, int amount = 1, bool free = false)
	{
		float num = (float)amount / (float)bp.targetItem.craftingStackable;
		foreach (ItemCraftTask item in queue)
		{
			if (!item.cancelled)
			{
				num += (float)item.amount / (float)item.blueprint.targetItem.craftingStackable;
			}
		}
		if (num > 8f)
		{
			return false;
		}
		if (amount < 1 || amount > bp.targetItem.craftingStackable)
		{
			return false;
		}
		object obj = Interface.CallHook("CanCraft", this, bp, amount, free);
		if (obj is bool)
		{
			return (bool)obj;
		}
		foreach (ItemAmount ingredient in bp.ingredients)
		{
			if (!DoesHaveUsableItem(ingredient.itemid, (int)ingredient.amount * amount))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanCraft(ItemDefinition def, int amount = 1, bool free = false)
	{
		ItemBlueprint component = def.GetComponent<ItemBlueprint>();
		if (CanCraft(component, amount, free))
		{
			return true;
		}
		return false;
	}

	public bool FastTrackTask(int taskID)
	{
		int taskID2 = taskID;
		if (queue.Count == 0)
		{
			return false;
		}
		if (owner != null && owner.IsTransferring())
		{
			return false;
		}
		ItemCraftTask value = queue.First.Value;
		if (value == null)
		{
			return false;
		}
		ItemCraftTask itemCraftTask = queue.FirstOrDefault((ItemCraftTask x) => x.taskUID == taskID2 && !x.cancelled);
		if (itemCraftTask == null)
		{
			return false;
		}
		if (itemCraftTask == value)
		{
			return false;
		}
		object obj = Interface.CallHook("CanFastTrackCraftTask", this, itemCraftTask, taskID);
		if (obj is bool)
		{
			return (bool)obj;
		}
		value.endTime = 0f;
		queue.Remove(itemCraftTask);
		queue.AddFirst(itemCraftTask);
		owner.Command("note.craft_fasttracked", taskID2);
		return true;
	}

	public ProtoBuf.ItemCrafter Save()
	{
		ProtoBuf.ItemCrafter itemCrafter = Facepunch.Pool.Get<ProtoBuf.ItemCrafter>();
		itemCrafter.queue = Facepunch.Pool.GetList<ProtoBuf.ItemCrafter.Task>();
		foreach (ItemCraftTask item in queue)
		{
			ProtoBuf.ItemCrafter.Task task = Facepunch.Pool.Get<ProtoBuf.ItemCrafter.Task>();
			task.itemID = item.blueprint.targetItem.itemid;
			task.remainingTime = ((item.endTime > 0f) ? (item.endTime - UnityEngine.Time.realtimeSinceStartup) : 0f);
			task.taskUID = item.taskUID;
			task.cancelled = item.cancelled;
			task.instanceData = item.instanceData?.Copy();
			task.amount = item.amount;
			task.skinID = item.skinID;
			task.takenItems = SaveItems(item.takenItems);
			task.numCrafted = item.numCrafted;
			task.conditionScale = item.conditionScale;
			task.workbenchEntity = (BaseNetworkableEx.IsValid(item.workbenchEntity) ? item.workbenchEntity.net.ID : default(NetworkableId));
			itemCrafter.queue.Add(task);
		}
		return itemCrafter;
		static List<ProtoBuf.Item> SaveItems(List<Item> items)
		{
			List<ProtoBuf.Item> list = Facepunch.Pool.GetList<ProtoBuf.Item>();
			if (items != null)
			{
				foreach (Item item2 in items)
				{
					list.Add(item2.Save(bIncludeContainer: true));
				}
			}
			return list;
		}
	}

	public void Load(ProtoBuf.ItemCrafter proto)
	{
		if (proto?.queue == null)
		{
			return;
		}
		queue.Clear();
		foreach (ProtoBuf.ItemCrafter.Task item in proto.queue)
		{
			ItemDefinition itemDefinition = ItemManager.FindItemDefinition(item.itemID);
			if (itemDefinition == null || !itemDefinition.TryGetComponent<ItemBlueprint>(out var component))
			{
				Debug.LogWarning($"ItemCrafter has queue task for item ID {item.itemID}, but it was not found or has no blueprint. Skipping it");
				continue;
			}
			ItemCraftTask itemCraftTask = Facepunch.Pool.Get<ItemCraftTask>();
			itemCraftTask.blueprint = component;
			itemCraftTask.endTime = ((item.remainingTime > 0f) ? (UnityEngine.Time.realtimeSinceStartup + item.remainingTime) : 0f);
			itemCraftTask.taskUID = item.taskUID;
			itemCraftTask.cancelled = item.cancelled;
			itemCraftTask.instanceData = item.instanceData?.Copy();
			itemCraftTask.amount = item.amount;
			itemCraftTask.skinID = item.skinID;
			itemCraftTask.takenItems = LoadItems(item.takenItems);
			itemCraftTask.numCrafted = item.numCrafted;
			itemCraftTask.conditionScale = item.conditionScale;
			itemCraftTask.workbenchEntity = new EntityRef<BaseEntity>
			{
				uid = item.workbenchEntity
			}.Get(serverside: true);
			queue.AddLast(itemCraftTask);
			taskUID = Mathf.Max(taskUID, itemCraftTask.taskUID);
		}
		static List<Item> LoadItems(List<ProtoBuf.Item> itemProtos)
		{
			List<Item> list = new List<Item>();
			if (itemProtos != null)
			{
				foreach (ProtoBuf.Item itemProto in itemProtos)
				{
					list.Add(ItemManager.Load(itemProto, null, isServer: true));
				}
			}
			return list;
		}
	}

	public void SendToOwner()
	{
		if (!BaseNetworkableEx.IsValid(owner) || !owner.IsConnected)
		{
			return;
		}
		foreach (ItemCraftTask item in queue)
		{
			owner.Command("note.craft_add", item.taskUID, item.blueprint.targetItem.itemid, item.amount, item.skinID);
		}
	}
}
