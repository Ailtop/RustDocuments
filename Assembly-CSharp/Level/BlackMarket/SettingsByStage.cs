using System;
using UnityEngine;

namespace Level.BlackMarket
{
	[Serializable]
	public class SettingsByStage
	{
		[SerializeField]
		[Range(0f, 100f)]
		private int _collectorPossibility;

		[SerializeField]
		[Range(0f, 100f)]
		private int _masterPossibility;

		[SerializeField]
		[Range(0f, 100f)]
		private int _headlessPossibility;

		[SerializeField]
		[Range(0f, 100f)]
		private int _quintessenceMeisterPossibility;

		[SerializeField]
		[Range(0f, 100f)]
		private int _tombRaiderPossibility;

		[Header("Collector")]
		[SerializeField]
		private RarityPossibilities _collectorItemPossibilities;

		[SerializeField]
		private float _collectorItemPriceMultiplier = 1f;

		[Header("Master (Chef)")]
		[SerializeField]
		private RarityPossibilities _masterDishPossibilities;

		[SerializeField]
		private float _masterDishPriceMultiplier = 1f;

		[Header("Headless")]
		[SerializeField]
		private RarityPossibilities _headlessHeadPossibilities;

		[Header("Essence Meister")]
		[SerializeField]
		private RarityPossibilities _quintessenceMeisterPossibilities;

		[SerializeField]
		private float _quintessenceMeisterPriceMultiplier = 1f;

		[Header("Tomb Raider")]
		[SerializeField]
		private RarityPossibilities _tombRaiderGearPossibilities;

		[SerializeField]
		private RarityPrices _tombRaiderUnlockPrices;

		public bool activateCollector => MMMaths.PercentChance(_collectorPossibility);

		public bool activateMaster => MMMaths.PercentChance(_masterPossibility);

		public bool activateHeadless => MMMaths.PercentChance(_headlessPossibility);

		public bool activateQuintessenceMeister => MMMaths.PercentChance(_quintessenceMeisterPossibility);

		public bool activateTombRaider => MMMaths.PercentChance(_tombRaiderPossibility);

		public RarityPossibilities collectorItemPossibilities => _collectorItemPossibilities;

		public float collectorItemPriceMultiplier => _collectorItemPriceMultiplier;

		public RarityPossibilities masterDishPossibilities => _masterDishPossibilities;

		public float masterDishPriceMultiplier => _masterDishPriceMultiplier;

		public RarityPossibilities headlessHeadPossibilities => _headlessHeadPossibilities;

		public RarityPossibilities quintessenceMeisterPossibilities => _quintessenceMeisterPossibilities;

		public float quintessenceMeisterPriceMultiplier => _quintessenceMeisterPriceMultiplier;

		public RarityPossibilities tombRaiderGearPossibilities => _tombRaiderGearPossibilities;

		public RarityPrices tombRaiderUnlockPrices => _tombRaiderUnlockPrices;
	}
}
