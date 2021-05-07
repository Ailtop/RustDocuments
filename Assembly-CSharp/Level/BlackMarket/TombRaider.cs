using System.Collections;
using Characters.Gear;
using Data;
using Level.Npc;
using Services;
using Singletons;
using TMPro;
using UnityEngine;

namespace Level.BlackMarket
{
	public class TombRaider : Npc
	{
		[SerializeField]
		private TMP_Text _priceDisplay;

		[SerializeField]
		private Transform _slot;

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private GameObject _talk;

		private int _unlockPrice;

		private Resource.GearReference _gearToUnlock;

		public string submitLine => Lingua.GetLocalizedStringArray("npc/TombRaider/submit/line").Random();

		private void Start()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			SettingsByStage marketSettings = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings;
			Rarity rarity = marketSettings.tombRaiderGearPossibilities.Evaluate();
			_gearToUnlock = Singleton<Service>.Instance.gearManager.GetGearToUnlock(rarity);
			if (_gearToUnlock == null || !marketSettings.activateTombRaider)
			{
				Deactivate();
				return;
			}
			_unlockPrice = marketSettings.tombRaiderUnlockPrices.get_Item(_gearToUnlock.rarity);
			Activate();
		}

		private IEnumerator CDropGear()
		{
			_003C_003Ec__DisplayClass9_0 _003C_003Ec__DisplayClass9_ = new _003C_003Ec__DisplayClass9_0();
			_003C_003Ec__DisplayClass9_._003C_003E4__this = this;
			Resource.Request<Gear> request = _gearToUnlock.LoadAsync();
			while (!request.isDone)
			{
				yield return null;
			}
			LevelManager levelManager = Singleton<Service>.Instance.levelManager;
			_003C_003Ec__DisplayClass9_.droppedGear = levelManager.DropGear(request.asset, _slot.position);
			_003C_003Ec__DisplayClass9_.droppedGear.dropped.price = _unlockPrice;
			_003C_003Ec__DisplayClass9_.droppedGear.dropped.priceCurrency = GameData.Currency.Type.DarkQuartz;
			_003C_003Ec__DisplayClass9_.destructible = _003C_003Ec__DisplayClass9_.droppedGear.destructible;
			_003C_003Ec__DisplayClass9_.droppedGear.destructible = false;
			_priceDisplay.text = _003C_003Ec__DisplayClass9_.droppedGear.dropped.price.ToString();
			_priceDisplay.color = (GameData.Currency.darkQuartz.Has(_unlockPrice) ? Color.white : Color.red);
			_003C_003Ec__DisplayClass9_.droppedGear.dropped.onLoot += _003C_003Ec__DisplayClass9_._003CCDropGear_003Eg__OnLoot_007C0;
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
	}
}
