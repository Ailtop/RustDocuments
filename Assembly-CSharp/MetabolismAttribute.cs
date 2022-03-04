using System;
using UnityEngine;

[Serializable]
public class MetabolismAttribute
{
	public enum Type
	{
		Calories = 0,
		Hydration = 1,
		Heartrate = 2,
		Poison = 3,
		Radiation = 4,
		Bleeding = 5,
		Health = 6,
		HealthOverTime = 7
	}

	public float startMin;

	public float startMax;

	public float min;

	public float max;

	public float value;

	public float lastValue;

	internal float lastGreatFraction;

	private const float greatInterval = 0.1f;

	public float greatFraction => Mathf.Floor(Fraction() / 0.1f) / 10f;

	public void Reset()
	{
		value = Mathf.Clamp(UnityEngine.Random.Range(startMin, startMax), min, max);
	}

	public float Fraction()
	{
		return Mathf.InverseLerp(min, max, value);
	}

	public float InverseFraction()
	{
		return 1f - Fraction();
	}

	public void Add(float val)
	{
		value = Mathf.Clamp(value + val, min, max);
	}

	public void Subtract(float val)
	{
		value = Mathf.Clamp(value - val, min, max);
	}

	public void Increase(float fTarget)
	{
		fTarget = Mathf.Clamp(fTarget, min, max);
		if (!(fTarget <= value))
		{
			value = fTarget;
		}
	}

	public void MoveTowards(float fTarget, float fRate)
	{
		if (fRate != 0f)
		{
			value = Mathf.Clamp(Mathf.MoveTowards(value, fTarget, fRate), min, max);
		}
	}

	public bool HasChanged()
	{
		bool result = lastValue != value;
		lastValue = value;
		return result;
	}

	public bool HasGreatlyChanged()
	{
		float num = greatFraction;
		bool result = lastGreatFraction != num;
		lastGreatFraction = num;
		return result;
	}

	public void SetValue(float newValue)
	{
		value = newValue;
	}
}
