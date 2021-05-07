using System;
using UnityEngine;

namespace Level
{
	[Serializable]
	public class CurrencyRangeByRarity
	{
		[SerializeField]
		private Vector2Int _commonRange;

		[SerializeField]
		private Vector2Int _rareRange;

		[SerializeField]
		private Vector2Int _uniqueRange;

		[SerializeField]
		private Vector2Int _legendaryRange;

		public int Evaluate(Rarity rarity)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected I4, but got Unknown
			switch ((int)rarity)
			{
			case 0:
				return Evaluate(_commonRange);
			case 1:
				return Evaluate(_rareRange);
			case 2:
				return Evaluate(_uniqueRange);
			case 3:
				return Evaluate(_legendaryRange);
			default:
				return Evaluate(_commonRange);
			}
		}

		private int Evaluate(Vector2Int range)
		{
			return UnityEngine.Random.Range(range.x, range.y);
		}
	}
}
