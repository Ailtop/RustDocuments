using UnityEngine;

public class RANDSwitch : ElectricalBlocker
{
	private bool rand;

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return GetCurrentEnergy() * (IsOn() ? 1 : 0);
	}

	public override void UpdateBlocked()
	{
		bool num = IsOn();
		SetFlag(Flags.On, rand, false, false);
		SetFlag(Flags.Reserved8, rand, false, false);
		UpdateHasPower(input1Amount + input2Amount, 1);
		if (num != IsOn())
		{
			MarkDirty();
		}
	}

	public bool RandomRoll()
	{
		return Random.Range(0, 2) == 1;
	}

	public override void UpdateFromInput(int inputAmount, int inputSlot)
	{
		if (inputSlot == 1 && inputAmount > 0)
		{
			input1Amount = inputAmount;
			rand = RandomRoll();
			UpdateBlocked();
		}
		if (inputSlot == 2 && inputAmount > 0)
		{
			rand = false;
			UpdateBlocked();
		}
		else
		{
			base.UpdateFromInput(inputAmount, inputSlot);
		}
	}
}
