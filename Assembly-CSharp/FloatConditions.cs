using System;

[Serializable]
public class FloatConditions
{
	[Serializable]
	public struct Condition
	{
		public enum Types
		{
			Equal = 0,
			NotEqual = 1,
			Higher = 2,
			Lower = 3
		}

		public Types type;

		public float value;

		public bool Test(float val)
		{
			switch (type)
			{
			case Types.Equal:
				return val == value;
			case Types.NotEqual:
				return val != value;
			case Types.Higher:
				return val > value;
			case Types.Lower:
				return val < value;
			default:
				return false;
			}
		}
	}

	public Condition[] conditions;

	public bool AllTrue(float val)
	{
		Condition[] array = conditions;
		foreach (Condition condition in array)
		{
			if (!condition.Test(val))
			{
				return false;
			}
		}
		return true;
	}
}
