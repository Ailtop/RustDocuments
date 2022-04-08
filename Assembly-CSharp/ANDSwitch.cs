using UnityEngine;

public class ANDSwitch : IOEntity
{
	private int input1Amount;

	private int input2Amount;

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (input1Amount <= 0 || input2Amount <= 0)
		{
			return 0;
		}
		return Mathf.Max(input1Amount, input2Amount);
	}

	public override void UpdateHasPower(int inputAmount, int inputSlot)
	{
		SetFlag(Flags.Reserved8, input1Amount > 0 || input2Amount > 0, recursive: false, networkupdate: false);
	}

	public override void UpdateFromInput(int inputAmount, int slot)
	{
		switch (slot)
		{
		case 0:
			input1Amount = inputAmount;
			break;
		case 1:
			input2Amount = inputAmount;
			break;
		}
		int num = ((input1Amount > 0 && input2Amount > 0) ? (input1Amount + input2Amount) : 0);
		bool b = num > 0;
		SetFlag(Flags.Reserved1, input1Amount > 0, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved2, input2Amount > 0, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved3, b, recursive: false, networkupdate: false);
		SetFlag(Flags.Reserved4, input1Amount > 0 && input2Amount > 0, recursive: false, networkupdate: false);
		SetFlag(Flags.On, num > 0);
		base.UpdateFromInput(inputAmount, slot);
	}
}
