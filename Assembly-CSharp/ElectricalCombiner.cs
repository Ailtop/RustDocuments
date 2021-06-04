using UnityEngine;

public class ElectricalCombiner : IOEntity
{
	public int input1Amount;

	public int input2Amount;

	public int input3Amount;

	public override bool BlockFluidDraining => true;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		int num = input1Amount + input2Amount + input3Amount;
		Mathf.Clamp(num - 1, 0, num);
		return num;
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		SetFlag(Flags.Reserved8, input1Amount > 0 || input2Amount > 0, false, false);
	}

	public override void UpdateFromInput(int inputAmount, int slot)
	{
		if (inputAmount > 0 && IsConnectedTo(this, slot, IOEntity.backtracking * 2, true))
		{
			inputAmount = 0;
			SetFlag(Flags.Reserved7, true);
		}
		else
		{
			SetFlag(Flags.Reserved7, false);
		}
		switch (slot)
		{
		case 0:
			input1Amount = inputAmount;
			break;
		case 1:
			input2Amount = inputAmount;
			break;
		case 2:
			input3Amount = inputAmount;
			break;
		}
		int num = input1Amount + input2Amount + input3Amount;
		bool b = num > 0;
		SetFlag(Flags.Reserved1, input1Amount > 0, false, false);
		SetFlag(Flags.Reserved2, input2Amount > 0, false, false);
		SetFlag(Flags.Reserved3, b, false, false);
		SetFlag(Flags.Reserved4, input1Amount > 0 || input2Amount > 0 || input3Amount > 0, false, false);
		SetFlag(Flags.On, num > 0);
		base.UpdateFromInput(num, slot);
	}
}
