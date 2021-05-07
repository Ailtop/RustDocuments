using System;
using UnityEngine;

namespace Level.BlackMarket
{
	[Serializable]
	public class GlobalSettings
	{
		[SerializeField]
		private RarityPrices _collectorItemPrices;

		[SerializeField]
		private RarityPrices _masterDishPrices;

		[SerializeField]
		private RarityPrices _quintessenceMeisterPrices;

		public RarityPrices collectorItemPrices => _collectorItemPrices;

		public RarityPrices masterDishPrices => _masterDishPrices;

		public RarityPrices quintessenceMeisterPrices => _quintessenceMeisterPrices;
	}
}
