using System.Collections;
using Characters.Gear.Quintessences;
using Data;
using Level.Npc;
using Services;
using Singletons;
using TMPro;
using UnityEngine;

namespace Level.BlackMarket
{
	public class QuintessenceMaster : Npc
	{
		[SerializeField]
		private TMP_Text _priceDisplay;

		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private GameObject _talk;

		private int _price;

		public string submitLine => Lingua.GetLocalizedStringArray("npc/QuintessenceMeister/submit/line").Random();

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.activateQuintessenceMeister)
			{
				Activate();
			}
			else
			{
				Deactivate();
			}
		}

		private IEnumerator CDropGear()
		{
			_003C_003Ec__DisplayClass8_0 _003C_003Ec__DisplayClass8_ = new _003C_003Ec__DisplayClass8_0();
			_003C_003Ec__DisplayClass8_._003C_003E4__this = this;
			SettingsByStage settingsByStage = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings;
			GlobalSettings globalSetting = Settings.instance.marketSettings;
			Rarity rarity = settingsByStage.quintessenceMeisterPossibilities.Evaluate();
			Resource.QuintessenceInfo quintessenceToTake = Singleton<Service>.Instance.gearManager.GetQuintessenceToTake(rarity);
			Resource.Request<Quintessence> request = quintessenceToTake.LoadAsync();
			while (!request.isDone)
			{
				yield return null;
			}
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			_003C_003Ec__DisplayClass8_.droppedGear = levelManager.DropGear(request.asset, _slot.position);
			float num = (float)globalSetting.quintessenceMeisterPrices.get_Item(rarity) * settingsByStage.quintessenceMeisterPriceMultiplier;
			num *= Random.Range(0.95f, 1.05f);
			_price = (int)(num / 10f) * 10;
			_003C_003Ec__DisplayClass8_.droppedGear.dropped.price = _price;
			_003C_003Ec__DisplayClass8_.destructible = _003C_003Ec__DisplayClass8_.droppedGear.destructible;
			_003C_003Ec__DisplayClass8_.droppedGear.destructible = false;
			_003C_003Ec__DisplayClass8_.droppedGear.dropped.dropMovement.Stop();
			_priceDisplay.text = _price.ToString();
			_003C_003Ec__DisplayClass8_.droppedGear.dropped.onLoot += _003C_003Ec__DisplayClass8_._003CCDropGear_003Eg__OnLoot_007C0;
		}

		protected override void OnActivate()
		{
			_lineText.gameObject.SetActive(true);
			_talk.SetActive(true);
			StartCoroutine(CDropGear());
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
