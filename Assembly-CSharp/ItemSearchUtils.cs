using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

public static class ItemSearchUtils
{
	public static IOrderedEnumerable<ItemDefinition> SearchForItems(string searchString, Func<ItemDefinition, bool> validFilter = null)
	{
		if (searchString == "")
		{
			searchString = "BALLS BALLS BALLS";
		}
		return from y in ItemManager.itemList.Where((ItemDefinition x) => IsValidSearchResult(searchString, x) && (validFilter == null || validFilter(x))).Take(60)
			orderby ScoreSearchResult(searchString, y)
			select y;
	}

	private static bool IsValidSearchResult(string search, ItemDefinition target)
	{
		if ((!(target.isRedirectOf != null) || target.redirectVendingBehaviour != ItemDefinition.RedirectVendingBehaviour.ListAsUniqueItem) && target.hidden)
		{
			return false;
		}
		if (!target.shortname.Contains(search, CompareOptions.IgnoreCase) && !target.displayName.translated.Contains(search, CompareOptions.IgnoreCase))
		{
			return target.displayDescription.translated.Contains(search, CompareOptions.IgnoreCase);
		}
		return true;
	}

	private static float ScoreSearchResult(string search, ItemDefinition target)
	{
		float num = 0f;
		if (target.shortname.Equals(search, StringComparison.CurrentCultureIgnoreCase) || target.displayName.translated.Equals(search, StringComparison.CurrentCultureIgnoreCase))
		{
			num -= (float)(500 - search.Length);
		}
		float a = (target.shortname.Contains(search, CompareOptions.IgnoreCase) ? ((float)search.Length / (float)target.shortname.Length) : 0f);
		float b = (target.displayName.translated.Contains(search, CompareOptions.IgnoreCase) ? ((float)search.Length / (float)target.displayName.translated.Length) : 0f);
		float num2 = Mathf.Max(a, b);
		num -= 50f * num2;
		if (target.displayDescription.translated.Contains(search, CompareOptions.IgnoreCase))
		{
			num -= (float)search.Length;
		}
		return num;
	}
}
