using Oxide.Core;
using UnityEngine;

public class WaterPump : LiquidContainer
{
	public Transform WaterResourceLocation;

	public float PumpInterval = 20f;

	public int AmountPerPump = 30;

	public int PowerConsumption = 5;

	public override bool IsGravitySource => true;

	public override int ConsumptionAmount()
	{
		return PowerConsumption;
	}

	public void CreateWater()
	{
		if (!IsFull())
		{
			ItemDefinition atPoint = WaterResource.GetAtPoint(WaterResourceLocation.position);
			if (atPoint != null && Interface.CallHook("OnWaterCollect", this, atPoint) == null)
			{
				base.inventory.AddItem(atPoint, AmountPerPump, 0uL);
				UpdateOnFlag();
			}
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = next.HasFlag(Flags.Reserved8);
		if (!base.isServer || old.HasFlag(Flags.Reserved8) == flag)
		{
			return;
		}
		if (flag)
		{
			if (!IsInvoking(CreateWater))
			{
				InvokeRandomized(CreateWater, PumpInterval, PumpInterval, PumpInterval * 0.1f);
			}
		}
		else if (IsInvoking(CreateWater))
		{
			CancelInvoke(CreateWater);
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return Mathf.Clamp(GetLiquidCount(), 0, maxOutputFlow);
	}

	public bool IsFull()
	{
		if (base.inventory.itemList.Count == 0)
		{
			return false;
		}
		if (base.inventory.itemList[0].amount < base.inventory.maxStackSize)
		{
			return false;
		}
		return true;
	}
}
