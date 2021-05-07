using System;
using System.Collections;
using Characters.Gear;
using FX;
using Services;
using Singletons;
using UnityEngine;

namespace Level
{
	public class AdventurerReward : MonoBehaviour, ILootable
	{
		[SerializeField]
		private SpriteRenderer _choiceTable;

		[SerializeField]
		private SoundInfo _buySound;

		[SerializeField]
		private AdventurerRewardSlot[] _slots;

		private Resource.GearReference[] _gearInfosToDrop;

		private Resource.Request<Gear>[] _gearRequests;

		public bool looted { get; private set; }

		public event Action onLoot;

		private void Awake()
		{
			_choiceTable.sprite = Singleton<Service>.Instance.levelManager.currentChapter.gateChoiceTable;
			Load();
		}

		private void Load()
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			_gearInfosToDrop = new Resource.GearReference[_slots.Length];
			_gearRequests = new Resource.Request<Gear>[_slots.Length];
			for (int i = 0; i < _slots.Length; i++)
			{
				_slots[i].gameObject.SetActive(true);
				RarityPossibilities gearPossibilities = Singleton<Service>.Instance.levelManager.currentChapter.currentStage.gearPossibilities;
				GearManager gearManager = Singleton<Service>.Instance.gearManager;
				Resource.WeaponReference weaponToTake = gearManager.GetWeaponToTake(gearPossibilities.Evaluate());
				Resource.QuintessenceInfo quintessenceToTake = gearManager.GetQuintessenceToTake(gearPossibilities.Evaluate());
				Resource.ItemInfo itemToTake = gearManager.GetItemToTake(gearPossibilities.Evaluate());
				_gearInfosToDrop[0] = weaponToTake;
				_gearInfosToDrop[1] = quintessenceToTake;
				_gearInfosToDrop[2] = itemToTake;
			}
			for (int j = 0; j < _slots.Length; j++)
			{
				_gearRequests[j] = _gearInfosToDrop[j].LoadAsync();
			}
		}

		public void Activate()
		{
			StartCoroutine(CDisplayItems());
		}

		private IEnumerator CDisplayItems()
		{
			for (int i = 0; i < _slots.Length; i++)
			{
				_003C_003Ec__DisplayClass15_0 _003C_003Ec__DisplayClass15_ = new _003C_003Ec__DisplayClass15_0();
				_003C_003Ec__DisplayClass15_._003C_003E4__this = this;
				_003C_003Ec__DisplayClass15_.cachedIndex = i;
				while (!_gearRequests[i].isDone)
				{
					yield return null;
				}
				_003C_003Ec__DisplayClass15_.gear = Singleton<Service>.Instance.levelManager.DropGear(_gearRequests[i].asset, _slots[i].displayPosition);
				_003C_003Ec__DisplayClass15_.gear.onDiscard += _003C_003Ec__DisplayClass15_._003CCDisplayItems_003Eg__OnDiscard_007C0;
				_003C_003Ec__DisplayClass15_.destructible = _003C_003Ec__DisplayClass15_.gear.destructible;
				_003C_003Ec__DisplayClass15_.gear.destructible = _003C_003Ec__DisplayClass15_.gear.currencyByDiscard > 0;
				_slots[i].droppedGear = _003C_003Ec__DisplayClass15_.gear.dropped;
				if (_003C_003Ec__DisplayClass15_.destructible)
				{
					_003C_003Ec__DisplayClass15_.gear.dropped.onLoot += _003C_003Ec__DisplayClass15_._003CCDisplayItems_003Eg__OnLoot_007C1;
				}
			}
		}
	}
}
