#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class MixingTable : StorageContainer
{
	public GameObject Particles;

	public RecipeList Recipes;

	public bool OnlyAcceptValidIngredients;

	public float lastTickTimestamp;

	private List<Item> inventoryItems = new List<Item>();

	private const float mixTickInterval = 1f;

	public Recipe currentRecipe;

	public int currentQuantity;

	public ItemDefinition currentProductionItem;

	public float RemainingMixTime { get; set; }

	public float TotalMixTime { get; set; }

	public BasePlayer MixStartingPlayer { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MixingTable.OnRpcMessage"))
		{
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SVSwitch "));
				}
				using (TimeWarning.New("SVSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(4167839872u, "SVSwitch", this, player, 3f))
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
							SVSwitch(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
		base.inventory.onItemAddedRemoved = OnItemAddedOrRemoved;
		RecipeDictionary.CacheRecipes(Recipes);
	}

	private bool CanAcceptItem(Item item, int targetSlot)
	{
		if (item == null)
		{
			return false;
		}
		if (!OnlyAcceptValidIngredients)
		{
			return true;
		}
		if (GetItemWaterAmount(item) > 0)
		{
			item = item.contents.itemList[0];
		}
		if (!(item.info == currentProductionItem))
		{
			return RecipeDictionary.ValidIngredientForARecipe(item, Recipes);
		}
		return true;
	}

	protected override void OnInventoryDirty()
	{
		base.OnInventoryDirty();
		if (IsOn())
		{
			StopMixing();
		}
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	private void SVSwitch(RPCMessage msg)
	{
		if (Interface.CallHook("OnMixingTableToggle", this, msg.player) != null)
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag != IsOn() && !(msg.player == null))
		{
			if (flag)
			{
				StartMixing(msg.player);
			}
			else
			{
				StopMixing();
			}
		}
	}

	public void StartMixing(BasePlayer player)
	{
		if (IsOn() || !CanStartMixing(player))
		{
			return;
		}
		MixStartingPlayer = player;
		List<Item> orderedContainerItems = GetOrderedContainerItems(base.inventory);
		int quantity;
		currentRecipe = RecipeDictionary.GetMatchingRecipeAndQuantity(Recipes, orderedContainerItems, out quantity);
		currentQuantity = quantity;
		if (!(currentRecipe == null) && (!currentRecipe.RequiresBlueprint || !(currentRecipe.ProducedItem != null) || player.blueprints.HasUnlocked(currentRecipe.ProducedItem)))
		{
			if (base.isServer)
			{
				lastTickTimestamp = UnityEngine.Time.realtimeSinceStartup;
			}
			RemainingMixTime = currentRecipe.MixingDuration * (float)currentQuantity;
			TotalMixTime = RemainingMixTime;
			ReturnExcessItems(orderedContainerItems, player);
			if (RemainingMixTime == 0f)
			{
				ProduceItem(currentRecipe, currentQuantity);
				return;
			}
			InvokeRepeating(TickMix, 1f, 1f);
			SetFlag(Flags.On, true);
			SendNetworkUpdateImmediate();
		}
	}

	protected virtual bool CanStartMixing(BasePlayer player)
	{
		return true;
	}

	public void StopMixing()
	{
		currentRecipe = null;
		currentQuantity = 0;
		RemainingMixTime = 0f;
		CancelInvoke(TickMix);
		if (IsOn())
		{
			SetFlag(Flags.On, false);
			SendNetworkUpdateImmediate();
		}
	}

	public void TickMix()
	{
		if (currentRecipe == null)
		{
			StopMixing();
			return;
		}
		if (base.isServer)
		{
			lastTickTimestamp = UnityEngine.Time.realtimeSinceStartup;
			RemainingMixTime -= 1f;
		}
		SendNetworkUpdateImmediate();
		if (RemainingMixTime <= 0f)
		{
			ProduceItem(currentRecipe, currentQuantity);
		}
	}

	public void ProduceItem(Recipe recipe, int quantity)
	{
		StopMixing();
		ConsumeInventory(recipe, quantity);
		CreateRecipeItems(recipe, quantity);
	}

	private void ConsumeInventory(Recipe recipe, int quantity)
	{
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			Item item = base.inventory.GetSlot(i);
			if (item != null)
			{
				if (GetItemWaterAmount(item) > 0)
				{
					item = item.contents.itemList[0];
				}
				int num = recipe.Ingredients[i].Count * quantity;
				if (num > 0)
				{
					item.UseItem(num);
				}
			}
		}
		ItemManager.DoRemoves();
	}

	private void ReturnExcessItems(List<Item> orderedContainerItems, BasePlayer player)
	{
		if (player == null || currentRecipe == null || orderedContainerItems == null || orderedContainerItems.Count != currentRecipe.Ingredients.Length)
		{
			return;
		}
		for (int i = 0; i < base.inventory.capacity; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot == null)
			{
				continue;
			}
			int num = slot.amount - currentRecipe.Ingredients[i].Count * currentQuantity;
			if (num > 0)
			{
				Item item = slot.SplitItem(num);
				if (!item.MoveToContainer(player.inventory.containerMain) && !item.MoveToContainer(player.inventory.containerBelt))
				{
					item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
				}
			}
		}
		ItemManager.DoRemoves();
	}

	protected virtual void CreateRecipeItems(Recipe recipe, int quantity)
	{
		if (recipe == null || recipe.ProducedItem == null)
		{
			return;
		}
		int num = quantity * recipe.ProducedItemCount;
		int stackable = recipe.ProducedItem.stackable;
		int num2 = Mathf.CeilToInt((float)num / (float)stackable);
		currentProductionItem = recipe.ProducedItem;
		for (int i = 0; i < num2; i++)
		{
			int num3 = ((num > stackable) ? stackable : num);
			Item item = ItemManager.Create(recipe.ProducedItem, num3, 0uL);
			if (!item.MoveToContainer(base.inventory))
			{
				item.Drop(base.inventory.dropPosition, base.inventory.dropVelocity);
			}
			num -= num3;
			if (num <= 0)
			{
				break;
			}
		}
		currentProductionItem = null;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mixingTable = Facepunch.Pool.Get<ProtoBuf.MixingTable>();
		if (info.forDisk)
		{
			info.msg.mixingTable.remainingMixTime = RemainingMixTime;
		}
		else
		{
			info.msg.mixingTable.remainingMixTime = RemainingMixTime - Mathf.Max(UnityEngine.Time.realtimeSinceStartup - lastTickTimestamp, 0f);
		}
		info.msg.mixingTable.totalMixTime = TotalMixTime;
	}

	private int GetItemWaterAmount(Item item)
	{
		if (item == null)
		{
			return 0;
		}
		if (item.contents != null && item.contents.capacity == 1 && item.contents.allowedContents == ItemContainer.ContentsType.Liquid && item.contents.itemList.Count > 0)
		{
			return item.contents.itemList[0].amount;
		}
		return 0;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.mixingTable != null)
		{
			RemainingMixTime = info.msg.mixingTable.remainingMixTime;
			TotalMixTime = info.msg.mixingTable.totalMixTime;
		}
	}

	public List<Item> GetOrderedContainerItems(ItemContainer container)
	{
		if (container == null)
		{
			return null;
		}
		if (container.itemList == null)
		{
			return null;
		}
		if (container.itemList.Count == 0)
		{
			return null;
		}
		inventoryItems.Clear();
		for (int i = 0; i < container.capacity; i++)
		{
			Item item = container.GetSlot(i);
			if (item == null)
			{
				break;
			}
			if (GetItemWaterAmount(item) > 0)
			{
				item = item.contents.itemList[0];
			}
			inventoryItems.Add(item);
		}
		return inventoryItems;
	}
}
