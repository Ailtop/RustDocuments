using UnityEngine;

namespace Characters.Gear.Synergy.Keywords
{
	public static class KeywordExtension
	{
		public static string GetName(this Keyword.Key key)
		{
			return Keyword.GetName(key);
		}

		public static Sprite GetIcon(this Keyword.Key key)
		{
			return Keyword.GetIcon(key);
		}
	}
}
