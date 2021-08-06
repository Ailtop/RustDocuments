#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

public class LiquidContainer : ContainerIOEntity
{
	public ItemDefinition defaultLiquid;

	public int startingAmount;

	public bool autofillOutputs;

	public float autofillTickRate = 2f;

	public int autofillTickAmount = 2;

	public int maxOutputFlow = 6;

	public ItemDefinition[] ValidItems;

	private int currentDrainAmount;

	private HashSet<IOEntity> connectedList = new HashSet<IOEntity>();

	private HashSet<ContainerIOEntity> pushTargets = new HashSet<ContainerIOEntity>();

	private const int maxPushTargets = 3;

	private IOEntity considerConnectedTo;

	private Action updateDrainAmountAction;

	private Action updatePushLiquidTargetsAction;

	private Action pushLiquidAction;

	private Action deductFuelAction;

	private float lastOutputDrainUpdate;

	public override bool IsGravitySource => true;

	protected override bool DisregardGravityRestrictionsOnLiquid
	{
		get
		{
			if (!HasFlag(Flags.Reserved8))
			{
				return base.DisregardGravityRestrictionsOnLiquid;
			}
			return true;
		}
	}

	public override bool BlockFluidDraining => true;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("LiquidContainer.OnRpcMessage"))
		{
			if (rpc == 2002733690 && player != null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log(string.Concat("SV_RPCMessage: ", player, " - SVDrink "));
				}
				using (TimeWarning.New("SVDrink"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(2002733690u, "SVDrink", this, player, 3f))
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
							RPCMessage rpc2 = rPCMessage;
							SVDrink(rpc2);
						}
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						player.Kick("RPC Error in SVDrink");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override bool IsRootEntity()
	{
		return true;
	}

	private bool CanAcceptItem(Item item, int count)
	{
		if (ValidItems == null || ValidItems.Length == 0)
		{
			return true;
		}
		ItemDefinition[] validItems = ValidItems;
		for (int i = 0; i < validItems.Length; i++)
		{
			if (validItems[i] == item.info)
			{
				return true;
			}
		}
		return false;
	}

	public override void ServerInit()
	{
		updateDrainAmountAction = UpdateDrainAmount;
		pushLiquidAction = PushLiquidThroughOutputs;
		deductFuelAction = DeductFuel;
		updatePushLiquidTargetsAction = UpdatePushLiquidTargets;
		base.ServerInit();
		if (startingAmount > 0)
		{
			base.inventory.AddItem(defaultLiquid, startingAmount, 0uL);
		}
		if (autofillOutputs && HasLiquidItem())
		{
			UpdatePushLiquidTargets();
		}
		ItemContainer itemContainer = base.inventory;
		itemContainer.canAcceptItem = (Func<Item, int, bool>)Delegate.Combine(itemContainer.canAcceptItem, new Func<Item, int, bool>(CanAcceptItem));
	}

	public override void OnCircuitChanged(bool forceUpdate)
	{
		base.OnCircuitChanged(forceUpdate);
		ClearDrains();
		Invoke(updateDrainAmountAction, 0.1f);
		if (autofillOutputs && HasLiquidItem())
		{
			Invoke(updatePushLiquidTargetsAction, 0.1f);
		}
	}

	public override void OnItemAddedOrRemoved(Item item, bool added)
	{
		base.OnItemAddedOrRemoved(item, added);
		UpdateOnFlag();
		MarkDirtyForceUpdateOutputs();
		Invoke(updateDrainAmountAction, 0.1f);
		if (connectedList.Count > 0)
		{
			List<IOEntity> obj = Facepunch.Pool.GetList<IOEntity>();
			foreach (IOEntity connected in connectedList)
			{
				if (connected != null)
				{
					obj.Add(connected);
				}
			}
			foreach (IOEntity item2 in obj)
			{
				item2.SendChangedToRoot(true);
			}
			Facepunch.Pool.FreeList(ref obj);
		}
		if (HasLiquidItem() && autofillOutputs)
		{
			Invoke(updatePushLiquidTargetsAction, 0.1f);
		}
	}

	private void ClearDrains()
	{
		foreach (IOEntity connected in connectedList)
		{
			if (connected != null)
			{
				connected.SetFuelType(null, null);
			}
		}
		connectedList.Clear();
	}

	public override int GetCurrentEnergy()
	{
		return Mathf.Clamp(GetLiquidCount(), 0, maxOutputFlow);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		if (!HasLiquidItem())
		{
			return base.CalculateCurrentEnergy(inputAmount, inputSlot);
		}
		return GetCurrentEnergy();
	}

	private void UpdateDrainAmount()
	{
		int amount = 0;
		Item liquidItem = GetLiquidItem();
		if (liquidItem != null)
		{
			IOSlot[] array = outputs;
			foreach (IOSlot iOSlot in array)
			{
				if (iOSlot.connectedTo.Get() != null)
				{
					CalculateDrain(iOSlot.connectedTo.Get(), base.transform.TransformPoint(iOSlot.handlePosition), IOEntity.backtracking, ref amount, this, liquidItem?.info);
				}
			}
		}
		currentDrainAmount = Mathf.Clamp(amount, 0, maxOutputFlow);
		if (currentDrainAmount <= 0 && IsInvoking(deductFuelAction))
		{
			CancelInvoke(deductFuelAction);
		}
		else if (currentDrainAmount > 0 && !IsInvoking(deductFuelAction))
		{
			InvokeRepeating(deductFuelAction, 0f, 1f);
		}
	}

	private void CalculateDrain(IOEntity ent, Vector3 fromSlotWorld, int depth, ref int amount, IOEntity lastEntity, ItemDefinition waterType)
	{
		if (ent == this || depth <= 0 || ent == null || lastEntity == null || ent is LiquidContainer)
		{
			return;
		}
		if (!ent.BlockFluidDraining && ent.HasFlag(Flags.On))
		{
			int num = ent.DesiredPower();
			amount += num;
			ent.SetFuelType(waterType, this);
			connectedList.Add(ent);
		}
		if (!ent.AllowLiquidPassthrough(lastEntity, fromSlotWorld))
		{
			return;
		}
		IOSlot[] array = ent.outputs;
		foreach (IOSlot iOSlot in array)
		{
			if (iOSlot.connectedTo.Get() != null && iOSlot.connectedTo.Get() != ent)
			{
				CalculateDrain(iOSlot.connectedTo.Get(), ent.transform.TransformPoint(iOSlot.handlePosition), depth - 1, ref amount, ent, waterType);
			}
		}
	}

	public override void UpdateOutputs()
	{
		base.UpdateOutputs();
		if (!(UnityEngine.Time.realtimeSinceStartup - lastOutputDrainUpdate < 0.2f))
		{
			lastOutputDrainUpdate = UnityEngine.Time.realtimeSinceStartup;
			if (HasLiquidItem() && autofillOutputs)
			{
				UpdatePushLiquidTargets();
			}
			ClearDrains();
			Invoke(updateDrainAmountAction, 0.1f);
		}
	}

	private void DeductFuel()
	{
		if (HasLiquidItem())
		{
			Item liquidItem = GetLiquidItem();
			liquidItem.amount -= currentDrainAmount;
			liquidItem.MarkDirty();
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
		}
	}

	protected void UpdateOnFlag()
	{
		SetFlag(Flags.On, base.inventory.itemList.Count > 0 && base.inventory.itemList[0].amount > 0);
	}

	public virtual void OpenTap(float duration)
	{
		if (!HasFlag(Flags.Reserved5))
		{
			SetFlag(Flags.Reserved5, true);
			Invoke(ShutTap, duration);
			SendNetworkUpdateImmediate();
		}
	}

	public virtual void ShutTap()
	{
		SetFlag(Flags.Reserved5, false);
		SendNetworkUpdateImmediate();
	}

	public bool HasLiquidItem()
	{
		return GetLiquidItem() != null;
	}

	public Item GetLiquidItem()
	{
		if (base.inventory.itemList.Count == 0)
		{
			return null;
		}
		return base.inventory.itemList[0];
	}

	public int GetLiquidCount()
	{
		if (!HasLiquidItem())
		{
			return 0;
		}
		return GetLiquidItem().amount;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void SVDrink(RPCMessage rpc)
	{
		if (!rpc.player.metabolism.CanConsume())
		{
			return;
		}
		foreach (Item item in base.inventory.itemList)
		{
			ItemModConsume component = item.info.GetComponent<ItemModConsume>();
			if (!(component == null) && component.CanDoAction(item, rpc.player))
			{
				component.DoAction(item, rpc.player);
				break;
			}
		}
	}

	private void UpdatePushLiquidTargets()
	{
		pushTargets.Clear();
		if (!HasLiquidItem() || IsConnectedTo(this, IOEntity.backtracking * 2))
		{
			return;
		}
		Item liquidItem = GetLiquidItem();
		using (TimeWarning.New("UpdatePushTargets"))
		{
			IOSlot[] array = outputs;
			foreach (IOSlot iOSlot in array)
			{
				if (iOSlot.type == IOType.Fluidic)
				{
					IOEntity iOEntity = iOSlot.connectedTo.Get();
					if (iOEntity != null)
					{
						CheckPushLiquid(iOEntity, liquidItem, this, IOEntity.backtracking * 4);
					}
				}
			}
		}
		if (pushTargets.Count > 0)
		{
			InvokeRandomized(pushLiquidAction, 0f, autofillTickRate, autofillTickRate * 0.2f);
		}
	}

	private void PushLiquidThroughOutputs()
	{
		if (!HasLiquidItem())
		{
			CancelInvoke(pushLiquidAction);
			return;
		}
		Item liquidItem = GetLiquidItem();
		if (pushTargets.Count > 0)
		{
			int num = Mathf.Clamp(autofillTickAmount, 0, liquidItem.amount) / pushTargets.Count;
			if (num == 0 && liquidItem.amount > 0)
			{
				num = liquidItem.amount;
			}
			foreach (ContainerIOEntity pushTarget in pushTargets)
			{
				if (pushTarget.inventory.CanAcceptItem(liquidItem, 0) == ItemContainer.CanAcceptResult.CanAccept && (pushTarget.inventory.CanAccept(liquidItem) || pushTarget.inventory.FindItemByItemID(liquidItem.info.itemid) != null))
				{
					int num2 = Mathf.Clamp(num, 0, pushTarget.inventory.GetMaxTransferAmount(liquidItem.info));
					pushTarget.inventory.AddItem(liquidItem.info, num2, 0uL);
					liquidItem.amount -= num2;
					liquidItem.MarkDirty();
					if (liquidItem.amount <= 0)
					{
						break;
					}
				}
			}
		}
		if (liquidItem.amount <= 0 || pushTargets.Count == 0)
		{
			if (liquidItem.amount <= 0)
			{
				liquidItem.Remove();
			}
			CancelInvoke(pushLiquidAction);
		}
	}

	private void CheckPushLiquid(IOEntity connected, Item ourFuel, IOEntity fromSource, int depth)
	{
		if (depth <= 0 || ourFuel.amount <= 0)
		{
			return;
		}
		Vector3 worldHandlePosition = Vector3.zero;
		IOEntity iOEntity = connected.FindGravitySource(ref worldHandlePosition, IOEntity.backtracking, true);
		if ((iOEntity != null && !connected.AllowLiquidPassthrough(iOEntity, worldHandlePosition)) || connected == this || ConsiderConnectedTo(connected))
		{
			return;
		}
		ContainerIOEntity containerIOEntity;
		if ((object)(containerIOEntity = connected as ContainerIOEntity) != null && !pushTargets.Contains(containerIOEntity) && containerIOEntity.inventory.CanAcceptItem(ourFuel, 0) == ItemContainer.CanAcceptResult.CanAccept)
		{
			pushTargets.Add(containerIOEntity);
			return;
		}
		IOSlot[] array = connected.outputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity2 = iOSlot.connectedTo.Get();
			Vector3 sourceWorldPosition = connected.transform.TransformPoint(iOSlot.handlePosition);
			if (iOEntity2 != null && iOEntity2 != fromSource && iOEntity2.AllowLiquidPassthrough(connected, sourceWorldPosition))
			{
				CheckPushLiquid(iOEntity2, ourFuel, fromSource, depth - 1);
				if (pushTargets.Count >= 3)
				{
					break;
				}
			}
		}
	}

	public void SetConnectedTo(IOEntity entity)
	{
		considerConnectedTo = entity;
	}

	protected override bool ConsiderConnectedTo(IOEntity entity)
	{
		return entity == considerConnectedTo;
	}
}
