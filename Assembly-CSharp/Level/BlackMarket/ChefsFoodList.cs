using System.Linq;
using UnityEngine;

namespace Level.BlackMarket
{
	public class ChefsFoodList : ScriptableObject
	{
		private EnumArray<Rarity, ChefsFood[]> _dishesByRarity;

		[SerializeField]
		private ChefsFood[] _foods;

		public ChefsFood Take(Rarity rarity)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			_dishesByRarity = new EnumArray<Rarity, ChefsFood[]>();
			foreach (IGrouping<Rarity, ChefsFood> item in from d in _foods
				group d by d.rarity)
			{
				_dishesByRarity[item.Key] = item.ToArray();
			}
			return _dishesByRarity[rarity].Random();
		}
	}
}
