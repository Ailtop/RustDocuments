using System;
using Characters.Gear.Items;
using Characters.Gear.Synergy;
using Characters.Gear.Synergy.Keywords;

namespace Characters.Player
{
	public class Inventory
	{
		public readonly Synergy synergy;

		public readonly WeaponInventory weapon;

		public readonly ItemInventory item;

		public readonly QuintessenceInventory quintessence;

		private Character _character;

		public event Action onUpdated;

		public Inventory(Character character)
		{
			_character = character;
			synergy = character.GetComponent<Synergy>();
			weapon = character.GetComponent<WeaponInventory>();
			item = character.GetComponent<ItemInventory>();
			quintessence = character.GetComponent<QuintessenceInventory>();
		}

		public void Initialize()
		{
			synergy.Initialize(_character);
			onUpdated += UpdateSynergy;
			weapon.onChanged += delegate
			{
				this.onUpdated();
			};
			item.onChanged += delegate
			{
				this.onUpdated();
			};
			quintessence.onChanged += delegate
			{
				this.onUpdated();
			};
		}

		private void UpdateSynergy()
		{
			EnumArray<Keyword.Key, int> keywordCounts = synergy.keywordCounts;
			keywordCounts.SetAll(0);
			foreach (Item item in item.items)
			{
				if (!(item == null))
				{
					keywordCounts[item.keyword1]++;
					keywordCounts[item.keyword2]++;
				}
			}
			synergy.UpdateBonus();
		}
	}
}
