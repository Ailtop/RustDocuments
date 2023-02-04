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
using UnityEngine.UI;

public class IndustrialConveyor : IndustrialEntity
{
	public struct ItemFilter
	{
		public ItemDefinition TargetItem;

		public ItemCategory? TargetCategory;

		public int MaxAmountInOutput;

		public int MinAmountToTransfer;

		public bool IsBlueprint;

		public int MinTransferRemaining;

		public void CopyTo(ProtoBuf.IndustrialConveyor.ItemFilter target)
		{
			if (TargetItem != null)
			{
				target.itemDef = TargetItem.itemid;
			}
			target.maxAmountInDestination = MaxAmountInOutput;
			if (TargetCategory.HasValue)
			{
				target.itemCategory = (int)TargetCategory.Value;
			}
			else
			{
				target.itemCategory = -1;
			}
			target.isBlueprint = (IsBlueprint ? 1 : 0);
			target.minAmountForMove = MinAmountToTransfer;
		}

		public ItemFilter(ProtoBuf.IndustrialConveyor.ItemFilter from)
		{
			this = new ItemFilter
			{
				TargetItem = ItemManager.FindItemDefinition(from.itemDef),
				MaxAmountInOutput = from.maxAmountInDestination
			};
			if (from.itemCategory >= 0)
			{
				TargetCategory = (ItemCategory)from.itemCategory;
			}
			else
			{
				TargetCategory = null;
			}
			IsBlueprint = from.isBlueprint == 1;
			MinAmountToTransfer = from.minAmountForMove;
		}
	}

	public int MaxStackSizePerMove = 128;

	public GameObjectRef FilterDialog;

	public List<ItemFilter> filterItems = new List<ItemFilter>();

	private const float ScreenUpdateRange = 30f;

	public const Flags FilterPassFlag = Flags.Reserved9;

	public const Flags FilterFailFlag = Flags.Reserved10;

	public SoundDefinition transferItemSoundDef;

	public SoundDefinition transferItemStartSoundDef;

	public const int MAX_FILTER_SIZE = 12;

	public Image IconTransferImage;

	private IIndustrialStorage workerOutput;

	private Func<IIndustrialStorage, int, bool> filterFunc;

	private List<ContainerInputOutput> splitOutputs = new List<ContainerInputOutput>();

	private List<ContainerInputOutput> splitInputs = new List<ContainerInputOutput>();

