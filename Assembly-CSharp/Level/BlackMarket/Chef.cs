using Data;
using Level.Npc;
using Services;
using Singletons;
using TMPro;
using UnityEngine;

namespace Level.BlackMarket
{
	public class Chef : Npc
	{
		[SerializeField]
		private TMP_Text _priceDisplay;

		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private ChefsFoodList _foodList;

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private GameObject _talk;

		private ChefsFood _foodInstance;

		private int _price;

		public string submitLine => Lingua.GetLocalizedStringArray("npc/chef/submit/line").Random();

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.activateMaster)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		protected override void OnActivate()
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			SettingsByStage marketSettings = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings;
			GlobalSettings marketSettings2 = Settings.instance.marketSettings;
			Rarity val = marketSettings.masterDishPossibilities.Evaluate();
			ChefsFood chefsFood = _foodList.Take(val);
			float num = (float)marketSettings2.masterDishPrices.get_Item(val) * marketSettings.masterDishPriceMultiplier;
			num *= Random.Range(0.95f, 1.05f);
			_price = (int)(num / 10f) * 10;
			_foodInstance = Object.Instantiate(chefsFood, _slot);
			_foodInstance.name = chefsFood.name;
			_foodInstance.price = _price;
			_foodInstance.onSold += delegate
			{
				_price = 0;
				_priceDisplay.text = "---";
				_lineText.Run(submitLine);
			};
			_foodInstance.Initialize();
			_priceDisplay.text = _price.ToString();
			_lineText.gameObject.SetActive(true);
			_talk.SetActive(true);
		}

		protected override void OnDeactivate()
		{
			_lineText.gameObject.SetActive(false);
			_priceDisplay.text = string.Empty;
		}

		public void Update()
		{
			_priceDisplay.color = (GameData.Currency.gold.Has(_price) ? Color.white : Color.red);
		}
	}
}
