#define UNITY_ASSERTIONS
using System.Collections.Generic;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerLoot : EntityComponent<BasePlayer>
{
	public BaseEntity entitySource;

	public Item itemSource;

	public List<ItemContainer> containers = new List<ItemContainer>();

	public bool PositionChecks = true;

	private bool isInvokingSendUpdate;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PlayerLoot.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsLooting()
	{
		return containers.Count > 0;
	}

	public void Clear()
	{
		if (!IsLooting())
		{
			return;
		}
		Interface.CallHook("OnPlayerLootEnd", this);
		MarkDirty();
		if ((bool)entitySource)
		{
			entitySource.SendMessage("PlayerStoppedLooting", base.baseEntity, SendMessageOptions.DontRequireReceiver);
		}
		foreach (ItemContainer container in containers)
		{
			if (container != null)
			{
				container.onDirty -= MarkDirty;
			}
		}
		containers.Clear();
		entitySource = null;
		itemSource = null;
	}

	public ItemContainer FindContainer(uint id)
	{
		Check();
		if (!IsLooting())
		{
			return null;
		}
		foreach (ItemContainer container in containers)
		{
			ItemContainer itemContainer = container.FindContainer(id);
			if (itemContainer != null)
			{
				return itemContainer;
			}
		}
		return null;
	}

	public Item FindItem(uint id)
	{
		Check();
		if (!IsLooting())
		{
			return null;
		}
		foreach (ItemContainer container in containers)
		{
			Item item = container.FindItemByUID(id);
			if (item != null && item.IsValid())
			{
				return item;
			}
		}
		return null;
	}

	public void Check()
	{
		if (!IsLooting() || !base.baseEntity.isServer)
		{
			return;
		}
		if (entitySource == null)
		{
			base.baseEntity.ChatMessage("Stopping Looting because lootable doesn't exist!");
			Clear();
		}
		else if (!entitySource.CanBeLooted(base.baseEntity))
		{
			Clear();
		}
		else
		{
			if (!PositionChecks)
			{
				return;
			}
			float num = entitySource.Distance(base.baseEntity.eyes.position);
			if (num > 3f)
			{
				LootDistanceOverride component = entitySource.GetComponent<LootDistanceOverride>();
				if (component == null || num > component.amount)
				{
					Clear();
				}
			}
		}
	}

	public void MarkDirty()
	{
		if (!isInvokingSendUpdate)
		{
			isInvokingSendUpdate = true;
			Invoke(SendUpdate, 0.1f);
		}
	}

	public void SendImmediate()
	{
		if (isInvokingSendUpdate)
		{
			isInvokingSendUpdate = false;
			CancelInvoke(SendUpdate);
		}
		SendUpdate();
	}

	private void SendUpdate()
	{
		isInvokingSendUpdate = false;
		if (!BaseNetworkableEx.IsValid(base.baseEntity) || Interface.CallHook("OnLootNetworkUpdate", this) != null)
		{
			return;
		}
		using PlayerUpdateLoot playerUpdateLoot = Pool.Get<PlayerUpdateLoot>();
		if ((bool)entitySource && entitySource.net != null)
		{
			playerUpdateLoot.entityID = entitySource.net.ID;
		}
		if (itemSource != null)
		{
			playerUpdateLoot.itemID = itemSource.uid;
		}
		if (containers.Count > 0)
		{
			playerUpdateLoot.containers = Pool.Get<List<ProtoBuf.ItemContainer>>();
			foreach (ItemContainer container in containers)
			{
				playerUpdateLoot.containers.Add(container.Save());
			}
		}
		base.baseEntity.ClientRPCPlayer(null, base.baseEntity, "UpdateLoot", playerUpdateLoot);
	}

	public bool StartLootingEntity(BaseEntity targetEntity, bool doPositionChecks = true)
	{
		Clear();
		if (!targetEntity)
		{
			return false;
		}
		if (!targetEntity.OnStartBeingLooted(base.baseEntity))
		{
			return false;
		}
		Assert.IsTrue(targetEntity.isServer, "Assure is server");
		PositionChecks = doPositionChecks;
		entitySource = targetEntity;
		itemSource = null;
		Interface.CallHook("OnLootEntity", GetComponent<BasePlayer>(), targetEntity);
		MarkDirty();
		return true;
	}

	public void AddContainer(ItemContainer container)
	{
		if (container != null)
		{
			containers.Add(container);
			container.onDirty += MarkDirty;
		}
	}

	public void RemoveContainer(ItemContainer container)
	{
		if (container != null)
		{
			container.onDirty -= MarkDirty;
			containers.Remove(container);
		}
	}

	public bool RemoveContainerAt(int index)
	{
		if (index < 0 || index >= containers.Count)
		{
			return false;
		}
		if (containers[index] != null)
		{
			containers[index].onDirty -= MarkDirty;
		}
		containers.RemoveAt(index);
		return true;
	}

	public void StartLootingItem(Item item)
	{
		Clear();
		if (item != null && item.contents != null)
		{
			PositionChecks = true;
			containers.Add(item.contents);
			item.contents.onDirty += MarkDirty;
			itemSource = item;
			entitySource = item.GetWorldEntity();
			Interface.CallHook("OnLootItem", GetComponent<BasePlayer>(), item);
			MarkDirty();
		}
	}
}
