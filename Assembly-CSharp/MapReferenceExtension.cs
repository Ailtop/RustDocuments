public static class MapReferenceExtension
{
	public static bool IsNullOrEmpty(this Resource.MapReference mapReference)
	{
		return mapReference?.empty ?? true;
	}
}
