using System.Collections.Generic;

namespace Rust;

public class DamageTypeList
{
	public float[] types = new float[25];

	public void Set(DamageType index, float amount)
	{
		types[(int)index] = amount;
	}

	public float Get(DamageType index)
	{
		return types[(int)index];
	}

	public void Add(DamageType index, float amount)
	{
		Set(index, Get(index) + amount);
	}

	public void Scale(DamageType index, float amount)
	{
		Set(index, Get(index) * amount);
	}

	public bool Has(DamageType index)
	{
		return Get(index) > 0f;
	}

	public float Total()
	{
		float num = 0f;
		for (int i = 0; i < types.Length; i++)
		{
			float num2 = types[i];
			if (!float.IsNaN(num2) && !float.IsInfinity(num2))
			{
				num += num2;
			}
		}
		return num;
	}

	public void Clear()
	{
		for (int i = 0; i < types.Length; i++)
		{
			types[i] = 0f;
		}
	}

	public void Add(List<DamageTypeEntry> entries)
	{
		foreach (DamageTypeEntry entry in entries)
		{
			Add(entry.type, entry.amount);
		}
	}

	public void ScaleAll(float amount)
	{
		for (int i = 0; i < types.Length; i++)
		{
			Scale((DamageType)i, amount);
		}
	}

	public DamageType GetMajorityDamageType()
	{
		int result = 0;
		float num = 0f;
		for (int i = 0; i < types.Length; i++)
		{
			float num2 = types[i];
			if (!float.IsNaN(num2) && !float.IsInfinity(num2) && !(num2 < num))
			{
				result = i;
				num = num2;
			}
		}
		return (DamageType)result;
	}

	public bool IsMeleeType()
	{
		return DamageTypeEx.IsMeleeType(GetMajorityDamageType());
	}

	public bool IsBleedCausing()
	{
		return DamageTypeEx.IsBleedCausing(GetMajorityDamageType());
	}

	public bool IsConsideredAnAttack()
	{
		return DamageTypeEx.IsConsideredAnAttack(GetMajorityDamageType());
	}
}