	private bool? lastFilterState;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("IndustrialConveyor.OnRpcMessage"))
		{
			if (rpc == 617569194 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - RPC_ChangeFilters "));
				}
				using (TimeWarning.New("RPC_ChangeFilters"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(617569194u, "RPC_ChangeFilters", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(617569194u, "RPC_ChangeFilters", this, player, 3f))
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
							RPC_ChangeFilters(msg2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in RPC_ChangeFilters");
					}
				}
				return true;
			}
			if (rpc == 4167839872u && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SvSwitch "));
				}
				using (TimeWarning.New("SvSwitch"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(4167839872u, "SvSwitch", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(4167839872u, "SvSwitch", this, player, 3f))
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
							SvSwitch(msg3);
						}
					}
					catch (Exception exception2)
					{
						Debug.LogException(exception2);
						player.Kick("RPC Error in SvSwitch");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = next.HasFlag(Flags.On);
		if (old.HasFlag(Flags.On) != flag && base.isServer)
		{
			float conveyorMoveFrequency = ConVar.Server.conveyorMoveFrequency;
			if (flag && conveyorMoveFrequency > 0f)
			{
				InvokeRandomized(ScheduleMove, conveyorMoveFrequency, conveyorMoveFrequency, conveyorMoveFrequency * 0.5f);
			}
			else
			{
				CancelInvoke(ScheduleMove);
			}
		}
	}

	private void ScheduleMove()
	{
		IndustrialEntity.Queue.Add(this);
	}

	private Item GetItemToMove(IIndustrialStorage storage, out ItemFilter associatedFilter, int slot, ItemContainer targetContainer = null)
	{
		associatedFilter = default(ItemFilter);
		(ItemFilter, int) tuple = default((ItemFilter, int));
		if (storage == null || storage.Container == null)
		{
			return null;
		}
		if (storage.Container.IsEmpty())
		{
			return null;
		}
		Vector2i vector2i = storage.OutputSlotRange(slot);
		for (int i = vector2i.x; i <= vector2i.y; i++)
		{
			Item slot2 = storage.Container.GetSlot(i);
			tuple = default((ItemFilter, int));
			if (slot2 != null && (filterItems.Count == 0 || FilterHasItem(slot2, out tuple)))
			{
				(associatedFilter, _) = tuple;
				if (targetContainer == null || !(associatedFilter.TargetItem != null) || associatedFilter.MaxAmountInOutput <= 0 || targetContainer.GetTotalItemAmount(associatedFilter.TargetItem, vector2i.x, vector2i.y) < associatedFilter.MaxAmountInOutput)
				{
					return slot2;
				}
			}
		}
		return null;
	}

	private bool FilterHasItem(Item item, out (ItemFilter filter, int index) filter)
	{
		filter = default((ItemFilter, int));
		for (int i = 0; i < filterItems.Count; i++)
		{
			ItemFilter itemFilter = filterItems[i];
			if (FilterMatches(itemFilter, item))
			{
				filter = (itemFilter, i);
				return true;
			}
		}
		return false;
	}

	private bool FilterMatches(ItemFilter filter, Item item)
	{
		if (item.IsBlueprint() && filter.IsBlueprint && item.blueprintTargetDef == filter.TargetItem)
		{
			return true;
		}
		if (filter.TargetItem == item.info && !filter.IsBlueprint)
		{
			return true;
		}
		if (filter.TargetItem != null && item.info.isRedirectOf == filter.TargetItem)
		{
			return true;
		}
		if (filter.TargetCategory.HasValue && item.info.category == filter.TargetCategory)
		{
			return true;
		}
		return false;
	}

	private bool FilterContainerInput(IIndustrialStorage storage, int slot)
	{
		ItemFilter associatedFilter;
		return GetItemToMove(storage, out associatedFilter, slot, workerOutput?.Container) != null;
	}

	protected override void RunJob()
	{
		base.RunJob();
		if (ConVar.Server.conveyorMoveFrequency <= 0f)
		{
			return;
		}
		if (filterFunc == null)
		{
			filterFunc = FilterContainerInput;
		}
		splitInputs.Clear();
		FindContainerSource(splitInputs, IOEntity.backtracking * 2, input: true, null);
		bool flag = CheckIfAnyInputPassesFilters(splitInputs);
		if (!lastFilterState.HasValue || flag != lastFilterState)
		{
			lastFilterState = flag;
			SetFlag(Flags.Reserved9, flag, recursive: false, networkupdate: false);
			SetFlag(Flags.Reserved10, !flag);
			ensureOutputsUpdated = true;
			MarkDirty();
		}
		if (!flag)
		{
			return;
		}
		IndustrialConveyorTransfer transfer = Facepunch.Pool.Get<IndustrialConveyorTransfer>();
		try
		{
			transfer.ItemTransfers = Facepunch.Pool.GetList<IndustrialConveyorTransfer.ItemTransfer>();
			transfer.inputEntities = Facepunch.Pool.GetList<uint>();
			transfer.outputEntities = Facepunch.Pool.GetList<uint>();
			splitOutputs.Clear();
			FindContainerSource(splitOutputs, IOEntity.backtracking * 2, input: false, null);
			foreach (ContainerInputOutput splitOutput in splitOutputs)
			{
				workerOutput = splitOutput.Storage;
				foreach (ContainerInputOutput splitInput in splitInputs)
				{
					int num = 0;
					IIndustrialStorage storage = splitInput.Storage;
					if (storage == null || splitOutput.Storage == null || splitInput.Storage.IndustrialEntity == splitOutput.Storage.IndustrialEntity)
					{
						continue;
					}
					ItemContainer container = storage.Container;
					ItemContainer container2 = splitOutput.Storage.Container;
					if (container == null || container2 == null || storage.Container == null || storage.Container.IsEmpty())
					{
						continue;
					}
					(ItemFilter, int) filter = default((ItemFilter, int));
					Vector2i vector2i = storage.OutputSlotRange(splitInput.SlotIndex);
					for (int i = vector2i.x; i <= vector2i.y; i++)
					{
						Vector2i vector2i2 = splitOutput.Storage.InputSlotRange(splitOutput.SlotIndex);
						Item slot = storage.Container.GetSlot(i);
						if (slot == null || (filterItems.Count != 0 && !FilterHasItem(slot, out filter)) || (filter.Item1.TargetItem != null && filter.Item1.MaxAmountInOutput > 0 && splitOutput.Storage.Container.GetTotalItemAmount(filter.Item1.TargetItem, vector2i2.x, vector2i2.y) >= filter.Item1.MaxAmountInOutput))
						{
							continue;
						}
						int num2 = Mathf.Min(MaxStackSizePerMove, slot.info.stackable) / splitOutputs.Count;
						if (slot.amount == 1 || (num2 <= 0 && slot.amount > 0))
						{
							num2 = 1;
						}
						if (filter.Item1.MinAmountToTransfer > 0)
						{
							num2 = Mathf.Min(num2, filter.Item1.MinTransferRemaining);
						}
						if (filter.Item1.MaxAmountInOutput > 0)
						{
							if (filter.Item1.TargetItem == slot.info && filter.Item1.TargetItem != null)
							{
								num2 = Mathf.Min(num2, filter.Item1.MaxAmountInOutput - container2.GetTotalItemAmount(slot.info, vector2i2.x, vector2i2.y));
							}
							else if (filter.Item1.TargetCategory.HasValue)
							{
								num2 = Mathf.Min(num2, filter.Item1.MaxAmountInOutput - container2.GetTotalCategoryAmount(filter.Item1.TargetCategory.Value, vector2i2.x, vector2i2.y));
							}
						}
						if (num2 <= 0)
						{
							continue;
						}
						Item item = null;
						int amount2 = slot.amount;
						if (slot.amount > num2)
						{
							item = slot.SplitItem(num2);
							amount2 = item.amount;
						}
						splitOutput.Storage.OnStorageItemTransferBegin();
						bool flag2 = false;
						for (int j = vector2i2.x; j <= vector2i2.y; j++)
						{
							Item slot2 = container2.GetSlot(j);
							if (slot2 != null && !(slot2.info == slot.info))
							{
								continue;
							}
							if (item != null)
							{
								if (item.MoveToContainer(container2, j, allowStack: true, ignoreStackLimit: false, null, allowSwap: false))
								{
									flag2 = true;
									break;
								}
							}
							else if (slot.MoveToContainer(container2, j, allowStack: true, ignoreStackLimit: false, null, allowSwap: false))
							{
								flag2 = true;
								break;
							}
						}
						if (filter.Item1.MinTransferRemaining > 0)
						{
							var (value, _) = filter;
							value.MinTransferRemaining -= amount2;
							filterItems[filter.Item2] = value;
						}
						if (!flag2 && item != null)
						{
							slot.amount += item.amount;
							slot.MarkDirty();
							item.Remove();
							item = null;
						}
						if (flag2)
						{
							num++;
							if (item != null)
							{
								AddTransfer(item.info.itemid, amount2, splitInput.Storage.IndustrialEntity, splitOutput.Storage.IndustrialEntity);
							}
							else
							{
								AddTransfer(slot.info.itemid, amount2, splitInput.Storage.IndustrialEntity, splitOutput.Storage.IndustrialEntity);
							}
						}
						splitOutput.Storage.OnStorageItemTransferEnd();
						if (num >= ConVar.Server.maxItemStacksMovedPerTickIndustrial)
						{
							break;
						}
					}
				}
			}
			if (transfer.ItemTransfers.Count > 0)
			{
				ClientRPCEx(new SendInfo(BaseNetworkable.GetConnectionsWithin(base.transform.position, 30f)), null, "ReceiveItemTransferDetails", transfer);
			}
		}
		finally
		{
			if (transfer != null)
			{
				((IDisposable)transfer).Dispose();
			}
		}
		void AddTransfer(int itemId, int amount, BaseEntity fromEntity, BaseEntity toEntity)
		{
			if (transfer != null && transfer.ItemTransfers != null)
			{
				if (fromEntity != null && !transfer.inputEntities.Contains(fromEntity.net.ID))
				{
					transfer.inputEntities.Add(fromEntity.net.ID);
				}
				if (toEntity != null && !transfer.outputEntities.Contains(toEntity.net.ID))
				{
					transfer.outputEntities.Add(toEntity.net.ID);
				}
				for (int k = 0; k < transfer.ItemTransfers.Count; k++)
				{
					IndustrialConveyorTransfer.ItemTransfer value2 = transfer.ItemTransfers[k];
					if (value2.itemId == itemId)
					{
						value2.amount += amount;
						transfer.ItemTransfers[k] = value2;
						return;
					}
				}
				IndustrialConveyorTransfer.ItemTransfer itemTransfer = default(IndustrialConveyorTransfer.ItemTransfer);
				itemTransfer.itemId = itemId;
				itemTransfer.amount = amount;
				IndustrialConveyorTransfer.ItemTransfer item2 = itemTransfer;
				transfer.ItemTransfers.Add(item2);
			}
		}
	}

	private bool CheckIfAnyInputPassesFilters(List<ContainerInputOutput> inputs)
	{
		if (filterItems.Count == 0)
		{
			foreach (ContainerInputOutput input in inputs)
			{
				if (GetItemToMove(input.Storage, out var _, input.SlotIndex) != null)
				{
					return true;
				}
			}
		}
		else
		{
			for (int i = 0; i < filterItems.Count; i++)
			{
				ItemFilter itemFilter = filterItems[i];
				int num = 0;
				foreach (ContainerInputOutput input2 in inputs)
				{
					Vector2i vector2i = input2.Storage.OutputSlotRange(input2.SlotIndex);
					for (int j = vector2i.x; j <= vector2i.y; j++)
					{
						Item slot = input2.Storage.Container.GetSlot(j);
						if (slot != null && FilterMatches(itemFilter, slot))
						{
							if (itemFilter.MinAmountToTransfer <= 0)
							{
								return true;
							}
							if (itemFilter.MinTransferRemaining > 0)
							{
								return true;
							}
							num += slot.amount;
							if (num >= itemFilter.MinAmountToTransfer)
							{
								itemFilter.MinTransferRemaining = itemFilter.MinAmountToTransfer;
								filterItems[i] = itemFilter;
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (filterItems.Count == 0)
		{
			return;
		}
		info.msg.industrialConveyor = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor>();
		info.msg.industrialConveyor.filters = Facepunch.Pool.GetList<ProtoBuf.IndustrialConveyor.ItemFilter>();
		foreach (ItemFilter filterItem in filterItems)
		{
			ProtoBuf.IndustrialConveyor.ItemFilter itemFilter = Facepunch.Pool.Get<ProtoBuf.IndustrialConveyor.ItemFilter>();
			filterItem.CopyTo(itemFilter);
			info.msg.industrialConveyor.filters.Add(itemFilter);
		}
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(1uL)]
	private void RPC_ChangeFilters(RPCMessage msg)
	{
		if (msg.player == null || !msg.player.CanBuild())
		{
			return;
		}
		filterItems.Clear();
		ProtoBuf.IndustrialConveyor.ItemFilterList itemFilterList = ProtoBuf.IndustrialConveyor.ItemFilterList.Deserialize(msg.read);
		if (itemFilterList.filters == null)
		{
			return;
		}
		int num = Mathf.Min(itemFilterList.filters.Count, 24);
		for (int i = 0; i < num; i++)
		{
			if (filterItems.Count >= 12)
			{
				break;
			}
			ItemFilter item = new ItemFilter(itemFilterList.filters[i]);
			if (item.TargetItem != null || item.TargetCategory.HasValue)
			{
				filterItems.Add(item);
			}
		}
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(2uL)]
	private void SvSwitch(RPCMessage msg)
	{
		if (Interface.CallHook("OnSwitchToggle", this, msg.player) == null)
		{
			SetSwitch(!IsOn());
			Interface.CallHook("OnSwitchToggled", this, msg.player);
		}
	}

	public virtual void SetSwitch(bool wantsOn)
	{
		if (wantsOn == IsOn())
		{
			return;
		}
		SetFlag(Flags.On, wantsOn);
		SetFlag(Flags.Busy, b: true);
		SetFlag(Flags.Reserved10, b: false);
		SetFlag(Flags.Reserved9, b: false);
		if (!wantsOn)
		{
			lastFilterState = null;
		}
		ensureOutputsUpdated = true;
		Invoke(Unbusy, 0.5f);
		for (int i = 0; i < filterItems.Count; i++)
		{
			ItemFilter value = filterItems[i];
			if (value.MinTransferRemaining > 0)
			{
				value.MinTransferRemaining = 0;
				filterItems[i] = value;
			}
		}
		SendNetworkUpdateImmediate();
		MarkDirty();
	}

	public void Unbusy()
	{
		SetFlag(Flags.Busy, b: false);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
	}

	public override void IOStateChanged(int inputAmount, int inputSlot)
	{
		base.IOStateChanged(inputAmount, inputSlot);
		if (inputSlot == 1)
		{
			bool flag = inputAmount >= ConsumptionAmount() && inputAmount > 0;
			SetFlag(Flags.Reserved8, flag);
			if (!flag)
			{
				SetFlag(Flags.Reserved9, b: false);
				SetFlag(Flags.Reserved10, b: false);
			}
			currentEnergy = inputAmount;
			ensureOutputsUpdated = true;
			MarkDirty();
			if (inputAmount <= 0 && IsOn())
			{
				SetSwitch(wantsOn: false);
			}
		}
		if (inputSlot == 2 && !IsOn() && inputAmount > 0 && IsPowered())
		{
			SetSwitch(wantsOn: true);
		}
		if (inputSlot == 3 && IsOn() && inputAmount > 0)
		{
			SetSwitch(wantsOn: false);
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		switch (outputSlot)
		{
		case 2:
			if (!HasFlag(Flags.Reserved10))
			{
				return 0;
			}
			return 1;
		case 3:
			if (!HasFlag(Flags.Reserved9))
			{
				return 0;
			}
			return 1;
		case 1:
			return GetCurrentEnergy();
		default:
			return 0;
		}
	}

	public override bool ShouldDrainBattery(IOEntity battery)
	{
		return IsOn();
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		filterItems.Clear();
		if (info.msg.industrialConveyor?.filters == null)
		{
			return;
		}
		foreach (ProtoBuf.IndustrialConveyor.ItemFilter filter in info.msg.industrialConveyor.filters)
		{
			ItemFilter item = new ItemFilter(filter);
			if (item.TargetItem != null || item.TargetCategory.HasValue)
			{
				filterItems.Add(item);
			}
		}
	}
}
