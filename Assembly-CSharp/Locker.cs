#define UNITY_ASSERTIONS
using System;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class Locker : StorageContainer
{
	private enum RowType
	{
		Clothing = 0,
		Belt = 1
	}

	public static class LockerFlags
	{
		public const Flags IsEquipping = Flags.Reserved1;
	}

	public GameObjectRef equipSound;

	private const int maxGearSets = 3;

	private const int attireSize = 8;

	private const int beltSize = 6;

	private const int columnSize = 2;

	private const int backpackSlotIndex = 7;

	private Item[] clothingBuffer = new Item[8];

	private const int setSize = 14;

	private bool isTransferringIndustrialItem;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Locker.OnRpcMessage"))
		{
			if (rpc == 1799659668 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log("SV_RPCMessage: " + player?.ToString() + " - RPC_Equip ");
				}
				using (TimeWarning.New("RPC_Equip"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1799659668u, "RPC_Equip", this, player, 3f))
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
							RPC_Equip(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_Equip");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsEquipping()
	{
		return HasFlag(Flags.Reserved1);
	}

	private RowType GetRowType(int slot)
	{
		if (slot == -1)
		{
			return RowType.Clothing;
		}
		if (slot % 14 >= 8)
		{
			return RowType.Belt;
		}
		return RowType.Clothing;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		SetFlag(Flags.Reserved1, b: false);
	}

	public void ClearEquipping()
	{
		SetFlag(Flags.Reserved1, b: false);
	}

	public void OnIndustrialItemTransferBegin()
	{
		isTransferringIndustrialItem = true;
	}

	public void OnIndustrialItemTransferEnd()
	{
		isTransferringIndustrialItem = false;
	}

	public override bool ItemFilter(Item item, int targetSlot)
	{
		if (!base.ItemFilter(item, targetSlot))
		{
			return false;
		}
		bool num = item.IsBackpack();
		bool flag = IsBackpackSlot(targetSlot);
		if (num != flag)
		{
			return false;
		}
		if (isTransferringIndustrialItem && GetRowType(targetSlot) == RowType.Belt && item.info.category == ItemCategory.Attire)
		{
			return false;
		}
		if (item.info.category == ItemCategory.Attire)
		{
			return true;
		}
		return GetRowType(targetSlot) == RowType.Belt;
	}

	private bool IsBackpackSlot(int slot)
	{
		return (slot - 7) % 14 == 0;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void RPC_Equip(RPCMessage msg)
	{
		int num = msg.read.Int32();
		if (num < 0 || num >= 3 || Interface.CallHook("OnLockerSwap", this, num, msg.player) != null || IsEquipping())
		{
			return;
		}
		BasePlayer player = msg.player;
		int num2 = num * 14;
		bool flag = false;
		for (int i = 0; i < clothingBuffer.Length; i++)
		{
			Item slot = player.inventory.containerWear.GetSlot(i);
			if (slot != null)
			{
				slot.RemoveFromContainer();
				clothingBuffer[i] = slot;
			}
		}
		for (int j = 0; j < 8; j++)
		{
			int num3 = num2 + j;
			Item slot2 = base.inventory.GetSlot(num3);
			Item item = clothingBuffer[j];
			if (slot2 != null)
			{
				flag = true;
				if (slot2.info.category != ItemCategory.Attire || !slot2.MoveToContainer(player.inventory.containerWear, j))
				{
					slot2.Drop(GetDropPosition(), GetDropVelocity());
				}
			}
			if (item != null)
			{
				flag = true;
				if (item.info.category != ItemCategory.Attire || !item.MoveToContainer(base.inventory, num3))
				{
					item.Drop(GetDropPosition(), GetDropVelocity());
				}
			}
			clothingBuffer[j] = null;
		}
		for (int k = 0; k < 6; k++)
		{
			int num4 = num2 + k + 8;
			int iTargetPos = k;
			Item slot3 = base.inventory.GetSlot(num4);
			Item slot4 = player.inventory.containerBelt.GetSlot(k);
			slot4?.RemoveFromContainer();
			if (slot3 != null)
			{
				flag = true;
				if (!slot3.MoveToContainer(player.inventory.containerBelt, iTargetPos))
				{
					slot3.Drop(GetDropPosition(), GetDropVelocity());
				}
			}
			if (slot4 != null)
			{
				flag = true;
				if (!slot4.MoveToContainer(base.inventory, num4))
				{
					slot4.Drop(GetDropPosition(), GetDropVelocity());
				}
			}
		}
		if (flag)
		{
			Effect.server.Run(equipSound.resourcePath, player, StringPool.Get("spine3"), Vector3.zero, Vector3.zero);
			SetFlag(Flags.Reserved1, b: true);
			Invoke(ClearEquipping, 1.5f);
		}
	}

	public override int GetIdealSlot(BasePlayer player, Item item)
	{
		for (int i = 0; i < inventorySlots; i++)
		{
			RowType rowType = GetRowType(i);
			if (item.info.category == ItemCategory.Attire)
			{
				if (rowType != 0)
				{
					continue;
				}
			}
			else if (rowType != RowType.Belt)
			{
				continue;
			}
			if (!base.inventory.SlotTaken(item, i) && (rowType != 0 || !DoesWearableConflictWithRow(item, i)))
			{
				return i;
			}
		}
		return int.MinValue;
	}

	private bool DoesWearableConflictWithRow(Item item, int pos)
	{
		int num = pos / 14 * 14;
		ItemModWearable itemModWearable = item.info.ItemModWearable;
		if (itemModWearable == null)
		{
			return false;
		}
		bool num2 = item.IsBackpack();
		bool flag = IsBackpackSlot(pos);
		if (num2 != flag)
		{
			return true;
		}
		for (int i = num; i < num + 8; i++)
		{
			Item slot = base.inventory.GetSlot(i);
			if (slot != null)
			{
				ItemModWearable itemModWearable2 = slot.info.ItemModWearable;
				if (!(itemModWearable2 == null) && !itemModWearable2.CanExistWith(itemModWearable))
				{
					return true;
				}
			}
		}
		return false;
	}

	public Vector2i GetIndustrialSlotRange(Vector3 localPosition)
	{
		if (localPosition.x < -0.3f)
		{
			return new Vector2i(28, 41);
		}
		if (localPosition.x > 0.3f)
		{
			return new Vector2i(0, 13);
		}
		return new Vector2i(14, 27);
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}

	public override bool CanPickup(BasePlayer player)
	{
		if (base.CanPickup(player))
		{
			return !HasAttachedStorageAdaptor();
		}
		return false;
	}
}
