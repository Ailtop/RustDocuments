using System;
using Characters.Gear;
using Level;
using Services;
using Singletons;
using UnityEngine;

namespace Characters.Abilities.Customs
{
	[Serializable]
	public class MedalOfCarleon : Ability
	{
		public class Instance : AbilityInstance<MedalOfCarleon>
		{
			private LevelManager _levelManager;

			public Instance(Character owner, MedalOfCarleon ability)
				: base(owner, ability)
			{
				_levelManager = Singleton<Service>.Instance.levelManager;
			}

			protected override void OnAttach()
			{
				_levelManager.onMapLoadedAndFadedIn += DrpoGold;
			}

			protected override void OnDetach()
			{
				_levelManager.onMapLoadedAndFadedIn -= DrpoGold;
			}

			private void DrpoGold()
			{
				int itemCountByTag = owner.playerComponents.inventory.item.GetItemCountByTag(Characters.Gear.Gear.Tag.Carleon);
				if (itemCountByTag != 0)
				{
					int num = ability._goldPerItem * itemCountByTag;
					if (num > 0)
					{
						_levelManager.DropGold(num, itemCountByTag * 4, _levelManager.player.transform.position);
					}
				}
			}
		}

		[SerializeField]
		private int _goldPerItem;

		public override IAbilityInstance CreateInstance(Character owner)
		{
			return new Instance(owner, this);
		}
	}
}
