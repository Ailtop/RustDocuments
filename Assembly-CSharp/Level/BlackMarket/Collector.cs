using System.Collections;
using Characters.Gear;
using FX;
using Level.Npc;
using Services;
using Singletons;
using UnityEngine;

namespace Level.BlackMarket
{
	public class Collector : Npc
	{
		[SerializeField]
		protected SoundInfo _buySound;

		[SerializeField]
		private CollectorReroll _reroll;

		[SerializeField]
		private CollectorGearSlot[] _slots;

		[SerializeField]
		private NpcLineText _lineText;

		[SerializeField]
		private GameObject _talk;

		public string submitLine => Lingua.GetLocalizedStringArray("npc/collector/submit/line").Random();

		private void Awake()
		{
			_reroll.onInteracted += Reroll;
		}

		private void Start()
		{
			if (Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.activateCollector)
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
			_lineText.gameObject.SetActive(true);
			_talk.SetActive(true);
			StartCoroutine(CDisplayItems());
		}

		protected override void OnDeactivate()
		{
			_lineText.gameObject.SetActive(false);
			CollectorGearSlot[] slots = _slots;
			for (int i = 0; i < slots.Length; i++)
			{
				slots[i].gameObject.SetActive(false);
			}
		}

		private void Reroll()
		{
			StartCoroutine(CDisplayItems());
		}

		private IEnumerator CDisplayItems()
		{
			_003C_003Ec__DisplayClass12_0 _003C_003Ec__DisplayClass12_ = new _003C_003Ec__DisplayClass12_0();
			_003C_003Ec__DisplayClass12_._003C_003E4__this = this;
			Chapter currentChapter = Singleton<Service>.Instance.levelManager.currentChapter;
			_003C_003Ec__DisplayClass12_.gearInfosToDrop = new Resource.GearReference[_slots.Length];
			Resource.Request<Gear>[] gearRequests = new Resource.Request<Gear>[_slots.Length];
			for (int j = 0; j < _slots.Length; j++)
			{
				_slots[j].gameObject.SetActive(true);
				Resource.ItemInfo itemToTake;
				do
				{
					Rarity rarity = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings.collectorItemPossibilities.Evaluate();
					itemToTake = Singleton<Service>.Instance.gearManager.GetItemToTake(rarity);
				}
				while (_003C_003Ec__DisplayClass12_._003CCDisplayItems_003Eg__Duplicated_007C0(itemToTake.name));
				_003C_003Ec__DisplayClass12_.gearInfosToDrop[j] = itemToTake;
			}
			for (int k = 0; k < _slots.Length; k++)
			{
				gearRequests[k] = _003C_003Ec__DisplayClass12_.gearInfosToDrop[k].LoadAsync();
			}
			for (int i = 0; i < _slots.Length; i++)
			{
				_003C_003Ec__DisplayClass12_1 _003C_003Ec__DisplayClass12_2 = new _003C_003Ec__DisplayClass12_1();
				_003C_003Ec__DisplayClass12_2.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass12_;
				while (!gearRequests[i].isDone)
				{
					yield return null;
				}
				_003C_003Ec__DisplayClass12_2.gear = Singleton<Service>.Instance.levelManager.DropGear(gearRequests[i].asset, _slots[i].itemPosition);
				SettingsByStage marketSettings = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.marketSettings;
				int price = (int)((float)Settings.instance.marketSettings.collectorItemPrices.get_Item(_003C_003Ec__DisplayClass12_2.gear.rarity) * marketSettings.collectorItemPriceMultiplier * Random.Range(0.95f, 1.05f) / 10f) * 10;
				_003C_003Ec__DisplayClass12_2.gear.dropped.price = price;
				_003C_003Ec__DisplayClass12_2.destructible = _003C_003Ec__DisplayClass12_2.gear.destructible;
				_003C_003Ec__DisplayClass12_2.gear.destructible = false;
				_003C_003Ec__DisplayClass12_2.gear.dropped.onLoot += _003C_003Ec__DisplayClass12_2._003CCDisplayItems_003Eg__OnLoot_007C1;
				CollectorGearSlot collectorGearSlot = _slots[i];
				if (collectorGearSlot.droppedGear != null && collectorGearSlot.droppedGear.price > 0 && collectorGearSlot.droppedGear.gear.state == Gear.State.Dropped)
				{
					Object.Destroy(collectorGearSlot.droppedGear.gear.gameObject);
				}
				_slots[i].droppedGear = _003C_003Ec__DisplayClass12_2.gear.dropped;
				_003C_003Ec__DisplayClass12_2.gear.dropped.dropMovement.Stop();
				_003C_003Ec__DisplayClass12_2.gear.dropped.dropMovement.Float();
			}
		}
	}
}
