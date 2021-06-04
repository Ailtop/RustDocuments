public static class HitAreaUtil
{
	public static string Format(HitArea area)
	{
		switch (area)
		{
		case (HitArea)0:
			return "None";
		case (HitArea)(-1):
			return "Generic";
		default:
			return area.ToString();
		}
	}
}
