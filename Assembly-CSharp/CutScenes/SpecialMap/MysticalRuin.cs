using System;
using Characters;
using Characters.Gear;
using Runnables;
using Services;
using Singletons;
using UnityEngine;

namespace CutScenes.SpecialMap
{
	public class MysticalRuin : MonoBehaviour
	{
		[SerializeField]
		private Runnable _runnable;

		[SerializeField]
		[Range(0f, 100f)]
		private float _weaponWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _itemWeight;

		[SerializeField]
		[Range(0f, 100f)]
		private float _quintessenceWeight;

		[SerializeField]
		private RarityPossibilities _rarityPossibilities;

		private WeightedRandomizer<Gear.Type> _weightedRandomizer;

		private Gear _gear;

		private void Awake()
		{
			_weightedRandomizer = WeightedRandomizer.From<Gear.Type>(new ValueTuple<Gear.Type, float>(Gear.Type.Item, _itemWeight), new ValueTuple<Gear.Type, float>(Gear.Type.Weapon, _weaponWeight), new ValueTuple<Gear.Type, float>(Gear.Type.Quintessence, _quintessenceWeight));
			DropGear();
		}

		private Gear.Type EvaluateGearType()
		{
			return _weightedRandomizer.TakeOne();
		}

		private void DropGear()
		{
			switch (EvaluateGearType())
			{
			case Gear.Type.Item:
				DropItem();
				break;
			case Gear.Type.Weapon:
				DropWeapon();
				break;
			case Gear.Type.Quintessence:
				DropQuintessence();
				break;
			}
		}

		private void DropItem()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass10_0 _003C_003Ec__DisplayClass10_ = new _003C_003Ec__DisplayClass10_0();
			_003C_003Ec__DisplayClass10_._003C_003E4__this = this;
			Resource.ItemInfo itemToTake;
			do
			{
				Rarity rarity = _rarityPossibilities.Evaluate();
				itemToTake = Singleton<Service>.Instance.gearManager.GetItemToTake(rarity);
			}
			while (itemToTake == null);
			_003C_003Ec__DisplayClass10_.request = itemToTake.LoadAsync();
			StartCoroutine(_003C_003Ec__DisplayClass10_._003CDropItem_003Eg__CDrop_007C0());
		}

		private void DropWeapon()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass11_0 _003C_003Ec__DisplayClass11_ = new _003C_003Ec__DisplayClass11_0();
			_003C_003Ec__DisplayClass11_._003C_003E4__this = this;
			Resource.ItemInfo itemToTake;
			do
			{
				Rarity rarity = _rarityPossibilities.Evaluate();
				itemToTake = Singleton<Service>.Instance.gearManager.GetItemToTake(rarity);
			}
			while (itemToTake == null);
			_003C_003Ec__DisplayClass11_.request = itemToTake.LoadAsync();
			StartCoroutine(_003C_003Ec__DisplayClass11_._003CDropWeapon_003Eg__CDrop_007C0());
		}

		private void DropQuintessence()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass12_0 _003C_003Ec__DisplayClass12_ = new _003C_003Ec__DisplayClass12_0();
			_003C_003Ec__DisplayClass12_._003C_003E4__this = this;
			Resource.QuintessenceInfo quintessenceToTake;
			do
			{
				Rarity rarity = _rarityPossibilities.Evaluate();
				quintessenceToTake = Singleton<Service>.Instance.gearManager.GetQuintessenceToTake(rarity);
			}
			while (quintessenceToTake == null);
			_003C_003Ec__DisplayClass12_.request = quintessenceToTake.LoadAsync();
			StartCoroutine(_003C_003Ec__DisplayClass12_._003CDropQuintessence_003Eg__CDrop_007C0());
		}

		private void Run(Character character)
		{
			_gear.dropped.onLoot -= Run;
			_gear.dropped.onDestroy -= Run;
			_runnable.Run();
		}
	}
}
