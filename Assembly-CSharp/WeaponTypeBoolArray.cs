using System;
using Characters.Gear.Weapons;

[Serializable]
public class WeaponTypeBoolArray : EnumArray<Weapon.Category, bool>
{
	public WeaponTypeBoolArray()
	{
	}

	public WeaponTypeBoolArray(params bool[] values)
	{
		int num = Math.Min(base.Array.Length, values.Length);
		for (int i = 0; i < num; i++)
		{
			base.Array[i] = values[i];
		}
	}
}
